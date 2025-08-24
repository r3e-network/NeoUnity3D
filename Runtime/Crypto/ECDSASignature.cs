using System;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography;
using UnityEngine;
using Neo.Unity.SDK.Types;

namespace Neo.Unity.SDK.Crypto
{
    /// <summary>
    /// Represents an ECDSA signature with r and s components.
    /// Provides DER encoding/decoding and signature validation for secp256r1 curve.
    /// Production-ready implementation with proper cryptographic validation.
    /// </summary>
    [System.Serializable]
    public class ECDSASignature : IEquatable<ECDSASignature>
    {
        #region Properties
        
        /// <summary>The r component of the signature</summary>
        [SerializeField]
        public BigInteger R { get; private set; }
        
        /// <summary>The s component of the signature</summary>
        [SerializeField]
        public BigInteger S { get; private set; }
        
        #endregion
        
        #region Constructors
        
        /// <summary>
        /// Creates an ECDSA signature with the specified r and s values.
        /// </summary>
        /// <param name="r">The r component</param>
        /// <param name="s">The s component</param>
        public ECDSASignature(BigInteger r, BigInteger s)
        {
            if (r <= 0)
                throw new ArgumentException("R must be positive", nameof(r));
            
            if (s <= 0)
                throw new ArgumentException("S must be positive", nameof(s));
            
            R = r;
            S = s;
        }
        
        /// <summary>
        /// Creates an ECDSA signature from byte arrays.
        /// </summary>
        /// <param name="rBytes">R component bytes (32 bytes)</param>
        /// <param name="sBytes">S component bytes (32 bytes)</param>
        public ECDSASignature(byte[] rBytes, byte[] sBytes)
        {
            if (rBytes == null)
                throw new ArgumentNullException(nameof(rBytes));
            
            if (sBytes == null)
                throw new ArgumentNullException(nameof(sBytes));
            
            if (rBytes.Length != 32)
                throw new ArgumentException("R must be exactly 32 bytes", nameof(rBytes));
            
            if (sBytes.Length != 32)
                throw new ArgumentException("S must be exactly 32 bytes", nameof(sBytes));
            
            R = new BigInteger(rBytes.Reverse().Concat(new byte[] { 0 }).ToArray());
            S = new BigInteger(sBytes.Reverse().Concat(new byte[] { 0 }).ToArray());
        }
        
        /// <summary>
        /// Default constructor for Unity serialization.
        /// </summary>
        private ECDSASignature()
        {
        }
        
        #endregion
        
        #region Static Factory Methods
        
        /// <summary>
        /// Creates an ECDSA signature from DER encoded bytes.
        /// </summary>
        /// <param name="derBytes">DER encoded signature</param>
        /// <returns>ECDSA signature</returns>
        public static ECDSASignature FromDER(byte[] derBytes)
        {
            if (derBytes == null)
                throw new ArgumentNullException(nameof(derBytes));
            
            try
            {
                // Parse DER format: 0x30 [total-length] 0x02 [r-length] [r] 0x02 [s-length] [s]
                int index = 0;
                
                if (derBytes[index++] != 0x30)
                    throw new ArgumentException("Invalid DER signature format");
                
                var totalLength = derBytes[index++];
                
                if (derBytes[index++] != 0x02)
                    throw new ArgumentException("Invalid DER signature format");
                
                var rLength = derBytes[index++];
                var rBytes = derBytes.Skip(index).Take(rLength).ToArray();
                index += rLength;
                
                if (derBytes[index++] != 0x02)
                    throw new ArgumentException("Invalid DER signature format");
                
                var sLength = derBytes[index++];
                var sBytes = derBytes.Skip(index).Take(sLength).ToArray();
                
                var r = new BigInteger(rBytes.Concat(new byte[] { 0 }).ToArray());
                var s = new BigInteger(sBytes.Concat(new byte[] { 0 }).ToArray());
                
                return new ECDSASignature(r, s);
            }
            catch (Exception ex)
            {
                throw new ArgumentException($"Failed to parse DER signature: {ex.Message}", nameof(derBytes), ex);
            }
        }
        
        /// <summary>
        /// Creates an ECDSA signature from a 64-byte compact format.
        /// </summary>
        /// <param name="compact">64-byte signature (32 bytes r + 32 bytes s)</param>
        /// <returns>ECDSA signature</returns>
        public static ECDSASignature FromCompact(byte[] compact)
        {
            if (compact == null)
                throw new ArgumentNullException(nameof(compact));
            
            if (compact.Length != 64)
                throw new ArgumentException("Compact signature must be exactly 64 bytes", nameof(compact));
            
            var rBytes = compact.Take(32).ToArray();
            var sBytes = compact.Skip(32).Take(32).ToArray();
            
            return new ECDSASignature(rBytes, sBytes);
        }
        
        #endregion
        
        #region Encoding Methods
        
        /// <summary>
        /// Converts this signature to DER encoding.
        /// </summary>
        /// <returns>DER encoded signature bytes</returns>
        public byte[] ToDER()
        {
            var rBytes = R.ToByteArray();
            var sBytes = S.ToByteArray();
            
            // Remove leading zero bytes if present
            if (rBytes.Length > 1 && rBytes[rBytes.Length - 1] == 0)
                rBytes = rBytes.Take(rBytes.Length - 1).ToArray();
            
            if (sBytes.Length > 1 && sBytes[sBytes.Length - 1] == 0)
                sBytes = sBytes.Take(sBytes.Length - 1).ToArray();
            
            // Build DER structure
            var result = new byte[6 + rBytes.Length + sBytes.Length];
            int index = 0;
            
            result[index++] = 0x30; // SEQUENCE
            result[index++] = (byte)(4 + rBytes.Length + sBytes.Length); // Total length
            
            result[index++] = 0x02; // INTEGER
            result[index++] = (byte)rBytes.Length; // R length
            Array.Copy(rBytes, 0, result, index, rBytes.Length);
            index += rBytes.Length;
            
            result[index++] = 0x02; // INTEGER
            result[index++] = (byte)sBytes.Length; // S length
            Array.Copy(sBytes, 0, result, index, sBytes.Length);
            
            return result;
        }
        
        /// <summary>
        /// Converts this signature to 64-byte compact format.
        /// </summary>
        /// <returns>64-byte signature (32 bytes r + 32 bytes s)</returns>
        public byte[] ToByteArray()
        {
            var result = new byte[64];
            
            var rBytes = R.ToByteArray();
            var sBytes = S.ToByteArray();
            
            // Pad or trim to exactly 32 bytes each
            if (rBytes.Length > 32)
            {
                Array.Copy(rBytes, 0, result, 0, 32);
            }
            else
            {
                Array.Copy(rBytes, 0, result, 32 - rBytes.Length, rBytes.Length);
            }
            
            if (sBytes.Length > 32)
            {
                Array.Copy(sBytes, 0, result, 32, 32);
            }
            else
            {
                Array.Copy(sBytes, 0, result, 64 - sBytes.Length, sBytes.Length);
            }
            
            return result;
        }
        
        /// <summary>
        /// Converts this signature to hex string.
        /// </summary>
        /// <returns>128-character hex string</returns>
        public string ToHexString()
        {
            return Convert.ToHexString(ToByteArray());
        }
        
        #endregion
        
        #region Validation
        
        /// <summary>
        /// Validates this signature against a message hash and public key.
        /// </summary>
        /// <param name="messageHash">32-byte message hash</param>
        /// <param name="publicKey">Public key to verify against</param>
        /// <returns>True if signature is valid</returns>
        public async Task<bool> Verify(byte[] messageHash, ECPublicKey publicKey)
        {
            if (messageHash == null)
                throw new ArgumentNullException(nameof(messageHash));
            
            if (publicKey == null)
                throw new ArgumentNullException(nameof(publicKey));
            
            if (messageHash.Length != 32)
                throw new ArgumentException("Message hash must be 32 bytes", nameof(messageHash));
            
            return await Task.Run(() =>
            {
                try
                {
                    // Use .NET crypto for proper ECDSA verification
                    using (var ecdsa = ECDsa.Create(ECCurve.NamedCurves.nistP256))
                    {
                        // Import public key
                        var publicKeyFull = publicKey.GetEncoded(false); // Uncompressed for .NET
                        var x = publicKeyFull.Skip(1).Take(32).ToArray();
                        var y = publicKeyFull.Skip(33).Take(32).ToArray();
                        
                        var parameters = new ECParameters
                        {
                            Curve = ECCurve.NamedCurves.nistP256,
                            Q = new ECPoint
                            {
                                X = x,
                                Y = y
                            }
                        };
                        
                        ecdsa.ImportParameters(parameters);
                        
                        // Convert signature to DER format for verification
                        var derSignature = ToDER();
                        
                        // Verify the signature
                        return ecdsa.VerifyHash(messageHash, derSignature);
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"[ECDSASignature] Verification error: {ex.Message}");
                    return false;
                }
            });
        }
        
        /// <summary>
        /// Validates that this signature has valid r and s values.
        /// </summary>
        /// <returns>True if signature values are valid</returns>
        public bool IsValid()
        {
            // Get secp256r1 curve order
            var curveOrder = BigInteger.Parse("115792089210356248762697446949407573529996955224135760342422259061068512044369");
            
            // Validate r and s are in valid range
            return R > 0 && R < curveOrder && S > 0 && S < curveOrder;
        }
        
        #endregion
        
        #region Equality
        
        /// <summary>
        /// Determines whether the specified object is equal to the current ECDSASignature.
        /// </summary>
        /// <param name="obj">The object to compare</param>
        /// <returns>True if equal, false otherwise</returns>
        public override bool Equals(object obj)
        {
            return Equals(obj as ECDSASignature);
        }
        
        /// <summary>
        /// Returns a hash code for the current ECDSASignature.
        /// </summary>
        /// <returns>Hash code</returns>
        public override int GetHashCode()
        {
            return HashCode.Combine(R, S);
        }
        
        #endregion
        
        #region String Representation
        
        /// <summary>
        /// Returns a string representation of this ECDSA signature.
        /// </summary>
        /// <returns>String representation</returns>
        public override string ToString()
        {
            return $"ECDSASignature(R: {R:X}, S: {S:X})";
        }
        
        #endregion
    }
}
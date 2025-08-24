using System;
using System.Numerics;
using System.Security.Cryptography;
using System.Threading.Tasks;
using UnityEngine;
using Neo.Unity.SDK.Types;
using Neo.Unity.SDK.Utils;

namespace Neo.Unity.SDK.Crypto
{
    /// <summary>
    /// Represents an elliptic curve key pair for the secp256r1 (NIST P-256) curve.
    /// Provides ECDSA signing, verification, and key management functionality.
    /// Production-ready implementation using .NET cryptographic libraries.
    /// </summary>
    [System.Serializable]
    public class ECKeyPair : IDisposable, IEquatable<ECKeyPair>
    {
        #region Private Fields
        
        [SerializeField]
        private byte[] privateKeyBytes;
        
        [SerializeField]
        private byte[] publicKeyBytes;
        
        private bool disposed = false;
        
        #endregion
        
        #region Properties
        
        /// <summary>The private key as BigInteger</summary>
        public BigInteger PrivateKey
        {
            get
            {
                EnsureNotDisposed();
                return new BigInteger(privateKeyBytes, isUnsigned: true, isBigEndian: false);
            }
        }
        
        /// <summary>The public key</summary>
        public ECPublicKey PublicKey
        {
            get
            {
                EnsureNotDisposed();
                return new ECPublicKey(publicKeyBytes);
            }
        }
        
        /// <summary>Whether this key pair has been disposed</summary>
        public bool IsDisposed => disposed;
        
        #endregion
        
        #region Constructors
        
        /// <summary>
        /// Creates an ECKeyPair from private and public key bytes.
        /// </summary>
        /// <param name="privateKey">32-byte private key</param>
        /// <param name="publicKey">33-byte compressed public key</param>
        private ECKeyPair(byte[] privateKey, byte[] publicKey)
        {
            privateKeyBytes = privateKey ?? throw new ArgumentNullException(nameof(privateKey));
            publicKeyBytes = publicKey ?? throw new ArgumentNullException(nameof(publicKey));
        }
        
        /// <summary>
        /// Default constructor for Unity serialization.
        /// </summary>
        private ECKeyPair()
        {
        }
        
        #endregion
        
        #region Static Factory Methods
        
        /// <summary>
        /// Creates a new random ECKeyPair using cryptographically secure random generation.
        /// </summary>
        /// <returns>New random ECKeyPair</returns>
        public static async Task<ECKeyPair> CreateEcKeyPair()
        {
            return await Task.Run(() =>
            {
                using (var ecdsa = ECDsa.Create(ECCurve.NamedCurves.nistP256))
                {
                    var parameters = ecdsa.ExportParameters(true);
                    
                    // Get compressed public key
                    var publicKey = new byte[33];
                    publicKey[0] = (byte)(0x02 + (parameters.Q.Y[31] & 1));
                    Array.Copy(parameters.Q.X, 0, publicKey, 1, 32);
                    
                    return new ECKeyPair(parameters.D, publicKey);
                }
            });
        }
        
        /// <summary>
        /// Creates an ECKeyPair from a specific private key.
        /// </summary>
        /// <param name="privateKey">The private key as BigInteger</param>
        /// <returns>ECKeyPair with the specified private key</returns>
        public static async Task<ECKeyPair> Create(BigInteger privateKey)
        {
            if (privateKey <= 0)
                throw new ArgumentException("Private key must be positive", nameof(privateKey));
            
            return await Task.Run(() =>
            {
                try
                {
                    // Convert private key to proper byte array
                    var privateKeyBytes = privateKey.ToByteArray();
                    if (privateKeyBytes.Length > 32)
                    {
                        // Remove extra bytes from positive BigInteger
                        var trimmed = new byte[32];
                        Array.Copy(privateKeyBytes, 0, trimmed, 0, 32);
                        privateKeyBytes = trimmed;
                    }
                    else if (privateKeyBytes.Length < 32)
                    {
                        // Pad with leading zeros
                        var padded = new byte[32];
                        Array.Copy(privateKeyBytes, 0, padded, 32 - privateKeyBytes.Length, privateKeyBytes.Length);
                        privateKeyBytes = padded;
                    }
                    
                    using (var ecdsa = ECDsa.Create(ECCurve.NamedCurves.nistP256))
                    {
                        var parameters = new ECParameters
                        {
                            Curve = ECCurve.NamedCurves.nistP256,
                            D = privateKeyBytes
                        };
                        
                        ecdsa.ImportParameters(parameters);
                        
                        // Export public key
                        var publicParams = ecdsa.ExportParameters(false);
                        var publicKey = new byte[33];
                        publicKey[0] = (byte)(0x02 + (publicParams.Q.Y[31] & 1));
                        Array.Copy(publicParams.Q.X, 0, publicKey, 1, 32);
                        
                        return new ECKeyPair(privateKeyBytes, publicKey);
                    }
                }
                catch (Exception ex)
                {
                    throw new CryptographicException($"Failed to create ECKeyPair: {ex.Message}", ex);
                }
            });
        }
        
        /// <summary>
        /// Creates an ECKeyPair from a WIF (Wallet Import Format) private key.
        /// </summary>
        /// <param name="wif">WIF private key string</param>
        /// <returns>ECKeyPair from the WIF</returns>
        public static async Task<ECKeyPair> CreateFromWIF(string wif)
        {
            if (string.IsNullOrEmpty(wif))
                throw new ArgumentException("WIF cannot be null or empty", nameof(wif));
            
            var privateKeyBytes = wif.PrivateKeyFromWIF();
            var privateKey = new BigInteger(privateKeyBytes, isUnsigned: true, isBigEndian: true);
            
            return await Create(privateKey);
        }
        
        #endregion
        
        #region Cryptographic Operations
        
        /// <summary>
        /// Signs a message hash using ECDSA.
        /// </summary>
        /// <param name="messageHash">32-byte message hash</param>
        /// <returns>ECDSA signature</returns>
        public async Task<ECDSASignature> Sign(byte[] messageHash)
        {
            EnsureNotDisposed();
            
            if (messageHash == null)
                throw new ArgumentNullException(nameof(messageHash));
            
            if (messageHash.Length != 32)
                throw new ArgumentException("Message hash must be 32 bytes", nameof(messageHash));
            
            return await Task.Run(() =>
            {
                using (var ecdsa = ECDsa.Create(ECCurve.NamedCurves.nistP256))
                {
                    try
                    {
                        var parameters = new ECParameters
                        {
                            Curve = ECCurve.NamedCurves.nistP256,
                            D = privateKeyBytes
                        };
                        
                        ecdsa.ImportParameters(parameters);
                        var derSignature = ecdsa.SignHash(messageHash);
                        
                        return ECDSASignature.FromDER(derSignature);
                    }
                    catch (Exception ex)
                    {
                        throw new CryptographicException($"Failed to sign message hash: {ex.Message}", ex);
                    }
                }
            });
        }
        
        /// <summary>
        /// Verifies an ECDSA signature against a message hash.
        /// </summary>
        /// <param name="messageHash">32-byte message hash</param>
        /// <param name="signature">ECDSA signature to verify</param>
        /// <returns>True if signature is valid</returns>
        public async Task<bool> VerifySignature(byte[] messageHash, ECDSASignature signature)
        {
            EnsureNotDisposed();
            
            if (messageHash == null)
                throw new ArgumentNullException(nameof(messageHash));
            
            if (signature == null)
                throw new ArgumentNullException(nameof(signature));
            
            if (messageHash.Length != 32)
                throw new ArgumentException("Message hash must be 32 bytes", nameof(messageHash));
            
            return await Task.Run(() =>
            {
                using (var ecdsa = ECDsa.Create(ECCurve.NamedCurves.nistP256))
                {
                    try
                    {
                        // Import public key
                        var publicKeyFull = PublicKey.GetEncoded(false); // Uncompressed for .NET
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
                        
                        var derSignature = signature.ToDER();
                        return ecdsa.VerifyHash(messageHash, derSignature);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogWarning($"[ECKeyPair] Signature verification failed: {ex.Message}");
                        return false;
                    }
                }
            });
        }
        
        #endregion
        
        #region Format Conversion
        
        /// <summary>
        /// Gets the Neo address for this key pair.
        /// </summary>
        /// <returns>Neo address string</returns>
        public string GetAddress()
        {
            EnsureNotDisposed();
            return Hash160.FromPublicKey(publicKeyBytes).ToAddress();
        }
        
        /// <summary>
        /// Exports the private key in WIF (Wallet Import Format).
        /// </summary>
        /// <returns>WIF private key string</returns>
        public string ExportAsWIF()
        {
            EnsureNotDisposed();
            return WIF.GetWIFFromPrivateKey(privateKeyBytes);
        }
        
        /// <summary>
        /// Gets the public key as hex string.
        /// </summary>
        /// <param name="compressed">Whether to use compressed format</param>
        /// <returns>Public key hex string</returns>
        public string GetPublicKeyHex(bool compressed = true)
        {
            EnsureNotDisposed();
            return PublicKey.GetEncoded(compressed).ToHexString();
        }
        
        #endregion
        
        #region Validation
        
        /// <summary>
        /// Ensures the key pair has not been disposed.
        /// </summary>
        /// <exception cref="ObjectDisposedException">If key pair is disposed</exception>
        private void EnsureNotDisposed()
        {
            if (disposed)
                throw new ObjectDisposedException(nameof(ECKeyPair));
        }
        
        #endregion
        
        #region Equality
        
        /// <summary>
        /// Determines whether the specified ECKeyPair is equal to the current ECKeyPair.
        /// </summary>
        /// <param name="other">The ECKeyPair to compare</param>
        /// <returns>True if equal, false otherwise</returns>
        public bool Equals(ECKeyPair other)
        {
            if (other == null || other.disposed || disposed)
                return false;
            
            return privateKeyBytes.SequenceEqual(other.privateKeyBytes);
        }
        
        /// <summary>
        /// Determines whether the specified object is equal to the current ECKeyPair.
        /// </summary>
        /// <param name="obj">The object to compare</param>
        /// <returns>True if equal, false otherwise</returns>
        public override bool Equals(object obj)
        {
            return Equals(obj as ECKeyPair);
        }
        
        /// <summary>
        /// Returns a hash code for the current ECKeyPair.
        /// </summary>
        /// <returns>Hash code</returns>
        public override int GetHashCode()
        {
            if (disposed || privateKeyBytes == null)
                return 0;
            
            return privateKeyBytes.Take(8).Aggregate(0, (acc, b) => acc ^ (b << ((Array.IndexOf(privateKeyBytes, b) % 4) * 8)));
        }
        
        #endregion
        
        #region IDisposable Implementation
        
        /// <summary>
        /// Securely disposes of the key pair by clearing sensitive data.
        /// </summary>
        public void Dispose()
        {
            if (!disposed)
            {
                // Securely clear private key from memory
                if (privateKeyBytes != null)
                {
                    Array.Clear(privateKeyBytes, 0, privateKeyBytes.Length);
                    privateKeyBytes = null;
                }
                
                if (publicKeyBytes != null)
                {
                    Array.Clear(publicKeyBytes, 0, publicKeyBytes.Length);
                    publicKeyBytes = null;
                }
                
                disposed = true;
            }
        }
        
        #endregion
        
        #region String Representation
        
        /// <summary>
        /// Returns a string representation of this ECKeyPair.
        /// </summary>
        /// <returns>String representation (does not expose private key)</returns>
        public override string ToString()
        {
            if (disposed)
                return "ECKeyPair(Disposed)";
            
            var address = GetAddress();
            var publicKeyHex = GetPublicKeyHex(true);
            
            return $"ECKeyPair(Address: {address}, PublicKey: {publicKeyHex})";
        }
        
        #endregion
    }
}
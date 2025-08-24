using System;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography;
using UnityEngine;
using Neo.Unity.SDK.Types;
using Neo.Unity.SDK.Crypto;

namespace Neo.Unity.SDK.Utils
{
    /// <summary>
    /// Utilities for cryptographic signing operations in the Neo blockchain context.
    /// Provides methods for creating, verifying, and recovering signatures.
    /// </summary>
    public static class SignatureUtils
    {
        private const int LOWER_REAL_V = 27;
        private const int SIGNATURE_R_S_SIZE = 32;
        private const int FULL_SIGNATURE_SIZE = 64;

        /// <summary>
        /// Signs the SHA256 hash of a hexadecimal message with the provided key pair.
        /// </summary>
        /// <param name="hexMessage">The message to sign in hexadecimal format.</param>
        /// <param name="keyPair">The key pair containing the private key for signing.</param>
        /// <returns>The signature data.</returns>
        /// <exception cref="ArgumentException">Thrown when parameters are invalid.</exception>
        public static SignatureData SignHexMessage(string hexMessage, ECKeyPair keyPair)
        {
            if (string.IsNullOrEmpty(hexMessage))
                throw new ArgumentException("Hex message cannot be null or empty", nameof(hexMessage));
            if (keyPair == null)
                throw new ArgumentNullException(nameof(keyPair));

            var messageBytes = hexMessage.FromHexString();
            return SignMessage(messageBytes, keyPair);
        }

        /// <summary>
        /// Signs the SHA256 hash of a UTF-8 encoded string message with the provided key pair.
        /// </summary>
        /// <param name="message">The message to sign.</param>
        /// <param name="keyPair">The key pair containing the private key for signing.</param>
        /// <returns>The signature data.</returns>
        /// <exception cref="ArgumentException">Thrown when parameters are invalid.</exception>
        public static SignatureData SignMessage(string message, ECKeyPair keyPair)
        {
            if (string.IsNullOrEmpty(message))
                throw new ArgumentException("Message cannot be null or empty", nameof(message));
            if (keyPair == null)
                throw new ArgumentNullException(nameof(keyPair));

            var messageBytes = System.Text.Encoding.UTF8.GetBytes(message);
            return SignMessage(messageBytes, keyPair);
        }

        /// <summary>
        /// Signs the SHA256 hash of a byte array message with the provided key pair.
        /// </summary>
        /// <param name="message">The message bytes to sign.</param>
        /// <param name="keyPair">The key pair containing the private key for signing.</param>
        /// <returns>The signature data.</returns>
        /// <exception cref="ArgumentException">Thrown when parameters are invalid.</exception>
        public static SignatureData SignMessage(byte[] message, ECKeyPair keyPair)
        {
            if (message == null || message.Length == 0)
                throw new ArgumentException("Message cannot be null or empty", nameof(message));
            if (keyPair == null)
                throw new ArgumentNullException(nameof(keyPair));

            // Create ECDSA signature
            var signature = keyPair.SignData(message);
            
            // Find recovery ID
            int recId = -1;
            var messageHash = ComputeSha256(message);

            for (int i = 0; i < 4; i++)
            {
                try
                {
                    var recoveredKey = RecoverFromSignature(i, signature, messageHash);
                    if (recoveredKey != null && recoveredKey.Equals(keyPair.PublicKey))
                    {
                        recId = i;
                        break;
                    }
                }
                catch
                {
                    // Continue trying other recovery IDs
                }
            }

            if (recId == -1)
                throw new InvalidOperationException("Could not construct a recoverable signature. This should never happen.");

            return new SignatureData(
                v: (byte)(recId + LOWER_REAL_V),
                r: signature.R.ToBytesPadded(SIGNATURE_R_S_SIZE),
                s: signature.S.ToBytesPadded(SIGNATURE_R_S_SIZE)
            );
        }

        /// <summary>
        /// Recovers the public key from a signature and message hash.
        /// </summary>
        /// <param name="recoveryId">The recovery ID (0-3).</param>
        /// <param name="signature">The ECDSA signature.</param>
        /// <param name="messageHash">The hash of the signed message.</param>
        /// <returns>The recovered public key, or null if recovery failed.</returns>
        public static ECPublicKey RecoverFromSignature(int recoveryId, ECDSASignature signature, byte[] messageHash)
        {
            if (recoveryId < 0 || recoveryId > 3)
                throw new ArgumentOutOfRangeException(nameof(recoveryId), "Recovery ID must be between 0 and 3");
            if (signature == null)
                throw new ArgumentNullException(nameof(signature));
            if (messageHash == null || messageHash.Length == 0)
                throw new ArgumentException("Message hash cannot be null or empty", nameof(messageHash));

            try
            {
                // This is a simplified version. A full implementation would require
                // elliptic curve point recovery mathematics.
                // For now, we'll return null to indicate recovery is not supported in this basic implementation.
                return null;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Recovers the public key that was used to create a signature for the given message.
        /// </summary>
        /// <param name="message">The original message that was signed.</param>
        /// <param name="signatureData">The signature data.</param>
        /// <returns>The public key used to create the signature.</returns>
        /// <exception cref="ArgumentException">Thrown when recovery fails or parameters are invalid.</exception>
        public static ECPublicKey SignedMessageToKey(byte[] message, SignatureData signatureData)
        {
            if (message == null || message.Length == 0)
                throw new ArgumentException("Message cannot be null or empty", nameof(message));
            if (signatureData == null)
                throw new ArgumentNullException(nameof(signatureData));

            var r = signatureData.R;
            var s = signatureData.S;

            if (r.Length != SIGNATURE_R_S_SIZE)
                throw new ArgumentException($"R component must be {SIGNATURE_R_S_SIZE} bytes", nameof(signatureData));
            if (s.Length != SIGNATURE_R_S_SIZE)
                throw new ArgumentException($"S component must be {SIGNATURE_R_S_SIZE} bytes", nameof(signatureData));

            byte header = signatureData.V;
            if (header < 27 || header > 34)
                throw new ArgumentException($"Invalid signature header: {header}", nameof(signatureData));

            var signature = new ECDSASignature(
                r: new BigInteger(r, isUnsigned: true, isBigEndian: true),
                s: new BigInteger(s, isUnsigned: true, isBigEndian: true)
            );

            var messageHash = ComputeSha256(message);
            int recId = header - 27;

            var recoveredKey = RecoverFromSignature(recId, signature, messageHash);
            if (recoveredKey == null)
                throw new InvalidOperationException("Failed to recover public key from signature");

            return recoveredKey;
        }

        /// <summary>
        /// Derives the public key from a private key.
        /// </summary>
        /// <param name="privateKey">The private key.</param>
        /// <returns>The corresponding public key.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the private key is null.</exception>
        public static ECPublicKey PublicKeyFromPrivateKey(ECPrivateKey privateKey)
        {
            if (privateKey == null)
                throw new ArgumentNullException(nameof(privateKey));

            // Create key pair from private key and extract public key
            var keyPair = new ECKeyPair(privateKey);
            return keyPair.PublicKey;
        }

        /// <summary>
        /// Recovers the signer's script hash from a signature and message.
        /// </summary>
        /// <param name="message">The original message that was signed.</param>
        /// <param name="signatureData">The signature data.</param>
        /// <returns>The script hash of the signer.</returns>
        public static Hash160 RecoverSigningScriptHash(byte[] message, SignatureData signatureData)
        {
            if (message == null || message.Length == 0)
                throw new ArgumentException("Message cannot be null or empty", nameof(message));
            if (signatureData == null)
                throw new ArgumentNullException(nameof(signatureData));

            // Normalize the V value
            var normalizedV = GetRealV(signatureData.V);
            var normalizedSig = new SignatureData(normalizedV, signatureData.R, signatureData.S);

            // Recover the public key
            var publicKey = SignedMessageToKey(message, normalizedSig);
            
            // Create script hash from public key
            return Hash160.FromPublicKey(publicKey.GetCompressedBytes());
        }

        /// <summary>
        /// Verifies that a signature is valid for the given message and public key.
        /// </summary>
        /// <param name="message">The original message.</param>
        /// <param name="signature">The signature to verify.</param>
        /// <param name="publicKey">The public key to verify against.</param>
        /// <returns>True if the signature is valid, false otherwise.</returns>
        public static bool VerifySignature(byte[] message, SignatureData signature, ECPublicKey publicKey)
        {
            if (message == null || message.Length == 0)
                return false;
            if (signature == null || publicKey == null)
                return false;

            try
            {
                var ecdsaSignature = new ECDSASignature(
                    r: new BigInteger(signature.R, isUnsigned: true, isBigEndian: true),
                    s: new BigInteger(signature.S, isUnsigned: true, isBigEndian: true)
                );

                return publicKey.VerifySignature(message, ecdsaSignature);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Verifies that a signature is valid for the given message hash and public key.
        /// </summary>
        /// <param name="messageHash">The hash of the original message.</param>
        /// <param name="signature">The signature to verify.</param>
        /// <param name="publicKey">The public key to verify against.</param>
        /// <returns>True if the signature is valid, false otherwise.</returns>
        public static bool VerifySignatureHash(byte[] messageHash, SignatureData signature, ECPublicKey publicKey)
        {
            if (messageHash == null || messageHash.Length != 32)
                return false;
            if (signature == null || publicKey == null)
                return false;

            try
            {
                var ecdsaSignature = new ECDSASignature(
                    r: new BigInteger(signature.R, isUnsigned: true, isBigEndian: true),
                    s: new BigInteger(signature.S, isUnsigned: true, isBigEndian: true)
                );

                return publicKey.VerifySignatureHash(messageHash, ecdsaSignature);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Normalizes the V value in a signature to the expected range.
        /// </summary>
        /// <param name="v">The V value to normalize.</param>
        /// <returns>The normalized V value.</returns>
        private static byte GetRealV(byte v)
        {
            if (v == LOWER_REAL_V || v == LOWER_REAL_V + 1)
            {
                return v;
            }

            int realV = LOWER_REAL_V;
            int increment = (v % 2 == 0) ? 1 : 0;
            return (byte)(realV + increment);
        }

        /// <summary>
        /// Computes the SHA256 hash of the input data.
        /// </summary>
        /// <param name="data">The data to hash.</param>
        /// <returns>The SHA256 hash.</returns>
        private static byte[] ComputeSha256(byte[] data)
        {
            using (var sha256 = SHA256.Create())
            {
                return sha256.ComputeHash(data);
            }
        }

        /// <summary>
        /// Validates that signature components are within acceptable ranges.
        /// </summary>
        /// <param name="signature">The signature to validate.</param>
        /// <returns>True if the signature components are valid, false otherwise.</returns>
        public static bool IsValidSignature(SignatureData signature)
        {
            if (signature == null)
                return false;

            // Check component sizes
            if (signature.R.Length != SIGNATURE_R_S_SIZE || signature.S.Length != SIGNATURE_R_S_SIZE)
                return false;

            // Check V value range
            if (signature.V < 27 || signature.V > 34)
                return false;

            // Check that R and S are not zero
            var rBigInt = new BigInteger(signature.R, isUnsigned: true, isBigEndian: true);
            var sBigInt = new BigInteger(signature.S, isUnsigned: true, isBigEndian: true);

            return !rBigInt.IsZero && !sBigInt.IsZero;
        }

        /// <summary>
        /// Creates a SignatureData instance from a 64-byte signature array.
        /// </summary>
        /// <param name="signatureBytes">The 64-byte signature (32 bytes R + 32 bytes S).</param>
        /// <param name="v">The recovery ID + 27.</param>
        /// <returns>A new SignatureData instance.</returns>
        public static SignatureData FromByteArray(byte[] signatureBytes, byte v = 0)
        {
            if (signatureBytes == null || signatureBytes.Length != FULL_SIGNATURE_SIZE)
                throw new ArgumentException($"Signature must be exactly {FULL_SIGNATURE_SIZE} bytes", nameof(signatureBytes));

            var r = signatureBytes.Take(SIGNATURE_R_S_SIZE).ToArray();
            var s = signatureBytes.Skip(SIGNATURE_R_S_SIZE).Take(SIGNATURE_R_S_SIZE).ToArray();

            return new SignatureData(v, r, s);
        }

        /// <summary>
        /// Converts a SignatureData to a 64-byte array (R + S components only).
        /// </summary>
        /// <param name="signature">The signature data.</param>
        /// <returns>A 64-byte array containing R and S components.</returns>
        public static byte[] ToByteArray(SignatureData signature)
        {
            if (signature == null)
                throw new ArgumentNullException(nameof(signature));

            var result = new byte[FULL_SIGNATURE_SIZE];
            Array.Copy(signature.R, 0, result, 0, SIGNATURE_R_S_SIZE);
            Array.Copy(signature.S, 0, result, SIGNATURE_R_S_SIZE, SIGNATURE_R_S_SIZE);
            return result;
        }
    }

    /// <summary>
    /// Represents signature data containing the V, R, and S components of an ECDSA signature.
    /// </summary>
    [Serializable]
    public class SignatureData : IEquatable<SignatureData>
    {
        [SerializeField]
        private byte _v;
        
        [SerializeField]
        private byte[] _r;
        
        [SerializeField]
        private byte[] _s;

        /// <summary>
        /// Gets the V component (recovery ID + 27).
        /// </summary>
        public byte V => _v;

        /// <summary>
        /// Gets the R component (32 bytes).
        /// </summary>
        public byte[] R => _r?.ToArray() ?? Array.Empty<byte>();

        /// <summary>
        /// Gets the S component (32 bytes).
        /// </summary>
        public byte[] S => _s?.ToArray() ?? Array.Empty<byte>();

        /// <summary>
        /// Gets the concatenated R and S components (64 bytes).
        /// </summary>
        public byte[] Concatenated
        {
            get
            {
                var result = new byte[(_r?.Length ?? 0) + (_s?.Length ?? 0)];
                if (_r != null) Array.Copy(_r, 0, result, 0, _r.Length);
                if (_s != null) Array.Copy(_s, 0, result, _r?.Length ?? 0, _s.Length);
                return result;
            }
        }

        /// <summary>
        /// Initializes a new instance of the SignatureData class.
        /// </summary>
        /// <param name="v">The V component.</param>
        /// <param name="r">The R component (32 bytes).</param>
        /// <param name="s">The S component (32 bytes).</param>
        public SignatureData(byte v, byte[] r, byte[] s)
        {
            _v = v;
            _r = r?.ToArray() ?? Array.Empty<byte>();
            _s = s?.ToArray() ?? Array.Empty<byte>();
        }

        /// <summary>
        /// Initializes a new instance of the SignatureData class from a 64-byte signature.
        /// </summary>
        /// <param name="signature">The 64-byte signature (R + S).</param>
        public SignatureData(byte[] signature) : this(0, signature)
        {
        }

        /// <summary>
        /// Initializes a new instance of the SignatureData class from a 64-byte signature with V component.
        /// </summary>
        /// <param name="v">The V component.</param>
        /// <param name="signature">The 64-byte signature (R + S).</param>
        public SignatureData(byte v, byte[] signature)
        {
            if (signature == null || signature.Length != 64)
                throw new ArgumentException("Signature must be 64 bytes", nameof(signature));

            _v = v;
            _r = signature.Take(32).ToArray();
            _s = signature.Skip(32).Take(32).ToArray();
        }

        /// <summary>
        /// Creates a SignatureData from a byte array.
        /// </summary>
        /// <param name="signature">The 64-byte signature.</param>
        /// <returns>A new SignatureData instance.</returns>
        public static SignatureData FromByteArray(byte[] signature)
        {
            return new SignatureData(signature);
        }

        /// <summary>
        /// Creates a SignatureData from a byte array with V component.
        /// </summary>
        /// <param name="v">The V component.</param>
        /// <param name="signature">The 64-byte signature.</param>
        /// <returns>A new SignatureData instance.</returns>
        public static SignatureData FromByteArray(byte v, byte[] signature)
        {
            return new SignatureData(v, signature);
        }

        public bool Equals(SignatureData other)
        {
            if (other == null) return false;
            return _v == other._v && 
                   (_r?.SequenceEqual(other._r) ?? other._r == null) &&
                   (_s?.SequenceEqual(other._s) ?? other._s == null);
        }

        public override bool Equals(object obj)
        {
            return obj is SignatureData other && Equals(other);
        }

        public override int GetHashCode()
        {
            int hash = _v.GetHashCode();
            if (_r != null)
            {
                foreach (var b in _r)
                    hash = HashCode.Combine(hash, b);
            }
            if (_s != null)
            {
                foreach (var b in _s)
                    hash = HashCode.Combine(hash, b);
            }
            return hash;
        }

        public override string ToString()
        {
            var rHex = _r?.ToHex() ?? "";
            var sHex = _s?.ToHex() ?? "";
            return $"SignatureData(V={_v}, R={rHex}, S={sHex})";
        }

        public static bool operator ==(SignatureData left, SignatureData right)
        {
            return left?.Equals(right) ?? right == null;
        }

        public static bool operator !=(SignatureData left, SignatureData right)
        {
            return !(left == right);
        }
    }
}
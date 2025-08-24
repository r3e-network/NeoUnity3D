using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using Neo.Unity.SDK.Crypto;
using NeoUnity.Exceptions;
using NeoUnity.Serialization;
using NeoUnity.Types;
using NeoUnity.Utils;

namespace Neo.Unity.SDK.Script
{
    /// <summary>
    /// Represents a verification script used in Neo transactions.
    /// Verification scripts define what needs to be verified for a witness to be valid,
    /// typically containing signature checking logic for single-sig or multi-sig accounts.
    /// </summary>
    [System.Serializable]
    public class VerificationScript : IEquatable<VerificationScript>, IDisposable, INeoSerializable
    {
        #region Fields
        
        private byte[] _script;
        private Hash160 _scriptHash;
        private bool _disposed;
        
        #endregion
        
        #region Properties
        
        /// <summary>
        /// Gets the verification script as a byte array.
        /// </summary>
        public byte[] Script
        {
            get
            {
                ValidateNotDisposed();
                return _script?.ToArray() ?? Array.Empty<byte>();
            }
        }
        
        /// <summary>
        /// Gets the length of the script in bytes.
        /// </summary>
        public int Length
        {
            get
            {
                ValidateNotDisposed();
                return _script?.Length ?? 0;
            }
        }
        
        /// <summary>
        /// Gets the serialization size of this verification script.
        /// </summary>
        public int Size
        {
            get
            {
                ValidateNotDisposed();
                return Length.GetVarSize() + Length;
            }
        }
        
        /// <summary>
        /// Gets the script hash of this verification script.
        /// The script hash is computed once and cached for performance.
        /// </summary>
        public Hash160 ScriptHash
        {
            get
            {
                ValidateNotDisposed();
                
                if (_scriptHash == null && _script != null && _script.Length > 0)
                {
                    try
                    {
                        _scriptHash = Hash160.FromScript(_script);
                        Extensions.SafeLog($"VerificationScript: Computed script hash {_scriptHash}", LogLevel.Verbose);
                    }
                    catch (Exception ex)
                    {
                        Extensions.SafeLog($"VerificationScript: Error computing script hash: {ex.Message}", LogLevel.Error);
                        throw new IllegalStateException($"Failed to compute script hash: {ex.Message}", ex);
                    }
                }
                
                return _scriptHash;
            }
        }
        
        /// <summary>
        /// Checks if this is an empty verification script.
        /// </summary>
        public bool IsEmpty => _script == null || _script.Length == 0;
        
        /// <summary>
        /// Checks if the verification script has been disposed.
        /// </summary>
        public bool IsDisposed => _disposed;
        
        #endregion
        
        #region Constructors
        
        /// <summary>
        /// Creates an empty verification script.
        /// </summary>
        public VerificationScript()
        {
            _script = Array.Empty<byte>();
            Extensions.SafeLog("VerificationScript: Created empty verification script");
        }
        
        /// <summary>
        /// Creates a verification script from the given byte array.
        /// </summary>
        /// <param name="script">The verification script bytes</param>
        public VerificationScript(byte[] script)
        {
            _script = script?.ToArray() ?? Array.Empty<byte>();
            Extensions.SafeLog($"VerificationScript: Created verification script with {_script.Length} bytes");
        }
        
        /// <summary>
        /// Creates a verification script for a single public key.
        /// </summary>
        /// <param name="publicKey">The public key for signature verification</param>
        public VerificationScript(ECPoint publicKey)
        {
            if (publicKey == null)
                throw new ArgumentNullException(nameof(publicKey));
            
            try
            {
                _script = ScriptBuilder.BuildVerificationScript(publicKey.GetEncoded(true));
                Extensions.SafeLog($"VerificationScript: Created single-sig verification script for public key {publicKey.ToHexString()}");
            }
            catch (Exception ex)
            {
                Extensions.SafeLog($"VerificationScript: Error creating single-sig script: {ex.Message}", LogLevel.Error);
                throw new IllegalArgumentException($"Failed to create verification script for public key: {ex.Message}", ex);
            }
        }
        
        /// <summary>
        /// Creates a multi-signature verification script.
        /// </summary>
        /// <param name="publicKeys">The public keys participating in multi-sig</param>
        /// <param name="signingThreshold">The minimum number of signatures required</param>
        public VerificationScript(ECPoint[] publicKeys, int signingThreshold)
        {
            ValidateMultiSigParameters(publicKeys, signingThreshold);
            
            try
            {
                _script = ScriptBuilder.BuildVerificationScript(publicKeys, signingThreshold);
                Extensions.SafeLog($"VerificationScript: Created multi-sig verification script with {publicKeys.Length} keys, threshold {signingThreshold}");
            }
            catch (Exception ex)
            {
                Extensions.SafeLog($"VerificationScript: Error creating multi-sig script: {ex.Message}", LogLevel.Error);
                throw new IllegalArgumentException($"Failed to create multi-sig verification script: {ex.Message}", ex);
            }
        }
        
        #endregion
        
        #region Static Factory Methods
        
        /// <summary>
        /// Creates a verification script from a hex string.
        /// </summary>
        /// <param name="hexScript">The script in hexadecimal format</param>
        /// <returns>The verification script</returns>
        public static VerificationScript FromHex(string hexScript)
        {
            if (string.IsNullOrEmpty(hexScript))
                return new VerificationScript();
            
            try
            {
                var bytes = hexScript.HexStringToByteArray();
                return new VerificationScript(bytes);
            }
            catch (Exception ex)
            {
                Extensions.SafeLog($"VerificationScript: Error parsing hex script: {ex.Message}", LogLevel.Error);
                throw new IllegalArgumentException($"Invalid hex script: {ex.Message}", ex);
            }
        }
        
        /// <summary>
        /// Creates a single-signature verification script from a public key.
        /// </summary>
        /// <param name="publicKey">The public key bytes (compressed format)</param>
        /// <returns>The verification script</returns>
        public static VerificationScript FromPublicKey(byte[] publicKey)
        {
            if (publicKey == null)
                throw new ArgumentNullException(nameof(publicKey));
            
            if (publicKey.Length != NeoConstants.PUBLIC_KEY_SIZE_COMPRESSED)
                throw new ArgumentException($"Public key must be {NeoConstants.PUBLIC_KEY_SIZE_COMPRESSED} bytes (compressed format)");
            
            try
            {
                var ecPoint = ECPoint.DecodePoint(publicKey);
                return new VerificationScript(ecPoint);
            }
            catch (Exception ex)
            {
                Extensions.SafeLog($"VerificationScript: Error creating from public key: {ex.Message}", LogLevel.Error);
                throw new IllegalArgumentException($"Invalid public key: {ex.Message}", ex);
            }
        }
        
        /// <summary>
        /// Creates a multi-signature verification script from public key bytes.
        /// </summary>
        /// <param name="publicKeys">Array of compressed public key bytes</param>
        /// <param name="signingThreshold">Minimum signatures required</param>
        /// <returns>The verification script</returns>
        public static VerificationScript FromPublicKeys(byte[][] publicKeys, int signingThreshold)
        {
            if (publicKeys == null)
                throw new ArgumentNullException(nameof(publicKeys));
            
            try
            {
                var ecPoints = publicKeys.Select(ECPoint.DecodePoint).ToArray();
                return new VerificationScript(ecPoints, signingThreshold);
            }
            catch (Exception ex)
            {
                Extensions.SafeLog($"VerificationScript: Error creating multi-sig from public keys: {ex.Message}", LogLevel.Error);
                throw new IllegalArgumentException($"Invalid public keys: {ex.Message}", ex);
            }
        }
        
        #endregion
        
        #region Script Analysis
        
        /// <summary>
        /// Gets the signing threshold for this verification script.
        /// </summary>
        /// <returns>The number of signatures required</returns>
        public int GetSigningThreshold()
        {
            ValidateNotDisposed();
            
            if (IsEmpty)
                return 0;
            
            if (IsSingleSigScript())
                return 1;
            
            if (IsMultiSigScript())
            {
                try
                {
                    using var reader = new BinaryReader(_script);
                    return ReadPushInteger(reader);
                }
                catch (Exception ex)
                {
                    Extensions.SafeLog($"VerificationScript: Error reading signing threshold: {ex.Message}", LogLevel.Error);
                    throw new IllegalStateException($"Cannot determine signing threshold: {ex.Message}", ex);
                }
            }
            
            throw new IllegalStateException("Script format does not match single-sig or multi-sig pattern");
        }
        
        /// <summary>
        /// Gets the number of accounts participating in this verification script.
        /// </summary>
        /// <returns>The number of participating accounts</returns>
        public async Task<int> GetAccountCountAsync()
        {
            ValidateNotDisposed();
            
            var publicKeys = await GetPublicKeysAsync();
            return publicKeys.Length;
        }
        
        /// <summary>
        /// Checks if this is a single signature verification script.
        /// </summary>
        /// <returns>True if single-sig, false otherwise</returns>
        public bool IsSingleSigScript()
        {
            ValidateNotDisposed();
            
            if (_script == null || _script.Length != 40)
                return false;
            
            // Validate single-sig pattern: PUSHDATA1 33 <33-byte-pubkey> SYSCALL <4-byte-hash>
            var interopHash = _script.Skip(_script.Length - 4).Take(4).ToArray().ToHexString();
            
            return _script[0] == (byte)OpCode.PUSHDATA1 &&
                   _script[1] == 33 &&
                   _script[35] == (byte)OpCode.SYSCALL &&
                   interopHash.Equals(InteropService.SystemCryptoCheckSig.GetHashString(), StringComparison.OrdinalIgnoreCase);
        }
        
        /// <summary>
        /// Checks if this is a multi-signature verification script.
        /// </summary>
        /// <returns>True if multi-sig, false otherwise</returns>
        public bool IsMultiSigScript()
        {
            ValidateNotDisposed();
            
            if (_script == null || _script.Length < 42)
                return false;
            
            try
            {
                using var reader = new BinaryReader(_script);
                
                // Read signing threshold (must be positive)
                var threshold = ReadPushInteger(reader);
                if (threshold <= 0 || threshold > NeoConstants.MAX_PUBLIC_KEYS_PER_MULTISIG_ACCOUNT)
                    return false;
                
                // Count public keys
                int keyCount = 0;
                while (reader.Available > 0 && reader.ReadByte() == (byte)OpCode.PUSHDATA1)
                {
                    if (reader.Available < 34) return false; // Need at least length byte + 33 key bytes
                    
                    if (reader.ReadByte() != 33) return false; // Public key length must be 33
                    
                    // Validate that this is a valid EC point
                    var keyBytes = reader.ReadBytes(33);
                    if (!IsValidECPoint(keyBytes)) return false;
                    
                    keyCount++;
                    reader.Mark(); // Mark position for potential reset
                }
                
                // Validate key count constraints
                if (keyCount < threshold || keyCount > NeoConstants.MAX_PUBLIC_KEYS_PER_MULTISIG_ACCOUNT)
                    return false;
                
                // Reset and verify final structure
                reader.Reset();
                if (ReadPushInteger(reader) != keyCount) return false;
                
                if (reader.Available < 5) return false; // Need SYSCALL + 4-byte hash
                
                if (reader.ReadByte() != (byte)OpCode.SYSCALL) return false;
                
                var hashBytes = reader.ReadBytes(4);
                var expectedHash = InteropService.SystemCryptoCheckMultisig.GetHash();
                
                return hashBytes.SequenceEqual(expectedHash);
            }
            catch (Exception ex)
            {
                Extensions.SafeLog($"VerificationScript: Error validating multi-sig script: {ex.Message}", LogLevel.Verbose);
                return false;
            }
        }
        
        /// <summary>
        /// Extracts the public keys from this verification script.
        /// </summary>
        /// <returns>Array of public keys in natural ordering</returns>
        public async Task<ECPoint[]> GetPublicKeysAsync()
        {
            ValidateNotDisposed();
            
            return await Task.Run(() => GetPublicKeys());
        }
        
        /// <summary>
        /// Synchronously extracts the public keys from this verification script.
        /// </summary>
        /// <returns>Array of public keys in natural ordering</returns>
        public ECPoint[] GetPublicKeys()
        {
            ValidateNotDisposed();
            
            if (IsEmpty)
                return Array.Empty<ECPoint>();
            
            try
            {
                using var reader = new BinaryReader(_script);
                
                if (IsSingleSigScript())
                {
                    // Skip PUSHDATA1 and length byte
                    reader.ReadBytes(2);
                    var keyBytes = reader.ReadBytes(33);
                    return new[] { ECPoint.DecodePoint(keyBytes) };
                }
                else if (IsMultiSigScript())
                {
                    var publicKeys = new List<ECPoint>();
                    
                    // Skip signing threshold
                    ReadPushInteger(reader);
                    
                    // Read all public keys
                    while (reader.Available > 0 && reader.ReadByte() == (byte)OpCode.PUSHDATA1)
                    {
                        if (reader.ReadByte() == 33) // Public key length
                        {
                            var keyBytes = reader.ReadBytes(33);
                            publicKeys.Add(ECPoint.DecodePoint(keyBytes));
                        }
                        else
                        {
                            break; // End of public keys
                        }
                    }
                    
                    return publicKeys.ToArray();
                }
                
                throw new IllegalStateException("Script format does not match single-sig or multi-sig pattern");
            }
            catch (Exception ex)
            {
                Extensions.SafeLog($"VerificationScript: Error extracting public keys: {ex.Message}", LogLevel.Error);
                throw new IllegalStateException($"Cannot extract public keys: {ex.Message}", ex);
            }
        }
        
        #endregion
        
        #region Validation
        
        /// <summary>
        /// Validates the verification script structure and content.
        /// </summary>
        /// <returns>True if valid, false otherwise</returns>
        public bool IsValid()
        {
            ValidateNotDisposed();
            
            if (IsEmpty)
                return true; // Empty scripts are considered valid
            
            try
            {
                // Try to determine script type and validate structure
                if (IsSingleSigScript() || IsMultiSigScript())
                {
                    // Verify we can extract public keys without error
                    var publicKeys = GetPublicKeys();
                    return publicKeys.Length > 0;
                }
                
                // For non-standard scripts, just check basic structure
                return _script.Length > 0;
            }
            catch (Exception ex)
            {
                Extensions.SafeLog($"VerificationScript: Validation failed: {ex.Message}", LogLevel.Verbose);
                return false;
            }
        }
        
        /// <summary>
        /// Validates the script and throws an exception if invalid.
        /// </summary>
        /// <exception cref="IllegalStateException">If the script is invalid</exception>
        public void ValidateScript()
        {
            if (!IsValid())
            {
                throw new IllegalStateException("Verification script is invalid or malformed");
            }
        }
        
        #endregion
        
        #region Serialization
        
        /// <summary>
        /// Serializes the verification script to a binary writer.
        /// </summary>
        /// <param name="writer">The binary writer</param>
        public void Serialize(BinaryWriter writer)
        {
            ValidateNotDisposed();
            
            if (writer == null)
                throw new ArgumentNullException(nameof(writer));
            
            writer.WriteVarBytes(_script ?? Array.Empty<byte>());
        }
        
        /// <summary>
        /// Deserializes a verification script from a binary reader.
        /// </summary>
        /// <param name="reader">The binary reader</param>
        /// <returns>The deserialized verification script</returns>
        public static VerificationScript Deserialize(BinaryReader reader)
        {
            if (reader == null)
                throw new ArgumentNullException(nameof(reader));
            
            try
            {
                var scriptBytes = reader.ReadVarBytes();
                return new VerificationScript(scriptBytes);
            }
            catch (Exception ex)
            {
                Extensions.SafeLog($"VerificationScript: Deserialization error: {ex.Message}", LogLevel.Error);
                throw new IllegalArgumentException($"Failed to deserialize verification script: {ex.Message}", ex);
            }
        }
        
        #endregion
        
        #region Conversion Methods
        
        /// <summary>
        /// Converts the script to a hexadecimal string.
        /// </summary>
        /// <returns>Hex string representation</returns>
        public string ToHexString()
        {
            ValidateNotDisposed();
            return _script?.ToHexString() ?? string.Empty;
        }
        
        /// <summary>
        /// Converts the script to a human-readable string using the ScriptReader.
        /// </summary>
        /// <returns>Human-readable script representation</returns>
        public async Task<string> ToReadableStringAsync()
        {
            ValidateNotDisposed();
            
            if (IsEmpty)
                return "Empty Script";
            
            return await ScriptReader.ConvertToOpCodeStringAsync(_script);
        }
        
        #endregion
        
        #region Private Helpers
        
        /// <summary>
        /// Validates multi-sig parameters.
        /// </summary>
        private static void ValidateMultiSigParameters(ECPoint[] publicKeys, int signingThreshold)
        {
            if (publicKeys == null)
                throw new ArgumentNullException(nameof(publicKeys));
            
            if (signingThreshold < 1 || signingThreshold > publicKeys.Length)
                throw new ArgumentException("Signing threshold must be between 1 and the number of public keys");
            
            if (publicKeys.Length > NeoConstants.MAX_PUBLIC_KEYS_PER_MULTISIG_ACCOUNT)
                throw new ArgumentException($"Too many public keys (max {NeoConstants.MAX_PUBLIC_KEYS_PER_MULTISIG_ACCOUNT})");
        }
        
        /// <summary>
        /// Reads a pushed integer from the script.
        /// </summary>
        private int ReadPushInteger(BinaryReader reader)
        {
            var opCode = (OpCode)reader.ReadByte();
            
            if (opCode >= OpCode.PUSHM1 && opCode <= OpCode.PUSH16)
            {
                return opCode == OpCode.PUSHM1 ? -1 : (int)opCode - (int)OpCode.PUSH0;
            }
            
            return opCode switch
            {
                OpCode.PUSHINT8 => reader.ReadSByte(),
                OpCode.PUSHINT16 => reader.ReadInt16(),
                OpCode.PUSHINT32 => reader.ReadInt32(),
                _ => throw new IllegalArgumentException($"Unexpected opcode for integer: {opCode}")
            };
        }
        
        /// <summary>
        /// Validates if bytes represent a valid EC point.
        /// </summary>
        private bool IsValidECPoint(byte[] keyBytes)
        {
            if (keyBytes == null || keyBytes.Length != 33)
                return false;
            
            try
            {
                ECPoint.DecodePoint(keyBytes);
                return true;
            }
            catch
            {
                return false;
            }
        }
        
        /// <summary>
        /// Validates that the script hasn't been disposed.
        /// </summary>
        private void ValidateNotDisposed()
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(VerificationScript));
        }
        
        #endregion
        
        #region Equality and Hashing
        
        /// <summary>
        /// Determines whether the specified verification script is equal to the current script.
        /// </summary>
        /// <param name="other">The verification script to compare</param>
        /// <returns>True if equal, false otherwise</returns>
        public bool Equals(VerificationScript other)
        {
            if (other == null) return false;
            if (ReferenceEquals(this, other)) return true;
            
            ValidateNotDisposed();
            other.ValidateNotDisposed();
            
            return (_script?.SequenceEqual(other._script) ?? other._script == null);
        }
        
        /// <summary>
        /// Determines whether the specified object is equal to the current script.
        /// </summary>
        /// <param name="obj">The object to compare</param>
        /// <returns>True if equal, false otherwise</returns>
        public override bool Equals(object obj)
        {
            return Equals(obj as VerificationScript);
        }
        
        /// <summary>
        /// Returns a hash code for the current script.
        /// </summary>
        /// <returns>Hash code</returns>
        public override int GetHashCode()
        {
            ValidateNotDisposed();
            return _script?.GetHashCode() ?? 0;
        }
        
        /// <summary>
        /// Equality operator.
        /// </summary>
        public static bool operator ==(VerificationScript left, VerificationScript right)
        {
            return left?.Equals(right) ?? right == null;
        }
        
        /// <summary>
        /// Inequality operator.
        /// </summary>
        public static bool operator !=(VerificationScript left, VerificationScript right)
        {
            return !(left == right);
        }
        
        #endregion
        
        #region IDisposable Implementation
        
        /// <summary>
        /// Disposes of the verification script resources.
        /// </summary>
        public void Dispose()
        {
            if (!_disposed)
            {
                _script = null;
                _scriptHash = null;
                _disposed = true;
                Extensions.SafeLog("VerificationScript disposed");
            }
        }
        
        #endregion
        
        #region ToString Override
        
        /// <summary>
        /// Returns a string representation of the verification script.
        /// </summary>
        /// <returns>String representation</returns>
        public override string ToString()
        {
            if (_disposed)
                return "VerificationScript(Disposed)";
            
            if (IsEmpty)
                return "VerificationScript(Empty)";
            
            try
            {
                var type = IsSingleSigScript() ? "SingleSig" : 
                          IsMultiSigScript() ? "MultiSig" : "Custom";
                var hash = ScriptHash?.ToString() ?? "Unknown";
                
                return $"VerificationScript({type}, Hash: {hash}, Size: {Length} bytes)";
            }
            catch
            {
                return $"VerificationScript(Size: {Length} bytes)";
            }
        }
        
        #endregion
    }
}
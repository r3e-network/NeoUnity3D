using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using Neo.Unity.SDK.Crypto;
using NeoUnity.Exceptions;
using NeoUnity.Serialization;
using NeoUnity.Utils;

namespace Neo.Unity.SDK.Script
{
    /// <summary>
    /// Represents an invocation script used in Neo transactions.
    /// Invocation scripts typically contain input data (like signatures) that are consumed by verification scripts.
    /// This is the counterpart to VerificationScript and together they form a complete witness.
    /// </summary>
    [System.Serializable]
    public class InvocationScript : IEquatable<InvocationScript>, IDisposable, INeoSerializable
    {
        #region Fields
        
        private byte[] _script;
        private bool _disposed;
        
        #endregion
        
        #region Properties
        
        /// <summary>
        /// Gets the invocation script as a byte array.
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
        /// Gets the serialization size of this invocation script.
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
        /// Checks if this is an empty invocation script.
        /// </summary>
        public bool IsEmpty => _script == null || _script.Length == 0;
        
        /// <summary>
        /// Checks if the invocation script has been disposed.
        /// </summary>
        public bool IsDisposed => _disposed;
        
        #endregion
        
        #region Constructors
        
        /// <summary>
        /// Creates an empty invocation script.
        /// </summary>
        public InvocationScript()
        {
            _script = Array.Empty<byte>();
            Extensions.SafeLog("InvocationScript: Created empty invocation script");
        }
        
        /// <summary>
        /// Creates an invocation script with the given script bytes.
        /// It is recommended to use factory methods like FromSignature() when creating signature scripts.
        /// </summary>
        /// <param name="script">The script bytes</param>
        public InvocationScript(byte[] script)
        {
            _script = script?.ToArray() ?? Array.Empty<byte>();
            Extensions.SafeLog($"InvocationScript: Created invocation script with {_script.Length} bytes");
        }
        
        #endregion
        
        #region Static Factory Methods
        
        /// <summary>
        /// Creates an invocation script from a single signature.
        /// </summary>
        /// <param name="signature">The ECDSA signature</param>
        /// <returns>The invocation script containing the signature</returns>
        public static InvocationScript FromSignature(ECDSASignature signature)
        {
            if (signature == null)
                throw new ArgumentNullException(nameof(signature));
            
            try
            {
                using var builder = new ScriptBuilder();
                var signatureBytes = signature.ToByteArray();
                
                if (signatureBytes.Length != NeoConstants.SIGNATURE_SIZE)
                    throw new ArgumentException($"Signature must be {NeoConstants.SIGNATURE_SIZE} bytes");
                
                builder.PushData(signatureBytes);
                var result = new InvocationScript(builder.ToArray());
                
                Extensions.SafeLog($"InvocationScript: Created from signature ({signatureBytes.Length} bytes)");
                return result;
            }
            catch (Exception ex)
            {
                Extensions.SafeLog($"InvocationScript: Error creating from signature: {ex.Message}", LogLevel.Error);
                throw new IllegalArgumentException($"Failed to create invocation script from signature: {ex.Message}", ex);
            }
        }
        
        /// <summary>
        /// Creates an invocation script from signature bytes.
        /// </summary>
        /// <param name="signatureBytes">The signature as a byte array (64 bytes)</param>
        /// <returns>The invocation script containing the signature</returns>
        public static InvocationScript FromSignature(byte[] signatureBytes)
        {
            if (signatureBytes == null)
                throw new ArgumentNullException(nameof(signatureBytes));
            
            if (signatureBytes.Length != NeoConstants.SIGNATURE_SIZE)
                throw new ArgumentException($"Signature must be {NeoConstants.SIGNATURE_SIZE} bytes");
            
            try
            {
                var signature = ECDSASignature.FromByteArray(signatureBytes);
                return FromSignature(signature);
            }
            catch (Exception ex)
            {
                Extensions.SafeLog($"InvocationScript: Error creating from signature bytes: {ex.Message}", LogLevel.Error);
                throw new IllegalArgumentException($"Invalid signature bytes: {ex.Message}", ex);
            }
        }
        
        /// <summary>
        /// Creates an invocation script by signing a message with the given key pair.
        /// </summary>
        /// <param name="message">The message to sign</param>
        /// <param name="keyPair">The key pair to use for signing</param>
        /// <returns>The invocation script containing the signature</returns>
        public static async Task<InvocationScript> FromMessageAndKeyPairAsync(byte[] message, ECKeyPair keyPair)
        {
            if (message == null)
                throw new ArgumentNullException(nameof(message));
            
            if (keyPair == null)
                throw new ArgumentNullException(nameof(keyPair));
            
            try
            {
                var signature = await keyPair.SignAsync(message);
                var result = FromSignature(signature);
                
                Extensions.SafeLog($"InvocationScript: Created from message signing ({message.Length} bytes message)");
                return result;
            }
            catch (Exception ex)
            {
                Extensions.SafeLog($"InvocationScript: Error signing message: {ex.Message}", LogLevel.Error);
                throw new IllegalArgumentException($"Failed to create invocation script from message signing: {ex.Message}", ex);
            }
        }
        
        /// <summary>
        /// Synchronous version of FromMessageAndKeyPairAsync for simpler usage.
        /// </summary>
        /// <param name="message">The message to sign</param>
        /// <param name="keyPair">The key pair to use for signing</param>
        /// <returns>The invocation script containing the signature</returns>
        public static InvocationScript FromMessageAndKeyPair(byte[] message, ECKeyPair keyPair)
        {
            return FromMessageAndKeyPairAsync(message, keyPair).GetAwaiter().GetResult();
        }
        
        /// <summary>
        /// Creates an invocation script from multiple signatures (for multi-sig scenarios).
        /// </summary>
        /// <param name="signatures">The array of signatures</param>
        /// <returns>The invocation script containing all signatures</returns>
        public static InvocationScript FromSignatures(ECDSASignature[] signatures)
        {
            if (signatures == null)
                throw new ArgumentNullException(nameof(signatures));
            
            if (signatures.Length == 0)
                return new InvocationScript();
            
            if (signatures.Length > NeoConstants.MAX_PUBLIC_KEYS_PER_MULTISIG_ACCOUNT)
                throw new ArgumentException($"Too many signatures (max {NeoConstants.MAX_PUBLIC_KEYS_PER_MULTISIG_ACCOUNT})");
            
            try
            {
                using var builder = new ScriptBuilder();
                
                // Push signatures in reverse order (last signature gets pushed first)
                // This matches Neo's expected stack order for multi-sig verification
                for (int i = signatures.Length - 1; i >= 0; i--)
                {
                    if (signatures[i] != null)
                    {
                        var signatureBytes = signatures[i].ToByteArray();
                        builder.PushData(signatureBytes);
                    }
                    else
                    {
                        // Push null for missing signatures in multi-sig scenarios
                        builder.OpCode(OpCode.PUSHNULL);
                    }
                }
                
                var result = new InvocationScript(builder.ToArray());
                Extensions.SafeLog($"InvocationScript: Created from {signatures.Length} signatures");
                return result;
            }
            catch (Exception ex)
            {
                Extensions.SafeLog($"InvocationScript: Error creating from signatures: {ex.Message}", LogLevel.Error);
                throw new IllegalArgumentException($"Failed to create invocation script from signatures: {ex.Message}", ex);
            }
        }
        
        /// <summary>
        /// Creates an invocation script from signature byte arrays (for multi-sig scenarios).
        /// </summary>
        /// <param name="signatureArrays">Array of signature byte arrays</param>
        /// <returns>The invocation script containing all signatures</returns>
        public static InvocationScript FromSignatures(byte[][] signatureArrays)
        {
            if (signatureArrays == null)
                throw new ArgumentNullException(nameof(signatureArrays));
            
            try
            {
                var signatures = signatureArrays.Select(bytes => 
                    bytes != null ? ECDSASignature.FromByteArray(bytes) : null).ToArray();
                    
                return FromSignatures(signatures);
            }
            catch (Exception ex)
            {
                Extensions.SafeLog($"InvocationScript: Error creating from signature arrays: {ex.Message}", LogLevel.Error);
                throw new IllegalArgumentException($"Invalid signature arrays: {ex.Message}", ex);
            }
        }
        
        /// <summary>
        /// Creates an invocation script from a hex string.
        /// </summary>
        /// <param name="hexScript">The script in hexadecimal format</param>
        /// <returns>The invocation script</returns>
        public static InvocationScript FromHex(string hexScript)
        {
            if (string.IsNullOrEmpty(hexScript))
                return new InvocationScript();
            
            try
            {
                var bytes = hexScript.HexStringToByteArray();
                return new InvocationScript(bytes);
            }
            catch (Exception ex)
            {
                Extensions.SafeLog($"InvocationScript: Error parsing hex script: {ex.Message}", LogLevel.Error);
                throw new IllegalArgumentException($"Invalid hex script: {ex.Message}", ex);
            }
        }
        
        #endregion
        
        #region Signature Analysis
        
        /// <summary>
        /// Extracts signatures from this invocation script if it contains signature data.
        /// </summary>
        /// <returns>Array of signatures found in the script</returns>
        public ECDSASignature[] GetSignatures()
        {
            ValidateNotDisposed();
            
            if (IsEmpty)
                return Array.Empty<ECDSASignature>();
            
            try
            {
                var signatures = new List<ECDSASignature>();
                using var reader = new BinaryReader(_script);
                
                while (reader.Available > 0)
                {
                    var opCode = (OpCode)reader.ReadByte();
                    
                    if (opCode == OpCode.PUSHDATA1)
                    {
                        var length = reader.ReadByte();
                        if (length == NeoConstants.SIGNATURE_SIZE && reader.Available >= length)
                        {
                            var signatureBytes = reader.ReadBytes(length);
                            try
                            {
                                var signature = ECDSASignature.FromByteArray(signatureBytes);
                                signatures.Add(signature);
                                Extensions.SafeLog($"InvocationScript: Found signature at position {reader.Position - length - 2}", LogLevel.Verbose);
                            }
                            catch
                            {
                                // Not a valid signature, skip
                                Extensions.SafeLog($"InvocationScript: Invalid signature data at position {reader.Position - length - 2}", LogLevel.Verbose);
                            }
                        }
                        else
                        {
                            // Skip non-signature data
                            if (reader.Available >= length)
                                reader.ReadBytes(length);
                        }
                    }
                    else if (opCode == OpCode.PUSHNULL)
                    {
                        // Null signature (empty slot in multi-signature array)
                        signatures.Add(null);
                    }
                    else
                    {
                        // Handle other opcodes that might push signature-sized data
                        var operandInfo = opCode.GetOperandSize();
                        if (operandInfo.HasValue)
                        {
                            var operand = operandInfo.Value;
                            if (operand.Size > 0)
                            {
                                if (reader.Available >= operand.Size)
                                    reader.ReadBytes(operand.Size);
                            }
                            else if (operand.PrefixSize > 0)
                            {
                                if (reader.Available >= operand.PrefixSize)
                                {
                                    var dataLength = ReadOperandLength(reader, operand.PrefixSize);
                                    if (reader.Available >= dataLength)
                                        reader.ReadBytes(dataLength);
                                }
                            }
                        }
                    }
                }
                
                Extensions.SafeLog($"InvocationScript: Extracted {signatures.Count} signatures from script");
                return signatures.ToArray();
            }
            catch (Exception ex)
            {
                Extensions.SafeLog($"InvocationScript: Error extracting signatures: {ex.Message}", LogLevel.Error);
                return Array.Empty<ECDSASignature>();
            }
        }
        
        /// <summary>
        /// Asynchronously extracts signatures from this invocation script.
        /// </summary>
        /// <returns>Task that returns array of signatures found in the script</returns>
        public async Task<ECDSASignature[]> GetSignaturesAsync()
        {
            ValidateNotDisposed();
            
            // For complex scripts, process on background thread
            if (_script != null && _script.Length > 1000)
            {
                return await Task.Run(() => GetSignatures());
            }
            
            return GetSignatures();
        }
        
        /// <summary>
        /// Gets the number of signatures in this invocation script.
        /// </summary>
        /// <returns>The signature count</returns>
        public int GetSignatureCount()
        {
            return GetSignatures().Count(sig => sig != null);
        }
        
        /// <summary>
        /// Checks if this invocation script contains valid signature data.
        /// </summary>
        /// <returns>True if the script contains at least one valid signature</returns>
        public bool ContainsSignatures()
        {
            return GetSignatureCount() > 0;
        }
        
        #endregion
        
        #region Validation
        
        /// <summary>
        /// Validates the invocation script structure and content.
        /// </summary>
        /// <returns>True if the script appears to be valid</returns>
        public bool IsValid()
        {
            ValidateNotDisposed();
            
            if (IsEmpty)
                return true; // Empty scripts are valid
            
            try
            {
                // Try to parse the script without errors
                using var reader = new BinaryReader(_script);
                var position = 0;
                
                while (reader.Available > 0)
                {
                    var opCode = (OpCode)reader.ReadByte();
                    position++;
                    
                    var operandInfo = opCode.GetOperandSize();
                    if (operandInfo.HasValue)
                    {
                        var operand = operandInfo.Value;
                        
                        if (operand.Size > 0)
                        {
                            if (reader.Available < operand.Size)
                                return false; // Incomplete operand
                            reader.ReadBytes(operand.Size);
                        }
                        else if (operand.PrefixSize > 0)
                        {
                            if (reader.Available < operand.PrefixSize)
                                return false; // Incomplete length prefix
                            
                            var dataLength = ReadOperandLength(reader, operand.PrefixSize);
                            if (reader.Available < dataLength)
                                return false; // Incomplete data
                            
                            reader.ReadBytes(dataLength);
                        }
                    }
                }
                
                return true;
            }
            catch (Exception ex)
            {
                Extensions.SafeLog($"InvocationScript: Validation failed: {ex.Message}", LogLevel.Verbose);
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
                throw new IllegalStateException("Invocation script is invalid or malformed");
            }
        }
        
        #endregion
        
        #region Serialization
        
        /// <summary>
        /// Serializes the invocation script to a binary writer.
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
        /// Deserializes an invocation script from a binary reader.
        /// </summary>
        /// <param name="reader">The binary reader</param>
        /// <returns>The deserialized invocation script</returns>
        public static InvocationScript Deserialize(BinaryReader reader)
        {
            if (reader == null)
                throw new ArgumentNullException(nameof(reader));
            
            try
            {
                var scriptBytes = reader.ReadVarBytes();
                return new InvocationScript(scriptBytes);
            }
            catch (Exception ex)
            {
                Extensions.SafeLog($"InvocationScript: Deserialization error: {ex.Message}", LogLevel.Error);
                throw new IllegalArgumentException($"Failed to deserialize invocation script: {ex.Message}", ex);
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
        /// Reads operand length from binary reader based on prefix size.
        /// </summary>
        private int ReadOperandLength(BinaryReader reader, int prefixSize)
        {
            return prefixSize switch
            {
                1 => reader.ReadByte(),
                2 => reader.ReadUInt16(),
                4 => (int)reader.ReadUInt32(),
                _ => throw new IllegalArgumentException($"Unsupported operand prefix size: {prefixSize}")
            };
        }
        
        /// <summary>
        /// Validates that the script hasn't been disposed.
        /// </summary>
        private void ValidateNotDisposed()
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(InvocationScript));
        }
        
        #endregion
        
        #region Equality and Hashing
        
        /// <summary>
        /// Determines whether the specified invocation script is equal to the current script.
        /// </summary>
        /// <param name="other">The invocation script to compare</param>
        /// <returns>True if equal, false otherwise</returns>
        public bool Equals(InvocationScript other)
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
            return Equals(obj as InvocationScript);
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
        public static bool operator ==(InvocationScript left, InvocationScript right)
        {
            return left?.Equals(right) ?? right == null;
        }
        
        /// <summary>
        /// Inequality operator.
        /// </summary>
        public static bool operator !=(InvocationScript left, InvocationScript right)
        {
            return !(left == right);
        }
        
        #endregion
        
        #region IDisposable Implementation
        
        /// <summary>
        /// Disposes of the invocation script resources.
        /// </summary>
        public void Dispose()
        {
            if (!_disposed)
            {
                _script = null;
                _disposed = true;
                Extensions.SafeLog("InvocationScript disposed");
            }
        }
        
        #endregion
        
        #region ToString Override
        
        /// <summary>
        /// Returns a string representation of the invocation script.
        /// </summary>
        /// <returns>String representation</returns>
        public override string ToString()
        {
            if (_disposed)
                return "InvocationScript(Disposed)";
            
            if (IsEmpty)
                return "InvocationScript(Empty)";
            
            try
            {
                var signatureCount = GetSignatureCount();
                var type = signatureCount > 0 ? $"{signatureCount} signature(s)" : "Custom";
                
                return $"InvocationScript({type}, Size: {Length} bytes)";
            }
            catch
            {
                return $"InvocationScript(Size: {Length} bytes)";
            }
        }
        
        #endregion
    }
    
    #region Utility Classes
    
    /// <summary>
    /// Utility class for working with invocation scripts in Unity.
    /// </summary>
    public static class InvocationScriptUtils
    {
        /// <summary>
        /// Creates a batch of invocation scripts from multiple signature sets.
        /// Useful for creating multiple witnesses at once.
        /// </summary>
        /// <param name="signatureSets">Array of signature arrays</param>
        /// <returns>Array of invocation scripts</returns>
        public static InvocationScript[] CreateBatchFromSignatures(ECDSASignature[][] signatureSets)
        {
            if (signatureSets == null)
                throw new ArgumentNullException(nameof(signatureSets));
            
            return signatureSets.Select(InvocationScript.FromSignatures).ToArray();
        }
        
        /// <summary>
        /// Validates a batch of invocation scripts.
        /// </summary>
        /// <param name="scripts">The scripts to validate</param>
        /// <returns>True if all scripts are valid</returns>
        public static bool ValidateBatch(InvocationScript[] scripts)
        {
            if (scripts == null)
                return false;
            
            return scripts.All(script => script?.IsValid() ?? false);
        }
        
        /// <summary>
        /// Gets the total size of a batch of invocation scripts.
        /// </summary>
        /// <param name="scripts">The scripts to measure</param>
        /// <returns>Total size in bytes</returns>
        public static int GetBatchSize(InvocationScript[] scripts)
        {
            if (scripts == null)
                return 0;
            
            return scripts.Sum(script => script?.Size ?? 0);
        }
        
        /// <summary>
        /// Logs detailed information about an invocation script for debugging.
        /// </summary>
        /// <param name="script">The script to analyze</param>
        /// <param name="label">Optional label for the log entry</param>
        public static void LogScriptInfo(InvocationScript script, string label = null)
        {
            if (script == null)
            {
                Debug.Log($"{label ?? "InvocationScript"}: null");
                return;
            }
            
            var info = new System.Text.StringBuilder();
            info.AppendLine($"{label ?? "InvocationScript"} Analysis:");
            info.AppendLine($"  Size: {script.Length} bytes");
            info.AppendLine($"  Empty: {script.IsEmpty}");
            info.AppendLine($"  Valid: {script.IsValid()}");
            info.AppendLine($"  Signature Count: {script.GetSignatureCount()}");
            info.AppendLine($"  Contains Signatures: {script.ContainsSignatures()}");
            info.AppendLine($"  Hex: {script.ToHexString()}");
            
            Debug.Log(info.ToString());
        }
    }
    
    #endregion
}
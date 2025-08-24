using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Neo.Unity.SDK.Types;
using Neo.Unity.SDK.Crypto;
using NeoUnity.Serialization;
using NeoUnity.Types;
using NeoUnity.Utils;

namespace Neo.Unity.SDK.Script
{
    /// <summary>
    /// Builds Neo VM scripts for contract invocations, verification scripts, and other blockchain operations.
    /// Supports method chaining for fluent API usage with proper async/await patterns for Unity compatibility.
    /// </summary>
    public class ScriptBuilder : IDisposable
    {
        #region Fields
        
        private readonly BinaryWriter _writer;
        private bool _disposed;
        
        #endregion
        
        #region Properties
        
        /// <summary>
        /// Gets the current size of the script in bytes.
        /// </summary>
        public int Size => _writer.Size;
        
        /// <summary>
        /// Checks if the script builder has been disposed.
        /// </summary>
        public bool IsDisposed => _disposed;
        
        #endregion
        
        #region Constructors
        
        /// <summary>
        /// Creates a new script builder.
        /// </summary>
        public ScriptBuilder()
        {
            _writer = new BinaryWriter();
        }
        
        /// <summary>
        /// Creates a new script builder with initial capacity.
        /// </summary>
        /// <param name="capacity">Initial capacity for the internal buffer</param>
        public ScriptBuilder(int capacity)
        {
            _writer = new BinaryWriter(capacity);
        }
        
        #endregion
        
        #region OpCode Operations
        
        /// <summary>
        /// Appends one or more OpCodes to the script.
        /// </summary>
        /// <param name="opCodes">The OpCodes to append</param>
        /// <returns>This ScriptBuilder for method chaining</returns>
        public ScriptBuilder OpCode(params OpCode[] opCodes)
        {
            ValidateNotDisposed();
            
            foreach (var opCode in opCodes)
            {
                _writer.WriteByte((byte)opCode);
                Extensions.SafeLog($"ScriptBuilder: Added OpCode {opCode} (0x{opCode:X2})", LogLevel.Verbose);
            }
            
            return this;
        }
        
        /// <summary>
        /// Appends an OpCode with its argument to the script.
        /// </summary>
        /// <param name="opCode">The OpCode to append</param>
        /// <param name="argument">The argument bytes for the OpCode</param>
        /// <returns>This ScriptBuilder for method chaining</returns>
        public ScriptBuilder OpCode(OpCode opCode, byte[] argument)
        {
            ValidateNotDisposed();
            
            if (argument == null)
                throw new ArgumentNullException(nameof(argument));
            
            _writer.WriteByte((byte)opCode);
            _writer.Write(argument);
            
            Extensions.SafeLog($"ScriptBuilder: Added OpCode {opCode} with {argument.Length} bytes argument", LogLevel.Verbose);
            
            return this;
        }
        
        #endregion
        
        #region Contract Calls
        
        /// <summary>
        /// Appends a contract call to the script with full control over parameters.
        /// </summary>
        /// <param name="scriptHash">The script hash of the contract to call</param>
        /// <param name="method">The method name to call</param>
        /// <param name="parameters">The parameters for the method (optional)</param>
        /// <param name="callFlags">The call flags to use (default: All)</param>
        /// <returns>This ScriptBuilder for method chaining</returns>
        public async Task<ScriptBuilder> ContractCallAsync(Hash160 scriptHash, string method, 
            ContractParameter[] parameters = null, CallFlags callFlags = CallFlags.All)
        {
            ValidateNotDisposed();
            
            if (scriptHash == null)
                throw new ArgumentNullException(nameof(scriptHash));
            
            if (string.IsNullOrEmpty(method))
                throw new ArgumentException("Method name cannot be null or empty", nameof(method));
            
            try
            {
                // Push parameters if any, otherwise push empty array
                if (parameters == null || parameters.Length == 0)
                {
                    OpCode(OpCode.NEWARRAY0);
                }
                else
                {
                    await PushParametersAsync(parameters);
                }
                
                // Push call flags, method name, and script hash
                PushInteger((int)callFlags);
                PushData(method);
                PushData(scriptHash.ToLittleEndianArray());
                
                // Add syscall to System.Contract.Call
                SysCall(InteropService.SystemContractCall);
                
                Extensions.SafeLog($"ScriptBuilder: Added contract call to {scriptHash} method '{method}' with {parameters?.Length ?? 0} parameters");
                
                return this;
            }
            catch (Exception ex)
            {
                Extensions.SafeLog($"ScriptBuilder: Error creating contract call: {ex.Message}", LogLevel.Error);
                throw;
            }
        }
        
        /// <summary>
        /// Synchronous version of contract call for simpler usage.
        /// </summary>
        /// <param name="scriptHash">The script hash of the contract to call</param>
        /// <param name="method">The method name to call</param>
        /// <param name="parameters">The parameters for the method (optional)</param>
        /// <param name="callFlags">The call flags to use (default: All)</param>
        /// <returns>This ScriptBuilder for method chaining</returns>
        public ScriptBuilder ContractCall(Hash160 scriptHash, string method, 
            ContractParameter[] parameters = null, CallFlags callFlags = CallFlags.All)
        {
            return ContractCallAsync(scriptHash, method, parameters, callFlags).GetAwaiter().GetResult();
        }
        
        #endregion
        
        #region System Calls
        
        /// <summary>
        /// Adds a system call (syscall) to the script.
        /// </summary>
        /// <param name="service">The interop service to call</param>
        /// <returns>This ScriptBuilder for method chaining</returns>
        public ScriptBuilder SysCall(InteropService service)
        {
            ValidateNotDisposed();
            
            OpCode(OpCode.SYSCALL);
            _writer.Write(service.GetHash());
            
            Extensions.SafeLog($"ScriptBuilder: Added syscall to {service.GetServiceName()}");
            
            return this;
        }
        
        #endregion
        
        #region Data Push Operations
        
        /// <summary>
        /// Pushes parameters onto the stack and packs them into an array.
        /// </summary>
        /// <param name="parameters">The parameters to push</param>
        /// <returns>This ScriptBuilder for method chaining</returns>
        public async Task<ScriptBuilder> PushParametersAsync(ContractParameter[] parameters)
        {
            ValidateNotDisposed();
            
            if (parameters == null)
                throw new ArgumentNullException(nameof(parameters));
            
            // Push each parameter individually
            foreach (var param in parameters)
            {
                await PushParameterAsync(param);
            }
            
            // Push parameter count and pack into array
            PushInteger(parameters.Length);
            OpCode(OpCode.PACK);
            
            Extensions.SafeLog($"ScriptBuilder: Pushed {parameters.Length} parameters");
            
            return this;
        }
        
        /// <summary>
        /// Pushes a single parameter onto the stack.
        /// </summary>
        /// <param name="parameter">The parameter to push</param>
        /// <returns>This ScriptBuilder for method chaining</returns>
        public async Task<ScriptBuilder> PushParameterAsync(ContractParameter parameter)
        {
            ValidateNotDisposed();
            
            if (parameter?.Value == null)
            {
                return OpCode(OpCode.PUSHNULL);
            }
            
            return parameter.Type switch
            {
                ContractParameterType.ByteArray or 
                ContractParameterType.Signature or 
                ContractParameterType.PublicKey => PushData((byte[])parameter.Value),
                
                ContractParameterType.Boolean => PushBoolean((bool)parameter.Value),
                
                ContractParameterType.Integer => parameter.Value switch
                {
                    BigInteger bigInt => PushInteger(bigInt),
                    int intValue => PushInteger(intValue),
                    long longValue => PushInteger(longValue),
                    byte byteValue => PushInteger(byteValue),
                    _ => throw new ArgumentException($"Unsupported integer type: {parameter.Value.GetType()}")
                },
                
                ContractParameterType.Hash160 => PushData(((Hash160)parameter.Value).ToLittleEndianArray()),
                ContractParameterType.Hash256 => PushData(((Hash256)parameter.Value).ToLittleEndianArray()),
                ContractParameterType.String => PushData((string)parameter.Value),
                
                ContractParameterType.Array => await PushArrayAsync((List<ContractParameter>)parameter.Value),
                ContractParameterType.Map => await PushMapAsync((Dictionary<ContractParameter, ContractParameter>)parameter.Value),
                
                ContractParameterType.Any => this, // Do nothing for Any type
                
                _ => throw new ArgumentException($"Parameter type '{parameter.Type}' not supported.")
            };
        }
        
        /// <summary>
        /// Pushes an integer value onto the stack with optimal encoding.
        /// </summary>
        /// <param name="value">The integer value to push</param>
        /// <returns>This ScriptBuilder for method chaining</returns>
        public ScriptBuilder PushInteger(BigInteger value)
        {
            ValidateNotDisposed();
            
            // Use optimized opcodes for small integers
            if (value >= -1 && value <= 16)
            {
                var opCodeValue = (byte)(OpCode.PUSH0 + (int)value);
                if (value == -1) opCodeValue = (byte)OpCode.PUSHM1;
                
                _writer.WriteByte(opCodeValue);
                Extensions.SafeLog($"ScriptBuilder: Pushed small integer {value} using optimized opcode");
                return this;
            }
            
            // Convert to minimal byte representation
            var bytes = value.ToByteArray();
            if (!BitConverter.IsLittleEndian)
                Array.Reverse(bytes);
            
            // Choose appropriate push instruction based on byte count
            return bytes.Length switch
            {
                1 => OpCode(OpCode.PUSHINT8, bytes),
                2 => OpCode(OpCode.PUSHINT16, bytes),
                <= 4 => OpCode(OpCode.PUSHINT32, PadNumber(value, 4)),
                <= 8 => OpCode(OpCode.PUSHINT64, PadNumber(value, 8)),
                <= 16 => OpCode(OpCode.PUSHINT128, PadNumber(value, 16)),
                <= 32 => OpCode(OpCode.PUSHINT256, PadNumber(value, 32)),
                _ => throw new ArgumentException($"Integer value {value} is too large (max 32 bytes)")
            };
        }
        
        /// <summary>
        /// Pushes an integer value onto the stack.
        /// </summary>
        /// <param name="value">The integer value to push</param>
        /// <returns>This ScriptBuilder for method chaining</returns>
        public ScriptBuilder PushInteger(int value)
        {
            return PushInteger(new BigInteger(value));
        }
        
        /// <summary>
        /// Pushes a boolean value onto the stack.
        /// </summary>
        /// <param name="value">The boolean value to push</param>
        /// <returns>This ScriptBuilder for method chaining</returns>
        public ScriptBuilder PushBoolean(bool value)
        {
            ValidateNotDisposed();
            return OpCode(value ? OpCode.PUSH1 : OpCode.PUSH0);
        }
        
        /// <summary>
        /// Pushes string data onto the stack with UTF-8 encoding.
        /// </summary>
        /// <param name="data">The string to push</param>
        /// <returns>This ScriptBuilder for method chaining</returns>
        public ScriptBuilder PushData(string data)
        {
            ValidateNotDisposed();
            
            if (data == null)
                throw new ArgumentNullException(nameof(data));
            
            var bytes = Encoding.UTF8.GetBytes(data);
            return PushData(bytes);
        }
        
        /// <summary>
        /// Pushes byte data onto the stack with length-prefixed encoding.
        /// </summary>
        /// <param name="data">The byte data to push</param>
        /// <returns>This ScriptBuilder for method chaining</returns>
        public ScriptBuilder PushData(byte[] data)
        {
            ValidateNotDisposed();
            
            if (data == null)
                throw new ArgumentNullException(nameof(data));
            
            // Choose appropriate push instruction based on data length
            if (data.Length < 256)
            {
                OpCode(OpCode.PUSHDATA1);
                _writer.WriteByte((byte)data.Length);
                _writer.Write(data);
            }
            else if (data.Length < 65536)
            {
                OpCode(OpCode.PUSHDATA2);
                _writer.WriteUInt16((ushort)data.Length);
                _writer.Write(data);
            }
            else
            {
                OpCode(OpCode.PUSHDATA4);
                _writer.WriteInt32(data.Length);
                _writer.Write(data);
            }
            
            Extensions.SafeLog($"ScriptBuilder: Pushed {data.Length} bytes of data");
            
            return this;
        }
        
        /// <summary>
        /// Pushes an array of contract parameters onto the stack.
        /// </summary>
        /// <param name="array">The array to push</param>
        /// <returns>This ScriptBuilder for method chaining</returns>
        public async Task<ScriptBuilder> PushArrayAsync(List<ContractParameter> array)
        {
            ValidateNotDisposed();
            
            if (array == null || array.Count == 0)
            {
                return OpCode(OpCode.NEWARRAY0);
            }
            
            return await PushParametersAsync(array.ToArray());
        }
        
        /// <summary>
        /// Pushes a map of contract parameters onto the stack.
        /// </summary>
        /// <param name="map">The map to push</param>
        /// <returns>This ScriptBuilder for method chaining</returns>
        public async Task<ScriptBuilder> PushMapAsync(Dictionary<ContractParameter, ContractParameter> map)
        {
            ValidateNotDisposed();
            
            if (map == null)
                throw new ArgumentNullException(nameof(map));
            
            foreach (var kvp in map)
            {
                await PushParameterAsync(kvp.Value);
                await PushParameterAsync(kvp.Key);
            }
            
            PushInteger(map.Count);
            OpCode(OpCode.PACKMAP);
            
            Extensions.SafeLog($"ScriptBuilder: Pushed map with {map.Count} entries");
            
            return this;
        }
        
        #endregion
        
        #region Utility Operations
        
        /// <summary>
        /// Adds a pack operation to create an array from stack items.
        /// </summary>
        /// <returns>This ScriptBuilder for method chaining</returns>
        public ScriptBuilder Pack()
        {
            ValidateNotDisposed();
            return OpCode(OpCode.PACK);
        }
        
        #endregion
        
        #region Static Factory Methods
        
        /// <summary>
        /// Builds a verification script for a single public key.
        /// </summary>
        /// <param name="publicKey">The compressed public key bytes</param>
        /// <returns>The verification script bytes</returns>
        public static byte[] BuildVerificationScript(byte[] publicKey)
        {
            if (publicKey == null)
                throw new ArgumentNullException(nameof(publicKey));
                
            if (publicKey.Length != NeoConstants.PUBLIC_KEY_SIZE_COMPRESSED)
                throw new ArgumentException($"Public key must be {NeoConstants.PUBLIC_KEY_SIZE_COMPRESSED} bytes");
            
            using var builder = new ScriptBuilder();
            return builder.PushData(publicKey)
                         .SysCall(InteropService.SystemCryptoCheckSig)
                         .ToArray();
        }
        
        /// <summary>
        /// Builds a verification script for multiple public keys (multi-sig).
        /// </summary>
        /// <param name="publicKeys">The public keys in compressed format</param>
        /// <param name="signingThreshold">Required number of signatures</param>
        /// <returns>The verification script bytes</returns>
        public static byte[] BuildVerificationScript(ECPoint[] publicKeys, int signingThreshold)
        {
            if (publicKeys == null)
                throw new ArgumentNullException(nameof(publicKeys));
                
            if (signingThreshold < 1 || signingThreshold > publicKeys.Length)
                throw new ArgumentException("Invalid signing threshold");
                
            if (publicKeys.Length > NeoConstants.MAX_PUBLIC_KEYS_PER_MULTISIG_ACCOUNT)
                throw new ArgumentException($"Too many public keys (max {NeoConstants.MAX_PUBLIC_KEYS_PER_MULTISIG_ACCOUNT})");
            
            using var builder = new ScriptBuilder();
            
            // Push signing threshold
            builder.PushInteger(signingThreshold);
            
            // Push all public keys (sorted for consistency)
            Array.Sort(publicKeys, (a, b) => a.CompareTo(b));
            foreach (var pubKey in publicKeys)
            {
                builder.PushData(pubKey.GetEncoded(true));
            }
            
            // Push public key count and add multi-sig check
            builder.PushInteger(publicKeys.Length);
            builder.SysCall(InteropService.SystemCryptoCheckMultisig);
            
            return builder.ToArray();
        }
        
        /// <summary>
        /// Builds a contract hash calculation script.
        /// </summary>
        /// <param name="sender">The deployer's script hash</param>
        /// <param name="nefCheckSum">The NEF file checksum</param>
        /// <param name="contractName">The contract name</param>
        /// <returns>The contract hash script bytes</returns>
        public static byte[] BuildContractHashScript(Hash160 sender, uint nefCheckSum, string contractName)
        {
            if (sender == null)
                throw new ArgumentNullException(nameof(sender));
                
            if (string.IsNullOrEmpty(contractName))
                throw new ArgumentException("Contract name cannot be null or empty");
            
            using var builder = new ScriptBuilder();
            return builder.OpCode(OpCode.ABORT)
                         .PushData(sender.ToLittleEndianArray())
                         .PushInteger(nefCheckSum)
                         .PushData(contractName)
                         .ToArray();
        }
        
        /// <summary>
        /// Builds a script that calls a contract and unwraps iterator results.
        /// Useful for RPC servers with sessions disabled.
        /// </summary>
        /// <param name="contractHash">The contract script hash</param>
        /// <param name="method">The method to call</param>
        /// <param name="parameters">Method parameters</param>
        /// <param name="maxItems">Maximum iterator items to retrieve</param>
        /// <param name="callFlags">Call flags to use</param>
        /// <returns>The complete script bytes</returns>
        public static async Task<byte[]> BuildContractCallAndUnwrapIteratorAsync(
            Hash160 contractHash, string method, ContractParameter[] parameters, 
            int maxItems, CallFlags callFlags = CallFlags.All)
        {
            using var builder = new ScriptBuilder();
            
            // Push max items limit
            builder.PushInteger(maxItems);
            
            // Add contract call
            await builder.ContractCallAsync(contractHash, method, parameters, callFlags);
            
            // Create empty result array
            builder.OpCode(OpCode.NEWARRAY0);
            
            // Iterator traversal loop
            var loopStart = builder.Size;
            builder.OpCode(OpCode.OVER)
                   .SysCall(InteropService.SystemIteratorNext);
            
            var jumpIfNotOffset = builder.Size;
            builder.OpCode(OpCode.JMPIFNOT, new byte[] { 0x00 });
            
            // Get value and add to array, check size limit
            builder.OpCode(OpCode.DUP, OpCode.PUSH2, OpCode.PICK)
                   .SysCall(InteropService.SystemIteratorValue)
                   .OpCode(OpCode.APPEND, OpCode.DUP, OpCode.SIZE, OpCode.PUSH3, OpCode.PICK, OpCode.GE);
            
            var jumpIfMaxOffset = builder.Size;
            builder.OpCode(OpCode.JMPIF, new byte[] { 0x00 });
            
            // Jump back to loop start
            var jumpOffset = builder.Size;
            var jumpDistance = (byte)(loopStart - jumpOffset);
            builder.OpCode(OpCode.JMP, new byte[] { jumpDistance });
            
            // Final cleanup
            var endOffset = builder.Size;
            builder.OpCode(OpCode.NIP, OpCode.NIP);
            
            // Fix jump targets
            var script = builder.ToArrayWithoutReset();
            script[jumpIfNotOffset + 1] = (byte)(endOffset - jumpIfNotOffset);
            script[jumpIfMaxOffset + 1] = (byte)(endOffset - jumpIfMaxOffset);
            
            return script;
        }
        
        #endregion
        
        #region Script Output
        
        /// <summary>
        /// Returns the script as a byte array and resets the builder.
        /// </summary>
        /// <returns>The complete script bytes</returns>
        public byte[] ToArray()
        {
            ValidateNotDisposed();
            Extensions.SafeLog($"ScriptBuilder: Generated script of {Size} bytes");
            return _writer.ToArray();
        }
        
        /// <summary>
        /// Returns the script as a byte array without resetting the builder.
        /// </summary>
        /// <returns>The complete script bytes</returns>
        public byte[] ToArrayWithoutReset()
        {
            ValidateNotDisposed();
            return _writer.ToArrayWithoutReset();
        }
        
        /// <summary>
        /// Returns the script as a hex string for debugging.
        /// </summary>
        /// <returns>Hexadecimal representation of the script</returns>
        public string ToHexString()
        {
            ValidateNotDisposed();
            return _writer.ToHexString();
        }
        
        #endregion
        
        #region Private Helpers
        
        /// <summary>
        /// Pads a BigInteger to the specified byte length.
        /// </summary>
        /// <param name="value">The value to pad</param>
        /// <param name="length">The target byte length</param>
        /// <returns>Padded byte array</returns>
        private byte[] PadNumber(BigInteger value, int length)
        {
            var bytes = value.ToByteArray();
            if (!BitConverter.IsLittleEndian)
                Array.Reverse(bytes);
            
            if (bytes.Length == length)
                return bytes;
            
            var padded = new byte[length];
            Array.Copy(bytes, padded, Math.Min(bytes.Length, length));
            
            // Sign extend for negative numbers
            if (value < 0 && bytes.Length < length)
            {
                for (int i = bytes.Length; i < length; i++)
                {
                    padded[i] = 0xFF;
                }
            }
            
            return padded;
        }
        
        /// <summary>
        /// Validates that the builder hasn't been disposed.
        /// </summary>
        /// <exception cref="ObjectDisposedException">If the builder has been disposed</exception>
        private void ValidateNotDisposed()
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(ScriptBuilder));
        }
        
        #endregion
        
        #region IDisposable Implementation
        
        /// <summary>
        /// Disposes of the script builder resources.
        /// </summary>
        public void Dispose()
        {
            if (!_disposed)
            {
                _writer?.Dispose();
                _disposed = true;
                Extensions.SafeLog("ScriptBuilder disposed");
            }
        }
        
        #endregion
        
        #region ToString Override
        
        /// <summary>
        /// Returns a string representation of the script builder.
        /// </summary>
        /// <returns>String representation</returns>
        public override string ToString()
        {
            return $"ScriptBuilder(Size: {Size} bytes, Disposed: {IsDisposed})";
        }
        
        #endregion
    }
    
    /// <summary>
    /// Represents call flags for contract invocations.
    /// </summary>
    [Flags]
    public enum CallFlags : byte
    {
        /// <summary>No special flags</summary>
        None = 0,
        
        /// <summary>Allow reading blockchain state</summary>
        ReadStates = 0b00000001,
        
        /// <summary>Allow writing to blockchain state</summary>
        WriteStates = 0b00000010,
        
        /// <summary>Allow contract calls</summary>
        AllowCall = 0b00000100,
        
        /// <summary>Allow notifications</summary>
        AllowNotify = 0b00001000,
        
        /// <summary>Read-only operations (ReadStates + AllowCall + AllowNotify)</summary>
        ReadOnly = ReadStates | AllowCall | AllowNotify,
        
        /// <summary>All flags enabled</summary>
        All = ReadStates | WriteStates | AllowCall | AllowNotify
    }
}
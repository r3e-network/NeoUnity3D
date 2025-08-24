using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Neo.Unity.SDK.Types;
using Neo.Unity.SDK.Serialization;
using Neo.Unity.SDK.Utils;
using Neo.Unity.SDK.Core;
using UnityEngine;

namespace Neo.Unity.SDK.Contracts
{
    /*
    ┌───────────────────────────────────────────────────────────────────────┐
    │                    NEO Executable Format 3 (NEF3)                     │
    ├──────────┬───────────────┬────────────────────────────────────────────┤
    │  Field   │     Type      │                  Comment                   │
    ├──────────┼───────────────┼────────────────────────────────────────────┤
    │ Magic    │ uint32        │ Magic header                               │
    │ Compiler │ byte[64]      │ Compiler name and version                  │
    ├──────────┼───────────────┼────────────────────────────────────────────┤
    │ Source   │ byte[]        │ The url of the source files, max 255 bytes │
    │ Reserve  │ byte[2]       │ Reserved for future extensions. Must be 0. │
    │ Tokens   │ MethodToken[] │ Method tokens                              │
    │ Reserve  │ byte[2]       │ Reserved for future extensions. Must be 0. │
    │ Script   │ byte[]        │ Var bytes for the payload                  │
    ├──────────┼───────────────┼────────────────────────────────────────────┤
    │ Checksum │ uint32        │ First four bytes of double SHA256 hash     │
    └──────────┴───────────────┴────────────────────────────────────────────┘
    */
    
    /// <summary>
    /// Represents a NEO Executable Format (NEF) file structure.
    /// NEF3 is the standard format for Neo smart contracts.
    /// </summary>
    public struct NefFile : INeoSerializable
    {
        #region Constants
        
        private const uint MAGIC = 0x3346454E; // "NEF3" in little-endian
        private const int MAGIC_SIZE = 4;
        private const int COMPILER_SIZE = 64;
        private const int MAX_SOURCE_URL_SIZE = 256;
        private const int MAX_SCRIPT_LENGTH = 512 * 1024; // 512KB
        private const int CHECKSUM_SIZE = 4;
        private const int HEADER_SIZE = MAGIC_SIZE + COMPILER_SIZE;
        
        #endregion
        
        #region Properties
        
        /// <summary>
        /// The compiler name and version with which this NEF file has been generated.
        /// </summary>
        public string Compiler { get; }
        
        /// <summary>
        /// The source code URL.
        /// </summary>
        public string SourceUrl { get; }
        
        /// <summary>
        /// The contract's method tokens. The tokens represent calls to other contracts.
        /// </summary>
        public MethodToken[] MethodTokens { get; }
        
        /// <summary>
        /// The contract script bytes.
        /// </summary>
        public byte[] Script { get; }
        
        /// <summary>
        /// The checksum bytes.
        /// </summary>
        public byte[] Checksum { get; private set; }
        
        /// <summary>
        /// The NEF file's checksum as an integer.
        /// The checksum bytes of the NEF file are read as a little endian unsigned integer.
        /// </summary>
        public uint ChecksumInteger => GetChecksumAsInteger(Checksum);
        
        /// <summary>
        /// Gets the size of this NEF file when serialized.
        /// </summary>
        public int Size => HEADER_SIZE + SourceUrl.GetVarSize() + 1 +
                          MethodTokens.GetVarSize() + 2 +
                          Script.GetVarSize() + CHECKSUM_SIZE;
        
        #endregion
        
        #region Constructors
        
        /// <summary>
        /// Creates an empty NefFile instance.
        /// </summary>
        public NefFile() : this(null, "", new MethodToken[0], new byte[0])
        {
        }
        
        /// <summary>
        /// Constructs a new NefFile from the given contract information.
        /// </summary>
        /// <param name="compiler">The compiler name and version</param>
        /// <param name="sourceUrl">The URL to the source code</param>
        /// <param name="methodTokens">The method tokens</param>
        /// <param name="script">The contract's script</param>
        /// <exception cref="ArgumentException">Thrown when parameters exceed size limits</exception>
        public NefFile(string compiler, string sourceUrl, MethodToken[] methodTokens, byte[] script)
        {
            if (compiler != null && System.Text.Encoding.UTF8.GetByteCount(compiler) > COMPILER_SIZE)
                throw new ArgumentException($"The compiler name and version string can be max {COMPILER_SIZE} bytes long, but was {System.Text.Encoding.UTF8.GetByteCount(compiler)} bytes long.", nameof(compiler));
            
            if (System.Text.Encoding.UTF8.GetByteCount(sourceUrl ?? "") >= MAX_SOURCE_URL_SIZE)
                throw new ArgumentException($"The source URL must not be longer than {MAX_SOURCE_URL_SIZE} bytes.", nameof(sourceUrl));
            
            if (script != null && script.Length > MAX_SCRIPT_LENGTH)
                throw new ArgumentException($"Script length exceeds maximum allowed size of {MAX_SCRIPT_LENGTH} bytes.", nameof(script));
            
            Compiler = compiler;
            SourceUrl = sourceUrl ?? "";
            MethodTokens = methodTokens ?? new MethodToken[0];
            Script = script ?? new byte[0];
            Checksum = new byte[CHECKSUM_SIZE];
            
            // Compute checksum
            Checksum = ComputeChecksum(this);
        }
        
        #endregion
        
        #region Static Methods
        
        /// <summary>
        /// Converts checksum bytes to an integer.
        /// The checksum is expected to be 4 bytes, and it is interpreted as a little endian unsigned integer.
        /// </summary>
        /// <param name="checksumBytes">The checksum bytes</param>
        /// <returns>The checksum as an integer</returns>
        public static uint GetChecksumAsInteger(byte[] checksumBytes)
        {
            if (checksumBytes == null || checksumBytes.Length != CHECKSUM_SIZE)
                return 0;
            
            return BitConverter.ToUInt32(checksumBytes, 0);
        }
        
        /// <summary>
        /// Computes the checksum for the given NEF file.
        /// </summary>
        /// <param name="file">The NEF file</param>
        /// <returns>The checksum</returns>
        public static byte[] ComputeChecksum(NefFile file)
        {
            return ComputeChecksumFromBytes(file.ToArray());
        }
        
        /// <summary>
        /// Computes the checksum from the bytes of a NEF file.
        /// </summary>
        /// <param name="bytes">The bytes of the NEF file</param>
        /// <returns>The checksum</returns>
        public static byte[] ComputeChecksumFromBytes(byte[] bytes)
        {
            if (bytes == null || bytes.Length < CHECKSUM_SIZE)
                return new byte[CHECKSUM_SIZE];
            
            var fileBytes = bytes.Take(bytes.Length - CHECKSUM_SIZE).ToArray();
            var hash = fileBytes.Hash256();
            return hash.Take(CHECKSUM_SIZE).ToArray();
        }
        
        /// <summary>
        /// Reads and constructs a NefFile instance from the given file path.
        /// </summary>
        /// <param name="filePath">The file path to read from</param>
        /// <returns>The deserialized NefFile instance</returns>
        /// <exception cref="ArgumentException">Thrown when file is too large</exception>
        /// <exception cref="FileNotFoundException">Thrown when file doesn't exist</exception>
        public static async Task<NefFile> ReadFromFileAsync(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException($"NEF file not found: {filePath}");
            
            var fileBytes = await File.ReadAllBytesAsync(filePath);
            
            if (fileBytes.Length > 0x100000) // 1MB limit
                throw new ArgumentException($"The given NEF file is too large. File was {fileBytes.Length} bytes, but a max of 1MB is allowed.", nameof(filePath));
            
            using var reader = new BinaryReader(fileBytes);
            return reader.ReadSerializable<NefFile>();
        }
        
        /// <summary>
        /// Reads and constructs a NefFile instance from byte array.
        /// </summary>
        /// <param name="fileBytes">The NEF file bytes</param>
        /// <returns>The deserialized NefFile instance</returns>
        /// <exception cref="ArgumentException">Thrown when file is too large</exception>
        public static NefFile ReadFromBytes(byte[] fileBytes)
        {
            if (fileBytes == null)
                throw new ArgumentNullException(nameof(fileBytes));
            
            if (fileBytes.Length > 0x100000) // 1MB limit
                throw new ArgumentException($"The given NEF file is too large. File was {fileBytes.Length} bytes, but a max of 1MB is allowed.", nameof(fileBytes));
            
            using var reader = new BinaryReader(fileBytes);
            return reader.ReadSerializable<NefFile>();
        }
        
        /// <summary>
        /// Deserializes and constructs a NefFile from the given stack item.
        /// It is expected that the stack item is of type ByteString and its content is simply a serialized NEF file.
        /// </summary>
        /// <param name="stackItem">The stack item to deserialize</param>
        /// <returns>The deserialized NefFile</returns>
        /// <exception cref="InvalidOperationException">Thrown when stack item is not a byte string</exception>
        public static NefFile ReadFromStackItem(StackItem stackItem)
        {
            if (stackItem.Type != StackItemType.ByteString)
                throw new InvalidOperationException($"Expected ByteString stack item, got {stackItem.Type}");
            
            var nefBytes = stackItem.GetByteArray();
            return ReadFromBytes(nefBytes);
        }
        
        #endregion
        
        #region INeoSerializable Implementation
        
        public void Serialize(BinaryWriter writer)
        {
            writer.WriteUInt32(MAGIC);
            writer.WriteFixedString(Compiler, COMPILER_SIZE);
            writer.WriteVarString(SourceUrl);
            writer.WriteByte(0); // Reserved byte
            writer.WriteSerializableArray(MethodTokens);
            writer.WriteUInt16(0); // Reserved bytes
            writer.WriteVarBytes(Script);
            writer.Write(Checksum);
        }
        
        public static NefFile Deserialize(BinaryReader reader)
        {
            var magic = reader.ReadUInt32();
            if (magic != MAGIC)
                throw new InvalidDataException("Wrong magic number in NEF file.");
            
            var compilerBytes = reader.ReadBytes(COMPILER_SIZE);
            var compiler = System.Text.Encoding.UTF8.GetString(compilerBytes.TrimTrailingZeros());
            
            var sourceUrl = reader.ReadVarString();
            if (System.Text.Encoding.UTF8.GetByteCount(sourceUrl) >= MAX_SOURCE_URL_SIZE)
                throw new InvalidDataException($"Source URL must not be longer than {MAX_SOURCE_URL_SIZE} bytes.");
            
            if (reader.ReadByte() != 0)
                throw new InvalidDataException("Reserve bytes in NEF file must be 0.");
            
            var methodTokens = reader.ReadSerializableArray<MethodToken>();
            
            if (reader.ReadUInt16() != 0)
                throw new InvalidDataException("Reserve bytes in NEF file must be 0.");
            
            var script = reader.ReadVarBytes(MAX_SCRIPT_LENGTH);
            if (script.Length == 0)
                throw new InvalidDataException("Script cannot be empty in NEF file.");
            
            var file = new NefFile(compiler, sourceUrl, methodTokens, script);
            var checksum = reader.ReadBytes(CHECKSUM_SIZE);
            
            if (!file.Checksum.SequenceEqual(checksum))
                throw new InvalidDataException("The checksums did not match.");
            
            return file;
        }
        
        #endregion
        
        #region Helper Methods
        
        /// <summary>
        /// Converts this NefFile to a byte array.
        /// </summary>
        /// <returns>The serialized NefFile as byte array</returns>
        public byte[] ToArray()
        {
            using var writer = new BinaryWriter();
            Serialize(writer);
            return writer.ToArray();
        }
        
        /// <summary>
        /// Validates the integrity of this NEF file.
        /// </summary>
        /// <returns>True if the file is valid, false otherwise</returns>
        public bool IsValid()
        {
            try
            {
                var computedChecksum = ComputeChecksum(this);
                return Checksum.SequenceEqual(computedChecksum);
            }
            catch
            {
                return false;
            }
        }
        
        #endregion
        
        #region Nested Types
        
        /// <summary>
        /// Represents a static call to another contract from within a smart contract.
        /// Method tokens are referenced in the smart contract's script whenever the referenced method is called.
        /// </summary>
        public struct MethodToken : INeoSerializable, IEquatable<MethodToken>
        {
            #region Constants
            
            private const int PARAMS_COUNT_SIZE = 2;
            private const int HAS_RETURN_VALUE_SIZE = 1;
            private const int CALL_FLAGS_SIZE = 1;
            
            #endregion
            
            #region Properties
            
            /// <summary>
            /// The script hash of the contract to call.
            /// </summary>
            public Hash160 Hash { get; }
            
            /// <summary>
            /// The method name to call.
            /// </summary>
            public string Method { get; }
            
            /// <summary>
            /// The number of parameters the method expects.
            /// </summary>
            public int ParametersCount { get; }
            
            /// <summary>
            /// Whether the method has a return value.
            /// </summary>
            public bool HasReturnValue { get; }
            
            /// <summary>
            /// The call flags for the method invocation.
            /// </summary>
            public CallFlags CallFlags { get; }
            
            /// <summary>
            /// Gets the size of this method token when serialized.
            /// </summary>
            public int Size => Hash160.LENGTH + Method.GetVarSize() + 
                              PARAMS_COUNT_SIZE + HAS_RETURN_VALUE_SIZE + CALL_FLAGS_SIZE;
            
            #endregion
            
            #region Constructor
            
            /// <summary>
            /// Constructs a new MethodToken.
            /// </summary>
            /// <param name="hash">The contract script hash</param>
            /// <param name="method">The method name</param>
            /// <param name="parametersCount">The number of parameters</param>
            /// <param name="hasReturnValue">Whether the method has a return value</param>
            /// <param name="callFlags">The call flags</param>
            public MethodToken(Hash160 hash, string method, int parametersCount, bool hasReturnValue, CallFlags callFlags)
            {
                Hash = hash ?? throw new ArgumentNullException(nameof(hash));
                Method = method ?? throw new ArgumentNullException(nameof(method));
                ParametersCount = parametersCount;
                HasReturnValue = hasReturnValue;
                CallFlags = callFlags;
            }
            
            #endregion
            
            #region INeoSerializable Implementation
            
            public void Serialize(BinaryWriter writer)
            {
                writer.WriteSerializable(Hash);
                writer.WriteVarString(Method);
                writer.WriteUInt16((ushort)ParametersCount);
                writer.WriteBoolean(HasReturnValue);
                writer.WriteByte((byte)CallFlags);
            }
            
            public static MethodToken Deserialize(BinaryReader reader)
            {
                var hash = reader.ReadSerializable<Hash160>();
                var method = reader.ReadVarString();
                var parametersCount = reader.ReadUInt16();
                var hasReturnValue = reader.ReadBoolean();
                var callFlags = (CallFlags)reader.ReadByte();
                
                return new MethodToken(hash, method, parametersCount, hasReturnValue, callFlags);
            }
            
            #endregion
            
            #region Equality
            
            public bool Equals(MethodToken other)
            {
                return Hash.Equals(other.Hash) &&
                       Method == other.Method &&
                       ParametersCount == other.ParametersCount &&
                       HasReturnValue == other.HasReturnValue &&
                       CallFlags == other.CallFlags;
            }
            
            public override bool Equals(object obj)
            {
                return obj is MethodToken other && Equals(other);
            }
            
            public override int GetHashCode()
            {
                return HashCode.Combine(Hash, Method, ParametersCount, HasReturnValue, CallFlags);
            }
            
            public static bool operator ==(MethodToken left, MethodToken right)
            {
                return left.Equals(right);
            }
            
            public static bool operator !=(MethodToken left, MethodToken right)
            {
                return !(left == right);
            }
            
            #endregion
            
            #region Overrides
            
            public override string ToString()
            {
                return $"MethodToken(Hash={Hash}, Method={Method}, Params={ParametersCount}, HasReturn={HasReturnValue}, Flags={CallFlags})";
            }
            
            #endregion
        }
        
        #endregion
        
        #region Overrides
        
        public override string ToString()
        {
            return $"NefFile(Compiler={Compiler}, SourceUrl={SourceUrl}, Tokens={MethodTokens?.Length ?? 0}, ScriptSize={Script?.Length ?? 0})";
        }
        
        public override bool Equals(object obj)
        {
            return obj is NefFile other &&
                   Compiler == other.Compiler &&
                   SourceUrl == other.SourceUrl &&
                   MethodTokens.SequenceEqual(other.MethodTokens) &&
                   Script.SequenceEqual(other.Script);
        }
        
        public override int GetHashCode()
        {
            return HashCode.Combine(Compiler, SourceUrl, MethodTokens, Script);
        }
        
        #endregion
    }
}
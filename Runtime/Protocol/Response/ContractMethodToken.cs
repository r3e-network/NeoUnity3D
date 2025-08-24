using System;
using System.Text.Json.Serialization;
using NeoUnity.Runtime.Serialization;
using NeoUnity.Runtime.Types;

namespace NeoUnity.Runtime.Protocol.Response
{
    /// <summary>
    /// Represents a contract method token containing method invocation information.
    /// Used for method calls and contract execution context.
    /// </summary>
    [Serializable]
    public class ContractMethodToken : IResponse<ContractMethodToken>, INeoSerializable, IEquatable<ContractMethodToken>
    {
        /// <summary>
        /// The contract hash.
        /// </summary>
        [JsonPropertyName("hash")]
        public Hash160 Hash { get; set; }

        /// <summary>
        /// The method name.
        /// </summary>
        [JsonPropertyName("method")]
        public string Method { get; set; }

        /// <summary>
        /// The parameter count for the method.
        /// </summary>
        [JsonPropertyName("paramcount")]
        public int ParamCount { get; set; }

        /// <summary>
        /// Indicates if the method has a return value.
        /// </summary>
        [JsonPropertyName("hasreturnvalue")]
        public bool ReturnValue { get; set; }

        /// <summary>
        /// The call flags for the method invocation.
        /// </summary>
        [JsonPropertyName("callflags")]
        public string CallFlags { get; set; }

        /// <summary>
        /// Default constructor.
        /// </summary>
        public ContractMethodToken()
        {
            Hash = Hash160.Zero;
            Method = string.Empty;
            ParamCount = 0;
            ReturnValue = false;
            CallFlags = string.Empty;
        }

        /// <summary>
        /// Initializes a new instance of the ContractMethodToken class.
        /// </summary>
        /// <param name="hash">The contract hash</param>
        /// <param name="method">The method name</param>
        /// <param name="paramCount">The parameter count</param>
        /// <param name="returnValue">Whether the method has a return value</param>
        /// <param name="callFlags">The call flags</param>
        public ContractMethodToken(Hash160 hash, string method, int paramCount, bool returnValue, string callFlags)
        {
            Hash = hash ?? throw new ArgumentNullException(nameof(hash));
            Method = method ?? throw new ArgumentNullException(nameof(method));
            ParamCount = paramCount;
            ReturnValue = returnValue;
            CallFlags = callFlags ?? throw new ArgumentNullException(nameof(callFlags));
        }

        /// <summary>
        /// Deserializes the contract method token from a binary reader.
        /// </summary>
        /// <param name="reader">The binary reader</param>
        public void FromBinaryReader(BinaryReader reader)
        {
            if (reader == null)
                throw new ArgumentNullException(nameof(reader));

            Hash = Hash160.FromBinaryReader(reader);
            Method = reader.ReadString();
            ParamCount = reader.ReadInt32();
            ReturnValue = reader.ReadBoolean();
            CallFlags = reader.ReadString();
        }

        /// <summary>
        /// Serializes the contract method token to a binary writer.
        /// </summary>
        /// <param name="writer">The binary writer</param>
        public void ToBinaryWriter(BinaryWriter writer)
        {
            if (writer == null)
                throw new ArgumentNullException(nameof(writer));

            Hash.ToBinaryWriter(writer);
            writer.WriteString(Method ?? string.Empty);
            writer.WriteInt32(ParamCount);
            writer.WriteBoolean(ReturnValue);
            writer.WriteString(CallFlags ?? string.Empty);
        }

        /// <summary>
        /// Gets the size of the serialized data.
        /// </summary>
        /// <returns>The size in bytes</returns>
        public int GetSerializedSize()
        {
            return Hash160.Size +
                   sizeof(int) + (Method?.Length ?? 0) * sizeof(char) +
                   sizeof(int) +
                   sizeof(bool) +
                   sizeof(int) + (CallFlags?.Length ?? 0) * sizeof(char);
        }

        /// <summary>
        /// Validates the contract method token data.
        /// </summary>
        /// <returns>True if valid, false otherwise</returns>
        public bool IsValid()
        {
            return Hash != null && 
                   !string.IsNullOrEmpty(Method) && 
                   ParamCount >= 0 && 
                   !string.IsNullOrEmpty(CallFlags);
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="obj">The object to compare</param>
        /// <returns>True if equal, false otherwise</returns>
        public override bool Equals(object obj)
        {
            return obj is ContractMethodToken other && Equals(other);
        }

        /// <summary>
        /// Determines whether the specified ContractMethodToken is equal to the current object.
        /// </summary>
        /// <param name="other">The ContractMethodToken to compare</param>
        /// <returns>True if equal, false otherwise</returns>
        public bool Equals(ContractMethodToken other)
        {
            if (other == null) return false;
            return Hash.Equals(other.Hash) &&
                   Method == other.Method &&
                   ParamCount == other.ParamCount &&
                   ReturnValue == other.ReturnValue &&
                   CallFlags == other.CallFlags;
        }

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>The hash code</returns>
        public override int GetHashCode()
        {
            return HashCode.Combine(Hash, Method, ParamCount, ReturnValue, CallFlags);
        }

        /// <summary>
        /// Returns a string representation of the contract method token.
        /// </summary>
        /// <returns>A string representation</returns>
        public override string ToString()
        {
            return $"ContractMethodToken: {Method} on {Hash} (params: {ParamCount}, hasReturn: {ReturnValue}, flags: {CallFlags})";
        }

        /// <summary>
        /// Equality operator.
        /// </summary>
        public static bool operator ==(ContractMethodToken left, ContractMethodToken right)
        {
            return Equals(left, right);
        }

        /// <summary>
        /// Inequality operator.
        /// </summary>
        public static bool operator !=(ContractMethodToken left, ContractMethodToken right)
        {
            return !Equals(left, right);
        }
    }
}
using System;
using UnityEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Neo.Unity.SDK.Protocol.Response
{
    /// <summary>
    /// Represents a transaction attribute that provides additional metadata or constraints.
    /// Neo supports different types of attributes like HighPriority and OracleResponse.
    /// </summary>
    [System.Serializable]
    [JsonConverter(typeof(TransactionAttributeConverter))]
    public abstract class TransactionAttribute
    {
        /// <summary>The type of this transaction attribute</summary>
        [JsonProperty("type")]
        public abstract TransactionAttributeType Type { get; }
        
        /// <summary>
        /// Gets the string representation of the attribute type.
        /// </summary>
        /// <returns>String representation of the type</returns>
        public virtual string GetTypeString()
        {
            return Type.ToString();
        }
        
        /// <summary>
        /// Validates this transaction attribute.
        /// </summary>
        /// <exception cref="InvalidOperationException">If validation fails</exception>
        public virtual void Validate()
        {
            // Base validation - override in derived classes for specific validation
        }
        
        /// <summary>
        /// Returns a string representation of this attribute.
        /// </summary>
        /// <returns>String representation</returns>
        public override string ToString()
        {
            return $"TransactionAttribute({Type})";
        }
    }
    
    /// <summary>
    /// High priority transaction attribute.
    /// Indicates that this transaction should be prioritized by validators.
    /// </summary>
    [System.Serializable]
    public class HighPriorityAttribute : TransactionAttribute
    {
        /// <summary>The type of this attribute</summary>
        public override TransactionAttributeType Type => TransactionAttributeType.HighPriority;
        
        /// <summary>
        /// Default constructor.
        /// </summary>
        public HighPriorityAttribute()
        {
        }
        
        /// <summary>
        /// Returns a string representation of this high priority attribute.
        /// </summary>
        /// <returns>String representation</returns>
        public override string ToString()
        {
            return "HighPriorityAttribute";
        }
    }
    
    /// <summary>
    /// Oracle response transaction attribute.
    /// Contains response data from an oracle request.
    /// </summary>
    [System.Serializable]
    public class OracleResponseAttribute : TransactionAttribute
    {
        /// <summary>The type of this attribute</summary>
        public override TransactionAttributeType Type => TransactionAttributeType.OracleResponse;
        
        /// <summary>The ID of the oracle request this response corresponds to</summary>
        [JsonProperty("id")]
        public long Id { get; set; }
        
        /// <summary>The oracle response code indicating success or failure</summary>
        [JsonProperty("code")]
        public OracleResponseCode Code { get; set; }
        
        /// <summary>The response data from the oracle (base64 encoded)</summary>
        [JsonProperty("result")]
        public string Result { get; set; }
        
        /// <summary>
        /// Default constructor for JSON deserialization.
        /// </summary>
        public OracleResponseAttribute()
        {
        }
        
        /// <summary>
        /// Creates a new oracle response attribute.
        /// </summary>
        /// <param name="id">The oracle request ID</param>
        /// <param name="code">The response code</param>
        /// <param name="result">The response data</param>
        public OracleResponseAttribute(long id, OracleResponseCode code, string result)
        {
            Id = id;
            Code = code;
            Result = result;
        }
        
        /// <summary>Whether this oracle response was successful</summary>
        [JsonIgnore]
        public bool IsSuccess => Code == OracleResponseCode.Success;
        
        /// <summary>Whether this oracle response indicates an error</summary>
        [JsonIgnore]
        public bool IsError => Code != OracleResponseCode.Success;
        
        /// <summary>Whether there is result data</summary>
        [JsonIgnore]
        public bool HasResult => !string.IsNullOrEmpty(Result);
        
        /// <summary>
        /// Gets the result data as decoded bytes.
        /// </summary>
        /// <returns>The result data bytes</returns>
        /// <exception cref="FormatException">If the result is not valid base64</exception>
        public byte[] GetResultBytes()
        {
            if (string.IsNullOrEmpty(Result))
                return new byte[0];
            
            try
            {
                return Convert.FromBase64String(Result);
            }
            catch (FormatException ex)
            {
                throw new FormatException($"Invalid base64 oracle response result: {Result}", ex);
            }
        }
        
        /// <summary>
        /// Tries to get the result data as decoded bytes.
        /// </summary>
        /// <param name="resultBytes">The result data bytes if successful</param>
        /// <returns>True if successful, false if invalid base64</returns>
        public bool TryGetResultBytes(out byte[] resultBytes)
        {
            try
            {
                resultBytes = GetResultBytes();
                return true;
            }
            catch
            {
                resultBytes = null;
                return false;
            }
        }
        
        /// <summary>
        /// Gets the result as a UTF-8 string.
        /// </summary>
        /// <returns>The result as string</returns>
        public string GetResultAsString()
        {
            var bytes = GetResultBytes();
            if (bytes.Length == 0)
                return string.Empty;
            
            try
            {
                return System.Text.Encoding.UTF8.GetString(bytes);
            }
            catch (ArgumentException)
            {
                // Return hex representation if not valid UTF-8
                return Convert.ToHexString(bytes);
            }
        }
        
        /// <summary>
        /// Validates this oracle response attribute.
        /// </summary>
        /// <exception cref="InvalidOperationException">If validation fails</exception>
        public override void Validate()
        {
            base.Validate();
            
            if (Id < 0)
                throw new InvalidOperationException("Oracle response ID must be non-negative.");
            
            if (HasResult)
            {
                try
                {
                    Convert.FromBase64String(Result);
                }
                catch (FormatException ex)
                {
                    throw new InvalidOperationException("Oracle response result is not valid base64.", ex);
                }
            }
        }
        
        /// <summary>
        /// Returns a string representation of this oracle response attribute.
        /// </summary>
        /// <returns>String representation</returns>
        public override string ToString()
        {
            var resultLength = HasResult ? Result.Length : 0;
            return $"OracleResponseAttribute(ID: {Id}, Code: {Code}, Result: {resultLength} chars)";
        }
    }
    
    /// <summary>
    /// Enumeration of transaction attribute types.
    /// </summary>
    public enum TransactionAttributeType : byte
    {
        /// <summary>High priority transaction attribute</summary>
        HighPriority = 0x01,
        
        /// <summary>Oracle response transaction attribute</summary>
        OracleResponse = 0x11
    }
    
    /// <summary>
    /// Enumeration of oracle response codes.
    /// </summary>
    public enum OracleResponseCode : byte
    {
        /// <summary>The oracle request was successful</summary>
        Success = 0x00,
        
        /// <summary>The oracle failed to process the request</summary>
        ProtocolNotSupported = 0x10,
        
        /// <summary>The oracle encountered a console error</summary>
        ConsoleError = 0x12,
        
        /// <summary>The oracle request timed out</summary>
        Timeout = 0x14,
        
        /// <summary>The oracle is forbidden from accessing the resource</summary>
        Forbidden = 0x16,
        
        /// <summary>The oracle content type is not supported</summary>
        ContentTypeNotSupported = 0x17,
        
        /// <summary>Generic oracle error</summary>
        Error = 0xff
    }
    
    /// <summary>
    /// Custom JSON converter for TransactionAttribute objects.
    /// Handles polymorphic deserialization based on the type property.
    /// </summary>
    public class TransactionAttributeConverter : JsonConverter<TransactionAttribute>
    {
        /// <summary>
        /// Reads and deserializes a TransactionAttribute from JSON.
        /// </summary>
        /// <param name="reader">The JSON reader</param>
        /// <param name="objectType">The object type</param>
        /// <param name="existingValue">The existing value</param>
        /// <param name="hasExistingValue">Whether there is an existing value</param>
        /// <param name="serializer">The JSON serializer</param>
        /// <returns>The deserialized TransactionAttribute</returns>
        public override TransactionAttribute ReadJson(JsonReader reader, Type objectType, TransactionAttribute existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            var jsonObject = Newtonsoft.Json.Linq.JObject.Load(reader);
            var typeString = jsonObject["type"]?.Value<string>();
            
            return typeString switch
            {
                "HighPriority" => jsonObject.ToObject<HighPriorityAttribute>(serializer),
                "OracleResponse" => jsonObject.ToObject<OracleResponseAttribute>(serializer),
                _ => throw new JsonException($"Unknown transaction attribute type: {typeString}")
            };
        }
        
        /// <summary>
        /// Writes a TransactionAttribute to JSON.
        /// </summary>
        /// <param name="writer">The JSON writer</param>
        /// <param name="value">The TransactionAttribute to serialize</param>
        /// <param name="serializer">The JSON serializer</param>
        public override void WriteJson(JsonWriter writer, TransactionAttribute value, JsonSerializer serializer)
        {
            serializer.Serialize(writer, value);
        }
    }
}
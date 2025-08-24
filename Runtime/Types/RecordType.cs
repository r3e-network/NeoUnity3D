using System;
using Newtonsoft.Json;

namespace Neo.Unity.SDK.Types
{
    /// <summary>
    /// Represents DNS record types supported by Neo Name Service (NNS).
    /// Used for domain name resolution and record management in Neo blockchain.
    /// </summary>
    [JsonConverter(typeof(RecordTypeConverter))]
    public enum RecordType : byte
    {
        /// <summary>IPv4 address record (A record)</summary>
        A = 1,
        
        /// <summary>Canonical name record (CNAME)</summary>
        CNAME = 5,
        
        /// <summary>Text record (TXT)</summary>
        TXT = 16,
        
        /// <summary>IPv6 address record (AAAA)</summary>
        AAAA = 28
    }
    
    /// <summary>
    /// Extension methods for RecordType operations.
    /// </summary>
    public static class RecordTypeExtensions
    {
        /// <summary>
        /// Gets the string representation of the record type.
        /// </summary>
        /// <param name="recordType">The record type</param>
        /// <returns>String representation</returns>
        public static string GetName(this RecordType recordType)
        {
            return recordType.ToString();
        }
        
        /// <summary>
        /// Gets the byte value of the record type.
        /// </summary>
        /// <param name="recordType">The record type</param>
        /// <returns>Byte value</returns>
        public static byte GetValue(this RecordType recordType)
        {
            return (byte)recordType;
        }
        
        /// <summary>
        /// Checks if the record type represents an IP address.
        /// </summary>
        /// <param name="recordType">The record type</param>
        /// <returns>True if IP address type</returns>
        public static bool IsIPAddress(this RecordType recordType)
        {
            return recordType == RecordType.A || recordType == RecordType.AAAA;
        }
        
        /// <summary>
        /// Parses a record type from its string name.
        /// </summary>
        /// <param name="name">The record type name</param>
        /// <returns>Parsed record type</returns>
        /// <exception cref="ArgumentException">If name is invalid</exception>
        public static RecordType FromName(string name)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentException("Record type name cannot be null or empty", nameof(name));
            
            if (Enum.TryParse<RecordType>(name, true, out var result))
                return result;
            
            throw new ArgumentException($"Unknown record type: {name}", nameof(name));
        }
        
        /// <summary>
        /// Parses a record type from its byte value.
        /// </summary>
        /// <param name="value">The byte value</param>
        /// <returns>Parsed record type</returns>
        /// <exception cref="ArgumentException">If value is invalid</exception>
        public static RecordType FromValue(byte value)
        {
            if (Enum.IsDefined(typeof(RecordType), value))
                return (RecordType)value;
            
            throw new ArgumentException($"Unknown record type value: {value}", nameof(value));
        }
    }
    
    /// <summary>
    /// JSON converter for RecordType serialization.
    /// </summary>
    public class RecordTypeConverter : JsonConverter<RecordType>
    {
        /// <summary>
        /// Reads RecordType from JSON.
        /// </summary>
        public override RecordType ReadJson(JsonReader reader, Type objectType, RecordType existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            switch (reader.TokenType)
            {
                case JsonToken.String:
                    var stringValue = (string)reader.Value;
                    return RecordTypeExtensions.FromName(stringValue);
                    
                case JsonToken.Integer:
                    var intValue = Convert.ToByte(reader.Value);
                    return RecordTypeExtensions.FromValue(intValue);
                    
                default:
                    throw new JsonException($"Unexpected token type {reader.TokenType} when parsing RecordType");
            }
        }
        
        /// <summary>
        /// Writes RecordType to JSON.
        /// </summary>
        public override void WriteJson(JsonWriter writer, RecordType value, JsonSerializer serializer)
        {
            writer.WriteValue(value.GetName());
        }
    }
}
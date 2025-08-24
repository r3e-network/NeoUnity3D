using System;
using Newtonsoft.Json;

namespace Neo.Unity.SDK.Types
{
    /// <summary>
    /// Represents the different types of contract parameters supported by the Neo VM.
    /// Each type corresponds to a specific data format for smart contract invocations.
    /// </summary>
    [JsonConverter(typeof(ContractParameterTypeConverter))]
    public enum ContractParameterType : byte
    {
        /// <summary>Any type - represents a wildcard parameter</summary>
        Any = 0x00,
        
        /// <summary>Boolean type - true or false</summary>
        Boolean = 0x10,
        
        /// <summary>Integer type - variable-length signed integer</summary>
        Integer = 0x11,
        
        /// <summary>Byte array type - arbitrary length byte sequence</summary>
        ByteArray = 0x12,
        
        /// <summary>String type - UTF-8 encoded string</summary>
        String = 0x13,
        
        /// <summary>Hash160 type - 160-bit hash (20 bytes)</summary>
        Hash160 = 0x14,
        
        /// <summary>Hash256 type - 256-bit hash (32 bytes)</summary>
        Hash256 = 0x15,
        
        /// <summary>Public key type - compressed ECDSA public key (33 bytes)</summary>
        PublicKey = 0x16,
        
        /// <summary>Signature type - ECDSA signature (64 bytes)</summary>
        Signature = 0x17,
        
        /// <summary>Array type - ordered collection of parameters</summary>
        Array = 0x20,
        
        /// <summary>Map type - key-value pairs collection</summary>
        Map = 0x22,
        
        /// <summary>Interop interface type - reference to external interface</summary>
        InteropInterface = 0x30,
        
        /// <summary>Void type - no return value</summary>
        Void = 0xFF
    }
    
    /// <summary>
    /// Extension methods for ContractParameterType.
    /// </summary>
    public static class ContractParameterTypeExtensions
    {
        /// <summary>
        /// Gets the JSON value representation of the contract parameter type.
        /// </summary>
        /// <param name="type">The contract parameter type</param>
        /// <returns>The JSON string representation</returns>
        public static string GetJsonValue(this ContractParameterType type)
        {
            return type switch
            {
                ContractParameterType.Any => "Any",
                ContractParameterType.Boolean => "Boolean", 
                ContractParameterType.Integer => "Integer",
                ContractParameterType.ByteArray => "ByteArray",
                ContractParameterType.String => "String",
                ContractParameterType.Hash160 => "Hash160",
                ContractParameterType.Hash256 => "Hash256", 
                ContractParameterType.PublicKey => "PublicKey",
                ContractParameterType.Signature => "Signature",
                ContractParameterType.Array => "Array",
                ContractParameterType.Map => "Map",
                ContractParameterType.InteropInterface => "InteropInterface",
                ContractParameterType.Void => "Void",
                _ => throw new ArgumentException($"Unknown contract parameter type: {type}")
            };
        }
        
        /// <summary>
        /// Parses a contract parameter type from its JSON string representation.
        /// </summary>
        /// <param name="jsonValue">The JSON string value</param>
        /// <returns>The corresponding contract parameter type</returns>
        /// <exception cref="ArgumentException">If the JSON value is not recognized</exception>
        public static ContractParameterType FromJsonValue(string jsonValue)
        {
            return jsonValue switch
            {
                "Any" => ContractParameterType.Any,
                "Boolean" => ContractParameterType.Boolean,
                "Integer" => ContractParameterType.Integer,
                "ByteArray" => ContractParameterType.ByteArray,
                "String" => ContractParameterType.String,
                "Hash160" => ContractParameterType.Hash160,
                "Hash256" => ContractParameterType.Hash256,
                "PublicKey" => ContractParameterType.PublicKey,
                "Signature" => ContractParameterType.Signature,
                "Array" => ContractParameterType.Array,
                "Map" => ContractParameterType.Map,
                "InteropInterface" => ContractParameterType.InteropInterface,
                "Void" => ContractParameterType.Void,
                _ => throw new ArgumentException($"Unknown contract parameter type JSON value: {jsonValue}")
            };
        }
        
        /// <summary>
        /// Checks if the type represents a collection type.
        /// </summary>
        /// <param name="type">The contract parameter type</param>
        /// <returns>True if the type is Array or Map</returns>
        public static bool IsCollectionType(this ContractParameterType type)
        {
            return type == ContractParameterType.Array || type == ContractParameterType.Map;
        }
        
        /// <summary>
        /// Checks if the type represents a hash type.
        /// </summary>
        /// <param name="type">The contract parameter type</param>
        /// <returns>True if the type is Hash160 or Hash256</returns>
        public static bool IsHashType(this ContractParameterType type)
        {
            return type == ContractParameterType.Hash160 || type == ContractParameterType.Hash256;
        }
        
        /// <summary>
        /// Checks if the type represents a numeric type.
        /// </summary>
        /// <param name="type">The contract parameter type</param>
        /// <returns>True if the type is Integer</returns>
        public static bool IsNumericType(this ContractParameterType type)
        {
            return type == ContractParameterType.Integer;
        }
        
        /// <summary>
        /// Checks if the type represents a binary data type.
        /// </summary>
        /// <param name="type">The contract parameter type</param>
        /// <returns>True if the type is ByteArray, Signature, or PublicKey</returns>
        public static bool IsBinaryType(this ContractParameterType type)
        {
            return type == ContractParameterType.ByteArray || 
                   type == ContractParameterType.Signature || 
                   type == ContractParameterType.PublicKey;
        }
    }
    
    /// <summary>
    /// Custom JSON converter for ContractParameterType.
    /// </summary>
    public class ContractParameterTypeConverter : JsonConverter<ContractParameterType>
    {
        /// <summary>
        /// Reads the JSON representation of a ContractParameterType.
        /// </summary>
        public override ContractParameterType ReadJson(JsonReader reader, Type objectType, ContractParameterType existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.String)
            {
                var value = (string)reader.Value;
                return ContractParameterTypeExtensions.FromJsonValue(value);
            }
            
            throw new JsonException($"Unexpected token type {reader.TokenType} when parsing ContractParameterType.");
        }
        
        /// <summary>
        /// Writes the JSON representation of a ContractParameterType.
        /// </summary>
        public override void WriteJson(JsonWriter writer, ContractParameterType value, JsonSerializer serializer)
        {
            writer.WriteValue(value.GetJsonValue());
        }
    }
}
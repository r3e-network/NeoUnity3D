using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using UnityEngine;
using Neo.Unity.SDK.Crypto;
using Neo.Unity.SDK.Wallet;
using Newtonsoft.Json;

namespace Neo.Unity.SDK.Types
{
    /// <summary>
    /// Contract parameters represent input parameters for contract invocations.
    /// Supports all Neo VM parameter types with proper JSON serialization.
    /// </summary>
    [System.Serializable]
    public class ContractParameter : IEquatable<ContractParameter>
    {
        #region Properties
        
        /// <summary>Optional parameter name</summary>
        [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
        public string Name { get; set; }
        
        /// <summary>Parameter type</summary>
        [JsonProperty("type")]
        public ContractParameterType Type { get; set; }
        
        /// <summary>Parameter value</summary>
        [JsonProperty("value", NullValueHandling = NullValueHandling.Ignore)]
        public object Value { get; set; }
        
        #endregion
        
        #region Constructors
        
        /// <summary>
        /// Creates a contract parameter with name, type, and value.
        /// </summary>
        /// <param name="name">Parameter name</param>
        /// <param name="type">Parameter type</param>
        /// <param name="value">Parameter value</param>
        public ContractParameter(string name, ContractParameterType type, object value = null)
        {
            Name = name;
            Type = type;
            Value = value;
        }
        
        /// <summary>
        /// Creates a contract parameter with type and value.
        /// </summary>
        /// <param name="type">Parameter type</param>
        /// <param name="value">Parameter value</param>
        public ContractParameter(ContractParameterType type, object value = null)
        {
            Type = type;
            Value = value;
        }
        
        /// <summary>
        /// Default constructor for JSON deserialization.
        /// </summary>
        public ContractParameter()
        {
        }
        
        #endregion
        
        #region Static Factory Methods
        
        /// <summary>Creates an Any parameter from the given value.</summary>
        /// <param name="value">Any object value</param>
        /// <returns>The contract parameter</returns>
        public static ContractParameter Any(object value = null)
        {
            return new ContractParameter(ContractParameterType.Any, value);
        }
        
        /// <summary>Creates a String parameter from the given value.</summary>
        /// <param name="value">The string value</param>
        /// <returns>The contract parameter</returns>
        public static ContractParameter String(string value)
        {
            return new ContractParameter(ContractParameterType.String, value);
        }
        
        /// <summary>Creates a ByteArray parameter from the given byte array.</summary>
        /// <param name="value">The byte array</param>
        /// <returns>The contract parameter</returns>
        public static ContractParameter ByteArray(byte[] value)
        {
            return new ContractParameter(ContractParameterType.ByteArray, value);
        }
        
        /// <summary>Creates a ByteArray parameter from the given hex string.</summary>
        /// <param name="hexValue">The hexadecimal string</param>
        /// <returns>The contract parameter</returns>
        public static ContractParameter ByteArray(string hexValue)
        {
            if (string.IsNullOrEmpty(hexValue))
                throw new ArgumentException("Hex value cannot be null or empty.", nameof(hexValue));
            
            if (!hexValue.IsValidHex())
                throw new ArgumentException("Argument is not a valid hex string.", nameof(hexValue));
            
            return new ContractParameter(ContractParameterType.ByteArray, Convert.FromHexString(hexValue));
        }
        
        /// <summary>Creates a ByteArray parameter from a string using UTF-8 encoding.</summary>
        /// <param name="value">The string value</param>
        /// <returns>The contract parameter</returns>
        public static ContractParameter ByteArrayFromString(string value)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));
            
            return new ContractParameter(ContractParameterType.ByteArray, System.Text.Encoding.UTF8.GetBytes(value));
        }
        
        /// <summary>Creates a Signature parameter from the given signature bytes.</summary>
        /// <param name="value">The signature bytes</param>
        /// <returns>The contract parameter</returns>
        public static ContractParameter Signature(byte[] value)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));
            
            if (value.Length != NeoConstants.SIGNATURE_SIZE)
                throw new ArgumentException($"Signature is expected to have a length of {NeoConstants.SIGNATURE_SIZE} bytes, but had {value.Length}.", nameof(value));
            
            return new ContractParameter(ContractParameterType.Signature, value);
        }
        
        /// <summary>Creates a Signature parameter from the given ECDSA signature.</summary>
        /// <param name="signature">The ECDSA signature</param>
        /// <returns>The contract parameter</returns>
        public static ContractParameter Signature(ECDSASignature signature)
        {
            if (signature == null)
                throw new ArgumentNullException(nameof(signature));
            
            return Signature(signature.ToByteArray());
        }
        
        /// <summary>Creates a Signature parameter from the given hex string.</summary>
        /// <param name="hexValue">The signature as hexadecimal string</param>
        /// <returns>The contract parameter</returns>
        public static ContractParameter Signature(string hexValue)
        {
            if (string.IsNullOrEmpty(hexValue))
                throw new ArgumentException("Hex value cannot be null or empty.", nameof(hexValue));
            
            if (!hexValue.IsValidHex())
                throw new ArgumentException("Argument is not a valid hex string.", nameof(hexValue));
            
            return Signature(Convert.FromHexString(hexValue));
        }
        
        /// <summary>Creates a Boolean parameter from the given boolean.</summary>
        /// <param name="value">The boolean value</param>
        /// <returns>The contract parameter</returns>
        public static ContractParameter Boolean(bool value)
        {
            return new ContractParameter(ContractParameterType.Boolean, value);
        }
        
        /// <summary>Creates an Integer parameter from the given integer.</summary>
        /// <param name="value">The integer value</param>
        /// <returns>The contract parameter</returns>
        public static ContractParameter Integer(int value)
        {
            return new ContractParameter(ContractParameterType.Integer, value);
        }
        
        /// <summary>Creates an Integer parameter from the given long.</summary>
        /// <param name="value">The long value</param>
        /// <returns>The contract parameter</returns>
        public static ContractParameter Integer(long value)
        {
            return new ContractParameter(ContractParameterType.Integer, value);
        }
        
        /// <summary>Creates an Integer parameter from the given byte.</summary>
        /// <param name="value">The byte value</param>
        /// <returns>The contract parameter</returns>
        public static ContractParameter Integer(byte value)
        {
            return Integer((int)value);
        }
        
        /// <summary>Creates an Integer parameter from the given BigInteger.</summary>
        /// <param name="value">The BigInteger value</param>
        /// <returns>The contract parameter</returns>
        public static ContractParameter Integer(BigInteger value)
        {
            return new ContractParameter(ContractParameterType.Integer, value);
        }
        
        /// <summary>Creates a Hash160 parameter from the given account.</summary>
        /// <param name="account">The account</param>
        /// <returns>The contract parameter</returns>
        public static ContractParameter Hash160(Account account)
        {
            if (account == null)
                throw new ArgumentNullException(nameof(account));
            
            return new ContractParameter(ContractParameterType.Hash160, account.GetScriptHash());
        }
        
        /// <summary>Creates a Hash160 parameter from the given script hash.</summary>
        /// <param name="value">The script hash</param>
        /// <returns>The contract parameter</returns>
        public static ContractParameter Hash160(Hash160 value)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));
            
            return new ContractParameter(ContractParameterType.Hash160, value);
        }
        
        /// <summary>Creates a Hash256 parameter from the given hash.</summary>
        /// <param name="value">The 256-bit hash</param>
        /// <returns>The contract parameter</returns>
        public static ContractParameter Hash256(Hash256 value)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));
            
            return new ContractParameter(ContractParameterType.Hash256, value);
        }
        
        /// <summary>Creates a Hash256 parameter from the given byte array.</summary>
        /// <param name="value">The 256-bit hash in big-endian order</param>
        /// <returns>The contract parameter</returns>
        public static ContractParameter Hash256(byte[] value)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));
            
            return new ContractParameter(ContractParameterType.Hash256, new Hash256(value));
        }
        
        /// <summary>Creates a Hash256 parameter from the given hex string.</summary>
        /// <param name="hexValue">The 256-bit hash in hexadecimal and big-endian order</param>
        /// <returns>The contract parameter</returns>
        public static ContractParameter Hash256(string hexValue)
        {
            if (string.IsNullOrEmpty(hexValue))
                throw new ArgumentException("Hex value cannot be null or empty.", nameof(hexValue));
            
            return new ContractParameter(ContractParameterType.Hash256, new Hash256(hexValue));
        }
        
        /// <summary>Creates a PublicKey parameter from the given public key bytes.</summary>
        /// <param name="value">The public key in compressed format</param>
        /// <returns>The contract parameter</returns>
        public static ContractParameter PublicKey(byte[] value)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));
            
            if (value.Length != NeoConstants.PUBLIC_KEY_SIZE_COMPRESSED)
                throw new ArgumentException($"Public key must be {NeoConstants.PUBLIC_KEY_SIZE_COMPRESSED} bytes but was {value.Length} bytes.", nameof(value));
            
            return new ContractParameter(ContractParameterType.PublicKey, value);
        }
        
        /// <summary>Creates a PublicKey parameter from the given hex string.</summary>
        /// <param name="hexValue">The public key in hexadecimal representation</param>
        /// <returns>The contract parameter</returns>
        public static ContractParameter PublicKey(string hexValue)
        {
            if (string.IsNullOrEmpty(hexValue))
                throw new ArgumentException("Hex value cannot be null or empty.", nameof(hexValue));
            
            return PublicKey(Convert.FromHexString(hexValue));
        }
        
        /// <summary>Creates a PublicKey parameter from the given ECPublicKey.</summary>
        /// <param name="value">The public key</param>
        /// <returns>The contract parameter</returns>
        public static ContractParameter PublicKey(ECPublicKey value)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));
            
            return PublicKey(value.GetEncoded(true));
        }
        
        /// <summary>Creates an Array parameter from the given values.</summary>
        /// <param name="values">The array entries</param>
        /// <returns>The contract parameter</returns>
        public static ContractParameter Array(params object[] values)
        {
            return Array(values?.ToList() ?? new List<object>());
        }
        
        /// <summary>Creates an Array parameter from the given values.</summary>
        /// <param name="values">The array entries</param>
        /// <returns>The contract parameter</returns>
        public static ContractParameter Array(List<object> values)
        {
            if (values == null)
                values = new List<object>();
            
            var contractParams = values.Select(MapToContractParameter).ToList();
            return new ContractParameter(ContractParameterType.Array, contractParams);
        }
        
        /// <summary>Creates a Map parameter from the given dictionary.</summary>
        /// <param name="values">The map entries</param>
        /// <returns>The contract parameter</returns>
        public static ContractParameter Map(Dictionary<object, object> values)
        {
            if (values == null || values.Count == 0)
                throw new ArgumentException("At least one map entry is required to create a map contract parameter.", nameof(values));
            
            var contractMap = new Dictionary<ContractParameter, ContractParameter>();
            
            foreach (var kvp in values)
            {
                var key = MapToContractParameter(kvp.Key);
                var value = MapToContractParameter(kvp.Value);
                
                // Validate key type
                if (key.Type == ContractParameterType.Array || key.Type == ContractParameterType.Map)
                {
                    throw new ArgumentException("Map keys cannot be of type Array or Map.", nameof(values));
                }
                
                contractMap[key] = value;
            }
            
            return new ContractParameter(ContractParameterType.Map, contractMap);
        }
        
        #endregion
        
        #region Object Mapping
        
        /// <summary>
        /// Maps the given object to a contract parameter of the appropriate type.
        /// </summary>
        /// <param name="value">The object to map</param>
        /// <returns>The contract parameter</returns>
        public static ContractParameter MapToContractParameter(object value)
        {
            return value switch
            {
                null => Any(null),
                ContractParameter param => param,
                bool boolVal => Boolean(boolVal),
                byte byteVal => Integer(byteVal),
                int intVal => Integer(intVal),
                long longVal => Integer(longVal),
                BigInteger bigIntVal => Integer(bigIntVal),
                byte[] bytesVal => ByteArray(bytesVal),
                string stringVal => String(stringVal),
                Hash160 hash160Val => Hash160(hash160Val),
                Hash256 hash256Val => Hash256(hash256Val),
                Account accountVal => Hash160(accountVal),
                ECPublicKey pubKeyVal => PublicKey(pubKeyVal),
                ECDSASignature sigVal => Signature(sigVal),
                object[] arrayVal => Array(arrayVal),
                List<object> listVal => Array(listVal),
                Dictionary<object, object> dictVal => Map(dictVal),
                _ => throw new ArgumentException($"The provided object of type {value.GetType().Name} could not be converted to a supported contract parameter type.")
            };
        }
        
        #endregion
        
        #region JSON Serialization
        
        /// <summary>
        /// Custom JSON serialization to match Neo RPC format.
        /// </summary>
        [JsonIgnore]
        public object JsonValue
        {
            get
            {
                if (Value == null)
                    return null;
                
                return Type switch
                {
                    ContractParameterType.Any => "",
                    ContractParameterType.Boolean => Value,
                    ContractParameterType.Integer => Value,
                    ContractParameterType.ByteArray or ContractParameterType.Signature => 
                        Convert.ToBase64String((byte[])Value),
                    ContractParameterType.String or ContractParameterType.InteropInterface => Value,
                    ContractParameterType.Hash160 => ((Hash160)Value).ToString(),
                    ContractParameterType.Hash256 => ((Hash256)Value).ToString(),
                    ContractParameterType.PublicKey => ((byte[])Value).ToHexString(),
                    ContractParameterType.Array => Value,
                    ContractParameterType.Map => ConvertMapForJson((Dictionary<ContractParameter, ContractParameter>)Value),
                    _ => throw new NotSupportedException($"Parameter type '{Type}' not supported for JSON serialization.")
                };
            }
        }
        
        /// <summary>
        /// Converts a map to JSON-compatible format.
        /// </summary>
        /// <param name="map">The map to convert</param>
        /// <returns>JSON-compatible map representation</returns>
        private static List<Dictionary<string, ContractParameter>> ConvertMapForJson(Dictionary<ContractParameter, ContractParameter> map)
        {
            return map.Select(kvp => new Dictionary<string, ContractParameter>
            {
                ["key"] = kvp.Key,
                ["value"] = kvp.Value
            }).ToList();
        }
        
        #endregion
        
        #region Validation
        
        /// <summary>
        /// Validates that this parameter is properly configured.
        /// </summary>
        /// <exception cref="InvalidOperationException">If parameter is invalid</exception>
        public void Validate()
        {
            switch (Type)
            {
                case ContractParameterType.ByteArray:
                case ContractParameterType.Signature:
                    if (Value != null && !(Value is byte[]))
                        throw new InvalidOperationException($"Parameter of type {Type} must have byte[] value or null.");
                    break;
                    
                case ContractParameterType.String:
                case ContractParameterType.InteropInterface:
                    if (Value != null && !(Value is string))
                        throw new InvalidOperationException($"Parameter of type {Type} must have string value or null.");
                    break;
                    
                case ContractParameterType.Boolean:
                    if (Value != null && !(Value is bool))
                        throw new InvalidOperationException($"Parameter of type {Type} must have bool value or null.");
                    break;
                    
                case ContractParameterType.Integer:
                    if (Value != null && !(Value is int || Value is long || Value is BigInteger))
                        throw new InvalidOperationException($"Parameter of type {Type} must have numeric value or null.");
                    break;
                    
                case ContractParameterType.Hash160:
                    if (Value != null && !(Value is Hash160))
                        throw new InvalidOperationException($"Parameter of type {Type} must have Hash160 value or null.");
                    break;
                    
                case ContractParameterType.Hash256:
                    if (Value != null && !(Value is Hash256))
                        throw new InvalidOperationException($"Parameter of type {Type} must have Hash256 value or null.");
                    break;
                    
                case ContractParameterType.PublicKey:
                    if (Value != null && !(Value is byte[] pubKeyBytes && pubKeyBytes.Length == NeoConstants.PUBLIC_KEY_SIZE_COMPRESSED))
                        throw new InvalidOperationException($"Parameter of type {Type} must have {NeoConstants.PUBLIC_KEY_SIZE_COMPRESSED}-byte array value or null.");
                    break;
                    
                case ContractParameterType.Array:
                    if (Value != null && !(Value is List<ContractParameter>))
                        throw new InvalidOperationException($"Parameter of type {Type} must have List<ContractParameter> value or null.");
                    break;
                    
                case ContractParameterType.Map:
                    if (Value != null && !(Value is Dictionary<ContractParameter, ContractParameter>))
                        throw new InvalidOperationException($"Parameter of type {Type} must have Dictionary<ContractParameter, ContractParameter> value or null.");
                    break;
            }
        }
        
        #endregion
        
        #region Equality and Hashing
        
        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="other">The contract parameter to compare</param>
        /// <returns>True if equal, false otherwise</returns>
        public bool Equals(ContractParameter other)
        {
            if (other == null)
                return false;
            
            return Name == other.Name &&
                   Type == other.Type &&
                   Equals(Value, other.Value);
        }
        
        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="obj">The object to compare</param>
        /// <returns>True if equal, false otherwise</returns>
        public override bool Equals(object obj)
        {
            return Equals(obj as ContractParameter);
        }
        
        /// <summary>
        /// Returns a hash code for the current object.
        /// </summary>
        /// <returns>Hash code</returns>
        public override int GetHashCode()
        {
            return HashCode.Combine(Name, Type, Value);
        }
        
        #endregion
        
        #region String Representation
        
        /// <summary>
        /// Returns a string representation of this contract parameter.
        /// </summary>
        /// <returns>String representation</returns>
        public override string ToString()
        {
            var nameStr = !string.IsNullOrEmpty(Name) ? $"{Name}: " : "";
            var valueStr = Value?.ToString() ?? "null";
            
            // Truncate long values for readability
            if (valueStr.Length > 50)
            {
                valueStr = valueStr.Substring(0, 47) + "...";
            }
            
            return $"{nameStr}{Type}({valueStr})";
        }
        
        #endregion
    }
}
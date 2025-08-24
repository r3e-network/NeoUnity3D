using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using UnityEngine;
using Newtonsoft.Json;

namespace Neo.Unity.SDK.Types
{
    /// <summary>
    /// Represents a stack item returned by the Neo VM during script execution.
    /// Stack items can contain various types of data and are used for contract return values.
    /// </summary>
    [System.Serializable]
    public class StackItem
    {
        #region Constants
        
        // JSON type values
        public const string ANY_VALUE = "Any";
        public const string POINTER_VALUE = "Pointer";
        public const string BOOLEAN_VALUE = "Boolean";
        public const string INTEGER_VALUE = "Integer";
        public const string BYTE_STRING_VALUE = "ByteString";
        public const string BUFFER_VALUE = "Buffer";
        public const string ARRAY_VALUE = "Array";
        public const string STRUCT_VALUE = "Struct";
        public const string MAP_VALUE = "Map";
        public const string INTEROP_INTERFACE_VALUE = "InteropInterface";
        
        // Byte type values
        public const byte ANY_BYTE = 0x00;
        public const byte POINTER_BYTE = 0x10;
        public const byte BOOLEAN_BYTE = 0x20;
        public const byte INTEGER_BYTE = 0x21;
        public const byte BYTE_STRING_BYTE = 0x28;
        public const byte BUFFER_BYTE = 0x30;
        public const byte ARRAY_BYTE = 0x40;
        public const byte STRUCT_BYTE = 0x41;
        public const byte MAP_BYTE = 0x48;
        public const byte INTEROP_INTERFACE_BYTE = 0x60;
        
        #endregion
        
        #region Properties
        
        /// <summary>The type of this stack item</summary>
        [JsonProperty("type")]
        public string Type { get; set; }
        
        /// <summary>The value of this stack item</summary>
        [JsonProperty("value")]
        public object Value { get; set; }
        
        /// <summary>Iterator ID for InteropInterface items</summary>
        [JsonProperty("id")]
        public string Id { get; set; }
        
        /// <summary>Interface name for InteropInterface items</summary>
        [JsonProperty("interface")]
        public string Interface { get; set; }
        
        #endregion
        
        #region Constructors
        
        /// <summary>
        /// Creates a new stack item with the specified type and value.
        /// </summary>
        /// <param name="type">The stack item type</param>
        /// <param name="value">The stack item value</param>
        public StackItem(string type, object value)
        {
            Type = type ?? throw new ArgumentNullException(nameof(type));
            Value = value;
        }
        
        /// <summary>
        /// Creates a new InteropInterface stack item.
        /// </summary>
        /// <param name="iteratorId">The iterator ID</param>
        /// <param name="interfaceName">The interface name</param>
        public StackItem(string iteratorId, string interfaceName)
        {
            Type = INTEROP_INTERFACE_VALUE;
            Id = iteratorId;
            Interface = interfaceName;
        }
        
        /// <summary>
        /// Default constructor for JSON deserialization.
        /// </summary>
        public StackItem()
        {
        }
        
        #endregion
        
        #region Type Checking Methods
        
        /// <summary>Checks if this is an Any type stack item</summary>
        public bool IsAny() => Type == ANY_VALUE;
        
        /// <summary>Checks if this is a Pointer type stack item</summary>
        public bool IsPointer() => Type == POINTER_VALUE;
        
        /// <summary>Checks if this is a Boolean type stack item</summary>
        public bool IsBoolean() => Type == BOOLEAN_VALUE;
        
        /// <summary>Checks if this is an Integer type stack item</summary>
        public bool IsInteger() => Type == INTEGER_VALUE;
        
        /// <summary>Checks if this is a ByteString type stack item</summary>
        public bool IsByteString() => Type == BYTE_STRING_VALUE;
        
        /// <summary>Checks if this is a Buffer type stack item</summary>
        public bool IsBuffer() => Type == BUFFER_VALUE;
        
        /// <summary>Checks if this is an Array type stack item</summary>
        public bool IsArray() => Type == ARRAY_VALUE;
        
        /// <summary>Checks if this is a Struct type stack item</summary>
        public bool IsStruct() => Type == STRUCT_VALUE;
        
        /// <summary>Checks if this is a Map type stack item</summary>
        public bool IsMap() => Type == MAP_VALUE;
        
        /// <summary>Checks if this is an InteropInterface type stack item</summary>
        public bool IsInteropInterface() => Type == INTEROP_INTERFACE_VALUE;
        
        #endregion
        
        #region Value Extraction Methods
        
        /// <summary>
        /// Gets the value as a generic object.
        /// </summary>
        /// <returns>The underlying value</returns>
        public object GetValue()
        {
            return Type switch
            {
                ANY_VALUE => Value,
                POINTER_VALUE or INTEGER_VALUE => GetInteger(),
                BOOLEAN_VALUE => GetBoolean(),
                BYTE_STRING_VALUE or BUFFER_VALUE => GetByteArray(),
                ARRAY_VALUE or STRUCT_VALUE => GetList(),
                MAP_VALUE => GetMap(),
                INTEROP_INTERFACE_VALUE => Id,
                _ => Value
            };
        }
        
        /// <summary>
        /// Gets the value as a boolean.
        /// Supports conversion from multiple types.
        /// </summary>
        /// <returns>The boolean value</returns>
        public bool GetBoolean()
        {
            return Type switch
            {
                ANY_VALUE => Value is bool boolVal && boolVal,
                BOOLEAN_VALUE => Value is bool directBool && directBool,
                INTEGER_VALUE => GetInteger() != 0,
                BYTE_STRING_VALUE or BUFFER_VALUE => GetInteger() > 0,
                _ => false
            };
        }
        
        /// <summary>
        /// Gets the value as an integer.
        /// Supports conversion from multiple types.
        /// </summary>
        /// <returns>The integer value</returns>
        public int GetInteger()
        {
            return Type switch
            {
                ANY_VALUE when Value is int intVal => intVal,
                BOOLEAN_VALUE => GetBoolean() ? 1 : 0,
                POINTER_VALUE or INTEGER_VALUE when Value is string strVal => int.Parse(strVal),
                POINTER_VALUE or INTEGER_VALUE when Value is int intVal => intVal,
                POINTER_VALUE or INTEGER_VALUE when Value is long longVal => (int)longVal,
                BYTE_STRING_VALUE or BUFFER_VALUE => GetIntegerFromBytes(),
                _ => throw new InvalidCastException($"Cannot convert {Type} to integer")
            };
        }
        
        /// <summary>
        /// Gets integer value from byte array (little-endian).
        /// </summary>
        /// <returns>The integer value</returns>
        private int GetIntegerFromBytes()
        {
            var bytes = GetByteArray();
            if (bytes.Length == 0)
                return 0;
            
            // Convert from little-endian byte array to integer
            var reversed = bytes.Reverse().ToArray();
            return new BigInteger(reversed, isUnsigned: false, isBigEndian: true).ToInt32();
        }
        
        /// <summary>
        /// Gets the value as a string.
        /// Supports conversion from multiple types.
        /// </summary>
        /// <returns>The string value</returns>
        public string GetString()
        {
            return Type switch
            {
                ANY_VALUE when Value is string strVal => strVal,
                BOOLEAN_VALUE => GetBoolean() ? "true" : "false",
                INTEGER_VALUE => GetInteger().ToString(),
                BYTE_STRING_VALUE or BUFFER_VALUE => GetStringFromBytes(),
                STRING_VALUE when Value is string directStr => directStr,
                _ => Value?.ToString() ?? ""
            };
        }
        
        /// <summary>
        /// Gets string value from byte array using UTF-8 encoding.
        /// </summary>
        /// <returns>The string value</returns>
        private string GetStringFromBytes()
        {
            var bytes = GetByteArray();
            try
            {
                return System.Text.Encoding.UTF8.GetString(bytes);
            }
            catch
            {
                // If UTF-8 decoding fails, return hex representation
                return Convert.ToHexString(bytes);
            }
        }
        
        /// <summary>
        /// Gets the value as a hex string.
        /// </summary>
        /// <returns>The hex string representation</returns>
        public string GetHexString()
        {
            return Type switch
            {
                BYTE_STRING_VALUE or BUFFER_VALUE => Convert.ToHexString(GetByteArray()),
                INTEGER_VALUE => GetByteArray().ToHexString(),
                _ => throw new InvalidCastException($"Cannot convert {Type} to hex string")
            };
        }
        
        /// <summary>
        /// Gets the value as a byte array.
        /// </summary>
        /// <returns>The byte array</returns>
        public byte[] GetByteArray()
        {
            return Type switch
            {
                BYTE_STRING_VALUE or BUFFER_VALUE when Value is string base64Str => 
                    Convert.FromBase64String(base64Str),
                BYTE_STRING_VALUE or BUFFER_VALUE when Value is byte[] bytesVal => 
                    bytesVal,
                INTEGER_VALUE => ConvertIntegerToBytes(),
                _ => throw new InvalidCastException($"Cannot convert {Type} to byte array")
            };
        }
        
        /// <summary>
        /// Converts integer value to byte array (little-endian).
        /// </summary>
        /// <returns>The byte array representation</returns>
        private byte[] ConvertIntegerToBytes()
        {
            var intValue = GetInteger();
            var bigInt = new BigInteger(intValue);
            return bigInt.ToByteArray(); // Little-endian by default
        }
        
        /// <summary>
        /// Gets the value as a list of stack items.
        /// </summary>
        /// <returns>The list of stack items</returns>
        public List<StackItem> GetList()
        {
            return Type switch
            {
                ARRAY_VALUE or STRUCT_VALUE when Value is List<StackItem> listVal => listVal,
                ARRAY_VALUE or STRUCT_VALUE when Value is StackItem[] arrayVal => arrayVal.ToList(),
                _ => throw new InvalidCastException($"Cannot convert {Type} to list")
            };
        }
        
        /// <summary>
        /// Gets the value as a dictionary map.
        /// </summary>
        /// <returns>The dictionary map</returns>
        public Dictionary<StackItem, StackItem> GetMap()
        {
            if (Type != MAP_VALUE)
                throw new InvalidCastException($"Cannot convert {Type} to map");
            
            if (Value is Dictionary<StackItem, StackItem> dictVal)
                return dictVal;
            
            // Handle JSON deserialization format
            if (Value is List<Dictionary<string, StackItem>> jsonMap)
            {
                var result = new Dictionary<StackItem, StackItem>();
                foreach (var item in jsonMap)
                {
                    if (item.TryGetValue("key", out var key) && item.TryGetValue("value", out var value))
                    {
                        result[key] = value;
                    }
                }
                return result;
            }
            
            throw new InvalidCastException("Cannot convert value to map");
        }
        
        /// <summary>
        /// Gets the iterator ID from an InteropInterface stack item.
        /// </summary>
        /// <returns>The iterator ID</returns>
        public string GetIteratorId()
        {
            if (Type != INTEROP_INTERFACE_VALUE)
                throw new InvalidCastException($"Cannot get iterator ID from {Type}");
            
            return Id ?? throw new InvalidOperationException("InteropInterface stack item has no iterator ID");
        }
        
        /// <summary>
        /// Gets the address from a ByteString representing a script hash.
        /// </summary>
        /// <returns>The Neo address</returns>
        public string GetAddress()
        {
            if (Type != BYTE_STRING_VALUE && Type != BUFFER_VALUE)
                throw new InvalidCastException($"Cannot convert {Type} to address");
            
            var bytes = GetByteArray();
            if (bytes.Length != 20) // Hash160 is 20 bytes
                throw new InvalidOperationException("Byte array must be 20 bytes for address conversion");
            
            // Reverse bytes for Hash160 format (little-endian to big-endian)
            var reversed = bytes.Reverse().ToArray();
            var hash160 = new Hash160(reversed);
            return hash160.ToAddress();
        }
        
        #endregion
        
        #region String Representation
        
        /// <summary>
        /// Gets a string representation of the value with length limit.
        /// </summary>
        /// <returns>String representation of the value</returns>
        public string GetValueString()
        {
            const int maxLength = 80;
            
            var valueString = Type switch
            {
                ANY_VALUE => Value?.ToString() ?? "null",
                POINTER_VALUE or INTEGER_VALUE => GetInteger().ToString(),
                BOOLEAN_VALUE => GetBoolean() ? "true" : "false",
                BYTE_STRING_VALUE or BUFFER_VALUE => Convert.ToHexString(GetByteArray()),
                ARRAY_VALUE or STRUCT_VALUE => string.Join(", ", GetList().Select(item => item.ToString())),
                MAP_VALUE => string.Join(", ", GetMap().Select(kvp => $"{kvp.Key} -> {kvp.Value}")),
                INTEROP_INTERFACE_VALUE => Id ?? "",
                _ => Value?.ToString() ?? ""
            };
            
            return valueString.Length > maxLength ? 
                valueString.Substring(0, maxLength - 3) + "..." : 
                valueString;
        }
        
        /// <summary>
        /// Returns a string representation of this stack item.
        /// </summary>
        /// <returns>String representation</returns>
        public override string ToString()
        {
            return $"{Type}{{value='{GetValueString()}'}}";
        }
        
        #endregion
        
        #region Equality and Hashing
        
        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="obj">The object to compare</param>
        /// <returns>True if equal, false otherwise</returns>
        public override bool Equals(object obj)
        {
            if (obj is StackItem other)
            {
                return Type == other.Type && 
                       Equals(Value, other.Value) &&
                       Id == other.Id &&
                       Interface == other.Interface;
            }
            return false;
        }
        
        /// <summary>
        /// Returns a hash code for the current object.
        /// </summary>
        /// <returns>Hash code</returns>
        public override int GetHashCode()
        {
            return HashCode.Combine(Type, Value, Id, Interface);
        }
        
        #endregion
        
        #region Static Factory Methods
        
        /// <summary>Creates an Any stack item.</summary>
        public static StackItem CreateAny(object value = null) => new StackItem(ANY_VALUE, value);
        
        /// <summary>Creates a Pointer stack item.</summary>
        public static StackItem CreatePointer(int value) => new StackItem(POINTER_VALUE, value);
        
        /// <summary>Creates a Boolean stack item.</summary>
        public static StackItem CreateBoolean(bool value) => new StackItem(BOOLEAN_VALUE, value);
        
        /// <summary>Creates an Integer stack item.</summary>
        public static StackItem CreateInteger(int value) => new StackItem(INTEGER_VALUE, value);
        
        /// <summary>Creates an Integer stack item from BigInteger.</summary>
        public static StackItem CreateInteger(BigInteger value) => new StackItem(INTEGER_VALUE, value.ToString());
        
        /// <summary>Creates a ByteString stack item.</summary>
        public static StackItem CreateByteString(byte[] value) => new StackItem(BYTE_STRING_VALUE, Convert.ToBase64String(value ?? new byte[0]));
        
        /// <summary>Creates a Buffer stack item.</summary>
        public static StackItem CreateBuffer(byte[] value) => new StackItem(BUFFER_VALUE, Convert.ToBase64String(value ?? new byte[0]));
        
        /// <summary>Creates an Array stack item.</summary>
        public static StackItem CreateArray(List<StackItem> value) => new StackItem(ARRAY_VALUE, value ?? new List<StackItem>());
        
        /// <summary>Creates a Struct stack item.</summary>
        public static StackItem CreateStruct(List<StackItem> value) => new StackItem(STRUCT_VALUE, value ?? new List<StackItem>());
        
        /// <summary>Creates a Map stack item.</summary>
        public static StackItem CreateMap(Dictionary<StackItem, StackItem> value) => new StackItem(MAP_VALUE, ConvertMapForJson(value ?? new Dictionary<StackItem, StackItem>()));
        
        /// <summary>Creates an InteropInterface stack item.</summary>
        public static StackItem CreateInteropInterface(string iteratorId, string interfaceName = "IIterator") => new StackItem(iteratorId, interfaceName);
        
        /// <summary>
        /// Converts a map to JSON-serializable format.
        /// </summary>
        /// <param name="map">The map to convert</param>
        /// <returns>JSON-compatible representation</returns>
        private static List<Dictionary<string, StackItem>> ConvertMapForJson(Dictionary<StackItem, StackItem> map)
        {
            return map.Select(kvp => new Dictionary<string, StackItem>
            {
                ["key"] = kvp.Key,
                ["value"] = kvp.Value
            }).ToList();
        }
        
        #endregion
    }
}
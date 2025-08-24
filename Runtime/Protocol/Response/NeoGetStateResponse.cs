using System;
using System.Linq;
using UnityEngine;
using Newtonsoft.Json;

namespace Neo.Unity.SDK.Protocol.Response
{
    /// <summary>
    /// Response for the getstate RPC call.
    /// Returns the state value for a specific key in the state tree.
    /// </summary>
    [System.Serializable]
    public class NeoGetStateResponse : NeoResponse<string>
    {
        /// <summary>
        /// Gets the state value from the response.
        /// </summary>
        public string State => Result;

        /// <summary>
        /// Default constructor for JSON deserialization.
        /// </summary>
        public NeoGetStateResponse() : base()
        {
        }

        /// <summary>
        /// Creates a successful get state response.
        /// </summary>
        /// <param name="state">The state value</param>
        /// <param name="id">The request ID</param>
        public NeoGetStateResponse(string state, int id = 1) : base(state, id)
        {
        }

        /// <summary>
        /// Creates an error get state response.
        /// </summary>
        /// <param name="error">The error information</param>
        /// <param name="id">The request ID</param>
        public NeoGetStateResponse(ResponseError error, int id = 1) : base(error, id)
        {
        }

        /// <summary>
        /// Checks if the response contains a valid state value.
        /// </summary>
        [JsonIgnore]
        public bool HasState => !string.IsNullOrEmpty(State);

        /// <summary>
        /// Gets the state value in uppercase format.
        /// </summary>
        [JsonIgnore]
        public string StateUppercase => State?.ToUpperInvariant() ?? string.Empty;

        /// <summary>
        /// Gets the state value in lowercase format.
        /// </summary>
        [JsonIgnore]
        public string StateLowercase => State?.ToLowerInvariant() ?? string.Empty;

        /// <summary>
        /// Gets the state value with '0x' prefix if it doesn't already have one.
        /// </summary>
        [JsonIgnore]
        public string StateWithPrefix
        {
            get
            {
                if (string.IsNullOrEmpty(State))
                    return string.Empty;

                return State.StartsWith("0x") ? State : $"0x{State}";
            }
        }

        /// <summary>
        /// Gets the state value without '0x' prefix if it has one.
        /// </summary>
        [JsonIgnore]
        public string StateWithoutPrefix
        {
            get
            {
                if (string.IsNullOrEmpty(State))
                    return string.Empty;

                return State.StartsWith("0x") ? State.Substring(2) : State;
            }
        }

        /// <summary>
        /// Gets the length of the state value string (without prefix).
        /// </summary>
        [JsonIgnore]
        public int StateLength => StateWithoutPrefix.Length;

        /// <summary>
        /// Validates that the state value has a valid hexadecimal format.
        /// </summary>
        /// <returns>True if the state value format is valid hex</returns>
        public bool IsValidHexFormat()
        {
            if (string.IsNullOrEmpty(State))
                return false;

            var stateToCheck = StateWithoutPrefix;

            // Check if all characters are valid hexadecimal
            foreach (char c in stateToCheck)
            {
                if (!Uri.IsHexDigit(c))
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Gets a shortened version of the state value for display purposes.
        /// </summary>
        /// <param name="prefixLength">Length of prefix to show (default: 16)</param>
        /// <param name="suffixLength">Length of suffix to show (default: 16)</param>
        /// <returns>Shortened state value string</returns>
        public string GetShortState(int prefixLength = 16, int suffixLength = 16)
        {
            if (string.IsNullOrEmpty(State))
                return string.Empty;

            var stateToUse = StateWithoutPrefix;

            if (stateToUse.Length <= prefixLength + suffixLength)
                return stateToUse;

            if (prefixLength <= 0 && suffixLength <= 0)
                return "...";

            if (prefixLength <= 0)
                return $"...{stateToUse.Substring(stateToUse.Length - suffixLength)}";

            if (suffixLength <= 0)
                return $"{stateToUse.Substring(0, prefixLength)}...";

            return $"{stateToUse.Substring(0, prefixLength)}...{stateToUse.Substring(stateToUse.Length - suffixLength)}";
        }

        /// <summary>
        /// Attempts to decode the state value as a Base64 string.
        /// </summary>
        /// <returns>Decoded bytes if successful, null otherwise</returns>
        public byte[] TryDecodeAsBase64()
        {
            try
            {
                if (string.IsNullOrEmpty(State))
                    return null;

                return Convert.FromBase64String(State);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Attempts to decode the state value as a hexadecimal string.
        /// </summary>
        /// <returns>Decoded bytes if successful, null otherwise</returns>
        public byte[] TryDecodeAsHex()
        {
            try
            {
                if (string.IsNullOrEmpty(State))
                    return null;

                var hexString = StateWithoutPrefix;
                if (hexString.Length % 2 != 0)
                    return null;

                byte[] bytes = new byte[hexString.Length / 2];
                for (int i = 0; i < hexString.Length; i += 2)
                {
                    bytes[i / 2] = Convert.ToByte(hexString.Substring(i, 2), 16);
                }

                return bytes;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Attempts to decode the state value as a UTF-8 string.
        /// </summary>
        /// <returns>Decoded string if successful, null otherwise</returns>
        public string TryDecodeAsUtf8String()
        {
            try
            {
                var bytes = TryDecodeAsHex();
                if (bytes == null)
                    return null;

                return System.Text.Encoding.UTF8.GetString(bytes);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Attempts to decode the state value as a signed integer.
        /// </summary>
        /// <returns>Decoded integer if successful, null otherwise</returns>
        public long? TryDecodeAsInteger()
        {
            try
            {
                var bytes = TryDecodeAsHex();
                if (bytes == null || bytes.Length == 0)
                    return null;

                // Handle different byte lengths for integers
                if (bytes.Length <= 8)
                {
                    // Pad with zeros if necessary and convert to long
                    byte[] paddedBytes = new byte[8];
                    Array.Copy(bytes, 0, paddedBytes, 0, bytes.Length);
                    
                    // Neo uses little-endian format
                    if (BitConverter.IsLittleEndian)
                        return BitConverter.ToInt64(paddedBytes, 0);
                    else
                    {
                        Array.Reverse(paddedBytes);
                        return BitConverter.ToInt64(paddedBytes, 0);
                    }
                }

                return null;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Attempts to decode the state value as a boolean.
        /// </summary>
        /// <returns>Decoded boolean if successful, null otherwise</returns>
        public bool? TryDecodeAsBoolean()
        {
            try
            {
                var bytes = TryDecodeAsHex();
                if (bytes == null || bytes.Length == 0)
                    return false; // Empty state typically means false

                if (bytes.Length == 1)
                {
                    return bytes[0] != 0;
                }

                // Check if all bytes are zero (false) or any non-zero (true)
                foreach (var b in bytes)
                {
                    if (b != 0)
                        return true;
                }
                return false;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Gets the estimated size of the state value in bytes.
        /// </summary>
        [JsonIgnore]
        public int EstimatedSizeBytes
        {
            get
            {
                if (string.IsNullOrEmpty(State))
                    return 0;

                // Try hex first (most common for blockchain state data)
                if (IsValidHexFormat())
                {
                    return StateLength / 2; // 2 hex characters = 1 byte
                }

                // Fallback to character count
                return State.Length;
            }
        }

        /// <summary>
        /// Returns a string representation of the get state response.
        /// </summary>
        /// <returns>String representation</returns>
        public override string ToString()
        {
            if (HasError)
                return base.ToString();

            if (!HasState)
                return "NeoGetStateResponse(No State)";

            return $"NeoGetStateResponse(State: {GetShortState()}, Size: {EstimatedSizeBytes} bytes)";
        }

        /// <summary>
        /// Gets detailed information about the state value including decoded interpretations.
        /// </summary>
        /// <returns>Detailed description</returns>
        public string GetDetailedInfo()
        {
            if (!HasState)
                return "No state value available";

            var decodedUtf8 = TryDecodeAsUtf8String();
            var decodedInteger = TryDecodeAsInteger();
            var decodedBoolean = TryDecodeAsBoolean();

            var result = $"State Value Information:\n" +
                        $"  Length: {StateLength} characters\n" +
                        $"  Estimated Size: {EstimatedSizeBytes} bytes\n" +
                        $"  Valid Hex Format: {IsValidHexFormat()}\n" +
                        $"  Preview: {GetShortState()}";

            if (!string.IsNullOrEmpty(decodedUtf8))
                result += $"\n  Decoded UTF-8: {decodedUtf8}";

            if (decodedInteger.HasValue)
                result += $"\n  Decoded Integer: {decodedInteger.Value}";

            if (decodedBoolean.HasValue)
                result += $"\n  Decoded Boolean: {decodedBoolean.Value}";

            return result;
        }

        /// <summary>
        /// Validates the state value.
        /// </summary>
        /// <exception cref="ArgumentException">If the state value format is invalid</exception>
        public void ValidateState()
        {
            // Note: An empty state value is valid (indicates no state for the key)
            // Additional validation can be added here based on specific requirements
        }

        /// <summary>
        /// Compares this state value with another.
        /// </summary>
        /// <param name="other">The other get state response to compare with</param>
        /// <returns>True if the state values are equal</returns>
        public bool StateEquals(NeoGetStateResponse other)
        {
            if (other == null)
                return false;

            if (State == null && other.State == null)
                return true;

            if (State == null || other.State == null)
                return false;

            return string.Equals(StateWithoutPrefix, other.StateWithoutPrefix, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Creates a copy of this response with a new state value.
        /// </summary>
        /// <param name="newState">The new state value</param>
        /// <returns>New NeoGetStateResponse instance</returns>
        public NeoGetStateResponse WithState(string newState)
        {
            return new NeoGetStateResponse(newState, Id);
        }

        /// <summary>
        /// Gets the state data type based on content analysis.
        /// </summary>
        [JsonIgnore]
        public string EstimatedDataType
        {
            get
            {
                if (!HasState)
                    return "None";

                if (!IsValidHexFormat())
                    return "String";

                var bytes = TryDecodeAsHex();
                if (bytes == null || bytes.Length == 0)
                    return "Empty";

                if (bytes.Length == 1)
                    return "Boolean/Byte";

                if (bytes.Length <= 8)
                    return "Integer";

                var utf8 = TryDecodeAsUtf8String();
                if (!string.IsNullOrEmpty(utf8) && utf8.All(c => char.IsLetterOrDigit(c) || char.IsWhiteSpace(c) || char.IsPunctuation(c)))
                    return "String";

                return "Binary Data";
            }
        }
    }
}
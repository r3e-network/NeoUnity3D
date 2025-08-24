using System;
using System.ComponentModel;
using Newtonsoft.Json;

namespace NeoUnity.Protocol.Response
{
    /// <summary>
    /// Represents Oracle response status codes
    /// </summary>
    [Serializable]
    [JsonConverter(typeof(OracleResponseCodeConverter))]
    public enum OracleResponseCode : byte
    {
        /// <summary>
        /// Oracle request completed successfully
        /// </summary>
        [Description("Success")]
        Success = 0x00,
        
        /// <summary>
        /// Protocol not supported by oracle
        /// </summary>
        [Description("ProtocolNotSupported")]
        ProtocolNotSupported = 0x10,
        
        /// <summary>
        /// Consensus unreachable among oracle nodes
        /// </summary>
        [Description("ConsensusUnreachable")]
        ConsensusUnreachable = 0x12,
        
        /// <summary>
        /// Requested resource not found
        /// </summary>
        [Description("NotFound")]
        NotFound = 0x14,
        
        /// <summary>
        /// Request timed out
        /// </summary>
        [Description("Timeout")]
        Timeout = 0x16,
        
        /// <summary>
        /// Access forbidden to requested resource
        /// </summary>
        [Description("Forbidden")]
        Forbidden = 0x18,
        
        /// <summary>
        /// Response data too large
        /// </summary>
        [Description("ResponseTooLarge")]
        ResponseTooLarge = 0x1a,
        
        /// <summary>
        /// Insufficient funds for oracle request
        /// </summary>
        [Description("InsufficientFunds")]
        InsufficientFunds = 0x1c,
        
        /// <summary>
        /// Content type not supported
        /// </summary>
        [Description("ContentTypeNotSupported")]
        ContentTypeNotSupported = 0x1f,
        
        /// <summary>
        /// General error occurred
        /// </summary>
        [Description("Error")]
        Error = 0xff
    }
    
    /// <summary>
    /// Extension methods for OracleResponseCode
    /// </summary>
    public static class OracleResponseCodeExtensions
    {
        /// <summary>
        /// Gets whether the response code indicates success
        /// </summary>
        /// <param name="code">Oracle response code</param>
        /// <returns>True if successful</returns>
        public static bool IsSuccess(this OracleResponseCode code)
        {
            return code == OracleResponseCode.Success;
        }
        
        /// <summary>
        /// Gets whether the response code indicates a retryable error
        /// </summary>
        /// <param name="code">Oracle response code</param>
        /// <returns>True if retryable</returns>
        public static bool IsRetryable(this OracleResponseCode code)
        {
            return code switch
            {
                OracleResponseCode.Timeout => true,
                OracleResponseCode.ConsensusUnreachable => true,
                OracleResponseCode.InsufficientFunds => false, // Don't retry funding issues
                OracleResponseCode.NotFound => false, // Don't retry 404s
                OracleResponseCode.Forbidden => false, // Don't retry access denied
                _ => false
            };
        }
        
        /// <summary>
        /// Gets the description attribute value for the enum
        /// </summary>
        /// <param name="code">Oracle response code</param>
        /// <returns>Description string</returns>
        public static string GetDescription(this OracleResponseCode code)
        {
            var field = code.GetType().GetField(code.ToString());
            if (field?.GetCustomAttributes(typeof(DescriptionAttribute), false) is DescriptionAttribute[] attributes && attributes.Length > 0)
            {
                return attributes[0].Description;
            }
            return code.ToString();
        }
        
        /// <summary>
        /// Gets a user-friendly message for the response code
        /// </summary>
        /// <param name="code">Oracle response code</param>
        /// <returns>User-friendly message</returns>
        public static string GetUserMessage(this OracleResponseCode code)
        {
            return code switch
            {
                OracleResponseCode.Success => "Oracle request completed successfully",
                OracleResponseCode.ProtocolNotSupported => "The requested protocol is not supported by the oracle",
                OracleResponseCode.ConsensusUnreachable => "Oracle nodes could not reach consensus on the response",
                OracleResponseCode.NotFound => "The requested resource was not found",
                OracleResponseCode.Timeout => "The oracle request timed out",
                OracleResponseCode.Forbidden => "Access to the requested resource is forbidden",
                OracleResponseCode.ResponseTooLarge => "The response data is too large to process",
                OracleResponseCode.InsufficientFunds => "Insufficient funds to complete the oracle request",
                OracleResponseCode.ContentTypeNotSupported => "The content type of the response is not supported",
                OracleResponseCode.Error => "An error occurred while processing the oracle request",
                _ => "Unknown oracle response code"
            };
        }
    }
    
    /// <summary>
    /// JSON converter for OracleResponseCode enum
    /// </summary>
    public class OracleResponseCodeConverter : JsonConverter<OracleResponseCode>
    {
        public override void WriteJson(JsonWriter writer, OracleResponseCode value, JsonSerializer serializer)
        {
            writer.WriteValue(value.GetDescription());
        }
        
        public override OracleResponseCode ReadJson(JsonReader reader, Type objectType, OracleResponseCode existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            if (reader.Value is string stringValue)
            {
                return stringValue switch
                {
                    "Success" => OracleResponseCode.Success,
                    "ProtocolNotSupported" => OracleResponseCode.ProtocolNotSupported,
                    "ConsensusUnreachable" => OracleResponseCode.ConsensusUnreachable,
                    "NotFound" => OracleResponseCode.NotFound,
                    "Timeout" => OracleResponseCode.Timeout,
                    "Forbidden" => OracleResponseCode.Forbidden,
                    "ResponseTooLarge" => OracleResponseCode.ResponseTooLarge,
                    "InsufficientFunds" => OracleResponseCode.InsufficientFunds,
                    "ContentTypeNotSupported" => OracleResponseCode.ContentTypeNotSupported,
                    "Error" => OracleResponseCode.Error,
                    _ => OracleResponseCode.Error
                };
            }
            
            if (reader.Value is long longValue)
            {
                return (OracleResponseCode)(byte)longValue;
            }
            
            return OracleResponseCode.Error;
        }
    }
}
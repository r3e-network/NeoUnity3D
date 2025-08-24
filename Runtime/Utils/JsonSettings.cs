using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Neo.Unity.SDK.Utils
{
    /// <summary>
    /// JSON serialization settings for Neo Unity SDK.
    /// Provides consistent JSON configuration across all SDK components.
    /// </summary>
    public static class JsonSettings
    {
        /// <summary>
        /// Default JSON serializer settings optimized for Neo blockchain data.
        /// </summary>
        public static readonly JsonSerializerSettings Default = new JsonSerializerSettings
        {
            // Use camelCase property names to match Neo RPC API
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
            
            // Handle null values gracefully
            NullValueHandling = NullValueHandling.Ignore,
            
            // Use ISO date format
            DateFormatHandling = DateFormatHandling.IsoDateFormat,
            DateTimeZoneHandling = DateTimeZoneHandling.Utc,
            
            // Preserve decimal precision
            FloatParseHandling = FloatParseHandling.Decimal,
            
            // Handle missing members gracefully
            MissingMemberHandling = MissingMemberHandling.Ignore,
            
            // Use compact formatting by default (override for debugging)
            Formatting = Formatting.None,
            
            // Don't escape forward slashes
            StringEscapeHandling = StringEscapeHandling.Default
        };
        
        /// <summary>
        /// JSON serializer settings for debugging with indented formatting.
        /// </summary>
        public static readonly JsonSerializerSettings Debug = new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
            NullValueHandling = NullValueHandling.Ignore,
            DateFormatHandling = DateFormatHandling.IsoDateFormat,
            DateTimeZoneHandling = DateTimeZoneHandling.Utc,
            FloatParseHandling = FloatParseHandling.Decimal,
            MissingMemberHandling = MissingMemberHandling.Ignore,
            Formatting = Formatting.Indented, // Pretty-print for debugging
            StringEscapeHandling = StringEscapeHandling.Default
        };
        
        /// <summary>
        /// JSON serializer settings for strict deserialization with error on missing members.
        /// </summary>
        public static readonly JsonSerializerSettings Strict = new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
            NullValueHandling = NullValueHandling.Include,
            DateFormatHandling = DateFormatHandling.IsoDateFormat,
            DateTimeZoneHandling = DateTimeZoneHandling.Utc,
            FloatParseHandling = FloatParseHandling.Decimal,
            MissingMemberHandling = MissingMemberHandling.Error, // Throw on missing members
            Formatting = Formatting.None,
            StringEscapeHandling = StringEscapeHandling.Default
        };
    }
}
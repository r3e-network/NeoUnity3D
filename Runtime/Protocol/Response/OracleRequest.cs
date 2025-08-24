using System;
using Newtonsoft.Json;
using UnityEngine;
using NeoUnity.Cryptography;

namespace NeoUnity.Protocol.Response
{
    /// <summary>
    /// Represents an Oracle request with callback information
    /// </summary>
    [Serializable]
    public struct OracleRequest : IEquatable<OracleRequest>
    {
        [JsonProperty("requestid")]
        [SerializeField] private int _requestId;
        
        [JsonProperty("originaltxid")]
        [SerializeField] private string _originalTransactionHash;
        
        [JsonProperty("gasforresponse")]
        [SerializeField] private long _gasForResponse;
        
        [JsonProperty("url")]
        [SerializeField] private string _url;
        
        [JsonProperty("filter")]
        [SerializeField] private string _filter;
        
        [JsonProperty("callbackcontract")]
        [SerializeField] private string _callbackContract;
        
        [JsonProperty("callbackmethod")]
        [SerializeField] private string _callbackMethod;
        
        [JsonProperty("userdata")]
        [SerializeField] private string _userData;
        
        /// <summary>
        /// Gets the unique request identifier
        /// </summary>
        public int RequestId => _requestId;
        
        /// <summary>
        /// Gets the original transaction hash that created this request
        /// </summary>
        public string OriginalTransactionHashString => _originalTransactionHash;
        
        /// <summary>
        /// Gets the original transaction hash as Hash256
        /// </summary>
        public Hash256 OriginalTransactionHash => new Hash256(_originalTransactionHash);
        
        /// <summary>
        /// Gets the GAS allocated for the response
        /// </summary>
        public long GasForResponse => _gasForResponse;
        
        /// <summary>
        /// Gets the URL to fetch data from
        /// </summary>
        public string Url => _url;
        
        /// <summary>
        /// Gets the JSON path filter for extracting data
        /// </summary>
        public string Filter => _filter;
        
        /// <summary>
        /// Gets the callback contract hash
        /// </summary>
        public string CallbackContractString => _callbackContract;
        
        /// <summary>
        /// Gets the callback contract hash as Hash160
        /// </summary>
        public Hash160 CallbackContract => new Hash160(_callbackContract);
        
        /// <summary>
        /// Gets the callback method name
        /// </summary>
        public string CallbackMethod => _callbackMethod;
        
        /// <summary>
        /// Gets the user-provided data
        /// </summary>
        public string UserData => _userData;
        
        /// <summary>
        /// Gets the GAS for response as decimal
        /// </summary>
        public decimal GasForResponseDecimal => _gasForResponse / 100_000_000m;
        
        /// <summary>
        /// Initializes a new instance of OracleRequest
        /// </summary>
        [JsonConstructor]
        public OracleRequest(
            int requestId,
            string originalTransactionHash,
            long gasForResponse,
            string url,
            string filter,
            string callbackContract,
            string callbackMethod,
            string userData)
        {
            _requestId = requestId;
            _originalTransactionHash = originalTransactionHash ?? throw new ArgumentNullException(nameof(originalTransactionHash));
            _gasForResponse = gasForResponse;
            _url = url ?? throw new ArgumentNullException(nameof(url));
            _filter = filter ?? "";
            _callbackContract = callbackContract ?? throw new ArgumentNullException(nameof(callbackContract));
            _callbackMethod = callbackMethod ?? throw new ArgumentNullException(nameof(callbackMethod));
            _userData = userData ?? "";
        }
        
        /// <summary>
        /// Initializes a new instance of OracleRequest with typed hashes
        /// </summary>
        public OracleRequest(
            int requestId,
            Hash256 originalTransactionHash,
            long gasForResponse,
            string url,
            string filter,
            Hash160 callbackContract,
            string callbackMethod,
            string userData)
        {
            _requestId = requestId;
            _originalTransactionHash = originalTransactionHash?.ToString() ?? throw new ArgumentNullException(nameof(originalTransactionHash));
            _gasForResponse = gasForResponse;
            _url = url ?? throw new ArgumentNullException(nameof(url));
            _filter = filter ?? "";
            _callbackContract = callbackContract?.ToString() ?? throw new ArgumentNullException(nameof(callbackContract));
            _callbackMethod = callbackMethod ?? throw new ArgumentNullException(nameof(callbackMethod));
            _userData = userData ?? "";
        }
        
        /// <summary>
        /// Gets whether this request has a filter
        /// </summary>
        public bool HasFilter => !string.IsNullOrEmpty(_filter);
        
        /// <summary>
        /// Gets whether this request has user data
        /// </summary>
        public bool HasUserData => !string.IsNullOrEmpty(_userData);
        
        /// <summary>
        /// Validates if the URL is a valid HTTP/HTTPS URL
        /// </summary>
        /// <returns>True if URL is valid</returns>
        public bool IsValidUrl()
        {
            return Uri.TryCreate(_url, UriKind.Absolute, out var uri) && 
                   (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
        }
        
        /// <summary>
        /// Gets the estimated cost for this oracle request
        /// </summary>
        /// <returns>Estimated cost based on URL length and gas allocated</returns>
        public decimal GetEstimatedCost()
        {
            // Base cost + URL length factor + gas for response
            var baseCost = 0.01m; // Base oracle fee
            var urlCost = (_url?.Length ?? 0) * 0.001m;
            return baseCost + urlCost + GasForResponseDecimal;
        }
        
        public bool Equals(OracleRequest other)
        {
            return _requestId == other._requestId &&
                   _originalTransactionHash == other._originalTransactionHash &&
                   _gasForResponse == other._gasForResponse &&
                   _url == other._url &&
                   _filter == other._filter &&
                   _callbackContract == other._callbackContract &&
                   _callbackMethod == other._callbackMethod &&
                   _userData == other._userData;
        }
        
        public override bool Equals(object obj)
        {
            return obj is OracleRequest other && Equals(other);
        }
        
        public override int GetHashCode()
        {
            return HashCode.Combine(
                _requestId, 
                _originalTransactionHash, 
                _gasForResponse, 
                _url, 
                _filter, 
                _callbackContract, 
                _callbackMethod, 
                _userData);
        }
        
        public static bool operator ==(OracleRequest left, OracleRequest right)
        {
            return left.Equals(right);
        }
        
        public static bool operator !=(OracleRequest left, OracleRequest right)
        {
            return !left.Equals(right);
        }
        
        public override string ToString()
        {
            return $"OracleRequest(Id: {_requestId}, URL: {_url}, Callback: {_callbackContract}.{_callbackMethod}, Gas: {GasForResponseDecimal})";
        }
    }
}
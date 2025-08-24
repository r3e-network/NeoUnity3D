using System;
using UnityEngine;
using Neo.Unity.SDK.Types;

namespace Neo.Unity.SDK.Core
{
    /// <summary>
    /// Neo Unity SDK configuration settings as a ScriptableObject for Unity Inspector integration.
    /// Configure blockchain connection parameters, network settings, and SDK behavior.
    /// </summary>
    [CreateAssetMenu(fileName = "NeoUnityConfig", menuName = "Neo Unity SDK/Configuration", order = 1)]
    public class NeoUnityConfig : ScriptableObject
    {
        #region Constants
        
        /// <summary>Default block time interval in milliseconds (15 seconds)</summary>
        public const int DEFAULT_BLOCK_TIME = 15_000;
        
        /// <summary>Default Neo address version byte</summary>
        public const byte DEFAULT_ADDRESS_VERSION = 0x35;
        
        /// <summary>Maximum valid until block increment base time in milliseconds (24 hours)</summary>
        public const int MAX_VALID_UNTIL_BLOCK_INCREMENT_BASE = 86_400_000;
        
        /// <summary>Mainnet NeoNameService contract hash</summary>
        public static readonly Hash160 MAINNET_NNS_CONTRACT_HASH = new Hash160("50ac1c37690cc2cfc594472833cf57505d5f46de");
        
        #endregion
        
        #region Inspector Fields
        
        [Header("Network Configuration")]
        [SerializeField] 
        [Tooltip("Neo RPC node endpoint URL")]
        private string nodeUrl = "https://mainnet1.neo.coz.io:443";
        
        [SerializeField] 
        [Tooltip("Network magic number (will be auto-fetched if not set)")]
        private int networkMagic = 0;
        
        [SerializeField] 
        [Tooltip("Block interval in milliseconds")]
        private int blockInterval = DEFAULT_BLOCK_TIME;
        
        [SerializeField] 
        [Tooltip("Polling interval for blockchain monitoring in milliseconds")]
        private int pollingInterval = DEFAULT_BLOCK_TIME;
        
        [SerializeField] 
        [Tooltip("Maximum valid until block increment in milliseconds")]
        private int maxValidUntilBlockIncrement = MAX_VALID_UNTIL_BLOCK_INCREMENT_BASE / DEFAULT_BLOCK_TIME;
        
        [Header("Blockchain Settings")]
        [SerializeField] 
        [Tooltip("Neo address version byte")]
        private byte addressVersion = DEFAULT_ADDRESS_VERSION;
        
        [SerializeField] 
        [Tooltip("Allow transmission of scripts that result in VM fault")]
        private bool allowTransmissionOnFault = false;
        
        [SerializeField] 
        [Tooltip("Neo Name Service resolver contract hash")]
        private string nnsResolver = "50ac1c37690cc2cfc594472833cf57505d5f46de";
        
        [Header("Unity Integration")]
        [SerializeField] 
        [Tooltip("Enable debug logging")]
        private bool enableDebugLogging = true;
        
        [SerializeField] 
        [Tooltip("Request timeout in seconds")]
        private float requestTimeout = 30f;
        
        [SerializeField] 
        [Tooltip("Maximum concurrent requests")]
        private int maxConcurrentRequests = 10;
        
        #endregion
        
        #region Properties
        
        /// <summary>Neo RPC node endpoint URL</summary>
        public string NodeUrl 
        { 
            get => nodeUrl; 
            set => nodeUrl = value; 
        }
        
        /// <summary>Network magic number (auto-fetched if not explicitly set)</summary>
        public int? NetworkMagic 
        { 
            get => networkMagic == 0 ? null : networkMagic; 
            set => networkMagic = value ?? 0; 
        }
        
        /// <summary>Block interval in milliseconds</summary>
        public int BlockInterval 
        { 
            get => blockInterval; 
            set => blockInterval = value; 
        }
        
        /// <summary>Polling interval for blockchain monitoring</summary>
        public int PollingInterval 
        { 
            get => pollingInterval; 
            set => pollingInterval = value; 
        }
        
        /// <summary>Maximum valid until block increment</summary>
        public int MaxValidUntilBlockIncrement 
        { 
            get => maxValidUntilBlockIncrement; 
            set => maxValidUntilBlockIncrement = value; 
        }
        
        /// <summary>Neo address version byte</summary>
        public byte AddressVersion 
        { 
            get => addressVersion; 
            set => addressVersion = value; 
        }
        
        /// <summary>Allow transmission of scripts that result in VM fault</summary>
        public bool AllowTransmissionOnFault 
        { 
            get => allowTransmissionOnFault; 
            set => allowTransmissionOnFault = value; 
        }
        
        /// <summary>Neo Name Service resolver contract hash</summary>
        public Hash160 NNSResolver 
        { 
            get => new Hash160(nnsResolver); 
            set => nnsResolver = value.ToString(); 
        }
        
        /// <summary>Enable debug logging</summary>
        public bool EnableDebugLogging 
        { 
            get => enableDebugLogging; 
            set => enableDebugLogging = value; 
        }
        
        /// <summary>Request timeout in seconds</summary>
        public float RequestTimeout 
        { 
            get => requestTimeout; 
            set => requestTimeout = value; 
        }
        
        /// <summary>Maximum concurrent requests</summary>
        public int MaxConcurrentRequests 
        { 
            get => maxConcurrentRequests; 
            set => maxConcurrentRequests = value; 
        }
        
        #endregion
        
        #region Fluent Configuration Methods
        
        /// <summary>
        /// Sets the polling interval for blockchain monitoring.
        /// </summary>
        /// <param name="pollingInterval">Polling interval in milliseconds</param>
        /// <returns>This configuration instance for method chaining</returns>
        public NeoUnityConfig SetPollingInterval(int pollingInterval)
        {
            this.pollingInterval = pollingInterval;
            return this;
        }
        
        /// <summary>
        /// Sets the network magic number.
        /// </summary>
        /// <param name="magic">Network magic number (must fit in 32-bit unsigned integer)</param>
        /// <returns>This configuration instance for method chaining</returns>
        /// <exception cref="ArgumentException">If magic number is invalid</exception>
        public NeoUnityConfig SetNetworkMagic(int magic)
        {
            if (magic < 0 || magic > 0xFFFFFFFF)
            {
                throw new ArgumentException("Network magic number must fit into a 32-bit unsigned integer, i.e., it must be positive and not greater than 0xFFFFFFFF.");
            }
            this.networkMagic = magic;
            return this;
        }
        
        /// <summary>
        /// Sets the block interval.
        /// </summary>
        /// <param name="blockInterval">Block interval in milliseconds</param>
        /// <returns>This configuration instance for method chaining</returns>
        public NeoUnityConfig SetBlockInterval(int blockInterval)
        {
            this.blockInterval = blockInterval;
            return this;
        }
        
        /// <summary>
        /// Sets the maximum valid until block increment.
        /// </summary>
        /// <param name="maxValidUntilBlockIncrement">Maximum valid until block increment in milliseconds</param>
        /// <returns>This configuration instance for method chaining</returns>
        public NeoUnityConfig SetMaxValidUntilBlockIncrement(int maxValidUntilBlockIncrement)
        {
            this.maxValidUntilBlockIncrement = maxValidUntilBlockIncrement;
            return this;
        }
        
        /// <summary>
        /// Sets the Neo Name Service resolver contract hash.
        /// </summary>
        /// <param name="nnsResolver">NNS resolver contract hash</param>
        /// <returns>This configuration instance for method chaining</returns>
        public NeoUnityConfig SetNNSResolver(Hash160 nnsResolver)
        {
            this.nnsResolver = nnsResolver.ToString();
            return this;
        }
        
        /// <summary>
        /// Allow transmission of scripts that result in VM fault.
        /// </summary>
        /// <returns>This configuration instance for method chaining</returns>
        public NeoUnityConfig AllowTransmissionOnFault()
        {
            this.allowTransmissionOnFault = true;
            return this;
        }
        
        /// <summary>
        /// Prevent transmission of scripts that result in VM fault (default behavior).
        /// </summary>
        /// <returns>This configuration instance for method chaining</returns>
        public NeoUnityConfig PreventTransmissionOnFault()
        {
            this.allowTransmissionOnFault = false;
            return this;
        }
        
        #endregion
        
        #region Unity Lifecycle
        
        private void OnValidate()
        {
            // Validate configuration values in Unity Inspector
            blockInterval = Mathf.Max(1000, blockInterval); // Minimum 1 second
            pollingInterval = Mathf.Max(1000, pollingInterval); // Minimum 1 second
            requestTimeout = Mathf.Clamp(requestTimeout, 1f, 300f); // 1-300 seconds
            maxConcurrentRequests = Mathf.Clamp(maxConcurrentRequests, 1, 50); // 1-50 requests
            
            // Validate URL format
            if (!string.IsNullOrEmpty(nodeUrl) && !nodeUrl.StartsWith("http"))
            {
                Debug.LogWarning($"[NeoUnityConfig] Invalid node URL format: {nodeUrl}. Should start with 'http' or 'https'.");
            }
            
            // Validate NNS resolver hash format
            if (!string.IsNullOrEmpty(nnsResolver) && nnsResolver.Length != 40)
            {
                Debug.LogWarning($"[NeoUnityConfig] Invalid NNS resolver hash format: {nnsResolver}. Should be 40 characters long.");
            }
        }
        
        #endregion
        
        #region Static Utilities
        
        /// <summary>
        /// Creates a default configuration for Neo mainnet.
        /// </summary>
        /// <returns>Default mainnet configuration</returns>
        public static NeoUnityConfig CreateMainnetConfig()
        {
            var config = CreateInstance<NeoUnityConfig>();
            config.nodeUrl = "https://mainnet1.neo.coz.io:443";
            config.networkMagic = 860833102; // Neo N3 mainnet magic
            config.nnsResolver = MAINNET_NNS_CONTRACT_HASH.ToString();
            config.name = "Neo Mainnet Config";
            return config;
        }
        
        /// <summary>
        /// Creates a default configuration for Neo testnet.
        /// </summary>
        /// <returns>Default testnet configuration</returns>
        public static NeoUnityConfig CreateTestnetConfig()
        {
            var config = CreateInstance<NeoUnityConfig>();
            config.nodeUrl = "https://testnet1.neo.coz.io:443";
            config.networkMagic = 894710606; // Neo N3 testnet magic
            config.nnsResolver = "0xa46c1e9f936d2967adf5d8ee5c3e2b4b5a7fff3a"; // Testnet NNS
            config.name = "Neo Testnet Config";
            return config;
        }
        
        #endregion
    }
}
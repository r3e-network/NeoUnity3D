using System;
using UnityEngine;
using Newtonsoft.Json;

namespace Neo.Unity.SDK.Protocol.Response
{
    /// <summary>
    /// Response for getting the version information of a Neo node.
    /// Contains node version, protocol parameters, and network configuration.
    /// </summary>
    [System.Serializable]
    public class NeoGetVersionResponse : NeoResponse<NeoVersion>
    {
        /// <summary>
        /// Gets the version information from the response.
        /// </summary>
        /// <returns>Version information or null if response failed</returns>
        public NeoVersion GetVersion()
        {
            return IsSuccess ? Result : null;
        }
        
        /// <summary>
        /// Gets the version information or throws if the response failed.
        /// </summary>
        /// <returns>Version information</returns>
        /// <exception cref="NeoRpcException">If the response contains an error</exception>
        public NeoVersion GetVersionOrThrow()
        {
            return GetResult();
        }
    }
    
    /// <summary>
    /// Represents version and configuration information of a Neo node.
    /// Contains network ports, protocol parameters, and node identification.
    /// </summary>
    [System.Serializable]
    public class NeoVersion
    {
        #region Properties
        
        /// <summary>The TCP port the node listens on</summary>
        [JsonProperty("tcpport")]
        public int? TcpPort { get; set; }
        
        /// <summary>The WebSocket port the node listens on</summary>
        [JsonProperty("wsport")]
        public int? WsPort { get; set; }
        
        /// <summary>A random nonce for this node instance</summary>
        [JsonProperty("nonce")]
        public long Nonce { get; set; }
        
        /// <summary>The user agent string of this node</summary>
        [JsonProperty("useragent")]
        public string UserAgent { get; set; }
        
        /// <summary>Protocol configuration parameters</summary>
        [JsonProperty("protocol")]
        public NeoProtocol Protocol { get; set; }
        
        #endregion
        
        #region Constructors
        
        /// <summary>
        /// Default constructor for JSON deserialization.
        /// </summary>
        public NeoVersion()
        {
        }
        
        /// <summary>
        /// Creates a new Neo version.
        /// </summary>
        /// <param name="tcpPort">TCP port</param>
        /// <param name="wsPort">WebSocket port</param>
        /// <param name="nonce">Node nonce</param>
        /// <param name="userAgent">User agent</param>
        /// <param name="protocol">Protocol parameters</param>
        public NeoVersion(int? tcpPort, int? wsPort, long nonce, string userAgent, NeoProtocol protocol)
        {
            TcpPort = tcpPort;
            WsPort = wsPort;
            Nonce = nonce;
            UserAgent = userAgent;
            Protocol = protocol;
        }
        
        #endregion
        
        #region Properties (Computed)
        
        /// <summary>Whether this node has a TCP port configured</summary>
        [JsonIgnore]
        public bool HasTcpPort => TcpPort.HasValue && TcpPort.Value > 0;
        
        /// <summary>Whether this node has a WebSocket port configured</summary>
        [JsonIgnore]
        public bool HasWsPort => WsPort.HasValue && WsPort.Value > 0;
        
        /// <summary>Whether this node has protocol information</summary>
        [JsonIgnore]
        public bool HasProtocol => Protocol != null;
        
        /// <summary>Whether this node supports WebSocket connections</summary>
        [JsonIgnore]
        public bool SupportsWebSocket => HasWsPort;
        
        /// <summary>The network this node is connected to</summary>
        [JsonIgnore]
        public long? Network => Protocol?.Network;
        
        /// <summary>Whether this is a MainNet node</summary>
        [JsonIgnore]
        public bool IsMainNet => Network == 860833102; // Neo MainNet magic number
        
        /// <summary>Whether this is a TestNet node</summary>
        [JsonIgnore]
        public bool IsTestNet => Network == 894710606; // Neo TestNet magic number
        
        #endregion
        
        #region Node Information Methods
        
        /// <summary>
        /// Gets the node software name from the user agent.
        /// </summary>
        /// <returns>Node software name</returns>
        public string GetNodeSoftware()
        {
            if (string.IsNullOrEmpty(UserAgent))
                return "Unknown";
            
            // Extract software name from user agent (e.g., "NEO-CLI/3.5.0")
            var parts = UserAgent.Split('/');
            return parts.Length > 0 ? parts[0] : UserAgent;
        }
        
        /// <summary>
        /// Gets the node software version from the user agent.
        /// </summary>
        /// <returns>Node software version</returns>
        public string GetNodeVersion()
        {
            if (string.IsNullOrEmpty(UserAgent))
                return "Unknown";
            
            // Extract version from user agent (e.g., "NEO-CLI/3.5.0")
            var parts = UserAgent.Split('/');
            return parts.Length > 1 ? parts[1] : "Unknown";
        }
        
        /// <summary>
        /// Gets the network name.
        /// </summary>
        /// <returns>Network name</returns>
        public string GetNetworkName()
        {
            if (!HasProtocol)
                return "Unknown";
            
            return Protocol.Network switch
            {
                860833102 => "MainNet",
                894710606 => "TestNet",
                1953787457 => "N3 MainNet",
                1951352142 => "N3 TestNet",
                _ => $"Custom ({Protocol.Network})"
            };
        }
        
        /// <summary>
        /// Checks if this node is compatible with a specific network.
        /// </summary>
        /// <param name="expectedNetwork">The expected network magic number</param>
        /// <returns>True if compatible</returns>
        public bool IsCompatibleWithNetwork(long expectedNetwork)
        {
            return Network == expectedNetwork;
        }
        
        #endregion
        
        #region Connection Methods
        
        /// <summary>
        /// Gets the TCP connection endpoint.
        /// </summary>
        /// <param name="host">The host address</param>
        /// <returns>TCP endpoint string</returns>
        public string GetTcpEndpoint(string host = "localhost")
        {
            if (!HasTcpPort)
                return null;
            
            return $"{host}:{TcpPort}";
        }
        
        /// <summary>
        /// Gets the WebSocket connection endpoint.
        /// </summary>
        /// <param name="host">The host address</param>
        /// <param name="secure">Whether to use secure WebSocket (wss)</param>
        /// <returns>WebSocket endpoint URL</returns>
        public string GetWebSocketEndpoint(string host = "localhost", bool secure = false)
        {
            if (!HasWsPort)
                return null;
            
            var protocol = secure ? "wss" : "ws";
            return $"{protocol}://{host}:{WsPort}";
        }
        
        /// <summary>
        /// Gets the HTTP RPC endpoint.
        /// </summary>
        /// <param name="host">The host address</param>
        /// <param name="secure">Whether to use HTTPS</param>
        /// <returns>HTTP RPC endpoint URL</returns>
        public string GetHttpEndpoint(string host = "localhost", bool secure = false)
        {
            // Neo nodes typically use standard HTTP ports for RPC
            var port = secure ? 443 : 80;
            var protocol = secure ? "https" : "http";
            return $"{protocol}://{host}:{port}";
        }
        
        #endregion
        
        #region Validation Methods
        
        /// <summary>
        /// Validates this version information.
        /// </summary>
        /// <exception cref="InvalidOperationException">If validation fails</exception>
        public void Validate()
        {
            if (string.IsNullOrEmpty(UserAgent))
                throw new InvalidOperationException("Node user agent cannot be null or empty.");
            
            if (TcpPort.HasValue && (TcpPort.Value <= 0 || TcpPort.Value > 65535))
                throw new InvalidOperationException($"Invalid TCP port: {TcpPort}");
            
            if (WsPort.HasValue && (WsPort.Value <= 0 || WsPort.Value > 65535))
                throw new InvalidOperationException($"Invalid WebSocket port: {WsPort}");
            
            Protocol?.Validate();
        }
        
        #endregion
        
        #region String Representation
        
        /// <summary>
        /// Returns a detailed string representation of this version.
        /// </summary>
        /// <returns>Detailed string representation</returns>
        public string ToDetailedString()
        {
            var result = $"Neo Node Version:\n";
            result += $"  Software: {GetNodeSoftware()}\n";
            result += $"  Version: {GetNodeVersion()}\n";
            result += $"  Network: {GetNetworkName()}\n";
            result += $"  Nonce: {Nonce}\n";
            
            if (HasTcpPort)
            {
                result += $"  TCP Port: {TcpPort}\n";
            }
            
            if (HasWsPort)
            {
                result += $"  WebSocket Port: {WsPort}\n";
            }
            
            if (HasProtocol)
            {
                result += $"  Block Time: {Protocol.MsPerBlock}ms\n";
                result += $"  Max TPS: ~{Protocol.MaxTransactionsPerBlock * 1000 / Protocol.MsPerBlock}\n";
            }
            
            return result;
        }
        
        /// <summary>
        /// Returns a string representation of this version.
        /// </summary>
        /// <returns>String representation</returns>
        public override string ToString()
        {
            var software = GetNodeSoftware();
            var version = GetNodeVersion();
            var network = GetNetworkName();
            return $"NeoVersion({software} {version} on {network})";
        }
        
        #endregion
    }
    
    /// <summary>
    /// Represents Neo protocol configuration parameters.
    /// Contains network constants and protocol limits.
    /// </summary>
    [System.Serializable]
    public class NeoProtocol
    {
        #region Properties
        
        /// <summary>The network magic number</summary>
        [JsonProperty("network")]
        public long Network { get; set; }
        
        /// <summary>The number of consensus validators</summary>
        [JsonProperty("validatorscount")]
        public int? ValidatorsCount { get; set; }
        
        /// <summary>Target milliseconds per block</summary>
        [JsonProperty("msperblock")]
        public int MsPerBlock { get; set; }
        
        /// <summary>Maximum valid until block increment</summary>
        [JsonProperty("maxvaliduntilblockincrement")]
        public int MaxValidUntilBlockIncrement { get; set; }
        
        /// <summary>Maximum number of traceable blocks</summary>
        [JsonProperty("maxtraceableblocks")]
        public int MaxTraceableBlocks { get; set; }
        
        /// <summary>Address version byte</summary>
        [JsonProperty("addressversion")]
        public int AddressVersion { get; set; }
        
        /// <summary>Maximum transactions per block</summary>
        [JsonProperty("maxtransactionsperblock")]
        public int MaxTransactionsPerBlock { get; set; }
        
        /// <summary>Maximum transactions in memory pool</summary>
        [JsonProperty("memorypoolmaxtransactions")]
        public int MemoryPoolMaxTransactions { get; set; }
        
        /// <summary>Initial GAS distribution amount</summary>
        [JsonProperty("initialgasdistribution")]
        public long InitialGasDistribution { get; set; }
        
        #endregion
        
        #region Constructors
        
        /// <summary>
        /// Default constructor for JSON deserialization.
        /// </summary>
        public NeoProtocol()
        {
        }
        
        /// <summary>
        /// Creates new protocol parameters.
        /// </summary>
        /// <param name="network">Network magic</param>
        /// <param name="validatorsCount">Validators count</param>
        /// <param name="msPerBlock">Milliseconds per block</param>
        /// <param name="maxValidUntilBlockIncrement">Max valid until block increment</param>
        /// <param name="maxTraceableBlocks">Max traceable blocks</param>
        /// <param name="addressVersion">Address version</param>
        /// <param name="maxTransactionsPerBlock">Max transactions per block</param>
        /// <param name="memoryPoolMaxTransactions">Max memory pool transactions</param>
        /// <param name="initialGasDistribution">Initial GAS distribution</param>
        public NeoProtocol(long network, int? validatorsCount, int msPerBlock, int maxValidUntilBlockIncrement,
                          int maxTraceableBlocks, int addressVersion, int maxTransactionsPerBlock,
                          int memoryPoolMaxTransactions, long initialGasDistribution)
        {
            Network = network;
            ValidatorsCount = validatorsCount;
            MsPerBlock = msPerBlock;
            MaxValidUntilBlockIncrement = maxValidUntilBlockIncrement;
            MaxTraceableBlocks = maxTraceableBlocks;
            AddressVersion = addressVersion;
            MaxTransactionsPerBlock = maxTransactionsPerBlock;
            MemoryPoolMaxTransactions = memoryPoolMaxTransactions;
            InitialGasDistribution = initialGasDistribution;
        }
        
        #endregion
        
        #region Properties (Computed)
        
        /// <summary>Target blocks per second</summary>
        [JsonIgnore]
        public double BlocksPerSecond => 1000.0 / MsPerBlock;
        
        /// <summary>Target seconds per block</summary>
        [JsonIgnore]
        public double SecondsPerBlock => MsPerBlock / 1000.0;
        
        /// <summary>Theoretical maximum transactions per second</summary>
        [JsonIgnore]
        public double MaxTransactionsPerSecond => MaxTransactionsPerBlock * BlocksPerSecond;
        
        /// <summary>Maximum transaction lifetime in blocks</summary>
        [JsonIgnore]
        public int MaxTransactionLifetime => MaxValidUntilBlockIncrement;
        
        /// <summary>Maximum transaction lifetime in seconds</summary>
        [JsonIgnore]
        public double MaxTransactionLifetimeSeconds => MaxTransactionLifetime * SecondsPerBlock;
        
        /// <summary>Whether this protocol has validator information</summary>
        [JsonIgnore]
        public bool HasValidatorInfo => ValidatorsCount.HasValue;
        
        #endregion
        
        #region Network Methods
        
        /// <summary>
        /// Gets the network type based on the magic number.
        /// </summary>
        /// <returns>Network type</returns>
        public NetworkType GetNetworkType()
        {
            return Network switch
            {
                860833102 => NetworkType.MainNet,
                894710606 => NetworkType.TestNet,
                1953787457 => NetworkType.N3MainNet,
                1951352142 => NetworkType.N3TestNet,
                _ => NetworkType.Custom
            };
        }
        
        /// <summary>
        /// Checks if this is a production network.
        /// </summary>
        /// <returns>True if this is MainNet</returns>
        public bool IsProductionNetwork()
        {
            var networkType = GetNetworkType();
            return networkType == NetworkType.MainNet || networkType == NetworkType.N3MainNet;
        }
        
        /// <summary>
        /// Checks if this is a test network.
        /// </summary>
        /// <returns>True if this is TestNet</returns>
        public bool IsTestNetwork()
        {
            var networkType = GetNetworkType();
            return networkType == NetworkType.TestNet || networkType == NetworkType.N3TestNet;
        }
        
        #endregion
        
        #region Performance Methods
        
        /// <summary>
        /// Calculates the estimated confirmation time for a number of confirmations.
        /// </summary>
        /// <param name="confirmations">Number of confirmations</param>
        /// <returns>Estimated time in seconds</returns>
        public double EstimateConfirmationTime(int confirmations)
        {
            return confirmations * SecondsPerBlock;
        }
        
        /// <summary>
        /// Calculates the number of blocks in a time period.
        /// </summary>
        /// <param name="timeSpan">The time period</param>
        /// <returns>Number of blocks</returns>
        public int CalculateBlocksInTimeSpan(TimeSpan timeSpan)
        {
            return (int)(timeSpan.TotalSeconds / SecondsPerBlock);
        }
        
        /// <summary>
        /// Gets performance statistics.
        /// </summary>
        /// <returns>Performance statistics</returns>
        public ProtocolPerformance GetPerformance()
        {
            return new ProtocolPerformance
            {
                BlockTime = SecondsPerBlock,
                MaxTPS = MaxTransactionsPerSecond,
                MaxBlockSize = MaxTransactionsPerBlock,
                MemPoolSize = MemoryPoolMaxTransactions,
                TransactionLifetime = MaxTransactionLifetimeSeconds,
                TraceableHistory = MaxTraceableBlocks
            };
        }
        
        #endregion
        
        #region Validation Methods
        
        /// <summary>
        /// Validates this protocol configuration.
        /// </summary>
        /// <exception cref="InvalidOperationException">If validation fails</exception>
        public void Validate()
        {
            if (MsPerBlock <= 0)
                throw new InvalidOperationException("Milliseconds per block must be positive.");
            
            if (MaxTransactionsPerBlock <= 0)
                throw new InvalidOperationException("Max transactions per block must be positive.");
            
            if (MemoryPoolMaxTransactions <= 0)
                throw new InvalidOperationException("Memory pool max transactions must be positive.");
            
            if (MaxValidUntilBlockIncrement <= 0)
                throw new InvalidOperationException("Max valid until block increment must be positive.");
            
            if (MaxTraceableBlocks <= 0)
                throw new InvalidOperationException("Max traceable blocks must be positive.");
            
            if (ValidatorsCount.HasValue && ValidatorsCount.Value <= 0)
                throw new InvalidOperationException("Validators count must be positive.");
        }
        
        #endregion
        
        #region String Representation
        
        /// <summary>
        /// Returns a string representation of this protocol.
        /// </summary>
        /// <returns>String representation</returns>
        public override string ToString()
        {
            return $"NeoProtocol(Network: {Network}, BlockTime: {MsPerBlock}ms, MaxTPS: ~{MaxTransactionsPerSecond:F1})";
        }
        
        #endregion
    }
    
    /// <summary>
    /// Enumeration of Neo network types.
    /// </summary>
    public enum NetworkType
    {
        /// <summary>Neo Legacy MainNet</summary>
        MainNet,
        
        /// <summary>Neo Legacy TestNet</summary>
        TestNet,
        
        /// <summary>Neo N3 MainNet</summary>
        N3MainNet,
        
        /// <summary>Neo N3 TestNet</summary>
        N3TestNet,
        
        /// <summary>Custom or private network</summary>
        Custom
    }
    
    /// <summary>
    /// Contains performance statistics for Neo protocol.
    /// </summary>
    [System.Serializable]
    public class ProtocolPerformance
    {
        /// <summary>Block time in seconds</summary>
        public double BlockTime { get; set; }
        
        /// <summary>Maximum transactions per second</summary>
        public double MaxTPS { get; set; }
        
        /// <summary>Maximum transactions per block</summary>
        public int MaxBlockSize { get; set; }
        
        /// <summary>Memory pool capacity</summary>
        public int MemPoolSize { get; set; }
        
        /// <summary>Transaction lifetime in seconds</summary>
        public double TransactionLifetime { get; set; }
        
        /// <summary>Number of traceable blocks</summary>
        public int TraceableHistory { get; set; }
        
        /// <summary>
        /// Returns a string representation of this performance data.
        /// </summary>
        /// <returns>String representation</returns>
        public override string ToString()
        {
            return $"ProtocolPerformance(BlockTime: {BlockTime}s, MaxTPS: {MaxTPS:F1}, TxLifetime: {TransactionLifetime}s)";
        }
    }
}
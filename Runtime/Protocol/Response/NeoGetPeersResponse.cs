using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Newtonsoft.Json;

namespace Neo.Unity.SDK.Protocol.Response
{
    /// <summary>
    /// Response for getting the peer information of a Neo node.
    /// Contains lists of connected, unconnected, and bad peers.
    /// </summary>
    [System.Serializable]
    public class NeoGetPeersResponse : NeoResponse<Peers>
    {
        /// <summary>
        /// Gets the peer information from the response.
        /// </summary>
        /// <returns>Peer information or null if response failed</returns>
        public Peers GetPeers()
        {
            return IsSuccess ? Result : null;
        }
        
        /// <summary>
        /// Gets the peer information or throws if the response failed.
        /// </summary>
        /// <returns>Peer information</returns>
        /// <exception cref="NeoRpcException">If the response contains an error</exception>
        public Peers GetPeersOrThrow()
        {
            return GetResult();
        }
    }
    
    /// <summary>
    /// Represents the peer information of a Neo node.
    /// Contains categorized lists of network peers and their connection status.
    /// </summary>
    [System.Serializable]
    public class Peers
    {
        #region Properties
        
        /// <summary>List of currently connected peers</summary>
        [JsonProperty("connected")]
        public List<AddressEntry> Connected { get; set; }
        
        /// <summary>List of bad/blacklisted peers</summary>
        [JsonProperty("bad")]
        public List<AddressEntry> Bad { get; set; }
        
        /// <summary>List of known but unconnected peers</summary>
        [JsonProperty("unconnected")]
        public List<AddressEntry> Unconnected { get; set; }
        
        #endregion
        
        #region Constructors
        
        /// <summary>
        /// Default constructor for JSON deserialization.
        /// </summary>
        public Peers()
        {
            Connected = new List<AddressEntry>();
            Bad = new List<AddressEntry>();
            Unconnected = new List<AddressEntry>();
        }
        
        /// <summary>
        /// Creates new peer information.
        /// </summary>
        /// <param name="connected">Connected peers</param>
        /// <param name="bad">Bad peers</param>
        /// <param name="unconnected">Unconnected peers</param>
        public Peers(List<AddressEntry> connected, List<AddressEntry> bad, List<AddressEntry> unconnected)
        {
            Connected = connected ?? new List<AddressEntry>();
            Bad = bad ?? new List<AddressEntry>();
            Unconnected = unconnected ?? new List<AddressEntry>();
        }
        
        #endregion
        
        #region Properties (Computed)
        
        /// <summary>Whether there are any connected peers</summary>
        [JsonIgnore]
        public bool HasConnectedPeers => Connected != null && Connected.Count > 0;
        
        /// <summary>Whether there are any bad peers</summary>
        [JsonIgnore]
        public bool HasBadPeers => Bad != null && Bad.Count > 0;
        
        /// <summary>Whether there are any unconnected peers</summary>
        [JsonIgnore]
        public bool HasUnconnectedPeers => Unconnected != null && Unconnected.Count > 0;
        
        /// <summary>Number of connected peers</summary>
        [JsonIgnore]
        public int ConnectedCount => Connected?.Count ?? 0;
        
        /// <summary>Number of bad peers</summary>
        [JsonIgnore]
        public int BadCount => Bad?.Count ?? 0;
        
        /// <summary>Number of unconnected peers</summary>
        [JsonIgnore]
        public int UnconnectedCount => Unconnected?.Count ?? 0;
        
        /// <summary>Total number of known peers</summary>
        [JsonIgnore]
        public int TotalKnownPeers => ConnectedCount + BadCount + UnconnectedCount;
        
        /// <summary>Number of healthy peers (connected + unconnected)</summary>
        [JsonIgnore]
        public int HealthyPeersCount => ConnectedCount + UnconnectedCount;
        
        /// <summary>Connection success rate as a percentage</summary>
        [JsonIgnore]
        public double ConnectionSuccessRate
        {
            get
            {
                var totalGoodPeers = ConnectedCount + UnconnectedCount;
                return TotalKnownPeers > 0 ? (double)totalGoodPeers / TotalKnownPeers * 100 : 0;
            }
        }
        
        #endregion
        
        #region Peer Operations
        
        /// <summary>
        /// Gets all peers (connected, bad, and unconnected) combined.
        /// </summary>
        /// <returns>List of all peers</returns>
        public List<AddressEntry> GetAllPeers()
        {
            var allPeers = new List<AddressEntry>();
            
            if (HasConnectedPeers)
                allPeers.AddRange(Connected);
            
            if (HasBadPeers)
                allPeers.AddRange(Bad);
            
            if (HasUnconnectedPeers)
                allPeers.AddRange(Unconnected);
            
            return allPeers;
        }
        
        /// <summary>
        /// Gets all healthy peers (connected and unconnected).
        /// </summary>
        /// <returns>List of healthy peers</returns>
        public List<AddressEntry> GetHealthyPeers()
        {
            var healthyPeers = new List<AddressEntry>();
            
            if (HasConnectedPeers)
                healthyPeers.AddRange(Connected);
            
            if (HasUnconnectedPeers)
                healthyPeers.AddRange(Unconnected);
            
            return healthyPeers;
        }
        
        /// <summary>
        /// Finds a peer by address and port.
        /// </summary>
        /// <param name="address">The IP address</param>
        /// <param name="port">The port number</param>
        /// <returns>The peer or null if not found</returns>
        public AddressEntry FindPeer(string address, int port)
        {
            return GetAllPeers().FirstOrDefault(p => p.Address == address && p.Port == port);
        }
        
        /// <summary>
        /// Finds a peer by address (any port).
        /// </summary>
        /// <param name="address">The IP address</param>
        /// <returns>The first matching peer or null if not found</returns>
        public AddressEntry FindPeerByAddress(string address)
        {
            return GetAllPeers().FirstOrDefault(p => p.Address == address);
        }
        
        /// <summary>
        /// Checks if a specific peer is connected.
        /// </summary>
        /// <param name="address">The IP address</param>
        /// <param name="port">The port number</param>
        /// <returns>True if the peer is connected</returns>
        public bool IsPeerConnected(string address, int port)
        {
            return HasConnectedPeers && Connected.Any(p => p.Address == address && p.Port == port);
        }
        
        /// <summary>
        /// Checks if a specific peer is blacklisted.
        /// </summary>
        /// <param name="address">The IP address</param>
        /// <param name="port">The port number</param>
        /// <returns>True if the peer is blacklisted</returns>
        public bool IsPeerBad(string address, int port)
        {
            return HasBadPeers && Bad.Any(p => p.Address == address && p.Port == port);
        }
        
        /// <summary>
        /// Gets all unique IP addresses.
        /// </summary>
        /// <returns>List of unique IP addresses</returns>
        public List<string> GetUniqueAddresses()
        {
            return GetAllPeers().Select(p => p.Address).Distinct().ToList();
        }
        
        /// <summary>
        /// Gets all unique ports.
        /// </summary>
        /// <returns>List of unique ports</returns>
        public List<int> GetUniquePorts()
        {
            return GetAllPeers().Select(p => p.Port).Distinct().OrderBy(p => p).ToList();
        }
        
        /// <summary>
        /// Groups peers by port.
        /// </summary>
        /// <returns>Dictionary of port to peers</returns>
        public Dictionary<int, List<AddressEntry>> GroupPeersByPort()
        {
            return GetAllPeers().GroupBy(p => p.Port)
                               .ToDictionary(g => g.Key, g => g.ToList());
        }
        
        /// <summary>
        /// Gets peers sorted by address.
        /// </summary>
        /// <returns>Peers sorted by IP address</returns>
        public List<AddressEntry> GetPeersSortedByAddress()
        {
            return GetAllPeers().OrderBy(p => p.Address).ThenBy(p => p.Port).ToList();
        }
        
        #endregion
        
        #region Network Analysis
        
        /// <summary>
        /// Analyzes the peer distribution by IP address range.
        /// </summary>
        /// <returns>Dictionary of network range to count</returns>
        public Dictionary<string, int> AnalyzePeerDistribution()
        {
            var distribution = new Dictionary<string, int>();
            
            foreach (var peer in GetAllPeers())
            {
                var networkRange = GetNetworkRange(peer.Address);
                if (distribution.ContainsKey(networkRange))
                {
                    distribution[networkRange]++;
                }
                else
                {
                    distribution[networkRange] = 1;
                }
            }
            
            return distribution;
        }
        
        /// <summary>
        /// Gets network health statistics.
        /// </summary>
        /// <returns>Network health information</returns>
        public NetworkHealth GetNetworkHealth()
        {
            return new NetworkHealth
            {
                ConnectedPeers = ConnectedCount,
                TotalKnownPeers = TotalKnownPeers,
                BadPeers = BadCount,
                ConnectionSuccessRate = ConnectionSuccessRate,
                UniqueAddresses = GetUniqueAddresses().Count,
                UniquePorts = GetUniquePorts().Count,
                IsHealthy = ConnectedCount > 0 && ConnectionSuccessRate > 50
            };
        }
        
        /// <summary>
        /// Gets the network range (first 3 octets) for an IP address.
        /// </summary>
        /// <param name="ipAddress">The IP address</param>
        /// <returns>Network range string</returns>
        private string GetNetworkRange(string ipAddress)
        {
            if (string.IsNullOrEmpty(ipAddress))
                return "Unknown";
            
            var parts = ipAddress.Split('.');
            if (parts.Length >= 3)
            {
                return $"{parts[0]}.{parts[1]}.{parts[2]}.x";
            }
            
            return ipAddress;
        }
        
        #endregion
        
        #region Validation Methods
        
        /// <summary>
        /// Validates this peer information.
        /// </summary>
        /// <exception cref="InvalidOperationException">If validation fails</exception>
        public void Validate()
        {
            if (Connected != null)
            {
                foreach (var peer in Connected)
                {
                    peer.Validate();
                }
            }
            
            if (Bad != null)
            {
                foreach (var peer in Bad)
                {
                    peer.Validate();
                }
            }
            
            if (Unconnected != null)
            {
                foreach (var peer in Unconnected)
                {
                    peer.Validate();
                }
            }
        }
        
        #endregion
        
        #region String Representation
        
        /// <summary>
        /// Returns a detailed string representation of peer information.
        /// </summary>
        /// <returns>Detailed string representation</returns>
        public string ToDetailedString()
        {
            var health = GetNetworkHealth();
            var result = $"Neo Node Peers:\n";
            result += $"  Connected: {ConnectedCount}\n";
            result += $"  Unconnected: {UnconnectedCount}\n";
            result += $"  Bad: {BadCount}\n";
            result += $"  Total Known: {TotalKnownPeers}\n";
            result += $"  Success Rate: {ConnectionSuccessRate:F1}%\n";
            result += $"  Health: {(health.IsHealthy ? "Healthy" : "Poor")}\n";
            result += $"  Unique IPs: {health.UniqueAddresses}\n";
            result += $"  Unique Ports: {health.UniquePorts}\n";
            
            if (HasConnectedPeers)
            {
                result += $"  Sample Connected: {Connected.Take(3).Select(p => p.ToString()).Aggregate((a, b) => a + ", " + b)}\n";
            }
            
            return result;
        }
        
        /// <summary>
        /// Returns a string representation of peer information.
        /// </summary>
        /// <returns>String representation</returns>
        public override string ToString()
        {
            return $"Peers(Connected: {ConnectedCount}, Bad: {BadCount}, Unconnected: {UnconnectedCount})";
        }
        
        #endregion
    }
    
    /// <summary>
    /// Represents a network address entry (IP address and port).
    /// </summary>
    [System.Serializable]
    public class AddressEntry
    {
        #region Properties
        
        /// <summary>The IP address</summary>
        [JsonProperty("address")]
        public string Address { get; set; }
        
        /// <summary>The port number</summary>
        [JsonProperty("port")]
        public int Port { get; set; }
        
        #endregion
        
        #region Constructors
        
        /// <summary>
        /// Default constructor for JSON deserialization.
        /// </summary>
        public AddressEntry()
        {
        }
        
        /// <summary>
        /// Creates a new address entry.
        /// </summary>
        /// <param name="address">The IP address</param>
        /// <param name="port">The port number</param>
        public AddressEntry(string address, int port)
        {
            Address = address;
            Port = port;
        }
        
        #endregion
        
        #region Properties (Computed)
        
        /// <summary>Whether this is a valid IP address format</summary>
        [JsonIgnore]
        public bool IsValidIpAddress => System.Net.IPAddress.TryParse(Address, out _);
        
        /// <summary>Whether this is a valid port number</summary>
        [JsonIgnore]
        public bool IsValidPort => Port > 0 && Port <= 65535;
        
        /// <summary>Whether this entry is valid overall</summary>
        [JsonIgnore]
        public bool IsValid => IsValidIpAddress && IsValidPort;
        
        /// <summary>Whether this is a localhost address</summary>
        [JsonIgnore]
        public bool IsLocalhost => Address == "127.0.0.1" || Address == "::1" || Address == "localhost";
        
        /// <summary>Whether this is a private network address</summary>
        [JsonIgnore]
        public bool IsPrivateAddress => IsPrivateNetworkAddress(Address);
        
        #endregion
        
        #region Methods
        
        /// <summary>
        /// Gets the full network endpoint string.
        /// </summary>
        /// <returns>Network endpoint (address:port)</returns>
        public string GetEndpoint()
        {
            return $"{Address}:{Port}";
        }
        
        /// <summary>
        /// Gets the network endpoint as a URI.
        /// </summary>
        /// <param name="scheme">The URI scheme (e.g., "tcp", "http")</param>
        /// <returns>Network endpoint URI</returns>
        public string GetEndpointUri(string scheme = "tcp")
        {
            return $"{scheme}://{Address}:{Port}";
        }
        
        /// <summary>
        /// Checks if this address is in a private network range.
        /// </summary>
        /// <param name="ipAddress">The IP address to check</param>
        /// <returns>True if the address is private</returns>
        private bool IsPrivateNetworkAddress(string ipAddress)
        {
            if (string.IsNullOrEmpty(ipAddress))
                return false;
            
            if (!System.Net.IPAddress.TryParse(ipAddress, out var ip))
                return false;
            
            var bytes = ip.GetAddressBytes();
            if (bytes.Length != 4) // IPv4 address validation (IPv6 support can be added later)
                return false;
            
            // Check for private ranges:
            // 10.0.0.0 - 10.255.255.255 (10.0.0.0/8)
            if (bytes[0] == 10)
                return true;
            
            // 172.16.0.0 - 172.31.255.255 (172.16.0.0/12)
            if (bytes[0] == 172 && bytes[1] >= 16 && bytes[1] <= 31)
                return true;
            
            // 192.168.0.0 - 192.168.255.255 (192.168.0.0/16)
            if (bytes[0] == 192 && bytes[1] == 168)
                return true;
            
            return false;
        }
        
        #endregion
        
        #region Validation Methods
        
        /// <summary>
        /// Validates this address entry.
        /// </summary>
        /// <exception cref="InvalidOperationException">If validation fails</exception>
        public void Validate()
        {
            if (string.IsNullOrEmpty(Address))
                throw new InvalidOperationException("Address entry IP address cannot be null or empty.");
            
            if (!IsValidIpAddress)
                throw new InvalidOperationException($"Invalid IP address format: {Address}");
            
            if (!IsValidPort)
                throw new InvalidOperationException($"Invalid port number: {Port}. Must be between 1 and 65535.");
        }
        
        #endregion
        
        #region Equality
        
        /// <summary>
        /// Determines whether the specified object is equal to the current address entry.
        /// </summary>
        /// <param name="obj">The object to compare</param>
        /// <returns>True if equal, false otherwise</returns>
        public override bool Equals(object obj)
        {
            if (obj is AddressEntry other)
            {
                return Address == other.Address && Port == other.Port;
            }
            return false;
        }
        
        /// <summary>
        /// Returns a hash code for the current address entry.
        /// </summary>
        /// <returns>Hash code</returns>
        public override int GetHashCode()
        {
            return HashCode.Combine(Address, Port);
        }
        
        #endregion
        
        #region String Representation
        
        /// <summary>
        /// Returns a string representation of this address entry.
        /// </summary>
        /// <returns>String representation</returns>
        public override string ToString()
        {
            return GetEndpoint();
        }
        
        #endregion
    }
    
    /// <summary>
    /// Contains network health information.
    /// </summary>
    [System.Serializable]
    public class NetworkHealth
    {
        /// <summary>Number of connected peers</summary>
        public int ConnectedPeers { get; set; }
        
        /// <summary>Total number of known peers</summary>
        public int TotalKnownPeers { get; set; }
        
        /// <summary>Number of bad peers</summary>
        public int BadPeers { get; set; }
        
        /// <summary>Connection success rate as percentage</summary>
        public double ConnectionSuccessRate { get; set; }
        
        /// <summary>Number of unique IP addresses</summary>
        public int UniqueAddresses { get; set; }
        
        /// <summary>Number of unique ports</summary>
        public int UniquePorts { get; set; }
        
        /// <summary>Whether the network appears healthy</summary>
        public bool IsHealthy { get; set; }
        
        /// <summary>
        /// Returns a string representation of network health.
        /// </summary>
        /// <returns>String representation</returns>
        public override string ToString()
        {
            var healthStatus = IsHealthy ? "Healthy" : "Poor";
            return $"NetworkHealth({ConnectedPeers}/{TotalKnownPeers} connected, {ConnectionSuccessRate:F1}% success, {healthStatus})";
        }
    }
}
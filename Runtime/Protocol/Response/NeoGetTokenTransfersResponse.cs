using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;
using NeoUnity.Cryptography;

namespace NeoUnity.Protocol.Response
{
    /// <summary>
    /// Response for getting token transfers for an address
    /// </summary>
    [Serializable]
    public class NeoGetTokenTransfersResponse : NeoResponse<NeoGetTokenTransfersResponse.TokenTransfers>
    {
        /// <summary>
        /// Gets the token transfers result
        /// </summary>
        public TokenTransfers TokenTransfersData => Result;
        
        /// <summary>
        /// Represents token transfers for an address
        /// </summary>
        [Serializable]
        public struct TokenTransfers : IEquatable<TokenTransfers>
        {
            [JsonProperty("sent")]
            [SerializeField] private List<TokenTransfer> _sent;
            
            [JsonProperty("received")]
            [SerializeField] private List<TokenTransfer> _received;
            
            [JsonProperty("address")]
            [SerializeField] private string _address;
            
            /// <summary>
            /// Gets the list of sent transfers
            /// </summary>
            public List<TokenTransfer> Sent => _sent ?? new List<TokenTransfer>();
            
            /// <summary>
            /// Gets the list of received transfers
            /// </summary>
            public List<TokenTransfer> Received => _received ?? new List<TokenTransfer>();
            
            /// <summary>
            /// Gets the transfer address
            /// </summary>
            public string Address => _address;
            
            /// <summary>
            /// Initializes a new instance of TokenTransfers
            /// </summary>
            [JsonConstructor]
            public TokenTransfers(List<TokenTransfer> sent, List<TokenTransfer> received, string address)
            {
                _sent = sent ?? new List<TokenTransfer>();
                _received = received ?? new List<TokenTransfer>();
                _address = address ?? throw new ArgumentNullException(nameof(address));
            }
            
            /// <summary>
            /// Gets the total number of transfers
            /// </summary>
            public int TotalTransferCount => Sent.Count + Received.Count;
            
            /// <summary>
            /// Gets all transfers combined (sent + received) sorted by timestamp
            /// </summary>
            /// <returns>All transfers sorted by timestamp</returns>
            public List<TokenTransfer> GetAllTransfers()
            {
                var allTransfers = new List<TokenTransfer>();
                allTransfers.AddRange(Sent);
                allTransfers.AddRange(Received);
                return allTransfers.OrderByDescending(t => t.Timestamp).ToList();
            }
            
            /// <summary>
            /// Gets transfers for a specific token
            /// </summary>
            /// <param name="assetHash">Asset hash to filter by</param>
            /// <returns>List of transfers for the specified token</returns>
            public List<TokenTransfer> GetTransfersForToken(string assetHash)
            {
                var allTransfers = GetAllTransfers();
                return allTransfers.Where(t => t.AssetHashString.Equals(assetHash, StringComparison.OrdinalIgnoreCase)).ToList();
            }
            
            /// <summary>
            /// Gets transfers within a block range
            /// </summary>
            /// <param name="startBlock">Start block (inclusive)</param>
            /// <param name="endBlock">End block (inclusive)</param>
            /// <returns>List of transfers within the block range</returns>
            public List<TokenTransfer> GetTransfersInBlockRange(int startBlock, int endBlock)
            {
                var allTransfers = GetAllTransfers();
                return allTransfers.Where(t => t.BlockIndex >= startBlock && t.BlockIndex <= endBlock).ToList();
            }
            
            /// <summary>
            /// Gets the net balance change for a specific token
            /// </summary>
            /// <param name="assetHash">Asset hash to calculate net change for</param>
            /// <returns>Net balance change (received - sent)</returns>
            public long GetNetBalanceChange(string assetHash)
            {
                var tokenTransfers = GetTransfersForToken(assetHash);
                var sentAmount = tokenTransfers.Where(t => Sent.Contains(t)).Sum(t => t.Amount);
                var receivedAmount = tokenTransfers.Where(t => Received.Contains(t)).Sum(t => t.Amount);
                return receivedAmount - sentAmount;
            }
            
            public bool Equals(TokenTransfers other)
            {
                return _address == other._address && 
                       Sent.Count == other.Sent.Count && 
                       Received.Count == other.Received.Count;
            }
            
            public override bool Equals(object obj)
            {
                return obj is TokenTransfers other && Equals(other);
            }
            
            public override int GetHashCode()
            {
                return HashCode.Combine(_address, Sent.Count, Received.Count);
            }
            
            public override string ToString()
            {
                return $"TokenTransfers(Address: {_address}, Sent: {Sent.Count}, Received: {Received.Count})";
            }
        }
        
        /// <summary>
        /// Represents a single token transfer
        /// </summary>
        [Serializable]
        public struct TokenTransfer : IEquatable<TokenTransfer>
        {
            [JsonProperty("timestamp")]
            [SerializeField] private long _timestamp;
            
            [JsonProperty("assethash")]
            [SerializeField] private string _assetHash;
            
            [JsonProperty("transferaddress")]
            [SerializeField] private string _transferAddress;
            
            [JsonProperty("amount")]
            [SerializeField] private string _amount;
            
            [JsonProperty("blockindex")]
            [SerializeField] private int _blockIndex;
            
            [JsonProperty("transfernotifyindex")]
            [SerializeField] private int _transferNotifyIndex;
            
            [JsonProperty("txhash")]
            [SerializeField] private string _txHash;
            
            /// <summary>
            /// Gets the timestamp of the transfer
            /// </summary>
            public long Timestamp => _timestamp;
            
            /// <summary>
            /// Gets the asset hash as string
            /// </summary>
            public string AssetHashString => _assetHash;
            
            /// <summary>
            /// Gets the asset hash as Hash160
            /// </summary>
            public Hash160 AssetHash => new Hash160(_assetHash);
            
            /// <summary>
            /// Gets the transfer address (sender or receiver depending on context)
            /// </summary>
            public string TransferAddress => _transferAddress;
            
            /// <summary>
            /// Gets the amount as string
            /// </summary>
            public string AmountString => _amount;
            
            /// <summary>
            /// Gets the amount as long
            /// </summary>
            public long Amount => long.TryParse(_amount, out var result) ? result : 0;
            
            /// <summary>
            /// Gets the block index
            /// </summary>
            public int BlockIndex => _blockIndex;
            
            /// <summary>
            /// Gets the transfer notification index
            /// </summary>
            public int TransferNotifyIndex => _transferNotifyIndex;
            
            /// <summary>
            /// Gets the transaction hash as string
            /// </summary>
            public string TxHashString => _txHash;
            
            /// <summary>
            /// Gets the transaction hash as Hash256
            /// </summary>
            public Hash256 TxHash => new Hash256(_txHash);
            
            /// <summary>
            /// Gets the amount as decimal (assuming 8 decimal places)
            /// </summary>
            public decimal AmountDecimal => Amount / 100_000_000m;
            
            /// <summary>
            /// Gets the timestamp as DateTime
            /// </summary>
            public DateTime TimestampDateTime => DateTimeOffset.FromUnixTimeMilliseconds(_timestamp).DateTime;
            
            /// <summary>
            /// Initializes a new instance of TokenTransfer
            /// </summary>
            [JsonConstructor]
            public TokenTransfer(
                long timestamp,
                string assetHash,
                string transferAddress,
                string amount,
                int blockIndex,
                int transferNotifyIndex,
                string txHash)
            {
                _timestamp = timestamp;
                _assetHash = assetHash ?? throw new ArgumentNullException(nameof(assetHash));
                _transferAddress = transferAddress ?? throw new ArgumentNullException(nameof(transferAddress));
                _amount = amount ?? "0";
                _blockIndex = blockIndex;
                _transferNotifyIndex = transferNotifyIndex;
                _txHash = txHash ?? throw new ArgumentNullException(nameof(txHash));
            }
            
            /// <summary>
            /// Gets the amount with specified decimal places
            /// </summary>
            /// <param name="decimals">Number of decimal places</param>
            /// <returns>Amount as decimal</returns>
            public decimal GetAmountWithDecimals(int decimals)
            {
                var divisor = (decimal)Math.Pow(10, decimals);
                return Amount / divisor;
            }
            
            /// <summary>
            /// Gets whether this transfer occurred recently
            /// </summary>
            /// <param name="thresholdHours">Threshold in hours</param>
            /// <returns>True if within threshold</returns>
            public bool IsRecent(int thresholdHours = 24)
            {
                var threshold = DateTime.UtcNow.AddHours(-thresholdHours);
                return TimestampDateTime > threshold;
            }
            
            /// <summary>
            /// Gets a formatted description of the transfer
            /// </summary>
            /// <returns>Human-readable transfer description</returns>
            public string GetDescription()
            {
                return $"Transfer of {AmountDecimal} tokens to/from {_transferAddress} at block {_blockIndex}";
            }
            
            public bool Equals(TokenTransfer other)
            {
                return _timestamp == other._timestamp &&
                       _assetHash == other._assetHash &&
                       _transferAddress == other._transferAddress &&
                       _amount == other._amount &&
                       _blockIndex == other._blockIndex &&
                       _transferNotifyIndex == other._transferNotifyIndex &&
                       _txHash == other._txHash;
            }
            
            public override bool Equals(object obj)
            {
                return obj is TokenTransfer other && Equals(other);
            }
            
            public override int GetHashCode()
            {
                return HashCode.Combine(_timestamp, _assetHash, _transferAddress, _amount, _blockIndex, _transferNotifyIndex, _txHash);
            }
            
            public override string ToString()
            {
                return $"TokenTransfer({AmountDecimal} {_assetHash} at block {_blockIndex})";
            }
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Newtonsoft.Json;
using Neo.Unity.SDK.Types;

namespace Neo.Unity.SDK.Protocol.Response
{
    /// <summary>
    /// Response for getting NEP-17 token transfers for an address.
    /// Contains sent and received NEP-17 token transfers for a specific address.
    /// </summary>
    [System.Serializable]
    public class NeoGetNep17TransfersResponse : NeoResponse<Nep17Transfers>
    {
        /// <summary>
        /// Gets the NEP-17 transfers from the response.
        /// </summary>
        /// <returns>NEP-17 transfers or null if response failed</returns>
        public Nep17Transfers GetTransfers()
        {
            return IsSuccess ? Result : null;
        }
        
        /// <summary>
        /// Gets the NEP-17 transfers or throws if the response failed.
        /// </summary>
        /// <returns>NEP-17 transfers</returns>
        /// <exception cref="NeoRpcException">If the response contains an error</exception>
        public Nep17Transfers GetTransfersOrThrow()
        {
            return GetResult();
        }
    }
    
    /// <summary>
    /// Represents all NEP-17 token transfers for a specific address.
    /// Contains both sent and received transfers.
    /// </summary>
    [System.Serializable]
    public class Nep17Transfers
    {
        #region Properties
        
        /// <summary>List of sent transfers</summary>
        [JsonProperty("sent")]
        public List<Nep17Transfer> Sent { get; set; }
        
        /// <summary>List of received transfers</summary>
        [JsonProperty("received")]
        public List<Nep17Transfer> Received { get; set; }
        
        /// <summary>The address these transfers belong to</summary>
        [JsonProperty("address")]
        public string Address { get; set; }
        
        #endregion
        
        #region Constructors
        
        /// <summary>
        /// Default constructor for JSON deserialization.
        /// </summary>
        public Nep17Transfers()
        {
            Sent = new List<Nep17Transfer>();
            Received = new List<Nep17Transfer>();
        }
        
        /// <summary>
        /// Creates new NEP-17 transfers.
        /// </summary>
        /// <param name="sent">Sent transfers</param>
        /// <param name="received">Received transfers</param>
        /// <param name="address">The address</param>
        public Nep17Transfers(List<Nep17Transfer> sent, List<Nep17Transfer> received, string address)
        {
            Sent = sent ?? new List<Nep17Transfer>();
            Received = received ?? new List<Nep17Transfer>();
            Address = address;
        }
        
        #endregion
        
        #region Properties (Computed)
        
        /// <summary>Whether there are any sent transfers</summary>
        [JsonIgnore]
        public bool HasSentTransfers => Sent != null && Sent.Count > 0;
        
        /// <summary>Whether there are any received transfers</summary>
        [JsonIgnore]
        public bool HasReceivedTransfers => Received != null && Received.Count > 0;
        
        /// <summary>Whether there are any transfers at all</summary>
        [JsonIgnore]
        public bool HasTransfers => HasSentTransfers || HasReceivedTransfers;
        
        /// <summary>Total number of sent transfers</summary>
        [JsonIgnore]
        public int SentCount => Sent?.Count ?? 0;
        
        /// <summary>Total number of received transfers</summary>
        [JsonIgnore]
        public int ReceivedCount => Received?.Count ?? 0;
        
        /// <summary>Total number of all transfers</summary>
        [JsonIgnore]
        public int TotalCount => SentCount + ReceivedCount;
        
        #endregion
        
        #region Transfer Operations
        
        /// <summary>
        /// Gets all transfers (sent and received) combined.
        /// </summary>
        /// <returns>List of all transfers</returns>
        public List<Nep17Transfer> GetAllTransfers()
        {
            var allTransfers = new List<Nep17Transfer>();
            
            if (HasSentTransfers)
                allTransfers.AddRange(Sent);
            
            if (HasReceivedTransfers)
                allTransfers.AddRange(Received);
            
            return allTransfers;
        }
        
        /// <summary>
        /// Gets all transfers sorted by timestamp (newest first).
        /// </summary>
        /// <returns>List of transfers sorted by timestamp</returns>
        public List<Nep17Transfer> GetAllTransfersSortedByTime()
        {
            return GetAllTransfers().OrderByDescending(t => t.Timestamp).ToList();
        }
        
        /// <summary>
        /// Gets all transfers sorted by block index (newest first).
        /// </summary>
        /// <returns>List of transfers sorted by block index</returns>
        public List<Nep17Transfer> GetAllTransfersSortedByBlock()
        {
            return GetAllTransfers().OrderByDescending(t => t.BlockIndex).ToList();
        }
        
        /// <summary>
        /// Gets transfers for a specific token.
        /// </summary>
        /// <param name="assetHash">The token contract hash</param>
        /// <returns>List of transfers for the token</returns>
        public List<Nep17Transfer> GetTransfersForToken(Hash160 assetHash)
        {
            if (assetHash == null)
                return new List<Nep17Transfer>();
            
            return GetAllTransfers().Where(t => t.AssetHash?.Equals(assetHash) == true).ToList();
        }
        
        /// <summary>
        /// Gets transfers for a specific token.
        /// </summary>
        /// <param name="assetHashString">The token contract hash as string</param>
        /// <returns>List of transfers for the token</returns>
        public List<Nep17Transfer> GetTransfersForToken(string assetHashString)
        {
            if (string.IsNullOrEmpty(assetHashString))
                return new List<Nep17Transfer>();
            
            return GetAllTransfers().Where(t => t.AssetHash?.ToString() == assetHashString).ToList();
        }
        
        /// <summary>
        /// Gets transfers within a specific block range.
        /// </summary>
        /// <param name="fromBlock">Starting block (inclusive)</param>
        /// <param name="toBlock">Ending block (inclusive)</param>
        /// <returns>List of transfers within the block range</returns>
        public List<Nep17Transfer> GetTransfersInBlockRange(long fromBlock, long toBlock)
        {
            return GetAllTransfers().Where(t => t.BlockIndex >= fromBlock && t.BlockIndex <= toBlock).ToList();
        }
        
        /// <summary>
        /// Gets transfers within a specific time range.
        /// </summary>
        /// <param name="fromTime">Starting timestamp (inclusive)</param>
        /// <param name="toTime">Ending timestamp (inclusive)</param>
        /// <returns>List of transfers within the time range</returns>
        public List<Nep17Transfer> GetTransfersInTimeRange(long fromTime, long toTime)
        {
            return GetAllTransfers().Where(t => t.Timestamp >= fromTime && t.Timestamp <= toTime).ToList();
        }
        
        /// <summary>
        /// Gets transfers above a certain amount.
        /// </summary>
        /// <param name="minimumAmount">Minimum transfer amount</param>
        /// <returns>List of transfers above the minimum amount</returns>
        public List<Nep17Transfer> GetTransfersAboveAmount(decimal minimumAmount)
        {
            return GetAllTransfers().Where(t => t.GetAmountDecimal() >= minimumAmount).ToList();
        }
        
        /// <summary>
        /// Gets the most recent transfers.
        /// </summary>
        /// <param name="count">Number of transfers to return</param>
        /// <returns>Most recent transfers</returns>
        public List<Nep17Transfer> GetRecentTransfers(int count)
        {
            return GetAllTransfersSortedByTime().Take(count).ToList();
        }
        
        /// <summary>
        /// Gets transfers involving a specific counterpart address.
        /// </summary>
        /// <param name="counterpartAddress">The counterpart address</param>
        /// <returns>List of transfers with the counterpart</returns>
        public List<Nep17Transfer> GetTransfersWithCounterpart(string counterpartAddress)
        {
            if (string.IsNullOrEmpty(counterpartAddress))
                return new List<Nep17Transfer>();
            
            return GetAllTransfers().Where(t => t.TransferAddress == counterpartAddress).ToList();
        }
        
        /// <summary>
        /// Gets all unique tokens involved in transfers.
        /// </summary>
        /// <returns>List of unique token hashes</returns>
        public List<Hash160> GetUniqueTokens()
        {
            var tokens = new HashSet<Hash160>();
            
            foreach (var transfer in GetAllTransfers())
            {
                if (transfer.AssetHash != null)
                    tokens.Add(transfer.AssetHash);
            }
            
            return tokens.ToList();
        }
        
        /// <summary>
        /// Gets all unique counterpart addresses.
        /// </summary>
        /// <returns>List of unique addresses</returns>
        public List<string> GetUniqueCounterparts()
        {
            var addresses = new HashSet<string>();
            
            foreach (var transfer in GetAllTransfers())
            {
                if (!string.IsNullOrEmpty(transfer.TransferAddress))
                    addresses.Add(transfer.TransferAddress);
            }
            
            return addresses.ToList();
        }
        
        #endregion
        
        #region Statistics
        
        /// <summary>
        /// Calculates the total sent amount for a specific token.
        /// </summary>
        /// <param name="assetHash">The token contract hash</param>
        /// <returns>Total sent amount</returns>
        public decimal GetTotalSentAmount(Hash160 assetHash)
        {
            if (assetHash == null || !HasSentTransfers)
                return 0;
            
            return Sent.Where(t => t.AssetHash?.Equals(assetHash) == true)
                      .Sum(t => t.GetAmountDecimal());
        }
        
        /// <summary>
        /// Calculates the total received amount for a specific token.
        /// </summary>
        /// <param name="assetHash">The token contract hash</param>
        /// <returns>Total received amount</returns>
        public decimal GetTotalReceivedAmount(Hash160 assetHash)
        {
            if (assetHash == null || !HasReceivedTransfers)
                return 0;
            
            return Received.Where(t => t.AssetHash?.Equals(assetHash) == true)
                          .Sum(t => t.GetAmountDecimal());
        }
        
        /// <summary>
        /// Calculates the net amount (received - sent) for a specific token.
        /// </summary>
        /// <param name="assetHash">The token contract hash</param>
        /// <returns>Net amount</returns>
        public decimal GetNetAmount(Hash160 assetHash)
        {
            return GetTotalReceivedAmount(assetHash) - GetTotalSentAmount(assetHash);
        }
        
        /// <summary>
        /// Gets transfer statistics.
        /// </summary>
        /// <returns>Transfer statistics</returns>
        public TransferStatistics GetStatistics()
        {
            return new TransferStatistics
            {
                Address = Address,
                SentCount = SentCount,
                ReceivedCount = ReceivedCount,
                TotalCount = TotalCount,
                UniqueTokenCount = GetUniqueTokens().Count,
                UniqueCounterpartCount = GetUniqueCounterparts().Count,
                EarliestTransfer = GetAllTransfers().OrderBy(t => t.Timestamp).FirstOrDefault(),
                LatestTransfer = GetAllTransfers().OrderByDescending(t => t.Timestamp).FirstOrDefault()
            };
        }
        
        #endregion
        
        #region Validation Methods
        
        /// <summary>
        /// Validates these NEP-17 transfers.
        /// </summary>
        /// <exception cref="InvalidOperationException">If validation fails</exception>
        public void Validate()
        {
            if (string.IsNullOrEmpty(Address))
                throw new InvalidOperationException("NEP-17 transfers address cannot be null or empty.");
            
            if (Sent != null)
            {
                foreach (var transfer in Sent)
                {
                    transfer.Validate();
                }
            }
            
            if (Received != null)
            {
                foreach (var transfer in Received)
                {
                    transfer.Validate();
                }
            }
        }
        
        #endregion
        
        #region String Representation
        
        /// <summary>
        /// Returns a detailed string representation of these transfers.
        /// </summary>
        /// <returns>Detailed string representation</returns>
        public string ToDetailedString()
        {
            var stats = GetStatistics();
            var result = $"NEP-17 Transfers for {Address}:\n";
            result += $"  Total Transfers: {TotalCount}\n";
            result += $"  Sent: {SentCount}\n";
            result += $"  Received: {ReceivedCount}\n";
            result += $"  Unique Tokens: {stats.UniqueTokenCount}\n";
            result += $"  Unique Counterparts: {stats.UniqueCounterpartCount}\n";
            
            if (stats.EarliestTransfer != null)
            {
                result += $"  Earliest: {stats.EarliestTransfer.GetTimestampDateTime()}\n";
            }
            
            if (stats.LatestTransfer != null)
            {
                result += $"  Latest: {stats.LatestTransfer.GetTimestampDateTime()}\n";
            }
            
            return result;
        }
        
        /// <summary>
        /// Returns a string representation of these transfers.
        /// </summary>
        /// <returns>String representation</returns>
        public override string ToString()
        {
            return $"Nep17Transfers(Address: {Address}, Sent: {SentCount}, Received: {ReceivedCount})";
        }
        
        #endregion
    }
    
    /// <summary>
    /// Represents a single NEP-17 token transfer.
    /// </summary>
    [System.Serializable]
    public class Nep17Transfer
    {
        #region Properties
        
        /// <summary>The timestamp of the transfer</summary>
        [JsonProperty("timestamp")]
        public long Timestamp { get; set; }
        
        /// <summary>The hash of the token contract</summary>
        [JsonProperty("assethash")]
        public Hash160 AssetHash { get; set; }
        
        /// <summary>The counterpart address (sender for received, recipient for sent)</summary>
        [JsonProperty("transferaddress")]
        public string TransferAddress { get; set; }
        
        /// <summary>The transfer amount as a string</summary>
        [JsonProperty("amount")]
        public string Amount { get; set; }
        
        /// <summary>The block index where this transfer occurred</summary>
        [JsonProperty("blockindex")]
        public long BlockIndex { get; set; }
        
        /// <summary>The index of the transfer notification within the transaction</summary>
        [JsonProperty("transfernotifyindex")]
        public int TransferNotifyIndex { get; set; }
        
        /// <summary>The hash of the transaction containing this transfer</summary>
        [JsonProperty("txhash")]
        public Hash256 TxHash { get; set; }
        
        #endregion
        
        #region Constructors
        
        /// <summary>
        /// Default constructor for JSON deserialization.
        /// </summary>
        public Nep17Transfer()
        {
        }
        
        /// <summary>
        /// Creates a new NEP-17 transfer.
        /// </summary>
        /// <param name="timestamp">Transfer timestamp</param>
        /// <param name="assetHash">Token contract hash</param>
        /// <param name="transferAddress">Counterpart address</param>
        /// <param name="amount">Transfer amount</param>
        /// <param name="blockIndex">Block index</param>
        /// <param name="transferNotifyIndex">Transfer notify index</param>
        /// <param name="txHash">Transaction hash</param>
        public Nep17Transfer(long timestamp, Hash160 assetHash, string transferAddress, string amount,
                            long blockIndex, int transferNotifyIndex, Hash256 txHash)
        {
            Timestamp = timestamp;
            AssetHash = assetHash;
            TransferAddress = transferAddress;
            Amount = amount;
            BlockIndex = blockIndex;
            TransferNotifyIndex = transferNotifyIndex;
            TxHash = txHash;
        }
        
        #endregion
        
        #region Properties (Computed)
        
        /// <summary>Whether this transfer has a non-zero amount</summary>
        [JsonIgnore]
        public bool HasNonZeroAmount => GetAmountDecimal() > 0;
        
        #endregion
        
        #region Amount Methods
        
        /// <summary>
        /// Gets the transfer amount as a decimal.
        /// </summary>
        /// <returns>Transfer amount as decimal</returns>
        public decimal GetAmountDecimal()
        {
            if (string.IsNullOrEmpty(Amount))
                return 0;
            
            if (decimal.TryParse(Amount, out var amount))
                return amount;
            
            return 0;
        }
        
        /// <summary>
        /// Gets the transfer amount formatted as a string.
        /// </summary>
        /// <param name="decimals">Number of decimal places for formatting</param>
        /// <returns>Formatted amount</returns>
        public string GetFormattedAmount(int decimals = 0)
        {
            var amount = GetAmountDecimal();
            
            if (decimals > 0)
            {
                var divisor = (decimal)Math.Pow(10, decimals);
                var formattedAmount = amount / divisor;
                return formattedAmount.ToString($"F{Math.Min(decimals, 8)}");
            }
            
            return amount.ToString();
        }
        
        #endregion
        
        #region Time Methods
        
        /// <summary>
        /// Gets the timestamp as a DateTime.
        /// </summary>
        /// <returns>Transfer timestamp as DateTime</returns>
        public DateTime GetTimestampDateTime()
        {
            try
            {
                return DateTimeOffset.FromUnixTimeMilliseconds(Timestamp).DateTime;
            }
            catch (ArgumentOutOfRangeException)
            {
                return DateTime.MinValue;
            }
        }
        
        /// <summary>
        /// Gets the timestamp as a DateTimeOffset.
        /// </summary>
        /// <returns>Transfer timestamp as DateTimeOffset</returns>
        public DateTimeOffset GetTimestampOffset()
        {
            try
            {
                return DateTimeOffset.FromUnixTimeMilliseconds(Timestamp);
            }
            catch (ArgumentOutOfRangeException)
            {
                return DateTimeOffset.MinValue;
            }
        }
        
        /// <summary>
        /// Gets the age of this transfer from now.
        /// </summary>
        /// <returns>Time since this transfer occurred</returns>
        public TimeSpan GetAge()
        {
            var transferTime = GetTimestampDateTime();
            return DateTime.UtcNow - transferTime;
        }
        
        #endregion
        
        #region Validation Methods
        
        /// <summary>
        /// Validates this NEP-17 transfer.
        /// </summary>
        /// <exception cref="InvalidOperationException">If validation fails</exception>
        public void Validate()
        {
            if (AssetHash == null)
                throw new InvalidOperationException("NEP-17 transfer asset hash cannot be null.");
            
            if (TxHash == null)
                throw new InvalidOperationException("NEP-17 transfer transaction hash cannot be null.");
            
            if (string.IsNullOrEmpty(Amount))
                throw new InvalidOperationException("NEP-17 transfer amount cannot be null or empty.");
            
            if (string.IsNullOrEmpty(TransferAddress))
                throw new InvalidOperationException("NEP-17 transfer address cannot be null or empty.");
            
            if (BlockIndex < 0)
                throw new InvalidOperationException("NEP-17 transfer block index cannot be negative.");
            
            if (TransferNotifyIndex < 0)
                throw new InvalidOperationException("NEP-17 transfer notify index cannot be negative.");
            
            // Validate amount is a valid number
            if (!decimal.TryParse(Amount, out _))
                throw new InvalidOperationException($"NEP-17 transfer amount is not a valid number: {Amount}");
        }
        
        #endregion
        
        #region String Representation
        
        /// <summary>
        /// Returns a detailed string representation of this transfer.
        /// </summary>
        /// <returns>Detailed string representation</returns>
        public string ToDetailedString()
        {
            var result = $"NEP-17 Transfer:\n";
            result += $"  Amount: {Amount}\n";
            result += $"  Token: {AssetHash}\n";
            result += $"  Counterpart: {TransferAddress}\n";
            result += $"  Transaction: {TxHash}\n";
            result += $"  Block: #{BlockIndex}\n";
            result += $"  Time: {GetTimestampDateTime()}\n";
            result += $"  Notify Index: {TransferNotifyIndex}\n";
            
            return result;
        }
        
        /// <summary>
        /// Returns a string representation of this transfer.
        /// </summary>
        /// <returns>String representation</returns>
        public override string ToString()
        {
            var hashPreview = AssetHash?.ToString()?.Substring(0, 8) + "...";
            var addressPreview = TransferAddress?.Length > 8 ? TransferAddress.Substring(0, 8) + "..." : TransferAddress;
            return $"Nep17Transfer({Amount} {hashPreview} ↔ {addressPreview})";
        }
        
        #endregion
    }
    
    /// <summary>
    /// Contains statistical information about NEP-17 transfers.
    /// </summary>
    [System.Serializable]
    public class TransferStatistics
    {
        /// <summary>The address these statistics are for</summary>
        public string Address { get; set; }
        
        /// <summary>Number of sent transfers</summary>
        public int SentCount { get; set; }
        
        /// <summary>Number of received transfers</summary>
        public int ReceivedCount { get; set; }
        
        /// <summary>Total number of transfers</summary>
        public int TotalCount { get; set; }
        
        /// <summary>Number of unique tokens</summary>
        public int UniqueTokenCount { get; set; }
        
        /// <summary>Number of unique counterpart addresses</summary>
        public int UniqueCounterpartCount { get; set; }
        
        /// <summary>The earliest transfer</summary>
        public Nep17Transfer EarliestTransfer { get; set; }
        
        /// <summary>The latest transfer</summary>
        public Nep17Transfer LatestTransfer { get; set; }
        
        /// <summary>
        /// Returns a string representation of these statistics.
        /// </summary>
        /// <returns>String representation</returns>
        public override string ToString()
        {
            return $"TransferStatistics(Total: {TotalCount}, Sent: {SentCount}, Received: {ReceivedCount}, Tokens: {UniqueTokenCount})";
        }
    }
}
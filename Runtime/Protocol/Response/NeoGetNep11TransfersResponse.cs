using System;
using System.Collections.Generic;
using UnityEngine;
using Neo.Unity.SDK.Types;
using Neo.Unity.SDK.Core;
using Newtonsoft.Json;

namespace Neo.Unity.SDK.Protocol.Response
{
    /// <summary>
    /// Response for getNep11Transfers RPC method.
    /// Returns NEP-11 (NFT) transfer history for a specific address.
    /// </summary>
    [System.Serializable]
    public class NeoGetNep11TransfersResponse : NeoResponse<NeoGetNep11TransfersResponse.Nep11Transfers>
    {
        /// <summary>
        /// NEP-11 transfers data structure containing transfer history for an address.
        /// </summary>
        [System.Serializable]
        public class Nep11Transfers
        {
            /// <summary>The address these transfers belong to</summary>
            [JsonProperty("address")]
            public string Address { get; set; }
            
            /// <summary>List of NEP-11 transfers sent from this address</summary>
            [JsonProperty("sent")]
            public List<Nep11Transfer> Sent { get; set; }
            
            /// <summary>List of NEP-11 transfers received by this address</summary>
            [JsonProperty("received")]
            public List<Nep11Transfer> Received { get; set; }
            
            /// <summary>
            /// Default constructor.
            /// </summary>
            public Nep11Transfers()
            {
                Sent = new List<Nep11Transfer>();
                Received = new List<Nep11Transfer>();
            }
            
            /// <summary>
            /// Gets all transfers (sent and received) in chronological order.
            /// </summary>
            /// <returns>All transfers sorted by timestamp</returns>
            public List<Nep11Transfer> GetAllTransfers()
            {
                var allTransfers = new List<Nep11Transfer>();
                
                if (Sent != null)
                    allTransfers.AddRange(Sent);
                
                if (Received != null)
                    allTransfers.AddRange(Received);
                
                // Sort by timestamp
                allTransfers.Sort((a, b) => a.Timestamp.CompareTo(b.Timestamp));
                
                return allTransfers;
            }
            
            /// <summary>
            /// Gets transfers for a specific NFT contract.
            /// </summary>
            /// <param name="assetHash">The NFT contract hash</param>
            /// <returns>Transfers for the specified contract</returns>
            public List<Nep11Transfer> GetTransfersForContract(Hash160 assetHash)
            {
                var contractTransfers = new List<Nep11Transfer>();
                
                if (assetHash == null)
                    return contractTransfers;
                
                if (Sent != null)
                {
                    contractTransfers.AddRange(Sent.FindAll(t => t.AssetHash.Equals(assetHash)));
                }
                
                if (Received != null)
                {
                    contractTransfers.AddRange(Received.FindAll(t => t.AssetHash.Equals(assetHash)));
                }
                
                return contractTransfers;
            }
            
            /// <summary>
            /// Gets the total number of NFT transfers.
            /// </summary>
            /// <returns>Total transfer count</returns>
            public int GetTotalTransferCount()
            {
                return (Sent?.Count ?? 0) + (Received?.Count ?? 0);
            }
            
            /// <summary>
            /// String representation of NEP-11 transfers.
            /// </summary>
            /// <returns>String representation</returns>
            public override string ToString()
            {
                return $"Nep11Transfers(Address: {Address}, Sent: {Sent?.Count ?? 0}, Received: {Received?.Count ?? 0})";
            }
        }
        
        /// <summary>
        /// Individual NEP-11 token transfer record.
        /// </summary>
        [System.Serializable]
        public class Nep11Transfer
        {
            /// <summary>The transaction hash containing this transfer</summary>
            [JsonProperty("txhash")]
            public Hash256 TransactionHash { get; set; }
            
            /// <summary>The NFT contract hash</summary>
            [JsonProperty("assethash")]
            public Hash160 AssetHash { get; set; }
            
            /// <summary>The sender address</summary>
            [JsonProperty("from")]
            public string From { get; set; }
            
            /// <summary>The recipient address</summary>
            [JsonProperty("to")]
            public string To { get; set; }
            
            /// <summary>The NFT token ID</summary>
            [JsonProperty("tokenid")]
            public string TokenId { get; set; }
            
            /// <summary>The timestamp of the transfer (Unix timestamp)</summary>
            [JsonProperty("timestamp")]
            public long Timestamp { get; set; }
            
            /// <summary>The block index containing this transfer</summary>
            [JsonProperty("blockindex")]
            public long BlockIndex { get; set; }
            
            /// <summary>The transaction index within the block</summary>
            [JsonProperty("transferindex")]
            public int TransferIndex { get; set; }
            
            /// <summary>Additional transfer data (if any)</summary>
            [JsonProperty("transfernotifyindex")]
            public int TransferNotifyIndex { get; set; }
            
            /// <summary>
            /// Gets the transfer timestamp as DateTime.
            /// </summary>
            /// <returns>Transfer DateTime</returns>
            public DateTime GetDateTime()
            {
                return DateTimeOffset.FromUnixTimeMilliseconds(Timestamp).DateTime;
            }
            
            /// <summary>
            /// Checks if this transfer was sent from the specified address.
            /// </summary>
            /// <param name="address">The address to check</param>
            /// <returns>True if sent from the address</returns>
            public bool IsSentFrom(string address)
            {
                return !string.IsNullOrEmpty(From) && From.Equals(address, StringComparison.OrdinalIgnoreCase);
            }
            
            /// <summary>
            /// Checks if this transfer was received by the specified address.
            /// </summary>
            /// <param name="address">The address to check</param>
            /// <returns>True if received by the address</returns>
            public bool IsReceivedBy(string address)
            {
                return !string.IsNullOrEmpty(To) && To.Equals(address, StringComparison.OrdinalIgnoreCase);
            }
            
            /// <summary>
            /// Gets the transfer direction relative to the specified address.
            /// </summary>
            /// <param name="address">The reference address</param>
            /// <returns>Transfer direction</returns>
            public TransferDirection GetDirection(string address)
            {
                if (IsSentFrom(address))
                    return TransferDirection.Sent;
                
                if (IsReceivedBy(address))
                    return TransferDirection.Received;
                
                return TransferDirection.Unknown;
            }
            
            /// <summary>
            /// String representation of NFT transfer.
            /// </summary>
            /// <returns>String representation</returns>
            public override string ToString()
            {
                return $"Nep11Transfer(TokenID: {TokenId}, From: {From}, To: {To}, TxHash: {TransactionHash})";
            }
        }
        
        /// <summary>
        /// Transfer direction enumeration.
        /// </summary>
        public enum TransferDirection
        {
            /// <summary>Transfer was sent from the address</summary>
            Sent,
            
            /// <summary>Transfer was received by the address</summary>
            Received,
            
            /// <summary>Transfer direction unknown</summary>
            Unknown
        }
    }
}
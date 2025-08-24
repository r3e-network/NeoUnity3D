using System;
using System.Collections.Generic;
using UnityEngine;
using Neo.Unity.SDK.Types;
using Neo.Unity.SDK.Core;
using Newtonsoft.Json;

namespace Neo.Unity.SDK.Protocol.Response
{
    /// <summary>
    /// Response for getNep11Balances RPC method.
    /// Returns NEP-11 (NFT) token balances for a specific address.
    /// </summary>
    [System.Serializable]
    public class NeoGetNep11BalancesResponse : NeoResponse<NeoGetNep11BalancesResponse.Nep11Balances>
    {
        /// <summary>
        /// NEP-11 balances data structure containing all NFT balances for an address.
        /// </summary>
        [System.Serializable]
        public class Nep11Balances
        {
            /// <summary>The address these balances belong to</summary>
            [JsonProperty("address")]
            public string Address { get; set; }
            
            /// <summary>List of NEP-11 token balances</summary>
            [JsonProperty("balance")]
            public List<Nep11Balance> Balances { get; set; }
            
            /// <summary>
            /// Default constructor.
            /// </summary>
            public Nep11Balances()
            {
                Balances = new List<Nep11Balance>();
            }
            
            /// <summary>
            /// Gets the balance for a specific token contract.
            /// </summary>
            /// <param name="assetHash">The NFT contract hash</param>
            /// <returns>Balance information or null if not found</returns>
            public Nep11Balance GetBalance(Hash160 assetHash)
            {
                if (assetHash == null || Balances == null)
                    return null;
                
                return Balances.Find(b => b.AssetHash.Equals(assetHash));
            }
            
            /// <summary>
            /// Gets the total number of different NFT contracts this address has tokens for.
            /// </summary>
            /// <returns>Number of different NFT contracts</returns>
            public int GetTokenContractCount()
            {
                return Balances?.Count ?? 0;
            }
            
            /// <summary>
            /// Gets the total number of NFT tokens across all contracts.
            /// </summary>
            /// <returns>Total NFT token count</returns>
            public long GetTotalTokenCount()
            {
                if (Balances == null)
                    return 0;
                
                long total = 0;
                foreach (var balance in Balances)
                {
                    total += balance.Amount;
                }
                
                return total;
            }
            
            /// <summary>
            /// String representation of NEP-11 balances.
            /// </summary>
            /// <returns>String representation</returns>
            public override string ToString()
            {
                return $"Nep11Balances(Address: {Address}, Contracts: {GetTokenContractCount()}, Total Tokens: {GetTotalTokenCount()})";
            }
        }
        
        /// <summary>
        /// Individual NEP-11 token balance for a specific contract.
        /// </summary>
        [System.Serializable]
        public class Nep11Balance
        {
            /// <summary>The NFT contract hash</summary>
            [JsonProperty("assethash")]
            public Hash160 AssetHash { get; set; }
            
            /// <summary>The NFT contract name</summary>
            [JsonProperty("name")]
            public string Name { get; set; }
            
            /// <summary>The NFT contract symbol</summary>
            [JsonProperty("symbol")]
            public string Symbol { get; set; }
            
            /// <summary>Number of tokens owned from this contract</summary>
            [JsonProperty("amount")]
            public long Amount { get; set; }
            
            /// <summary>List of token IDs owned (if enumerable)</summary>
            [JsonProperty("tokens")]
            public List<TokenInfo> Tokens { get; set; }
            
            /// <summary>
            /// Default constructor.
            /// </summary>
            public Nep11Balance()
            {
                Tokens = new List<TokenInfo>();
            }
            
            /// <summary>
            /// Checks if this balance represents an enumerable NFT contract.
            /// </summary>
            /// <returns>True if token IDs are available</returns>
            public bool IsEnumerable()
            {
                return Tokens != null && Tokens.Count > 0;
            }
            
            /// <summary>
            /// Gets a specific token by ID.
            /// </summary>
            /// <param name="tokenId">The token ID to find</param>
            /// <returns>Token info or null if not found</returns>
            public TokenInfo GetToken(string tokenId)
            {
                if (string.IsNullOrEmpty(tokenId) || Tokens == null)
                    return null;
                
                return Tokens.Find(t => t.TokenId == tokenId);
            }
            
            /// <summary>
            /// String representation of NFT balance.
            /// </summary>
            /// <returns>String representation</returns>
            public override string ToString()
            {
                return $"Nep11Balance(Contract: {Name} ({Symbol}), Amount: {Amount}, Hash: {AssetHash})";
            }
        }
        
        /// <summary>
        /// Information about an individual NFT token.
        /// </summary>
        [System.Serializable]
        public class TokenInfo
        {
            /// <summary>The unique token ID</summary>
            [JsonProperty("tokenid")]
            public string TokenId { get; set; }
            
            /// <summary>The token amount (usually 1 for NFTs)</summary>
            [JsonProperty("amount")]
            public long Amount { get; set; }
            
            /// <summary>
            /// String representation of token info.
            /// </summary>
            /// <returns>String representation</returns>
            public override string ToString()
            {
                return $"TokenInfo(ID: {TokenId}, Amount: {Amount})";
            }
        }
    }
}
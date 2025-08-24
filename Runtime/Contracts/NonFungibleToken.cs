using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Neo.Unity.SDK.Core;
using Neo.Unity.SDK.Types;
using Neo.Unity.SDK.Transaction;
using Neo.Unity.SDK.Wallet;

namespace Neo.Unity.SDK.Contracts
{
    /// <summary>
    /// Represents a non-fungible token contract that is compliant with the NEP-11 standard
    /// and provides methods to invoke it. NEP-11 is the Neo standard for non-fungible tokens (NFTs).
    /// </summary>
    [System.Serializable]
    public abstract class NonFungibleToken : Token
    {
        #region Constants

        private const string BALANCE_OF = "balanceOf";
        private const string TOKENS_OF = "tokensOf";
        private const string TRANSFER = "transfer";
        private const string OWNER_OF = "ownerOf";
        private const string PROPERTIES = "properties";

        #endregion

        #region Constructor

        /// <summary>
        /// Constructs a NonFungibleToken instance representing the NEP-11 token contract with the given script hash.
        /// </summary>
        /// <param name="scriptHash">The token contract's script hash</param>
        /// <param name="neoUnity">The NeoUnity instance to use for invocations</param>
        protected NonFungibleToken(Hash160 scriptHash, NeoUnity neoUnity) : base(scriptHash, neoUnity)
        {
        }

        /// <summary>
        /// Constructs a NonFungibleToken instance using the singleton NeoUnity instance.
        /// </summary>
        /// <param name="scriptHash">The token contract's script hash</param>
        protected NonFungibleToken(Hash160 scriptHash) : base(scriptHash)
        {
        }

        #endregion

        #region NEP-11 Methods

        /// <summary>
        /// Gets the number of tokens owned by the given account.
        /// </summary>
        /// <param name="account">The account to query</param>
        /// <returns>The number of tokens owned</returns>
        public async Task<long> GetBalanceOf(Account account)
        {
            if (account == null)
            {
                throw new ArgumentNullException(nameof(account));
            }

            return await GetBalanceOf(account.GetScriptHash());
        }

        /// <summary>
        /// Gets the number of tokens owned by the given script hash.
        /// </summary>
        /// <param name="scriptHash">The script hash to query</param>
        /// <returns>The number of tokens owned</returns>
        public async Task<long> GetBalanceOf(Hash160 scriptHash)
        {
            if (scriptHash == null)
            {
                throw new ArgumentNullException(nameof(scriptHash));
            }

            var result = await CallFunctionReturningInt(BALANCE_OF, ContractParameter.Hash160(scriptHash));
            
            if (NeoUnity.Config.EnableDebugLogging)
            {
                Debug.Log($"[NonFungibleToken] Balance of {scriptHash}: {result} tokens");
            }

            return result;
        }

        /// <summary>
        /// Gets the owner of the specified token.
        /// </summary>
        /// <param name="tokenId">The token ID as byte array</param>
        /// <returns>The script hash of the token owner</returns>
        public async Task<Hash160> GetOwnerOf(byte[] tokenId)
        {
            if (tokenId == null || tokenId.Length == 0)
            {
                throw new ArgumentException("Token ID cannot be null or empty.", nameof(tokenId));
            }

            return await CallFunctionReturningScriptHash(OWNER_OF, ContractParameter.ByteArray(tokenId));
        }

        /// <summary>
        /// Gets the owner of the specified token.
        /// </summary>
        /// <param name="tokenId">The token ID as string</param>
        /// <returns>The script hash of the token owner</returns>
        public async Task<Hash160> GetOwnerOf(string tokenId)
        {
            if (string.IsNullOrEmpty(tokenId))
            {
                throw new ArgumentException("Token ID cannot be null or empty.", nameof(tokenId));
            }

            var tokenBytes = System.Text.Encoding.UTF8.GetBytes(tokenId);
            return await GetOwnerOf(tokenBytes);
        }

        /// <summary>
        /// Gets all tokens owned by the specified account using an iterator.
        /// </summary>
        /// <param name="scriptHash">The script hash to query</param>
        /// <returns>An iterator of token IDs (as byte arrays)</returns>
        public async Task<Iterator<byte[]>> GetTokensOf(Hash160 scriptHash)
        {
            if (scriptHash == null)
            {
                throw new ArgumentNullException(nameof(scriptHash));
            }

            return await CallFunctionReturningIterator<byte[]>(TOKENS_OF, 
                new[] { ContractParameter.Hash160(scriptHash) }, 
                item => item.GetByteArray());
        }

        /// <summary>
        /// Gets all tokens owned by the specified account without using sessions.
        /// </summary>
        /// <param name="scriptHash">The script hash to query</param>
        /// <param name="count">Maximum number of tokens to retrieve</param>
        /// <returns>List of token IDs (as byte arrays)</returns>
        public async Task<List<byte[]>> GetTokensOfUnwrapped(Hash160 scriptHash, int count = DEFAULT_ITERATOR_COUNT)
        {
            if (scriptHash == null)
            {
                throw new ArgumentNullException(nameof(scriptHash));
            }

            var stackItems = await CallFunctionAndUnwrapIterator(TOKENS_OF, 
                new[] { ContractParameter.Hash160(scriptHash) }, 
                count);

            var tokenIds = new List<byte[]>();
            foreach (var item in stackItems)
            {
                tokenIds.Add(item.GetByteArray());
            }

            return tokenIds;
        }

        /// <summary>
        /// Gets the properties of the specified token.
        /// </summary>
        /// <param name="tokenId">The token ID as byte array</param>
        /// <returns>Dictionary of token properties</returns>
        public async Task<Dictionary<string, string>> GetProperties(byte[] tokenId)
        {
            if (tokenId == null || tokenId.Length == 0)
            {
                throw new ArgumentException("Token ID cannot be null or empty.", nameof(tokenId));
            }

            var invocationResult = await CallInvokeFunction(PROPERTIES, ContractParameter.ByteArray(tokenId));
            var result = invocationResult.GetResult();
            ThrowIfFaultState(result);

            var stackItem = result.GetFirstStackItem();
            if (!stackItem.IsMap())
            {
                throw new ContractException("Properties result must be a map");
            }

            var properties = new Dictionary<string, string>();
            var mapItems = stackItem.GetMap();

            foreach (var kvp in mapItems)
            {
                var key = kvp.Key.GetString();
                var value = kvp.Value.GetString();
                properties[key] = value;
            }

            return properties;
        }

        /// <summary>
        /// Gets the properties of the specified token.
        /// </summary>
        /// <param name="tokenId">The token ID as string</param>
        /// <returns>Dictionary of token properties</returns>
        public async Task<Dictionary<string, string>> GetProperties(string tokenId)
        {
            if (string.IsNullOrEmpty(tokenId))
            {
                throw new ArgumentException("Token ID cannot be null or empty.", nameof(tokenId));
            }

            var tokenBytes = System.Text.Encoding.UTF8.GetBytes(tokenId);
            return await GetProperties(tokenBytes);
        }

        /// <summary>
        /// Creates a transfer transaction for the specified token.
        /// </summary>
        /// <param name="from">The current owner account</param>
        /// <param name="to">The recipient script hash</param>
        /// <param name="tokenId">The token ID to transfer</param>
        /// <param name="data">Optional data for the transfer</param>
        /// <returns>A transaction builder ready for signing</returns>
        public async Task<TransactionBuilder> Transfer(Account from, Hash160 to, byte[] tokenId, ContractParameter data = null)
        {
            if (from == null)
            {
                throw new ArgumentNullException(nameof(from));
            }

            var builder = await Transfer(from.GetScriptHash(), to, tokenId, data);
            return builder.SetSigners(new List<Signer> { new AccountSigner(from) { Scope = WitnessScope.CalledByEntry } });
        }

        /// <summary>
        /// Creates a transfer transaction for the specified token.
        /// </summary>
        /// <param name="from">The current owner script hash</param>
        /// <param name="to">The recipient script hash</param>
        /// <param name="tokenId">The token ID to transfer</param>
        /// <param name="data">Optional data for the transfer</param>
        /// <returns>A transaction builder ready for signing</returns>
        public async Task<TransactionBuilder> Transfer(Hash160 from, Hash160 to, byte[] tokenId, ContractParameter data = null)
        {
            if (from == null)
            {
                throw new ArgumentNullException(nameof(from));
            }

            if (to == null)
            {
                throw new ArgumentNullException(nameof(to));
            }

            if (tokenId == null || tokenId.Length == 0)
            {
                throw new ArgumentException("Token ID cannot be null or empty.", nameof(tokenId));
            }

            var parameters = new List<ContractParameter>
            {
                ContractParameter.Hash160(from),
                ContractParameter.Hash160(to),
                ContractParameter.ByteArray(tokenId)
            };

            if (data != null)
            {
                parameters.Add(data);
            }

            var transferScript = await BuildInvokeFunctionScript(TRANSFER, parameters.ToArray());
            return new TransactionBuilder(NeoUnity).SetScript(transferScript);
        }

        /// <summary>
        /// Creates a transfer transaction for the specified token using string token ID.
        /// </summary>
        /// <param name="from">The current owner account</param>
        /// <param name="to">The recipient script hash</param>
        /// <param name="tokenId">The token ID as string</param>
        /// <param name="data">Optional data for the transfer</param>
        /// <returns>A transaction builder ready for signing</returns>
        public async Task<TransactionBuilder> Transfer(Account from, Hash160 to, string tokenId, ContractParameter data = null)
        {
            var tokenBytes = System.Text.Encoding.UTF8.GetBytes(tokenId);
            return await Transfer(from, to, tokenBytes, data);
        }

        #endregion

        #region Unity Integration Helpers

        /// <summary>
        /// Checks if an account owns any tokens from this contract.
        /// </summary>
        /// <param name="account">The account to check</param>
        /// <returns>True if the account owns any tokens</returns>
        public async Task<bool> HasTokens(Account account)
        {
            var balance = await GetBalanceOf(account);
            return balance > 0;
        }

        /// <summary>
        /// Gets a formatted token count for display.
        /// </summary>
        /// <param name="scriptHash">The script hash to check</param>
        /// <returns>Formatted token count (e.g., "5 tokens")</returns>
        public async Task<string> GetFormattedTokenCount(Hash160 scriptHash)
        {
            var balance = await GetBalanceOf(scriptHash);
            var symbol = await GetSymbol();
            return balance == 1 ? $"1 {symbol}" : $"{balance} {symbol}s";
        }

        /// <summary>
        /// Validates that a token exists and is owned by the specified account.
        /// </summary>
        /// <param name="tokenId">The token ID to validate</param>
        /// <param name="expectedOwner">The expected owner</param>
        /// <returns>True if the token exists and is owned by the expected owner</returns>
        public async Task<bool> ValidateTokenOwnership(byte[] tokenId, Hash160 expectedOwner)
        {
            try
            {
                var actualOwner = await GetOwnerOf(tokenId);
                return actualOwner.Equals(expectedOwner);
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[NonFungibleToken] Failed to validate token ownership: {ex.Message}");
                return false;
            }
        }

        #endregion
    }

    /// <summary>
    /// Represents NFT transfer information for multi-transfer operations.
    /// </summary>
    [System.Serializable]
    public class NFTTransfer
    {
        /// <summary>The recipient script hash</summary>
        [SerializeField]
        public Hash160 To { get; set; }

        /// <summary>The token ID to transfer</summary>
        [SerializeField]
        public byte[] TokenId { get; set; }

        /// <summary>Optional data to pass to the recipient</summary>
        [SerializeField]
        public ContractParameter Data { get; set; }

        /// <summary>
        /// Creates a new NFT transfer.
        /// </summary>
        /// <param name="to">The recipient script hash</param>
        /// <param name="tokenId">The token ID</param>
        /// <param name="data">Optional contract data</param>
        public NFTTransfer(Hash160 to, byte[] tokenId, ContractParameter data = null)
        {
            To = to ?? throw new ArgumentNullException(nameof(to));
            TokenId = tokenId ?? throw new ArgumentNullException(nameof(tokenId));
            Data = data;
        }

        /// <summary>
        /// Creates a new NFT transfer with string token ID.
        /// </summary>
        /// <param name="to">The recipient script hash</param>
        /// <param name="tokenId">The token ID as string</param>
        /// <param name="data">Optional contract data</param>
        public NFTTransfer(Hash160 to, string tokenId, ContractParameter data = null)
        {
            To = to ?? throw new ArgumentNullException(nameof(to));
            TokenId = System.Text.Encoding.UTF8.GetBytes(tokenId ?? throw new ArgumentNullException(nameof(tokenId)));
            Data = data;
        }
    }
}
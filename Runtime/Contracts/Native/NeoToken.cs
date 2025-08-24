using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Neo.Unity.SDK.Core;
using Neo.Unity.SDK.Types;
using Neo.Unity.SDK.Transaction;
using Neo.Unity.SDK.Wallet;
using Neo.Unity.SDK.Crypto;

namespace Neo.Unity.SDK.Contracts.Native
{
    /// <summary>
    /// Represents the NeoToken native contract and provides methods to invoke its functions.
    /// The NEO token is the governance token of the Neo blockchain, used for voting and committee participation.
    /// </summary>
    [System.Serializable]
    public class NeoToken : FungibleToken
    {
        #region Constants

        public const string NAME = "NeoToken";
        public static readonly Hash160 SCRIPT_HASH = SmartContract.CalcNativeContractHash(NAME);
        public const int DECIMALS = 0;
        public const string SYMBOL = "NEO";
        public const int TOTAL_SUPPLY = 100_000_000;

        private const string UNCLAIMED_GAS = "unclaimedGas";
        private const string REGISTER_CANDIDATE = "registerCandidate";
        private const string UNREGISTER_CANDIDATE = "unregisterCandidate";
        private const string VOTE = "vote";
        private const string GET_CANDIDATES = "getCandidates";
        private const string GET_ALL_CANDIDATES = "getAllCandidates";
        private const string GET_CANDIDATE_VOTES = "getCandidateVote";
        private const string GET_COMMITTEE = "getCommittee";
        private const string GET_NEXT_BLOCK_VALIDATORS = "getNextBlockValidators";
        private const string SET_GAS_PER_BLOCK = "setGasPerBlock";
        private const string GET_GAS_PER_BLOCK = "getGasPerBlock";
        private const string SET_REGISTER_PRICE = "setRegisterPrice";
        private const string GET_REGISTER_PRICE = "getRegisterPrice";
        private const string GET_ACCOUNT_STATE = "getAccountState";

        #endregion

        #region Constructor

        /// <summary>
        /// Constructs a new NeoToken instance that uses the given NeoUnity instance for invocations.
        /// </summary>
        /// <param name="neoUnity">The NeoUnity instance to use for invocations</param>
        public NeoToken(NeoUnity neoUnity) : base(SCRIPT_HASH, neoUnity)
        {
        }

        /// <summary>
        /// Constructs a new NeoToken instance using the singleton NeoUnity instance.
        /// </summary>
        public NeoToken() : base(SCRIPT_HASH)
        {
        }

        #endregion

        #region Token Metadata Overrides

        /// <summary>
        /// Returns the name of the NEO token.
        /// Doesn't require a call to the Neo node.
        /// </summary>
        /// <returns>The name</returns>
        public async Task<string> GetName()
        {
            return NAME;
        }

        /// <summary>
        /// Returns the symbol of the NEO token.
        /// Doesn't require a call to the Neo node.
        /// </summary>
        /// <returns>The symbol</returns>
        public override async Task<string> GetSymbol()
        {
            return SYMBOL;
        }

        /// <summary>
        /// Returns the total supply of the NEO token.
        /// Doesn't require a call to the Neo node.
        /// </summary>
        /// <returns>The total supply</returns>
        public override async Task<int> GetTotalSupply()
        {
            return TOTAL_SUPPLY;
        }

        /// <summary>
        /// Returns the number of decimals of the NEO token.
        /// Doesn't require a call to the Neo node.
        /// </summary>
        /// <returns>The number of decimals</returns>
        public override async Task<int> GetDecimals()
        {
            return DECIMALS;
        }

        #endregion

        #region Unclaimed Gas

        /// <summary>
        /// Gets the amount of unclaimed GAS at the given height for the given account.
        /// </summary>
        /// <param name="account">The account</param>
        /// <param name="blockHeight">The block height</param>
        /// <returns>The amount of unclaimed GAS in GAS fractions</returns>
        public async Task<long> GetUnclaimedGas(Account account, int blockHeight)
        {
            if (account == null)
            {
                throw new ArgumentNullException(nameof(account));
            }

            return await GetUnclaimedGas(account.GetScriptHash(), blockHeight);
        }

        /// <summary>
        /// Gets the amount of unclaimed GAS at the given height for the given account script hash.
        /// </summary>
        /// <param name="scriptHash">The account's script hash</param>
        /// <param name="blockHeight">The block height</param>
        /// <returns>The amount of unclaimed GAS in GAS fractions</returns>
        public async Task<long> GetUnclaimedGas(Hash160 scriptHash, int blockHeight)
        {
            if (scriptHash == null)
            {
                throw new ArgumentNullException(nameof(scriptHash));
            }

            if (blockHeight < 0)
            {
                throw new ArgumentException("Block height cannot be negative.", nameof(blockHeight));
            }

            var result = await CallFunctionReturningInt(UNCLAIMED_GAS, 
                ContractParameter.Hash160(scriptHash), 
                ContractParameter.Integer(blockHeight));

            if (NeoUnity.Config.EnableDebugLogging)
            {
                Debug.Log($"[NeoToken] Unclaimed GAS for {scriptHash} at block {blockHeight}: {result}");
            }

            return result;
        }

        #endregion

        #region Candidate Registration

        /// <summary>
        /// Creates a transaction script for registering a candidate with the given public key and initializes a TransactionBuilder based on this script.
        /// Note that the transaction has to be signed with the account corresponding to the public key.
        /// </summary>
        /// <param name="candidateKey">The public key to register as a candidate</param>
        /// <returns>A transaction builder ready for signing</returns>
        public async Task<TransactionBuilder> RegisterCandidate(ECPublicKey candidateKey)
        {
            if (candidateKey == null)
            {
                throw new ArgumentNullException(nameof(candidateKey));
            }

            var encodedKey = candidateKey.GetEncoded(true);
            return await InvokeFunction(REGISTER_CANDIDATE, ContractParameter.PublicKey(encodedKey));
        }

        /// <summary>
        /// Creates a transaction script for unregistering a candidate with the given public key and initializes a TransactionBuilder based on this script.
        /// </summary>
        /// <param name="candidateKey">The public key to unregister as a candidate</param>
        /// <returns>A transaction builder ready for signing</returns>
        public async Task<TransactionBuilder> UnregisterCandidate(ECPublicKey candidateKey)
        {
            if (candidateKey == null)
            {
                throw new ArgumentNullException(nameof(candidateKey));
            }

            var encodedKey = candidateKey.GetEncoded(true);
            return await InvokeFunction(UNREGISTER_CANDIDATE, ContractParameter.PublicKey(encodedKey));
        }

        #endregion

        #region Committee and Candidates Information

        /// <summary>
        /// Gets the public keys of the current committee members.
        /// </summary>
        /// <returns>The committee members' public keys</returns>
        public async Task<List<ECPublicKey>> GetCommittee()
        {
            return await CallFunctionReturningListOfPublicKeys(GET_COMMITTEE);
        }

        /// <summary>
        /// Gets the public keys of the registered candidates and their corresponding vote count.
        /// Note that this method returns at max 256 candidates. Use GetAllCandidatesIterator() to traverse through all candidates if there are more than 256.
        /// </summary>
        /// <returns>The candidates</returns>
        public async Task<List<Candidate>> GetCandidates()
        {
            var invocationResult = await CallInvokeFunction(GET_CANDIDATES);
            var result = invocationResult.GetResult();
            ThrowIfFaultState(result);

            var stackItem = result.GetFirstStackItem();
            if (!stackItem.IsArray())
            {
                throw new ContractException($"Unexpected return type. Expected Array, got {stackItem.Type}");
            }

            var candidateList = new List<Candidate>();
            var arrayItems = stackItem.GetList();

            foreach (var item in arrayItems)
            {
                candidateList.Add(MapCandidate(item));
            }

            if (NeoUnity.Config.EnableDebugLogging)
            {
                Debug.Log($"[NeoToken] Retrieved {candidateList.Count} candidates");
            }

            return candidateList;
        }

        /// <summary>
        /// Checks if there is a candidate with the provided public key.
        /// Note that this only checks the first 256 candidates. Use GetAllCandidatesIterator() to traverse through all candidates if there are more than 256.
        /// </summary>
        /// <param name="publicKey">The candidate's public key</param>
        /// <returns>True if the public key belongs to a candidate, otherwise false</returns>
        public async Task<bool> IsCandidate(ECPublicKey publicKey)
        {
            if (publicKey == null)
            {
                throw new ArgumentNullException(nameof(publicKey));
            }

            var candidates = await GetCandidates();
            return candidates.Exists(c => c.PublicKey.Equals(publicKey));
        }

        /// <summary>
        /// Gets an iterator of all registered candidates.
        /// Use the method Iterator.Traverse() to traverse the iterator and retrieve all candidates.
        /// </summary>
        /// <returns>An iterator of all registered candidates</returns>
        public async Task<Iterator<Candidate>> GetAllCandidatesIterator()
        {
            return await CallFunctionReturningIterator<Candidate>(GET_ALL_CANDIDATES, null, MapCandidate);
        }

        /// <summary>
        /// Maps a stack item to a Candidate object.
        /// </summary>
        /// <param name="stackItem">The stack item containing candidate data</param>
        /// <returns>The mapped candidate</returns>
        private Candidate MapCandidate(StackItem stackItem)
        {
            if (!stackItem.IsArray())
            {
                throw new ContractException("Candidate stack item must be an array");
            }

            var list = stackItem.GetList();
            if (list.Count < 2)
            {
                throw new ContractException("Candidate array must have at least 2 elements");
            }

            var publicKeyBytes = list[0].GetByteArray();
            var votes = list[1].GetInteger();

            var publicKey = new ECPublicKey(publicKeyBytes);
            return new Candidate(publicKey, votes);
        }

        /// <summary>
        /// Gets the votes for a specific candidate.
        /// </summary>
        /// <param name="publicKey">The candidate's public key</param>
        /// <returns>The candidate's votes, or -1 if it was not found</returns>
        public async Task<long> GetCandidateVotes(ECPublicKey publicKey)
        {
            if (publicKey == null)
            {
                throw new ArgumentNullException(nameof(publicKey));
            }

            var encodedKey = publicKey.GetEncoded(true);
            return await CallFunctionReturningInt(GET_CANDIDATE_VOTES, ContractParameter.PublicKey(encodedKey));
        }

        /// <summary>
        /// Gets the public keys of the next block's validators.
        /// </summary>
        /// <returns>The validators' public keys</returns>
        public async Task<List<ECPublicKey>> GetNextBlockValidators()
        {
            return await CallFunctionReturningListOfPublicKeys(GET_NEXT_BLOCK_VALIDATORS);
        }

        /// <summary>
        /// Helper method to call functions that return lists of public keys.
        /// </summary>
        /// <param name="function">The function name to call</param>
        /// <returns>List of public keys</returns>
        private async Task<List<ECPublicKey>> CallFunctionReturningListOfPublicKeys(string function)
        {
            var invocationResult = await CallInvokeFunction(function);
            var result = invocationResult.GetResult();
            ThrowIfFaultState(result);

            var stackItem = result.GetFirstStackItem();
            if (!stackItem.IsArray())
            {
                throw new ContractException($"Unexpected return type. Expected Array, got {stackItem.Type}");
            }

            var publicKeys = new List<ECPublicKey>();
            var arrayItems = stackItem.GetList();

            foreach (var item in arrayItems)
            {
                publicKeys.Add(ExtractPublicKey(item));
            }

            return publicKeys;
        }

        /// <summary>
        /// Extracts a public key from a stack item.
        /// </summary>
        /// <param name="keyItem">The stack item containing the public key</param>
        /// <returns>The extracted public key</returns>
        private ECPublicKey ExtractPublicKey(StackItem keyItem)
        {
            if (!keyItem.IsByteString())
            {
                throw new ContractException($"Unexpected return type. Expected ByteString, got {keyItem.Type}");
            }

            try
            {
                var keyBytes = keyItem.GetByteArray();
                return new ECPublicKey(keyBytes);
            }
            catch (Exception ex)
            {
                throw new ContractException($"Byte array return type did not contain public key in expected format: {ex.Message}", ex);
            }
        }

        #endregion

        #region Voting

        /// <summary>
        /// Creates a transaction script to vote for the given candidate and initializes a TransactionBuilder based on this script.
        /// </summary>
        /// <param name="voter">The account that casts the vote</param>
        /// <param name="candidate">The candidate to vote for. If null, then the current vote of the voter is withdrawn</param>
        /// <returns>A transaction builder ready for signing</returns>
        public async Task<TransactionBuilder> Vote(Account voter, ECPublicKey candidate = null)
        {
            if (voter == null)
            {
                throw new ArgumentNullException(nameof(voter));
            }

            return await Vote(voter.GetScriptHash(), candidate);
        }

        /// <summary>
        /// Creates a transaction script to vote for the given candidate and initializes a TransactionBuilder based on this script.
        /// </summary>
        /// <param name="voter">The account script hash that casts the vote</param>
        /// <param name="candidate">The candidate to vote for. If null, then the current vote of the voter is withdrawn</param>
        /// <returns>A transaction builder ready for signing</returns>
        public async Task<TransactionBuilder> Vote(Hash160 voter, ECPublicKey candidate = null)
        {
            if (voter == null)
            {
                throw new ArgumentNullException(nameof(voter));
            }

            if (candidate == null)
            {
                return await InvokeFunction(VOTE, 
                    ContractParameter.Hash160(voter), 
                    ContractParameter.Any(null));
            }

            var encodedKey = candidate.GetEncoded(true);
            return await InvokeFunction(VOTE, 
                ContractParameter.Hash160(voter), 
                ContractParameter.PublicKey(encodedKey));
        }

        /// <summary>
        /// Creates a transaction script to cancel the vote of voter and initializes a TransactionBuilder based on the script.
        /// </summary>
        /// <param name="voter">The account for which to cancel the vote</param>
        /// <returns>A transaction builder ready for signing</returns>
        public async Task<TransactionBuilder> CancelVote(Account voter)
        {
            return await Vote(voter, null);
        }

        /// <summary>
        /// Creates a transaction script to cancel the vote of voter and initializes a TransactionBuilder based on the script.
        /// </summary>
        /// <param name="voter">The account script hash for which to cancel the vote</param>
        /// <returns>A transaction builder ready for signing</returns>
        public async Task<TransactionBuilder> CancelVote(Hash160 voter)
        {
            return await Vote(voter, null);
        }

        /// <summary>
        /// Builds a script to vote for a candidate.
        /// </summary>
        /// <param name="voter">The account that casts the vote</param>
        /// <param name="candidate">The candidate to vote for. If null, then the current vote of the voter is withdrawn</param>
        /// <returns>The vote script as byte array</returns>
        public async Task<byte[]> BuildVoteScript(Hash160 voter, ECPublicKey candidate = null)
        {
            if (voter == null)
            {
                throw new ArgumentNullException(nameof(voter));
            }

            if (candidate == null)
            {
                return await BuildInvokeFunctionScript(VOTE, 
                    ContractParameter.Hash160(voter), 
                    ContractParameter.Any(null));
            }

            var encodedKey = candidate.GetEncoded(true);
            return await BuildInvokeFunctionScript(VOTE, 
                ContractParameter.Hash160(voter), 
                ContractParameter.PublicKey(encodedKey));
        }

        #endregion

        #region Network Settings

        /// <summary>
        /// Gets the number of GAS generated in each block.
        /// </summary>
        /// <returns>The max GAS amount per block in GAS fractions</returns>
        public async Task<long> GetGasPerBlock()
        {
            return await CallFunctionReturningInt(GET_GAS_PER_BLOCK);
        }

        /// <summary>
        /// Creates a transaction script to set the number of GAS generated in each block and initializes a TransactionBuilder based on this script.
        /// This contract invocation can only be successful if it is signed by the network committee.
        /// </summary>
        /// <param name="gasPerBlock">The maximum amount of GAS in one block in GAS fractions</param>
        /// <returns>The transaction builder ready for committee signing</returns>
        public async Task<TransactionBuilder> SetGasPerBlock(long gasPerBlock)
        {
            if (gasPerBlock < 0)
            {
                throw new ArgumentException("Gas per block cannot be negative.", nameof(gasPerBlock));
            }

            return await InvokeFunction(SET_GAS_PER_BLOCK, ContractParameter.Integer(gasPerBlock));
        }

        /// <summary>
        /// Gets the price to register as a candidate.
        /// </summary>
        /// <returns>The price to register as a candidate in GAS fractions</returns>
        public async Task<long> GetRegisterPrice()
        {
            return await CallFunctionReturningInt(GET_REGISTER_PRICE);
        }

        /// <summary>
        /// Creates a transaction script to set the price for candidate registration and initializes a TransactionBuilder based on this script.
        /// This contract invocation can only be successful if it is signed by the network committee.
        /// </summary>
        /// <param name="registerPrice">The price to register as a candidate in GAS fractions</param>
        /// <returns>The transaction builder ready for committee signing</returns>
        public async Task<TransactionBuilder> SetRegisterPrice(long registerPrice)
        {
            if (registerPrice < 0)
            {
                throw new ArgumentException("Register price cannot be negative.", nameof(registerPrice));
            }

            return await InvokeFunction(SET_REGISTER_PRICE, ContractParameter.Integer(registerPrice));
        }

        #endregion

        #region Account State

        /// <summary>
        /// Gets the state of an account.
        /// </summary>
        /// <param name="account">The account to get the state from</param>
        /// <returns>The account state</returns>
        public async Task<NeoAccountState> GetAccountState(Account account)
        {
            if (account == null)
            {
                throw new ArgumentNullException(nameof(account));
            }

            return await GetAccountState(account.GetScriptHash());
        }

        /// <summary>
        /// Gets the state of an account by script hash.
        /// </summary>
        /// <param name="scriptHash">The account script hash to get the state from</param>
        /// <returns>The account state</returns>
        public async Task<NeoAccountState> GetAccountState(Hash160 scriptHash)
        {
            if (scriptHash == null)
            {
                throw new ArgumentNullException(nameof(scriptHash));
            }

            var invocationResult = await CallInvokeFunction(GET_ACCOUNT_STATE, ContractParameter.Hash160(scriptHash));
            var result = invocationResult.GetResult();
            ThrowIfFaultState(result);

            if (result.Stack == null || result.Stack.Count == 0)
            {
                throw new ContractException("Account State stack was empty.");
            }

            var stackItem = result.Stack[0];
            
            // Check if account has no balance (returns Any/null)
            if (stackItem.IsAny())
            {
                return NeoAccountState.WithNoBalance();
            }

            if (!stackItem.IsArray())
            {
                throw new ContractException("Account state must be an array or null.");
            }

            var stateArray = stackItem.GetList();
            if (stateArray.Count < 3)
            {
                throw new ContractException("Account state array must have at least 3 elements.");
            }

            var balance = stateArray[0].GetInteger();
            var updateHeight = stateArray[1].GetInteger();
            var publicKeyItem = stateArray[2];

            // Check if account has no vote (publicKey is Any/null)
            if (publicKeyItem.IsAny())
            {
                return NeoAccountState.WithNoVote(balance, (int)updateHeight);
            }

            var publicKeyBytes = publicKeyItem.GetByteArray();
            var publicKey = new ECPublicKey(publicKeyBytes);

            return new NeoAccountState(balance, (int)updateHeight, publicKey);
        }

        #endregion

        #region Unity Integration

        /// <summary>
        /// Gets formatted NEO balance for display purposes.
        /// Since NEO has 0 decimals, this returns whole numbers.
        /// </summary>
        /// <param name="scriptHash">The script hash to get balance for</param>
        /// <returns>Formatted balance string (e.g., "100 NEO")</returns>
        public async Task<string> GetFormattedNeoBalance(Hash160 scriptHash)
        {
            var balance = await GetBalanceOf(scriptHash);
            return $"{balance} {SYMBOL}";
        }

        /// <summary>
        /// Checks if an account can vote (has NEO balance).
        /// </summary>
        /// <param name="account">The account to check</param>
        /// <returns>True if the account can vote (has NEO balance)</returns>
        public async Task<bool> CanVote(Account account)
        {
            var balance = await GetBalanceOf(account);
            return balance > 0;
        }

        /// <summary>
        /// Gets the voting power of an account (equivalent to NEO balance).
        /// </summary>
        /// <param name="account">The account to check</param>
        /// <returns>The voting power (NEO balance)</returns>
        public async Task<long> GetVotingPower(Account account)
        {
            return await GetBalanceOf(account);
        }

        #endregion
    }

    /// <summary>
    /// Represents the state of a Neo candidate.
    /// </summary>
    [System.Serializable]
    public class Candidate : IEquatable<Candidate>
    {
        /// <summary>The candidate's public key</summary>
        public ECPublicKey PublicKey { get; private set; }

        /// <summary>The candidate's votes based on the summed up NEO balances of this candidate's voters</summary>
        public long Votes { get; private set; }

        /// <summary>
        /// Creates a new candidate.
        /// </summary>
        /// <param name="publicKey">The candidate's public key</param>
        /// <param name="votes">The candidate's vote count</param>
        public Candidate(ECPublicKey publicKey, long votes)
        {
            PublicKey = publicKey ?? throw new ArgumentNullException(nameof(publicKey));
            Votes = votes;
        }

        /// <summary>
        /// Gets the Neo address of this candidate.
        /// </summary>
        /// <returns>The candidate's Neo address</returns>
        public string GetAddress()
        {
            // Create a temporary key pair to get the address
            var keyPair = ECKeyPair.Create(new ECPrivateKey(new byte[32]), PublicKey);
            return keyPair.GetAddress();
        }

        public bool Equals(Candidate other)
        {
            if (other == null) return false;
            return PublicKey.Equals(other.PublicKey) && Votes == other.Votes;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as Candidate);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(PublicKey, Votes);
        }

        public override string ToString()
        {
            return $"Candidate(PublicKey: {PublicKey.GetEncoded(true).ToHexString()}, Votes: {Votes})";
        }
    }

    /// <summary>
    /// Represents the state of a Neo account.
    /// </summary>
    [System.Serializable]
    public class NeoAccountState
    {
        /// <summary>The account's NEO balance</summary>
        public long Balance { get; private set; }

        /// <summary>The block height when the balance was last updated</summary>
        public int BalanceHeight { get; private set; }

        /// <summary>The public key the account is voting for, or null if not voting</summary>
        public ECPublicKey VoteTo { get; private set; }

        /// <summary>Whether the account has a balance</summary>
        public bool HasBalance => Balance > 0;

        /// <summary>Whether the account is currently voting</summary>
        public bool IsVoting => VoteTo != null;

        /// <summary>
        /// Creates a new Neo account state.
        /// </summary>
        /// <param name="balance">The NEO balance</param>
        /// <param name="balanceHeight">The balance update height</param>
        /// <param name="voteTo">The public key being voted for, or null</param>
        public NeoAccountState(long balance, int balanceHeight, ECPublicKey voteTo = null)
        {
            Balance = balance;
            BalanceHeight = balanceHeight;
            VoteTo = voteTo;
        }

        /// <summary>
        /// Creates an account state with no balance.
        /// </summary>
        /// <returns>Account state with zero balance</returns>
        public static NeoAccountState WithNoBalance()
        {
            return new NeoAccountState(0, 0, null);
        }

        /// <summary>
        /// Creates an account state with balance but no vote.
        /// </summary>
        /// <param name="balance">The NEO balance</param>
        /// <param name="balanceHeight">The balance update height</param>
        /// <returns>Account state with balance but no vote</returns>
        public static NeoAccountState WithNoVote(long balance, int balanceHeight)
        {
            return new NeoAccountState(balance, balanceHeight, null);
        }

        public override string ToString()
        {
            var voteInfo = IsVoting ? $", Voting for: {VoteTo.GetEncoded(true).ToHexString()}" : "";
            return $"NeoAccountState(Balance: {Balance}, Height: {BalanceHeight}{voteInfo})";
        }
    }
}
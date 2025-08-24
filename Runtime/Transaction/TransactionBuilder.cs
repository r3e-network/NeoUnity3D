using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using Neo.Unity.SDK.Core;
using Neo.Unity.SDK.Types;
using Neo.Unity.SDK.Crypto;
using Neo.Unity.SDK.Script;
using Neo.Unity.SDK.Wallet;

namespace Neo.Unity.SDK.Transaction
{
    /// <summary>
    /// Used to build a NeoTransaction. When signing the TransactionBuilder, a transaction is created
    /// that can be sent to the Neo node. Implements the builder pattern with method chaining for easy use.
    /// </summary>
    [System.Serializable]
    public class TransactionBuilder
    {
        #region Constants
        
        /// <summary>GAS token contract hash</summary>
        public static readonly Hash160 GAS_TOKEN_HASH = new Hash160("d2a4cff31913016155e38e474a2c06d08be276cf");
        
        /// <summary>NEP-17 balanceOf function name</summary>
        private const string BALANCE_OF_FUNCTION = "balanceOf";
        
        /// <summary>Dummy public key for fee calculation</summary>
        private const string DUMMY_PUB_KEY = "02ec143f00b88524caf36a0121c2de09eef0519ddbe1c710a00f0e2663201ee4c0";
        
        #endregion
        
        #region Private Fields
        
        private readonly NeoUnity neoUnity;
        
        [SerializeField]
        private byte version;
        
        [SerializeField]
        private uint nonce;
        
        [SerializeField]
        private uint? validUntilBlock;
        
        [SerializeField]
        private List<Signer> signers;
        
        [SerializeField]
        private long additionalNetworkFee;
        
        [SerializeField]
        private long additionalSystemFee;
        
        [SerializeField]
        private List<TransactionAttribute> attributes;
        
        [SerializeField]
        private byte[] script;
        
        // Unity doesn't serialize delegates, so we handle this differently
        private Action<long, long> feeConsumer;
        private Exception feeError;
        
        #endregion
        
        #region Properties
        
        /// <summary>The current signers for this transaction</summary>
        public IReadOnlyList<Signer> Signers => signers?.AsReadOnly() ?? new List<Signer>().AsReadOnly();
        
        /// <summary>The current script for this transaction</summary>
        public byte[] Script => script;
        
        /// <summary>Whether this transaction has high priority attribute</summary>
        private bool IsHighPriority => attributes?.Any(attr => attr is HighPriorityAttribute) ?? false;
        
        #endregion
        
        #region Constructor
        
        /// <summary>
        /// Creates a new transaction builder.
        /// </summary>
        /// <param name="neoUnity">The NeoUnity instance to use for blockchain operations</param>
        public TransactionBuilder(NeoUnity neoUnity)
        {
            this.neoUnity = neoUnity ?? throw new ArgumentNullException(nameof(neoUnity));
            
            // Initialize with defaults
            version = NeoConstants.CURRENT_TX_VERSION;
            nonce = GenerateRandomNonce();
            signers = new List<Signer>();
            attributes = new List<TransactionAttribute>();
            script = new byte[0];
            additionalNetworkFee = 0;
            additionalSystemFee = 0;
        }
        
        /// <summary>
        /// Generates a random nonce for transaction uniqueness.
        /// </summary>
        /// <returns>Random nonce value</returns>
        private uint GenerateRandomNonce()
        {
            return (uint)UnityEngine.Random.Range(0, int.MaxValue);
        }
        
        #endregion
        
        #region Builder Methods - Basic Configuration
        
        /// <summary>
        /// Sets the version for this transaction.
        /// It is set to NeoConstants.CURRENT_TX_VERSION by default.
        /// </summary>
        /// <param name="version">The transaction version number</param>
        /// <returns>This transaction builder for method chaining</returns>
        public TransactionBuilder SetVersion(byte version)
        {
            this.version = version;
            return this;
        }
        
        /// <summary>
        /// Sets the nonce (number used once) for this transaction. The nonce is a number from 0 to 2^32.
        /// It is set to a random value by default.
        /// </summary>
        /// <param name="nonce">The transaction nonce</param>
        /// <returns>This transaction builder for method chaining</returns>
        /// <exception cref="ArgumentException">If nonce is out of valid range</exception>
        public TransactionBuilder SetNonce(uint nonce)
        {
            this.nonce = nonce;
            return this;
        }
        
        /// <summary>
        /// Sets the number of the block up to which this transaction can be included.
        /// If that block number is reached in the network and this transaction is not yet included in a block, it becomes invalid.
        /// By default, it is set to the maximum, which is the current chain height plus maxValidUntilBlockIncrement.
        /// </summary>
        /// <param name="blockNumber">The block number</param>
        /// <returns>This transaction builder for method chaining</returns>
        /// <exception cref="ArgumentException">If block number is invalid</exception>
        public TransactionBuilder SetValidUntilBlock(uint blockNumber)
        {
            validUntilBlock = blockNumber;
            return this;
        }
        
        #endregion
        
        #region Builder Methods - Script Configuration
        
        /// <summary>
        /// Sets the script for this transaction. It defines the actions that this transaction will perform on the blockchain.
        /// </summary>
        /// <param name="script">The contract script</param>
        /// <returns>This transaction builder for method chaining</returns>
        public TransactionBuilder SetScript(byte[] script)
        {
            this.script = script ?? throw new ArgumentNullException(nameof(script));
            return this;
        }
        
        /// <summary>
        /// Concatenates the existing script and the provided script, i.e. the provided script is appended to the existing script.
        /// This method may be used to create an advanced transaction that contains multiple invocations.
        /// </summary>
        /// <param name="additionalScript">The script to append</param>
        /// <returns>This transaction builder for method chaining</returns>
        public TransactionBuilder ExtendScript(byte[] additionalScript)
        {
            if (additionalScript == null)
                throw new ArgumentNullException(nameof(additionalScript));
            
            if (script == null || script.Length == 0)
            {
                script = additionalScript;
            }
            else
            {
                var newScript = new byte[script.Length + additionalScript.Length];
                Array.Copy(script, 0, newScript, 0, script.Length);
                Array.Copy(additionalScript, 0, newScript, script.Length, additionalScript.Length);
                script = newScript;
            }
            
            return this;
        }
        
        #endregion
        
        #region Builder Methods - Signer Configuration
        
        /// <summary>
        /// Sets the signer belonging to the given sender account to the first index of the list of signers for this transaction.
        /// The first signer covers the fees for the transaction if there is no signer present with fee-only witness scope.
        /// </summary>
        /// <param name="sender">The account of the signer to be set to the first index</param>
        /// <returns>This transaction builder for method chaining</returns>
        public TransactionBuilder SetFirstSigner(Account sender)
        {
            if (sender == null)
                throw new ArgumentNullException(nameof(sender));
            
            return SetFirstSigner(sender.GetScriptHash());
        }
        
        /// <summary>
        /// Sets the signer with script hash sender to the first index of the list of signers for this transaction.
        /// The first signer covers the fees for the transaction if there is no signer present with fee-only witness scope.
        /// </summary>
        /// <param name="senderHash">The script hash of the signer to be set to the first index</param>
        /// <returns>This transaction builder for method chaining</returns>
        public TransactionBuilder SetFirstSigner(Hash160 senderHash)
        {
            if (senderHash == null)
                throw new ArgumentNullException(nameof(senderHash));
            
            // Check for fee-only witness scope
            if (signers.Any(s => s.Scopes.Contains(WitnessScope.None)))
            {
                throw new InvalidOperationException("This transaction contains a signer with fee-only witness scope that will cover the fees. Hence, the order of the signers does not affect the payment of the fees.");
            }
            
            // Find the signer to move to first position
            var targetSigner = signers.FirstOrDefault(s => s.SignerHash.Equals(senderHash));
            if (targetSigner == null)
            {
                throw new InvalidOperationException($"Could not find a signer with script hash {senderHash}. Make sure to add the signer before calling this method.");
            }
            
            // Move signer to first position
            signers.Remove(targetSigner);
            signers.Insert(0, targetSigner);
            
            return this;
        }
        
        /// <summary>
        /// Sets the signers of this transaction. If the list of signers already contains signers, they are replaced.
        /// If one of the signers has the fee-only witness scope, this account is used to cover the transaction fees.
        /// Otherwise, the first signer is used as the sender of this transaction.
        /// </summary>
        /// <param name="signers">The signers for this transaction</param>
        /// <returns>This transaction builder for method chaining</returns>
        public TransactionBuilder SetSigners(params Signer[] signers)
        {
            return SetSigners(signers?.ToList() ?? new List<Signer>());
        }
        
        /// <summary>
        /// Sets the signers of this transaction. If the list of signers already contains signers, they are replaced.
        /// If one of the signers has the fee-only witness scope, this account is used to cover the transaction fees.
        /// Otherwise, the first signer is used as the sender of this transaction.
        /// </summary>
        /// <param name="signers">The signers for this transaction</param>
        /// <returns>This transaction builder for method chaining</returns>
        public TransactionBuilder SetSigners(List<Signer> signers)
        {
            if (signers == null)
                throw new ArgumentNullException(nameof(signers));
            
            // Check for duplicate signer hashes
            var signerHashes = signers.Select(s => s.SignerHash).ToList();
            if (signerHashes.Count != signerHashes.Distinct().Count())
            {
                throw new ArgumentException("Cannot add multiple signers concerning the same account.");
            }
            
            // Check for maximum attributes limit
            ThrowIfMaxAttributesExceeded(signers.Count, attributes.Count);
            
            this.signers = new List<Signer>(signers);
            return this;
        }
        
        #endregion
        
        #region Builder Methods - Fee Configuration
        
        /// <summary>
        /// Configures the transaction with an additional network fee.
        /// The basic network fee required to send this transaction is added automatically.
        /// </summary>
        /// <param name="fee">The additional network fee in fractions of GAS</param>
        /// <returns>This transaction builder for method chaining</returns>
        public TransactionBuilder SetAdditionalNetworkFee(long fee)
        {
            additionalNetworkFee = Math.Max(0, fee);
            return this;
        }
        
        /// <summary>
        /// Configures the transaction with an additional system fee.
        /// The basic system fee required to send this transaction is added automatically.
        /// Use this if you expect the transaction to consume more GAS because of chain state changes
        /// happening between creating the transaction and actually sending it.
        /// </summary>
        /// <param name="fee">The additional system fee in fractions of GAS</param>
        /// <returns>This transaction builder for method chaining</returns>
        public TransactionBuilder SetAdditionalSystemFee(long fee)
        {
            additionalSystemFee = Math.Max(0, fee);
            return this;
        }
        
        #endregion
        
        #region Builder Methods - Attributes
        
        /// <summary>
        /// Adds the given attributes to this transaction.
        /// The maximum number of attributes on a transaction is given in NeoConstants.MAX_TRANSACTION_ATTRIBUTES.
        /// </summary>
        /// <param name="attributes">The attributes to add</param>
        /// <returns>This transaction builder for method chaining</returns>
        public TransactionBuilder AddAttributes(params TransactionAttribute[] attributes)
        {
            return AddAttributes(attributes?.ToList() ?? new List<TransactionAttribute>());
        }
        
        /// <summary>
        /// Adds the given attributes to this transaction.
        /// The maximum number of attributes on a transaction is given in NeoConstants.MAX_TRANSACTION_ATTRIBUTES.
        /// </summary>
        /// <param name="attributes">The attributes to add</param>
        /// <returns>This transaction builder for method chaining</returns>
        public TransactionBuilder AddAttributes(List<TransactionAttribute> attributes)
        {
            if (attributes == null)
                throw new ArgumentNullException(nameof(attributes));
            
            ThrowIfMaxAttributesExceeded(signers.Count, this.attributes.Count + attributes.Count);
            
            foreach (var attr in attributes)
            {
                // Avoid duplicate high priority attributes
                if (!(attr is HighPriorityAttribute) || !IsHighPriority)
                {
                    this.attributes.Add(attr);
                }
            }
            
            return this;
        }
        
        #endregion
        
        #region Builder Methods - Fee Handling
        
        /// <summary>
        /// Checks if the sender account of this transaction can cover the network and system fees.
        /// If not, executes the given action supplying it with the required fee and the sender's GAS balance.
        /// The check and potential execution of the action is only performed when the transaction is built.
        /// </summary>
        /// <param name="feeConsumer">The action to execute if fees cannot be covered</param>
        /// <returns>This transaction builder for method chaining</returns>
        public TransactionBuilder DoIfSenderCannotCoverFees(Action<long, long> feeConsumer)
        {
            if (feeError != null)
            {
                throw new InvalidOperationException("Cannot handle a consumer for this case, since an exception will be thrown if the sender cannot cover the fees.");
            }
            
            this.feeConsumer = feeConsumer;
            return this;
        }
        
        /// <summary>
        /// Checks if the sender account of this transaction can cover the network and system fees.
        /// If not, throws the provided exception.
        /// The check and potential throwing of the exception is only performed when the transaction is built.
        /// </summary>
        /// <param name="error">The error to throw if fees cannot be covered</param>
        /// <returns>This transaction builder for method chaining</returns>
        public TransactionBuilder ThrowIfSenderCannotCoverFees(Exception error)
        {
            if (feeConsumer != null)
            {
                throw new InvalidOperationException("Cannot handle a supplier for this case, since a consumer will be executed if the sender cannot cover the fees.");
            }
            
            this.feeError = error;
            return this;
        }
        
        #endregion
        
        #region Transaction Building
        
        /// <summary>
        /// Builds the transaction without signing it.
        /// </summary>
        /// <returns>The unsigned transaction</returns>
        public async Task<NeoTransaction> GetUnsignedTransaction()
        {
            // Validate script
            if (script == null || script.Length == 0)
            {
                throw new TransactionException("Cannot build a transaction without a script.");
            }
            
            // Set validUntilBlock if not set
            if (validUntilBlock == null)
            {
                var currentBlockCount = await neoUnity.GetBlockCount().SendAsync();
                var blockCount = currentBlockCount.GetResult();
                validUntilBlock = (uint)(blockCount + neoUnity.MaxValidUntilBlockIncrement - 1);
            }
            
            // Validate signers
            if (signers.Count == 0)
            {
                throw new InvalidOperationException("Cannot create a transaction without signers. At least one signer with witness scope fee-only or higher is required.");
            }
            
            // Check high priority permissions
            if (IsHighPriority)
            {
                var isAllowed = await IsAllowedForHighPriority();
                if (!isAllowed)
                {
                    throw new InvalidOperationException("This transaction does not have a committee member as signer. Only committee members can send transactions with high priority.");
                }
            }
            
            // Calculate fees
            var systemFee = await GetSystemFeeForScript() + additionalSystemFee;
            var networkFee = await CalculateNetworkFee() + additionalNetworkFee;
            var totalFees = systemFee + networkFee;
            
            // Handle fee coverage
            if (feeError != null && !await CanSenderCoverFees(totalFees))
            {
                throw feeError;
            }
            else if (feeConsumer != null)
            {
                var gasBalance = await GetSenderGasBalance();
                if (totalFees > gasBalance)
                {
                    feeConsumer(totalFees, gasBalance);
                }
            }
            
            return new NeoTransaction(
                neoUnity: neoUnity,
                version: version,
                nonce: nonce,
                validUntilBlock: validUntilBlock.Value,
                signers: new List<Signer>(signers),
                systemFee: systemFee,
                networkFee: networkFee,
                attributes: new List<TransactionAttribute>(attributes),
                script: script,
                witnesses: new List<Witness>()
            );
        }
        
        /// <summary>
        /// Makes an invokescript call to the Neo node with the transaction in its current configuration.
        /// No changes are made to the blockchain state.
        /// Make sure to add all necessary signers to the builder before making this call.
        /// </summary>
        /// <returns>The call's response</returns>
        public async Task<NeoInvokeScriptResponse> CallInvokeScript()
        {
            if (script == null || script.Length == 0)
            {
                throw new TransactionException("Cannot make an 'invokescript' call without the script being configured.");
            }
            
            return await neoUnity.InvokeScript(script.ToHexString(), signers).SendAsync();
        }
        
        /// <summary>
        /// Builds the transaction, creates signatures for every signer and adds them to the transaction as witnesses.
        /// For each signer of the transaction, a corresponding account with an EC key pair must exist.
        /// </summary>
        /// <returns>The signed transaction</returns>
        public async Task<NeoTransaction> Sign()
        {
            var transaction = await GetUnsignedTransaction();
            var txBytes = await transaction.GetHashData();
            
            foreach (var signer in transaction.Signers)
            {
                if (signer is ContractSigner contractSigner)
                {
                    transaction.AddWitness(Witness.CreateContractWitness(contractSigner.VerifyParams));
                }
                else if (signer is AccountSigner accountSigner)
                {
                    var account = accountSigner.Account;
                    
                    if (account.IsMultiSig)
                    {
                        throw new InvalidOperationException("Transactions with multi-sig signers cannot be signed automatically.");
                    }
                    
                    if (account.KeyPair == null)
                    {
                        throw new TransactionException($"Cannot create transaction signature because account {account.Address} does not hold a private key.");
                    }
                    
                    transaction.AddWitness(await Witness.Create(txBytes, account.KeyPair));
                }
            }
            
            return transaction;
        }
        
        #endregion
        
        #region Private Helper Methods
        
        /// <summary>
        /// Checks if the maximum number of transaction attributes would be exceeded.
        /// </summary>
        /// <param name="signerCount">Number of signers</param>
        /// <param name="attributeCount">Number of attributes</param>
        private void ThrowIfMaxAttributesExceeded(int signerCount, int attributeCount)
        {
            if (signerCount + attributeCount > NeoConstants.MAX_TRANSACTION_ATTRIBUTES)
            {
                throw new ArgumentException($"A transaction cannot have more than {NeoConstants.MAX_TRANSACTION_ATTRIBUTES} attributes (including signers).");
            }
        }
        
        /// <summary>
        /// Checks if the transaction is allowed to have high priority.
        /// </summary>
        /// <returns>True if allowed, false otherwise</returns>
        private async Task<bool> IsAllowedForHighPriority()
        {
            try
            {
                var committeeResponse = await neoUnity.GetCommittee().SendAsync();
                var committee = committeeResponse.GetResult();
                
                var committeePubKeys = committee.Select(pubKey => new ECPublicKey(pubKey).GetEncoded(true))
                                                .Select(Hash160.FromPublicKey)
                                                .ToList();
                
                var signerHashes = signers.Select(s => s.SignerHash).ToList();
                
                return signerHashes.Any(hash => committeePubKeys.Contains(hash)) ||
                       SignersContainMultiSigWithCommitteeMember(committeePubKeys);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[TransactionBuilder] Failed to check high priority permissions: {ex.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// Checks if any multi-sig signers contain a committee member.
        /// </summary>
        /// <param name="committee">Committee member hashes</param>
        /// <returns>True if a multi-sig signer contains a committee member</returns>
        private bool SignersContainMultiSigWithCommitteeMember(List<Hash160> committee)
        {
            foreach (var signer in signers)
            {
                if (signer is AccountSigner accountSigner && accountSigner.Account.IsMultiSig)
                {
                    var verificationScript = accountSigner.Account.VerificationScript;
                    if (verificationScript != null)
                    {
                        try
                        {
                            var publicKeys = verificationScript.GetPublicKeys();
                            var keyHashes = publicKeys.Select(pk => Hash160.FromPublicKey(pk.GetEncoded(true))).ToList();
                            
                            if (keyHashes.Any(keyHash => committee.Contains(keyHash)))
                            {
                                return true;
                            }
                        }
                        catch (Exception ex)
                        {
                            Debug.LogWarning($"[TransactionBuilder] Failed to parse multi-sig verification script: {ex.Message}");
                        }
                    }
                }
            }
            
            return false;
        }
        
        /// <summary>
        /// Gets the system fee required for the script execution.
        /// </summary>
        /// <returns>The system fee in GAS fractions</returns>
        private async Task<long> GetSystemFeeForScript()
        {
            try
            {
                var response = await neoUnity.InvokeScript(script.ToHexString(), signers).SendAsync();
                var result = response.GetResult();
                
                if (result.HasStateFault() && !neoUnity.Config.AllowTransmissionOnFault)
                {
                    throw new TransactionException($"The VM exited due to the following exception: {result.Exception}");
                }
                
                return long.Parse(result.GasConsumed);
            }
            catch (Exception ex)
            {
                throw new TransactionException($"Failed to calculate system fee: {ex.Message}", ex);
            }
        }
        
        /// <summary>
        /// Calculates the network fee for this transaction.
        /// </summary>
        /// <returns>The network fee in GAS fractions</returns>
        private async Task<long> CalculateNetworkFee()
        {
            try
            {
                var tempTransaction = new NeoTransaction(
                    neoUnity: neoUnity,
                    version: version,
                    nonce: nonce,
                    validUntilBlock: validUntilBlock ?? 0,
                    signers: new List<Signer>(signers),
                    systemFee: 0,
                    networkFee: 0,
                    attributes: new List<TransactionAttribute>(attributes),
                    script: script,
                    witnesses: new List<Witness>()
                );
                
                bool hasAtLeastOneSigningAccount = false;
                
                // Add fake witnesses for fee calculation
                foreach (var signer in signers)
                {
                    if (signer is ContractSigner contractSigner)
                    {
                        tempTransaction.AddWitness(Witness.CreateContractWitness(contractSigner.VerifyParams));
                    }
                    else if (signer is AccountSigner accountSigner)
                    {
                        var fakeVerificationScript = await CreateFakeVerificationScript(accountSigner.Account);
                        tempTransaction.AddWitness(new Witness(new byte[0], fakeVerificationScript.Script));
                        hasAtLeastOneSigningAccount = true;
                    }
                }
                
                if (!hasAtLeastOneSigningAccount)
                {
                    throw new TransactionException("A transaction requires at least one signing account (i.e. an AccountSigner). None was provided.");
                }
                
                var response = await neoUnity.CalculateNetworkFee(tempTransaction.ToArray().ToHexString()).SendAsync();
                return response.GetResult().NetworkFee;
            }
            catch (Exception ex)
            {
                throw new TransactionException($"Failed to calculate network fee: {ex.Message}", ex);
            }
        }
        
        /// <summary>
        /// Gets the GAS balance of the sender account.
        /// </summary>
        /// <returns>The GAS balance in fractions</returns>
        private async Task<long> GetSenderGasBalance()
        {
            try
            {
                var response = await neoUnity.InvokeFunction(
                    GAS_TOKEN_HASH,
                    BALANCE_OF_FUNCTION,
                    new List<ContractParameter> { ContractParameter.Hash160(signers[0].SignerHash) },
                    new List<Signer>()
                ).SendAsync();
                
                var result = response.GetResult();
                return result.Stack[0].GetInteger();
            }
            catch (Exception ex)
            {
                throw new TransactionException($"Failed to get sender GAS balance: {ex.Message}", ex);
            }
        }
        
        /// <summary>
        /// Checks if the sender can cover the specified fees.
        /// </summary>
        /// <param name="fees">The fees to check</param>
        /// <returns>True if the sender can cover the fees</returns>
        private async Task<bool> CanSenderCoverFees(long fees)
        {
            try
            {
                var balance = await GetSenderGasBalance();
                return balance >= fees;
            }
            catch
            {
                return false;
            }
        }
        
        /// <summary>
        /// Creates a fake verification script for fee calculation purposes.
        /// </summary>
        /// <param name="account">The account to create a fake script for</param>
        /// <returns>The fake verification script</returns>
        private async Task<VerificationScript> CreateFakeVerificationScript(Account account)
        {
            try
            {
                var dummyKey = new ECPublicKey(DUMMY_PUB_KEY);
                
                if (account.IsMultiSig)
                {
                    var dummyKeys = new List<ECPublicKey>();
                    for (int i = 0; i < account.GetNumberOfParticipants(); i++)
                    {
                        dummyKeys.Add(dummyKey);
                    }
                    return new VerificationScript(dummyKeys, account.GetSigningThreshold());
                }
                
                return new VerificationScript(dummyKey);
            }
            catch (Exception ex)
            {
                throw new TransactionException($"Failed to create fake verification script: {ex.Message}", ex);
            }
        }
        
        #endregion
    }
    
    /// <summary>
    /// Exception thrown when transaction building or configuration fails.
    /// </summary>
    public class TransactionException : NeoUnityException
    {
        /// <summary>
        /// Creates a new transaction exception.
        /// </summary>
        /// <param name="message">The exception message</param>
        public TransactionException(string message) : base(message)
        {
        }
        
        /// <summary>
        /// Creates a new transaction exception with an inner exception.
        /// </summary>
        /// <param name="message">The exception message</param>
        /// <param name="innerException">The inner exception</param>
        public TransactionException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
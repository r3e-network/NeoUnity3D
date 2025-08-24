using System;
using System.Threading.Tasks;
using UnityEngine;
using Neo.Unity.SDK.Core;
using Neo.Unity.SDK.Types;
using Neo.Unity.SDK.Transaction;
using Neo.Unity.SDK.Wallet;

namespace Neo.Unity.SDK.Contracts.Native
{
    /// <summary>
    /// Represents the PolicyContract native contract and provides methods to invoke its functions.
    /// The Policy contract manages system-wide parameters such as fees and account blocking.
    /// </summary>
    [System.Serializable]
    public class PolicyContract : SmartContract
    {
        #region Constants

        private const string NAME = "PolicyContract";
        public static readonly Hash160 SCRIPT_HASH = SmartContract.CalcNativeContractHash(NAME);

        private const string GET_FEE_PER_BYTE = "getFeePerByte";
        private const string GET_EXEC_FEE_FACTOR = "getExecFeeFactor";
        private const string GET_STORAGE_PRICE = "getStoragePrice";
        private const string IS_BLOCKED = "isBlocked";
        private const string SET_FEE_PER_BYTE = "setFeePerByte";
        private const string SET_EXEC_FEE_FACTOR = "setExecFeeFactor";
        private const string SET_STORAGE_PRICE = "setStoragePrice";
        private const string BLOCK_ACCOUNT = "blockAccount";
        private const string UNBLOCK_ACCOUNT = "unblockAccount";

        #endregion

        #region Constructor

        /// <summary>
        /// Constructs a new PolicyContract instance that uses the given NeoUnity instance for invocations.
        /// </summary>
        /// <param name="neoUnity">The NeoUnity instance to use for invocations</param>
        public PolicyContract(NeoUnity neoUnity) : base(SCRIPT_HASH, neoUnity)
        {
        }

        /// <summary>
        /// Constructs a new PolicyContract instance using the singleton NeoUnity instance.
        /// </summary>
        public PolicyContract() : base(SCRIPT_HASH)
        {
        }

        #endregion

        #region Fee Management

        /// <summary>
        /// Gets the fee paid per byte of transaction.
        /// </summary>
        /// <returns>The system fee per transaction byte in GAS fractions</returns>
        public async Task<long> GetFeePerByte()
        {
            var result = await CallFunctionReturningInt(GET_FEE_PER_BYTE);
            
            if (NeoUnity.Config.EnableDebugLogging)
            {
                Debug.Log($"[PolicyContract] Fee per byte: {result} GAS fractions");
            }
            
            return result;
        }

        /// <summary>
        /// Gets the execution fee factor.
        /// This factor is used to calculate the execution fee for contract invocations.
        /// </summary>
        /// <returns>The execution fee factor</returns>
        public async Task<long> GetExecFeeFactor()
        {
            var result = await CallFunctionReturningInt(GET_EXEC_FEE_FACTOR);
            
            if (NeoUnity.Config.EnableDebugLogging)
            {
                Debug.Log($"[PolicyContract] Execution fee factor: {result}");
            }
            
            return result;
        }

        /// <summary>
        /// Gets the GAS price for one byte of smart contract storage.
        /// </summary>
        /// <returns>The storage price per byte in GAS fractions</returns>
        public async Task<long> GetStoragePrice()
        {
            var result = await CallFunctionReturningInt(GET_STORAGE_PRICE);
            
            if (NeoUnity.Config.EnableDebugLogging)
            {
                Debug.Log($"[PolicyContract] Storage price per byte: {result} GAS fractions");
            }
            
            return result;
        }

        /// <summary>
        /// Creates a transaction script to set the fee per byte and initializes a TransactionBuilder based on this script.
        /// This method can only be successfully invoked by the committee, i.e., the transaction has to be signed by the committee members.
        /// </summary>
        /// <param name="fee">The fee per byte in GAS fractions</param>
        /// <returns>A transaction builder ready for committee signing</returns>
        public async Task<TransactionBuilder> SetFeePerByte(long fee)
        {
            if (fee < 0)
            {
                throw new ArgumentException("Fee per byte cannot be negative.", nameof(fee));
            }

            if (NeoUnity.Config.EnableDebugLogging)
            {
                Debug.Log($"[PolicyContract] Setting fee per byte to: {fee} GAS fractions");
            }

            return await InvokeFunction(SET_FEE_PER_BYTE, ContractParameter.Integer(fee));
        }

        /// <summary>
        /// Creates a transaction script to set the execution fee factor and initializes a TransactionBuilder based on this script.
        /// This method can only be successfully invoked by the committee.
        /// </summary>
        /// <param name="factor">The execution fee factor</param>
        /// <returns>A transaction builder ready for committee signing</returns>
        public async Task<TransactionBuilder> SetExecFeeFactor(long factor)
        {
            if (factor < 0)
            {
                throw new ArgumentException("Execution fee factor cannot be negative.", nameof(factor));
            }

            if (NeoUnity.Config.EnableDebugLogging)
            {
                Debug.Log($"[PolicyContract] Setting execution fee factor to: {factor}");
            }

            return await InvokeFunction(SET_EXEC_FEE_FACTOR, ContractParameter.Integer(factor));
        }

        /// <summary>
        /// Creates a transaction script to set the storage price and initializes a TransactionBuilder based on this script.
        /// This method can only be successfully invoked by the committee.
        /// </summary>
        /// <param name="price">The storage price per byte in GAS fractions</param>
        /// <returns>A transaction builder ready for committee signing</returns>
        public async Task<TransactionBuilder> SetStoragePrice(long price)
        {
            if (price < 0)
            {
                throw new ArgumentException("Storage price cannot be negative.", nameof(price));
            }

            if (NeoUnity.Config.EnableDebugLogging)
            {
                Debug.Log($"[PolicyContract] Setting storage price to: {price} GAS fractions per byte");
            }

            return await InvokeFunction(SET_STORAGE_PRICE, ContractParameter.Integer(price));
        }

        #endregion

        #region Account Blocking

        /// <summary>
        /// Checks whether an account is blocked in the Neo network.
        /// Blocked accounts cannot send transactions or receive transfers.
        /// </summary>
        /// <param name="scriptHash">The script hash of the account to check</param>
        /// <returns>True if the account is blocked, otherwise false</returns>
        public async Task<bool> IsBlocked(Hash160 scriptHash)
        {
            if (scriptHash == null)
            {
                throw new ArgumentNullException(nameof(scriptHash));
            }

            var result = await CallFunctionReturningBool(IS_BLOCKED, ContractParameter.Hash160(scriptHash));
            
            if (NeoUnity.Config.EnableDebugLogging)
            {
                Debug.Log($"[PolicyContract] Account {scriptHash} blocked status: {result}");
            }
            
            return result;
        }

        /// <summary>
        /// Checks whether an account is blocked by Neo address.
        /// </summary>
        /// <param name="address">The Neo address to check</param>
        /// <returns>True if the account is blocked, otherwise false</returns>
        public async Task<bool> IsBlocked(string address)
        {
            if (string.IsNullOrEmpty(address))
            {
                throw new ArgumentException("Address cannot be null or empty.", nameof(address));
            }

            var scriptHash = Hash160.FromAddress(address);
            return await IsBlocked(scriptHash);
        }

        /// <summary>
        /// Checks whether an account is blocked.
        /// </summary>
        /// <param name="account">The account to check</param>
        /// <returns>True if the account is blocked, otherwise false</returns>
        public async Task<bool> IsBlocked(Account account)
        {
            if (account == null)
            {
                throw new ArgumentNullException(nameof(account));
            }

            return await IsBlocked(account.GetScriptHash());
        }

        /// <summary>
        /// Creates a transaction script to block an account in the Neo network and initializes a TransactionBuilder based on this script.
        /// This method can only be successfully invoked by the committee, i.e., the transaction has to be signed by the committee members.
        /// </summary>
        /// <param name="scriptHash">The script hash of the account to block</param>
        /// <returns>A transaction builder ready for committee signing</returns>
        public async Task<TransactionBuilder> BlockAccount(Hash160 scriptHash)
        {
            if (scriptHash == null)
            {
                throw new ArgumentNullException(nameof(scriptHash));
            }

            if (NeoUnity.Config.EnableDebugLogging)
            {
                Debug.Log($"[PolicyContract] Creating transaction to block account: {scriptHash}");
            }

            return await InvokeFunction(BLOCK_ACCOUNT, ContractParameter.Hash160(scriptHash));
        }

        /// <summary>
        /// Creates a transaction script to block an account by address and initializes a TransactionBuilder based on this script.
        /// This method can only be successfully invoked by the committee.
        /// </summary>
        /// <param name="address">The Neo address of the account to block</param>
        /// <returns>A transaction builder ready for committee signing</returns>
        public async Task<TransactionBuilder> BlockAccount(string address)
        {
            if (string.IsNullOrEmpty(address))
            {
                throw new ArgumentException("Address cannot be null or empty.", nameof(address));
            }

            var scriptHash = Hash160.FromAddress(address);
            return await BlockAccount(scriptHash);
        }

        /// <summary>
        /// Creates a transaction script to block an account and initializes a TransactionBuilder based on this script.
        /// This method can only be successfully invoked by the committee.
        /// </summary>
        /// <param name="account">The account to block</param>
        /// <returns>A transaction builder ready for committee signing</returns>
        public async Task<TransactionBuilder> BlockAccount(Account account)
        {
            if (account == null)
            {
                throw new ArgumentNullException(nameof(account));
            }

            return await BlockAccount(account.GetScriptHash());
        }

        /// <summary>
        /// Creates a transaction script to unblock an account in the Neo network and initializes a TransactionBuilder based on this script.
        /// This method can only be successfully invoked by the committee, i.e., the transaction has to be signed by the committee members.
        /// </summary>
        /// <param name="scriptHash">The script hash of the account to unblock</param>
        /// <returns>A transaction builder ready for committee signing</returns>
        public async Task<TransactionBuilder> UnblockAccount(Hash160 scriptHash)
        {
            if (scriptHash == null)
            {
                throw new ArgumentNullException(nameof(scriptHash));
            }

            if (NeoUnity.Config.EnableDebugLogging)
            {
                Debug.Log($"[PolicyContract] Creating transaction to unblock account: {scriptHash}");
            }

            return await InvokeFunction(UNBLOCK_ACCOUNT, ContractParameter.Hash160(scriptHash));
        }

        /// <summary>
        /// Creates a transaction script to unblock an account by address and initializes a TransactionBuilder based on this script.
        /// This method can only be successfully invoked by the committee.
        /// </summary>
        /// <param name="address">The Neo address of the account to unblock</param>
        /// <returns>A transaction builder ready for committee signing</returns>
        public async Task<TransactionBuilder> UnblockAccount(string address)
        {
            if (string.IsNullOrEmpty(address))
            {
                throw new ArgumentException("Address cannot be null or empty.", nameof(address));
            }

            var scriptHash = Hash160.FromAddress(address);
            return await UnblockAccount(scriptHash);
        }

        /// <summary>
        /// Creates a transaction script to unblock an account and initializes a TransactionBuilder based on this script.
        /// This method can only be successfully invoked by the committee.
        /// </summary>
        /// <param name="account">The account to unblock</param>
        /// <returns>A transaction builder ready for committee signing</returns>
        public async Task<TransactionBuilder> UnblockAccount(Account account)
        {
            if (account == null)
            {
                throw new ArgumentNullException(nameof(account));
            }

            return await UnblockAccount(account.GetScriptHash());
        }

        #endregion

        #region Unity Integration Helpers

        /// <summary>
        /// Gets all current policy settings in a single call for display purposes.
        /// </summary>
        /// <returns>A PolicySettings object containing all current policy values</returns>
        public async Task<PolicySettings> GetPolicySettings()
        {
            var feePerByteTask = GetFeePerByte();
            var execFeeFactorTask = GetExecFeeFactor();
            var storagePriceTask = GetStoragePrice();

            await Task.WhenAll(feePerByteTask, execFeeFactorTask, storagePriceTask);

            return new PolicySettings
            {
                FeePerByte = feePerByteTask.Result,
                ExecFeeFactor = execFeeFactorTask.Result,
                StoragePrice = storagePriceTask.Result
            };
        }

        /// <summary>
        /// Estimates the network fee for a transaction based on current policy settings.
        /// </summary>
        /// <param name="transactionSize">The size of the transaction in bytes</param>
        /// <returns>The estimated network fee in GAS fractions</returns>
        public async Task<long> EstimateNetworkFee(int transactionSize)
        {
            if (transactionSize <= 0)
            {
                throw new ArgumentException("Transaction size must be positive.", nameof(transactionSize));
            }

            var feePerByte = await GetFeePerByte();
            var networkFee = feePerByte * transactionSize;

            if (NeoUnity.Config.EnableDebugLogging)
            {
                Debug.Log($"[PolicyContract] Estimated network fee for {transactionSize} bytes: {networkFee} GAS fractions");
            }

            return networkFee;
        }

        /// <summary>
        /// Checks if an account can send transactions (not blocked and has sufficient gas for fees).
        /// </summary>
        /// <param name="account">The account to check</param>
        /// <param name="estimatedTxSize">The estimated transaction size in bytes (default: 250)</param>
        /// <returns>A validation result with details about the account's ability to transact</returns>
        public async Task<AccountTransactionValidation> ValidateAccountForTransaction(Account account, int estimatedTxSize = 250)
        {
            if (account == null)
            {
                throw new ArgumentNullException(nameof(account));
            }

            var scriptHash = account.GetScriptHash();
            
            // Check if account is blocked
            var isBlockedTask = IsBlocked(scriptHash);
            
            // Estimate network fee
            var networkFeeTask = EstimateNetworkFee(estimatedTxSize);
            
            // Check GAS balance
            var gasToken = new GasToken(NeoUnity);
            var gasBalanceTask = gasToken.GetBalanceOf(scriptHash);

            await Task.WhenAll(isBlockedTask, networkFeeTask, gasBalanceTask);

            var isBlocked = isBlockedTask.Result;
            var networkFee = networkFeeTask.Result;
            var gasBalance = gasBalanceTask.Result;

            return new AccountTransactionValidation
            {
                CanTransact = !isBlocked && gasBalance >= networkFee,
                IsBlocked = isBlocked,
                HasSufficientGas = gasBalance >= networkFee,
                GasBalance = gasBalance,
                EstimatedNetworkFee = networkFee,
                RemainingGasAfterFee = Math.Max(0, gasBalance - networkFee)
            };
        }

        #endregion
    }

    /// <summary>
    /// Represents the current policy settings of the Neo network.
    /// </summary>
    [System.Serializable]
    public class PolicySettings
    {
        /// <summary>The fee per byte of transaction in GAS fractions</summary>
        public long FeePerByte { get; set; }

        /// <summary>The execution fee factor</summary>
        public long ExecFeeFactor { get; set; }

        /// <summary>The storage price per byte in GAS fractions</summary>
        public long StoragePrice { get; set; }

        /// <summary>
        /// Gets the fee per byte in decimal GAS format.
        /// </summary>
        /// <returns>Fee per byte in decimal GAS</returns>
        public decimal GetFeePerByteGas()
        {
            return GasToken.FromGasFractions(FeePerByte);
        }

        /// <summary>
        /// Gets the storage price in decimal GAS format.
        /// </summary>
        /// <returns>Storage price in decimal GAS</returns>
        public decimal GetStoragePriceGas()
        {
            return GasToken.FromGasFractions(StoragePrice);
        }

        public override string ToString()
        {
            return $"PolicySettings(FeePerByte: {GetFeePerByteGas():F8} GAS, " +
                   $"ExecFeeFactor: {ExecFeeFactor}, " +
                   $"StoragePrice: {GetStoragePriceGas():F8} GAS)";
        }
    }

    /// <summary>
    /// Represents the validation result for an account's ability to send transactions.
    /// </summary>
    [System.Serializable]
    public class AccountTransactionValidation
    {
        /// <summary>Whether the account can send transactions</summary>
        public bool CanTransact { get; set; }

        /// <summary>Whether the account is blocked</summary>
        public bool IsBlocked { get; set; }

        /// <summary>Whether the account has sufficient GAS for the estimated fee</summary>
        public bool HasSufficientGas { get; set; }

        /// <summary>The account's current GAS balance in fractions</summary>
        public long GasBalance { get; set; }

        /// <summary>The estimated network fee in GAS fractions</summary>
        public long EstimatedNetworkFee { get; set; }

        /// <summary>The remaining GAS after paying the fee</summary>
        public long RemainingGasAfterFee { get; set; }

        /// <summary>
        /// Gets the GAS balance in decimal format.
        /// </summary>
        /// <returns>GAS balance in decimal GAS</returns>
        public decimal GetGasBalanceDecimal()
        {
            return GasToken.FromGasFractions(GasBalance);
        }

        /// <summary>
        /// Gets the estimated network fee in decimal format.
        /// </summary>
        /// <returns>Network fee in decimal GAS</returns>
        public decimal GetNetworkFeeDecimal()
        {
            return GasToken.FromGasFractions(EstimatedNetworkFee);
        }

        /// <summary>
        /// Gets the remaining GAS after fee in decimal format.
        /// </summary>
        /// <returns>Remaining GAS in decimal format</returns>
        public decimal GetRemainingGasDecimal()
        {
            return GasToken.FromGasFractions(RemainingGasAfterFee);
        }

        /// <summary>
        /// Gets a user-friendly reason why the account cannot transact, if applicable.
        /// </summary>
        /// <returns>Reason string, or null if the account can transact</returns>
        public string GetBlockingReason()
        {
            if (CanTransact) return null;

            if (IsBlocked) return "Account is blocked by network policy";
            if (!HasSufficientGas) return $"Insufficient GAS balance. Required: {GetNetworkFeeDecimal():F8} GAS, Available: {GetGasBalanceDecimal():F8} GAS";

            return "Unknown reason";
        }

        public override string ToString()
        {
            var status = CanTransact ? "Can Transact" : $"Cannot Transact ({GetBlockingReason()})";
            return $"AccountTransactionValidation({status}, GAS: {GetGasBalanceDecimal():F8}, Fee: {GetNetworkFeeDecimal():F8})";
        }
    }
}
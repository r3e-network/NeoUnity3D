using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using Neo.Unity.SDK.Core;
using Neo.Unity.SDK.Types;
using Neo.Unity.SDK.Crypto;
using Neo.Unity.SDK.Utils;
using Newtonsoft.Json;

namespace Neo.Unity.SDK.Wallet
{
    /// <summary>
    /// The Neo wallet manages a collection of accounts and provides methods for account operations,
    /// balance checking, and NEP-6 wallet file compatibility.
    /// Unity-optimized wallet implementation with ScriptableObject support.
    /// </summary>
    [System.Serializable]
    public class NeoWallet
    {
        #region Constants
        
        /// <summary>Default wallet name</summary>
        private const string DEFAULT_WALLET_NAME = "NeoUnityWallet";
        
        /// <summary>Current NEP-6 wallet version</summary>
        public const string CURRENT_VERSION = "3.0";
        
        #endregion
        
        #region Private Fields
        
        [SerializeField]
        private string name;
        
        [SerializeField]
        private string version;
        
        [SerializeField]
        private ScryptParams scryptParams;
        
        [SerializeField]
        private List<Account> accounts;
        
        [SerializeField]
        private string defaultAccountAddress;
        
        // Dictionary for fast account lookup by script hash (not serialized by Unity)
        private Dictionary<Hash160, Account> accountsMap;
        
        #endregion
        
        #region Properties
        
        /// <summary>The name of this wallet</summary>
        public string Name 
        { 
            get => name ?? DEFAULT_WALLET_NAME; 
            set => name = value; 
        }
        
        /// <summary>The version of this wallet</summary>
        public string Version 
        { 
            get => version ?? CURRENT_VERSION; 
            set => version = value; 
        }
        
        /// <summary>The scrypt parameters for encryption/decryption</summary>
        public ScryptParams ScryptParams 
        { 
            get => scryptParams ?? ScryptParams.Default; 
            set => scryptParams = value; 
        }
        
        /// <summary>All accounts in this wallet</summary>
        public IReadOnlyList<Account> Accounts 
        { 
            get 
            {
                EnsureAccountsMapInitialized();
                return accountsMap.OrderBy(kvp => kvp.Key.ToString()).Select(kvp => kvp.Value).ToList().AsReadOnly();
            }
        }
        
        /// <summary>The default account of this wallet</summary>
        public Account DefaultAccount 
        { 
            get 
            {
                if (string.IsNullOrEmpty(defaultAccountAddress))
                    return null;
                    
                try
                {
                    var scriptHash = Hash160.FromAddress(defaultAccountAddress);
                    EnsureAccountsMapInitialized();
                    return accountsMap.TryGetValue(scriptHash, out var account) ? account : null;
                }
                catch
                {
                    return null;
                }
            }
        }
        
        /// <summary>Number of accounts in this wallet</summary>
        public int AccountCount => accounts?.Count ?? 0;
        
        #endregion
        
        #region Constructors
        
        /// <summary>
        /// Creates a new empty wallet with default settings.
        /// </summary>
        public NeoWallet()
        {
            name = DEFAULT_WALLET_NAME;
            version = CURRENT_VERSION;
            scryptParams = ScryptParams.Default;
            accounts = new List<Account>();
            InitializeAccountsMap();
        }
        
        /// <summary>
        /// Creates a new wallet with the specified name.
        /// </summary>
        /// <param name="walletName">The name for the wallet</param>
        public NeoWallet(string walletName) : this()
        {
            name = walletName ?? DEFAULT_WALLET_NAME;
        }
        
        #endregion
        
        #region Account Management
        
        /// <summary>
        /// Sets the given account as the default account of this wallet.
        /// </summary>
        /// <param name="account">The new default account</param>
        /// <returns>This wallet for method chaining</returns>
        /// <exception cref="ArgumentException">If account is not in this wallet</exception>
        public NeoWallet SetDefaultAccount(Account account)
        {
            if (account == null)
                throw new ArgumentNullException(nameof(account));
                
            return SetDefaultAccount(account.GetScriptHash());
        }
        
        /// <summary>
        /// Sets the account with the given script hash as the default account of this wallet.
        /// </summary>
        /// <param name="accountHash">The script hash of the new default account</param>
        /// <returns>This wallet for method chaining</returns>
        /// <exception cref="ArgumentException">If account is not in this wallet</exception>
        public NeoWallet SetDefaultAccount(Hash160 accountHash)
        {
            if (accountHash == null)
                throw new ArgumentNullException(nameof(accountHash));
                
            EnsureAccountsMapInitialized();
            if (!accountsMap.ContainsKey(accountHash))
            {
                throw new ArgumentException($"Cannot set default account on wallet. Wallet does not contain the account with script hash {accountHash}.");
            }
            
            defaultAccountAddress = accountHash.ToAddress();
            
            // Update the account's wallet reference
            if (accountsMap.TryGetValue(accountHash, out var account))
            {
                account.SetWallet(this);
            }
            
            return this;
        }
        
        /// <summary>
        /// Checks whether an account is the default account in the wallet.
        /// </summary>
        /// <param name="account">The account to check</param>
        /// <returns>True if the account is the default account</returns>
        public bool IsDefault(Account account)
        {
            if (account == null)
                return false;
                
            return IsDefault(account.GetScriptHash());
        }
        
        /// <summary>
        /// Checks whether an account is the default account in the wallet.
        /// </summary>
        /// <param name="accountHash">The script hash of the account to check</param>
        /// <returns>True if the account is the default account</returns>
        public bool IsDefault(Hash160 accountHash)
        {
            if (accountHash == null || DefaultAccount == null)
                return false;
                
            return DefaultAccount.GetScriptHash().Equals(accountHash);
        }
        
        /// <summary>
        /// Adds the given accounts to this wallet if they don't already exist.
        /// </summary>
        /// <param name="accounts">The accounts to add</param>
        /// <returns>This wallet for method chaining</returns>
        /// <exception cref="ArgumentException">If an account is already in another wallet</exception>
        public NeoWallet AddAccounts(params Account[] accounts)
        {
            return AddAccounts(accounts?.ToList() ?? new List<Account>());
        }
        
        /// <summary>
        /// Adds the given accounts to this wallet if they don't already exist.
        /// </summary>
        /// <param name="accounts">The accounts to add</param>
        /// <returns>This wallet for method chaining</returns>
        /// <exception cref="ArgumentException">If an account is already in another wallet</exception>
        public NeoWallet AddAccounts(List<Account> accounts)
        {
            if (accounts == null)
                return this;
                
            EnsureAccountsMapInitialized();
            
            // Filter out accounts that already exist in this wallet
            var newAccounts = accounts.Where(acc => !accountsMap.ContainsKey(acc.GetScriptHash())).ToList();
            
            // Check if any new account is already in another wallet
            var accountInOtherWallet = newAccounts.FirstOrDefault(acc => acc.Wallet != null && acc.Wallet != this);
            if (accountInOtherWallet != null)
            {
                throw new ArgumentException($"The account {accountInOtherWallet.Address} is already contained in another wallet. Please remove this account from its containing wallet before adding it to another wallet.");
            }
            
            // Add new accounts
            foreach (var account in newAccounts)
            {
                var scriptHash = account.GetScriptHash();
                accountsMap[scriptHash] = account;
                this.accounts.Add(account);
                account.SetWallet(this);
                
                // Set first account as default if no default is set
                if (string.IsNullOrEmpty(defaultAccountAddress))
                {
                    defaultAccountAddress = account.Address;
                }
            }
            
            return this;
        }
        
        /// <summary>
        /// Removes the account from this wallet.
        /// If there is only one account in the wallet, it cannot be removed.
        /// </summary>
        /// <param name="account">The account to remove</param>
        /// <returns>True if an account was removed, false if not found</returns>
        /// <exception cref="InvalidOperationException">If trying to remove the last account</exception>
        public bool RemoveAccount(Account account)
        {
            if (account == null)
                return false;
                
            return RemoveAccount(account.GetScriptHash());
        }
        
        /// <summary>
        /// Removes the account from this wallet.
        /// If there is only one account in the wallet, it cannot be removed.
        /// </summary>
        /// <param name="accountHash">The script hash of the account to remove</param>
        /// <returns>True if an account was removed, false if not found</returns>
        /// <exception cref="InvalidOperationException">If trying to remove the last account</exception>
        public bool RemoveAccount(Hash160 accountHash)
        {
            if (accountHash == null)
                return false;
                
            EnsureAccountsMapInitialized();
            
            if (!accountsMap.ContainsKey(accountHash))
                return false;
                
            if (accountsMap.Count <= 1)
            {
                throw new InvalidOperationException($"The account {accountHash.ToAddress()} is the only account in the wallet. It cannot be removed.");
            }
            
            // Remove from both collections
            if (accountsMap.TryGetValue(accountHash, out var account))
            {
                account.SetWallet(null);
                accountsMap.Remove(accountHash);
                accounts.Remove(account);
                
                // If this was the default account, set a new default
                if (IsDefault(accountHash))
                {
                    var newDefaultHash = accountsMap.Keys.First();
                    defaultAccountAddress = newDefaultHash.ToAddress();
                }
                
                return true;
            }
            
            return false;
        }
        
        /// <summary>
        /// Checks if this wallet contains an account with the given script hash.
        /// </summary>
        /// <param name="accountHash">The script hash to check</param>
        /// <returns>True if the wallet contains the account</returns>
        public bool HoldsAccount(Hash160 accountHash)
        {
            if (accountHash == null)
                return false;
                
            EnsureAccountsMapInitialized();
            return accountsMap.ContainsKey(accountHash);
        }
        
        /// <summary>
        /// Gets the account with the given script hash if it exists in this wallet.
        /// </summary>
        /// <param name="accountHash">The script hash of the account</param>
        /// <returns>The account if found, null otherwise</returns>
        public Account GetAccount(Hash160 accountHash)
        {
            if (accountHash == null)
                return null;
                
            EnsureAccountsMapInitialized();
            return accountsMap.TryGetValue(accountHash, out var account) ? account : null;
        }
        
        /// <summary>
        /// Gets all accounts in this wallet.
        /// </summary>
        /// <returns>List of all accounts</returns>
        public List<Account> GetAccounts()
        {
            return Accounts.ToList();
        }
        
        #endregion
        
        #region Encryption/Decryption
        
        /// <summary>
        /// Decrypts all accounts in this wallet using the provided password.
        /// </summary>
        /// <param name="password">The password to use for decryption</param>
        public async Task DecryptAllAccounts(string password)
        {
            if (string.IsNullOrEmpty(password))
                throw new ArgumentException("Password cannot be null or empty.", nameof(password));
            
            var tasks = new List<Task>();
            foreach (var account in Accounts)
            {
                tasks.Add(account.DecryptPrivateKey(password, ScryptParams));
            }
            
            await Task.WhenAll(tasks);
            
            if (NeoUnity.Instance.Config.EnableDebugLogging)
            {
                Debug.Log($"[NeoWallet] Decrypted {AccountCount} accounts");
            }
        }
        
        /// <summary>
        /// Encrypts all accounts in this wallet using the provided password.
        /// </summary>
        /// <param name="password">The password to use for encryption</param>
        public async Task EncryptAllAccounts(string password)
        {
            if (string.IsNullOrEmpty(password))
                throw new ArgumentException("Password cannot be null or empty.", nameof(password));
            
            var tasks = new List<Task>();
            foreach (var account in Accounts)
            {
                tasks.Add(account.EncryptPrivateKey(password, ScryptParams));
            }
            
            await Task.WhenAll(tasks);
            
            if (NeoUnity.Instance.Config.EnableDebugLogging)
            {
                Debug.Log($"[NeoWallet] Encrypted {AccountCount} accounts");
            }
        }
        
        #endregion
        
        #region Balance Operations
        
        /// <summary>
        /// Gets the balances of all NEP-17 tokens that this wallet owns.
        /// The token amounts are returned in token fractions.
        /// Requires a Neo node with the RpcNep17Tracker plugin installed.
        /// </summary>
        /// <param name="neoUnity">The NeoUnity instance to use for RPC calls</param>
        /// <returns>Dictionary mapping token script hashes to token amounts</returns>
        public async Task<Dictionary<Hash160, long>> GetNep17TokenBalances(NeoUnity neoUnity = null)
        {
            neoUnity = neoUnity ?? NeoUnity.Instance;
            var balances = new Dictionary<Hash160, long>();
            
            var tasks = Accounts.Select(async account => 
            {
                try
                {
                    return await account.GetNep17Balances(neoUnity);
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"[NeoWallet] Failed to get NEP-17 balances for account {account.Address}: {ex.Message}");
                    return new Dictionary<Hash160, long>();
                }
            });
            
            var accountBalances = await Task.WhenAll(tasks);
            
            foreach (var accountBalance in accountBalances)
            {
                foreach (var kvp in accountBalance)
                {
                    if (balances.ContainsKey(kvp.Key))
                    {
                        balances[kvp.Key] += kvp.Value;
                    }
                    else
                    {
                        balances[kvp.Key] = kvp.Value;
                    }
                }
            }
            
            return balances;
        }
        
        #endregion
        
        #region NEP-6 Wallet File Operations
        
        /// <summary>
        /// Converts this wallet to a NEP-6 wallet format.
        /// </summary>
        /// <returns>The NEP-6 wallet representation</returns>
        public async Task<NEP6Wallet> ToNEP6Wallet()
        {
            var nep6Accounts = new List<NEP6Account>();
            
            foreach (var account in Accounts)
            {
                nep6Accounts.Add(await account.ToNEP6Account());
            }
            
            return new NEP6Wallet(Name, Version, ScryptParams, nep6Accounts, null);
        }
        
        /// <summary>
        /// Saves this wallet as a NEP-6 wallet file.
        /// </summary>
        /// <param name="filePath">The file path to save to</param>
        /// <returns>This wallet for method chaining</returns>
        public async Task<NeoWallet> SaveNEP6Wallet(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
                throw new ArgumentException("File path cannot be null or empty.", nameof(filePath));
            
            var nep6Wallet = await ToNEP6Wallet();
            var json = JsonConvert.SerializeObject(nep6Wallet, JsonSettings.Default);
            
            // Ensure directory exists
            var directory = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            
            // If path is directory, append wallet name
            if (Directory.Exists(filePath))
            {
                filePath = Path.Combine(filePath, $"{Name}.json");
            }
            
            await File.WriteAllTextAsync(filePath, json);
            
            if (NeoUnity.Instance.Config.EnableDebugLogging)
            {
                Debug.Log($"[NeoWallet] Saved NEP-6 wallet to: {filePath}");
            }
            
            return this;
        }
        
        /// <summary>
        /// Loads a wallet from a NEP-6 wallet file.
        /// </summary>
        /// <param name="filePath">The path to the NEP-6 wallet file</param>
        /// <returns>The loaded wallet</returns>
        public static async Task<NeoWallet> FromNEP6WalletFile(string filePath)
        {
            if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
                throw new ArgumentException("Invalid file path or file does not exist.", nameof(filePath));
            
            var json = await File.ReadAllTextAsync(filePath);
            var nep6Wallet = JsonConvert.DeserializeObject<NEP6Wallet>(json, JsonSettings.Default);
            
            return await FromNEP6Wallet(nep6Wallet);
        }
        
        /// <summary>
        /// Creates a wallet from a NEP-6 wallet object.
        /// </summary>
        /// <param name="nep6Wallet">The NEP-6 wallet</param>
        /// <returns>The created wallet</returns>
        public static async Task<NeoWallet> FromNEP6Wallet(NEP6Wallet nep6Wallet)
        {
            if (nep6Wallet == null)
                throw new ArgumentNullException(nameof(nep6Wallet));
            
            var accounts = new List<Account>();
            foreach (var nep6Account in nep6Wallet.Accounts)
            {
                accounts.Add(await Account.FromNEP6Account(nep6Account));
            }
            
            var defaultAccount = nep6Wallet.Accounts.FirstOrDefault(acc => acc.IsDefault);
            if (defaultAccount == null)
            {
                throw new ArgumentException("The NEP-6 wallet does not contain any default account.");
            }
            
            var wallet = new NeoWallet(nep6Wallet.Name)
            {
                version = nep6Wallet.Version,
                scryptParams = nep6Wallet.Scrypt
            };
            
            wallet.AddAccounts(accounts);
            
            var defaultAccountObj = accounts.FirstOrDefault(acc => acc.Address == defaultAccount.Address);
            if (defaultAccountObj != null)
            {
                wallet.SetDefaultAccount(defaultAccountObj);
            }
            
            return wallet;
        }
        
        #endregion
        
        #region Static Factory Methods
        
        /// <summary>
        /// Creates a new wallet with one randomly generated account.
        /// </summary>
        /// <returns>The new wallet</returns>
        public static async Task<NeoWallet> Create()
        {
            var account = await Account.Create();
            var wallet = new NeoWallet();
            wallet.AddAccounts(account);
            wallet.SetDefaultAccount(account);
            return wallet;
        }
        
        /// <summary>
        /// Creates a new wallet with one account and encrypts it with the provided password.
        /// </summary>
        /// <param name="password">The password to encrypt the account with</param>
        /// <returns>The new wallet</returns>
        public static async Task<NeoWallet> Create(string password)
        {
            var wallet = await Create();
            await wallet.EncryptAllAccounts(password);
            return wallet;
        }
        
        /// <summary>
        /// Creates a new wallet with one account, encrypts it, and saves it as a NEP-6 file.
        /// </summary>
        /// <param name="password">The password to encrypt the account with</param>
        /// <param name="filePath">The path to save the wallet file</param>
        /// <returns>The new wallet</returns>
        public static async Task<NeoWallet> Create(string password, string filePath)
        {
            var wallet = await Create(password);
            await wallet.SaveNEP6Wallet(filePath);
            return wallet;
        }
        
        /// <summary>
        /// Creates a new wallet with the given accounts.
        /// The first account is set as the default account.
        /// </summary>
        /// <param name="accounts">The accounts to add to the wallet</param>
        /// <returns>The new wallet</returns>
        public static NeoWallet WithAccounts(params Account[] accounts)
        {
            return WithAccounts(accounts?.ToList() ?? new List<Account>());
        }
        
        /// <summary>
        /// Creates a new wallet with the given accounts.
        /// The first account is set as the default account.
        /// </summary>
        /// <param name="accounts">The accounts to add to the wallet</param>
        /// <returns>The new wallet</returns>
        public static NeoWallet WithAccounts(List<Account> accounts)
        {
            if (accounts == null || accounts.Count == 0)
            {
                throw new ArgumentException("No accounts provided to initialize a wallet.");
            }
            
            var wallet = new NeoWallet();
            wallet.AddAccounts(accounts);
            wallet.SetDefaultAccount(accounts[0]);
            return wallet;
        }
        
        #endregion
        
        #region Private Helper Methods
        
        /// <summary>
        /// Ensures the accounts map is initialized and synchronized with the accounts list.
        /// </summary>
        private void EnsureAccountsMapInitialized()
        {
            if (accountsMap == null)
            {
                InitializeAccountsMap();
            }
        }
        
        /// <summary>
        /// Initializes the accounts map from the accounts list.
        /// </summary>
        private void InitializeAccountsMap()
        {
            accountsMap = new Dictionary<Hash160, Account>();
            
            if (accounts != null)
            {
                foreach (var account in accounts)
                {
                    try
                    {
                        var scriptHash = account.GetScriptHash();
                        accountsMap[scriptHash] = account;
                        account.SetWallet(this);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogWarning($"[NeoWallet] Failed to initialize account {account?.Address}: {ex.Message}");
                    }
                }
            }
        }
        
        #endregion
        
        #region Unity Integration
        
        /// <summary>
        /// Called by Unity when the object is deserialized.
        /// </summary>
        [System.Runtime.Serialization.OnDeserialized]
        private void OnDeserialized(System.Runtime.Serialization.StreamingContext context)
        {
            InitializeAccountsMap();
        }
        
        /// <summary>
        /// Unity callback for validation in the Inspector.
        /// </summary>
        private void OnValidate()
        {
            if (string.IsNullOrEmpty(name))
            {
                name = DEFAULT_WALLET_NAME;
            }
            
            if (string.IsNullOrEmpty(version))
            {
                version = CURRENT_VERSION;
            }
            
            if (scryptParams == null)
            {
                scryptParams = ScryptParams.Default;
            }
            
            if (accounts == null)
            {
                accounts = new List<Account>();
            }
        }
        
        #endregion
        
        #region String Representation
        
        /// <summary>
        /// Returns a string representation of this wallet.
        /// </summary>
        /// <returns>String representation</returns>
        public override string ToString()
        {
            return $"NeoWallet(Name: {Name}, Accounts: {AccountCount}, Default: {DefaultAccount?.Address ?? "None"})";
        }
        
        #endregion
    }
}
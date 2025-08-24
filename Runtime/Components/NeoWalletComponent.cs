using System;
using System.Threading.Tasks;
using UnityEngine;
using Neo.Unity.SDK.Core;
using Neo.Unity.SDK.Wallet;
using Neo.Unity.SDK.Types;

namespace Neo.Unity.SDK.Components
{
    /// <summary>
    /// Unity MonoBehaviour component for wallet operations in games.
    /// Provides a game-friendly interface for wallet creation, management, and operations.
    /// </summary>
    public class NeoWalletComponent : MonoBehaviour
    {
        #region Inspector Configuration
        
        [Header("Wallet Configuration")]
        [SerializeField]
        [Tooltip("The wallet file path (optional - can be set via code)")]
        private string walletFilePath;
        
        [SerializeField]
        [Tooltip("Auto-load wallet on Start()")]
        private bool autoLoadWallet = false;
        
        [SerializeField]
        [Tooltip("Wallet name for new wallets")]
        private string walletName = "Unity Game Wallet";
        
        [Header("Security Settings")]
        [SerializeField]
        [Tooltip("Enable wallet encryption (requires password)")]
        private bool enableEncryption = true;
        
        [SerializeField]
        [Tooltip("Lock wallet after specified minutes of inactivity (0 = no auto-lock)")]
        private float autoLockMinutes = 10f;
        
        [Header("Display Settings")]
        [SerializeField]
        [Tooltip("Show wallet info in console")]
        private bool showWalletInfo = true;
        
        #endregion
        
        #region Events
        
        /// <summary>Fired when a wallet is loaded or created</summary>
        public event Action<NeoWallet> OnWalletLoaded;
        
        /// <summary>Fired when wallet loading fails</summary>
        public event Action<string> OnWalletLoadFailed;
        
        /// <summary>Fired when a new account is created</summary>
        public event Action<Account> OnAccountCreated;
        
        /// <summary>Fired when wallet is locked/unlocked</summary>
        public event Action<bool> OnWalletLockChanged;
        
        #endregion
        
        #region Private Fields
        
        private NeoWallet wallet;
        private DateTime lastActivity;
        private bool isLocked = false;
        
        #endregion
        
        #region Properties
        
        /// <summary>The current active wallet</summary>
        public NeoWallet Wallet => wallet;
        
        /// <summary>Whether a wallet is loaded</summary>
        public bool HasWallet => wallet != null;
        
        /// <summary>Whether the wallet is currently locked</summary>
        public bool IsLocked => isLocked;
        
        /// <summary>The default account in the current wallet</summary>
        public Account DefaultAccount => wallet?.DefaultAccount;
        
        /// <summary>The address of the default account</summary>
        public string DefaultAddress => DefaultAccount?.Address;
        
        #endregion
        
        #region Unity Lifecycle
        
        private async void Start()
        {
            if (autoLoadWallet && !string.IsNullOrEmpty(walletFilePath))
            {
                await LoadWallet(walletFilePath);
            }
            
            lastActivity = DateTime.UtcNow;
            
            if (autoLockMinutes > 0)
            {
                InvokeRepeating(nameof(CheckAutoLock), 60f, 60f); // Check every minute
            }
        }
        
        private void OnDestroy()
        {
            CancelInvoke();
        }
        
        #endregion
        
        #region Wallet Operations
        
        /// <summary>
        /// Creates a new wallet with a random account.
        /// </summary>
        /// <param name="walletName">Optional wallet name</param>
        /// <returns>The created wallet</returns>
        public async Task<NeoWallet> CreateNewWallet(string walletName = null)
        {
            try
            {
                wallet = await NeoWallet.Create();
                wallet.Name = walletName ?? this.walletName;
                
                UpdateActivity();
                
                if (showWalletInfo)
                {
                    Debug.Log($"[NeoWalletComponent] Created new wallet: {wallet.Name}");
                    Debug.Log($"[NeoWalletComponent] Default address: {wallet.DefaultAccount.Address}");
                }
                
                OnWalletLoaded?.Invoke(wallet);
                return wallet;
            }
            catch (Exception ex)
            {
                var errorMessage = $"Failed to create wallet: {ex.Message}";
                Debug.LogError($"[NeoWalletComponent] {errorMessage}");
                OnWalletLoadFailed?.Invoke(errorMessage);
                throw;
            }
        }
        
        /// <summary>
        /// Loads a wallet from a NEP-6 file.
        /// </summary>
        /// <param name="filePath">Path to the wallet file</param>
        /// <returns>The loaded wallet</returns>
        public async Task<NeoWallet> LoadWallet(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
                throw new ArgumentException("File path cannot be null or empty.", nameof(filePath));
            
            try
            {
                wallet = await NeoWallet.FromNEP6WalletFile(filePath);
                walletFilePath = filePath;
                
                UpdateActivity();
                
                if (showWalletInfo)
                {
                    Debug.Log($"[NeoWalletComponent] Loaded wallet: {wallet.Name} from {filePath}");
                    Debug.Log($"[NeoWalletComponent] Accounts: {wallet.AccountCount}");
                }
                
                OnWalletLoaded?.Invoke(wallet);
                return wallet;
            }
            catch (Exception ex)
            {
                var errorMessage = $"Failed to load wallet from {filePath}: {ex.Message}";
                Debug.LogError($"[NeoWalletComponent] {errorMessage}");
                OnWalletLoadFailed?.Invoke(errorMessage);
                throw;
            }
        }
        
        /// <summary>
        /// Saves the current wallet to a NEP-6 file.
        /// </summary>
        /// <param name="filePath">Path to save the wallet file</param>
        public async Task SaveWallet(string filePath = null)
        {
            if (wallet == null)
                throw new InvalidOperationException("No wallet to save.");
            
            var saveFilePath = filePath ?? walletFilePath;
            if (string.IsNullOrEmpty(saveFilePath))
                throw new ArgumentException("No file path specified for saving wallet.");
            
            try
            {
                await wallet.SaveNEP6Wallet(saveFilePath);
                walletFilePath = saveFilePath;
                
                if (showWalletInfo)
                {
                    Debug.Log($"[NeoWalletComponent] Saved wallet to: {saveFilePath}");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[NeoWalletComponent] Failed to save wallet: {ex.Message}");
                throw;
            }
        }
        
        /// <summary>
        /// Creates a new account in the current wallet.
        /// </summary>
        /// <returns>The created account</returns>
        public async Task<Account> CreateNewAccount()
        {
            if (wallet == null)
                throw new InvalidOperationException("No wallet available. Create or load a wallet first.");
            
            try
            {
                var account = await Account.Create();
                wallet.AddAccounts(account);
                
                UpdateActivity();
                
                if (showWalletInfo)
                {
                    Debug.Log($"[NeoWalletComponent] Created new account: {account.Address}");
                }
                
                OnAccountCreated?.Invoke(account);
                return account;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[NeoWalletComponent] Failed to create account: {ex.Message}");
                throw;
            }
        }
        
        #endregion
        
        #region Balance Operations
        
        /// <summary>
        /// Gets the NEO balance of the default account.
        /// </summary>
        /// <returns>NEO balance in fractions</returns>
        public async Task<long> GetNeoBalance()
        {
            return await GetTokenBalance(NeoToken.SCRIPT_HASH);
        }
        
        /// <summary>
        /// Gets the GAS balance of the default account.
        /// </summary>
        /// <returns>GAS balance in fractions</returns>
        public async Task<long> GetGasBalance()
        {
            return await GetTokenBalance(GasToken.SCRIPT_HASH);
        }
        
        /// <summary>
        /// Gets the balance of a specific token for the default account.
        /// </summary>
        /// <param name="tokenHash">The token contract hash</param>
        /// <returns>Token balance in fractions</returns>
        public async Task<long> GetTokenBalance(Hash160 tokenHash)
        {
            if (wallet?.DefaultAccount == null)
                throw new InvalidOperationException("No wallet or default account available.");
            
            UpdateActivity();
            return await wallet.DefaultAccount.GetTokenBalance(tokenHash);
        }
        
        #endregion
        
        #region Security Methods
        
        /// <summary>
        /// Locks the wallet to prevent operations.
        /// </summary>
        public void LockWallet()
        {
            if (wallet != null)
            {
                foreach (var account in wallet.GetAccounts())
                {
                    account.Lock();
                }
                
                isLocked = true;
                
                if (showWalletInfo)
                {
                    Debug.Log("[NeoWalletComponent] Wallet locked");
                }
                
                OnWalletLockChanged?.Invoke(true);
            }
        }
        
        /// <summary>
        /// Unlocks the wallet for operations.
        /// </summary>
        /// <param name="password">The wallet password (if encrypted)</param>
        public async Task UnlockWallet(string password = null)
        {
            if (wallet == null)
                return;
            
            try
            {
                if (enableEncryption && !string.IsNullOrEmpty(password))
                {
                    await wallet.DecryptAllAccounts(password);
                }
                
                foreach (var account in wallet.GetAccounts())
                {
                    account.Unlock();
                }
                
                isLocked = false;
                UpdateActivity();
                
                if (showWalletInfo)
                {
                    Debug.Log("[NeoWalletComponent] Wallet unlocked");
                }
                
                OnWalletLockChanged?.Invoke(false);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[NeoWalletComponent] Failed to unlock wallet: {ex.Message}");
                throw;
            }
        }
        
        /// <summary>
        /// Updates the last activity timestamp.
        /// </summary>
        private void UpdateActivity()
        {
            lastActivity = DateTime.UtcNow;
        }
        
        /// <summary>
        /// Checks if the wallet should be auto-locked due to inactivity.
        /// </summary>
        private void CheckAutoLock()
        {
            if (autoLockMinutes > 0 && !isLocked && wallet != null)
            {
                var inactiveMinutes = (DateTime.UtcNow - lastActivity).TotalMinutes;
                
                if (inactiveMinutes >= autoLockMinutes)
                {
                    LockWallet();
                    
                    if (showWalletInfo)
                    {
                        Debug.Log($"[NeoWalletComponent] Auto-locked wallet after {inactiveMinutes:F1} minutes of inactivity");
                    }
                }
            }
        }
        
        #endregion
        
        #region Inspector Context Menu Methods
        
        [ContextMenu("Show Wallet Info")]
        private void ShowWalletInfo()
        {
            if (wallet == null)
            {
                Debug.Log("[NeoWalletComponent] No wallet loaded");
                return;
            }
            
            Debug.Log($"[NeoWalletComponent] Wallet Info:\n" +
                     $"Name: {wallet.Name}\n" +
                     $"Accounts: {wallet.AccountCount}\n" +
                     $"Default Account: {wallet.DefaultAccount?.Address ?? "None"}\n" +
                     $"Locked: {isLocked}");
        }
        
        [ContextMenu("Show Account Addresses")]
        private void ShowAccountAddresses()
        {
            if (wallet == null)
            {
                Debug.Log("[NeoWalletComponent] No wallet loaded");
                return;
            }
            
            Debug.Log($"[NeoWalletComponent] Account Addresses ({wallet.AccountCount}):");
            
            var accounts = wallet.GetAccounts();
            for (int i = 0; i < accounts.Count; i++)
            {
                var account = accounts[i];
                var marker = account.IsDefault ? " (Default)" : "";
                var lockStatus = account.IsLocked ? " [Locked]" : "";
                Debug.Log($"  {i + 1}. {account.Address}{marker}{lockStatus}");
            }
        }
        
        #endregion
    }
}
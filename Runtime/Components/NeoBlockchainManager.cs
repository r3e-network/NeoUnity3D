using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Neo.Unity.SDK.Core;
using Neo.Unity.SDK.Wallet;
using Neo.Unity.SDK.Types;
using Neo.Unity.SDK.Contracts;

namespace Neo.Unity.SDK.Components
{
    /// <summary>
    /// Unity MonoBehaviour component for managing Neo blockchain connections and operations.
    /// Provides a drag-and-drop solution for integrating Neo blockchain functionality into Unity games.
    /// </summary>
    public class NeoBlockchainManager : MonoBehaviour
    {
        #region Inspector Configuration
        
        [Header("Neo Configuration")]
        [SerializeField]
        [Tooltip("Neo Unity SDK configuration asset")]
        private NeoUnityConfig neoConfig;
        
        [SerializeField]
        [Tooltip("Auto-initialize on Start()")]
        private bool autoInitialize = true;
        
        [SerializeField]
        [Tooltip("Log blockchain events to console")]
        private bool enableEventLogging = true;
        
        [Header("Wallet Management")]
        [SerializeField]
        [Tooltip("Auto-create wallet if none exists")]
        private bool autoCreateWallet = true;
        
        [SerializeField]
        [Tooltip("Default wallet name for auto-created wallets")]
        private string defaultWalletName = "GameWallet";
        
        [Header("Performance Settings")]
        [SerializeField]
        [Range(5f, 60f)]
        [Tooltip("Block polling interval in seconds")]
        private float blockPollingInterval = 15f;
        
        [SerializeField]
        [Range(1, 10)]
        [Tooltip("Maximum concurrent blockchain requests")]
        private int maxConcurrentRequests = 5;
        
        #endregion
        
        #region Events
        
        /// <summary>Fired when the SDK is successfully initialized</summary>
        public event Action OnInitialized;
        
        /// <summary>Fired when initialization fails</summary>
        public event Action<string> OnInitializationFailed;
        
        /// <summary>Fired when a new block is detected</summary>
        public event Action<int> OnNewBlock;
        
        /// <summary>Fired when wallet balance changes</summary>
        public event Action<Hash160, long> OnBalanceChanged;
        
        /// <summary>Fired when a transaction is confirmed</summary>
        public event Action<Hash256> OnTransactionConfirmed;
        
        #endregion
        
        #region Private Fields
        
        private bool isInitialized = false;
        private NeoWallet currentWallet;
        private int lastKnownBlockHeight = 0;
        private readonly Dictionary<Hash160, long> lastKnownBalances = new Dictionary<Hash160, long>();
        
        #endregion
        
        #region Properties
        
        /// <summary>Whether the Neo SDK is initialized and ready for use</summary>
        public bool IsInitialized => isInitialized && NeoUnity.Instance.IsInitialized;
        
        /// <summary>The current active wallet</summary>
        public NeoWallet CurrentWallet => currentWallet;
        
        /// <summary>The current Neo configuration</summary>
        public NeoUnityConfig Config => neoConfig;
        
        /// <summary>The current block height</summary>
        public int CurrentBlockHeight => lastKnownBlockHeight;
        
        #endregion
        
        #region Unity Lifecycle
        
        private async void Start()
        {
            if (autoInitialize)
            {
                await InitializeBlockchain();
            }
        }
        
        private void OnDestroy()
        {
            StopAllCoroutines();
        }
        
        private void OnValidate()
        {
            // Validate configuration in Inspector
            if (neoConfig == null)
            {
                Debug.LogWarning("[NeoBlockchainManager] No Neo configuration assigned. Create one via: Assets → Create → Neo Unity SDK → Configuration");
            }
            
            blockPollingInterval = Mathf.Clamp(blockPollingInterval, 5f, 60f);
            maxConcurrentRequests = Mathf.Clamp(maxConcurrentRequests, 1, 10);
            
            if (string.IsNullOrEmpty(defaultWalletName))
            {
                defaultWalletName = "GameWallet";
            }
        }
        
        #endregion
        
        #region Initialization
        
        /// <summary>
        /// Initializes the Neo blockchain connection.
        /// </summary>
        /// <returns>True if initialization was successful</returns>
        public async Task<bool> InitializeBlockchain()
        {
            if (isInitialized)
            {
                Debug.LogWarning("[NeoBlockchainManager] Already initialized");
                return true;
            }
            
            try
            {
                // Use provided config or create default
                var config = neoConfig ?? NeoUnityConfig.CreateMainnetConfig();
                
                // Update polling interval from Inspector
                config.SetPollingInterval((int)(blockPollingInterval * 1000));
                
                // Initialize Neo Unity SDK
                var success = await NeoUnity.Instance.Initialize(config);
                
                if (!success)
                {
                    throw new Exception("Failed to initialize Neo Unity SDK");
                }
                
                // Auto-create wallet if needed
                if (autoCreateWallet && currentWallet == null)
                {
                    await CreateNewWallet();
                }
                
                // Start monitoring
                StartCoroutine(MonitorBlockchain());
                
                isInitialized = true;
                
                if (enableEventLogging)
                {
                    Debug.Log($"[NeoBlockchainManager] Successfully initialized with node: {config.NodeUrl}");
                }
                
                OnInitialized?.Invoke();
                return true;
            }
            catch (Exception ex)
            {
                var errorMessage = $"Failed to initialize blockchain: {ex.Message}";
                Debug.LogError($"[NeoBlockchainManager] {errorMessage}");
                
                OnInitializationFailed?.Invoke(errorMessage);
                return false;
            }
        }
        
        #endregion
        
        #region Wallet Management
        
        /// <summary>
        /// Creates a new wallet with a random account.
        /// </summary>
        /// <returns>The created wallet</returns>
        public async Task<NeoWallet> CreateNewWallet()
        {
            try
            {
                currentWallet = await NeoWallet.Create();
                currentWallet.Name = defaultWalletName;
                
                if (enableEventLogging)
                {
                    Debug.Log($"[NeoBlockchainManager] Created new wallet: {currentWallet.DefaultAccount.Address}");
                }
                
                return currentWallet;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[NeoBlockchainManager] Failed to create wallet: {ex.Message}");
                throw;
            }
        }
        
        /// <summary>
        /// Loads a wallet from a NEP-6 file.
        /// </summary>
        /// <param name="filePath">Path to the NEP-6 wallet file</param>
        /// <returns>The loaded wallet</returns>
        public async Task<NeoWallet> LoadWallet(string filePath)
        {
            try
            {
                currentWallet = await NeoWallet.FromNEP6WalletFile(filePath);
                
                if (enableEventLogging)
                {
                    Debug.Log($"[NeoBlockchainManager] Loaded wallet: {currentWallet.Name} with {currentWallet.AccountCount} accounts");
                }
                
                return currentWallet;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[NeoBlockchainManager] Failed to load wallet: {ex.Message}");
                throw;
            }
        }
        
        /// <summary>
        /// Sets the current wallet.
        /// </summary>
        /// <param name="wallet">The wallet to set as current</param>
        public void SetCurrentWallet(NeoWallet wallet)
        {
            currentWallet = wallet;
            
            if (enableEventLogging && wallet != null)
            {
                Debug.Log($"[NeoBlockchainManager] Set current wallet: {wallet.Name}");
            }
        }
        
        #endregion
        
        #region Blockchain Monitoring
        
        /// <summary>
        /// Coroutine for monitoring blockchain state.
        /// </summary>
        private System.Collections.IEnumerator MonitorBlockchain()
        {
            while (isInitialized && Application.isPlaying)
            {
                try
                {
                    // Check for new blocks
                    _ = CheckForNewBlocks();
                    
                    // Check for balance changes
                    if (currentWallet != null)
                    {
                        _ = CheckForBalanceChanges();
                    }
                }
                catch (Exception ex)
                {
                    if (enableEventLogging)
                    {
                        Debug.LogWarning($"[NeoBlockchainManager] Monitoring error: {ex.Message}");
                    }
                }
                
                yield return new WaitForSeconds(blockPollingInterval);
            }
        }
        
        /// <summary>
        /// Checks for new blocks and fires events.
        /// </summary>
        private async Task CheckForNewBlocks()
        {
            try
            {
                var response = await NeoUnity.Instance.GetBlockCount().SendAsync();
                var currentHeight = response.GetResult();
                
                if (currentHeight > lastKnownBlockHeight)
                {
                    if (enableEventLogging && lastKnownBlockHeight > 0)
                    {
                        Debug.Log($"[NeoBlockchainManager] New block detected: {currentHeight}");
                    }
                    
                    lastKnownBlockHeight = currentHeight;
                    OnNewBlock?.Invoke(currentHeight);
                }
            }
            catch (Exception ex)
            {
                if (enableEventLogging)
                {
                    Debug.LogWarning($"[NeoBlockchainManager] Failed to check block height: {ex.Message}");
                }
            }
        }
        
        /// <summary>
        /// Checks for balance changes and fires events.
        /// </summary>
        private async Task CheckForBalanceChanges()
        {
            try
            {
                var balances = await currentWallet.GetNep17TokenBalances();
                
                foreach (var kvp in balances)
                {
                    var tokenHash = kvp.Key;
                    var currentBalance = kvp.Value;
                    
                    if (lastKnownBalances.TryGetValue(tokenHash, out var lastBalance))
                    {
                        if (currentBalance != lastBalance)
                        {
                            if (enableEventLogging)
                            {
                                Debug.Log($"[NeoBlockchainManager] Balance changed for {tokenHash}: {lastBalance} → {currentBalance}");
                            }
                            
                            OnBalanceChanged?.Invoke(tokenHash, currentBalance);
                        }
                    }
                    
                    lastKnownBalances[tokenHash] = currentBalance;
                }
            }
            catch (Exception ex)
            {
                if (enableEventLogging)
                {
                    Debug.LogWarning($"[NeoBlockchainManager] Failed to check balances: {ex.Message}");
                }
            }
        }
        
        #endregion
        
        #region Public Utility Methods
        
        /// <summary>
        /// Gets the current block height.
        /// </summary>
        /// <returns>The current block height</returns>
        public async Task<int> GetCurrentBlockHeight()
        {
            EnsureInitialized();
            var response = await NeoUnity.Instance.GetBlockCount().SendAsync();
            return response.GetResult();
        }
        
        /// <summary>
        /// Gets the balance of a specific token for the default account.
        /// </summary>
        /// <param name="tokenHash">The token contract hash</param>
        /// <returns>The balance in token fractions</returns>
        public async Task<long> GetTokenBalance(Hash160 tokenHash)
        {
            EnsureInitialized();
            
            if (currentWallet?.DefaultAccount == null)
            {
                throw new InvalidOperationException("No wallet or default account available.");
            }
            
            return await currentWallet.DefaultAccount.GetTokenBalance(tokenHash);
        }
        
        /// <summary>
        /// Creates a fungible token instance for the specified contract.
        /// </summary>
        /// <param name="tokenHash">The token contract hash</param>
        /// <returns>The fungible token instance</returns>
        public FungibleToken GetFungibleToken(Hash160 tokenHash)
        {
            EnsureInitialized();
            return new FungibleToken(tokenHash);
        }
        
        /// <summary>
        /// Validates that the SDK is initialized.
        /// </summary>
        private void EnsureInitialized()
        {
            if (!isInitialized)
            {
                throw new InvalidOperationException("Neo blockchain manager is not initialized. Call InitializeBlockchain() first.");
            }
        }
        
        #endregion
        
        #region Inspector Methods
        
        /// <summary>
        /// Context menu method for manual initialization.
        /// </summary>
        [ContextMenu("Initialize Blockchain")]
        private async void InitializeFromInspector()
        {
            await InitializeBlockchain();
        }
        
        /// <summary>
        /// Context menu method for creating a new wallet.
        /// </summary>
        [ContextMenu("Create New Wallet")]
        private async void CreateWalletFromInspector()
        {
            await CreateNewWallet();
        }
        
        /// <summary>
        /// Context menu method for checking current status.
        /// </summary>
        [ContextMenu("Check Status")]
        private async void CheckStatus()
        {
            if (!isInitialized)
            {
                Debug.Log("[NeoBlockchainManager] Status: Not Initialized");
                return;
            }
            
            try
            {
                var blockHeight = await GetCurrentBlockHeight();
                var walletInfo = currentWallet != null ? $"{currentWallet.Name} ({currentWallet.AccountCount} accounts)" : "No wallet";
                
                Debug.Log($"[NeoBlockchainManager] Status: Initialized\n" +
                         $"Block Height: {blockHeight}\n" +
                         $"Wallet: {walletInfo}\n" +
                         $"Node: {neoConfig?.NodeUrl ?? "Unknown"}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[NeoBlockchainManager] Status check failed: {ex.Message}");
            }
        }
        
        #endregion
    }
}
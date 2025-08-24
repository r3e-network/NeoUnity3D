using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEditor;
using Neo.Unity.SDK.Core;
using Neo.Unity.SDK.Wallet;
using Neo.Unity.SDK.Types;
using Neo.Unity.SDK.Contracts;

namespace Neo.Unity.SDK.Editor.Tools
{
    /// <summary>
    /// Unity Editor window for Neo blockchain development tools.
    /// Provides a comprehensive interface for wallet management, contract interaction,
    /// and blockchain development within the Unity Editor.
    /// </summary>
    public class NeoUnityWindow : EditorWindow
    {
        #region Window Management
        
        [MenuItem("Window/Neo Unity SDK/Neo Developer Tools")]
        public static void ShowWindow()
        {
            var window = GetWindow<NeoUnityWindow>("Neo Unity SDK");
            window.minSize = new Vector2(400, 600);
            window.Show();
        }
        
        #endregion
        
        #region Private Fields
        
        private Vector2 scrollPosition;
        private int selectedTab = 0;
        private readonly string[] tabNames = { "Wallet", "Contracts", "Network", "Tools" };
        
        // Wallet tab fields
        private NeoWallet currentWallet;
        private string walletPassword = "";
        private string walletName = "Unity Developer Wallet";
        private string walletFilePath = "";
        private bool showWalletDetails = false;
        
        // Contract tab fields
        private string contractHash = "";
        private string contractFunction = "";
        private string contractParams = "";
        private string contractResult = "";
        
        // Network tab fields
        private NeoUnityConfig networkConfig;
        private string customNodeUrl = "";
        private bool showNetworkDetails = false;
        
        // Tools tab fields
        private string addressToValidate = "";
        private string validationResult = "";
        private string hashToConvert = "";
        private string conversionResult = "";
        
        #endregion
        
        #region Unity Editor GUI
        
        private void OnGUI()
        {
            EditorGUILayout.Space(10);
            
            // Header
            EditorGUILayout.LabelField("Neo Unity SDK Developer Tools", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Blockchain development tools for Unity", EditorStyles.helpBox);
            
            EditorGUILayout.Space(10);
            
            // Tab selection
            selectedTab = GUILayout.Toolbar(selectedTab, tabNames);
            
            EditorGUILayout.Space(10);
            
            // Scroll view for content
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            
            switch (selectedTab)
            {
                case 0: DrawWalletTab(); break;
                case 1: DrawContractsTab(); break;
                case 2: DrawNetworkTab(); break;
                case 3: DrawToolsTab(); break;
            }
            
            EditorGUILayout.EndScrollView();
        }
        
        #endregion
        
        #region Wallet Tab
        
        private void DrawWalletTab()
        {
            EditorGUILayout.LabelField("Wallet Management", EditorStyles.boldLabel);
            
            // Current wallet status
            if (currentWallet != null)
            {
                EditorGUILayout.HelpBox($"Wallet: {currentWallet.Name} ({currentWallet.AccountCount} accounts)", MessageType.Info);
                
                if (currentWallet.DefaultAccount != null)
                {
                    EditorGUILayout.LabelField("Default Account:", currentWallet.DefaultAccount.Address);
                }
                
                showWalletDetails = EditorGUILayout.Foldout(showWalletDetails, "Wallet Details");
                if (showWalletDetails)
                {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.LabelField("Version:", currentWallet.Version);
                    
                    foreach (var account in currentWallet.GetAccounts())
                    {
                        var label = account.IsDefault ? $"{account.Address} (Default)" : account.Address;
                        var lockStatus = account.IsLocked ? " [Locked]" : "";
                        EditorGUILayout.LabelField("Account:", label + lockStatus);
                    }
                    EditorGUI.indentLevel--;
                }
                
                EditorGUILayout.Space(5);
                
                // Wallet actions
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Add Account"))
                {
                    _ = CreateNewAccount();
                }
                if (GUILayout.Button("Save Wallet"))
                {
                    _ = SaveCurrentWallet();
                }
                EditorGUILayout.EndHorizontal();
            }
            else
            {
                EditorGUILayout.HelpBox("No wallet loaded", MessageType.Warning);
            }
            
            EditorGUILayout.Space(10);
            
            // Create new wallet
            EditorGUILayout.LabelField("Create New Wallet", EditorStyles.boldLabel);
            walletName = EditorGUILayout.TextField("Wallet Name:", walletName);
            walletPassword = EditorGUILayout.PasswordField("Password (optional):", walletPassword);
            
            if (GUILayout.Button("Create New Wallet"))
            {
                _ = CreateNewWallet();
            }
            
            EditorGUILayout.Space(10);
            
            // Load existing wallet
            EditorGUILayout.LabelField("Load Existing Wallet", EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal();
            walletFilePath = EditorGUILayout.TextField("File Path:", walletFilePath);
            if (GUILayout.Button("Browse", GUILayout.Width(60)))
            {
                var path = EditorUtility.OpenFilePanel("Select NEP-6 Wallet", "", "json");
                if (!string.IsNullOrEmpty(path))
                {
                    walletFilePath = path;
                }
            }
            EditorGUILayout.EndHorizontal();
            
            EditorGUI.BeginDisabledGroup(string.IsNullOrEmpty(walletFilePath));
            if (GUILayout.Button("Load Wallet"))
            {
                _ = LoadWallet();
            }
            EditorGUI.EndDisabledGroup();
        }
        
        /// <summary>
        /// Creates a new wallet.
        /// </summary>
        private async Task CreateNewWallet()
        {
            try
            {
                if (string.IsNullOrEmpty(walletPassword))
                {
                    currentWallet = await NeoWallet.Create();
                }
                else
                {
                    currentWallet = await NeoWallet.Create(walletPassword);
                }
                
                currentWallet.Name = walletName;
                
                EditorUtility.DisplayDialog("Success", $"Wallet created successfully!\nDefault Address: {currentWallet.DefaultAccount.Address}", "OK");
                Repaint();
            }
            catch (Exception ex)
            {
                EditorUtility.DisplayDialog("Error", $"Failed to create wallet: {ex.Message}", "OK");
            }
        }
        
        /// <summary>
        /// Loads a wallet from file.
        /// </summary>
        private async Task LoadWallet()
        {
            try
            {
                currentWallet = await NeoWallet.FromNEP6WalletFile(walletFilePath);
                EditorUtility.DisplayDialog("Success", $"Wallet loaded successfully!\nName: {currentWallet.Name}\nAccounts: {currentWallet.AccountCount}", "OK");
                Repaint();
            }
            catch (Exception ex)
            {
                EditorUtility.DisplayDialog("Error", $"Failed to load wallet: {ex.Message}", "OK");
            }
        }
        
        /// <summary>
        /// Creates a new account in the current wallet.
        /// </summary>
        private async Task CreateNewAccount()
        {
            if (currentWallet == null)
                return;
            
            try
            {
                var account = await Account.Create();
                currentWallet.AddAccounts(account);
                
                EditorUtility.DisplayDialog("Success", $"New account created!\nAddress: {account.Address}", "OK");
                Repaint();
            }
            catch (Exception ex)
            {
                EditorUtility.DisplayDialog("Error", $"Failed to create account: {ex.Message}", "OK");
            }
        }
        
        /// <summary>
        /// Saves the current wallet.
        /// </summary>
        private async Task SaveCurrentWallet()
        {
            if (currentWallet == null)
                return;
            
            try
            {
                var path = EditorUtility.SaveFilePanel("Save Wallet", "", $"{currentWallet.Name}.json", "json");
                if (!string.IsNullOrEmpty(path))
                {
                    await currentWallet.SaveNEP6Wallet(path);
                    EditorUtility.DisplayDialog("Success", $"Wallet saved to: {path}", "OK");
                }
            }
            catch (Exception ex)
            {
                EditorUtility.DisplayDialog("Error", $"Failed to save wallet: {ex.Message}", "OK");
            }
        }
        
        #endregion
        
        #region Contracts Tab
        
        private void DrawContractsTab()
        {
            EditorGUILayout.LabelField("Smart Contract Interaction", EditorStyles.boldLabel);
            
            // Contract hash input
            contractHash = EditorGUILayout.TextField("Contract Hash:", contractHash);
            
            // Function name
            contractFunction = EditorGUILayout.TextField("Function Name:", contractFunction);
            
            // Parameters (simplified - JSON format)
            EditorGUILayout.LabelField("Parameters (JSON array):");
            contractParams = EditorGUILayout.TextArea(contractParams, GUILayout.Height(80));
            
            EditorGUILayout.Space(5);
            
            // Action buttons
            EditorGUILayout.BeginHorizontal();
            
            EditorGUI.BeginDisabledGroup(string.IsNullOrEmpty(contractHash) || string.IsNullOrEmpty(contractFunction));
            if (GUILayout.Button("Call Function"))
            {
                _ = CallContractFunction();
            }
            EditorGUI.EndDisabledGroup();
            
            if (GUILayout.Button("Get Contract Info"))
            {
                _ = GetContractInfo();
            }
            
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Space(10);
            
            // Results
            if (!string.IsNullOrEmpty(contractResult))
            {
                EditorGUILayout.LabelField("Result:", EditorStyles.boldLabel);
                EditorGUILayout.TextArea(contractResult, GUILayout.Height(120));
                
                if (GUILayout.Button("Clear Result"))
                {
                    contractResult = "";
                }
            }
            
            EditorGUILayout.Space(10);
            
            // Common contracts shortcuts
            EditorGUILayout.LabelField("Quick Contract Access", EditorStyles.boldLabel);
            
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("NEO Token"))
            {
                contractHash = NeoToken.SCRIPT_HASH.ToString();
                contractFunction = "symbol";
                contractParams = "[]";
            }
            if (GUILayout.Button("GAS Token"))
            {
                contractHash = GasToken.SCRIPT_HASH.ToString();
                contractFunction = "decimals";
                contractParams = "[]";
            }
            EditorGUILayout.EndHorizontal();
        }
        
        /// <summary>
        /// Calls a contract function with the specified parameters.
        /// </summary>
        private async Task CallContractFunction()
        {
            try
            {
                if (!NeoUnity.Instance.IsInitialized)
                {
                    await NeoUnity.Instance.Initialize();
                }
                
                var hash = new Hash160(contractHash);
                var response = await NeoUnity.Instance.InvokeFunction(hash, contractFunction, new System.Collections.Generic.List<Types.ContractParameter>()).SendAsync();
                var result = response.GetResult();
                
                contractResult = $"Gas Consumed: {result.GasConsumed}\n";
                contractResult += $"State: {result.State}\n";
                contractResult += $"Stack Items: {result.Stack.Count}\n\n";
                
                for (int i = 0; i < result.Stack.Count; i++)
                {
                    var item = result.Stack[i];
                    contractResult += $"Stack[{i}]: {item.Type} = {item.GetValue()}\n";
                }
                
                Repaint();
            }
            catch (Exception ex)
            {
                contractResult = $"Error: {ex.Message}";
                Repaint();
            }
        }
        
        /// <summary>
        /// Gets information about a contract.
        /// </summary>
        private async Task GetContractInfo()
        {
            try
            {
                if (!NeoUnity.Instance.IsInitialized)
                {
                    await NeoUnity.Instance.Initialize();
                }
                
                var hash = new Hash160(contractHash);
                var response = await NeoUnity.Instance.GetContractState(hash).SendAsync();
                var contractState = response.GetResult();
                
                contractResult = $"Contract Name: {contractState.Manifest.Name}\n";
                contractResult += $"Author: {contractState.Manifest.Author}\n";
                contractResult += $"Version: {contractState.Manifest.SupportedStandards}\n";
                contractResult += $"Hash: {contractState.Hash}\n";
                contractResult += $"Update Counter: {contractState.UpdateCounter}\n\n";
                
                contractResult += "Methods:\n";
                foreach (var method in contractState.Manifest.Abi.Methods)
                {
                    contractResult += $"- {method.Name}({string.Join(", ", method.Parameters.ConvertAll(p => p.Type.ToString()))})\n";
                }
                
                Repaint();
            }
            catch (Exception ex)
            {
                contractResult = $"Error: {ex.Message}";
                Repaint();
            }
        }
        
        #endregion
        
        #region Network Tab
        
        private void DrawNetworkTab()
        {
            EditorGUILayout.LabelField("Network Configuration", EditorStyles.boldLabel);
            
            // Current config
            networkConfig = EditorGUILayout.ObjectField("Neo Config:", networkConfig, typeof(NeoUnityConfig), false) as NeoUnityConfig;
            
            if (networkConfig != null)
            {
                EditorGUILayout.HelpBox($"Current Node: {networkConfig.NodeUrl}", MessageType.Info);
                
                showNetworkDetails = EditorGUILayout.Foldout(showNetworkDetails, "Network Details");
                if (showNetworkDetails)
                {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.LabelField("Network Magic:", networkConfig.NetworkMagic?.ToString() ?? "Auto-detect");
                    EditorGUILayout.LabelField("Block Interval:", $"{networkConfig.BlockInterval} ms");
                    EditorGUILayout.LabelField("Polling Interval:", $"{networkConfig.PollingInterval} ms");
                    EditorGUILayout.LabelField("Debug Logging:", networkConfig.EnableDebugLogging.ToString());
                    EditorGUI.indentLevel--;
                }
            }
            
            EditorGUILayout.Space(10);
            
            // Quick network selection
            EditorGUILayout.LabelField("Quick Network Setup", EditorStyles.boldLabel);
            
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Mainnet"))
            {
                CreateNetworkConfig("https://mainnet1.neo.coz.io:443", 860833102);
            }
            if (GUILayout.Button("Testnet"))
            {
                CreateNetworkConfig("https://testnet1.neo.coz.io:443", 894710606);
            }
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Space(10);
            
            // Custom node
            EditorGUILayout.LabelField("Custom Node", EditorStyles.boldLabel);
            customNodeUrl = EditorGUILayout.TextField("Node URL:", customNodeUrl);
            
            EditorGUI.BeginDisabledGroup(string.IsNullOrEmpty(customNodeUrl));
            if (GUILayout.Button("Connect to Custom Node"))
            {
                CreateNetworkConfig(customNodeUrl, null);
            }
            EditorGUI.EndDisabledGroup();
            
            EditorGUILayout.Space(10);
            
            // Network testing
            EditorGUILayout.LabelField("Network Testing", EditorStyles.boldLabel);
            
            if (GUILayout.Button("Test Connection"))
            {
                _ = TestNetworkConnection();
            }
            
            if (GUILayout.Button("Get Network Info"))
            {
                _ = GetNetworkInfo();
            }
        }
        
        /// <summary>
        /// Creates a network configuration asset.
        /// </summary>
        /// <param name="nodeUrl">The node URL</param>
        /// <param name="networkMagic">The network magic number</param>
        private void CreateNetworkConfig(string nodeUrl, int? networkMagic)
        {
            var config = ScriptableObject.CreateInstance<NeoUnityConfig>();
            config.NodeUrl = nodeUrl;
            
            if (networkMagic.HasValue)
            {
                config.SetNetworkMagic(networkMagic.Value);
            }
            
            var isMainnet = nodeUrl.Contains("mainnet");
            var networkName = isMainnet ? "Mainnet" : "Testnet";
            var assetPath = $"Assets/Neo{networkName}Config.asset";
            
            AssetDatabase.CreateAsset(config, assetPath);
            AssetDatabase.SaveAssets();
            
            networkConfig = config;
            EditorUtility.FocusProjectWindow();
            Selection.activeObject = config;
            
            EditorUtility.DisplayDialog("Success", $"Created {networkName} configuration at {assetPath}", "OK");
        }
        
        /// <summary>
        /// Tests the network connection.
        /// </summary>
        private async Task TestNetworkConnection()
        {
            try
            {
                var config = networkConfig ?? NeoUnityConfig.CreateTestnetConfig();
                var success = await NeoUnity.Instance.Initialize(config);
                
                if (success)
                {
                    var version = await NeoUnity.Instance.GetVersion().SendAsync();
                    var versionInfo = version.GetResult();
                    
                    EditorUtility.DisplayDialog("Connection Test", 
                        $"Connection successful!\n" +
                        $"Node: {versionInfo.UserAgent}\n" +
                        $"Protocol: {versionInfo.Protocol.Network}", "OK");
                }
                else
                {
                    EditorUtility.DisplayDialog("Connection Test", "Connection failed!", "OK");
                }
            }
            catch (Exception ex)
            {
                EditorUtility.DisplayDialog("Connection Test", $"Connection failed: {ex.Message}", "OK");
            }
        }
        
        /// <summary>
        /// Gets network information.
        /// </summary>
        private async Task GetNetworkInfo()
        {
            try
            {
                var config = networkConfig ?? NeoUnityConfig.CreateTestnetConfig();
                await NeoUnity.Instance.Initialize(config);
                
                var versionTask = NeoUnity.Instance.GetVersion().SendAsync();
                var blockCountTask = NeoUnity.Instance.GetBlockCount().SendAsync();
                var peersTask = NeoUnity.Instance.GetPeers().SendAsync();
                
                await Task.WhenAll(versionTask, blockCountTask, peersTask);
                
                var version = versionTask.Result.GetResult();
                var blockCount = blockCountTask.Result.GetResult();
                var peers = peersTask.Result.GetResult();
                
                var info = $"Network Information:\n\n" +
                          $"Node: {version.UserAgent}\n" +
                          $"Protocol Version: {version.Protocol.Network}\n" +
                          $"Block Height: {blockCount:N0}\n" +
                          $"Connected Peers: {peers.Connected.Count}\n" +
                          $"Unconnected Peers: {peers.Unconnected.Count}";
                
                EditorUtility.DisplayDialog("Network Info", info, "OK");
            }
            catch (Exception ex)
            {
                EditorUtility.DisplayDialog("Network Info", $"Failed to get network info: {ex.Message}", "OK");
            }
        }
        
        #endregion
        
        #region Tools Tab
        
        private void DrawToolsTab()
        {
            EditorGUILayout.LabelField("Developer Utilities", EditorStyles.boldLabel);
            
            // Address validation
            EditorGUILayout.LabelField("Address Validation", EditorStyles.boldLabel);
            addressToValidate = EditorGUILayout.TextField("Address:", addressToValidate);
            
            if (GUILayout.Button("Validate Address"))
            {
                ValidateAddress();
            }
            
            if (!string.IsNullOrEmpty(validationResult))
            {
                var messageType = validationResult.Contains("Valid") ? MessageType.Info : MessageType.Warning;
                EditorGUILayout.HelpBox(validationResult, messageType);
            }
            
            EditorGUILayout.Space(10);
            
            // Hash conversion
            EditorGUILayout.LabelField("Hash Conversion", EditorStyles.boldLabel);
            hashToConvert = EditorGUILayout.TextField("Hash/Address:", hashToConvert);
            
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Hash160 → Address"))
            {
                ConvertHashToAddress();
            }
            if (GUILayout.Button("Address → Hash160"))
            {
                ConvertAddressToHash();
            }
            EditorGUILayout.EndHorizontal();
            
            if (!string.IsNullOrEmpty(conversionResult))
            {
                EditorGUILayout.HelpBox(conversionResult, MessageType.Info);
                
                if (GUILayout.Button("Copy to Clipboard"))
                {
                    EditorGUIUtility.systemCopyBuffer = conversionResult;
                }
            }
            
            EditorGUILayout.Space(10);
            
            // SDK Information
            EditorGUILayout.LabelField("SDK Information", EditorStyles.boldLabel);
            
            if (GUILayout.Button("Show SDK Status"))
            {
                ShowSDKStatus();
            }
            
            if (GUILayout.Button("Open Documentation"))
            {
                Application.OpenURL("https://docs.neo.org/unity");
            }
            
            if (GUILayout.Button("Open GitHub Repository"))
            {
                Application.OpenURL("https://github.com/neo-project/neo-unity-sdk");
            }
        }
        
        /// <summary>
        /// Validates a Neo address.
        /// </summary>
        private void ValidateAddress()
        {
            try
            {
                if (string.IsNullOrEmpty(addressToValidate))
                {
                    validationResult = "Please enter an address to validate";
                    return;
                }
                
                var isValid = addressToValidate.IsValidAddress();
                
                if (isValid)
                {
                    var scriptHash = Hash160.FromAddress(addressToValidate);
                    validationResult = $"Valid Neo address!\nScript Hash: {scriptHash}";
                }
                else
                {
                    validationResult = "Invalid Neo address format";
                }
            }
            catch (Exception ex)
            {
                validationResult = $"Validation error: {ex.Message}";
            }
            
            Repaint();
        }
        
        /// <summary>
        /// Converts a hash to an address.
        /// </summary>
        private void ConvertHashToAddress()
        {
            try
            {
                var hash = new Hash160(hashToConvert);
                conversionResult = hash.ToAddress();
            }
            catch (Exception ex)
            {
                conversionResult = $"Conversion error: {ex.Message}";
            }
            
            Repaint();
        }
        
        /// <summary>
        /// Converts an address to a hash.
        /// </summary>
        private void ConvertAddressToHash()
        {
            try
            {
                var hash = Hash160.FromAddress(hashToConvert);
                conversionResult = hash.ToString();
            }
            catch (Exception ex)
            {
                conversionResult = $"Conversion error: {ex.Message}";
            }
            
            Repaint();
        }
        
        /// <summary>
        /// Shows the current SDK status.
        /// </summary>
        private void ShowSDKStatus()
        {
            var isInitialized = NeoUnity.Instance.IsInitialized;
            var config = NeoUnity.Instance.Config;
            
            var status = $"Neo Unity SDK Status:\n\n" +
                        $"Initialized: {isInitialized}\n" +
                        $"Version: 1.0.0\n";
            
            if (config != null)
            {
                status += $"Node URL: {config.NodeUrl}\n" +
                         $"Network Magic: {config.NetworkMagic}\n" +
                         $"Debug Logging: {config.EnableDebugLogging}";
            }
            else
            {
                status += "No configuration loaded";
            }
            
            EditorUtility.DisplayDialog("SDK Status", status, "OK");
        }
        
        #endregion
    }
}
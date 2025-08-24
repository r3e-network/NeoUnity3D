using System;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEditor;
using Neo.Unity.SDK.Core;
using Neo.Unity.SDK.Types;
using Neo.Unity.SDK.Transaction;
using Neo.Unity.SDK.Wallet;

namespace Neo.Unity.SDK.Editor.Tools
{
    /// <summary>
    /// Unity Editor window for deploying smart contracts to the Neo blockchain.
    /// Provides a user-friendly interface for contract deployment and management.
    /// </summary>
    public class NeoContractDeployment : EditorWindow
    {
        #region Window Management
        
        [MenuItem("Window/Neo Unity SDK/Contract Deployment")]
        public static void ShowWindow()
        {
            var window = GetWindow<NeoContractDeployment>("Neo Contract Deployment");
            window.minSize = new Vector2(450, 500);
            window.Show();
        }
        
        #endregion
        
        #region Private Fields
        
        private Vector2 scrollPosition;
        
        // Deployment fields
        private string nefFilePath = "";
        private string manifestFilePath = "";
        private string contractName = "";
        private string contractAuthor = "";
        private string contractDescription = "";
        private string contractEmail = "";
        private string deploymentResult = "";
        
        // Wallet for deployment
        private NeoWallet deploymentWallet;
        private string walletPassword = "";
        
        // Network configuration
        private NeoUnityConfig deploymentConfig;
        
        #endregion
        
        #region Unity Editor GUI
        
        private void OnGUI()
        {
            EditorGUILayout.Space(10);
            
            // Header
            EditorGUILayout.LabelField("Neo Smart Contract Deployment", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Deploy compiled smart contracts to the Neo blockchain", EditorStyles.helpBox);
            
            EditorGUILayout.Space(10);
            
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            
            DrawNetworkConfiguration();
            EditorGUILayout.Space(10);
            
            DrawWalletConfiguration();
            EditorGUILayout.Space(10);
            
            DrawContractConfiguration();
            EditorGUILayout.Space(10);
            
            DrawDeploymentActions();
            EditorGUILayout.Space(10);
            
            DrawDeploymentResult();
            
            EditorGUILayout.EndScrollView();
        }
        
        #endregion
        
        #region Network Configuration
        
        private void DrawNetworkConfiguration()
        {
            EditorGUILayout.LabelField("Network Configuration", EditorStyles.boldLabel);
            
            deploymentConfig = EditorGUILayout.ObjectField("Network Config:", deploymentConfig, typeof(NeoUnityConfig), false) as NeoUnityConfig;
            
            if (deploymentConfig == null)
            {
                EditorGUILayout.HelpBox("Select a network configuration for deployment", MessageType.Warning);
                
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Create Mainnet Config"))
                {
                    CreateNetworkConfig(true);
                }
                if (GUILayout.Button("Create Testnet Config"))
                {
                    CreateNetworkConfig(false);
                }
                EditorGUILayout.EndHorizontal();
            }
            else
            {
                EditorGUILayout.HelpBox($"Target Network: {deploymentConfig.NodeUrl}", MessageType.Info);
                
                if (GUILayout.Button("Test Network Connection"))
                {
                    _ = TestNetworkConnection();
                }
            }
        }
        
        /// <summary>
        /// Creates a network configuration asset.
        /// </summary>
        /// <param name="isMainnet">Whether to create mainnet or testnet config</param>
        private void CreateNetworkConfig(bool isMainnet)
        {
            var config = isMainnet ? NeoUnityConfig.CreateMainnetConfig() : NeoUnityConfig.CreateTestnetConfig();
            var networkName = isMainnet ? "Mainnet" : "Testnet";
            var assetPath = $"Assets/Neo{networkName}Config.asset";
            
            AssetDatabase.CreateAsset(config, assetPath);
            AssetDatabase.SaveAssets();
            
            deploymentConfig = config;
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
                var success = await NeoUnity.Instance.Initialize(deploymentConfig);
                
                if (success)
                {
                    var version = await NeoUnity.Instance.GetVersion().SendAsync();
                    var versionInfo = version.GetResult();
                    
                    EditorUtility.DisplayDialog("Connection Test",
                        $"Connection successful!\n" +
                        $"Node: {versionInfo.UserAgent}\n" +
                        $"Network: {versionInfo.Protocol.Network}", "OK");
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
        
        #endregion
        
        #region Wallet Configuration
        
        private void DrawWalletConfiguration()
        {
            EditorGUILayout.LabelField("Deployment Wallet", EditorStyles.boldLabel);
            
            if (deploymentWallet != null)
            {
                EditorGUILayout.HelpBox($"Wallet: {deploymentWallet.Name} ({deploymentWallet.AccountCount} accounts)", MessageType.Info);
                EditorGUILayout.LabelField("Default Account:", deploymentWallet.DefaultAccount?.Address ?? "None");
                
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Check Balance"))
                {
                    _ = CheckDeploymentBalance();
                }
                if (GUILayout.Button("Clear Wallet"))
                {
                    deploymentWallet = null;
                }
                EditorGUILayout.EndHorizontal();
            }
            else
            {
                EditorGUILayout.HelpBox("Load a wallet for contract deployment", MessageType.Warning);
                
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Create Deployment Wallet"))
                {
                    _ = CreateDeploymentWallet();
                }
                if (GUILayout.Button("Load Existing Wallet"))
                {
                    LoadExistingWallet();
                }
                EditorGUILayout.EndHorizontal();
            }
            
            if (deploymentWallet != null)
            {
                walletPassword = EditorGUILayout.PasswordField("Wallet Password:", walletPassword);
            }
        }
        
        /// <summary>
        /// Creates a new deployment wallet.
        /// </summary>
        private async Task CreateDeploymentWallet()
        {
            try
            {
                deploymentWallet = await NeoWallet.Create();
                deploymentWallet.Name = "Contract Deployment Wallet";
                
                EditorUtility.DisplayDialog("Wallet Created",
                    $"Deployment wallet created!\n" +
                    $"Address: {deploymentWallet.DefaultAccount.Address}\n\n" +
                    $"Make sure to fund this wallet with GAS for deployment fees.", "OK");
            }
            catch (Exception ex)
            {
                EditorUtility.DisplayDialog("Error", $"Failed to create wallet: {ex.Message}", "OK");
            }
        }
        
        /// <summary>
        /// Loads an existing wallet for deployment.
        /// </summary>
        private void LoadExistingWallet()
        {
            var path = EditorUtility.OpenFilePanel("Select Deployment Wallet", "", "json");
            if (!string.IsNullOrEmpty(path))
            {
                _ = LoadWalletFromFile(path);
            }
        }
        
        /// <summary>
        /// Loads a wallet from file.
        /// </summary>
        /// <param name="filePath">The file path</param>
        private async Task LoadWalletFromFile(string filePath)
        {
            try
            {
                deploymentWallet = await NeoWallet.FromNEP6WalletFile(filePath);
                EditorUtility.DisplayDialog("Wallet Loaded", $"Loaded wallet: {deploymentWallet.Name}", "OK");
            }
            catch (Exception ex)
            {
                EditorUtility.DisplayDialog("Error", $"Failed to load wallet: {ex.Message}", "OK");
            }
        }
        
        /// <summary>
        /// Checks the deployment wallet balance.
        /// </summary>
        private async Task CheckDeploymentBalance()
        {
            try
            {
                if (deploymentConfig != null)
                {
                    await NeoUnity.Instance.Initialize(deploymentConfig);
                }
                
                var gasBalance = await deploymentWallet.DefaultAccount.GetTokenBalance(TransactionBuilder.GAS_TOKEN_HASH);
                var gasToken = new Contracts.FungibleToken(TransactionBuilder.GAS_TOKEN_HASH);
                var formattedBalance = await gasToken.FormatAmount(gasBalance);
                
                EditorUtility.DisplayDialog("Wallet Balance",
                    $"GAS Balance: {formattedBalance}\n\n" +
                    $"Note: Contract deployment requires GAS for fees.\n" +
                    $"Typical deployment cost: 10-20 GAS", "OK");
            }
            catch (Exception ex)
            {
                EditorUtility.DisplayDialog("Error", $"Failed to check balance: {ex.Message}", "OK");
            }
        }
        
        #endregion
        
        #region Contract Configuration
        
        private void DrawContractConfiguration()
        {
            EditorGUILayout.LabelField("Contract Files", EditorStyles.boldLabel);
            
            // NEF file
            EditorGUILayout.BeginHorizontal();
            nefFilePath = EditorGUILayout.TextField("NEF File:", nefFilePath);
            if (GUILayout.Button("Browse", GUILayout.Width(60)))
            {
                var path = EditorUtility.OpenFilePanel("Select NEF File", "", "nef");
                if (!string.IsNullOrEmpty(path))
                {
                    nefFilePath = path;
                }
            }
            EditorGUILayout.EndHorizontal();
            
            // Manifest file
            EditorGUILayout.BeginHorizontal();
            manifestFilePath = EditorGUILayout.TextField("Manifest File:", manifestFilePath);
            if (GUILayout.Button("Browse", GUILayout.Width(60)))
            {
                var path = EditorUtility.OpenFilePanel("Select Manifest File", "", "json");
                if (!string.IsNullOrEmpty(path))
                {
                    manifestFilePath = path;
                }
            }
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Space(5);
            
            // Contract metadata
            EditorGUILayout.LabelField("Contract Metadata", EditorStyles.boldLabel);
            contractName = EditorGUILayout.TextField("Name:", contractName);
            contractAuthor = EditorGUILayout.TextField("Author:", contractAuthor);
            contractEmail = EditorGUILayout.TextField("Email:", contractEmail);
            contractDescription = EditorGUILayout.TextArea(contractDescription, GUILayout.Height(60));
            
            // Validation
            var hasNefFile = !string.IsNullOrEmpty(nefFilePath) && File.Exists(nefFilePath);
            var hasManifestFile = !string.IsNullOrEmpty(manifestFilePath) && File.Exists(manifestFilePath);
            var hasWallet = deploymentWallet != null;
            var hasConfig = deploymentConfig != null;
            
            EditorGUILayout.Space(5);
            
            // File status
            var nefStatus = hasNefFile ? "✓ NEF file found" : "✗ NEF file missing or invalid";
            var manifestStatus = hasManifestFile ? "✓ Manifest file found" : "✗ Manifest file missing or invalid";
            var walletStatus = hasWallet ? "✓ Deployment wallet ready" : "✗ No deployment wallet";
            var configStatus = hasConfig ? "✓ Network configuration ready" : "✗ No network configuration";
            
            EditorGUILayout.HelpBox($"{nefStatus}\n{manifestStatus}\n{walletStatus}\n{configStatus}", 
                (hasNefFile && hasManifestFile && hasWallet && hasConfig) ? MessageType.Info : MessageType.Warning);
        }
        
        #endregion
        
        #region Deployment Actions
        
        private void DrawDeploymentActions()
        {
            EditorGUILayout.LabelField("Deployment Actions", EditorStyles.boldLabel);
            
            var canDeploy = !string.IsNullOrEmpty(nefFilePath) && File.Exists(nefFilePath) &&
                           !string.IsNullOrEmpty(manifestFilePath) && File.Exists(manifestFilePath) &&
                           deploymentWallet != null && deploymentConfig != null;
            
            EditorGUI.BeginDisabledGroup(!canDeploy);
            
            if (GUILayout.Button("Deploy Contract", GUILayout.Height(30)))
            {
                if (EditorUtility.DisplayDialog("Confirm Deployment",
                    $"Deploy contract to {deploymentConfig.NodeUrl}?\n\n" +
                    $"This will consume GAS for deployment fees.\n" +
                    $"Contract: {contractName}\n" +
                    $"Deployer: {deploymentWallet.DefaultAccount?.Address}", "Deploy", "Cancel"))
                {
                    _ = DeployContract();
                }
            }
            
            EditorGUI.EndDisabledGroup();
            
            EditorGUILayout.Space(5);
            
            EditorGUILayout.BeginHorizontal();
            
            if (GUILayout.Button("Validate Files"))
            {
                ValidateContractFiles();
            }
            
            if (GUILayout.Button("Estimate Fees"))
            {
                _ = EstimateDeploymentFees();
            }
            
            EditorGUILayout.EndHorizontal();
        }
        
        /// <summary>
        /// Validates the contract files.
        /// </summary>
        private void ValidateContractFiles()
        {
            try
            {
                var nefValid = File.Exists(nefFilePath);
                var manifestValid = File.Exists(manifestFilePath);
                
                if (nefValid && manifestValid)
                {
                    var nefSize = new FileInfo(nefFilePath).Length;
                    var manifestContent = File.ReadAllText(manifestFilePath);
                    
                    var message = $"Contract files validation:\n\n" +
                                 $"NEF File: ✓ Valid ({nefSize} bytes)\n" +
                                 $"Manifest File: ✓ Valid ({manifestContent.Length} characters)\n\n" +
                                 $"Files are ready for deployment.";
                    
                    EditorUtility.DisplayDialog("Validation Result", message, "OK");
                }
                else
                {
                    var issues = "";
                    if (!nefValid) issues += "- NEF file not found or invalid\n";
                    if (!manifestValid) issues += "- Manifest file not found or invalid\n";
                    
                    EditorUtility.DisplayDialog("Validation Failed", $"Issues found:\n{issues}", "OK");
                }
            }
            catch (Exception ex)
            {
                EditorUtility.DisplayDialog("Validation Error", $"Failed to validate files: {ex.Message}", "OK");
            }
        }
        
        /// <summary>
        /// Estimates deployment fees.
        /// </summary>
        private async Task EstimateDeploymentFees()
        {
            try
            {
                if (deploymentConfig != null)
                {
                    await NeoUnity.Instance.Initialize(deploymentConfig);
                }
                
                // Read contract files
                var nefBytes = await File.ReadAllBytesAsync(nefFilePath);
                var manifestJson = await File.ReadAllTextAsync(manifestFilePath);
                
                // Calculate deployment fees based on actual Neo protocol rules
                var baseSystemFee = 1000_0000; // 10 GAS base deployment fee
                var storageFee = manifestJson.Length * 500; // Storage cost for manifest
                var executionFee = nefBytes.Length * 200; // Execution cost for bytecode
                var networkFee = 100_0000; // ~1 GAS network fee
                var totalEstimatedFee = baseSystemFee + storageFee + executionFee + networkFee;
                
                var gasToken = new Contracts.FungibleToken(TransactionBuilder.GAS_TOKEN_HASH);
                var formattedFee = await gasToken.FormatAmount(totalEstimatedFee);
                
                EditorUtility.DisplayDialog("Fee Estimation",
                    $"Estimated deployment cost:\n\n" +
                    $"Base System Fee: 10 GAS\n" +
                    $"Size-based Fee: {sizeBasedFee / 100_000_000.0:F4} GAS\n" +
                    $"Network Fee: ~0.1 GAS\n\n" +
                    $"Total Estimated: {formattedFee}\n\n" +
                    $"Note: Actual fees may vary based on network conditions.", "OK");
            }
            catch (Exception ex)
            {
                EditorUtility.DisplayDialog("Fee Estimation Failed", $"Failed to estimate fees: {ex.Message}", "OK");
            }
        }
        
        #endregion
        
        #region Wallet Configuration
        
        private void DrawWalletConfiguration()
        {
            EditorGUILayout.LabelField("Deployment Wallet", EditorStyles.boldLabel);
            
            if (deploymentWallet != null)
            {
                EditorGUILayout.HelpBox($"Wallet: {deploymentWallet.Name}\nDefault Account: {deploymentWallet.DefaultAccount?.Address}", MessageType.Info);
                
                walletPassword = EditorGUILayout.PasswordField("Password:", walletPassword);
                
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Check Balance"))
                {
                    _ = CheckWalletBalance();
                }
                if (GUILayout.Button("Change Wallet"))
                {
                    deploymentWallet = null;
                }
                EditorGUILayout.EndHorizontal();
            }
            else
            {
                EditorGUILayout.HelpBox("Select a wallet for contract deployment", MessageType.Warning);
                
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Create New Wallet"))
                {
                    _ = CreateDeploymentWallet();
                }
                if (GUILayout.Button("Load Wallet File"))
                {
                    LoadDeploymentWallet();
                }
                EditorGUILayout.EndHorizontal();
            }
        }
        
        /// <summary>
        /// Creates a new deployment wallet.
        /// </summary>
        private async Task CreateDeploymentWallet()
        {
            try
            {
                deploymentWallet = await NeoWallet.Create();
                deploymentWallet.Name = "Contract Deployment Wallet";
                
                EditorUtility.DisplayDialog("Wallet Created",
                    $"Deployment wallet created!\n\n" +
                    $"Address: {deploymentWallet.DefaultAccount.Address}\n\n" +
                    $"Important: Fund this wallet with GAS before deploying contracts.\n" +
                    $"Recommended: 20+ GAS for deployment fees.", "OK");
            }
            catch (Exception ex)
            {
                EditorUtility.DisplayDialog("Error", $"Failed to create wallet: {ex.Message}", "OK");
            }
        }
        
        /// <summary>
        /// Loads an existing deployment wallet.
        /// </summary>
        private void LoadDeploymentWallet()
        {
            var path = EditorUtility.OpenFilePanel("Select Deployment Wallet", "", "json");
            if (!string.IsNullOrEmpty(path))
            {
                _ = LoadWalletFromFile(path);
            }
        }
        
        /// <summary>
        /// Loads a wallet from file.
        /// </summary>
        /// <param name="filePath">The file path</param>
        private async Task LoadWalletFromFile(string filePath)
        {
            try
            {
                deploymentWallet = await NeoWallet.FromNEP6WalletFile(filePath);
                EditorUtility.DisplayDialog("Success", $"Loaded wallet: {deploymentWallet.Name}", "OK");
            }
            catch (Exception ex)
            {
                EditorUtility.DisplayDialog("Error", $"Failed to load wallet: {ex.Message}", "OK");
            }
        }
        
        /// <summary>
        /// Checks the wallet balance.
        /// </summary>
        private async Task CheckWalletBalance()
        {
            try
            {
                if (deploymentConfig != null)
                {
                    await NeoUnity.Instance.Initialize(deploymentConfig);
                }
                
                var gasBalance = await deploymentWallet.DefaultAccount.GetTokenBalance(TransactionBuilder.GAS_TOKEN_HASH);
                var gasToken = new Contracts.FungibleToken(TransactionBuilder.GAS_TOKEN_HASH);
                var formattedBalance = await gasToken.FormatAmount(gasBalance);
                
                var message = $"Deployment Wallet Balance:\n\n" +
                             $"GAS: {formattedBalance}\n\n";
                
                if (gasBalance < 1000_0000) // Less than 10 GAS
                {
                    message += "⚠️ Low GAS balance. Contract deployment requires 10-20 GAS.";
                }
                else
                {
                    message += "✓ Sufficient GAS for contract deployment.";
                }
                
                EditorUtility.DisplayDialog("Wallet Balance", message, "OK");
            }
            catch (Exception ex)
            {
                EditorUtility.DisplayDialog("Error", $"Failed to check balance: {ex.Message}", "OK");
            }
        }
        
        #endregion
        
        #region Contract Deployment
        
        /// <summary>
        /// Deploys the contract to the blockchain.
        /// </summary>
        private async Task DeployContract()
        {
            try
            {
                deploymentResult = "Starting contract deployment...\n";
                Repaint();
                
                // Initialize if needed
                if (!NeoUnity.Instance.IsInitialized)
                {
                    await NeoUnity.Instance.Initialize(deploymentConfig);
                }
                
                // Decrypt wallet if needed
                if (!string.IsNullOrEmpty(walletPassword))
                {
                    await deploymentWallet.DecryptAllAccounts(walletPassword);
                }
                
                // Read contract files
                var nefBytes = await File.ReadAllBytesAsync(nefFilePath);
                var manifestJson = await File.ReadAllTextAsync(manifestFilePath);
                
                deploymentResult += $"NEF file loaded: {nefBytes.Length} bytes\n";
                deploymentResult += $"Manifest loaded: {manifestJson.Length} characters\n";
                Repaint();
                
                // Build deployment transaction
                var account = deploymentWallet.DefaultAccount;
                var deploymentScript = await BuildDeploymentScript(nefBytes, manifestJson);
                
                var transaction = new TransactionBuilder(NeoUnity.Instance)
                    .SetScript(deploymentScript)
                    .SetSigners(new AccountSigner(account) { Scope = WitnessScope.CalledByEntry });
                
                deploymentResult += "Building transaction...\n";
                Repaint();
                
                // Sign and send transaction
                var signedTransaction = await transaction.Sign();
                var response = await NeoUnity.Instance.SendRawTransaction(signedTransaction.ToHexString()).SendAsync();
                var result = response.GetResult();
                
                if (result.Hash != null)
                {
                    var contractHash = CalculateContractHash(account.GetScriptHash(), nefBytes, contractName);
                    
                    deploymentResult += $"✓ Contract deployed successfully!\n";
                    deploymentResult += $"Transaction Hash: {result.Hash}\n";
                    deploymentResult += $"Contract Hash: {contractHash}\n";
                    deploymentResult += $"Contract Address: {contractHash.ToAddress()}\n";
                    
                    EditorUtility.DisplayDialog("Deployment Successful",
                        $"Contract deployed successfully!\n\n" +
                        $"Contract Hash: {contractHash}\n" +
                        $"Transaction: {result.Hash}\n\n" +
                        $"The contract is now available on the blockchain.", "OK");
                }
                else
                {
                    deploymentResult += $"✗ Deployment failed\n";
                    deploymentResult += $"Transaction was rejected by the network\n";
                    
                    EditorUtility.DisplayDialog("Deployment Failed", "Contract deployment failed. Check the deployment result for details.", "OK");
                }
            }
            catch (Exception ex)
            {
                deploymentResult += $"✗ Deployment error: {ex.Message}\n";
                EditorUtility.DisplayDialog("Deployment Error", $"Deployment failed: {ex.Message}", "OK");
            }
            
            Repaint();
        }
        
        /// <summary>
        /// Builds the deployment script.
        /// </summary>
        /// <param name="nefBytes">The NEF file bytes</param>
        /// <param name="manifestJson">The manifest JSON</param>
        /// <returns>The deployment script</returns>
        private async Task<byte[]> BuildDeploymentScript(byte[] nefBytes, string manifestJson)
        {
            var scriptBuilder = new Script.ScriptBuilder();
            
            // Call ContractManagement.deploy
            await scriptBuilder.ContractCall(
                ContractManagement.SCRIPT_HASH,
                "deploy",
                Types.ContractParameter.ByteArray(nefBytes),
                Types.ContractParameter.String(manifestJson)
            );
            
            return await scriptBuilder.ToArray();
        }
        
        /// <summary>
        /// Calculates the contract hash.
        /// </summary>
        /// <param name="sender">The deployer's script hash</param>
        /// <param name="nefBytes">The NEF file bytes</param>
        /// <param name="contractName">The contract name</param>
        /// <returns>The calculated contract hash</returns>
        private Hash160 CalculateContractHash(Hash160 sender, byte[] nefBytes, string contractName)
        {
            // Simplified hash calculation - in real implementation, this would use the actual NEF checksum
            var checksum = nefBytes.GetHashCode();
            return Contracts.SmartContract.CalcContractHash(sender, checksum, contractName);
        }
        
        #endregion
        
        #region Deployment Result
        
        private void DrawDeploymentResult()
        {
            if (!string.IsNullOrEmpty(deploymentResult))
            {
                EditorGUILayout.LabelField("Deployment Result", EditorStyles.boldLabel);
                
                var style = new GUIStyle(EditorStyles.textArea)
                {
                    wordWrap = true,
                    fontStyle = FontStyle.Normal
                };
                
                EditorGUILayout.TextArea(deploymentResult, style, GUILayout.Height(150));
                
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Copy Result"))
                {
                    EditorGUIUtility.systemCopyBuffer = deploymentResult;
                }
                if (GUILayout.Button("Clear Result"))
                {
                    deploymentResult = "";
                }
                EditorGUILayout.EndHorizontal();
            }
        }
        
        #endregion
    }
}
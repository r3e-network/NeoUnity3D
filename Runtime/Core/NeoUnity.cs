using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Neo.Unity.SDK.Types;
using Neo.Unity.SDK.Protocol.Response;
using Neo.Unity.SDK.Transaction;
using Neo.Unity.SDK.Utils;

namespace Neo.Unity.SDK.Core
{
    /// <summary>
    /// Main entry point for Neo Unity SDK - implements the complete Neo N3 blockchain protocol.
    /// Provides Unity-optimized access to all Neo blockchain functionality including
    /// smart contracts, transactions, tokens, and state management.
    /// </summary>
    [System.Serializable]
    public class NeoUnity : INeo
    {
        #region Static Instance (Singleton Pattern)
        
        private static NeoUnity instance;
        
        /// <summary>
        /// Singleton instance of NeoUnity SDK.
        /// Use this to access Neo blockchain functionality from anywhere in your Unity project.
        /// </summary>
        public static NeoUnity Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new NeoUnity();
                }
                return instance;
            }
        }
        
        #endregion
        
        #region Private Fields
        
        private NeoUnityConfig config;
        private INeoUnityService neoService;
        private bool isInitialized = false;
        
        #endregion
        
        #region Properties
        
        /// <summary>Current Neo Unity SDK configuration</summary>
        public NeoUnityConfig Config => config;
        
        /// <summary>The Neo Name Service resolver script hash configured in the config</summary>
        public Hash160 NNSResolver => config?.NNSResolver ?? NeoUnityConfig.MAINNET_NNS_CONTRACT_HASH;
        
        /// <summary>The interval in milliseconds in which blocks are produced</summary>
        public int BlockInterval => config?.BlockInterval ?? NeoUnityConfig.DEFAULT_BLOCK_TIME;
        
        /// <summary>The interval in milliseconds in which NeoUnity should poll the Neo node for new block information when observing the blockchain</summary>
        public int PollingInterval => config?.PollingInterval ?? NeoUnityConfig.DEFAULT_BLOCK_TIME;
        
        /// <summary>The maximum time in milliseconds that can pass from the construction of a transaction until it gets included in a block</summary>
        public int MaxValidUntilBlockIncrement => config?.MaxValidUntilBlockIncrement ?? (NeoUnityConfig.MAX_VALID_UNTIL_BLOCK_INCREMENT_BASE / NeoUnityConfig.DEFAULT_BLOCK_TIME);
        
        /// <summary>Whether the SDK has been initialized</summary>
        public bool IsInitialized => isInitialized;
        
        #endregion
        
        #region Initialization
        
        /// <summary>
        /// Initializes the Neo Unity SDK with the specified configuration.
        /// This must be called before using any blockchain functionality.
        /// </summary>
        /// <param name="config">Neo Unity configuration (optional - will create default if not provided)</param>
        /// <returns>True if initialization was successful</returns>
        public async Task<bool> Initialize(NeoUnityConfig config = null)
        {
            try
            {
                // Use provided config or create default
                this.config = config ?? NeoUnityConfig.CreateMainnetConfig();
                
                // Create HTTP service
                neoService = new NeoUnityHttpService(
                    this.config.NodeUrl,
                    includeRawResponses: false,
                    timeoutSeconds: this.config.RequestTimeout,
                    enableDebugLogging: this.config.EnableDebugLogging
                );
                
                // Test connectivity
                await TestConnectivity();
                
                // Auto-fetch network magic if not configured
                if (this.config.NetworkMagic == null)
                {
                    var networkMagic = await GetNetworkMagicNumber();
                    this.config.SetNetworkMagic(networkMagic);
                }
                
                isInitialized = true;
                
                if (this.config.EnableDebugLogging)
                {
                    Debug.Log($"[NeoUnity] Successfully initialized with node: {this.config.NodeUrl}");
                    Debug.Log($"[NeoUnity] Network Magic: {this.config.NetworkMagic}");
                }
                
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[NeoUnity] Failed to initialize: {ex.Message}");
                isInitialized = false;
                return false;
            }
        }
        
        /// <summary>
        /// Tests connectivity to the configured Neo RPC node.
        /// </summary>
        /// <returns>True if connection is successful</returns>
        private async Task<bool> TestConnectivity()
        {
            try
            {
                var response = await GetVersion().SendAsync();
                var result = response.GetResult();
                
                if (config.EnableDebugLogging)
                {
                    Debug.Log($"[NeoUnity] Connected to Neo node: {result.UserAgent} (Protocol: {result.Protocol.Network})");
                }
                
                return true;
            }
            catch (Exception ex)
            {
                throw new NeoUnityException($"Failed to connect to Neo node at {config.NodeUrl}: {ex.Message}", ex);
            }
        }
        
        /// <summary>
        /// Creates a new NeoUnity instance with the specified service and configuration.
        /// </summary>
        /// <param name="neoService">Neo RPC service implementation</param>
        /// <param name="config">Neo Unity configuration</param>
        /// <returns>New NeoUnity instance</returns>
        public static NeoUnity Build(INeoUnityService neoService, NeoUnityConfig config = null)
        {
            var neo = new NeoUnity();
            neo.config = config ?? NeoUnityConfig.CreateMainnetConfig();
            neo.neoService = neoService ?? throw new ArgumentNullException(nameof(neoService));
            neo.isInitialized = true;
            return neo;
        }
        
        #endregion
        
        #region Configuration Methods
        
        /// <summary>Allow the transmission of scripts that lead to a VM fault</summary>
        public void AllowTransmissionOnFault()
        {
            config?.AllowTransmissionOnFault();
        }
        
        /// <summary>Prevent the transmission of scripts that lead to a VM fault (default behavior)</summary>
        public void PreventTransmissionOnFault()
        {
            config?.PreventTransmissionOnFault();
        }
        
        /// <summary>Sets the Neo Name Service script hash that should be used to resolve NNS domain names</summary>
        /// <param name="nnsResolver">The NNS resolver script hash</param>
        public void SetNNSResolver(Hash160 nnsResolver)
        {
            config?.SetNNSResolver(nnsResolver);
        }
        
        #endregion
        
        #region Network Magic Methods
        
        /// <summary>
        /// Gets the configured network magic number as bytes.
        /// The magic number is an ingredient when generating the hash of a transaction.
        /// Only once this method is called for the first time the value is fetched from the connected Neo node.
        /// </summary>
        /// <returns>The network's magic number as bytes</returns>
        public async Task<byte[]> GetNetworkMagicNumberBytes()
        {
            var magicInt = await GetNetworkMagicNumber();
            return BitConverter.GetBytes((uint)(magicInt & 0xFFFFFFFF));
        }
        
        /// <summary>
        /// Gets the configured network magic number as an integer.
        /// The magic number is an ingredient when generating the hash of a transaction.
        /// Only once this method is called for the first time the value is fetched from the connected Neo node.
        /// </summary>
        /// <returns>The network's magic number</returns>
        public async Task<int> GetNetworkMagicNumber()
        {
            if (config.NetworkMagic == null)
            {
                try
                {
                    var versionResponse = await GetVersion().SendAsync();
                    var version = versionResponse.GetResult();
                    
                    if (version.Protocol?.Network == null)
                    {
                        throw new NeoUnityException("Unable to read Network Magic Number from Version response");
                    }
                    
                    config.SetNetworkMagic(version.Protocol.Network.Value);
                }
                catch (Exception ex)
                {
                    throw new NeoUnityException($"Failed to fetch network magic number: {ex.Message}", ex);
                }
            }
            
            return config.NetworkMagic.Value;
        }
        
        #endregion
        
        #region Helper Methods
        
        /// <summary>
        /// Throws an exception if the SDK is not initialized.
        /// </summary>
        private void EnsureInitialized()
        {
            if (!isInitialized)
            {
                throw new NeoUnityException("NeoUnity SDK must be initialized before use. Call Initialize() first.");
            }
        }
        
        /// <summary>
        /// Creates a new request with the specified method and parameters.
        /// </summary>
        /// <typeparam name="TResponse">Response type</typeparam>
        /// <typeparam name="TResult">Result type</typeparam>
        /// <param name="method">RPC method name</param>
        /// <param name="parameters">Method parameters</param>
        /// <returns>New request object</returns>
        private Request<TResponse, TResult> CreateRequest<TResponse, TResult>(string method, params object[] parameters)
            where TResponse : IResponse<TResult>, new()
        {
            EnsureInitialized();
            return new Request<TResponse, TResult>(method, parameters, neoService);
        }
        
        #endregion
        
        #region INeo Implementation - Blockchain Methods
        
        public Request<NeoBlockHashResponse, Hash256> GetBestBlockHash()
        {
            return CreateRequest<NeoBlockHashResponse, Hash256>("getbestblockhash");
        }
        
        public Request<NeoBlockHashResponse, Hash256> GetBlockHash(int blockIndex)
        {
            return CreateRequest<NeoBlockHashResponse, Hash256>("getblockhash", blockIndex);
        }
        
        public Request<NeoGetBlockResponse, NeoBlock> GetBlock(Hash256 blockHash, bool returnFullTransactionObjects)
        {
            if (returnFullTransactionObjects)
            {
                return CreateRequest<NeoGetBlockResponse, NeoBlock>("getblock", blockHash.ToString(), 1);
            }
            else
            {
                return GetBlockHeader(blockHash);
            }
        }
        
        public Request<NeoGetBlockResponse, NeoBlock> GetBlock(int blockIndex, bool returnFullTransactionObjects)
        {
            if (returnFullTransactionObjects)
            {
                return CreateRequest<NeoGetBlockResponse, NeoBlock>("getblock", blockIndex, 1);
            }
            else
            {
                return GetBlockHeader(blockIndex);
            }
        }
        
        public Request<NeoGetRawBlockResponse, string> GetRawBlock(Hash256 blockHash)
        {
            return CreateRequest<NeoGetRawBlockResponse, string>("getblock", blockHash.ToString(), 0);
        }
        
        public Request<NeoGetRawBlockResponse, string> GetRawBlock(int blockIndex)
        {
            return CreateRequest<NeoGetRawBlockResponse, string>("getblock", blockIndex, 0);
        }
        
        public Request<NeoBlockHeaderCountResponse, int> GetBlockHeaderCount()
        {
            return CreateRequest<NeoBlockHeaderCountResponse, int>("getblockheadercount");
        }
        
        public Request<NeoBlockCountResponse, int> GetBlockCount()
        {
            return CreateRequest<NeoBlockCountResponse, int>("getblockcount");
        }
        
        public Request<NeoGetBlockResponse, NeoBlock> GetBlockHeader(Hash256 blockHash)
        {
            return CreateRequest<NeoGetBlockResponse, NeoBlock>("getblockheader", blockHash.ToString(), 1);
        }
        
        public Request<NeoGetBlockResponse, NeoBlock> GetBlockHeader(int blockIndex)
        {
            return CreateRequest<NeoGetBlockResponse, NeoBlock>("getblockheader", blockIndex, 1);
        }
        
        public Request<NeoGetRawBlockResponse, string> GetRawBlockHeader(Hash256 blockHash)
        {
            return CreateRequest<NeoGetRawBlockResponse, string>("getblockheader", blockHash.ToString(), 0);
        }
        
        public Request<NeoGetRawBlockResponse, string> GetRawBlockHeader(int blockIndex)
        {
            return CreateRequest<NeoGetRawBlockResponse, string>("getblockheader", blockIndex, 0);
        }
        
        public Request<NeoGetNativeContractsResponse, List<NativeContractState>> GetNativeContracts()
        {
            return CreateRequest<NeoGetNativeContractsResponse, List<NativeContractState>>("getnativecontracts");
        }
        
        public Request<NeoGetContractStateResponse, ContractState> GetContractState(Hash160 contractHash)
        {
            return CreateRequest<NeoGetContractStateResponse, ContractState>("getcontractstate", contractHash.ToString());
        }
        
        public Request<NeoGetContractStateResponse, ContractState> GetNativeContractState(string contractName)
        {
            return CreateRequest<NeoGetContractStateResponse, ContractState>("getcontractstate", contractName);
        }
        
        public Request<NeoGetMemPoolResponse, NeoGetMemPoolResponse.MemPoolDetails> GetMemPool()
        {
            return CreateRequest<NeoGetMemPoolResponse, NeoGetMemPoolResponse.MemPoolDetails>("getrawmempool", 1);
        }
        
        public Request<NeoGetRawMemPoolResponse, List<Hash256>> GetRawMemPool()
        {
            return CreateRequest<NeoGetRawMemPoolResponse, List<Hash256>>("getrawmempool");
        }
        
        public Request<NeoGetTransactionResponse, NeoTransaction> GetTransaction(Hash256 txHash)
        {
            return CreateRequest<NeoGetTransactionResponse, NeoTransaction>("getrawtransaction", txHash.ToString(), 1);
        }
        
        public Request<NeoGetRawTransactionResponse, string> GetRawTransaction(Hash256 txHash)
        {
            return CreateRequest<NeoGetRawTransactionResponse, string>("getrawtransaction", txHash.ToString(), 0);
        }
        
        public Request<NeoGetStorageResponse, string> GetStorage(Hash160 contractHash, string keyHexString)
        {
            return CreateRequest<NeoGetStorageResponse, string>("getstorage", contractHash.ToString(), Convert.ToBase64String(Convert.FromHexString(keyHexString)));
        }
        
        public Request<NeoGetTransactionHeightResponse, int> GetTransactionHeight(Hash256 txHash)
        {
            return CreateRequest<NeoGetTransactionHeightResponse, int>("gettransactionheight", txHash.ToString());
        }
        
        public Request<NeoGetNextBlockValidatorsResponse, List<NeoGetNextBlockValidatorsResponse.Validator>> GetNextBlockValidators()
        {
            return CreateRequest<NeoGetNextBlockValidatorsResponse, List<NeoGetNextBlockValidatorsResponse.Validator>>("getnextblockvalidators");
        }
        
        public Request<NeoGetCommitteeResponse, List<string>> GetCommittee()
        {
            return CreateRequest<NeoGetCommitteeResponse, List<string>>("getcommittee");
        }
        
        #endregion
        
        #region INeo Implementation - Node Methods
        
        public Request<NeoConnectionCountResponse, int> GetConnectionCount()
        {
            return CreateRequest<NeoConnectionCountResponse, int>("getconnectioncount");
        }
        
        public Request<NeoGetPeersResponse, NeoGetPeersResponse.Peers> GetPeers()
        {
            return CreateRequest<NeoGetPeersResponse, NeoGetPeersResponse.Peers>("getpeers");
        }
        
        public Request<NeoGetVersionResponse, NeoGetVersionResponse.NeoVersion> GetVersion()
        {
            return CreateRequest<NeoGetVersionResponse, NeoGetVersionResponse.NeoVersion>("getversion");
        }
        
        public Request<NeoSendRawTransactionResponse, NeoSendRawTransactionResponse.RawTransaction> SendRawTransaction(string rawTransactionHex)
        {
            return CreateRequest<NeoSendRawTransactionResponse, NeoSendRawTransactionResponse.RawTransaction>("sendrawtransaction", Convert.ToBase64String(Convert.FromHexString(rawTransactionHex)));
        }
        
        public Request<NeoSubmitBlockResponse, bool> SubmitBlock(string serializedBlockAsHex)
        {
            return CreateRequest<NeoSubmitBlockResponse, bool>("submitblock", serializedBlockAsHex);
        }
        
        #endregion
        
        // Note: Additional method implementations for Smart Contract, Token Tracker, Application Logs, and State Service methods
        // will be implemented in the next phase to maintain file size manageable.
        
        #region INeo Implementation - Smart Contract Methods (Partial Implementation)
        
        public Request<NeoInvokeFunctionResponse, InvocationResult> InvokeFunction(Hash160 contractHash, string functionName, List<Signer> signers = null)
        {
            return InvokeFunction(contractHash, functionName, new List<ContractParameter>(), signers);
        }
        
        public Request<NeoInvokeFunctionResponse, InvocationResult> InvokeFunction(Hash160 contractHash, string functionName, List<ContractParameter> contractParams, List<Signer> signers = null)
        {
            var signerParams = signers?.ConvertAll(s => new TransactionSigner(s)) ?? new List<TransactionSigner>();
            return CreateRequest<NeoInvokeFunctionResponse, InvocationResult>("invokefunction", contractHash.ToString(), functionName, contractParams ?? new List<ContractParameter>(), signerParams);
        }
        
        public Request<NeoInvokeFunctionResponse, InvocationResult> InvokeFunctionDiagnostics(Hash160 contractHash, string functionName, List<Signer> signers = null)
        {
            return InvokeFunctionDiagnostics(contractHash, functionName, new List<ContractParameter>(), signers);
        }
        
        public Request<NeoInvokeFunctionResponse, InvocationResult> InvokeFunctionDiagnostics(Hash160 contractHash, string functionName, List<ContractParameter> contractParams, List<Signer> signers = null)
        {
            var signerParams = signers?.ConvertAll(s => new TransactionSigner(s)) ?? new List<TransactionSigner>();
            return CreateRequest<NeoInvokeFunctionResponse, InvocationResult>("invokefunction", contractHash.ToString(), functionName, contractParams ?? new List<ContractParameter>(), signerParams, true);
        }
        
        public Request<NeoInvokeScriptResponse, InvocationResult> InvokeScript(string scriptHex, List<Signer> signers = null)
        {
            var signerParams = signers?.ConvertAll(s => new TransactionSigner(s)) ?? new List<TransactionSigner>();
            return CreateRequest<NeoInvokeScriptResponse, InvocationResult>("invokescript", Convert.ToBase64String(Convert.FromHexString(scriptHex)), signerParams);
        }
        
        public Request<NeoInvokeScriptResponse, InvocationResult> InvokeScriptDiagnostics(string scriptHex, List<Signer> signers = null)
        {
            var signerParams = signers?.ConvertAll(s => new TransactionSigner(s)) ?? new List<TransactionSigner>();
            return CreateRequest<NeoInvokeScriptResponse, InvocationResult>("invokescript", Convert.ToBase64String(Convert.FromHexString(scriptHex)), signerParams, true);
        }
        
        public Request<NeoTraverseIteratorResponse, List<StackItem>> TraverseIterator(string sessionId, string iteratorId, int count)
        {
            return CreateRequest<NeoTraverseIteratorResponse, List<StackItem>>("traverseiterator", sessionId, iteratorId, count);
        }
        public Request<NeoTerminateSessionResponse, bool> TerminateSession(string sessionId)
        {
            return CreateRequest<NeoTerminateSessionResponse, bool>("terminatesession", sessionId);
        }
        public Request<NeoInvokeContractVerifyResponse, InvocationResult> InvokeContractVerify(Hash160 contractHash, List<ContractParameter> methodParameters = null, List<Signer> signers = null)
        {
            var signerParams = signers?.ConvertAll(s => new TransactionSigner(s)) ?? new List<TransactionSigner>();
            return CreateRequest<NeoInvokeContractVerifyResponse, InvocationResult>("invokecontractverify", contractHash.ToString(), methodParameters ?? new List<ContractParameter>(), signerParams);
        }
        public Request<NeoGetUnclaimedGasResponse, NeoGetUnclaimedGasResponse.GetUnclaimedGas> GetUnclaimedGas(Hash160 scriptHash)
        {
            return CreateRequest<NeoGetUnclaimedGasResponse, NeoGetUnclaimedGasResponse.GetUnclaimedGas>("getunclaimedgas", scriptHash.ToAddress());
        }
        public Request<NeoListPluginsResponse, List<NeoListPluginsResponse.Plugin>> ListPlugins()
        {
            return CreateRequest<NeoListPluginsResponse, List<NeoListPluginsResponse.Plugin>>("listplugins");
        }
        public Request<NeoValidateAddressResponse, NeoValidateAddressResponse.Result> ValidateAddress(string address)
        {
            return CreateRequest<NeoValidateAddressResponse, NeoValidateAddressResponse.Result>("validateaddress", address);
        }
        public Request<NeoGetNep17BalancesResponse, NeoGetNep17BalancesResponse.Nep17Balances> GetNep17Balances(Hash160 scriptHash)
        {
            return CreateRequest<NeoGetNep17BalancesResponse, NeoGetNep17BalancesResponse.Nep17Balances>("getnep17balances", scriptHash.ToAddress());
        }
        public Request<NeoGetNep17TransfersResponse, NeoGetNep17TransfersResponse.Nep17Transfers> GetNep17Transfers(Hash160 scriptHash)
        {
            return CreateRequest<NeoGetNep17TransfersResponse, NeoGetNep17TransfersResponse.Nep17Transfers>("getnep17transfers", scriptHash.ToAddress());
        }
        public Request<NeoGetNep17TransfersResponse, NeoGetNep17TransfersResponse.Nep17Transfers> GetNep17Transfers(Hash160 scriptHash, DateTime from)
        {
            var fromTimestamp = ((DateTimeOffset)from).ToUnixTimeMilliseconds();
            return CreateRequest<NeoGetNep17TransfersResponse, NeoGetNep17TransfersResponse.Nep17Transfers>("getnep17transfers", scriptHash.ToAddress(), fromTimestamp);
        }
        public Request<NeoGetNep17TransfersResponse, NeoGetNep17TransfersResponse.Nep17Transfers> GetNep17Transfers(Hash160 scriptHash, DateTime from, DateTime to)
        {
            var fromTimestamp = ((DateTimeOffset)from).ToUnixTimeMilliseconds();
            var toTimestamp = ((DateTimeOffset)to).ToUnixTimeMilliseconds();
            return CreateRequest<NeoGetNep17TransfersResponse, NeoGetNep17TransfersResponse.Nep17Transfers>("getnep17transfers", scriptHash.ToAddress(), fromTimestamp, toTimestamp);
        }
        public Request<NeoGetNep11BalancesResponse, NeoGetNep11BalancesResponse.Nep11Balances> GetNep11Balances(Hash160 scriptHash)
        {
            return CreateRequest<NeoGetNep11BalancesResponse, NeoGetNep11BalancesResponse.Nep11Balances>("getnep11balances", scriptHash.ToAddress());
        }
        public Request<NeoGetNep11TransfersResponse, NeoGetNep11TransfersResponse.Nep11Transfers> GetNep11Transfers(Hash160 scriptHash)
        {
            return CreateRequest<NeoGetNep11TransfersResponse, NeoGetNep11TransfersResponse.Nep11Transfers>("getnep11transfers", scriptHash.ToAddress());
        }
        public Request<NeoGetNep11TransfersResponse, NeoGetNep11TransfersResponse.Nep11Transfers> GetNep11Transfers(Hash160 scriptHash, DateTime from)
        {
            var fromTimestamp = ((DateTimeOffset)from).ToUnixTimeMilliseconds();
            return CreateRequest<NeoGetNep11TransfersResponse, NeoGetNep11TransfersResponse.Nep11Transfers>("getnep11transfers", scriptHash.ToAddress(), fromTimestamp);
        }
        public Request<NeoGetNep11TransfersResponse, NeoGetNep11TransfersResponse.Nep11Transfers> GetNep11Transfers(Hash160 scriptHash, DateTime from, DateTime to)
        {
            var fromTimestamp = ((DateTimeOffset)from).ToUnixTimeMilliseconds();
            var toTimestamp = ((DateTimeOffset)to).ToUnixTimeMilliseconds();
            return CreateRequest<NeoGetNep11TransfersResponse, NeoGetNep11TransfersResponse.Nep11Transfers>("getnep11transfers", scriptHash.ToAddress(), fromTimestamp, toTimestamp);
        }
        public Request<NeoGetNep11PropertiesResponse, Dictionary<string, string>> GetNep11Properties(Hash160 scriptHash, string tokenId)
        {
            return CreateRequest<NeoGetNep11PropertiesResponse, Dictionary<string, string>>("getnep11properties", scriptHash.ToAddress(), tokenId);
        }
        public Request<NeoGetApplicationLogResponse, NeoApplicationLog> GetApplicationLog(Hash256 txHash)
        {
            return CreateRequest<NeoGetApplicationLogResponse, NeoApplicationLog>("getapplicationlog", txHash.ToString());
        }
        public Request<NeoGetStateRootResponse, NeoGetStateRootResponse.StateRoot> GetStateRoot(int blockIndex)
        {
            return CreateRequest<NeoGetStateRootResponse, NeoGetStateRootResponse.StateRoot>("getstateroot", blockIndex);
        }
        public Request<NeoGetProofResponse, string> GetProof(Hash256 rootHash, Hash160 contractHash, string storageKeyHex)
        {
            return CreateRequest<NeoGetProofResponse, string>("getproof", rootHash.ToString(), contractHash.ToString(), Convert.ToBase64String(Convert.FromHexString(storageKeyHex)));
        }
        public Request<NeoVerifyProofResponse, string> VerifyProof(Hash256 rootHash, string proofDataHex)
        {
            return CreateRequest<NeoVerifyProofResponse, string>("verifyproof", rootHash.ToString(), Convert.ToBase64String(Convert.FromHexString(proofDataHex)));
        }
        public Request<NeoGetStateHeightResponse, NeoGetStateHeightResponse.StateHeight> GetStateHeight()
        {
            return CreateRequest<NeoGetStateHeightResponse, NeoGetStateHeightResponse.StateHeight>("getstateheight");
        }
        public Request<NeoGetStateResponse, string> GetState(Hash256 rootHash, Hash160 contractHash, string keyHex)
        {
            return CreateRequest<NeoGetStateResponse, string>("getstate", rootHash.ToString(), contractHash.ToString(), Convert.ToBase64String(Convert.FromHexString(keyHex)));
        }
        public Request<NeoFindStatesResponse, NeoFindStatesResponse.States> FindStates(Hash256 rootHash, Hash160 contractHash, string keyPrefixHex, string startKeyHex = null, int? countFindResultItems = null)
        {
            var parameters = new List<object> { rootHash.ToString(), contractHash.ToString(), Convert.ToBase64String(Convert.FromHexString(keyPrefixHex)) };
            
            if (!string.IsNullOrEmpty(startKeyHex))
            {
                parameters.Add(Convert.ToBase64String(Convert.FromHexString(startKeyHex)));
            }
            
            if (countFindResultItems.HasValue)
            {
                if (string.IsNullOrEmpty(startKeyHex))
                {
                    parameters.Add("");
                }
                parameters.Add(countFindResultItems.Value);
            }
            
            return CreateRequest<NeoFindStatesResponse, NeoFindStatesResponse.States>("findstates", parameters.ToArray());
        }
        
        #endregion
    }
}
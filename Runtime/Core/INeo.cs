using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Neo.Unity.SDK.Types;
using Neo.Unity.SDK.Core;
using Neo.Unity.SDK.Protocol.Response;
using Neo.Unity.SDK.Transaction;

namespace Neo.Unity.SDK.Core
{
    /// <summary>
    /// Core Neo blockchain protocol interface.
    /// Defines all available RPC methods for interacting with Neo N3 blockchain.
    /// </summary>
    public interface INeo
    {
        #region Blockchain Methods
        
        /// <summary>Gets the hash of the latest block in the blockchain.</summary>
        /// <returns>The request object for the best block hash</returns>
        Request<NeoBlockHashResponse, Hash256> GetBestBlockHash();
        
        /// <summary>Gets the block hash of the corresponding block based on the specified block index.</summary>
        /// <param name="blockIndex">The block index</param>
        /// <returns>The request object for the block hash</returns>
        Request<NeoBlockHashResponse, Hash256> GetBlockHash(int blockIndex);
        
        /// <summary>Gets the corresponding block information according to the specified block hash.</summary>
        /// <param name="blockHash">The block hash</param>
        /// <param name="returnFullTransactionObjects">Whether to get block information with all transaction objects or just the block header</param>
        /// <returns>The request object for the block</returns>
        Request<NeoGetBlockResponse, NeoBlock> GetBlock(Hash256 blockHash, bool returnFullTransactionObjects);
        
        /// <summary>Gets the corresponding block information according to the specified block index.</summary>
        /// <param name="blockIndex">The block index</param>
        /// <param name="returnFullTransactionObjects">Whether to get block information with all transaction objects or just the block header</param>
        /// <returns>The request object for the block</returns>
        Request<NeoGetBlockResponse, NeoBlock> GetBlock(int blockIndex, bool returnFullTransactionObjects);
        
        /// <summary>Gets the corresponding raw block information according to the specified block hash.</summary>
        /// <param name="blockHash">The block hash</param>
        /// <returns>The request object for the raw block</returns>
        Request<NeoGetRawBlockResponse, string> GetRawBlock(Hash256 blockHash);
        
        /// <summary>Gets the corresponding raw block information according to the specified block index.</summary>
        /// <param name="blockIndex">The block index</param>
        /// <returns>The request object for the raw block</returns>
        Request<NeoGetRawBlockResponse, string> GetRawBlock(int blockIndex);
        
        /// <summary>Gets the block header count of the blockchain.</summary>
        /// <returns>The request object for the block header count</returns>
        Request<NeoBlockHeaderCountResponse, int> GetBlockHeaderCount();
        
        /// <summary>Gets the block count of the blockchain.</summary>
        /// <returns>The request object for the block count</returns>
        Request<NeoBlockCountResponse, int> GetBlockCount();
        
        /// <summary>Gets the corresponding block header information according to the specified block hash.</summary>
        /// <param name="blockHash">The block hash</param>
        /// <returns>The request object for the block header</returns>
        Request<NeoGetBlockResponse, NeoBlock> GetBlockHeader(Hash256 blockHash);
        
        /// <summary>Gets the corresponding block header information according to the specified index.</summary>
        /// <param name="blockIndex">The block index</param>
        /// <returns>The request object for the block header</returns>
        Request<NeoGetBlockResponse, NeoBlock> GetBlockHeader(int blockIndex);
        
        /// <summary>Gets the corresponding raw block header information according to the specified block hash.</summary>
        /// <param name="blockHash">The block hash</param>
        /// <returns>The request object for the raw block header</returns>
        Request<NeoGetRawBlockResponse, string> GetRawBlockHeader(Hash256 blockHash);
        
        /// <summary>Gets the corresponding raw block header information according to the specified index.</summary>
        /// <param name="blockIndex">The block index</param>
        /// <returns>The request object for the raw block header</returns>
        Request<NeoGetRawBlockResponse, string> GetRawBlockHeader(int blockIndex);
        
        /// <summary>Gets the native contracts list, which includes the basic information of native contracts and the contract descriptive file manifest.json.</summary>
        /// <returns>The request object for native contracts</returns>
        Request<NeoGetNativeContractsResponse, List<NativeContractState>> GetNativeContracts();
        
        /// <summary>Gets the contract information.</summary>
        /// <param name="contractHash">The contract script hash</param>
        /// <returns>The request object for the contract state</returns>
        Request<NeoGetContractStateResponse, ContractState> GetContractState(Hash160 contractHash);
        
        /// <summary>Gets the native contract information by its name. This RPC only works for native contracts.</summary>
        /// <param name="contractName">The name of the native contract</param>
        /// <returns>The request object for the contract state</returns>
        Request<NeoGetContractStateResponse, ContractState> GetNativeContractState(string contractName);
        
        /// <summary>Gets a list of unconfirmed or confirmed transactions in memory.</summary>
        /// <returns>The request object for the memory pool</returns>
        Request<NeoGetMemPoolResponse, NeoGetMemPoolResponse.MemPoolDetails> GetMemPool();
        
        /// <summary>Gets a list of confirmed transactions in memory.</summary>
        /// <returns>The request object for the raw memory pool</returns>
        Request<NeoGetRawMemPoolResponse, List<Hash256>> GetRawMemPool();
        
        /// <summary>Gets the corresponding transaction information based on the specified transaction hash.</summary>
        /// <param name="txHash">The transaction hash</param>
        /// <returns>The request object for the transaction</returns>
        Request<NeoGetTransactionResponse, NeoTransaction> GetTransaction(Hash256 txHash);
        
        /// <summary>Gets the corresponding raw transaction information based on the specified transaction hash.</summary>
        /// <param name="txHash">The transaction hash</param>
        /// <returns>The request object for the raw transaction</returns>
        Request<NeoGetRawTransactionResponse, string> GetRawTransaction(Hash256 txHash);
        
        /// <summary>Gets the stored value according to the contract hash and the key.</summary>
        /// <param name="contractHash">The contract hash</param>
        /// <param name="keyHexString">The key to look up in storage as a hexadecimal string</param>
        /// <returns>The request object for storage value</returns>
        Request<NeoGetStorageResponse, string> GetStorage(Hash160 contractHash, string keyHexString);
        
        /// <summary>Gets the transaction height with the specified transaction hash.</summary>
        /// <param name="txHash">The transaction hash</param>
        /// <returns>The request object for the transaction height</returns>
        Request<NeoGetTransactionHeightResponse, int> GetTransactionHeight(Hash256 txHash);
        
        /// <summary>Gets the validators of the next block.</summary>
        /// <returns>The request object for the next block validators</returns>
        Request<NeoGetNextBlockValidatorsResponse, List<NeoGetNextBlockValidatorsResponse.Validator>> GetNextBlockValidators();
        
        /// <summary>Gets the public key list of current Neo committee members.</summary>
        /// <returns>The request object for committee members</returns>
        Request<NeoGetCommitteeResponse, List<string>> GetCommittee();
        
        #endregion
        
        #region Node Methods
        
        /// <summary>Gets the current number of connections for the node.</summary>
        /// <returns>The request object for connection count</returns>
        Request<NeoConnectionCountResponse, int> GetConnectionCount();
        
        /// <summary>Gets a list of nodes that the node is currently connected or disconnected from.</summary>
        /// <returns>The request object for peers</returns>
        Request<NeoGetPeersResponse, NeoGetPeersResponse.Peers> GetPeers();
        
        /// <summary>Gets the version information of the node.</summary>
        /// <returns>The request object for version information</returns>
        Request<NeoGetVersionResponse, NeoGetVersionResponse.NeoVersion> GetVersion();
        
        /// <summary>Broadcasts a transaction over the Neo network.</summary>
        /// <param name="rawTransactionHex">The raw transaction in hexadecimal</param>
        /// <returns>The request object for sending raw transaction</returns>
        Request<NeoSendRawTransactionResponse, NeoSendRawTransactionResponse.RawTransaction> SendRawTransaction(string rawTransactionHex);
        
        /// <summary>Broadcasts a new block over the Neo network.</summary>
        /// <param name="serializedBlockAsHex">The block in hexadecimal</param>
        /// <returns>The request object for submitting block</returns>
        Request<NeoSubmitBlockResponse, bool> SubmitBlock(string serializedBlockAsHex);
        
        #endregion
        
        #region Smart Contract Methods
        
        /// <summary>Invokes the function with functionName of the smart contract with the specified contract hash.</summary>
        /// <param name="contractHash">The contract hash to invoke</param>
        /// <param name="functionName">The function to invoke</param>
        /// <param name="signers">The signers</param>
        /// <returns>The request object for function invocation</returns>
        Request<NeoInvokeFunctionResponse, InvocationResult> InvokeFunction(Hash160 contractHash, string functionName, List<Signer> signers = null);
        
        /// <summary>Invokes the function with functionName of the smart contract with the specified contract hash.</summary>
        /// <param name="contractHash">The contract hash to invoke</param>
        /// <param name="functionName">The function to invoke</param>
        /// <param name="contractParams">The parameters of the function</param>
        /// <param name="signers">The signers</param>
        /// <returns>The request object for function invocation</returns>
        Request<NeoInvokeFunctionResponse, InvocationResult> InvokeFunction(Hash160 contractHash, string functionName, List<ContractParameter> contractParams, List<Signer> signers = null);
        
        /// <summary>Invokes the function with functionName of the smart contract with the specified contract hash. Includes diagnostics from the invocation.</summary>
        /// <param name="contractHash">The contract hash to invoke</param>
        /// <param name="functionName">The function to invoke</param>
        /// <param name="signers">The signers</param>
        /// <returns>The request object for function invocation with diagnostics</returns>
        Request<NeoInvokeFunctionResponse, InvocationResult> InvokeFunctionDiagnostics(Hash160 contractHash, string functionName, List<Signer> signers = null);
        
        /// <summary>Invokes the function with functionName of the smart contract with the specified contract hash. Includes diagnostics from the invocation.</summary>
        /// <param name="contractHash">The contract hash to invoke</param>
        /// <param name="functionName">The function to invoke</param>
        /// <param name="contractParams">The parameters of the function</param>
        /// <param name="signers">The signers</param>
        /// <returns>The request object for function invocation with diagnostics</returns>
        Request<NeoInvokeFunctionResponse, InvocationResult> InvokeFunctionDiagnostics(Hash160 contractHash, string functionName, List<ContractParameter> contractParams, List<Signer> signers = null);
        
        /// <summary>Invokes a script.</summary>
        /// <param name="scriptHex">The script to invoke</param>
        /// <param name="signers">The signers</param>
        /// <returns>The request object for script invocation</returns>
        Request<NeoInvokeScriptResponse, InvocationResult> InvokeScript(string scriptHex, List<Signer> signers = null);
        
        /// <summary>Invokes a script. Includes diagnostics from the invocation.</summary>
        /// <param name="scriptHex">The script to invoke</param>
        /// <param name="signers">The signers</param>
        /// <returns>The request object for script invocation with diagnostics</returns>
        Request<NeoInvokeScriptResponse, InvocationResult> InvokeScriptDiagnostics(string scriptHex, List<Signer> signers = null);
        
        /// <summary>Returns the results from an iterator. The results are limited to count items.</summary>
        /// <param name="sessionId">The session id</param>
        /// <param name="iteratorId">The iterator id</param>
        /// <param name="count">The maximal number of stack items returned</param>
        /// <returns>The request object for iterator traversal</returns>
        Request<NeoTraverseIteratorResponse, List<StackItem>> TraverseIterator(string sessionId, string iteratorId, int count);
        
        /// <summary>Terminates an open session.</summary>
        /// <param name="sessionId">The session id</param>
        /// <returns>The request object for session termination</returns>
        Request<NeoTerminateSessionResponse, bool> TerminateSession(string sessionId);
        
        /// <summary>Invokes a contract in verification mode. Requires an open wallet on the Neo node that contains the accounts for the signers.</summary>
        /// <param name="contractHash">The contract hash</param>
        /// <param name="methodParameters">A list of parameters of the verify function</param>
        /// <param name="signers">The signers</param>
        /// <returns>The request object for contract verification</returns>
        Request<NeoInvokeContractVerifyResponse, InvocationResult> InvokeContractVerify(Hash160 contractHash, List<ContractParameter> methodParameters = null, List<Signer> signers = null);
        
        /// <summary>Gets the unclaimed GAS of the account with the specified script hash.</summary>
        /// <param name="scriptHash">The account's script hash</param>
        /// <returns>The request object for unclaimed gas</returns>
        Request<NeoGetUnclaimedGasResponse, NeoGetUnclaimedGasResponse.GetUnclaimedGas> GetUnclaimedGas(Hash160 scriptHash);
        
        #endregion
        
        #region Utilities Methods
        
        /// <summary>Gets a list of plugins loaded by the node.</summary>
        /// <returns>The request object for plugins list</returns>
        Request<NeoListPluginsResponse, List<NeoListPluginsResponse.Plugin>> ListPlugins();
        
        /// <summary>Verifies whether the address is a valid Neo address.</summary>
        /// <param name="address">The address to verify</param>
        /// <returns>The request object for address validation</returns>
        Request<NeoValidateAddressResponse, NeoValidateAddressResponse.Result> ValidateAddress(string address);
        
        #endregion
        
        #region Token Tracker Methods - NEP-17
        
        /// <summary>Gets the balance of all NEP-17 token assets in the specified script hash.</summary>
        /// <param name="scriptHash">The account's script hash</param>
        /// <returns>The request object for NEP-17 balances</returns>
        Request<NeoGetNep17BalancesResponse, NeoGetNep17BalancesResponse.Nep17Balances> GetNep17Balances(Hash160 scriptHash);
        
        /// <summary>Gets all the NEP-17 transaction information occurred in the specified script hash.</summary>
        /// <param name="scriptHash">The account's script hash</param>
        /// <returns>The request object for NEP-17 transfers</returns>
        Request<NeoGetNep17TransfersResponse, NeoGetNep17TransfersResponse.Nep17Transfers> GetNep17Transfers(Hash160 scriptHash);
        
        /// <summary>Gets all the NEP-17 transaction information occurred in the specified script hash since the specified time.</summary>
        /// <param name="scriptHash">The account's script hash</param>
        /// <param name="from">The timestamp transactions occurred since</param>
        /// <returns>The request object for NEP-17 transfers</returns>
        Request<NeoGetNep17TransfersResponse, NeoGetNep17TransfersResponse.Nep17Transfers> GetNep17Transfers(Hash160 scriptHash, DateTime from);
        
        /// <summary>Gets all the NEP-17 transaction information occurred in the specified script hash in the specified time range.</summary>
        /// <param name="scriptHash">The account's script hash</param>
        /// <param name="from">The start timestamp</param>
        /// <param name="to">The end timestamp</param>
        /// <returns>The request object for NEP-17 transfers</returns>
        Request<NeoGetNep17TransfersResponse, NeoGetNep17TransfersResponse.Nep17Transfers> GetNep17Transfers(Hash160 scriptHash, DateTime from, DateTime to);
        
        #endregion
        
        #region Token Tracker Methods - NEP-11
        
        /// <summary>Gets all NEP-11 balances of the specified account.</summary>
        /// <param name="scriptHash">The account's script hash</param>
        /// <returns>The request object for NEP-11 balances</returns>
        Request<NeoGetNep11BalancesResponse, NeoGetNep11BalancesResponse.Nep11Balances> GetNep11Balances(Hash160 scriptHash);
        
        /// <summary>Gets all NEP-11 transaction of the given account.</summary>
        /// <param name="scriptHash">The account's script hash</param>
        /// <returns>The request object for NEP-11 transfers</returns>
        Request<NeoGetNep11TransfersResponse, NeoGetNep11TransfersResponse.Nep11Transfers> GetNep11Transfers(Hash160 scriptHash);
        
        /// <summary>Gets all NEP-11 transaction of the given account since the given time.</summary>
        /// <param name="scriptHash">The account's script hash</param>
        /// <param name="from">The date from when to report transactions</param>
        /// <returns>The request object for NEP-11 transfers</returns>
        Request<NeoGetNep11TransfersResponse, NeoGetNep11TransfersResponse.Nep11Transfers> GetNep11Transfers(Hash160 scriptHash, DateTime from);
        
        /// <summary>Gets all NEP-11 transactions of the given account in the time span between from and to.</summary>
        /// <param name="scriptHash">The account's script hash</param>
        /// <param name="from">The start timestamp</param>
        /// <param name="to">The end timestamp</param>
        /// <returns>The request object for NEP-11 transfers</returns>
        Request<NeoGetNep11TransfersResponse, NeoGetNep11TransfersResponse.Nep11Transfers> GetNep11Transfers(Hash160 scriptHash, DateTime from, DateTime to);
        
        /// <summary>Gets the properties of the token with tokenId from the NEP-11 contract with scriptHash.</summary>
        /// <param name="scriptHash">The account's script hash</param>
        /// <param name="tokenId">The ID of the token as a hexadecimal string</param>
        /// <returns>The request object for NEP-11 properties</returns>
        Request<NeoGetNep11PropertiesResponse, Dictionary<string, string>> GetNep11Properties(Hash160 scriptHash, string tokenId);
        
        #endregion
        
        #region Application Logs
        
        /// <summary>Gets the application logs of the specified transaction hash.</summary>
        /// <param name="txHash">The transaction hash</param>
        /// <returns>The request object for application log</returns>
        Request<NeoGetApplicationLogResponse, NeoApplicationLog> GetApplicationLog(Hash256 txHash);
        
        #endregion
        
        #region State Service
        
        /// <summary>Gets the state root by the block height.</summary>
        /// <param name="blockIndex">The block index</param>
        /// <returns>The request object for state root</returns>
        Request<NeoGetStateRootResponse, NeoGetStateRootResponse.StateRoot> GetStateRoot(int blockIndex);
        
        /// <summary>Gets the proof based on the root hash, the contract hash and the storage key.</summary>
        /// <param name="rootHash">The root hash</param>
        /// <param name="contractHash">The contract hash</param>
        /// <param name="storageKeyHex">The storage key</param>
        /// <returns>The request object for proof</returns>
        Request<NeoGetProofResponse, string> GetProof(Hash256 rootHash, Hash160 contractHash, string storageKeyHex);
        
        /// <summary>Verifies the proof data and gets the value of the storage corresponding to the key.</summary>
        /// <param name="rootHash">The root hash</param>
        /// <param name="proofDataHex">The proof data of the state root</param>
        /// <returns>The request object for proof verification</returns>
        Request<NeoVerifyProofResponse, string> VerifyProof(Hash256 rootHash, string proofDataHex);
        
        /// <summary>Gets the state root height.</summary>
        /// <returns>The request object for state height</returns>
        Request<NeoGetStateHeightResponse, NeoGetStateHeightResponse.StateHeight> GetStateHeight();
        
        /// <summary>Gets the state.</summary>
        /// <param name="rootHash">The root hash</param>
        /// <param name="contractHash">The contract hash</param>
        /// <param name="keyHex">The storage key</param>
        /// <returns>The request object for state</returns>
        Request<NeoGetStateResponse, string> GetState(Hash256 rootHash, Hash160 contractHash, string keyHex);
        
        /// <summary>Gets a list of states that match the provided key prefix. Includes proofs of the first and last entry.</summary>
        /// <param name="rootHash">The root hash</param>
        /// <param name="contractHash">The contract hash</param>
        /// <param name="keyPrefixHex">The key prefix</param>
        /// <param name="startKeyHex">The start key</param>
        /// <param name="countFindResultItems">The number of results</param>
        /// <returns>The request object for finding states</returns>
        Request<NeoFindStatesResponse, NeoFindStatesResponse.States> FindStates(Hash256 rootHash, Hash160 contractHash, string keyPrefixHex, string startKeyHex = null, int? countFindResultItems = null);
        
        #endregion
    }
}
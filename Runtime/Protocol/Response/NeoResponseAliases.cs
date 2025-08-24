using System;
using System.Collections.Generic;
using UnityEngine;
using NeoUnity.Cryptography;

namespace NeoUnity.Protocol.Response
{
    /// <summary>
    /// Provides type aliases and convenient response classes for common RPC methods
    /// </summary>
    
    // Basic response types with single result values
    
    /// <summary>
    /// Response for getblockcount RPC method
    /// </summary>
    [Serializable]
    public class NeoBlockCountResponse : NeoResponse<int>
    {
        /// <summary>Gets the current block count</summary>
        public int BlockCount => Result;
    }
    
    /// <summary>
    /// Response for getblockhash RPC method
    /// </summary>
    [Serializable]
    public class NeoBlockHashResponse : NeoResponse<string>
    {
        /// <summary>Gets the block hash as string</summary>
        public string BlockHashString => Result;
        
        /// <summary>Gets the block hash as Hash256</summary>
        public Hash256 BlockHash => new Hash256(Result);
    }
    
    /// <summary>
    /// Alias for connection count (used by multiple methods)
    /// </summary>
    [Serializable]
    public class NeoConnectionCountResponse : NeoResponse<int>
    {
        /// <summary>Gets the connection count</summary>
        public int Count => Result;
    }
    
    /// <summary>
    /// Alias for block header count (same as connection count)
    /// </summary>
    [Serializable]
    public class NeoBlockHeaderCountResponse : NeoConnectionCountResponse { }
    
    /// <summary>
    /// Response for calculatenetworkfee RPC method
    /// </summary>
    [Serializable]
    public class NeoCalculateNetworkFeeResponse : NeoResponse<NeoNetworkFee>
    {
        /// <summary>Gets the calculated network fee</summary>
        public NeoNetworkFee NetworkFee => Result;
    }
    
    /// <summary>
    /// Response for closewallet RPC method
    /// </summary>
    [Serializable]
    public class NeoCloseWalletResponse : NeoResponse<bool>
    {
        /// <summary>Gets whether wallet was closed successfully</summary>
        public bool CloseWallet => Result;
    }
    
    /// <summary>
    /// Response for dumpprivkey RPC method
    /// </summary>
    [Serializable]
    public class NeoDumpPrivKeyResponse : NeoResponse<string>
    {
        /// <summary>Gets the private key as string</summary>
        public string PrivateKey => Result;
    }
    
    // Express-specific response types
    
    /// <summary>
    /// Response for express createcheckpoint RPC method
    /// </summary>
    [Serializable]
    public class NeoExpressCreateCheckpointResponse : NeoResponse<string>
    {
        /// <summary>Gets the checkpoint filename</summary>
        public string Filename => Result;
    }
    
    /// <summary>
    /// Response for express createoracleresponsetx RPC method
    /// </summary>
    [Serializable]
    public class NeoExpressCreateOracleResponseTxResponse : NeoResponse<string>
    {
        /// <summary>Gets the oracle response transaction</summary>
        public string OracleResponseTx => Result;
    }
    
    /// <summary>
    /// Response for express getcontractstorage RPC method
    /// </summary>
    [Serializable]
    public class NeoExpressGetContractStorageResponse : NeoResponse<List<ContractStorageEntry>>
    {
        /// <summary>Gets the contract storage entries</summary>
        public List<ContractStorageEntry> ContractStorage => Result ?? new List<ContractStorageEntry>();
    }
    
    /// <summary>
    /// Response for express getnep17contracts RPC method
    /// </summary>
    [Serializable]
    public class NeoExpressGetNep17ContractsResponse : NeoResponse<List<Nep17Contract>>
    {
        /// <summary>Gets the NEP-17 contracts</summary>
        public List<Nep17Contract> Nep17Contracts => Result ?? new List<Nep17Contract>();
    }
    
    /// <summary>
    /// Response for express getpopulatedblocks RPC method
    /// </summary>
    [Serializable]
    public class NeoExpressGetPopulatedBlocksResponse : NeoResponse<PopulatedBlocks>
    {
        /// <summary>Gets the populated blocks</summary>
        public PopulatedBlocks PopulatedBlocks => Result;
    }
    
    /// <summary>
    /// Response for express listcontracts RPC method
    /// </summary>
    [Serializable]
    public class NeoExpressListContractsResponse : NeoResponse<List<ExpressContractState>>
    {
        /// <summary>Gets the contracts</summary>
        public List<ExpressContractState> Contracts => Result ?? new List<ExpressContractState>();
    }
    
    /// <summary>
    /// Response for express listoraclerequests RPC method
    /// </summary>
    [Serializable]
    public class NeoExpressListOracleRequestsResponse : NeoResponse<List<OracleRequest>>
    {
        /// <summary>Gets the oracle requests</summary>
        public List<OracleRequest> OracleRequests => Result ?? new List<OracleRequest>();
    }
    
    /// <summary>
    /// Response for express shutdown RPC method
    /// </summary>
    [Serializable]
    public class NeoExpressShutdownResponse : NeoResponse<ExpressShutdown>
    {
        /// <summary>Gets the express shutdown info</summary>
        public ExpressShutdown ExpressShutdown => Result;
    }
    
    // Standard Neo RPC response types
    
    /// <summary>
    /// Response for getapplicationlog RPC method
    /// </summary>
    [Serializable]
    public class NeoGetApplicationLogResponse : NeoResponse<NeoApplicationLog>
    {
        /// <summary>Gets the application log</summary>
        public NeoApplicationLog ApplicationLog => Result;
    }
    
    /// <summary>
    /// Response for getblock RPC method
    /// </summary>
    [Serializable]
    public class NeoGetBlockResponse : NeoResponse<NeoBlock>
    {
        /// <summary>Gets the block</summary>
        public NeoBlock Block => Result;
    }
    
    /// <summary>
    /// Response for getcommittee RPC method
    /// </summary>
    [Serializable]
    public class NeoGetCommitteeResponse : NeoResponse<List<string>>
    {
        /// <summary>Gets the committee members</summary>
        public List<string> Committee => Result ?? new List<string>();
    }
    
    /// <summary>
    /// Response for getcontractstate RPC method
    /// </summary>
    [Serializable]
    public class NeoGetContractStateResponse : NeoResponse<ContractState>
    {
        /// <summary>Gets the contract state</summary>
        public ContractState ContractState => Result;
    }
    
    /// <summary>
    /// Response for getnativecontracts RPC method
    /// </summary>
    [Serializable]
    public class NeoGetNativeContractsResponse : NeoResponse<List<NativeContractState>>
    {
        /// <summary>Gets the native contracts</summary>
        public List<NativeContractState> NativeContracts => Result ?? new List<NativeContractState>();
    }
    
    /// <summary>
    /// Response for getnep11properties RPC method
    /// </summary>
    [Serializable]
    public class NeoGetNep11PropertiesResponse : NeoResponse<Dictionary<string, string>>
    {
        /// <summary>Gets the NEP-11 properties</summary>
        public Dictionary<string, string> Properties => Result ?? new Dictionary<string, string>();
    }
    
    /// <summary>
    /// Response for getnewaddress RPC method
    /// </summary>
    [Serializable]
    public class NeoGetNewAddressResponse : NeoResponse<string>
    {
        /// <summary>Gets the new address</summary>
        public string Address => Result;
    }
    
    /// <summary>
    /// Response for getrawblock RPC method
    /// </summary>
    [Serializable]
    public class NeoGetRawBlockResponse : NeoResponse<string>
    {
        /// <summary>Gets the raw block data</summary>
        public string RawBlock => Result;
    }
    
    /// <summary>
    /// Response for getrawmempool RPC method
    /// </summary>
    [Serializable]
    public class NeoGetRawMemPoolResponse : NeoResponse<List<string>>
    {
        /// <summary>Gets the raw mempool transaction hashes</summary>
        public List<string> TransactionHashes => Result ?? new List<string>();
        
        /// <summary>Gets the transaction hashes as Hash256 objects</summary>
        public List<Hash256> TransactionHash256s
        {
            get
            {
                var hashes = new List<Hash256>();
                if (Result != null)
                {
                    foreach (var hash in Result)
                    {
                        hashes.Add(new Hash256(hash));
                    }
                }
                return hashes;
            }
        }
    }
    
    /// <summary>
    /// Response for getrawtransaction RPC method
    /// </summary>
    [Serializable]
    public class NeoGetRawTransactionResponse : NeoResponse<string>
    {
        /// <summary>Gets the raw transaction data</summary>
        public string RawTransaction => Result;
    }
    
    /// <summary>
    /// Response for getstorage RPC method
    /// </summary>
    [Serializable]
    public class NeoGetStorageResponse : NeoResponse<string>
    {
        /// <summary>Gets the storage value</summary>
        public string Storage => Result;
    }
    
    /// <summary>
    /// Response for gettransaction RPC method
    /// </summary>
    [Serializable]
    public class NeoGetTransactionResponse : NeoResponse<NeoTransaction>
    {
        /// <summary>Gets the transaction</summary>
        public NeoTransaction Transaction => Result;
    }
    
    /// <summary>
    /// Response for getwalletheight RPC method
    /// </summary>
    [Serializable]
    public class NeoGetWalletHeightResponse : NeoResponse<int>
    {
        /// <summary>Gets the wallet height</summary>
        public int Height => Result;
    }
    
    /// <summary>
    /// Alias for transaction height (same as wallet height)
    /// </summary>
    [Serializable]
    public class NeoGetTransactionHeightResponse : NeoGetWalletHeightResponse { }
    
    /// <summary>
    /// Response for getwalletunclaimedgas RPC method
    /// </summary>
    [Serializable]
    public class NeoGetWalletUnclaimedGasResponse : NeoResponse<string>
    {
        /// <summary>Gets the wallet unclaimed GAS</summary>
        public string WalletUnclaimedGas => Result;
        
        /// <summary>Gets the unclaimed GAS as decimal</summary>
        public decimal WalletUnclaimedGasDecimal => decimal.TryParse(Result, out var result) ? result / 100_000_000m : 0m;
    }
    
    /// <summary>
    /// Response for importprivkey RPC method
    /// </summary>
    [Serializable]
    public class NeoImportPrivKeyResponse : NeoResponse<NeoAddress>
    {
        /// <summary>Gets the imported address</summary>
        public NeoAddress Address => Result;
    }
    
    /// <summary>
    /// Response for invoke* RPC methods
    /// </summary>
    [Serializable]
    public class NeoInvokeResponse : NeoResponse<InvocationResult>
    {
        /// <summary>Gets the invocation result</summary>
        public InvocationResult InvocationResult => Result;
    }
    
    /// <summary>
    /// Alias for invokecontractverify (same as invoke)
    /// </summary>
    [Serializable]
    public class NeoInvokeContractVerifyResponse : NeoInvokeResponse { }
    
    /// <summary>
    /// Alias for invokefunction (same as invoke)
    /// </summary>
    [Serializable]
    public class NeoInvokeFunctionResponse : NeoInvokeResponse { }
    
    /// <summary>
    /// Alias for invokescript (same as invoke)
    /// </summary>
    [Serializable]
    public class NeoInvokeScriptResponse : NeoInvokeResponse { }
    
    /// <summary>
    /// Response for listaddress RPC method
    /// </summary>
    [Serializable]
    public class NeoListAddressResponse : NeoResponse<List<NeoAddress>>
    {
        /// <summary>Gets the addresses</summary>
        public List<NeoAddress> Addresses => Result ?? new List<NeoAddress>();
    }
    
    /// <summary>
    /// Response for openwallet RPC method
    /// </summary>
    [Serializable]
    public class NeoOpenWalletResponse : NeoResponse<bool>
    {
        /// <summary>Gets whether wallet was opened successfully</summary>
        public bool OpenWallet => Result;
    }
    
    /// <summary>
    /// Response for sendfrom RPC method
    /// </summary>
    [Serializable]
    public class NeoSendFromResponse : NeoResponse<NeoTransaction>
    {
        /// <summary>Gets the send from transaction</summary>
        public NeoTransaction SendFromTransaction => Result;
    }
    
    /// <summary>
    /// Response for sendmany RPC method
    /// </summary>
    [Serializable]
    public class NeoSendManyResponse : NeoResponse<NeoTransaction>
    {
        /// <summary>Gets the send many transaction</summary>
        public NeoTransaction SendManyTransaction => Result;
    }
    
    /// <summary>
    /// Response for sendtoaddress RPC method
    /// </summary>
    [Serializable]
    public class NeoSendToAddressResponse : NeoResponse<NeoTransaction>
    {
        /// <summary>Gets the send to address transaction</summary>
        public NeoTransaction SendToAddressTransaction => Result;
    }
    
    /// <summary>
    /// Response for submitblock RPC method
    /// </summary>
    [Serializable]
    public class NeoSubmitBlockResponse : NeoResponse<bool>
    {
        /// <summary>Gets whether block was submitted successfully</summary>
        public bool SubmitBlock => Result;
    }
    
    /// <summary>
    /// Response for terminatesession RPC method
    /// </summary>
    [Serializable]
    public class NeoTerminateSessionResponse : NeoResponse<bool>
    {
        /// <summary>Gets whether session was terminated successfully</summary>
        public bool TerminateSession => Result;
    }
}
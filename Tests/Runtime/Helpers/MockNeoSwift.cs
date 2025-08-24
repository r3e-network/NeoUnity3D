using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Neo.Unity.SDK.Core;
using Neo.Unity.SDK.Types;
using Neo.Unity.SDK.Protocol.Response;
using Neo.Unity.SDK.Contracts;
using Neo.Unity.SDK.Transaction;
using System.Linq;

namespace Neo.Unity.SDK.Tests.Helpers
{
    /// <summary>
    /// Mock implementation of INeoUnity for testing
    /// </summary>
    public class MockNeoSwift : INeoUnity
    {
        private readonly Dictionary<string, object> _mockResponses;
        private readonly Dictionary<string, Dictionary<string, object>> _mockInvokeFunctions;
        private Action<object> _requestInterceptor;
        private bool _shouldTimeout;
        private TimeSpan _timeoutDelay;
        private string _errorMessage;
        private int _errorCode;
        private int _requestId = 0;

        public MockNeoSwift()
        {
            _mockResponses = new Dictionary<string, object>();
            _mockInvokeFunctions = new Dictionary<string, Dictionary<string, object>>();
        }

        public string Url { get; set; } = "mock://test";

        #region Setup Methods

        public void SetupRequestInterceptor(Action<object> interceptor)
        {
            _requestInterceptor = interceptor;
        }

        public void SetupTimeout(TimeSpan delay)
        {
            _shouldTimeout = true;
            _timeoutDelay = delay;
        }

        public void SetupErrorResponse(string message, int code)
        {
            _errorMessage = message;
            _errorCode = code;
        }

        public void SetupGetContractState()
        {
            var manifest = new ContractManifest
            {
                Name = "neow3j"
            };
            _mockResponses["getcontractstate"] = new NeoGetContractStateResponse { Manifest = manifest };
        }

        public void SetupInvokeFunction(string functionName, object returnValue)
        {
            if (!_mockInvokeFunctions.ContainsKey("invokefunction"))
                _mockInvokeFunctions["invokefunction"] = new Dictionary<string, object>();

            var stackItems = new List<StackItem>();
            if (returnValue is string str)
            {
                stackItems.Add(new StackItem { Type = "ByteString", Value = str });
            }
            else if (returnValue is int || returnValue is long)
            {
                stackItems.Add(new StackItem { Type = "Integer", Value = returnValue });
            }
            else if (returnValue is bool)
            {
                stackItems.Add(new StackItem { Type = "Boolean", Value = returnValue });
            }
            else if (returnValue is Hash160 hash160)
            {
                stackItems.Add(new StackItem { Type = "ByteString", Value = hash160.ToArray() });
            }
            else
            {
                stackItems.Add(new StackItem { Type = "Any", Value = returnValue });
            }

            var result = new NeoInvokeFunctionResponse
            {
                InvocationResult = new InvocationResult
                {
                    Stack = stackItems,
                    State = "HALT",
                    GasConsumed = "1000000"
                }
            };

            _mockInvokeFunctions["invokefunction"][functionName] = result;
        }

        public void SetupInvokeFunctionWithIterator(string functionName, string iteratorId, string sessionId)
        {
            if (!_mockInvokeFunctions.ContainsKey("invokefunction"))
                _mockInvokeFunctions["invokefunction"] = new Dictionary<string, object>();

            var result = new NeoInvokeFunctionResponse
            {
                InvocationResult = new InvocationResult
                {
                    Stack = new List<StackItem>
                    {
                        new StackItem { Type = "InteropInterface", Value = iteratorId }
                    },
                    State = "HALT",
                    GasConsumed = "1000000",
                    SessionId = sessionId
                }
            };

            _mockInvokeFunctions["invokefunction"][functionName] = result;
        }

        public void SetupInvokeFunctionWithoutSession(string functionName)
        {
            if (!_mockInvokeFunctions.ContainsKey("invokefunction"))
                _mockInvokeFunctions["invokefunction"] = new Dictionary<string, object>();

            var result = new NeoInvokeFunctionResponse
            {
                InvocationResult = new InvocationResult
                {
                    Stack = new List<StackItem>
                    {
                        new StackItem { Type = "InteropInterface", Value = "iterator_id" }
                    },
                    State = "HALT",
                    GasConsumed = "1000000"
                }
            };

            _mockInvokeFunctions["invokefunction"][functionName] = result;
        }

        public void SetupInvokeFunctionWithError(string functionName, string errorMessage)
        {
            if (!_mockInvokeFunctions.ContainsKey("invokefunction"))
                _mockInvokeFunctions["invokefunction"] = new Dictionary<string, object>();

            var result = new NeoInvokeFunctionResponse
            {
                InvocationResult = new InvocationResult
                {
                    Stack = new List<StackItem>(),
                    State = "FAULT",
                    Exception = errorMessage,
                    GasConsumed = "1000000"
                }
            };

            _mockInvokeFunctions["invokefunction"][functionName] = result;
        }

        public void SetupTraverseIterator(string iteratorId, string sessionId, string[] results)
        {
            var stackItems = results.Select(r => new StackItem { Type = "ByteString", Value = r }).ToList();
            _mockResponses["traverseiterator"] = new NeoTraverseIteratorResponse
            {
                Result = stackItems
            };
        }

        public void SetupTraverseIteratorComplex(string iteratorId, string sessionId, object[] results)
        {
            var stackItems = results.Select(r => new StackItem { Type = "Array", Value = r }).ToList();
            _mockResponses["traverseiterator"] = new NeoTraverseIteratorResponse
            {
                Result = stackItems
            };
        }

        public void SetupTerminateSession(string sessionId)
        {
            _mockResponses["terminatesession"] = new NeoTerminateSessionResponse
            {
                Result = true
            };
        }

        public void SetupInvokeScriptWithArray(string method, Hash160[] addresses)
        {
            var stackItems = addresses.Select(addr => new StackItem 
            { 
                Type = "ByteString", 
                Value = new { Address = addr.ToAddress() }
            }).ToList();

            _mockResponses["invokescript"] = new NeoInvokeScriptResponse
            {
                InvocationResult = new InvocationResult
                {
                    Stack = stackItems,
                    State = "HALT",
                    GasConsumed = "1000000"
                }
            };
        }

        #endregion

        #region INeoUnity Implementation

        public async Task<NeoGetBestBlockHashResponse> GetBestBlockHash()
        {
            _requestInterceptor?.Invoke(CreateRequest("getbestblockhash"));
            
            if (_shouldTimeout)
                await Task.Delay(_timeoutDelay);
                
            if (!string.IsNullOrEmpty(_errorMessage))
                throw new NeoUnityException(_errorMessage);

            return new NeoGetBestBlockHashResponse { Result = "mock_best_block_hash" };
        }

        public async Task<NeoGetBlockHashResponse> GetBlockHash(int blockIndex)
        {
            _requestInterceptor?.Invoke(CreateRequest("getblockhash", blockIndex));
            return new NeoGetBlockHashResponse { Result = $"mock_block_hash_{blockIndex}" };
        }

        public async Task<NeoGetBlockResponse> GetBlock(int blockIndex, bool fullTransactionObjects = true)
        {
            var method = fullTransactionObjects ? "getblock" : "getblockheader";
            _requestInterceptor?.Invoke(CreateRequest(method, blockIndex, fullTransactionObjects ? 1 : 1));
            
            return new NeoGetBlockResponse
            {
                Result = new NeoBlock
                {
                    Hash = $"mock_block_hash_{blockIndex}",
                    Index = blockIndex,
                    Time = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
                }
            };
        }

        public async Task<NeoGetBlockResponse> GetBlock(Hash256 blockHash, bool fullTransactionObjects = true)
        {
            var method = fullTransactionObjects ? "getblock" : "getblockheader";
            _requestInterceptor?.Invoke(CreateRequest(method, blockHash.ToString(), fullTransactionObjects ? 1 : 1));
            
            return new NeoGetBlockResponse
            {
                Result = new NeoBlock
                {
                    Hash = blockHash.ToString(),
                    Index = 12345,
                    Time = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
                }
            };
        }

        public async Task<NeoGetBlockResponse> GetRawBlock(int blockIndex)
        {
            _requestInterceptor?.Invoke(CreateRequest("getblock", blockIndex, 0));
            return new NeoGetBlockResponse { Result = new NeoBlock { Hash = $"raw_block_{blockIndex}" } };
        }

        public async Task<NeoGetBlockCountResponse> GetBlockHeaderCount()
        {
            _requestInterceptor?.Invoke(CreateRequest("getblockheadercount"));
            return new NeoGetBlockCountResponse { Result = 1000 };
        }

        public async Task<NeoGetBlockCountResponse> GetBlockCount()
        {
            _requestInterceptor?.Invoke(CreateRequest("getblockcount"));
            
            if (!string.IsNullOrEmpty(_errorMessage))
                throw new NeoUnityException(_errorMessage);
                
            return new NeoGetBlockCountResponse { Result = 1000 };
        }

        public async Task<NeoListNativeContractsResponse> GetNativeContracts()
        {
            _requestInterceptor?.Invoke(CreateRequest("getnativecontracts"));
            return new NeoListNativeContractsResponse { Result = new List<NativeContractState>() };
        }

        public async Task<NeoGetBlockResponse> GetBlockHeader(int blockIndex)
        {
            _requestInterceptor?.Invoke(CreateRequest("getblockheader", blockIndex, 1));
            return new NeoGetBlockResponse { Result = new NeoBlock { Hash = $"header_{blockIndex}" } };
        }

        public async Task<NeoGetBlockResponse> GetRawBlockHeader(int blockIndex)
        {
            _requestInterceptor?.Invoke(CreateRequest("getblockheader", blockIndex, 0));
            return new NeoGetBlockResponse { Result = new NeoBlock { Hash = $"raw_header_{blockIndex}" } };
        }

        public async Task<NeoGetContractStateResponse> GetContractState(Hash160 contractHash)
        {
            _requestInterceptor?.Invoke(CreateRequest("getcontractstate", contractHash.ToString()));
            
            if (_mockResponses.ContainsKey("getcontractstate"))
                return _mockResponses["getcontractstate"] as NeoGetContractStateResponse;
                
            return new NeoGetContractStateResponse { Manifest = new ContractManifest { Name = "mock_contract" } };
        }

        public async Task<NeoGetContractStateResponse> GetNativeContractState(string contractName)
        {
            _requestInterceptor?.Invoke(CreateRequest("getcontractstate", contractName));
            return new NeoGetContractStateResponse { Manifest = new ContractManifest { Name = contractName } };
        }

        public async Task<NeoGetMemPoolResponse> GetMemPool()
        {
            _requestInterceptor?.Invoke(CreateRequest("getrawmempool", 1));
            return new NeoGetMemPoolResponse { Result = new MemPool() };
        }

        public async Task<NeoGetRawMemPoolResponse> GetRawMemPool()
        {
            _requestInterceptor?.Invoke(CreateRequest("getrawmempool"));
            return new NeoGetRawMemPoolResponse { Result = new List<string>() };
        }

        public async Task<NeoGetTransactionResponse> GetTransaction(Hash256 transactionHash)
        {
            _requestInterceptor?.Invoke(CreateRequest("getrawtransaction", transactionHash.ToString(), 1));
            return new NeoGetTransactionResponse { Result = new NeoTransaction { Hash = transactionHash.ToString() } };
        }

        public async Task<NeoGetTransactionResponse> GetRawTransaction(Hash256 transactionHash)
        {
            _requestInterceptor?.Invoke(CreateRequest("getrawtransaction", transactionHash.ToString(), 0));
            return new NeoGetTransactionResponse { Result = new NeoTransaction { Hash = transactionHash.ToString() } };
        }

        public async Task<NeoGetStorageResponse> GetStorage(Hash160 contractHash, string key)
        {
            var base64Key = Convert.ToBase64String(ByteExtensions.HexToBytes(key));
            _requestInterceptor?.Invoke(CreateRequest("getstorage", contractHash.ToString(), base64Key));
            return new NeoGetStorageResponse { Result = "mock_storage_value" };
        }

        public async Task<NeoGetTransactionHeightResponse> GetTransactionHeight(Hash256 transactionHash)
        {
            _requestInterceptor?.Invoke(CreateRequest("gettransactionheight", transactionHash.ToString()));
            return new NeoGetTransactionHeightResponse { Result = 12345 };
        }

        public async Task<NeoGetNextBlockValidatorsResponse> GetNextBlockValidators()
        {
            _requestInterceptor?.Invoke(CreateRequest("getnextblockvalidators"));
            return new NeoGetNextBlockValidatorsResponse { Result = new List<string>() };
        }

        public async Task<NeoGetCommitteeResponse> GetCommittee()
        {
            _requestInterceptor?.Invoke(CreateRequest("getcommittee"));
            return new NeoGetCommitteeResponse { Result = new List<string>() };
        }

        public async Task<NeoGetConnectionCountResponse> GetConnectionCount()
        {
            _requestInterceptor?.Invoke(CreateRequest("getconnectioncount"));
            return new NeoGetConnectionCountResponse { Result = 10 };
        }

        public async Task<NeoGetPeersResponse> GetPeers()
        {
            _requestInterceptor?.Invoke(CreateRequest("getpeers"));
            return new NeoGetPeersResponse { Result = new Peers() };
        }

        public async Task<NeoGetVersionResponse> GetVersion()
        {
            _requestInterceptor?.Invoke(CreateRequest("getversion"));
            return new NeoGetVersionResponse { Result = new Version() };
        }

        public async Task<NeoSendRawTransactionResponse> SendRawTransaction(string rawTransactionHex)
        {
            _requestInterceptor?.Invoke(CreateRequest("sendrawtransaction", rawTransactionHex));
            return new NeoSendRawTransactionResponse { Result = new SendRawTransaction() };
        }

        public async Task<NeoSubmitBlockResponse> SubmitBlock(string blockHex)
        {
            _requestInterceptor?.Invoke(CreateRequest("submitblock", blockHex));
            return new NeoSubmitBlockResponse { Result = true };
        }

        public async Task<NeoInvokeFunctionResponse> InvokeFunction(Hash160 contractHash, string functionName, List<ContractParameter> parameters, List<AccountSigner> signers = null)
        {
            var signerArray = signers?.Select(s => new
            {
                allowedcontracts = s.AllowedContracts?.Select(c => c.ToString()).ToList() ?? new List<string>(),
                account = s.Account.ToString(),
                rules = s.Rules?.Select(r => new
                {
                    condition = new
                    {
                        type = r.Condition.GetType().Name.Replace("WitnessCondition", ""),
                        hash = r.Condition is CalledByContractCondition cbc ? cbc.Hash.ToString() : null
                    },
                    action = r.Action.ToString()
                }).ToList() ?? new List<object>(),
                allowedgroups = s.AllowedGroups?.Select(g => g.ToString()).ToList() ?? new List<string>(),
                scopes = string.Join(",", Enum.GetValues<WitnessScope>().Where(scope => s.Scope.HasFlag(scope)).Select(scope => scope.ToString()))
            }).ToList() ?? new List<object>();

            _requestInterceptor?.Invoke(CreateRequest("invokefunction", contractHash.ToString(), functionName, parameters.ToArray(), signerArray.ToArray()));
            
            if (_mockInvokeFunctions.ContainsKey("invokefunction") && _mockInvokeFunctions["invokefunction"].ContainsKey(functionName))
                return _mockInvokeFunctions["invokefunction"][functionName] as NeoInvokeFunctionResponse;
                
            return new NeoInvokeFunctionResponse
            {
                InvocationResult = new InvocationResult
                {
                    Stack = new List<StackItem> { new StackItem { Type = "Any", Value = "mock_result" } },
                    State = "HALT"
                }
            };
        }

        public async Task<NeoInvokeFunctionResponse> InvokeFunctionDiagnostics(Hash160 contractHash, string functionName, List<ContractParameter> parameters, List<AccountSigner> signers = null)
        {
            _requestInterceptor?.Invoke(CreateRequest("invokefunction", contractHash.ToString(), functionName, parameters.ToArray(), signers?.ToArray() ?? new object[0], 1));
            return await InvokeFunction(contractHash, functionName, parameters, signers);
        }

        public async Task<NeoInvokeScriptResponse> InvokeScript(string scriptHex, List<AccountSigner> signers = null)
        {
            var scriptBase64 = Convert.ToBase64String(ByteExtensions.HexToBytes(scriptHex));
            _requestInterceptor?.Invoke(CreateRequest("invokescript", scriptBase64, signers?.ToArray() ?? new object[0]));
            
            if (_mockResponses.ContainsKey("invokescript"))
                return _mockResponses["invokescript"] as NeoInvokeScriptResponse;
                
            return new NeoInvokeScriptResponse
            {
                InvocationResult = new InvocationResult
                {
                    Stack = new List<StackItem>(),
                    State = "HALT"
                }
            };
        }

        public async Task<NeoInvokeScriptResponse> InvokeScriptDiagnostics(string scriptHex, List<AccountSigner> signers = null)
        {
            var scriptBase64 = Convert.ToBase64String(ByteExtensions.HexToBytes(scriptHex));
            _requestInterceptor?.Invoke(CreateRequest("invokescript", scriptBase64, signers?.ToArray() ?? new object[0], 1));
            return await InvokeScript(scriptHex, signers);
        }

        public async Task<NeoTraverseIteratorResponse> TraverseIterator(string sessionId, string iteratorId, int count)
        {
            _requestInterceptor?.Invoke(CreateRequest("traverseiterator", sessionId, iteratorId, count));
            
            if (_mockResponses.ContainsKey("traverseiterator"))
                return _mockResponses["traverseiterator"] as NeoTraverseIteratorResponse;
                
            return new NeoTraverseIteratorResponse { Result = new List<StackItem>() };
        }

        public async Task<NeoTerminateSessionResponse> TerminateSession(string sessionId)
        {
            _requestInterceptor?.Invoke(CreateRequest("terminatesession", sessionId));
            
            if (_mockResponses.ContainsKey("terminatesession"))
                return _mockResponses["terminatesession"] as NeoTerminateSessionResponse;
                
            return new NeoTerminateSessionResponse { Result = true };
        }

        public async Task<NeoListPluginsResponse> ListPlugins()
        {
            _requestInterceptor?.Invoke(CreateRequest("listplugins"));
            return new NeoListPluginsResponse { Result = new List<Plugin>() };
        }

        public async Task<NeoValidateAddressResponse> ValidateAddress(string address)
        {
            _requestInterceptor?.Invoke(CreateRequest("validateaddress", address));
            return new NeoValidateAddressResponse { Result = new AddressValidation() };
        }

        public async Task<NeoGetNep17TransfersResponse> GetNep17Transfers(Hash160 accountHash, DateTime? fromDate = null, DateTime? toDate = null)
        {
            var parameters = new List<object> { accountHash.ToAddress() };
            if (fromDate.HasValue)
                parameters.Add(((DateTimeOffset)fromDate.Value).ToUnixTimeSeconds());
            if (toDate.HasValue)
                parameters.Add(((DateTimeOffset)toDate.Value).ToUnixTimeSeconds());
                
            _requestInterceptor?.Invoke(CreateRequest("getnep17transfers", parameters.ToArray()));
            return new NeoGetNep17TransfersResponse { Result = new Nep17Transfers() };
        }

        public async Task<NeoGetNep17BalancesResponse> GetNep17Balances(Hash160 accountHash)
        {
            _requestInterceptor?.Invoke(CreateRequest("getnep17balances", accountHash.ToAddress()));
            return new NeoGetNep17BalancesResponse { Result = new Nep17Balances() };
        }

        public async Task<NeoApplicationLogResponse> GetApplicationLog(Hash256 transactionHash)
        {
            _requestInterceptor?.Invoke(CreateRequest("getapplicationlog", transactionHash.ToString()));
            return new NeoApplicationLogResponse { Result = new NeoApplicationLog() };
        }

        public async Task<NeoGetStateRootResponse> GetStateRoot(int blockIndex)
        {
            _requestInterceptor?.Invoke(CreateRequest("getstateroot", blockIndex));
            return new NeoGetStateRootResponse { Result = new StateRoot() };
        }

        public async Task<NeoGetStateHeightResponse> GetStateHeight()
        {
            _requestInterceptor?.Invoke(CreateRequest("getstateheight"));
            return new NeoGetStateHeightResponse { Result = new StateHeight() };
        }

        #endregion

        private object CreateRequest(string method, params object[] parameters)
        {
            return new
            {
                jsonrpc = "2.0",
                method = method,
                id = ++_requestId,
                @params = parameters
            };
        }

        public void Dispose()
        {
            _mockResponses.Clear();
            _mockInvokeFunctions.Clear();
        }
    }

    public class NeoUnityException : Exception
    {
        public NeoUnityException(string message) : base(message) { }
        public NeoUnityException(string message, Exception innerException) : base(message, innerException) { }
    }

    public class ContractException : Exception
    {
        public ContractException(string message) : base(message) { }
        public ContractException(string message, Exception innerException) : base(message, innerException) { }
    }

    // Condition classes for witness rules
    public abstract class WitnessConditionBase { }
    
    public class CalledByContractCondition : WitnessConditionBase
    {
        public Hash160 Hash { get; set; }
        public CalledByContractCondition(Hash160 hash) { Hash = hash; }
    }

    public static class WitnessCondition
    {
        public static CalledByContractCondition CalledByContract(Hash160 hash)
        {
            return new CalledByContractCondition(hash);
        }
    }
}
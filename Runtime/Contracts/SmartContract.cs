using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Neo.Unity.SDK.Core;
using Neo.Unity.SDK.Types;
using Neo.Unity.SDK.Script;
using Neo.Unity.SDK.Transaction;
using Neo.Unity.SDK.Protocol.Response;

namespace Neo.Unity.SDK.Contracts
{
    /// <summary>
    /// Represents a smart contract on the Neo blockchain and provides methods to invoke and deploy it.
    /// Unity-optimized base class for all smart contract interactions.
    /// </summary>
    [System.Serializable]
    public class SmartContract
    {
        #region Constants
        
        /// <summary>Default maximum number of items to retrieve from an iterator</summary>
        public const int DEFAULT_ITERATOR_COUNT = 100;
        
        #endregion
        
        #region Properties
        
        /// <summary>The script hash of this smart contract</summary>
        public Hash160 ScriptHash { get; protected set; }
        
        /// <summary>The NeoUnity instance used for all invocations</summary>
        protected NeoUnity NeoUnity { get; set; }
        
        #endregion
        
        #region Constructor
        
        /// <summary>
        /// Constructs a SmartContract representing the smart contract with the given script hash.
        /// Uses the given NeoUnity instance for all invocations.
        /// </summary>
        /// <param name="scriptHash">The smart contract's script hash</param>
        /// <param name="neoUnity">The NeoUnity instance to use for invocations</param>
        public SmartContract(Hash160 scriptHash, NeoUnity neoUnity)
        {
            ScriptHash = scriptHash ?? throw new ArgumentNullException(nameof(scriptHash));
            NeoUnity = neoUnity ?? throw new ArgumentNullException(nameof(neoUnity));
        }
        
        /// <summary>
        /// Constructs a SmartContract using the singleton NeoUnity instance.
        /// </summary>
        /// <param name="scriptHash">The smart contract's script hash</param>
        public SmartContract(Hash160 scriptHash) : this(scriptHash, Core.NeoUnity.Instance)
        {
        }
        
        #endregion
        
        #region Transaction Building
        
        /// <summary>
        /// Initializes a TransactionBuilder for an invocation of this contract with the provided function and parameters.
        /// The order of the parameters is relevant.
        /// </summary>
        /// <param name="function">The function to invoke</param>
        /// <param name="parameters">The parameters to pass with the invocation</param>
        /// <returns>A transaction builder allowing to set further details of the invocation</returns>
        public async Task<TransactionBuilder> InvokeFunction(string function, params ContractParameter[] parameters)
        {
            var script = await BuildInvokeFunctionScript(function, parameters);
            return new TransactionBuilder(NeoUnity).SetScript(script);
        }
        
        /// <summary>
        /// Builds a script to invoke a function on this smart contract.
        /// </summary>
        /// <param name="function">The function to invoke</param>
        /// <param name="parameters">The parameters to pass with the invocation</param>
        /// <returns>The script as byte array</returns>
        public async Task<byte[]> BuildInvokeFunctionScript(string function, params ContractParameter[] parameters)
        {
            if (string.IsNullOrEmpty(function))
            {
                throw new ArgumentException("The invocation function must not be empty.", nameof(function));
            }
            
            var scriptBuilder = new ScriptBuilder();
            return await scriptBuilder.ContractCall(ScriptHash, function, parameters).ToArray();
        }
        
        #endregion
        
        #region Contract Call Methods
        
        /// <summary>
        /// Sends an invokefunction RPC call to the given contract function expecting a string as return type.
        /// </summary>
        /// <param name="function">The function to call</param>
        /// <param name="parameters">The contract parameters to include in the call</param>
        /// <returns>The string returned by the contract</returns>
        public async Task<string> CallFunctionReturningString(string function, params ContractParameter[] parameters)
        {
            var invocationResult = await CallInvokeFunction(function, parameters);
            var result = invocationResult.GetResult();
            ThrowIfFaultState(result);
            
            var stackItem = result.GetFirstStackItem();
            if (!stackItem.IsByteString())
            {
                throw new ContractException($"Unexpected return type. Expected ByteString, got {stackItem.Type}");
            }
            
            return stackItem.GetString();
        }
        
        /// <summary>
        /// Sends an invokefunction RPC call to the given contract function expecting an integer as return type.
        /// </summary>
        /// <param name="function">The function to call</param>
        /// <param name="parameters">The contract parameters to include in the call</param>
        /// <returns>The integer returned by the contract</returns>
        public async Task<int> CallFunctionReturningInt(string function, params ContractParameter[] parameters)
        {
            var invocationResult = await CallInvokeFunction(function, parameters);
            var result = invocationResult.GetResult();
            ThrowIfFaultState(result);
            
            var stackItem = result.GetFirstStackItem();
            if (!stackItem.IsInteger())
            {
                throw new ContractException($"Unexpected return type. Expected Integer, got {stackItem.Type}");
            }
            
            return stackItem.GetInteger();
        }
        
        /// <summary>
        /// Sends an invokefunction RPC call to the given contract function expecting a boolean as return type.
        /// </summary>
        /// <param name="function">The function to call</param>
        /// <param name="parameters">The contract parameters to include in the call</param>
        /// <returns>The boolean returned by the contract</returns>
        public async Task<bool> CallFunctionReturningBool(string function, params ContractParameter[] parameters)
        {
            var invocationResult = await CallInvokeFunction(function, parameters);
            var result = invocationResult.GetResult();
            ThrowIfFaultState(result);
            
            var stackItem = result.GetFirstStackItem();
            
            // Boolean can be represented in multiple stack item types
            if (stackItem.IsBoolean() || stackItem.IsInteger() || stackItem.IsByteString() || stackItem.IsBuffer())
            {
                return stackItem.GetBoolean();
            }
            
            throw new ContractException($"Unexpected return type. Expected Boolean-compatible type, got {stackItem.Type}");
        }
        
        /// <summary>
        /// Sends an invokefunction RPC call to the given contract function expecting a script hash as return type.
        /// </summary>
        /// <param name="function">The function to call</param>
        /// <param name="parameters">The contract parameters to include in the call</param>
        /// <returns>The script hash returned by the contract</returns>
        public async Task<Hash160> CallFunctionReturningScriptHash(string function, params ContractParameter[] parameters)
        {
            var invocationResult = await CallInvokeFunction(function, parameters);
            var result = invocationResult.GetResult();
            ThrowIfFaultState(result);
            
            var stackItem = result.GetFirstStackItem();
            return ExtractScriptHash(stackItem);
        }
        
        /// <summary>
        /// Extracts a script hash from a stack item.
        /// </summary>
        /// <param name="item">The stack item containing the script hash</param>
        /// <returns>The extracted script hash</returns>
        private Hash160 ExtractScriptHash(StackItem item)
        {
            if (!item.IsByteString())
            {
                throw new ContractException($"Unexpected return type. Expected ByteString, got {item.Type}");
            }
            
            try
            {
                var hexString = item.GetHexString();
                // Reverse the hex string for proper hash160 format
                return new Hash160(ReverseHexString(hexString));
            }
            catch (Exception ex)
            {
                throw new ContractException($"Return type did not contain script hash in expected format: {ex.Message}", ex);
            }
        }
        
        /// <summary>
        /// Reverses a hex string (swaps byte order).
        /// </summary>
        /// <param name="hexString">The hex string to reverse</param>
        /// <returns>The reversed hex string</returns>
        private string ReverseHexString(string hexString)
        {
            if (hexString.Length % 2 != 0)
            {
                throw new ArgumentException("Hex string must have even length");
            }
            
            var result = "";
            for (int i = hexString.Length - 2; i >= 0; i -= 2)
            {
                result += hexString.Substring(i, 2);
            }
            return result;
        }
        
        #endregion
        
        #region Iterator Methods
        
        /// <summary>
        /// Sends an invokefunction RPC call to the given contract function expecting an InteropInterface as return type.
        /// Returns an iterator that can be traversed to retrieve the iterator items.
        /// Requires sessions to be enabled on the Neo node.
        /// </summary>
        /// <typeparam name="T">The type to map stack items to</typeparam>
        /// <param name="function">The function to call</param>
        /// <param name="parameters">The contract parameters to include in the call</param>
        /// <param name="mapper">The function to apply on the stack items in the iterator</param>
        /// <returns>The iterator</returns>
        public async Task<Iterator<T>> CallFunctionReturningIterator<T>(string function, ContractParameter[] parameters = null, Func<StackItem, T> mapper = null)
        {
            var invocationResult = await CallInvokeFunction(function, parameters ?? new ContractParameter[0]);
            var result = invocationResult.GetResult();
            ThrowIfFaultState(result);
            
            var stackItem = result.GetFirstStackItem();
            if (!stackItem.IsInteropInterface())
            {
                throw new ContractException($"Unexpected return type. Expected InteropInterface, got {stackItem.Type}");
            }
            
            if (string.IsNullOrEmpty(result.SessionId))
            {
                throw new NeoUnityException("No session id was found. The connected Neo node might not support sessions.");
            }
            
            return new Iterator<T>(NeoUnity, result.SessionId, stackItem.GetIteratorId(), mapper ?? (item => (T)(object)item));
        }
        
        /// <summary>
        /// Sends an invokefunction RPC call expecting an InteropInterface as return type,
        /// then traverses the iterator to retrieve the first DEFAULT_ITERATOR_COUNT stack items.
        /// Requires sessions to be enabled on the Neo node.
        /// </summary>
        /// <typeparam name="T">The type to map stack items to</typeparam>
        /// <param name="function">The function to call</param>
        /// <param name="parameters">The contract parameters to include in the call</param>
        /// <param name="maxIteratorResultItems">The maximal number of items to return</param>
        /// <param name="mapper">The function to apply on the stack items in the iterator</param>
        /// <returns>The mapped iterator items</returns>
        public async Task<List<T>> CallFunctionAndTraverseIterator<T>(string function, ContractParameter[] parameters = null, 
            int maxIteratorResultItems = DEFAULT_ITERATOR_COUNT, Func<StackItem, T> mapper = null)
        {
            var iterator = await CallFunctionReturningIterator<T>(function, parameters, mapper);
            var iteratorItems = await iterator.Traverse(maxIteratorResultItems);
            await iterator.TerminateSession();
            return iteratorItems;
        }
        
        /// <summary>
        /// Calls function of this contract and expects an iterator as the return value.
        /// That iterator is then traversed and its entries are put in an array which is returned.
        /// This all happens on the NeoVM, useful for Neo nodes that don't have iterator sessions enabled.
        /// </summary>
        /// <param name="function">The function to call</param>
        /// <param name="parameters">The contract parameters to include in the call</param>
        /// <param name="maxIteratorResultItems">The maximal number of iterator result items</param>
        /// <param name="signers">The list of signers for this request</param>
        /// <returns>A list of stack items of the returned iterator</returns>
        public async Task<List<StackItem>> CallFunctionAndUnwrapIterator(string function, ContractParameter[] parameters, 
            int maxIteratorResultItems, List<Signer> signers = null)
        {
            var script = await ScriptBuilder.BuildContractCallAndUnwrapIterator(ScriptHash, function, parameters, maxIteratorResultItems);
            var invocationResult = await NeoUnity.InvokeScript(script.ToHexString(), signers ?? new List<Signer>()).SendAsync();
            var result = invocationResult.GetResult();
            ThrowIfFaultState(result);
            
            var firstStackItem = result.GetFirstStackItem();
            return firstStackItem.GetList() ?? new List<StackItem>();
        }
        
        #endregion
        
        #region Direct RPC Methods
        
        /// <summary>
        /// Sends an invokefunction RPC call to the given contract function.
        /// </summary>
        /// <param name="function">The function to call</param>
        /// <param name="parameters">The contract parameters to include in the call</param>
        /// <param name="signers">The list of signers for this request</param>
        /// <returns>The call's response</returns>
        public async Task<NeoInvokeFunctionResponse> CallInvokeFunction(string function, ContractParameter[] parameters = null, List<Signer> signers = null)
        {
            if (string.IsNullOrEmpty(function))
            {
                throw new ArgumentException("The invocation function must not be empty.", nameof(function));
            }
            
            var paramList = parameters != null ? new List<ContractParameter>(parameters) : new List<ContractParameter>();
            return await NeoUnity.InvokeFunction(ScriptHash, function, paramList, signers ?? new List<Signer>()).SendAsync();
        }
        
        #endregion
        
        #region Contract Metadata
        
        /// <summary>
        /// Gets the manifest of this smart contract.
        /// </summary>
        /// <returns>The manifest of this smart contract</returns>
        public async Task<ContractManifest> GetManifest()
        {
            var response = await NeoUnity.GetContractState(ScriptHash).SendAsync();
            return response.GetResult().Manifest;
        }
        
        /// <summary>
        /// Gets the name of this smart contract.
        /// </summary>
        /// <returns>The name of this smart contract</returns>
        public async Task<string> GetName()
        {
            var manifest = await GetManifest();
            return manifest.Name;
        }
        
        #endregion
        
        #region Static Contract Hash Calculation
        
        /// <summary>
        /// Calculates the hash of a native contract based on its name.
        /// </summary>
        /// <param name="contractName">The native contract name</param>
        /// <returns>The contract hash</returns>
        public static Hash160 CalcNativeContractHash(string contractName)
        {
            return CalcContractHash(Hash160.ZERO, 0, contractName);
        }
        
        /// <summary>
        /// Calculates the hash of the contract deployed by sender.
        /// A contract's hash doesn't change after deployment. Even if the contract's script is updated the hash stays the same.
        /// It depends on the initial NEF checksum, contract name, and the sender of the deployment transaction.
        /// </summary>
        /// <param name="sender">The sender of the contract deployment transaction</param>
        /// <param name="nefChecksum">The checksum of the contract's NEF file</param>
        /// <param name="contractName">The contract's name</param>
        /// <returns>The hash of the contract</returns>
        public static Hash160 CalcContractHash(Hash160 sender, int nefChecksum, string contractName)
        {
            var script = ScriptBuilder.BuildContractHashScript(sender, nefChecksum, contractName);
            return Hash160.FromScript(script);
        }
        
        #endregion
        
        #region Utility Methods
        
        /// <summary>
        /// Throws an exception if the invocation result has a fault state.
        /// </summary>
        /// <param name="invocationResult">The invocation result to check</param>
        protected void ThrowIfFaultState(InvocationResult invocationResult)
        {
            if (invocationResult.HasStateFault())
            {
                throw new ContractException($"Contract invocation resulted in fault state: {invocationResult.Exception}");
            }
        }
        
        #endregion
    }
    
    /// <summary>
    /// Exception thrown when smart contract operations fail.
    /// </summary>
    public class ContractException : NeoUnityException
    {
        /// <summary>
        /// Creates a new contract exception.
        /// </summary>
        /// <param name="message">The exception message</param>
        public ContractException(string message) : base(message)
        {
        }
        
        /// <summary>
        /// Creates a new contract exception with an inner exception.
        /// </summary>
        /// <param name="message">The exception message</param>
        /// <param name="innerException">The inner exception</param>
        public ContractException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
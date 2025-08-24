using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Neo.Unity.SDK.Core;
using Neo.Unity.SDK.Types;
using Neo.Unity.SDK.Transaction;

namespace Neo.Unity.SDK.Contracts.Native
{
    /// <summary>
    /// Represents the ContractManagement native contract and provides methods to invoke its functions.
    /// The Contract Management contract handles contract deployment, updates, and metadata management.
    /// </summary>
    [System.Serializable]
    public class ContractManagement : SmartContract
    {
        #region Constants

        private const string NAME = "ContractManagement";
        public static readonly Hash160 SCRIPT_HASH = SmartContract.CalcNativeContractHash(NAME);

        private const string GET_MINIMUM_DEPLOYMENT_FEE = "getMinimumDeploymentFee";
        private const string SET_MINIMUM_DEPLOYMENT_FEE = "setMinimumDeploymentFee";
        private const string GET_CONTRACT_BY_ID = "getContractById";
        private const string GET_CONTRACT_HASHES = "getContractHashes";
        private const string HAS_METHOD = "hasMethod";
        private const string DEPLOY = "deploy";
        private const string UPDATE = "update";
        private const string DESTROY = "destroy";

        public const int MAX_MANIFEST_SIZE = 65536; // 64KB
        public const int DEFAULT_ITERATOR_COUNT = 100;

        #endregion

        #region Constructor

        /// <summary>
        /// Constructs a new ContractManagement instance that uses the given NeoUnity instance for invocations.
        /// </summary>
        /// <param name="neoUnity">The NeoUnity instance to use for invocations</param>
        public ContractManagement(NeoUnity neoUnity) : base(SCRIPT_HASH, neoUnity)
        {
        }

        /// <summary>
        /// Constructs a new ContractManagement instance using the singleton NeoUnity instance.
        /// </summary>
        public ContractManagement() : base(SCRIPT_HASH)
        {
        }

        #endregion

        #region Deployment Fee Management

        /// <summary>
        /// Gets the minimum fee required for contract deployment.
        /// </summary>
        /// <returns>The minimum required fee for contract deployment in GAS fractions</returns>
        public async Task<long> GetMinimumDeploymentFee()
        {
            var result = await CallFunctionReturningInt(GET_MINIMUM_DEPLOYMENT_FEE);
            
            if (NeoUnity.Config.EnableDebugLogging)
            {
                Debug.Log($"[ContractManagement] Minimum deployment fee: {result} GAS fractions");
            }
            
            return result;
        }

        /// <summary>
        /// Creates a transaction script to set the minimum deployment fee and initializes a TransactionBuilder based on this script.
        /// This method can only be successfully invoked by the committee, i.e., the transaction has to be signed by the committee members.
        /// </summary>
        /// <param name="minimumFee">The minimum deployment fee in GAS fractions</param>
        /// <returns>A transaction builder ready for committee signing</returns>
        public async Task<TransactionBuilder> SetMinimumDeploymentFee(long minimumFee)
        {
            if (minimumFee < 0)
            {
                throw new ArgumentException("Minimum deployment fee cannot be negative.", nameof(minimumFee));
            }

            if (NeoUnity.Config.EnableDebugLogging)
            {
                Debug.Log($"[ContractManagement] Setting minimum deployment fee to: {minimumFee} GAS fractions");
            }

            return await InvokeFunction(SET_MINIMUM_DEPLOYMENT_FEE, ContractParameter.Integer(minimumFee));
        }

        #endregion

        #region Contract Information

        /// <summary>
        /// Gets the contract state of the contract with the given contract hash.
        /// Makes use of the RPC getContractState method.
        /// </summary>
        /// <param name="contractHash">The contract hash</param>
        /// <returns>The contract state</returns>
        public async Task<ContractState> GetContract(Hash160 contractHash)
        {
            if (contractHash == null)
            {
                throw new ArgumentNullException(nameof(contractHash));
            }

            try
            {
                var response = await NeoUnity.GetContractState(contractHash).SendAsync();
                var result = response.GetResult();
                
                if (NeoUnity.Config.EnableDebugLogging)
                {
                    Debug.Log($"[ContractManagement] Retrieved contract state for: {contractHash}");
                }
                
                return result;
            }
            catch (Exception ex)
            {
                throw new ContractException($"Failed to get contract state for {contractHash}: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Gets the contract state of the contract with the given contract ID.
        /// </summary>
        /// <param name="contractId">The contract ID</param>
        /// <returns>The contract state</returns>
        public async Task<ContractState> GetContractById(int contractId)
        {
            if (contractId < 0)
            {
                throw new ArgumentException("Contract ID cannot be negative.", nameof(contractId));
            }

            var contractHash = await GetContractHashById(contractId);
            return await GetContract(contractHash);
        }

        /// <summary>
        /// Gets the contract hash for a given contract ID.
        /// </summary>
        /// <param name="contractId">The contract ID</param>
        /// <returns>The contract hash</returns>
        private async Task<Hash160> GetContractHashById(int contractId)
        {
            try
            {
                var invocationResult = await CallInvokeFunction(GET_CONTRACT_BY_ID, ContractParameter.Integer(contractId));
                var result = invocationResult.GetResult();
                ThrowIfFaultState(result);

                var stackItem = result.GetFirstStackItem();
                if (!stackItem.IsArray())
                {
                    throw new ContractException("Contract ID query did not return an array");
                }

                var list = stackItem.GetList();
                if (list.Count < 3)
                {
                    throw new ContractException("Contract ID query returned insufficient data");
                }

                var hashBytes = list[2].GetByteArray();
                return new Hash160(hashBytes);
            }
            catch (Exception ex)
            {
                throw new ContractException($"Could not get the contract hash for contract ID {contractId}: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Gets all non-native contract hashes and IDs using an iterator.
        /// </summary>
        /// <returns>An iterator of contract identifiers</returns>
        public async Task<Iterator<ContractIdentifiers>> GetContractHashes()
        {
            return await CallFunctionReturningIterator<ContractIdentifiers>(GET_CONTRACT_HASHES, null, MapContractIdentifiers);
        }

        /// <summary>
        /// Gets all non-native contract hashes and IDs without using sessions.
        /// Use this method if sessions are disabled on the Neo node.
        /// This method returns at most DEFAULT_ITERATOR_COUNT values.
        /// </summary>
        /// <returns>List of contract identifiers</returns>
        public async Task<List<ContractIdentifiers>> GetContractHashesUnwrapped()
        {
            var stackItems = await CallFunctionAndUnwrapIterator(GET_CONTRACT_HASHES, new ContractParameter[0], DEFAULT_ITERATOR_COUNT);
            var contractIds = new List<ContractIdentifiers>();

            foreach (var item in stackItems)
            {
                contractIds.Add(MapContractIdentifiers(item));
            }

            if (NeoUnity.Config.EnableDebugLogging)
            {
                Debug.Log($"[ContractManagement] Retrieved {contractIds.Count} contract identifiers");
            }

            return contractIds;
        }

        /// <summary>
        /// Maps a stack item to a ContractIdentifiers object.
        /// </summary>
        /// <param name="stackItem">The stack item containing contract identifier data</param>
        /// <returns>The mapped contract identifiers</returns>
        private ContractIdentifiers MapContractIdentifiers(StackItem stackItem)
        {
            if (!stackItem.IsArray())
            {
                throw new ContractException("Contract identifiers stack item must be an array");
            }

            var list = stackItem.GetList();
            if (list.Count < 2)
            {
                throw new ContractException("Contract identifiers array must have at least 2 elements");
            }

            var id = list[0].GetInteger();
            var hashBytes = list[1].GetByteArray();
            var hash = new Hash160(hashBytes);

            return new ContractIdentifiers((int)id, hash);
        }

        /// <summary>
        /// Checks if a method exists in a contract.
        /// </summary>
        /// <param name="contractHash">The contract hash</param>
        /// <param name="method">The method name</param>
        /// <param name="paramCount">The number of parameters</param>
        /// <returns>True if the method exists, otherwise false</returns>
        public async Task<bool> HasMethod(Hash160 contractHash, string method, int paramCount)
        {
            if (contractHash == null)
            {
                throw new ArgumentNullException(nameof(contractHash));
            }

            if (string.IsNullOrEmpty(method))
            {
                throw new ArgumentException("Method name cannot be null or empty.", nameof(method));
            }

            if (paramCount < 0)
            {
                throw new ArgumentException("Parameter count cannot be negative.", nameof(paramCount));
            }

            var result = await CallFunctionReturningBool(HAS_METHOD,
                ContractParameter.Hash160(contractHash),
                ContractParameter.String(method),
                ContractParameter.Integer(paramCount));

            if (NeoUnity.Config.EnableDebugLogging)
            {
                Debug.Log($"[ContractManagement] Contract {contractHash} has method '{method}' with {paramCount} params: {result}");
            }

            return result;
        }

        #endregion

        #region Contract Deployment

        /// <summary>
        /// Creates a transaction script for deploying a contract and initializes a TransactionBuilder based on this script.
        /// </summary>
        /// <param name="nefBytes">The NEF file as byte array</param>
        /// <param name="manifestJson">The contract manifest as JSON string</param>
        /// <param name="data">Optional data to pass to the deployed contract's _deploy method</param>
        /// <returns>A transaction builder ready for deployment</returns>
        public async Task<TransactionBuilder> Deploy(byte[] nefBytes, string manifestJson, ContractParameter data = null)
        {
            if (nefBytes == null || nefBytes.Length == 0)
            {
                throw new ArgumentException("NEF bytes cannot be null or empty.", nameof(nefBytes));
            }

            if (string.IsNullOrEmpty(manifestJson))
            {
                throw new ArgumentException("Manifest JSON cannot be null or empty.", nameof(manifestJson));
            }

            var manifestBytes = System.Text.Encoding.UTF8.GetBytes(manifestJson);
            if (manifestBytes.Length > MAX_MANIFEST_SIZE)
            {
                throw new ArgumentException($"The given contract manifest is too long. Manifest was {manifestBytes.Length} bytes, but max {MAX_MANIFEST_SIZE} bytes is allowed.");
            }

            var parameters = new List<ContractParameter>
            {
                ContractParameter.ByteArray(nefBytes),
                ContractParameter.ByteArray(manifestBytes)
            };

            if (data != null)
            {
                parameters.Add(data);
            }

            if (NeoUnity.Config.EnableDebugLogging)
            {
                Debug.Log($"[ContractManagement] Creating deployment transaction. NEF size: {nefBytes.Length} bytes, Manifest size: {manifestBytes.Length} bytes");
            }

            return await InvokeFunction(DEPLOY, parameters.ToArray());
        }

        /// <summary>
        /// Creates a transaction script for deploying a contract using a NefFile object.
        /// </summary>
        /// <param name="nefFile">The NEF file</param>
        /// <param name="manifestJson">The contract manifest as JSON string</param>
        /// <param name="data">Optional data to pass to the deployed contract's _deploy method</param>
        /// <returns>A transaction builder ready for deployment</returns>
        public async Task<TransactionBuilder> Deploy(NefFile nefFile, string manifestJson, ContractParameter data = null)
        {
            if (nefFile == null)
            {
                throw new ArgumentNullException(nameof(nefFile));
            }

            return await Deploy(nefFile.ToByteArray(), manifestJson, data);
        }

        #endregion

        #region Contract Update

        /// <summary>
        /// Creates a transaction script for updating a contract and initializes a TransactionBuilder based on this script.
        /// This method can only be called from within the contract being updated.
        /// </summary>
        /// <param name="nefBytes">The new NEF file as byte array (null to keep current)</param>
        /// <param name="manifestJson">The new contract manifest as JSON string (null to keep current)</param>
        /// <param name="data">Optional data to pass to the contract's _deploy method during update</param>
        /// <returns>A transaction builder ready for contract update</returns>
        public async Task<TransactionBuilder> Update(byte[] nefBytes = null, string manifestJson = null, ContractParameter data = null)
        {
            if (nefBytes == null && string.IsNullOrEmpty(manifestJson))
            {
                throw new ArgumentException("At least one of NEF bytes or manifest JSON must be provided for update.");
            }

            var parameters = new List<ContractParameter>();

            // Add NEF bytes (null if not updating)
            parameters.Add(nefBytes != null ? ContractParameter.ByteArray(nefBytes) : ContractParameter.Any(null));

            // Add manifest bytes (null if not updating)
            if (!string.IsNullOrEmpty(manifestJson))
            {
                var manifestBytes = System.Text.Encoding.UTF8.GetBytes(manifestJson);
                if (manifestBytes.Length > MAX_MANIFEST_SIZE)
                {
                    throw new ArgumentException($"The given contract manifest is too long. Manifest was {manifestBytes.Length} bytes, but max {MAX_MANIFEST_SIZE} bytes is allowed.");
                }
                parameters.Add(ContractParameter.ByteArray(manifestBytes));
            }
            else
            {
                parameters.Add(ContractParameter.Any(null));
            }

            // Add optional data
            if (data != null)
            {
                parameters.Add(data);
            }

            if (NeoUnity.Config.EnableDebugLogging)
            {
                var nefInfo = nefBytes != null ? $"{nefBytes.Length} bytes" : "unchanged";
                var manifestInfo = !string.IsNullOrEmpty(manifestJson) ? $"{manifestJson.Length} chars" : "unchanged";
                Debug.Log($"[ContractManagement] Creating update transaction. NEF: {nefInfo}, Manifest: {manifestInfo}");
            }

            return await InvokeFunction(UPDATE, parameters.ToArray());
        }

        #endregion

        #region Contract Destruction

        /// <summary>
        /// Creates a transaction script for destroying a contract and initializes a TransactionBuilder based on this script.
        /// This method can only be called from within the contract being destroyed.
        /// </summary>
        /// <returns>A transaction builder ready for contract destruction</returns>
        public async Task<TransactionBuilder> Destroy()
        {
            if (NeoUnity.Config.EnableDebugLogging)
            {
                Debug.Log("[ContractManagement] Creating contract destruction transaction");
            }

            return await InvokeFunction(DESTROY);
        }

        #endregion

        #region Unity Integration Helpers

        /// <summary>
        /// Calculates the total cost for deploying a contract including minimum deployment fee.
        /// </summary>
        /// <param name="nefSize">The size of the NEF file in bytes</param>
        /// <param name="manifestSize">The size of the manifest in bytes</param>
        /// <returns>The estimated deployment cost in GAS fractions</returns>
        public async Task<long> EstimateDeploymentCost(int nefSize, int manifestSize)
        {
            var minimumFee = await GetMinimumDeploymentFee();
            
            // Add policy contract storage costs and execution costs
            var policyContract = new PolicyContract(NeoUnity);
            var storagePrice = await policyContract.GetStoragePrice();
            
            // Rough estimate: storage for NEF + manifest + deployment overhead
            var storageSize = nefSize + manifestSize + 1000; // Add overhead for metadata
            var storageCost = storagePrice * storageSize;
            
            var totalCost = minimumFee + storageCost;

            if (NeoUnity.Config.EnableDebugLogging)
            {
                Debug.Log($"[ContractManagement] Estimated deployment cost: {totalCost} GAS fractions " +
                         $"(MinFee: {minimumFee}, Storage: {storageCost})");
            }

            return totalCost;
        }

        /// <summary>
        /// Validates contract deployment prerequisites.
        /// </summary>
        /// <param name="senderAccount">The account deploying the contract</param>
        /// <param name="nefSize">The size of the NEF file</param>
        /// <param name="manifestSize">The size of the manifest</param>
        /// <returns>Deployment validation result</returns>
        public async Task<DeploymentValidation> ValidateDeployment(Wallet.Account senderAccount, int nefSize, int manifestSize)
        {
            if (senderAccount == null)
            {
                throw new ArgumentNullException(nameof(senderAccount));
            }

            var validation = new DeploymentValidation();
            
            try
            {
                // Check deployment cost
                var estimatedCost = await EstimateDeploymentCost(nefSize, manifestSize);
                validation.EstimatedCost = estimatedCost;

                // Check GAS balance
                var gasToken = new GasToken(NeoUnity);
                var gasBalance = await gasToken.GetBalanceOf(senderAccount);
                validation.HasSufficientGas = gasBalance >= estimatedCost;
                validation.GasBalance = gasBalance;

                // Check account is not blocked
                var policyContract = new PolicyContract(NeoUnity);
                var isBlocked = await policyContract.IsBlocked(senderAccount);
                validation.IsAccountBlocked = isBlocked;

                // Check manifest size
                validation.IsManifestSizeValid = manifestSize <= MAX_MANIFEST_SIZE;

                validation.CanDeploy = validation.HasSufficientGas && 
                                     !validation.IsAccountBlocked && 
                                     validation.IsManifestSizeValid;
            }
            catch (Exception ex)
            {
                validation.ValidationError = ex.Message;
                validation.CanDeploy = false;
            }

            return validation;
        }

        #endregion
    }

    /// <summary>
    /// Represents contract identifiers (ID and hash).
    /// </summary>
    [System.Serializable]
    public class ContractIdentifiers
    {
        /// <summary>The contract ID</summary>
        public int Id { get; private set; }

        /// <summary>The contract hash</summary>
        public Hash160 Hash { get; private set; }

        /// <summary>
        /// Creates new contract identifiers.
        /// </summary>
        /// <param name="id">The contract ID</param>
        /// <param name="hash">The contract hash</param>
        public ContractIdentifiers(int id, Hash160 hash)
        {
            Id = id;
            Hash = hash ?? throw new ArgumentNullException(nameof(hash));
        }

        public override string ToString()
        {
            return $"ContractIdentifiers(Id: {Id}, Hash: {Hash})";
        }
    }

    /// <summary>
    /// Represents a NEF (Neo Executable Format) file.
    /// </summary>
    [System.Serializable]
    public class NefFile
    {
        /// <summary>The NEF magic number</summary>
        public const uint NEF_MAGIC = 0x3346454E; // "NEF3"

        /// <summary>The NEF compiler name</summary>
        public string Compiler { get; set; }

        /// <summary>The NEF tokens</summary>
        public byte[] Tokens { get; set; }

        /// <summary>The NEF script bytecode</summary>
        public byte[] Script { get; set; }

        /// <summary>The NEF checksum</summary>
        public uint Checksum { get; set; }

        /// <summary>
        /// Creates a new NEF file.
        /// </summary>
        /// <param name="compiler">The compiler name</param>
        /// <param name="tokens">The tokens</param>
        /// <param name="script">The script bytecode</param>
        /// <param name="checksum">The checksum</param>
        public NefFile(string compiler, byte[] tokens, byte[] script, uint checksum)
        {
            Compiler = compiler ?? throw new ArgumentNullException(nameof(compiler));
            Tokens = tokens ?? throw new ArgumentNullException(nameof(tokens));
            Script = script ?? throw new ArgumentNullException(nameof(script));
            Checksum = checksum;
        }

        /// <summary>
        /// Converts the NEF file to a byte array.
        /// </summary>
        /// <returns>The NEF file as byte array</returns>
        public byte[] ToByteArray()
        {
            // NEF (Neo Executable Format) serialization following Neo protocol specification
            var result = new List<byte>();
            
            // Add magic
            result.AddRange(BitConverter.GetBytes(NEF_MAGIC));
            
            // Add compiler (with length prefix)
            var compilerBytes = System.Text.Encoding.UTF8.GetBytes(Compiler);
            result.Add((byte)compilerBytes.Length);
            result.AddRange(compilerBytes);
            
            // Add tokens (with length prefix)
            result.AddRange(BitConverter.GetBytes((ushort)Tokens.Length));
            result.AddRange(Tokens);
            
            // Add script (with length prefix)
            result.AddRange(BitConverter.GetBytes(Script.Length));
            result.AddRange(Script);
            
            // Add checksum
            result.AddRange(BitConverter.GetBytes(Checksum));
            
            return result.ToArray();
        }
    }

    /// <summary>
    /// Represents contract deployment validation results.
    /// </summary>
    [System.Serializable]
    public class DeploymentValidation
    {
        /// <summary>Whether the contract can be deployed</summary>
        public bool CanDeploy { get; set; }

        /// <summary>Whether the account has sufficient GAS</summary>
        public bool HasSufficientGas { get; set; }

        /// <summary>Whether the account is blocked</summary>
        public bool IsAccountBlocked { get; set; }

        /// <summary>Whether the manifest size is valid</summary>
        public bool IsManifestSizeValid { get; set; }

        /// <summary>The estimated deployment cost in GAS fractions</summary>
        public long EstimatedCost { get; set; }

        /// <summary>The account's GAS balance in fractions</summary>
        public long GasBalance { get; set; }

        /// <summary>Any validation error message</summary>
        public string ValidationError { get; set; }

        /// <summary>
        /// Gets the estimated cost in decimal GAS format.
        /// </summary>
        /// <returns>Estimated cost in decimal GAS</returns>
        public decimal GetEstimatedCostGas()
        {
            return GasToken.FromGasFractions(EstimatedCost);
        }

        /// <summary>
        /// Gets the GAS balance in decimal format.
        /// </summary>
        /// <returns>GAS balance in decimal GAS</returns>
        public decimal GetGasBalanceDecimal()
        {
            return GasToken.FromGasFractions(GasBalance);
        }

        /// <summary>
        /// Gets a user-friendly reason why deployment cannot proceed, if applicable.
        /// </summary>
        /// <returns>Blocking reason or null if deployment can proceed</returns>
        public string GetBlockingReason()
        {
            if (CanDeploy) return null;

            if (!string.IsNullOrEmpty(ValidationError)) return ValidationError;
            if (IsAccountBlocked) return "Account is blocked by network policy";
            if (!HasSufficientGas) return $"Insufficient GAS. Required: {GetEstimatedCostGas():F8}, Available: {GetGasBalanceDecimal():F8}";
            if (!IsManifestSizeValid) return $"Manifest too large. Must be <= {MAX_MANIFEST_SIZE} bytes";

            return "Unknown deployment issue";
        }

        public override string ToString()
        {
            var status = CanDeploy ? "Can Deploy" : $"Cannot Deploy ({GetBlockingReason()})";
            return $"DeploymentValidation({status}, Cost: {GetEstimatedCostGas():F8} GAS)";
        }
    }

    /// <summary>
    /// Placeholder for ContractState (would be implemented based on actual response structure).
    /// </summary>
    [System.Serializable]
    public class ContractState
    {
        public int Id { get; set; }
        public Hash160 Hash { get; set; }
        public byte[] Nef { get; set; }
        public string Manifest { get; set; }
        public int UpdateCounter { get; set; }
    }
}
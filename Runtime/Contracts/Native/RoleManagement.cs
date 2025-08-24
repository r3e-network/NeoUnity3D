using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Neo.Unity.SDK.Core;
using Neo.Unity.SDK.Types;
using Neo.Unity.SDK.Transaction;
using Neo.Unity.SDK.Crypto;

namespace Neo.Unity.SDK.Contracts.Native
{
    /// <summary>
    /// Represents the RoleManagement native contract that is used to assign roles to and check roles of designated nodes.
    /// This contract manages node role assignments for various network services like oracles, state validators, and NeoFS alphabet nodes.
    /// </summary>
    [System.Serializable]
    public class RoleManagement : SmartContract
    {
        #region Constants

        private const string NAME = "RoleManagement";
        public static readonly Hash160 SCRIPT_HASH = SmartContract.CalcNativeContractHash(NAME);

        public const string GET_DESIGNATED_BY_ROLE = "getDesignatedByRole";
        public const string DESIGNATE_AS_ROLE = "designateAsRole";

        #endregion

        #region Constructor

        /// <summary>
        /// Constructs a new RoleManagement instance that uses the given NeoUnity instance for invocations.
        /// </summary>
        /// <param name="neoUnity">The NeoUnity instance to use for invocations</param>
        public RoleManagement(NeoUnity neoUnity) : base(SCRIPT_HASH, neoUnity)
        {
        }

        /// <summary>
        /// Constructs a new RoleManagement instance using the singleton NeoUnity instance.
        /// </summary>
        public RoleManagement() : base(SCRIPT_HASH)
        {
        }

        #endregion

        #region Role Management

        /// <summary>
        /// Gets the nodes that were assigned to the given role at the given block index.
        /// </summary>
        /// <param name="role">The role to query</param>
        /// <param name="blockIndex">The block index to query at</param>
        /// <returns>The public keys of the designated nodes for this role</returns>
        public async Task<List<ECPublicKey>> GetDesignatedByRole(Role role, int blockIndex)
        {
            await CheckBlockIndexValidity(blockIndex);

            var invocationResult = await CallInvokeFunction(GET_DESIGNATED_BY_ROLE, 
                ContractParameter.Integer((int)role), 
                ContractParameter.Integer(blockIndex));

            var result = invocationResult.GetResult();
            ThrowIfFaultState(result);

            if (result.Stack == null || result.Stack.Count == 0)
            {
                throw new ContractException("The invocation result did not have a list of roles");
            }

            var stackItem = result.Stack[0];
            if (!stackItem.IsArray())
            {
                // Return empty list if no nodes are designated for this role
                if (NeoUnity.Config.EnableDebugLogging)
                {
                    Debug.Log($"[RoleManagement] No nodes designated for role {role} at block {blockIndex}");
                }
                return new List<ECPublicKey>();
            }

            var designatedNodes = new List<ECPublicKey>();
            var arrayItems = stackItem.GetList();

            foreach (var item in arrayItems)
            {
                try
                {
                    var publicKeyBytes = item.GetByteArray();
                    var publicKey = new ECPublicKey(publicKeyBytes);
                    designatedNodes.Add(publicKey);
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"[RoleManagement] Failed to parse public key from role designation: {ex.Message}");
                }
            }

            if (NeoUnity.Config.EnableDebugLogging)
            {
                Debug.Log($"[RoleManagement] Found {designatedNodes.Count} nodes designated for role {role} at block {blockIndex}");
            }

            return designatedNodes;
        }

        /// <summary>
        /// Validates that the block index is valid for the current blockchain state.
        /// </summary>
        /// <param name="blockIndex">The block index to validate</param>
        /// <exception cref="ArgumentException">Thrown if the block index is invalid</exception>
        private async Task CheckBlockIndexValidity(int blockIndex)
        {
            if (blockIndex < 0)
            {
                throw new ArgumentException("The block index must be non-negative.", nameof(blockIndex));
            }

            try
            {
                var currentBlockCount = await NeoUnity.GetBlockCount().SendAsync();
                var blockCount = currentBlockCount.GetResult();

                if (blockIndex >= blockCount)
                {
                    throw new ArgumentException($"The provided block index ({blockIndex}) is too high. The current block count is {blockCount}.");
                }
            }
            catch (Exception ex) when (!(ex is ArgumentException))
            {
                throw new ContractException($"Failed to validate block index: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Creates a transaction script to designate nodes as a specific role and initializes a TransactionBuilder based on this script.
        /// This method can only be successfully invoked by the committee, i.e., the transaction has to be signed by the committee members.
        /// </summary>
        /// <param name="role">The designation role</param>
        /// <param name="publicKeys">The public keys of the nodes that are being designated</param>
        /// <returns>A transaction builder ready for committee signing</returns>
        public async Task<TransactionBuilder> DesignateAsRole(Role role, List<ECPublicKey> publicKeys)
        {
            if (publicKeys == null || publicKeys.Count == 0)
            {
                throw new ArgumentException("At least one public key is required for designation.", nameof(publicKeys));
            }

            var publicKeyParameters = new List<ContractParameter>();
            foreach (var publicKey in publicKeys)
            {
                if (publicKey == null)
                {
                    throw new ArgumentException("Public key cannot be null.", nameof(publicKeys));
                }

                var encodedKey = publicKey.GetEncoded(true);
                publicKeyParameters.Add(ContractParameter.PublicKey(encodedKey));
            }

            if (NeoUnity.Config.EnableDebugLogging)
            {
                Debug.Log($"[RoleManagement] Creating designation transaction for role {role} with {publicKeys.Count} nodes");
            }

            return await InvokeFunction(DESIGNATE_AS_ROLE,
                ContractParameter.Integer((int)role),
                ContractParameter.Array(publicKeyParameters.ToArray()));
        }

        /// <summary>
        /// Convenience method to designate nodes as a specific role using an array of public keys.
        /// </summary>
        /// <param name="role">The designation role</param>
        /// <param name="publicKeys">The public keys of the nodes that are being designated</param>
        /// <returns>A transaction builder ready for committee signing</returns>
        public async Task<TransactionBuilder> DesignateAsRole(Role role, params ECPublicKey[] publicKeys)
        {
            return await DesignateAsRole(role, new List<ECPublicKey>(publicKeys));
        }

        #endregion

        #region Role Queries

        /// <summary>
        /// Gets the current nodes designated for a role (using the latest block).
        /// </summary>
        /// <param name="role">The role to query</param>
        /// <returns>The public keys of the currently designated nodes</returns>
        public async Task<List<ECPublicKey>> GetCurrentDesignatedByRole(Role role)
        {
            try
            {
                var currentBlockCount = await NeoUnity.GetBlockCount().SendAsync();
                var latestBlockIndex = Math.Max(0, currentBlockCount.GetResult() - 1);
                
                return await GetDesignatedByRole(role, latestBlockIndex);
            }
            catch (Exception ex)
            {
                throw new ContractException($"Failed to get current designated nodes for role {role}: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Checks if a specific public key is designated for a role at a given block index.
        /// </summary>
        /// <param name="role">The role to check</param>
        /// <param name="publicKey">The public key to check</param>
        /// <param name="blockIndex">The block index to check at</param>
        /// <returns>True if the public key is designated for the role at the specified block</returns>
        public async Task<bool> IsDesignatedForRole(Role role, ECPublicKey publicKey, int blockIndex)
        {
            if (publicKey == null)
            {
                throw new ArgumentNullException(nameof(publicKey));
            }

            var designatedNodes = await GetDesignatedByRole(role, blockIndex);
            return designatedNodes.Exists(node => node.Equals(publicKey));
        }

        /// <summary>
        /// Checks if a specific public key is currently designated for a role.
        /// </summary>
        /// <param name="role">The role to check</param>
        /// <param name="publicKey">The public key to check</param>
        /// <returns>True if the public key is currently designated for the role</returns>
        public async Task<bool> IsCurrentlyDesignatedForRole(Role role, ECPublicKey publicKey)
        {
            if (publicKey == null)
            {
                throw new ArgumentNullException(nameof(publicKey));
            }

            var designatedNodes = await GetCurrentDesignatedByRole(role);
            return designatedNodes.Exists(node => node.Equals(publicKey));
        }

        /// <summary>
        /// Gets all current role designations for display or management purposes.
        /// </summary>
        /// <returns>A dictionary mapping roles to their designated nodes</returns>
        public async Task<Dictionary<Role, List<ECPublicKey>>> GetAllCurrentDesignations()
        {
            var designations = new Dictionary<Role, List<ECPublicKey>>();
            var roles = Enum.GetValues(typeof(Role));

            var tasks = new List<Task<KeyValuePair<Role, List<ECPublicKey>>>>();
            
            foreach (Role role in roles)
            {
                tasks.Add(GetRoleDesignationAsync(role));
            }

            var results = await Task.WhenAll(tasks);
            
            foreach (var result in results)
            {
                designations[result.Key] = result.Value;
            }

            if (NeoUnity.Config.EnableDebugLogging)
            {
                Debug.Log($"[RoleManagement] Retrieved designations for {designations.Count} roles");
            }

            return designations;
        }

        /// <summary>
        /// Helper method for parallel role designation retrieval.
        /// </summary>
        /// <param name="role">The role to get designations for</param>
        /// <returns>A key-value pair of role and designated nodes</returns>
        private async Task<KeyValuePair<Role, List<ECPublicKey>>> GetRoleDesignationAsync(Role role)
        {
            try
            {
                var nodes = await GetCurrentDesignatedByRole(role);
                return new KeyValuePair<Role, List<ECPublicKey>>(role, nodes);
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[RoleManagement] Failed to get designation for role {role}: {ex.Message}");
                return new KeyValuePair<Role, List<ECPublicKey>>(role, new List<ECPublicKey>());
            }
        }

        #endregion

        #region Unity Integration Helpers

        /// <summary>
        /// Gets a summary of role designations for UI display.
        /// </summary>
        /// <returns>A summary of current role designations</returns>
        public async Task<RoleDesignationSummary> GetDesignationSummary()
        {
            var allDesignations = await GetAllCurrentDesignations();
            
            return new RoleDesignationSummary
            {
                StateValidators = allDesignations.GetValueOrDefault(Role.StateValidator, new List<ECPublicKey>()),
                Oracles = allDesignations.GetValueOrDefault(Role.Oracle, new List<ECPublicKey>()),
                NeoFSAlphabetNodes = allDesignations.GetValueOrDefault(Role.NeoFSAlphabetNode, new List<ECPublicKey>()),
                TotalDesignatedNodes = allDesignations.Values.SelectMany(nodes => nodes).Distinct().Count()
            };
        }

        /// <summary>
        /// Validates a role designation operation before submitting to the network.
        /// </summary>
        /// <param name="role">The role to designate</param>
        /// <param name="publicKeys">The public keys to designate</param>
        /// <returns>Validation result with details about the operation</returns>
        public async Task<RoleDesignationValidation> ValidateRoleDesignation(Role role, List<ECPublicKey> publicKeys)
        {
            var validation = new RoleDesignationValidation
            {
                Role = role,
                ProposedNodes = publicKeys?.Count ?? 0
            };

            try
            {
                // Basic validation
                if (publicKeys == null || publicKeys.Count == 0)
                {
                    validation.IsValid = false;
                    validation.ValidationError = "At least one public key is required for designation";
                    return validation;
                }

                // Check for duplicate keys
                var uniqueKeys = publicKeys.Distinct().ToList();
                if (uniqueKeys.Count != publicKeys.Count)
                {
                    validation.IsValid = false;
                    validation.ValidationError = "Duplicate public keys are not allowed in role designation";
                    return validation;
                }

                // Get current designations for comparison
                var currentNodes = await GetCurrentDesignatedByRole(role);
                validation.CurrentNodes = currentNodes.Count;
                validation.NodesAdded = uniqueKeys.Where(key => !currentNodes.Any(current => current.Equals(key))).Count();
                validation.NodesRemoved = currentNodes.Where(current => !uniqueKeys.Any(key => key.Equals(current))).Count();

                validation.IsValid = true;
            }
            catch (Exception ex)
            {
                validation.IsValid = false;
                validation.ValidationError = $"Validation failed: {ex.Message}";
            }

            return validation;
        }

        #endregion
    }

    /// <summary>
    /// Represents the different roles that can be assigned to network nodes.
    /// </summary>
    public enum Role
    {
        /// <summary>State validator nodes that participate in consensus</summary>
        StateValidator = 0x04,

        /// <summary>Oracle nodes that provide external data to smart contracts</summary>
        Oracle = 0x08,

        /// <summary>NeoFS alphabet nodes for distributed file system</summary>
        NeoFSAlphabetNode = 0x10
    }

    /// <summary>
    /// Extension methods for the Role enum.
    /// </summary>
    public static class RoleExtensions
    {
        /// <summary>
        /// Gets the string representation of the role.
        /// </summary>
        /// <param name="role">The role</param>
        /// <returns>The role name as string</returns>
        public static string ToJsonValue(this Role role)
        {
            return role switch
            {
                Role.StateValidator => "StateValidator",
                Role.Oracle => "Oracle",
                Role.NeoFSAlphabetNode => "NeoFSAlphabetNode",
                _ => role.ToString()
            };
        }

        /// <summary>
        /// Gets a user-friendly description of the role.
        /// </summary>
        /// <param name="role">The role</param>
        /// <returns>A description of what this role does</returns>
        public static string GetDescription(this Role role)
        {
            return role switch
            {
                Role.StateValidator => "Validates state transitions and participates in consensus",
                Role.Oracle => "Provides external data feeds to smart contracts",
                Role.NeoFSAlphabetNode => "Manages distributed file system operations",
                _ => "Unknown role"
            };
        }
    }

    /// <summary>
    /// Summary of current role designations across all roles.
    /// </summary>
    [System.Serializable]
    public class RoleDesignationSummary
    {
        /// <summary>State validator nodes</summary>
        public List<ECPublicKey> StateValidators { get; set; } = new List<ECPublicKey>();

        /// <summary>Oracle nodes</summary>
        public List<ECPublicKey> Oracles { get; set; } = new List<ECPublicKey>();

        /// <summary>NeoFS alphabet nodes</summary>
        public List<ECPublicKey> NeoFSAlphabetNodes { get; set; } = new List<ECPublicKey>();

        /// <summary>Total number of unique designated nodes across all roles</summary>
        public int TotalDesignatedNodes { get; set; }

        /// <summary>
        /// Gets the total number of designations across all roles.
        /// </summary>
        /// <returns>Sum of all role designations</returns>
        public int GetTotalDesignations()
        {
            return StateValidators.Count + Oracles.Count + NeoFSAlphabetNodes.Count;
        }

        /// <summary>
        /// Checks if any roles have designated nodes.
        /// </summary>
        /// <returns>True if there are any role designations</returns>
        public bool HasAnyDesignations()
        {
            return GetTotalDesignations() > 0;
        }

        public override string ToString()
        {
            return $"RoleDesignationSummary(StateValidators: {StateValidators.Count}, " +
                   $"Oracles: {Oracles.Count}, NeoFS: {NeoFSAlphabetNodes.Count}, " +
                   $"Total Unique: {TotalDesignatedNodes})";
        }
    }

    /// <summary>
    /// Validation result for role designation operations.
    /// </summary>
    [System.Serializable]
    public class RoleDesignationValidation
    {
        /// <summary>The role being designated</summary>
        public Role Role { get; set; }

        /// <summary>Whether the designation is valid</summary>
        public bool IsValid { get; set; }

        /// <summary>Number of currently designated nodes for this role</summary>
        public int CurrentNodes { get; set; }

        /// <summary>Number of nodes being proposed for designation</summary>
        public int ProposedNodes { get; set; }

        /// <summary>Number of nodes that would be added</summary>
        public int NodesAdded { get; set; }

        /// <summary>Number of nodes that would be removed</summary>
        public int NodesRemoved { get; set; }

        /// <summary>Validation error message if not valid</summary>
        public string ValidationError { get; set; }

        /// <summary>
        /// Gets a summary of the changes this designation would make.
        /// </summary>
        /// <returns>Description of the changes</returns>
        public string GetChangesSummary()
        {
            if (!IsValid) return $"Invalid: {ValidationError}";

            var changes = new List<string>();
            if (NodesAdded > 0) changes.Add($"{NodesAdded} added");
            if (NodesRemoved > 0) changes.Add($"{NodesRemoved} removed");
            
            if (changes.Count == 0) return "No changes";
            return string.Join(", ", changes);
        }

        public override string ToString()
        {
            var status = IsValid ? $"Valid ({GetChangesSummary()})" : $"Invalid ({ValidationError})";
            return $"RoleDesignationValidation({Role}: {status})";
        }
    }
}
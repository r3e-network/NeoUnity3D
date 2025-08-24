using System;
using UnityEngine;
using Newtonsoft.Json;
using Neo.Unity.SDK.Types;

namespace Neo.Unity.SDK.Protocol.Response
{
    /// <summary>
    /// Represents the complete state of a deployed smart contract on the Neo blockchain.
    /// Contains contract metadata, NEF file, manifest, and update information.
    /// </summary>
    [System.Serializable]
    public class ContractState
    {
        #region Properties
        
        /// <summary>The unique ID of this contract</summary>
        [JsonProperty("id")]
        public int Id { get; set; }
        
        /// <summary>The number of times this contract has been updated</summary>
        [JsonProperty("updatecounter")]
        public int UpdateCounter { get; set; }
        
        /// <summary>The script hash of this contract</summary>
        [JsonProperty("hash")]
        public Hash160 Hash { get; set; }
        
        /// <summary>The NEF (Neo Executable Format) file containing the contract bytecode</summary>
        [JsonProperty("nef")]
        public ContractNef Nef { get; set; }
        
        /// <summary>The contract manifest describing the contract's interface and permissions</summary>
        [JsonProperty("manifest")]
        public ContractManifest Manifest { get; set; }
        
        #endregion
        
        #region Constructors
        
        /// <summary>
        /// Default constructor for JSON deserialization.
        /// </summary>
        public ContractState()
        {
        }
        
        /// <summary>
        /// Creates a new contract state.
        /// </summary>
        /// <param name="id">Contract ID</param>
        /// <param name="updateCounter">Update counter</param>
        /// <param name="hash">Contract hash</param>
        /// <param name="nef">NEF file</param>
        /// <param name="manifest">Contract manifest</param>
        public ContractState(int id, int updateCounter, Hash160 hash, ContractNef nef, ContractManifest manifest)
        {
            Id = id;
            UpdateCounter = updateCounter;
            Hash = hash;
            Nef = nef;
            Manifest = manifest;
        }
        
        #endregion
        
        #region Properties (Computed)
        
        /// <summary>Whether this contract has been updated</summary>
        [JsonIgnore]
        public bool HasBeenUpdated => UpdateCounter > 0;
        
        /// <summary>Whether this contract has a valid NEF file</summary>
        [JsonIgnore]
        public bool HasValidNef => Nef != null && Nef.HasValidMagic && Nef.HasScript;
        
        /// <summary>Whether this contract has a manifest</summary>
        [JsonIgnore]
        public bool HasManifest => Manifest != null;
        
        /// <summary>Whether this contract has a name</summary>
        [JsonIgnore]
        public bool HasName => Manifest?.HasName == true;
        
        /// <summary>The contract name (from manifest)</summary>
        [JsonIgnore]
        public string Name => Manifest?.Name ?? $"Contract_{Id}";
        
        /// <summary>Whether this contract is a NEP-17 token</summary>
        [JsonIgnore]
        public bool IsNep17Token => Manifest?.IsNep17Token == true;
        
        /// <summary>Whether this contract is a NEP-11 NFT</summary>
        [JsonIgnore]
        public bool IsNep11Token => Manifest?.IsNep11Token == true;
        
        /// <summary>Whether this contract supports any token standard</summary>
        [JsonIgnore]
        public bool IsTokenContract => IsNep17Token || IsNep11Token;
        
        /// <summary>Number of methods in this contract</summary>
        [JsonIgnore]
        public int MethodCount => Manifest?.MethodCount ?? 0;
        
        /// <summary>Number of events this contract can emit</summary>
        [JsonIgnore]
        public int EventCount => Manifest?.EventCount ?? 0;
        
        /// <summary>Size of the contract script in bytes</summary>
        [JsonIgnore]
        public int ScriptSize => Nef?.ScriptSize ?? 0;
        
        /// <summary>Number of method tokens used by this contract</summary>
        [JsonIgnore]
        public int TokenCount => Nef?.TokenCount ?? 0;
        
        #endregion
        
        #region Contract Information Methods
        
        /// <summary>
        /// Gets the compiler information used to build this contract.
        /// </summary>
        /// <returns>Compiler information or empty string</returns>
        public string GetCompilerInfo()
        {
            return Nef?.Compiler ?? string.Empty;
        }
        
        /// <summary>
        /// Gets the compiler name used to build this contract.
        /// </summary>
        /// <returns>Compiler name or empty string</returns>
        public string GetCompilerName()
        {
            return Nef?.GetCompilerName() ?? string.Empty;
        }
        
        /// <summary>
        /// Gets the compiler version used to build this contract.
        /// </summary>
        /// <returns>Compiler version or empty string</returns>
        public string GetCompilerVersion()
        {
            return Nef?.GetCompilerVersion() ?? string.Empty;
        }
        
        /// <summary>
        /// Gets all standards supported by this contract.
        /// </summary>
        /// <returns>List of supported standards</returns>
        public List<string> GetSupportedStandards()
        {
            return Manifest?.SupportedStandards ?? new List<string>();
        }
        
        /// <summary>
        /// Gets all token standards supported by this contract.
        /// </summary>
        /// <returns>List of supported token standards</returns>
        public List<string> GetSupportedTokenStandards()
        {
            return Manifest?.GetSupportedTokenStandards() ?? new List<string>();
        }
        
        /// <summary>
        /// Checks if this contract supports a specific standard.
        /// </summary>
        /// <param name="standard">The standard name (e.g., "NEP-17", "NEP-11")</param>
        /// <returns>True if the standard is supported</returns>
        public bool SupportsStandard(string standard)
        {
            return Manifest?.SupportsStandard(standard) == true;
        }
        
        #endregion
        
        #region Method Operations
        
        /// <summary>
        /// Checks if this contract has a specific method.
        /// </summary>
        /// <param name="methodName">The method name</param>
        /// <returns>True if the method exists</returns>
        public bool HasMethod(string methodName)
        {
            return Manifest?.HasMethod(methodName) == true;
        }
        
        /// <summary>
        /// Gets a method by name.
        /// </summary>
        /// <param name="methodName">The method name</param>
        /// <returns>The method or null if not found</returns>
        public ContractMethod GetMethod(string methodName)
        {
            return Manifest?.GetMethod(methodName);
        }
        
        /// <summary>
        /// Gets all public (safe) methods of this contract.
        /// </summary>
        /// <returns>List of public methods</returns>
        public List<ContractMethod> GetPublicMethods()
        {
            return Manifest?.GetPublicMethods() ?? new List<ContractMethod>();
        }
        
        /// <summary>
        /// Checks if this contract has common token methods.
        /// </summary>
        /// <returns>True if this appears to be a token contract</returns>
        public bool HasTokenMethods()
        {
            if (!HasManifest)
                return false;
            
            var commonMethods = new[] { "symbol", "decimals", "totalSupply", "balanceOf" };
            return commonMethods.All(method => HasMethod(method));
        }
        
        #endregion
        
        #region Permission Methods
        
        /// <summary>
        /// Checks if this contract has permission to call another contract's method.
        /// </summary>
        /// <param name="contractHash">The target contract hash</param>
        /// <param name="methodName">The method name</param>
        /// <returns>True if permission exists</returns>
        public bool HasPermission(string contractHash, string methodName)
        {
            return Manifest?.HasPermission(contractHash, methodName) == true;
        }
        
        /// <summary>
        /// Checks if this contract trusts another contract.
        /// </summary>
        /// <param name="contractHash">The contract hash to check</param>
        /// <returns>True if the contract is trusted</returns>
        public bool TrustsContract(string contractHash)
        {
            return Manifest?.TrustsContract(contractHash) == true;
        }
        
        /// <summary>
        /// Checks if this contract trusts all contracts.
        /// </summary>
        /// <returns>True if the contract trusts all contracts</returns>
        public bool TrustsAll()
        {
            return Manifest?.TrustsAll() == true;
        }
        
        #endregion
        
        #region Validation Methods
        
        /// <summary>
        /// Validates this contract state.
        /// </summary>
        /// <exception cref="InvalidOperationException">If validation fails</exception>
        public void Validate()
        {
            if (Id < 0)
                throw new InvalidOperationException("Contract ID cannot be negative.");
            
            if (Hash == null)
                throw new InvalidOperationException("Contract hash cannot be null.");
            
            if (UpdateCounter < 0)
                throw new InvalidOperationException("Contract update counter cannot be negative.");
            
            if (Nef == null)
                throw new InvalidOperationException("Contract NEF cannot be null.");
            
            if (Manifest == null)
                throw new InvalidOperationException("Contract manifest cannot be null.");
            
            // Validate components
            try
            {
                Nef.Validate();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Contract NEF validation failed.", ex);
            }
            
            try
            {
                Manifest.Validate();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Contract manifest validation failed.", ex);
            }
        }
        
        /// <summary>
        /// Checks if this contract state is consistent and valid.
        /// </summary>
        /// <returns>True if the contract state is valid</returns>
        public bool IsValid()
        {
            try
            {
                Validate();
                return true;
            }
            catch
            {
                return false;
            }
        }
        
        #endregion
        
        #region Statistics
        
        /// <summary>
        /// Gets comprehensive statistics about this contract.
        /// </summary>
        /// <returns>Contract statistics</returns>
        public ContractStatistics GetStatistics()
        {
            return new ContractStatistics
            {
                Id = Id,
                Name = Name,
                Hash = Hash?.ToString(),
                UpdateCounter = UpdateCounter,
                ScriptSize = ScriptSize,
                MethodCount = MethodCount,
                EventCount = EventCount,
                TokenCount = TokenCount,
                SupportedStandards = GetSupportedStandards(),
                IsTokenContract = IsTokenContract,
                CompilerName = GetCompilerName(),
                CompilerVersion = GetCompilerVersion(),
                HasValidNef = HasValidNef,
                IsValid = IsValid()
            };
        }
        
        #endregion
        
        #region Equality
        
        /// <summary>
        /// Determines whether the specified object is equal to the current contract state.
        /// </summary>
        /// <param name="obj">The object to compare</param>
        /// <returns>True if equal, false otherwise</returns>
        public override bool Equals(object obj)
        {
            if (obj is ContractState other)
            {
                return Id == other.Id && 
                       UpdateCounter == other.UpdateCounter &&
                       Hash?.Equals(other.Hash) == true &&
                       Nef?.Equals(other.Nef) == true &&
                       Manifest?.Equals(other.Manifest) == true;
            }
            return false;
        }
        
        /// <summary>
        /// Returns a hash code for the current contract state.
        /// </summary>
        /// <returns>Hash code</returns>
        public override int GetHashCode()
        {
            return HashCode.Combine(Id, UpdateCounter, Hash, Nef, Manifest);
        }
        
        #endregion
        
        #region String Representation
        
        /// <summary>
        /// Returns a detailed string representation of this contract state.
        /// </summary>
        /// <returns>Detailed string representation</returns>
        public string ToDetailedString()
        {
            var result = $"ContractState:\n";
            result += $"  ID: {Id}\n";
            result += $"  Name: {Name}\n";
            result += $"  Hash: {Hash}\n";
            result += $"  Update Counter: {UpdateCounter}\n";
            result += $"  Script Size: {ScriptSize} bytes\n";
            result += $"  Methods: {MethodCount}\n";
            result += $"  Events: {EventCount}\n";
            result += $"  Tokens: {TokenCount}\n";
            result += $"  Compiler: {GetCompilerInfo()}\n";
            
            var standards = GetSupportedStandards();
            if (standards.Count > 0)
            {
                result += $"  Standards: {string.Join(", ", standards)}\n";
            }
            
            result += $"  Valid: {IsValid()}\n";
            
            return result;
        }
        
        /// <summary>
        /// Returns a string representation of this contract state.
        /// </summary>
        /// <returns>String representation</returns>
        public override string ToString()
        {
            var standards = GetSupportedTokenStandards();
            var standardsStr = standards.Count > 0 ? $" ({string.Join(",", standards)})" : "";
            return $"ContractState(#{Id}: {Name}{standardsStr}, {MethodCount} methods, {ScriptSize}B)";
        }
        
        #endregion
    }
    
    /// <summary>
    /// Contains statistical information about a contract.
    /// </summary>
    [System.Serializable]
    public class ContractStatistics
    {
        /// <summary>Contract ID</summary>
        public int Id { get; set; }
        
        /// <summary>Contract name</summary>
        public string Name { get; set; }
        
        /// <summary>Contract hash</summary>
        public string Hash { get; set; }
        
        /// <summary>Number of updates</summary>
        public int UpdateCounter { get; set; }
        
        /// <summary>Script size in bytes</summary>
        public int ScriptSize { get; set; }
        
        /// <summary>Number of methods</summary>
        public int MethodCount { get; set; }
        
        /// <summary>Number of events</summary>
        public int EventCount { get; set; }
        
        /// <summary>Number of method tokens</summary>
        public int TokenCount { get; set; }
        
        /// <summary>Supported standards</summary>
        public List<string> SupportedStandards { get; set; }
        
        /// <summary>Whether this is a token contract</summary>
        public bool IsTokenContract { get; set; }
        
        /// <summary>Compiler name</summary>
        public string CompilerName { get; set; }
        
        /// <summary>Compiler version</summary>
        public string CompilerVersion { get; set; }
        
        /// <summary>Whether the NEF is valid</summary>
        public bool HasValidNef { get; set; }
        
        /// <summary>Whether the contract is valid overall</summary>
        public bool IsValid { get; set; }
        
        /// <summary>
        /// Default constructor.
        /// </summary>
        public ContractStatistics()
        {
            SupportedStandards = new List<string>();
        }
        
        /// <summary>
        /// Returns a string representation of these statistics.
        /// </summary>
        /// <returns>String representation</returns>
        public override string ToString()
        {
            var typeStr = IsTokenContract ? "Token" : "Contract";
            var validStr = IsValid ? "Valid" : "Invalid";
            return $"ContractStatistics({typeStr}: {Name}, {MethodCount} methods, {ScriptSize}B, {validStr})";
        }
    }
}
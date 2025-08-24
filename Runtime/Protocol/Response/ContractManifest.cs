using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Newtonsoft.Json;
using Neo.Unity.SDK.Types;

namespace Neo.Unity.SDK.Protocol.Response
{
    /// <summary>
    /// Represents a Neo contract manifest that describes a smart contract's interface, permissions, and metadata.
    /// Contains ABI information, supported standards, permissions, and trust relationships.
    /// </summary>
    [System.Serializable]
    public class ContractManifest
    {
        #region Properties
        
        /// <summary>The name of the contract</summary>
        [JsonProperty("name")]
        public string Name { get; set; }
        
        /// <summary>The groups this contract belongs to</summary>
        [JsonProperty("groups")]
        public List<ContractGroup> Groups { get; set; }
        
        /// <summary>Features supported by this contract</summary>
        [JsonProperty("features")]
        public Dictionary<string, object> Features { get; set; }
        
        /// <summary>List of standards this contract supports (NEP-17, NEP-11, etc.)</summary>
        [JsonProperty("supportedstandards")]
        public List<string> SupportedStandards { get; set; }
        
        /// <summary>The Application Binary Interface of the contract</summary>
        [JsonProperty("abi")]
        public ContractABI Abi { get; set; }
        
        /// <summary>List of permissions this contract has</summary>
        [JsonProperty("permissions")]
        public List<ContractPermission> Permissions { get; set; }
        
        /// <summary>List of contracts this contract trusts</summary>
        [JsonProperty("trusts")]
        public List<string> Trusts { get; set; }
        
        /// <summary>Extra metadata for the contract</summary>
        [JsonProperty("extra")]
        public Dictionary<string, object> Extra { get; set; }
        
        #endregion
        
        #region Constructors
        
        /// <summary>
        /// Default constructor for JSON deserialization.
        /// </summary>
        public ContractManifest()
        {
            Groups = new List<ContractGroup>();
            Features = new Dictionary<string, object>();
            SupportedStandards = new List<string>();
            Permissions = new List<ContractPermission>();
            Trusts = new List<string>();
            Extra = new Dictionary<string, object>();
        }
        
        /// <summary>
        /// Creates a new contract manifest.
        /// </summary>
        /// <param name="name">Contract name</param>
        /// <param name="groups">Contract groups</param>
        /// <param name="features">Contract features</param>
        /// <param name="supportedStandards">Supported standards</param>
        /// <param name="abi">Contract ABI</param>
        /// <param name="permissions">Contract permissions</param>
        /// <param name="trusts">Trusted contracts</param>
        /// <param name="extra">Extra metadata</param>
        public ContractManifest(string name, List<ContractGroup> groups = null, Dictionary<string, object> features = null,
                               List<string> supportedStandards = null, ContractABI abi = null, List<ContractPermission> permissions = null,
                               List<string> trusts = null, Dictionary<string, object> extra = null)
        {
            Name = name;
            Groups = groups ?? new List<ContractGroup>();
            Features = features ?? new Dictionary<string, object>();
            SupportedStandards = supportedStandards ?? new List<string>();
            Abi = abi;
            Permissions = permissions ?? new List<ContractPermission>();
            Trusts = trusts ?? new List<string>();
            Extra = extra ?? new Dictionary<string, object>();
        }
        
        #endregion
        
        #region Properties (Computed)
        
        /// <summary>Whether this contract has a name</summary>
        [JsonIgnore]
        public bool HasName => !string.IsNullOrEmpty(Name);
        
        /// <summary>Whether this contract belongs to any groups</summary>
        [JsonIgnore]
        public bool HasGroups => Groups != null && Groups.Count > 0;
        
        /// <summary>Whether this contract has features defined</summary>
        [JsonIgnore]
        public bool HasFeatures => Features != null && Features.Count > 0;
        
        /// <summary>Whether this contract supports any standards</summary>
        [JsonIgnore]
        public bool HasSupportedStandards => SupportedStandards != null && SupportedStandards.Count > 0;
        
        /// <summary>Whether this contract has an ABI</summary>
        [JsonIgnore]
        public bool HasAbi => Abi != null;
        
        /// <summary>Whether this contract has permissions</summary>
        [JsonIgnore]
        public bool HasPermissions => Permissions != null && Permissions.Count > 0;
        
        /// <summary>Whether this contract trusts other contracts</summary>
        [JsonIgnore]
        public bool HasTrusts => Trusts != null && Trusts.Count > 0;
        
        /// <summary>Whether this contract has extra metadata</summary>
        [JsonIgnore]
        public bool HasExtra => Extra != null && Extra.Count > 0;
        
        /// <summary>Whether this contract supports NEP-17 token standard</summary>
        [JsonIgnore]
        public bool IsNep17Token => SupportedStandards?.Contains("NEP-17") == true;
        
        /// <summary>Whether this contract supports NEP-11 NFT standard</summary>
        [JsonIgnore]
        public bool IsNep11Token => SupportedStandards?.Contains("NEP-11") == true;
        
        /// <summary>Number of groups this contract belongs to</summary>
        [JsonIgnore]
        public int GroupCount => Groups?.Count ?? 0;
        
        /// <summary>Number of methods in this contract</summary>
        [JsonIgnore]
        public int MethodCount => Abi?.Methods?.Count ?? 0;
        
        /// <summary>Number of events in this contract</summary>
        [JsonIgnore]
        public int EventCount => Abi?.Events?.Count ?? 0;
        
        #endregion
        
        #region Standard Support Methods
        
        /// <summary>
        /// Checks if this contract supports a specific standard.
        /// </summary>
        /// <param name="standard">The standard name (e.g., "NEP-17", "NEP-11")</param>
        /// <returns>True if the standard is supported</returns>
        public bool SupportsStandard(string standard)
        {
            return SupportedStandards?.Contains(standard) == true;
        }
        
        /// <summary>
        /// Adds support for a standard.
        /// </summary>
        /// <param name="standard">The standard name</param>
        public void AddSupportedStandard(string standard)
        {
            if (SupportedStandards == null)
                SupportedStandards = new List<string>();
            
            if (!string.IsNullOrEmpty(standard) && !SupportedStandards.Contains(standard))
                SupportedStandards.Add(standard);
        }
        
        /// <summary>
        /// Gets all supported token standards.
        /// </summary>
        /// <returns>List of supported token standards</returns>
        public List<string> GetSupportedTokenStandards()
        {
            if (!HasSupportedStandards)
                return new List<string>();
            
            return SupportedStandards.Where(s => s.StartsWith("NEP-")).ToList();
        }
        
        #endregion
        
        #region Method Operations
        
        /// <summary>
        /// Gets a method by name.
        /// </summary>
        /// <param name="methodName">The method name</param>
        /// <returns>The method or null if not found</returns>
        public ContractMethod GetMethod(string methodName)
        {
            if (string.IsNullOrEmpty(methodName) || !HasAbi || Abi.Methods == null)
                return null;
            
            return Abi.Methods.FirstOrDefault(m => m.Name == methodName);
        }
        
        /// <summary>
        /// Checks if this contract has a specific method.
        /// </summary>
        /// <param name="methodName">The method name</param>
        /// <returns>True if the method exists</returns>
        public bool HasMethod(string methodName)
        {
            return GetMethod(methodName) != null;
        }
        
        /// <summary>
        /// Gets all public methods (safe methods).
        /// </summary>
        /// <returns>List of public methods</returns>
        public List<ContractMethod> GetPublicMethods()
        {
            if (!HasAbi || Abi.Methods == null)
                return new List<ContractMethod>();
            
            return Abi.Methods.Where(m => m.Safe).ToList();
        }
        
        /// <summary>
        /// Gets all methods with specific parameter count.
        /// </summary>
        /// <param name="parameterCount">The parameter count</param>
        /// <returns>List of methods with the specified parameter count</returns>
        public List<ContractMethod> GetMethodsWithParameterCount(int parameterCount)
        {
            if (!HasAbi || Abi.Methods == null)
                return new List<ContractMethod>();
            
            return Abi.Methods.Where(m => m.Parameters?.Count == parameterCount).ToList();
        }
        
        #endregion
        
        #region Permission Methods
        
        /// <summary>
        /// Checks if this contract has permission to call a specific contract method.
        /// </summary>
        /// <param name="contractHash">The target contract hash</param>
        /// <param name="methodName">The method name</param>
        /// <returns>True if permission exists</returns>
        public bool HasPermission(string contractHash, string methodName)
        {
            if (!HasPermissions)
                return false;
            
            foreach (var permission in Permissions)
            {
                if (permission.Contract == "*" || permission.Contract == contractHash)
                {
                    if (permission.Methods == null || permission.Methods.Contains("*") || permission.Methods.Contains(methodName))
                    {
                        return true;
                    }
                }
            }
            
            return false;
        }
        
        /// <summary>
        /// Gets all contracts this manifest has permissions for.
        /// </summary>
        /// <returns>List of contract hashes or wildcards</returns>
        public List<string> GetPermittedContracts()
        {
            if (!HasPermissions)
                return new List<string>();
            
            return Permissions.Select(p => p.Contract).Distinct().ToList();
        }
        
        #endregion
        
        #region Trust Methods
        
        /// <summary>
        /// Checks if this contract trusts a specific contract.
        /// </summary>
        /// <param name="contractHash">The contract hash to check</param>
        /// <returns>True if the contract is trusted</returns>
        public bool TrustsContract(string contractHash)
        {
            if (!HasTrusts)
                return false;
            
            return Trusts.Contains("*") || Trusts.Contains(contractHash);
        }
        
        /// <summary>
        /// Checks if this contract trusts all contracts (wildcard trust).
        /// </summary>
        /// <returns>True if the contract trusts all contracts</returns>
        public bool TrustsAll()
        {
            return Trusts?.Contains("*") == true;
        }
        
        #endregion
        
        #region Validation Methods
        
        /// <summary>
        /// Validates this contract manifest.
        /// </summary>
        /// <exception cref="InvalidOperationException">If validation fails</exception>
        public void Validate()
        {
            if (string.IsNullOrEmpty(Name))
                throw new InvalidOperationException("Contract name cannot be null or empty.");
            
            if (Groups != null)
            {
                foreach (var group in Groups)
                {
                    group.Validate();
                }
            }
            
            if (Permissions != null)
            {
                foreach (var permission in Permissions)
                {
                    permission.Validate();
                }
            }
            
            Abi?.Validate();
        }
        
        #endregion
        
        #region String Representation
        
        /// <summary>
        /// Returns a detailed string representation of this manifest.
        /// </summary>
        /// <returns>Detailed string representation</returns>
        public string ToDetailedString()
        {
            var result = $"ContractManifest:\n";
            result += $"  Name: {Name}\n";
            result += $"  Groups: {GroupCount}\n";
            result += $"  Supported Standards: {string.Join(", ", SupportedStandards ?? new List<string>())}\n";
            result += $"  Methods: {MethodCount}\n";
            result += $"  Events: {EventCount}\n";
            result += $"  Permissions: {Permissions?.Count ?? 0}\n";
            result += $"  Trusts: {(TrustsAll() ? "All (*)" : (Trusts?.Count.ToString() ?? "0"))}\n";
            
            if (HasFeatures)
            {
                result += $"  Features: {string.Join(", ", Features.Keys)}\n";
            }
            
            return result;
        }
        
        /// <summary>
        /// Returns a string representation of this manifest.
        /// </summary>
        /// <returns>String representation</returns>
        public override string ToString()
        {
            var standards = HasSupportedStandards ? string.Join(",", SupportedStandards) : "None";
            return $"ContractManifest(Name: {Name}, Standards: {standards}, Methods: {MethodCount})";
        }
        
        #endregion
    }
    
    /// <summary>
    /// Represents a contract group that a contract belongs to.
    /// </summary>
    [System.Serializable]
    public class ContractGroup
    {
        /// <summary>The public key of the group</summary>
        [JsonProperty("pubkey")]
        public string PubKey { get; set; }
        
        /// <summary>The signature proving membership in the group</summary>
        [JsonProperty("signature")]
        public string Signature { get; set; }
        
        /// <summary>
        /// Default constructor for JSON deserialization.
        /// </summary>
        public ContractGroup()
        {
        }
        
        /// <summary>
        /// Creates a new contract group.
        /// </summary>
        /// <param name="pubKey">The group public key</param>
        /// <param name="signature">The membership signature</param>
        public ContractGroup(string pubKey, string signature)
        {
            PubKey = pubKey;
            Signature = signature;
        }
        
        /// <summary>
        /// Validates this contract group.
        /// </summary>
        /// <exception cref="InvalidOperationException">If validation fails</exception>
        public void Validate()
        {
            if (string.IsNullOrEmpty(PubKey))
                throw new InvalidOperationException("Contract group public key cannot be null or empty.");
            
            if (string.IsNullOrEmpty(Signature))
                throw new InvalidOperationException("Contract group signature cannot be null or empty.");
        }
        
        /// <summary>
        /// Returns a string representation of this contract group.
        /// </summary>
        /// <returns>String representation</returns>
        public override string ToString()
        {
            var keyPreview = PubKey?.Length > 8 ? PubKey.Substring(0, 8) + "..." : PubKey;
            return $"ContractGroup(PubKey: {keyPreview})";
        }
    }
    
    /// <summary>
    /// Represents the Application Binary Interface of a contract.
    /// </summary>
    [System.Serializable]
    public class ContractABI
    {
        /// <summary>List of methods in this contract</summary>
        [JsonProperty("methods")]
        public List<ContractMethod> Methods { get; set; }
        
        /// <summary>List of events this contract can emit</summary>
        [JsonProperty("events")]
        public List<ContractEvent> Events { get; set; }
        
        /// <summary>
        /// Default constructor for JSON deserialization.
        /// </summary>
        public ContractABI()
        {
            Methods = new List<ContractMethod>();
            Events = new List<ContractEvent>();
        }
        
        /// <summary>
        /// Creates a new contract ABI.
        /// </summary>
        /// <param name="methods">Contract methods</param>
        /// <param name="events">Contract events</param>
        public ContractABI(List<ContractMethod> methods, List<ContractEvent> events = null)
        {
            Methods = methods ?? new List<ContractMethod>();
            Events = events ?? new List<ContractEvent>();
        }
        
        /// <summary>
        /// Validates this contract ABI.
        /// </summary>
        /// <exception cref="InvalidOperationException">If validation fails</exception>
        public void Validate()
        {
            if (Methods != null)
            {
                foreach (var method in Methods)
                {
                    method.Validate();
                }
            }
            
            if (Events != null)
            {
                foreach (var evt in Events)
                {
                    evt.Validate();
                }
            }
        }
        
        /// <summary>
        /// Returns a string representation of this ABI.
        /// </summary>
        /// <returns>String representation</returns>
        public override string ToString()
        {
            return $"ContractABI(Methods: {Methods?.Count ?? 0}, Events: {Events?.Count ?? 0})";
        }
    }
    
    /// <summary>
    /// Represents a method in a contract.
    /// </summary>
    [System.Serializable]
    public class ContractMethod
    {
        /// <summary>The name of the method</summary>
        [JsonProperty("name")]
        public string Name { get; set; }
        
        /// <summary>List of parameters this method accepts</summary>
        [JsonProperty("parameters")]
        public List<ContractParameter> Parameters { get; set; }
        
        /// <summary>The offset of this method in the contract script</summary>
        [JsonProperty("offset")]
        public int Offset { get; set; }
        
        /// <summary>The return type of this method</summary>
        [JsonProperty("returntype")]
        public ContractParameterType ReturnType { get; set; }
        
        /// <summary>Whether this method is safe (read-only)</summary>
        [JsonProperty("safe")]
        public bool Safe { get; set; }
        
        /// <summary>
        /// Default constructor for JSON deserialization.
        /// </summary>
        public ContractMethod()
        {
            Parameters = new List<ContractParameter>();
        }
        
        /// <summary>
        /// Creates a new contract method.
        /// </summary>
        /// <param name="name">Method name</param>
        /// <param name="parameters">Method parameters</param>
        /// <param name="offset">Method offset</param>
        /// <param name="returnType">Return type</param>
        /// <param name="safe">Whether the method is safe</param>
        public ContractMethod(string name, List<ContractParameter> parameters, int offset, ContractParameterType returnType, bool safe)
        {
            Name = name;
            Parameters = parameters ?? new List<ContractParameter>();
            Offset = offset;
            ReturnType = returnType;
            Safe = safe;
        }
        
        /// <summary>Number of parameters</summary>
        [JsonIgnore]
        public int ParameterCount => Parameters?.Count ?? 0;
        
        /// <summary>
        /// Validates this contract method.
        /// </summary>
        /// <exception cref="InvalidOperationException">If validation fails</exception>
        public void Validate()
        {
            if (string.IsNullOrEmpty(Name))
                throw new InvalidOperationException("Contract method name cannot be null or empty.");
            
            if (Offset < 0)
                throw new InvalidOperationException("Contract method offset cannot be negative.");
        }
        
        /// <summary>
        /// Returns a string representation of this method.
        /// </summary>
        /// <returns>String representation</returns>
        public override string ToString()
        {
            var safeStr = Safe ? "safe" : "unsafe";
            return $"ContractMethod({Name}({ParameterCount} params) -> {ReturnType}, {safeStr})";
        }
    }
    
    /// <summary>
    /// Represents an event that a contract can emit.
    /// </summary>
    [System.Serializable]
    public class ContractEvent
    {
        /// <summary>The name of the event</summary>
        [JsonProperty("name")]
        public string Name { get; set; }
        
        /// <summary>List of parameters this event emits</summary>
        [JsonProperty("parameters")]
        public List<ContractParameter> Parameters { get; set; }
        
        /// <summary>
        /// Default constructor for JSON deserialization.
        /// </summary>
        public ContractEvent()
        {
            Parameters = new List<ContractParameter>();
        }
        
        /// <summary>
        /// Creates a new contract event.
        /// </summary>
        /// <param name="name">Event name</param>
        /// <param name="parameters">Event parameters</param>
        public ContractEvent(string name, List<ContractParameter> parameters = null)
        {
            Name = name;
            Parameters = parameters ?? new List<ContractParameter>();
        }
        
        /// <summary>Number of parameters</summary>
        [JsonIgnore]
        public int ParameterCount => Parameters?.Count ?? 0;
        
        /// <summary>
        /// Validates this contract event.
        /// </summary>
        /// <exception cref="InvalidOperationException">If validation fails</exception>
        public void Validate()
        {
            if (string.IsNullOrEmpty(Name))
                throw new InvalidOperationException("Contract event name cannot be null or empty.");
        }
        
        /// <summary>
        /// Returns a string representation of this event.
        /// </summary>
        /// <returns>String representation</returns>
        public override string ToString()
        {
            return $"ContractEvent({Name}({ParameterCount} params))";
        }
    }
    
    /// <summary>
    /// Represents a permission that a contract has.
    /// </summary>
    [System.Serializable]
    public class ContractPermission
    {
        /// <summary>The contract this permission applies to ("*" for all contracts)</summary>
        [JsonProperty("contract")]
        public string Contract { get; set; }
        
        /// <summary>The methods this permission applies to (null or ["*"] for all methods)</summary>
        [JsonProperty("methods")]
        public List<string> Methods { get; set; }
        
        /// <summary>
        /// Default constructor for JSON deserialization.
        /// </summary>
        public ContractPermission()
        {
            Methods = new List<string>();
        }
        
        /// <summary>
        /// Creates a new contract permission.
        /// </summary>
        /// <param name="contract">The target contract ("*" for all)</param>
        /// <param name="methods">The allowed methods (null or ["*"] for all)</param>
        public ContractPermission(string contract, List<string> methods = null)
        {
            Contract = contract;
            Methods = methods ?? new List<string>();
        }
        
        /// <summary>Whether this permission allows all contracts</summary>
        [JsonIgnore]
        public bool AllowsAllContracts => Contract == "*";
        
        /// <summary>Whether this permission allows all methods</summary>
        [JsonIgnore]
        public bool AllowsAllMethods => Methods == null || Methods.Contains("*");
        
        /// <summary>
        /// Validates this contract permission.
        /// </summary>
        /// <exception cref="InvalidOperationException">If validation fails</exception>
        public void Validate()
        {
            if (string.IsNullOrEmpty(Contract))
                throw new InvalidOperationException("Contract permission contract cannot be null or empty.");
        }
        
        /// <summary>
        /// Returns a string representation of this permission.
        /// </summary>
        /// <returns>String representation</returns>
        public override string ToString()
        {
            var contractStr = AllowsAllContracts ? "All" : Contract;
            var methodsStr = AllowsAllMethods ? "All" : $"{Methods?.Count ?? 0} methods";
            return $"ContractPermission(Contract: {contractStr}, Methods: {methodsStr})";
        }
    }
}
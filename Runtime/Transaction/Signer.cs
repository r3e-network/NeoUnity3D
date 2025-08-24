using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Neo.Unity.SDK.Types;
using Neo.Unity.SDK.Crypto;
using Neo.Unity.SDK.Serialization;

namespace Neo.Unity.SDK.Transaction
{
    /// <summary>
    /// A signer of a transaction. It defines a scope in which the signer's signature is valid.
    /// This determines which contracts can use the witness/signature in an invocation.
    /// </summary>
    [System.Serializable]
    public class Signer : INeoSerializable
    {
        #region Properties
        
        /// <summary>The script hash of the signer account</summary>
        [SerializeField]
        public Hash160 SignerHash { get; protected set; }
        
        /// <summary>The scopes in which the signer's signatures can be used. Multiple scopes can be combined.</summary>
        [SerializeField]
        public List<WitnessScope> Scopes { get; protected set; }
        
        /// <summary>The contract hashes of the contracts that are allowed to use the witness</summary>
        [SerializeField]
        public List<Hash160> AllowedContracts { get; protected set; }
        
        /// <summary>The group public keys of contracts that are allowed to use the witness</summary>
        [SerializeField]
        public List<ECPublicKey> AllowedGroups { get; protected set; }
        
        /// <summary>The rules that the witness must meet</summary>
        [SerializeField]
        public List<WitnessRule> Rules { get; protected set; }
        
        #endregion
        
        #region Constructor
        
        /// <summary>
        /// Creates a new signer with the specified hash and scope.
        /// </summary>
        /// <param name="signerHash">The script hash of the signer</param>
        /// <param name="scope">The witness scope</param>
        protected Signer(Hash160 signerHash, WitnessScope scope)
        {
            SignerHash = signerHash ?? throw new ArgumentNullException(nameof(signerHash));
            Scopes = new List<WitnessScope> { scope };
            AllowedContracts = new List<Hash160>();
            AllowedGroups = new List<ECPublicKey>();
            Rules = new List<WitnessRule>();
        }
        
        /// <summary>
        /// Internal constructor for deserialization.
        /// </summary>
        /// <param name="signerHash">The script hash</param>
        /// <param name="scopes">The witness scopes</param>
        /// <param name="allowedContracts">The allowed contracts</param>
        /// <param name="allowedGroups">The allowed groups</param>
        /// <param name="rules">The witness rules</param>
        protected Signer(Hash160 signerHash, List<WitnessScope> scopes, List<Hash160> allowedContracts, 
                         List<ECPublicKey> allowedGroups, List<WitnessRule> rules)
        {
            SignerHash = signerHash ?? throw new ArgumentNullException(nameof(signerHash));
            Scopes = scopes ?? new List<WitnessScope>();
            AllowedContracts = allowedContracts ?? new List<Hash160>();
            AllowedGroups = allowedGroups ?? new List<ECPublicKey>();
            Rules = rules ?? new List<WitnessRule>();
        }
        
        /// <summary>
        /// Default constructor for Unity serialization.
        /// </summary>
        protected Signer()
        {
            Scopes = new List<WitnessScope>();
            AllowedContracts = new List<Hash160>();
            AllowedGroups = new List<ECPublicKey>();
            Rules = new List<WitnessRule>();
        }
        
        #endregion
        
        #region Configuration Methods
        
        /// <summary>
        /// Adds the given contracts to this signer's scope. These contracts are allowed to use the signer's witness.
        /// </summary>
        /// <param name="allowedContracts">The hashes of the allowed contracts</param>
        /// <returns>This signer for method chaining</returns>
        /// <exception cref="ArgumentException">If configuration is invalid</exception>
        public virtual Signer SetAllowedContracts(params Hash160[] allowedContracts)
        {
            return SetAllowedContracts(allowedContracts?.ToList() ?? new List<Hash160>());
        }
        
        /// <summary>
        /// Adds the given contracts to this signer's scope. These contracts are allowed to use the signer's witness.
        /// </summary>
        /// <param name="allowedContracts">The hashes of the allowed contracts</param>
        /// <returns>This signer for method chaining</returns>
        /// <exception cref="ArgumentException">If configuration is invalid</exception>
        public virtual Signer SetAllowedContracts(List<Hash160> allowedContracts)
        {
            if (allowedContracts == null || allowedContracts.Count == 0)
                return this;
                
            if (Scopes.Contains(WitnessScope.Global))
            {
                throw new ArgumentException("Cannot set allowed contracts on a Signer with global scope.");
            }
            
            if (AllowedContracts.Count + allowedContracts.Count > NeoConstants.MAX_SIGNER_SUBITEMS)
            {
                throw new ArgumentException($"Cannot set more than {NeoConstants.MAX_SIGNER_SUBITEMS} allowed contracts on a signer.");
            }
            
            // Remove 'None' scope and add 'CustomContracts' scope
            Scopes.RemoveAll(scope => scope == WitnessScope.None);
            if (!Scopes.Contains(WitnessScope.CustomContracts))
            {
                Scopes.Add(WitnessScope.CustomContracts);
            }
            
            AllowedContracts.AddRange(allowedContracts);
            return this;
        }
        
        /// <summary>
        /// Adds the given contract groups to this signer's scope. The contracts in these groups are allowed to use the signer's witness.
        /// </summary>
        /// <param name="allowedGroups">The public keys of the allowed contract groups</param>
        /// <returns>This signer for method chaining</returns>
        /// <exception cref="ArgumentException">If configuration is invalid</exception>
        public virtual Signer SetAllowedGroups(params ECPublicKey[] allowedGroups)
        {
            return SetAllowedGroups(allowedGroups?.ToList() ?? new List<ECPublicKey>());
        }
        
        /// <summary>
        /// Adds the given contract groups to this signer's scope. The contracts in these groups are allowed to use the signer's witness.
        /// </summary>
        /// <param name="allowedGroups">The public keys of the allowed contract groups</param>
        /// <returns>This signer for method chaining</returns>
        /// <exception cref="ArgumentException">If configuration is invalid</exception>
        public virtual Signer SetAllowedGroups(List<ECPublicKey> allowedGroups)
        {
            if (allowedGroups == null || allowedGroups.Count == 0)
                return this;
                
            if (Scopes.Contains(WitnessScope.Global))
            {
                throw new ArgumentException("Cannot set allowed contract groups on a Signer with global scope.");
            }
            
            if (AllowedGroups.Count + allowedGroups.Count > NeoConstants.MAX_SIGNER_SUBITEMS)
            {
                throw new ArgumentException($"Cannot set more than {NeoConstants.MAX_SIGNER_SUBITEMS} allowed contract groups on a signer.");
            }
            
            // Remove 'None' scope and add 'CustomGroups' scope
            Scopes.RemoveAll(scope => scope == WitnessScope.None);
            if (!Scopes.Contains(WitnessScope.CustomGroups))
            {
                Scopes.Add(WitnessScope.CustomGroups);
            }
            
            AllowedGroups.AddRange(allowedGroups);
            return this;
        }
        
        /// <summary>
        /// Adds the given witness rules to this signer.
        /// </summary>
        /// <param name="rules">The witness rules</param>
        /// <returns>This signer for method chaining</returns>
        /// <exception cref="ArgumentException">If configuration is invalid</exception>
        public virtual Signer SetRules(params WitnessRule[] rules)
        {
            return SetRules(rules?.ToList() ?? new List<WitnessRule>());
        }
        
        /// <summary>
        /// Adds the given witness rules to this signer.
        /// </summary>
        /// <param name="rules">The witness rules</param>
        /// <returns>This signer for method chaining</returns>
        /// <exception cref="ArgumentException">If configuration is invalid</exception>
        public virtual Signer SetRules(List<WitnessRule> rules)
        {
            if (rules == null || rules.Count == 0)
                return this;
                
            if (Scopes.Contains(WitnessScope.Global))
            {
                throw new ArgumentException("Cannot set witness rules on a Signer with global scope.");
            }
            
            if (Rules.Count + rules.Count > NeoConstants.MAX_SIGNER_SUBITEMS)
            {
                throw new ArgumentException($"Cannot set more than {NeoConstants.MAX_SIGNER_SUBITEMS} witness rules on a signer.");
            }
            
            // Validate rule nesting depth
            foreach (var rule in rules)
            {
                CheckRuleDepth(rule.Condition, WitnessCondition.MAX_NESTING_DEPTH);
            }
            
            // Remove 'None' scope and add 'WitnessRules' scope
            Scopes.RemoveAll(scope => scope == WitnessScope.None);
            if (!Scopes.Contains(WitnessScope.WitnessRules))
            {
                Scopes.Add(WitnessScope.WitnessRules);
            }
            
            Rules.AddRange(rules);
            return this;
        }
        
        /// <summary>
        /// Recursively checks the nesting depth of witness conditions.
        /// </summary>
        /// <param name="condition">The condition to check</param>
        /// <param name="depth">The remaining allowed depth</param>
        private void CheckRuleDepth(WitnessCondition condition, int depth)
        {
            if (depth < 0)
            {
                throw new ArgumentException($"A maximum nesting depth of {WitnessCondition.MAX_NESTING_DEPTH} is allowed for witness conditions.");
            }
            
            switch (condition.Type)
            {
                case WitnessConditionType.And:
                case WitnessConditionType.Or:
                    if (condition.Expressions != null)
                    {
                        foreach (var expr in condition.Expressions)
                        {
                            CheckRuleDepth(expr, depth - 1);
                        }
                    }
                    break;
            }
        }
        
        #endregion
        
        #region Equality and Hashing
        
        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="obj">The object to compare</param>
        /// <returns>True if equal, false otherwise</returns>
        public override bool Equals(object obj)
        {
            if (obj is Signer other)
            {
                return SignerHash.Equals(other.SignerHash) &&
                       Scopes.SequenceEqual(other.Scopes) &&
                       AllowedContracts.SequenceEqual(other.AllowedContracts) &&
                       AllowedGroups.SequenceEqual(other.AllowedGroups) &&
                       Rules.SequenceEqual(other.Rules);
            }
            return false;
        }
        
        /// <summary>
        /// Returns a hash code for the current object.
        /// </summary>
        /// <returns>Hash code</returns>
        public override int GetHashCode()
        {
            var hashCode = SignerHash.GetHashCode();
            hashCode = HashCode.Combine(hashCode, Scopes);
            hashCode = HashCode.Combine(hashCode, AllowedContracts);
            hashCode = HashCode.Combine(hashCode, AllowedGroups);
            hashCode = HashCode.Combine(hashCode, Rules);
            return hashCode;
        }
        
        #endregion
        
        #region INeoSerializable Implementation
        
        /// <summary>
        /// Gets the size in bytes when serialized.
        /// </summary>
        public virtual int Size
        {
            get
            {
                var size = NeoConstants.HASH160_SIZE + 1; // Hash + scope byte
                
                if (Scopes.Contains(WitnessScope.CustomContracts))
                {
                    size += AllowedContracts.GetVarSize();
                }
                
                if (Scopes.Contains(WitnessScope.CustomGroups))
                {
                    size += AllowedGroups.GetVarSize();
                }
                
                if (Scopes.Contains(WitnessScope.WitnessRules))
                {
                    size += Rules.GetVarSize();
                }
                
                return size;
            }
        }
        
        /// <summary>
        /// Serializes the signer to a binary writer.
        /// </summary>
        /// <param name="writer">The binary writer</param>
        public virtual void Serialize(BinaryWriter writer)
        {
            if (writer == null)
                throw new ArgumentNullException(nameof(writer));
            
            writer.WriteSerializableFixed(SignerHash);
            writer.WriteByte(WitnessScopeHelper.CombineScopes(Scopes));
            
            if (Scopes.Contains(WitnessScope.CustomContracts))
            {
                writer.WriteSerializableVariable(AllowedContracts);
            }
            
            if (Scopes.Contains(WitnessScope.CustomGroups))
            {
                writer.WriteSerializableVariable(AllowedGroups);
            }
            
            if (Scopes.Contains(WitnessScope.WitnessRules))
            {
                writer.WriteSerializableVariable(Rules);
            }
        }
        
        /// <summary>
        /// Deserializes a signer from a binary reader.
        /// </summary>
        /// <param name="reader">The binary reader</param>
        /// <returns>The deserialized signer</returns>
        public static Signer Deserialize(BinaryReader reader)
        {
            if (reader == null)
                throw new ArgumentNullException(nameof(reader));
            
            var signerHash = reader.ReadSerializable<Hash160>();
            var scopes = WitnessScopeHelper.ExtractCombinedScopes(reader.ReadByte());
            var allowedContracts = new List<Hash160>();
            var allowedGroups = new List<ECPublicKey>();
            var rules = new List<WitnessRule>();
            
            var allowedScopes = new[] { WitnessScope.CustomContracts, WitnessScope.CustomGroups, WitnessScope.WitnessRules };
            
            foreach (var scope in allowedScopes)
            {
                if (scopes.Contains(scope))
                {
                    int count = 0;
                    string errorLabel = "";
                    
                    switch (scope)
                    {
                        case WitnessScope.CustomContracts:
                            allowedContracts = reader.ReadSerializableList<Hash160>();
                            count = allowedContracts.Count;
                            errorLabel = "allowed contracts";
                            break;
                            
                        case WitnessScope.CustomGroups:
                            allowedGroups = reader.ReadSerializableList<ECPublicKey>();
                            count = allowedGroups.Count;
                            errorLabel = "allowed contract groups";
                            break;
                            
                        case WitnessScope.WitnessRules:
                            rules = reader.ReadSerializableList<WitnessRule>();
                            count = rules.Count;
                            errorLabel = "rules";
                            break;
                    }
                    
                    if (count > NeoConstants.MAX_SIGNER_SUBITEMS)
                    {
                        throw new ArgumentException($"A signer's scope can only contain {NeoConstants.MAX_SIGNER_SUBITEMS} {errorLabel}. The input data contained {count}.");
                    }
                }
            }
            
            return new Signer(signerHash, scopes, allowedContracts, allowedGroups, rules);
        }
        
        #endregion
        
        #region String Representation
        
        /// <summary>
        /// Returns a string representation of this signer.
        /// </summary>
        /// <returns>String representation</returns>
        public override string ToString()
        {
            var scopeStr = string.Join(", ", Scopes);
            return $"Signer(Hash: {SignerHash}, Scopes: {scopeStr})";
        }
        
        #endregion
    }
    
    /// <summary>
    /// Helper class for working with witness scopes.
    /// </summary>
    public static class WitnessScopeHelper
    {
        /// <summary>
        /// Combines multiple witness scopes into a single byte value.
        /// </summary>
        /// <param name="scopes">The scopes to combine</param>
        /// <returns>Combined scope byte</returns>
        public static byte CombineScopes(List<WitnessScope> scopes)
        {
            if (scopes == null || scopes.Count == 0)
                return (byte)WitnessScope.None;
            
            byte result = 0;
            foreach (var scope in scopes)
            {
                result |= (byte)scope;
            }
            
            return result;
        }
        
        /// <summary>
        /// Extracts individual witness scopes from a combined byte value.
        /// </summary>
        /// <param name="combinedScopes">The combined scope byte</param>
        /// <returns>List of individual scopes</returns>
        public static List<WitnessScope> ExtractCombinedScopes(byte combinedScopes)
        {
            var result = new List<WitnessScope>();
            
            foreach (WitnessScope scope in Enum.GetValues(typeof(WitnessScope)))
            {
                if ((combinedScopes & (byte)scope) != 0)
                {
                    result.Add(scope);
                }
            }
            
            return result.Count > 0 ? result : new List<WitnessScope> { WitnessScope.None };
        }
    }
}
using System;
using Newtonsoft.Json;
using UnityEngine;
using NeoUnity.Cryptography;

namespace NeoUnity.Protocol.Response
{
    /// <summary>
    /// Represents an Express contract state with hash and manifest
    /// </summary>
    [Serializable]
    public class ExpressContractState : IEquatable<ExpressContractState>
    {
        [JsonProperty("hash")]
        [SerializeField] private string _hash;
        
        [JsonProperty("manifest")]
        [SerializeField] private ContractManifest _manifest;
        
        /// <summary>
        /// Gets the contract hash
        /// </summary>
        public Hash160 Hash => new Hash160(_hash);
        
        /// <summary>
        /// Gets the contract manifest
        /// </summary>
        public ContractManifest Manifest => _manifest;
        
        /// <summary>
        /// Initializes a new instance of ExpressContractState
        /// </summary>
        /// <param name="hash">Contract hash</param>
        /// <param name="manifest">Contract manifest</param>
        [JsonConstructor]
        public ExpressContractState(string hash, ContractManifest manifest)
        {
            _hash = hash ?? throw new ArgumentNullException(nameof(hash));
            _manifest = manifest ?? throw new ArgumentNullException(nameof(manifest));
        }
        
        /// <summary>
        /// Initializes a new instance of ExpressContractState
        /// </summary>
        /// <param name="hash">Contract hash</param>
        /// <param name="manifest">Contract manifest</param>
        public ExpressContractState(Hash160 hash, ContractManifest manifest)
        {
            _hash = hash?.ToString() ?? throw new ArgumentNullException(nameof(hash));
            _manifest = manifest ?? throw new ArgumentNullException(nameof(manifest));
        }
        
        public bool Equals(ExpressContractState other)
        {
            if (other == null) return false;
            return _hash == other._hash && Equals(_manifest, other._manifest);
        }
        
        public override bool Equals(object obj)
        {
            return obj is ExpressContractState other && Equals(other);
        }
        
        public override int GetHashCode()
        {
            return HashCode.Combine(_hash, _manifest);
        }
        
        public static bool operator ==(ExpressContractState left, ExpressContractState right)
        {
            if (ReferenceEquals(left, right)) return true;
            if (left is null || right is null) return false;
            return left.Equals(right);
        }
        
        public static bool operator !=(ExpressContractState left, ExpressContractState right)
        {
            return !(left == right);
        }
        
        public override string ToString()
        {
            return $"ExpressContractState(Hash: {_hash}, Manifest: {_manifest})";
        }
    }
}
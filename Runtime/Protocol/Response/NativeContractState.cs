using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;
using NeoUnity.Cryptography;

namespace NeoUnity.Protocol.Response
{
    /// <summary>
    /// Represents a native contract state extending ExpressContractState
    /// </summary>
    [Serializable]
    public class NativeContractState : ExpressContractState
    {
        [JsonProperty("id")]
        [SerializeField] private int _id;
        
        [JsonProperty("nef")]
        [SerializeField] private ContractNef _nef;
        
        [JsonProperty("updatehistory")]
        [SerializeField] private List<int> _updateHistory;
        
        /// <summary>
        /// Gets the contract ID
        /// </summary>
        public int Id => _id;
        
        /// <summary>
        /// Gets the contract NEF (Neo Executable Format)
        /// </summary>
        public ContractNef Nef => _nef;
        
        /// <summary>
        /// Gets the update history as a list of block heights
        /// </summary>
        public List<int> UpdateHistory => _updateHistory ?? new List<int>();
        
        /// <summary>
        /// Initializes a new instance of NativeContractState
        /// </summary>
        /// <param name="id">Contract ID</param>
        /// <param name="hash">Contract hash</param>
        /// <param name="nef">Contract NEF</param>
        /// <param name="manifest">Contract manifest</param>
        /// <param name="updateHistory">Update history</param>
        public NativeContractState(
            int id, 
            Hash160 hash, 
            ContractNef nef, 
            ContractManifest manifest, 
            List<int> updateHistory) 
            : base(hash, manifest)
        {
            _id = id;
            _nef = nef ?? throw new ArgumentNullException(nameof(nef));
            _updateHistory = updateHistory ?? new List<int>();
        }
        
        /// <summary>
        /// Initializes a new instance of NativeContractState for JSON deserialization
        /// </summary>
        /// <param name="id">Contract ID</param>
        /// <param name="hash">Contract hash as string</param>
        /// <param name="nef">Contract NEF</param>
        /// <param name="manifest">Contract manifest</param>
        /// <param name="updateHistory">Update history</param>
        [JsonConstructor]
        public NativeContractState(
            int id,
            string hash,
            ContractNef nef,
            ContractManifest manifest,
            List<int> updateHistory)
            : base(hash, manifest)
        {
            _id = id;
            _nef = nef ?? throw new ArgumentNullException(nameof(nef));
            _updateHistory = updateHistory ?? new List<int>();
        }
        
        /// <summary>
        /// Checks if the contract has been updated at the specified block height
        /// </summary>
        /// <param name="blockHeight">Block height to check</param>
        /// <returns>True if updated at the specified height</returns>
        public bool WasUpdatedAtHeight(int blockHeight)
        {
            return _updateHistory?.Contains(blockHeight) ?? false;
        }
        
        /// <summary>
        /// Gets the latest update height
        /// </summary>
        /// <returns>Latest update height or -1 if no updates</returns>
        public int GetLatestUpdateHeight()
        {
            if (_updateHistory == null || _updateHistory.Count == 0)
                return -1;
                
            int maxHeight = _updateHistory[0];
            for (int i = 1; i < _updateHistory.Count; i++)
            {
                if (_updateHistory[i] > maxHeight)
                    maxHeight = _updateHistory[i];
            }
            return maxHeight;
        }
        
        public override bool Equals(object obj)
        {
            if (obj is not NativeContractState other) return false;
            
            return base.Equals(other) && 
                   _id == other._id && 
                   Equals(_nef, other._nef) && 
                   UpdateHistory.Count == other.UpdateHistory.Count;
        }
        
        public override int GetHashCode()
        {
            return HashCode.Combine(base.GetHashCode(), _id, _nef, _updateHistory?.Count ?? 0);
        }
        
        public override string ToString()
        {
            return $"NativeContractState(Id: {_id}, Hash: {Hash}, UpdateHistory: [{string.Join(", ", _updateHistory ?? new List<int>())}])";
        }
    }
}
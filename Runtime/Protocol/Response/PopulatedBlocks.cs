using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;

namespace NeoUnity.Protocol.Response
{
    /// <summary>
    /// Represents a collection of populated block indices with cache ID
    /// </summary>
    [Serializable]
    public struct PopulatedBlocks : IEquatable<PopulatedBlocks>
    {
        [JsonProperty("cacheId")]
        [SerializeField] private string _cacheId;
        
        [JsonProperty("blocks")]
        [SerializeField] private List<int> _blocks;
        
        /// <summary>
        /// Gets the cache identifier for this block collection
        /// </summary>
        public string CacheId => _cacheId;
        
        /// <summary>
        /// Gets the list of block indices that are populated
        /// </summary>
        public List<int> Blocks => _blocks ?? new List<int>();
        
        /// <summary>
        /// Gets the number of populated blocks
        /// </summary>
        public int Count => _blocks?.Count ?? 0;
        
        /// <summary>
        /// Gets whether there are any populated blocks
        /// </summary>
        public bool HasBlocks => _blocks != null && _blocks.Count > 0;
        
        /// <summary>
        /// Initializes a new instance of PopulatedBlocks
        /// </summary>
        /// <param name="cacheId">Cache identifier</param>
        /// <param name="blocks">List of populated block indices</param>
        [JsonConstructor]
        public PopulatedBlocks(string cacheId, List<int> blocks)
        {
            _cacheId = cacheId ?? throw new ArgumentNullException(nameof(cacheId));
            _blocks = blocks ?? new List<int>();
        }
        
        /// <summary>
        /// Creates a PopulatedBlocks from an array
        /// </summary>
        /// <param name="cacheId">Cache identifier</param>
        /// <param name="blocks">Array of populated block indices</param>
        /// <returns>PopulatedBlocks instance</returns>
        public static PopulatedBlocks FromArray(string cacheId, int[] blocks)
        {
            return new PopulatedBlocks(cacheId, blocks?.ToList() ?? new List<int>());
        }
        
        /// <summary>
        /// Gets the lowest populated block index
        /// </summary>
        /// <returns>Lowest block index or -1 if no blocks</returns>
        public int GetMinBlockIndex()
        {
            if (!HasBlocks) return -1;
            
            int min = _blocks[0];
            for (int i = 1; i < _blocks.Count; i++)
            {
                if (_blocks[i] < min)
                    min = _blocks[i];
            }
            return min;
        }
        
        /// <summary>
        /// Gets the highest populated block index
        /// </summary>
        /// <returns>Highest block index or -1 if no blocks</returns>
        public int GetMaxBlockIndex()
        {
            if (!HasBlocks) return -1;
            
            int max = _blocks[0];
            for (int i = 1; i < _blocks.Count; i++)
            {
                if (_blocks[i] > max)
                    max = _blocks[i];
            }
            return max;
        }
        
        /// <summary>
        /// Checks if a specific block index is populated
        /// </summary>
        /// <param name="blockIndex">Block index to check</param>
        /// <returns>True if block is populated</returns>
        public bool ContainsBlock(int blockIndex)
        {
            return _blocks?.Contains(blockIndex) ?? false;
        }
        
        /// <summary>
        /// Gets the block range covered by populated blocks
        /// </summary>
        /// <returns>Tuple of (min, max) block indices or (-1, -1) if no blocks</returns>
        public (int min, int max) GetBlockRange()
        {
            if (!HasBlocks) return (-1, -1);
            return (GetMinBlockIndex(), GetMaxBlockIndex());
        }
        
        /// <summary>
        /// Gets blocks within a specific range
        /// </summary>
        /// <param name="startBlock">Start block index (inclusive)</param>
        /// <param name="endBlock">End block index (inclusive)</param>
        /// <returns>List of blocks within the specified range</returns>
        public List<int> GetBlocksInRange(int startBlock, int endBlock)
        {
            if (!HasBlocks) return new List<int>();
            
            return _blocks.Where(block => block >= startBlock && block <= endBlock).ToList();
        }
        
        /// <summary>
        /// Finds gaps in the populated block sequence
        /// </summary>
        /// <returns>List of missing block indices</returns>
        public List<int> FindGaps()
        {
            if (!HasBlocks || Count <= 1) return new List<int>();
            
            var sortedBlocks = _blocks.OrderBy(x => x).ToList();
            var gaps = new List<int>();
            
            for (int i = 0; i < sortedBlocks.Count - 1; i++)
            {
                int current = sortedBlocks[i];
                int next = sortedBlocks[i + 1];
                
                for (int gap = current + 1; gap < next; gap++)
                {
                    gaps.Add(gap);
                }
            }
            
            return gaps;
        }
        
        /// <summary>
        /// Gets statistics about the populated blocks
        /// </summary>
        /// <returns>Formatted statistics string</returns>
        public string GetStatistics()
        {
            if (!HasBlocks) return "No populated blocks";
            
            var (min, max) = GetBlockRange();
            var gaps = FindGaps().Count;
            var coverage = Count / (double)(max - min + 1) * 100;
            
            return $"Blocks: {Count}, Range: {min}-{max}, Gaps: {gaps}, Coverage: {coverage:F1}%";
        }
        
        /// <summary>
        /// Validates the cache ID format
        /// </summary>
        /// <returns>True if cache ID is valid</returns>
        public bool IsValidCacheId()
        {
            return !string.IsNullOrEmpty(_cacheId) && _cacheId.Length >= 8;
        }
        
        public bool Equals(PopulatedBlocks other)
        {
            return _cacheId == other._cacheId && 
                   Count == other.Count &&
                   (Blocks.SequenceEqual(other.Blocks));
        }
        
        public override bool Equals(object obj)
        {
            return obj is PopulatedBlocks other && Equals(other);
        }
        
        public override int GetHashCode()
        {
            return HashCode.Combine(_cacheId, Count);
        }
        
        public static bool operator ==(PopulatedBlocks left, PopulatedBlocks right)
        {
            return left.Equals(right);
        }
        
        public static bool operator !=(PopulatedBlocks left, PopulatedBlocks right)
        {
            return !left.Equals(right);
        }
        
        public override string ToString()
        {
            return $"PopulatedBlocks(CacheId: {_cacheId}, Count: {Count}, {GetStatistics()})";
        }
    }
}
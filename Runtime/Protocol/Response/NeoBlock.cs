using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Newtonsoft.Json;
using Neo.Unity.SDK.Types;

namespace Neo.Unity.SDK.Protocol.Response
{
    /// <summary>
    /// Represents a Neo blockchain block with all its transactions and metadata.
    /// Contains block header information, consensus data, and transaction list.
    /// </summary>
    [System.Serializable]
    public class NeoBlock
    {
        #region Properties
        
        /// <summary>The unique hash of this block</summary>
        [JsonProperty("hash")]
        public Hash256 Hash { get; set; }
        
        /// <summary>The size of this block in bytes</summary>
        [JsonProperty("size")]
        public int Size { get; set; }
        
        /// <summary>The block version</summary>
        [JsonProperty("version")]
        public int Version { get; set; }
        
        /// <summary>The hash of the previous block</summary>
        [JsonProperty("previousblockhash")]
        public Hash256 PreviousBlockHash { get; set; }
        
        /// <summary>The merkle root of all transactions in this block</summary>
        [JsonProperty("merkleroot")]
        public Hash256 MerkleRootHash { get; set; }
        
        /// <summary>The timestamp of this block</summary>
        [JsonProperty("time")]
        public long Time { get; set; }
        
        /// <summary>The block index (height)</summary>
        [JsonProperty("index")]
        public long Index { get; set; }
        
        /// <summary>The primary consensus node index</summary>
        [JsonProperty("primary")]
        public int? Primary { get; set; }
        
        /// <summary>The address of the next consensus participant</summary>
        [JsonProperty("nextconsensus")]
        public string NextConsensus { get; set; }
        
        /// <summary>The witness data for block consensus</summary>
        [JsonProperty("witnesses")]
        public List<NeoWitness> Witnesses { get; set; }
        
        /// <summary>The transactions included in this block</summary>
        [JsonProperty("tx")]
        public List<NeoTransaction> Transactions { get; set; }
        
        /// <summary>The number of confirmations this block has</summary>
        [JsonProperty("confirmations")]
        public int Confirmations { get; set; }
        
        /// <summary>The hash of the next block (if available)</summary>
        [JsonProperty("nextblockhash")]
        public Hash256 NextBlockHash { get; set; }
        
        #endregion
        
        #region Constructors
        
        /// <summary>
        /// Default constructor for JSON deserialization.
        /// </summary>
        public NeoBlock()
        {
            Witnesses = new List<NeoWitness>();
            Transactions = new List<NeoTransaction>();
        }
        
        /// <summary>
        /// Creates a new Neo block.
        /// </summary>
        /// <param name="hash">Block hash</param>
        /// <param name="size">Block size</param>
        /// <param name="version">Block version</param>
        /// <param name="previousBlockHash">Previous block hash</param>
        /// <param name="merkleRootHash">Merkle root hash</param>
        /// <param name="time">Block timestamp</param>
        /// <param name="index">Block index</param>
        /// <param name="nextConsensus">Next consensus address</param>
        /// <param name="confirmations">Number of confirmations</param>
        /// <param name="witnesses">Block witnesses</param>
        /// <param name="transactions">Block transactions</param>
        /// <param name="primary">Primary consensus node</param>
        /// <param name="nextBlockHash">Next block hash</param>
        public NeoBlock(Hash256 hash, int size, int version, Hash256 previousBlockHash, Hash256 merkleRootHash, 
                       long time, long index, string nextConsensus, int confirmations,
                       List<NeoWitness> witnesses = null, List<NeoTransaction> transactions = null, 
                       int? primary = null, Hash256 nextBlockHash = null)
        {
            Hash = hash;
            Size = size;
            Version = version;
            PreviousBlockHash = previousBlockHash;
            MerkleRootHash = merkleRootHash;
            Time = time;
            Index = index;
            NextConsensus = nextConsensus;
            Confirmations = confirmations;
            Witnesses = witnesses ?? new List<NeoWitness>();
            Transactions = transactions ?? new List<NeoTransaction>();
            Primary = primary;
            NextBlockHash = nextBlockHash;
        }
        
        #endregion
        
        #region Properties (Computed)
        
        /// <summary>Whether this block is the genesis block</summary>
        [JsonIgnore]
        public bool IsGenesisBlock => Index == 0;
        
        /// <summary>Whether this block has witnesses</summary>
        [JsonIgnore]
        public bool HasWitnesses => Witnesses != null && Witnesses.Count > 0;
        
        /// <summary>Whether this block has transactions</summary>
        [JsonIgnore]
        public bool HasTransactions => Transactions != null && Transactions.Count > 0;
        
        /// <summary>Whether this is the latest block (next block hash is null)</summary>
        [JsonIgnore]
        public bool IsLatestBlock => NextBlockHash == null;
        
        /// <summary>Number of witnesses</summary>
        [JsonIgnore]
        public int WitnessCount => Witnesses?.Count ?? 0;
        
        /// <summary>Number of transactions</summary>
        [JsonIgnore]
        public int TransactionCount => Transactions?.Count ?? 0;
        
        /// <summary>Block height (alias for Index)</summary>
        [JsonIgnore]
        public long Height => Index;
        
        #endregion
        
        #region Time Methods
        
        /// <summary>
        /// Gets the block time as a DateTime.
        /// </summary>
        /// <returns>Block time as DateTime</returns>
        public DateTime GetBlockTimeDateTime()
        {
            try
            {
                return DateTimeOffset.FromUnixTimeSeconds(Time).DateTime;
            }
            catch (ArgumentOutOfRangeException)
            {
                return DateTime.MinValue;
            }
        }
        
        /// <summary>
        /// Gets the block time as a DateTimeOffset.
        /// </summary>
        /// <returns>Block time as DateTimeOffset</returns>
        public DateTimeOffset GetBlockTimeOffset()
        {
            try
            {
                return DateTimeOffset.FromUnixTimeSeconds(Time);
            }
            catch (ArgumentOutOfRangeException)
            {
                return DateTimeOffset.MinValue;
            }
        }
        
        /// <summary>
        /// Gets the age of this block from now.
        /// </summary>
        /// <returns>Time since this block was created</returns>
        public TimeSpan GetBlockAge()
        {
            var blockTime = GetBlockTimeDateTime();
            return DateTime.UtcNow - blockTime;
        }
        
        #endregion
        
        #region Transaction Methods
        
        /// <summary>
        /// Gets a transaction by its hash.
        /// </summary>
        /// <param name="transactionHash">The transaction hash</param>
        /// <returns>The transaction or null if not found</returns>
        public NeoTransaction GetTransaction(Hash256 transactionHash)
        {
            if (transactionHash == null || !HasTransactions)
                return null;
            
            return Transactions.FirstOrDefault(tx => tx.Hash?.Equals(transactionHash) == true);
        }
        
        /// <summary>
        /// Gets a transaction by its hash.
        /// </summary>
        /// <param name="transactionHashString">The transaction hash as string</param>
        /// <returns>The transaction or null if not found</returns>
        public NeoTransaction GetTransaction(string transactionHashString)
        {
            if (string.IsNullOrEmpty(transactionHashString) || !HasTransactions)
                return null;
            
            return Transactions.FirstOrDefault(tx => tx.Hash?.ToString() == transactionHashString);
        }
        
        /// <summary>
        /// Checks if this block contains a specific transaction.
        /// </summary>
        /// <param name="transactionHash">The transaction hash</param>
        /// <returns>True if the transaction is in this block</returns>
        public bool ContainsTransaction(Hash256 transactionHash)
        {
            return GetTransaction(transactionHash) != null;
        }
        
        /// <summary>
        /// Checks if this block contains a specific transaction.
        /// </summary>
        /// <param name="transactionHashString">The transaction hash as string</param>
        /// <returns>True if the transaction is in this block</returns>
        public bool ContainsTransaction(string transactionHashString)
        {
            return GetTransaction(transactionHashString) != null;
        }
        
        /// <summary>
        /// Gets all transactions from a specific sender.
        /// </summary>
        /// <param name="senderAddress">The sender address</param>
        /// <returns>List of transactions from the sender</returns>
        public List<NeoTransaction> GetTransactionsFromSender(string senderAddress)
        {
            if (string.IsNullOrEmpty(senderAddress) || !HasTransactions)
                return new List<NeoTransaction>();
            
            return Transactions.Where(tx => tx.Sender == senderAddress).ToList();
        }
        
        /// <summary>
        /// Gets all successful transactions (state = HALT).
        /// </summary>
        /// <returns>List of successful transactions</returns>
        public List<NeoTransaction> GetSuccessfulTransactions()
        {
            if (!HasTransactions)
                return new List<NeoTransaction>();
            
            return Transactions.Where(tx => tx.IsSuccessful).ToList();
        }
        
        /// <summary>
        /// Gets all failed transactions (state = FAULT).
        /// </summary>
        /// <returns>List of failed transactions</returns>
        public List<NeoTransaction> GetFailedTransactions()
        {
            if (!HasTransactions)
                return new List<NeoTransaction>();
            
            return Transactions.Where(tx => tx.HasFaulted).ToList();
        }
        
        /// <summary>
        /// Calculates the total fees paid in this block.
        /// </summary>
        /// <returns>Total fees as decimal</returns>
        public decimal CalculateTotalFees()
        {
            if (!HasTransactions)
                return 0;
            
            return Transactions.Sum(tx => tx.GetTotalFeeDecimal());
        }
        
        #endregion
        
        #region Validation Methods
        
        /// <summary>
        /// Validates the basic structure of this block.
        /// </summary>
        /// <exception cref="InvalidOperationException">If validation fails</exception>
        public void Validate()
        {
            if (Hash == null)
                throw new InvalidOperationException("Block hash cannot be null.");
            
            if (PreviousBlockHash == null && !IsGenesisBlock)
                throw new InvalidOperationException("Previous block hash cannot be null for non-genesis blocks.");
            
            if (MerkleRootHash == null)
                throw new InvalidOperationException("Merkle root hash cannot be null.");
            
            if (string.IsNullOrEmpty(NextConsensus))
                throw new InvalidOperationException("Next consensus address cannot be null or empty.");
            
            if (Size <= 0)
                throw new InvalidOperationException("Block size must be positive.");
            
            if (Index < 0)
                throw new InvalidOperationException("Block index must be non-negative.");
            
            if (Time <= 0)
                throw new InvalidOperationException("Block time must be positive.");
        }
        
        #endregion
        
        #region Statistics
        
        /// <summary>
        /// Gets statistics about this block.
        /// </summary>
        /// <returns>Block statistics</returns>
        public BlockStatistics GetStatistics()
        {
            return new BlockStatistics
            {
                TransactionCount = TransactionCount,
                SuccessfulTransactionCount = GetSuccessfulTransactions().Count,
                FailedTransactionCount = GetFailedTransactions().Count,
                TotalFees = CalculateTotalFees(),
                BlockSize = Size,
                AverageTransactionSize = TransactionCount > 0 ? (double)Size / TransactionCount : 0,
                BlockAge = GetBlockAge()
            };
        }
        
        #endregion
        
        #region String Representation
        
        /// <summary>
        /// Returns a detailed string representation of this block.
        /// </summary>
        /// <returns>Detailed string representation</returns>
        public string ToDetailedString()
        {
            var result = $"NeoBlock:\n";
            result += $"  Hash: {Hash}\n";
            result += $"  Index: {Index}\n";
            result += $"  Size: {Size} bytes\n";
            result += $"  Version: {Version}\n";
            result += $"  Time: {GetBlockTimeDateTime()} ({Time})\n";
            result += $"  Previous Block: {PreviousBlockHash}\n";
            result += $"  Merkle Root: {MerkleRootHash}\n";
            result += $"  Next Consensus: {NextConsensus}\n";
            result += $"  Transactions: {TransactionCount}\n";
            result += $"  Witnesses: {WitnessCount}\n";
            result += $"  Confirmations: {Confirmations}\n";
            
            if (Primary.HasValue)
            {
                result += $"  Primary: {Primary}\n";
            }
            
            if (NextBlockHash != null)
            {
                result += $"  Next Block: {NextBlockHash}\n";
            }
            else
            {
                result += $"  Status: Latest Block\n";
            }
            
            var stats = GetStatistics();
            result += $"  Total Fees: {stats.TotalFees}\n";
            result += $"  Success Rate: {stats.SuccessRate:P1}\n";
            
            return result;
        }
        
        /// <summary>
        /// Returns a string representation of this block.
        /// </summary>
        /// <returns>String representation</returns>
        public override string ToString()
        {
            return $"NeoBlock(#{Index}, {Hash}, {TransactionCount} txs, {Size}B)";
        }
        
        #endregion
    }
    
    /// <summary>
    /// Contains statistical information about a block.
    /// </summary>
    [System.Serializable]
    public class BlockStatistics
    {
        /// <summary>Total number of transactions</summary>
        public int TransactionCount { get; set; }
        
        /// <summary>Number of successful transactions</summary>
        public int SuccessfulTransactionCount { get; set; }
        
        /// <summary>Number of failed transactions</summary>
        public int FailedTransactionCount { get; set; }
        
        /// <summary>Total fees paid in this block</summary>
        public decimal TotalFees { get; set; }
        
        /// <summary>Block size in bytes</summary>
        public int BlockSize { get; set; }
        
        /// <summary>Average transaction size in bytes</summary>
        public double AverageTransactionSize { get; set; }
        
        /// <summary>Age of this block</summary>
        public TimeSpan BlockAge { get; set; }
        
        /// <summary>Success rate of transactions in this block</summary>
        [JsonIgnore]
        public double SuccessRate => TransactionCount > 0 ? (double)SuccessfulTransactionCount / TransactionCount : 0.0;
        
        /// <summary>
        /// Returns a string representation of these statistics.
        /// </summary>
        /// <returns>String representation</returns>
        public override string ToString()
        {
            return $"BlockStatistics(Txs: {TransactionCount}, Success: {SuccessRate:P1}, Fees: {TotalFees}, Size: {BlockSize}B)";
        }
    }
}
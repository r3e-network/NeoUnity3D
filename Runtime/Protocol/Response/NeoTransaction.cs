using System;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using Neo.Unity.SDK.Types;

namespace Neo.Unity.SDK.Protocol.Response
{
    /// <summary>
    /// Represents a Neo transaction with all its components.
    /// Contains transaction metadata, signers, witnesses, and execution information.
    /// </summary>
    [System.Serializable]
    public class NeoTransaction
    {
        #region Properties
        
        /// <summary>The unique hash of this transaction</summary>
        [JsonProperty("hash")]
        public Hash256 Hash { get; set; }
        
        /// <summary>The size of this transaction in bytes</summary>
        [JsonProperty("size")]
        public int Size { get; set; }
        
        /// <summary>The transaction version</summary>
        [JsonProperty("version")]
        public int Version { get; set; }
        
        /// <summary>Random number to prevent transaction replay</summary>
        [JsonProperty("nonce")]
        public long Nonce { get; set; }
        
        /// <summary>The sender address of this transaction</summary>
        [JsonProperty("sender")]
        public string Sender { get; set; }
        
        /// <summary>System fee paid for this transaction</summary>
        [JsonProperty("sysfee")]
        public string SystemFee { get; set; }
        
        /// <summary>Network fee paid for this transaction</summary>
        [JsonProperty("netfee")]
        public string NetworkFee { get; set; }
        
        /// <summary>Block number until which this transaction is valid</summary>
        [JsonProperty("validuntilblock")]
        public long ValidUntilBlock { get; set; }
        
        /// <summary>List of signers for this transaction</summary>
        [JsonProperty("signers")]
        public List<TransactionSigner> Signers { get; set; }
        
        /// <summary>List of transaction attributes</summary>
        [JsonProperty("attributes")]
        public List<TransactionAttribute> Attributes { get; set; }
        
        /// <summary>The script that this transaction executes</summary>
        [JsonProperty("script")]
        public string Script { get; set; }
        
        /// <summary>List of witnesses (signatures) for this transaction</summary>
        [JsonProperty("witnesses")]
        public List<NeoWitness> Witnesses { get; set; }
        
        /// <summary>Hash of the block containing this transaction (if mined)</summary>
        [JsonProperty("blockhash")]
        public Hash256 BlockHash { get; set; }
        
        /// <summary>Number of confirmations (if mined)</summary>
        [JsonProperty("confirmations")]
        public int? Confirmations { get; set; }
        
        /// <summary>Timestamp of the block containing this transaction (if mined)</summary>
        [JsonProperty("blocktime")]
        public long? BlockTime { get; set; }
        
        /// <summary>The VM state after execution</summary>
        [JsonProperty("vmstate")]
        public NeoVMStateType? VMState { get; set; }
        
        #endregion
        
        #region Constructors
        
        /// <summary>
        /// Default constructor for JSON deserialization.
        /// </summary>
        public NeoTransaction()
        {
            Signers = new List<TransactionSigner>();
            Attributes = new List<TransactionAttribute>();
            Witnesses = new List<NeoWitness>();
        }
        
        /// <summary>
        /// Creates a new Neo transaction.
        /// </summary>
        /// <param name="hash">Transaction hash</param>
        /// <param name="size">Transaction size</param>
        /// <param name="version">Transaction version</param>
        /// <param name="nonce">Transaction nonce</param>
        /// <param name="sender">Sender address</param>
        /// <param name="systemFee">System fee</param>
        /// <param name="networkFee">Network fee</param>
        /// <param name="validUntilBlock">Valid until block</param>
        /// <param name="script">Transaction script</param>
        /// <param name="signers">Transaction signers</param>
        /// <param name="attributes">Transaction attributes</param>
        /// <param name="witnesses">Transaction witnesses</param>
        public NeoTransaction(Hash256 hash, int size, int version, long nonce, string sender, 
                             string systemFee, string networkFee, long validUntilBlock, string script,
                             List<TransactionSigner> signers = null, List<TransactionAttribute> attributes = null, 
                             List<NeoWitness> witnesses = null)
        {
            Hash = hash;
            Size = size;
            Version = version;
            Nonce = nonce;
            Sender = sender;
            SystemFee = systemFee;
            NetworkFee = networkFee;
            ValidUntilBlock = validUntilBlock;
            Script = script;
            Signers = signers ?? new List<TransactionSigner>();
            Attributes = attributes ?? new List<TransactionAttribute>();
            Witnesses = witnesses ?? new List<NeoWitness>();
        }
        
        #endregion
        
        #region Properties (Computed)
        
        /// <summary>Whether this transaction has been mined into a block</summary>
        [JsonIgnore]
        public bool IsConfirmed => BlockHash != null && Confirmations.HasValue;
        
        /// <summary>Whether this transaction is still in the mempool</summary>
        [JsonIgnore]
        public bool IsPending => !IsConfirmed;
        
        /// <summary>Whether this transaction execution was successful</summary>
        [JsonIgnore]
        public bool IsSuccessful => VMState == NeoVMStateType.Halt;
        
        /// <summary>Whether this transaction execution faulted</summary>
        [JsonIgnore]
        public bool HasFaulted => VMState == NeoVMStateType.Fault;
        
        /// <summary>Whether this transaction has signers</summary>
        [JsonIgnore]
        public bool HasSigners => Signers != null && Signers.Count > 0;
        
        /// <summary>Whether this transaction has attributes</summary>
        [JsonIgnore]
        public bool HasAttributes => Attributes != null && Attributes.Count > 0;
        
        /// <summary>Whether this transaction has witnesses</summary>
        [JsonIgnore]
        public bool HasWitnesses => Witnesses != null && Witnesses.Count > 0;
        
        /// <summary>Number of signers</summary>
        [JsonIgnore]
        public int SignerCount => Signers?.Count ?? 0;
        
        /// <summary>Number of attributes</summary>
        [JsonIgnore]
        public int AttributeCount => Attributes?.Count ?? 0;
        
        /// <summary>Number of witnesses</summary>
        [JsonIgnore]
        public int WitnessCount => Witnesses?.Count ?? 0;
        
        #endregion
        
        #region Fee Methods
        
        /// <summary>
        /// Gets the system fee as a decimal value.
        /// </summary>
        /// <returns>System fee as decimal</returns>
        public decimal GetSystemFeeDecimal()
        {
            if (string.IsNullOrEmpty(SystemFee))
                return 0;
            
            if (decimal.TryParse(SystemFee, out var fee))
                return fee;
            
            return 0;
        }
        
        /// <summary>
        /// Gets the network fee as a decimal value.
        /// </summary>
        /// <returns>Network fee as decimal</returns>
        public decimal GetNetworkFeeDecimal()
        {
            if (string.IsNullOrEmpty(NetworkFee))
                return 0;
            
            if (decimal.TryParse(NetworkFee, out var fee))
                return fee;
            
            return 0;
        }
        
        /// <summary>
        /// Gets the total fee as a decimal value.
        /// </summary>
        /// <returns>Total fee as decimal</returns>
        public decimal GetTotalFeeDecimal()
        {
            return GetSystemFeeDecimal() + GetNetworkFeeDecimal();
        }
        
        /// <summary>
        /// Gets the system fee in GAS fractions (smallest unit).
        /// </summary>
        /// <returns>System fee in fractions</returns>
        public long GetSystemFeeFractions()
        {
            var feeDecimal = GetSystemFeeDecimal();
            return (long)(feeDecimal * 100_000_000); // Convert to GAS fractions
        }
        
        /// <summary>
        /// Gets the network fee in GAS fractions (smallest unit).
        /// </summary>
        /// <returns>Network fee in fractions</returns>
        public long GetNetworkFeeFractions()
        {
            var feeDecimal = GetNetworkFeeDecimal();
            return (long)(feeDecimal * 100_000_000); // Convert to GAS fractions
        }
        
        /// <summary>
        /// Gets the total fee in GAS fractions (smallest unit).
        /// </summary>
        /// <returns>Total fee in fractions</returns>
        public long GetTotalFeeFractions()
        {
            return GetSystemFeeFractions() + GetNetworkFeeFractions();
        }
        
        #endregion
        
        #region Script Methods
        
        /// <summary>
        /// Gets the transaction script as decoded bytes.
        /// </summary>
        /// <returns>The script bytes</returns>
        /// <exception cref="FormatException">If the script is not valid base64</exception>
        public byte[] GetScriptBytes()
        {
            if (string.IsNullOrEmpty(Script))
                return new byte[0];
            
            try
            {
                return Convert.FromBase64String(Script);
            }
            catch (FormatException ex)
            {
                throw new FormatException($"Invalid base64 transaction script: {Script}", ex);
            }
        }
        
        /// <summary>
        /// Tries to get the transaction script as decoded bytes.
        /// </summary>
        /// <param name="scriptBytes">The script bytes if successful</param>
        /// <returns>True if successful, false if invalid base64</returns>
        public bool TryGetScriptBytes(out byte[] scriptBytes)
        {
            try
            {
                scriptBytes = GetScriptBytes();
                return true;
            }
            catch
            {
                scriptBytes = null;
                return false;
            }
        }
        
        #endregion
        
        #region Validation Methods
        
        /// <summary>
        /// Validates the basic structure of this transaction.
        /// </summary>
        /// <exception cref="InvalidOperationException">If validation fails</exception>
        public void Validate()
        {
            if (Hash == null)
                throw new InvalidOperationException("Transaction hash cannot be null.");
            
            if (string.IsNullOrEmpty(Sender))
                throw new InvalidOperationException("Transaction sender cannot be null or empty.");
            
            if (string.IsNullOrEmpty(Script))
                throw new InvalidOperationException("Transaction script cannot be null or empty.");
            
            if (Size <= 0)
                throw new InvalidOperationException("Transaction size must be positive.");
            
            if (ValidUntilBlock <= 0)
                throw new InvalidOperationException("ValidUntilBlock must be positive.");
            
            // Validate script is valid base64
            try
            {
                Convert.FromBase64String(Script);
            }
            catch (FormatException ex)
            {
                throw new InvalidOperationException("Transaction script is not valid base64.", ex);
            }
        }
        
        /// <summary>
        /// Throws an exception if this transaction execution faulted.
        /// </summary>
        /// <exception cref="TransactionExecutionException">If the transaction faulted</exception>
        public void ThrowIfFaulted()
        {
            if (HasFaulted)
            {
                throw new TransactionExecutionException($"Transaction {Hash} execution faulted with state: {VMState}");
            }
        }
        
        #endregion
        
        #region Block Information
        
        /// <summary>
        /// Gets the block time as a DateTime (if available).
        /// </summary>
        /// <returns>Block time as DateTime or null</returns>
        public DateTime? GetBlockTimeDateTime()
        {
            if (!BlockTime.HasValue)
                return null;
            
            try
            {
                return DateTimeOffset.FromUnixTimeSeconds(BlockTime.Value).DateTime;
            }
            catch (ArgumentOutOfRangeException)
            {
                return null;
            }
        }
        
        /// <summary>
        /// Gets the confirmation status description.
        /// </summary>
        /// <returns>Confirmation status</returns>
        public string GetConfirmationStatus()
        {
            if (!IsConfirmed)
                return "Pending (Mempool)";
            
            if (Confirmations == 1)
                return "1 Confirmation";
            
            return $"{Confirmations} Confirmations";
        }
        
        #endregion
        
        #region String Representation
        
        /// <summary>
        /// Returns a detailed string representation of this transaction.
        /// </summary>
        /// <returns>Detailed string representation</returns>
        public string ToDetailedString()
        {
            var result = $"NeoTransaction:\n";
            result += $"  Hash: {Hash}\n";
            result += $"  Size: {Size} bytes\n";
            result += $"  Version: {Version}\n";
            result += $"  Sender: {Sender}\n";
            result += $"  System Fee: {SystemFee}\n";
            result += $"  Network Fee: {NetworkFee}\n";
            result += $"  Valid Until Block: {ValidUntilBlock}\n";
            result += $"  Signers: {SignerCount}\n";
            result += $"  Attributes: {AttributeCount}\n";
            result += $"  Witnesses: {WitnessCount}\n";
            
            if (IsConfirmed)
            {
                result += $"  Block Hash: {BlockHash}\n";
                result += $"  Confirmations: {Confirmations}\n";
                if (BlockTime.HasValue)
                {
                    result += $"  Block Time: {GetBlockTimeDateTime()}\n";
                }
            }
            else
            {
                result += $"  Status: Pending (Mempool)\n";
            }
            
            if (VMState.HasValue)
            {
                result += $"  VM State: {VMState}\n";
            }
            
            return result;
        }
        
        /// <summary>
        /// Returns a string representation of this transaction.
        /// </summary>
        /// <returns>String representation</returns>
        public override string ToString()
        {
            var status = IsConfirmed ? $"{Confirmations} conf" : "pending";
            return $"NeoTransaction({Hash}, {Size}B, {status})";
        }
        
        #endregion
    }
    
    /// <summary>
    /// Exception thrown when transaction execution fails.
    /// </summary>
    public class TransactionExecutionException : Exception
    {
        /// <summary>
        /// Creates a new transaction execution exception.
        /// </summary>
        /// <param name="message">The exception message</param>
        public TransactionExecutionException(string message) : base(message)
        {
        }
        
        /// <summary>
        /// Creates a new transaction execution exception with an inner exception.
        /// </summary>
        /// <param name="message">The exception message</param>
        /// <param name="innerException">The inner exception</param>
        public TransactionExecutionException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
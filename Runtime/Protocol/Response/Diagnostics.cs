using System;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

namespace Neo.Unity.SDK.Protocol.Response
{
    /// <summary>
    /// Contains diagnostic information about smart contract execution.
    /// Provides detailed performance and execution metrics.
    /// </summary>
    [System.Serializable]
    public class Diagnostics
    {
        #region Properties
        
        /// <summary>Total CPU time consumed during execution</summary>
        [JsonProperty("cputime")]
        public long CpuTime { get; set; }
        
        /// <summary>Number of storage operations performed</summary>
        [JsonProperty("storageoperations")]
        public int StorageOperations { get; set; }
        
        /// <summary>Details of individual storage operations</summary>
        [JsonProperty("storageitem")]
        public List<StorageItem> StorageItems { get; set; }
        
        /// <summary>Number of invoked contracts</summary>
        [JsonProperty("invokedcontracts")]
        public List<InvokedContract> InvokedContracts { get; set; }
        
        #endregion
        
        #region Constructors
        
        /// <summary>
        /// Default constructor for JSON deserialization.
        /// </summary>
        public Diagnostics()
        {
            StorageItems = new List<StorageItem>();
            InvokedContracts = new List<InvokedContract>();
        }
        
        /// <summary>
        /// Creates new diagnostics information.
        /// </summary>
        /// <param name="cpuTime">CPU time consumed</param>
        /// <param name="storageOperations">Number of storage operations</param>
        /// <param name="storageItems">Storage operation details</param>
        /// <param name="invokedContracts">Invoked contract details</param>
        public Diagnostics(long cpuTime, int storageOperations, List<StorageItem> storageItems = null, List<InvokedContract> invokedContracts = null)
        {
            CpuTime = cpuTime;
            StorageOperations = storageOperations;
            StorageItems = storageItems ?? new List<StorageItem>();
            InvokedContracts = invokedContracts ?? new List<InvokedContract>();
        }
        
        #endregion
        
        #region Properties (Computed)
        
        /// <summary>Whether this execution performed storage operations</summary>
        [JsonIgnore]
        public bool HasStorageOperations => StorageOperations > 0;
        
        /// <summary>Whether this execution invoked other contracts</summary>
        [JsonIgnore]
        public bool HasInvokedContracts => InvokedContracts != null && InvokedContracts.Count > 0;
        
        /// <summary>CPU time in milliseconds</summary>
        [JsonIgnore]
        public double CpuTimeMilliseconds => CpuTime / 1000.0;
        
        /// <summary>CPU time in microseconds</summary>
        [JsonIgnore]
        public double CpuTimeMicroseconds => CpuTime;
        
        #endregion
        
        #region String Representation
        
        /// <summary>
        /// Returns a detailed string representation of the diagnostics.
        /// </summary>
        /// <returns>String representation</returns>
        public override string ToString()
        {
            return $"Diagnostics(CPU: {CpuTimeMilliseconds:F2}ms, Storage: {StorageOperations}, Contracts: {InvokedContracts?.Count ?? 0})";
        }
        
        #endregion
    }
    
    /// <summary>
    /// Represents a storage operation performed during contract execution.
    /// </summary>
    [System.Serializable]
    public class StorageItem
    {
        /// <summary>The storage key that was accessed</summary>
        [JsonProperty("key")]
        public string Key { get; set; }
        
        /// <summary>The storage value</summary>
        [JsonProperty("value")]
        public string Value { get; set; }
        
        /// <summary>The type of operation (get, put, delete, etc.)</summary>
        [JsonProperty("operation")]
        public string Operation { get; set; }
        
        /// <summary>
        /// Default constructor for JSON deserialization.
        /// </summary>
        public StorageItem()
        {
        }
        
        /// <summary>
        /// Creates a new storage item.
        /// </summary>
        /// <param name="key">The storage key</param>
        /// <param name="value">The storage value</param>
        /// <param name="operation">The operation type</param>
        public StorageItem(string key, string value, string operation)
        {
            Key = key;
            Value = value;
            Operation = operation;
        }
        
        /// <summary>
        /// Returns a string representation of this storage item.
        /// </summary>
        /// <returns>String representation</returns>
        public override string ToString()
        {
            return $"StorageItem({Operation}: {Key} = {Value?.Substring(0, Math.Min(Value?.Length ?? 0, 20))}{(Value?.Length > 20 ? "..." : "")})";
        }
    }
    
    /// <summary>
    /// Represents a contract that was invoked during execution.
    /// </summary>
    [System.Serializable]
    public class InvokedContract
    {
        /// <summary>The hash of the invoked contract</summary>
        [JsonProperty("hash")]
        public string Hash { get; set; }
        
        /// <summary>The method that was called</summary>
        [JsonProperty("method")]
        public string Method { get; set; }
        
        /// <summary>Number of times this contract method was called</summary>
        [JsonProperty("callcount")]
        public int CallCount { get; set; }
        
        /// <summary>
        /// Default constructor for JSON deserialization.
        /// </summary>
        public InvokedContract()
        {
        }
        
        /// <summary>
        /// Creates a new invoked contract record.
        /// </summary>
        /// <param name="hash">The contract hash</param>
        /// <param name="method">The method name</param>
        /// <param name="callCount">The call count</param>
        public InvokedContract(string hash, string method, int callCount)
        {
            Hash = hash;
            Method = method;
            CallCount = callCount;
        }
        
        /// <summary>
        /// Returns a string representation of this invoked contract.
        /// </summary>
        /// <returns>String representation</returns>
        public override string ToString()
        {
            return $"InvokedContract({Hash}::{Method} x{CallCount})";
        }
    }
}
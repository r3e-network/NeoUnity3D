using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Newtonsoft.Json;
using Neo.Unity.SDK.Types;

namespace Neo.Unity.SDK.Protocol.Response
{
    /// <summary>
    /// Response for getting application execution logs for a transaction.
    /// Contains execution results, gas consumption, and emitted notifications.
    /// </summary>
    [System.Serializable]
    public class NeoGetApplicationLogResponse : NeoResponse<NeoApplicationLog>
    {
        /// <summary>
        /// Gets the application log from the response.
        /// </summary>
        /// <returns>Application log or null if response failed</returns>
        public NeoApplicationLog GetApplicationLog()
        {
            return IsSuccess ? Result : null;
        }
        
        /// <summary>
        /// Gets the application log or throws if the response failed.
        /// </summary>
        /// <returns>Application log</returns>
        /// <exception cref="NeoRpcException">If the response contains an error</exception>
        public NeoApplicationLog GetApplicationLogOrThrow()
        {
            return GetResult();
        }
    }
    
    /// <summary>
    /// Represents an application execution log for a Neo transaction.
    /// Contains detailed information about contract executions and their results.
    /// </summary>
    [System.Serializable]
    public class NeoApplicationLog
    {
        #region Properties
        
        /// <summary>The transaction ID this log belongs to</summary>
        [JsonProperty("txid")]
        public Hash256 TransactionId { get; set; }
        
        /// <summary>List of executions that occurred during transaction processing</summary>
        [JsonProperty("executions")]
        public List<Execution> Executions { get; set; }
        
        #endregion
        
        #region Constructors
        
        /// <summary>
        /// Default constructor for JSON deserialization.
        /// </summary>
        public NeoApplicationLog()
        {
            Executions = new List<Execution>();
        }
        
        /// <summary>
        /// Creates a new application log.
        /// </summary>
        /// <param name="transactionId">The transaction ID</param>
        /// <param name="executions">The executions</param>
        public NeoApplicationLog(Hash256 transactionId, List<Execution> executions = null)
        {
            TransactionId = transactionId;
            Executions = executions ?? new List<Execution>();
        }
        
        #endregion
        
        #region Properties (Computed)
        
        /// <summary>Whether there are any executions</summary>
        [JsonIgnore]
        public bool HasExecutions => Executions != null && Executions.Count > 0;
        
        /// <summary>Number of executions</summary>
        [JsonIgnore]
        public int ExecutionCount => Executions?.Count ?? 0;
        
        /// <summary>Whether all executions were successful</summary>
        [JsonIgnore]
        public bool AllExecutionsSuccessful => HasExecutions && Executions.All(e => e.IsSuccessful);
        
        /// <summary>Whether any execution faulted</summary>
        [JsonIgnore]
        public bool HasFaultedExecution => HasExecutions && Executions.Any(e => e.HasFaulted);
        
        /// <summary>Number of successful executions</summary>
        [JsonIgnore]
        public int SuccessfulExecutionCount => Executions?.Count(e => e.IsSuccessful) ?? 0;
        
        /// <summary>Number of faulted executions</summary>
        [JsonIgnore]
        public int FaultedExecutionCount => Executions?.Count(e => e.HasFaulted) ?? 0;
        
        /// <summary>Total number of notifications across all executions</summary>
        [JsonIgnore]
        public int TotalNotificationCount => Executions?.Sum(e => e.NotificationCount) ?? 0;
        
        #endregion
        
        #region Execution Methods
        
        /// <summary>
        /// Gets executions of a specific trigger type.
        /// </summary>
        /// <param name="triggerType">The trigger type</param>
        /// <returns>List of matching executions</returns>
        public List<Execution> GetExecutionsByTrigger(string triggerType)
        {
            if (string.IsNullOrEmpty(triggerType) || !HasExecutions)
                return new List<Execution>();
            
            return Executions.Where(e => e.Trigger?.Equals(triggerType, StringComparison.OrdinalIgnoreCase) == true).ToList();
        }
        
        /// <summary>
        /// Gets all successful executions.
        /// </summary>
        /// <returns>List of successful executions</returns>
        public List<Execution> GetSuccessfulExecutions()
        {
            if (!HasExecutions)
                return new List<Execution>();
            
            return Executions.Where(e => e.IsSuccessful).ToList();
        }
        
        /// <summary>
        /// Gets all faulted executions.
        /// </summary>
        /// <returns>List of faulted executions</returns>
        public List<Execution> GetFaultedExecutions()
        {
            if (!HasExecutions)
                return new List<Execution>();
            
            return Executions.Where(e => e.HasFaulted).ToList();
        }
        
        /// <summary>
        /// Gets the main execution (usually the first Application trigger).
        /// </summary>
        /// <returns>The main execution or null</returns>
        public Execution GetMainExecution()
        {
            if (!HasExecutions)
                return null;
            
            // Look for Application trigger first
            var appExecution = Executions.FirstOrDefault(e => e.Trigger == "Application");
            if (appExecution != null)
                return appExecution;
            
            // Fall back to first execution
            return Executions.FirstOrDefault();
        }
        
        #endregion
        
        #region Gas Calculation
        
        /// <summary>
        /// Calculates the total gas consumed across all executions.
        /// </summary>
        /// <returns>Total gas consumed as decimal</returns>
        public decimal GetTotalGasConsumed()
        {
            if (!HasExecutions)
                return 0;
            
            return Executions.Sum(e => e.GetGasConsumedDecimal());
        }
        
        /// <summary>
        /// Gets the gas consumed by successful executions only.
        /// </summary>
        /// <returns>Gas consumed by successful executions</returns>
        public decimal GetSuccessfulGasConsumed()
        {
            return GetSuccessfulExecutions().Sum(e => e.GetGasConsumedDecimal());
        }
        
        /// <summary>
        /// Gets the gas wasted by faulted executions.
        /// </summary>
        /// <returns>Gas wasted by faulted executions</returns>
        public decimal GetWastedGas()
        {
            return GetFaultedExecutions().Sum(e => e.GetGasConsumedDecimal());
        }
        
        #endregion
        
        #region Notification Methods
        
        /// <summary>
        /// Gets all notifications from all executions.
        /// </summary>
        /// <returns>List of all notifications</returns>
        public List<Notification> GetAllNotifications()
        {
            if (!HasExecutions)
                return new List<Notification>();
            
            var allNotifications = new List<Notification>();
            foreach (var execution in Executions)
            {
                if (execution.HasNotifications)
                    allNotifications.AddRange(execution.Notifications);
            }
            
            return allNotifications;
        }
        
        /// <summary>
        /// Gets notifications from a specific contract.
        /// </summary>
        /// <param name="contractHash">The contract hash</param>
        /// <returns>List of notifications from the contract</returns>
        public List<Notification> GetNotificationsFromContract(Hash160 contractHash)
        {
            if (contractHash == null)
                return new List<Notification>();
            
            return GetAllNotifications().Where(n => n.Contract?.Equals(contractHash) == true).ToList();
        }
        
        /// <summary>
        /// Gets notifications with a specific event name.
        /// </summary>
        /// <param name="eventName">The event name</param>
        /// <returns>List of matching notifications</returns>
        public List<Notification> GetNotificationsByEvent(string eventName)
        {
            if (string.IsNullOrEmpty(eventName))
                return new List<Notification>();
            
            return GetAllNotifications().Where(n => n.EventName == eventName).ToList();
        }
        
        /// <summary>
        /// Gets all unique contracts that emitted notifications.
        /// </summary>
        /// <returns>List of unique contract hashes</returns>
        public List<Hash160> GetNotifyingContracts()
        {
            var contracts = new HashSet<Hash160>();
            
            foreach (var notification in GetAllNotifications())
            {
                if (notification.Contract != null)
                    contracts.Add(notification.Contract);
            }
            
            return contracts.ToList();
        }
        
        /// <summary>
        /// Gets all unique event names.
        /// </summary>
        /// <returns>List of unique event names</returns>
        public List<string> GetUniqueEventNames()
        {
            var eventNames = new HashSet<string>();
            
            foreach (var notification in GetAllNotifications())
            {
                if (!string.IsNullOrEmpty(notification.EventName))
                    eventNames.Add(notification.EventName);
            }
            
            return eventNames.ToList();
        }
        
        #endregion
        
        #region Stack Operations
        
        /// <summary>
        /// Gets the result stack from the main execution.
        /// </summary>
        /// <returns>Main execution stack or empty list</returns>
        public List<StackItem> GetMainExecutionStack()
        {
            var mainExecution = GetMainExecution();
            return mainExecution?.Stack ?? new List<StackItem>();
        }
        
        /// <summary>
        /// Gets the first stack item from the main execution.
        /// </summary>
        /// <returns>First stack item or null</returns>
        public StackItem GetMainExecutionResult()
        {
            var stack = GetMainExecutionStack();
            return stack.Count > 0 ? stack[0] : null;
        }
        
        #endregion
        
        #region Statistics
        
        /// <summary>
        /// Gets execution statistics.
        /// </summary>
        /// <returns>Application log statistics</returns>
        public ApplicationLogStatistics GetStatistics()
        {
            return new ApplicationLogStatistics
            {
                TransactionId = TransactionId?.ToString(),
                ExecutionCount = ExecutionCount,
                SuccessfulExecutions = SuccessfulExecutionCount,
                FaultedExecutions = FaultedExecutionCount,
                TotalNotifications = TotalNotificationCount,
                TotalGasConsumed = GetTotalGasConsumed(),
                SuccessfulGasConsumed = GetSuccessfulGasConsumed(),
                WastedGas = GetWastedGas(),
                UniqueContracts = GetNotifyingContracts().Count,
                UniqueEvents = GetUniqueEventNames().Count,
                IsSuccessful = AllExecutionsSuccessful
            };
        }
        
        #endregion
        
        #region Validation Methods
        
        /// <summary>
        /// Validates this application log.
        /// </summary>
        /// <exception cref="InvalidOperationException">If validation fails</exception>
        public void Validate()
        {
            if (TransactionId == null)
                throw new InvalidOperationException("Application log transaction ID cannot be null.");
            
            if (Executions != null)
            {
                foreach (var execution in Executions)
                {
                    execution.Validate();
                }
            }
        }
        
        #endregion
        
        #region String Representation
        
        /// <summary>
        /// Returns a detailed string representation of this application log.
        /// </summary>
        /// <returns>Detailed string representation</returns>
        public string ToDetailedString()
        {
            var stats = GetStatistics();
            var result = $"Application Log for {TransactionId}:\n";
            result += $"  Executions: {ExecutionCount} ({SuccessfulExecutionCount} successful, {FaultedExecutionCount} faulted)\n";
            result += $"  Total Gas: {GetTotalGasConsumed()}\n";
            result += $"  Notifications: {TotalNotificationCount}\n";
            result += $"  Contracts: {stats.UniqueContracts}\n";
            result += $"  Events: {stats.UniqueEvents}\n";
            result += $"  Overall Success: {(AllExecutionsSuccessful ? "Yes" : "No")}\n";
            
            if (HasExecutions)
            {
                result += $"\n  Executions:\n";
                for (int i = 0; i < Math.Min(Executions.Count, 5); i++)
                {
                    var execution = Executions[i];
                    result += $"    [{i}] {execution.Trigger}: {execution.State} (Gas: {execution.GasConsumed})\n";
                }
                
                if (Executions.Count > 5)
                {
                    result += $"    ... and {Executions.Count - 5} more executions\n";
                }
            }
            
            return result;
        }
        
        /// <summary>
        /// Returns a string representation of this application log.
        /// </summary>
        /// <returns>String representation</returns>
        public override string ToString()
        {
            var status = AllExecutionsSuccessful ? "Success" : HasFaultedExecution ? "Fault" : "Mixed";
            return $"NeoApplicationLog({TransactionId}, {ExecutionCount} executions, {status})";
        }
        
        #endregion
    }
    
    /// <summary>
    /// Represents a single execution within an application log.
    /// Contains execution state, gas consumption, results, and notifications.
    /// </summary>
    [System.Serializable]
    public class Execution
    {
        #region Properties
        
        /// <summary>The trigger type for this execution (e.g., "Application", "Verification")</summary>
        [JsonProperty("trigger")]
        public string Trigger { get; set; }
        
        /// <summary>The VM state after execution</summary>
        [JsonProperty("vmstate")]
        public NeoVMStateType State { get; set; }
        
        /// <summary>Exception message if execution faulted</summary>
        [JsonProperty("exception")]
        public string Exception { get; set; }
        
        /// <summary>Gas consumed during this execution</summary>
        [JsonProperty("gasconsumed")]
        public string GasConsumed { get; set; }
        
        /// <summary>VM stack after execution</summary>
        [JsonProperty("stack")]
        public List<StackItem> Stack { get; set; }
        
        /// <summary>Notifications emitted during execution</summary>
        [JsonProperty("notifications")]
        public List<Notification> Notifications { get; set; }
        
        #endregion
        
        #region Constructors
        
        /// <summary>
        /// Default constructor for JSON deserialization.
        /// </summary>
        public Execution()
        {
            Stack = new List<StackItem>();
            Notifications = new List<Notification>();
        }
        
        /// <summary>
        /// Creates a new execution.
        /// </summary>
        /// <param name="trigger">Trigger type</param>
        /// <param name="state">VM state</param>
        /// <param name="exception">Exception message</param>
        /// <param name="gasConsumed">Gas consumed</param>
        /// <param name="stack">VM stack</param>
        /// <param name="notifications">Notifications</param>
        public Execution(string trigger, NeoVMStateType state, string exception, string gasConsumed,
                        List<StackItem> stack = null, List<Notification> notifications = null)
        {
            Trigger = trigger;
            State = state;
            Exception = exception;
            GasConsumed = gasConsumed;
            Stack = stack ?? new List<StackItem>();
            Notifications = notifications ?? new List<Notification>();
        }
        
        #endregion
        
        #region Properties (Computed)
        
        /// <summary>Whether this execution was successful</summary>
        [JsonIgnore]
        public bool IsSuccessful => State == NeoVMStateType.Halt;
        
        /// <summary>Whether this execution faulted</summary>
        [JsonIgnore]
        public bool HasFaulted => State == NeoVMStateType.Fault;
        
        /// <summary>Whether this execution has an exception</summary>
        [JsonIgnore]
        public bool HasException => !string.IsNullOrEmpty(Exception);
        
        /// <summary>Whether this execution has stack items</summary>
        [JsonIgnore]
        public bool HasStackItems => Stack != null && Stack.Count > 0;
        
        /// <summary>Whether this execution has notifications</summary>
        [JsonIgnore]
        public bool HasNotifications => Notifications != null && Notifications.Count > 0;
        
        /// <summary>Number of stack items</summary>
        [JsonIgnore]
        public int StackItemCount => Stack?.Count ?? 0;
        
        /// <summary>Number of notifications</summary>
        [JsonIgnore]
        public int NotificationCount => Notifications?.Count ?? 0;
        
        #endregion
        
        #region Gas Methods
        
        /// <summary>
        /// Gets the gas consumed as a decimal.
        /// </summary>
        /// <returns>Gas consumed as decimal</returns>
        public decimal GetGasConsumedDecimal()
        {
            if (string.IsNullOrEmpty(GasConsumed))
                return 0;
            
            if (decimal.TryParse(GasConsumed, out var gas))
                return gas;
            
            return 0;
        }
        
        /// <summary>
        /// Gets the gas consumed in fractions (smallest unit).
        /// </summary>
        /// <returns>Gas consumed in fractions</returns>
        public long GetGasConsumedFractions()
        {
            var gasDecimal = GetGasConsumedDecimal();
            return (long)(gasDecimal * 100_000_000); // Convert to GAS fractions
        }
        
        #endregion
        
        #region Stack Operations
        
        /// <summary>
        /// Gets the first stack item.
        /// </summary>
        /// <returns>First stack item or null</returns>
        public StackItem GetFirstStackItem()
        {
            return HasStackItems ? Stack[0] : null;
        }
        
        /// <summary>
        /// Tries to get the first stack item.
        /// </summary>
        /// <param name="stackItem">The first stack item if available</param>
        /// <returns>True if successful, false if no stack items</returns>
        public bool TryGetFirstStackItem(out StackItem stackItem)
        {
            if (HasStackItems)
            {
                stackItem = Stack[0];
                return true;
            }
            
            stackItem = null;
            return false;
        }
        
        /// <summary>
        /// Gets a stack item at the specified index.
        /// </summary>
        /// <param name="index">Stack index</param>
        /// <returns>Stack item at index</returns>
        /// <exception cref="ArgumentOutOfRangeException">If index is out of range</exception>
        public StackItem GetStackItem(int index)
        {
            if (!HasStackItems || index < 0 || index >= Stack.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(index), $"Stack index {index} is out of range. Stack has {StackItemCount} items.");
            }
            
            return Stack[index];
        }
        
        #endregion
        
        #region Validation Methods
        
        /// <summary>
        /// Validates this execution.
        /// </summary>
        /// <exception cref="InvalidOperationException">If validation fails</exception>
        public void Validate()
        {
            if (string.IsNullOrEmpty(Trigger))
                throw new InvalidOperationException("Execution trigger cannot be null or empty.");
            
            if (string.IsNullOrEmpty(GasConsumed))
                throw new InvalidOperationException("Execution gas consumed cannot be null or empty.");
            
            // Validate gas consumed is a valid number
            if (!decimal.TryParse(GasConsumed, out _))
                throw new InvalidOperationException($"Execution gas consumed is not a valid number: {GasConsumed}");
            
            if (Notifications != null)
            {
                foreach (var notification in Notifications)
                {
                    notification.Validate();
                }
            }
        }
        
        /// <summary>
        /// Throws an exception if this execution faulted.
        /// </summary>
        /// <exception cref="ExecutionFaultException">If the execution faulted</exception>
        public void ThrowIfFaulted()
        {
            if (HasFaulted)
            {
                throw new ExecutionFaultException($"Execution faulted with state: {State}. Exception: {Exception ?? "Unknown"}");
            }
        }
        
        #endregion
        
        #region String Representation
        
        /// <summary>
        /// Returns a detailed string representation of this execution.
        /// </summary>
        /// <returns>Detailed string representation</returns>
        public string ToDetailedString()
        {
            var result = $"Execution ({Trigger}):\n";
            result += $"  State: {State}\n";
            result += $"  Gas Consumed: {GasConsumed}\n";
            result += $"  Stack Items: {StackItemCount}\n";
            result += $"  Notifications: {NotificationCount}\n";
            
            if (HasException)
            {
                result += $"  Exception: {Exception}\n";
            }
            
            if (HasNotifications)
            {
                result += $"  Events: {string.Join(", ", Notifications.Select(n => n.EventName).Distinct())}\n";
            }
            
            return result;
        }
        
        /// <summary>
        /// Returns a string representation of this execution.
        /// </summary>
        /// <returns>String representation</returns>
        public override string ToString()
        {
            var status = IsSuccessful ? "Success" : "Fault";
            return $"Execution({Trigger}, {State}, Gas: {GasConsumed}, {status})";
        }
        
        #endregion
    }
    
    /// <summary>
    /// Contains statistical information about an application log.
    /// </summary>
    [System.Serializable]
    public class ApplicationLogStatistics
    {
        /// <summary>Transaction ID</summary>
        public string TransactionId { get; set; }
        
        /// <summary>Number of executions</summary>
        public int ExecutionCount { get; set; }
        
        /// <summary>Number of successful executions</summary>
        public int SuccessfulExecutions { get; set; }
        
        /// <summary>Number of faulted executions</summary>
        public int FaultedExecutions { get; set; }
        
        /// <summary>Total number of notifications</summary>
        public int TotalNotifications { get; set; }
        
        /// <summary>Total gas consumed</summary>
        public decimal TotalGasConsumed { get; set; }
        
        /// <summary>Gas consumed by successful executions</summary>
        public decimal SuccessfulGasConsumed { get; set; }
        
        /// <summary>Gas wasted by faulted executions</summary>
        public decimal WastedGas { get; set; }
        
        /// <summary>Number of unique contracts that emitted notifications</summary>
        public int UniqueContracts { get; set; }
        
        /// <summary>Number of unique event names</summary>
        public int UniqueEvents { get; set; }
        
        /// <summary>Whether all executions were successful</summary>
        public bool IsSuccessful { get; set; }
        
        /// <summary>Success rate as a percentage</summary>
        [JsonIgnore]
        public double SuccessRate => ExecutionCount > 0 ? (double)SuccessfulExecutions / ExecutionCount * 100 : 0;
        
        /// <summary>
        /// Returns a string representation of these statistics.
        /// </summary>
        /// <returns>String representation</returns>
        public override string ToString()
        {
            var status = IsSuccessful ? "Success" : "Mixed";
            return $"ApplicationLogStatistics({ExecutionCount} exec, {SuccessRate:F1}% success, Gas: {TotalGasConsumed}, {status})";
        }
    }
    
    /// <summary>
    /// Exception thrown when contract execution fails.
    /// </summary>
    public class ExecutionFaultException : Exception
    {
        /// <summary>
        /// Creates a new execution fault exception.
        /// </summary>
        /// <param name="message">The exception message</param>
        public ExecutionFaultException(string message) : base(message)
        {
        }
        
        /// <summary>
        /// Creates a new execution fault exception with an inner exception.
        /// </summary>
        /// <param name="message">The exception message</param>
        /// <param name="innerException">The inner exception</param>
        public ExecutionFaultException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
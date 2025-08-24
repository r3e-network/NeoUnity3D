using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Neo.Unity.SDK.Types;
using Newtonsoft.Json;

namespace Neo.Unity.SDK.Protocol.Response
{
    /// <summary>
    /// Represents the result of a smart contract invocation on the Neo blockchain.
    /// Contains execution state, gas consumption, stack results, and diagnostic information.
    /// </summary>
    [System.Serializable]
    public class InvocationResult
    {
        #region Properties
        
        /// <summary>The script that was executed</summary>
        [JsonProperty("script")]
        public string Script { get; set; }
        
        /// <summary>The execution state of the VM</summary>
        [JsonProperty("state")]
        public NeoVMStateType State { get; set; }
        
        /// <summary>The amount of GAS consumed during execution</summary>
        [JsonProperty("gasconsumed")]
        public string GasConsumed { get; set; }
        
        /// <summary>Exception message if the execution faulted</summary>
        [JsonProperty("exception")]
        public string Exception { get; set; }
        
        /// <summary>Notifications emitted during execution</summary>
        [JsonProperty("notifications")]
        public List<Notification> Notifications { get; set; }
        
        /// <summary>Diagnostic information about the execution</summary>
        [JsonProperty("diagnostics")]
        public Diagnostics Diagnostics { get; set; }
        
        /// <summary>The VM stack after execution</summary>
        [JsonProperty("stack")]
        public List<StackItem> Stack { get; set; }
        
        /// <summary>Transaction hash if this was a transaction</summary>
        [JsonProperty("tx")]
        public string TransactionHash { get; set; }
        
        /// <summary>Pending signature information for multi-sig transactions</summary>
        [JsonProperty("pendingsignature")]
        public PendingSignature PendingSignature { get; set; }
        
        /// <summary>Session ID for iterator operations</summary>
        [JsonProperty("session")]
        public string SessionId { get; set; }
        
        #endregion
        
        #region Computed Properties
        
        /// <summary>Whether the execution resulted in a fault state</summary>
        [JsonIgnore]
        public bool HasStateFault => State == NeoVMStateType.Fault;
        
        /// <summary>Whether the execution completed successfully</summary>
        [JsonIgnore]
        public bool IsSuccess => State == NeoVMStateType.Halt;
        
        /// <summary>Whether there are notifications from the execution</summary>
        [JsonIgnore]
        public bool HasNotifications => Notifications != null && Notifications.Count > 0;
        
        /// <summary>Whether there are items on the stack</summary>
        [JsonIgnore]
        public bool HasStackItems => Stack != null && Stack.Count > 0;
        
        /// <summary>Whether this result has a session for iterator operations</summary>
        [JsonIgnore]
        public bool HasSession => !string.IsNullOrEmpty(SessionId);
        
        #endregion
        
        #region Constructors
        
        /// <summary>
        /// Creates a new invocation result.
        /// </summary>
        /// <param name="script">The executed script</param>
        /// <param name="state">The VM state</param>
        /// <param name="gasConsumed">Gas consumed</param>
        /// <param name="exception">Exception message</param>
        /// <param name="notifications">Notifications</param>
        /// <param name="diagnostics">Diagnostics</param>
        /// <param name="stack">VM stack</param>
        /// <param name="transactionHash">Transaction hash</param>
        /// <param name="pendingSignature">Pending signature</param>
        /// <param name="sessionId">Session ID</param>
        public InvocationResult(string script, NeoVMStateType state, string gasConsumed, string exception,
                               List<Notification> notifications, Diagnostics diagnostics, List<StackItem> stack,
                               string transactionHash, PendingSignature pendingSignature, string sessionId)
        {
            Script = script;
            State = state;
            GasConsumed = gasConsumed;
            Exception = exception;
            Notifications = notifications ?? new List<Notification>();
            Diagnostics = diagnostics;
            Stack = stack ?? new List<StackItem>();
            TransactionHash = transactionHash;
            PendingSignature = pendingSignature;
            SessionId = sessionId;
        }
        
        /// <summary>
        /// Default constructor for JSON deserialization.
        /// </summary>
        public InvocationResult()
        {
            Notifications = new List<Notification>();
            Stack = new List<StackItem>();
        }
        
        #endregion
        
        #region Stack Operations
        
        /// <summary>
        /// Gets the first item from the execution stack.
        /// </summary>
        /// <returns>The first stack item</returns>
        /// <exception cref="InvalidOperationException">If the stack is empty</exception>
        public StackItem GetFirstStackItem()
        {
            if (Stack == null || Stack.Count == 0)
            {
                throw new InvalidOperationException("The stack is empty. This means that no items were left on the NeoVM stack after this invocation.");
            }
            
            return Stack[0];
        }
        
        /// <summary>
        /// Gets the stack item at the specified index.
        /// </summary>
        /// <param name="index">The stack index</param>
        /// <returns>The stack item at the specified index</returns>
        /// <exception cref="ArgumentOutOfRangeException">If index is out of range</exception>
        public StackItem GetStackItem(int index)
        {
            if (Stack == null || index < 0 || index >= Stack.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(index), $"Stack index {index} is out of range. Stack has {Stack?.Count ?? 0} items.");
            }
            
            return Stack[index];
        }
        
        /// <summary>
        /// Tries to get the first stack item.
        /// </summary>
        /// <param name="stackItem">The first stack item if available</param>
        /// <returns>True if successful, false if stack is empty</returns>
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
        
        #endregion
        
        #region Validation Methods
        
        /// <summary>
        /// Throws an exception if the invocation resulted in a fault state.
        /// </summary>
        /// <exception cref="ContractInvocationException">If the execution faulted</exception>
        public void ThrowIfFault()
        {
            if (HasStateFault)
            {
                throw new ContractInvocationException($"Contract invocation faulted: {Exception ?? "Unknown error"}");
            }
        }
        
        /// <summary>
        /// Validates that the execution was successful and has the expected number of stack items.
        /// </summary>
        /// <param name="expectedStackItems">Expected number of stack items</param>
        /// <exception cref="ContractInvocationException">If validation fails</exception>
        public void ValidateSuccess(int expectedStackItems = 1)
        {
            ThrowIfFault();
            
            if (Stack == null || Stack.Count < expectedStackItems)
            {
                throw new ContractInvocationException($"Expected at least {expectedStackItems} stack items, but got {Stack?.Count ?? 0}");
            }
        }
        
        #endregion
        
        #region Convenience Methods
        
        /// <summary>
        /// Gets the consumed GAS as a decimal value.
        /// </summary>
        /// <returns>The GAS consumed as decimal</returns>
        public decimal GetGasConsumedDecimal()
        {
            if (string.IsNullOrEmpty(GasConsumed))
                return 0;
            
            if (decimal.TryParse(GasConsumed, out var gas))
                return gas;
            
            return 0;
        }
        
        /// <summary>
        /// Gets the consumed GAS as integer fractions.
        /// </summary>
        /// <returns>The GAS consumed in fractions</returns>
        public long GetGasConsumedFractions()
        {
            var gasDecimal = GetGasConsumedDecimal();
            return (long)(gasDecimal * 100_000_000); // Convert to GAS fractions
        }
        
        /// <summary>
        /// Gets all notifications of a specific contract.
        /// </summary>
        /// <param name="contractHash">The contract hash to filter by</param>
        /// <returns>Notifications from the specified contract</returns>
        public List<Notification> GetNotificationsFromContract(Hash160 contractHash)
        {
            if (contractHash == null || !HasNotifications)
                return new List<Notification>();
            
            return Notifications.Where(n => n.Contract?.Equals(contractHash) == true).ToList();
        }
        
        /// <summary>
        /// Gets the first notification with the specified event name.
        /// </summary>
        /// <param name="eventName">The event name to search for</param>
        /// <returns>The first matching notification or null</returns>
        public Notification GetNotificationByEvent(string eventName)
        {
            if (string.IsNullOrEmpty(eventName) || !HasNotifications)
                return null;
            
            return Notifications.FirstOrDefault(n => n.EventName == eventName);
        }
        
        #endregion
        
        #region String Representation
        
        /// <summary>
        /// Returns a detailed string representation of this invocation result.
        /// </summary>
        /// <returns>Detailed string representation</returns>
        public string ToDetailedString()
        {
            var result = $"InvocationResult:\n";
            result += $"  State: {State}\n";
            result += $"  Gas Consumed: {GasConsumed}\n";
            result += $"  Stack Items: {Stack?.Count ?? 0}\n";
            result += $"  Notifications: {Notifications?.Count ?? 0}\n";
            
            if (!string.IsNullOrEmpty(Exception))
            {
                result += $"  Exception: {Exception}\n";
            }
            
            if (!string.IsNullOrEmpty(SessionId))
            {
                result += $"  Session ID: {SessionId}\n";
            }
            
            if (HasStackItems)
            {
                result += "  Stack:\n";
                for (int i = 0; i < Math.Min(Stack.Count, 5); i++) // Show first 5 items
                {
                    result += $"    [{i}]: {Stack[i]}\n";
                }
                
                if (Stack.Count > 5)
                {
                    result += $"    ... and {Stack.Count - 5} more items\n";
                }
            }
            
            return result;
        }
        
        /// <summary>
        /// Returns a string representation of this invocation result.
        /// </summary>
        /// <returns>String representation</returns>
        public override string ToString()
        {
            return $"InvocationResult(State: {State}, Gas: {GasConsumed}, Stack: {Stack?.Count ?? 0}, Notifications: {Notifications?.Count ?? 0})";
        }
        
        #endregion
    }
    
    /// <summary>
    /// Exception thrown when contract invocation fails or produces unexpected results.
    /// </summary>
    public class ContractInvocationException : Exception
    {
        /// <summary>
        /// Creates a new contract invocation exception.
        /// </summary>
        /// <param name="message">The exception message</param>
        public ContractInvocationException(string message) : base(message)
        {
        }
        
        /// <summary>
        /// Creates a new contract invocation exception with an inner exception.
        /// </summary>
        /// <param name="message">The exception message</param>
        /// <param name="innerException">The inner exception</param>
        public ContractInvocationException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
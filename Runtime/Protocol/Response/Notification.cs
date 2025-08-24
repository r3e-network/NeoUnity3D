using System;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using Neo.Unity.SDK.Types;

namespace Neo.Unity.SDK.Protocol.Response
{
    /// <summary>
    /// Represents a notification emitted by a smart contract during execution.
    /// Contains contract hash, event name, and state data.
    /// </summary>
    [System.Serializable]
    public class Notification
    {
        #region Properties
        
        /// <summary>The hash of the contract that emitted this notification</summary>
        [JsonProperty("contract")]
        public Hash160 Contract { get; set; }
        
        /// <summary>The name of the event that was fired</summary>
        [JsonProperty("eventname")]
        public string EventName { get; set; }
        
        /// <summary>The state data associated with the notification</summary>
        [JsonProperty("state")]
        public List<StackItem> State { get; set; }
        
        #endregion
        
        #region Constructors
        
        /// <summary>
        /// Default constructor for JSON deserialization.
        /// </summary>
        public Notification()
        {
            State = new List<StackItem>();
        }
        
        /// <summary>
        /// Creates a new notification.
        /// </summary>
        /// <param name="contract">The contract hash</param>
        /// <param name="eventName">The event name</param>
        /// <param name="state">The state data</param>
        public Notification(Hash160 contract, string eventName, List<StackItem> state = null)
        {
            Contract = contract;
            EventName = eventName;
            State = state ?? new List<StackItem>();
        }
        
        #endregion
        
        #region Methods
        
        /// <summary>
        /// Gets the state item at the specified index.
        /// </summary>
        /// <param name="index">The index</param>
        /// <returns>The state item</returns>
        /// <exception cref="ArgumentOutOfRangeException">If index is out of range</exception>
        public StackItem GetStateItem(int index)
        {
            if (State == null || index < 0 || index >= State.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(index), $"State index {index} is out of range. State has {State?.Count ?? 0} items.");
            }
            
            return State[index];
        }
        
        /// <summary>
        /// Tries to get the state item at the specified index.
        /// </summary>
        /// <param name="index">The index</param>
        /// <param name="stateItem">The state item if found</param>
        /// <returns>True if successful, false if index is out of range</returns>
        public bool TryGetStateItem(int index, out StackItem stateItem)
        {
            if (State != null && index >= 0 && index < State.Count)
            {
                stateItem = State[index];
                return true;
            }
            
            stateItem = null;
            return false;
        }
        
        /// <summary>
        /// Gets the first state item.
        /// </summary>
        /// <returns>The first state item</returns>
        /// <exception cref="InvalidOperationException">If there are no state items</exception>
        public StackItem GetFirstStateItem()
        {
            if (State == null || State.Count == 0)
            {
                throw new InvalidOperationException("No state items available in this notification.");
            }
            
            return State[0];
        }
        
        /// <summary>
        /// Tries to get the first state item.
        /// </summary>
        /// <param name="stateItem">The first state item if found</param>
        /// <returns>True if successful, false if no state items</returns>
        public bool TryGetFirstStateItem(out StackItem stateItem)
        {
            if (State != null && State.Count > 0)
            {
                stateItem = State[0];
                return true;
            }
            
            stateItem = null;
            return false;
        }
        
        /// <summary>
        /// Whether this notification has any state items.
        /// </summary>
        public bool HasState => State != null && State.Count > 0;
        
        /// <summary>
        /// Number of state items in this notification.
        /// </summary>
        public int StateCount => State?.Count ?? 0;
        
        #endregion
        
        #region String Representation
        
        /// <summary>
        /// Returns a string representation of this notification.
        /// </summary>
        /// <returns>String representation</returns>
        public override string ToString()
        {
            return $"Notification(Contract: {Contract}, Event: {EventName}, State: {StateCount} items)";
        }
        
        #endregion
    }
}
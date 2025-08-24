using System;
using Newtonsoft.Json;
using UnityEngine;

namespace NeoUnity.Protocol.Response
{
    /// <summary>
    /// Represents an Express shutdown response containing process ID
    /// </summary>
    [Serializable]
    public struct ExpressShutdown : IEquatable<ExpressShutdown>
    {
        [JsonProperty("process-id")]
        [SerializeField] private int _processId;
        
        /// <summary>
        /// Gets the process ID
        /// </summary>
        public int ProcessId => _processId;
        
        /// <summary>
        /// Initializes a new instance of ExpressShutdown
        /// </summary>
        /// <param name="processId">The process ID</param>
        [JsonConstructor]
        public ExpressShutdown(int processId)
        {
            _processId = processId;
        }
        
        public bool Equals(ExpressShutdown other)
        {
            return _processId == other._processId;
        }
        
        public override bool Equals(object obj)
        {
            return obj is ExpressShutdown other && Equals(other);
        }
        
        public override int GetHashCode()
        {
            return _processId;
        }
        
        public static bool operator ==(ExpressShutdown left, ExpressShutdown right)
        {
            return left.Equals(right);
        }
        
        public static bool operator !=(ExpressShutdown left, ExpressShutdown right)
        {
            return !left.Equals(right);
        }
        
        public override string ToString()
        {
            return $"ExpressShutdown(ProcessId: {_processId})";
        }
    }
}
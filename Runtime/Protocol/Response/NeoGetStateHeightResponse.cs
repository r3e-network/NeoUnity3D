using System;
using UnityEngine;
using Newtonsoft.Json;

namespace Neo.Unity.SDK.Protocol.Response
{
    /// <summary>
    /// Response for the getstateheight RPC call.
    /// Returns the current state height information including local and validated root indices.
    /// </summary>
    [System.Serializable]
    public class NeoGetStateHeightResponse : NeoResponse<NeoGetStateHeightResponse.StateHeight>
    {
        /// <summary>
        /// Gets the state height information from the response.
        /// </summary>
        public StateHeight StateHeight => Result;

        /// <summary>
        /// Default constructor for JSON deserialization.
        /// </summary>
        public NeoGetStateHeightResponse() : base()
        {
        }

        /// <summary>
        /// Creates a successful state height response.
        /// </summary>
        /// <param name="stateHeight">The state height information</param>
        /// <param name="id">The request ID</param>
        public NeoGetStateHeightResponse(StateHeight stateHeight, int id = 1) : base(stateHeight, id)
        {
        }

        /// <summary>
        /// Creates an error state height response.
        /// </summary>
        /// <param name="error">The error information</param>
        /// <param name="id">The request ID</param>
        public NeoGetStateHeightResponse(ResponseError error, int id = 1) : base(error, id)
        {
        }

        /// <summary>
        /// Contains state height information including local and validated root indices.
        /// </summary>
        [System.Serializable]
        public class StateHeight
        {
            /// <summary>The index of the local state root</summary>
            [JsonProperty("localrootindex")]
            public int LocalRootIndex { get; set; }

            /// <summary>The index of the validated state root</summary>
            [JsonProperty("validatedrootindex")]
            public int ValidatedRootIndex { get; set; }

            /// <summary>
            /// Default constructor for JSON deserialization.
            /// </summary>
            public StateHeight()
            {
            }

            /// <summary>
            /// Creates new state height information.
            /// </summary>
            /// <param name="localRootIndex">The local root index</param>
            /// <param name="validatedRootIndex">The validated root index</param>
            public StateHeight(int localRootIndex, int validatedRootIndex)
            {
                LocalRootIndex = localRootIndex;
                ValidatedRootIndex = validatedRootIndex;
            }

            /// <summary>
            /// Gets the difference between local and validated root indices.
            /// This indicates how many blocks behind the validated state is from the local state.
            /// </summary>
            [JsonIgnore]
            public int ValidationLag => Math.Max(0, LocalRootIndex - ValidatedRootIndex);

            /// <summary>
            /// Checks if the local and validated states are synchronized.
            /// </summary>
            [JsonIgnore]
            public bool IsSynchronized => LocalRootIndex == ValidatedRootIndex;

            /// <summary>
            /// Checks if there is a significant validation lag (more than 1 block).
            /// </summary>
            [JsonIgnore]
            public bool HasSignificantLag => ValidationLag > 1;

            /// <summary>
            /// Gets the validation progress as a percentage (0-100).
            /// Returns 100 if synchronized or if local index is 0.
            /// </summary>
            [JsonIgnore]
            public double ValidationProgressPercent
            {
                get
                {
                    if (LocalRootIndex <= 0)
                        return 100.0;

                    if (ValidatedRootIndex < 0)
                        return 0.0;

                    return Math.Min(100.0, (double)ValidatedRootIndex / LocalRootIndex * 100.0);
                }
            }

            /// <summary>
            /// Gets the validation status as a descriptive string.
            /// </summary>
            [JsonIgnore]
            public string ValidationStatus
            {
                get
                {
                    if (IsSynchronized)
                        return "Synchronized";

                    var lag = ValidationLag;
                    if (lag == 1)
                        return "1 block behind";
                    
                    return $"{lag} blocks behind";
                }
            }

            /// <summary>
            /// Checks if the state height values are valid (non-negative).
            /// </summary>
            [JsonIgnore]
            public bool IsValid => LocalRootIndex >= 0 && ValidatedRootIndex >= 0;

            /// <summary>
            /// Gets the synchronization ratio (validated/local).
            /// Returns 1.0 if synchronized or if local index is 0.
            /// </summary>
            [JsonIgnore]
            public double SynchronizationRatio
            {
                get
                {
                    if (LocalRootIndex <= 0)
                        return 1.0;

                    if (ValidatedRootIndex < 0)
                        return 0.0;

                    return Math.Min(1.0, (double)ValidatedRootIndex / LocalRootIndex);
                }
            }

            /// <summary>
            /// Returns a string representation of the state height.
            /// </summary>
            /// <returns>String representation</returns>
            public override string ToString()
            {
                return $"StateHeight(Local: {LocalRootIndex}, Validated: {ValidatedRootIndex}, Status: {ValidationStatus})";
            }

            /// <summary>
            /// Gets a detailed description of the state height information.
            /// </summary>
            /// <returns>Detailed description</returns>
            public string GetDetailedInfo()
            {
                return $"State Height Information:\n" +
                       $"  Local Root Index: {LocalRootIndex}\n" +
                       $"  Validated Root Index: {ValidatedRootIndex}\n" +
                       $"  Validation Lag: {ValidationLag} blocks\n" +
                       $"  Validation Progress: {ValidationProgressPercent:F2}%\n" +
                       $"  Status: {ValidationStatus}\n" +
                       $"  Is Synchronized: {IsSynchronized}";
            }

            /// <summary>
            /// Validates the state height information.
            /// </summary>
            /// <exception cref="ArgumentException">If the state height information is invalid</exception>
            public void Validate()
            {
                if (LocalRootIndex < 0)
                    throw new ArgumentException("Local root index cannot be negative");

                if (ValidatedRootIndex < 0)
                    throw new ArgumentException("Validated root index cannot be negative");

                if (ValidatedRootIndex > LocalRootIndex)
                    throw new ArgumentException("Validated root index cannot be greater than local root index");
            }

            /// <summary>
            /// Compares this state height with another.
            /// </summary>
            /// <param name="other">The other state height to compare with</param>
            /// <returns>Comparison result based on local root index</returns>
            public int CompareTo(StateHeight other)
            {
                if (other == null)
                    return 1;

                var localComparison = LocalRootIndex.CompareTo(other.LocalRootIndex);
                if (localComparison != 0)
                    return localComparison;

                return ValidatedRootIndex.CompareTo(other.ValidatedRootIndex);
            }

            /// <summary>
            /// Checks if this state height is equal to another.
            /// </summary>
            /// <param name="other">The other state height to compare with</param>
            /// <returns>True if both indices are equal</returns>
            public bool Equals(StateHeight other)
            {
                if (other == null)
                    return false;

                return LocalRootIndex == other.LocalRootIndex && 
                       ValidatedRootIndex == other.ValidatedRootIndex;
            }

            /// <summary>
            /// Gets the hash code based on both indices.
            /// </summary>
            /// <returns>Hash code</returns>
            public override int GetHashCode()
            {
                return LocalRootIndex.GetHashCode() ^ (ValidatedRootIndex.GetHashCode() << 1);
            }

            /// <summary>
            /// Creates a copy of this state height with updated indices.
            /// </summary>
            /// <param name="newLocalIndex">The new local root index</param>
            /// <param name="newValidatedIndex">The new validated root index</param>
            /// <returns>New StateHeight instance</returns>
            public StateHeight WithIndices(int newLocalIndex, int newValidatedIndex)
            {
                return new StateHeight(newLocalIndex, newValidatedIndex);
            }

            /// <summary>
            /// Creates a copy of this state height with updated local index.
            /// </summary>
            /// <param name="newLocalIndex">The new local root index</param>
            /// <returns>New StateHeight instance</returns>
            public StateHeight WithLocalIndex(int newLocalIndex)
            {
                return new StateHeight(newLocalIndex, ValidatedRootIndex);
            }

            /// <summary>
            /// Creates a copy of this state height with updated validated index.
            /// </summary>
            /// <param name="newValidatedIndex">The new validated root index</param>
            /// <returns>New StateHeight instance</returns>
            public StateHeight WithValidatedIndex(int newValidatedIndex)
            {
                return new StateHeight(LocalRootIndex, newValidatedIndex);
            }
        }
    }
}
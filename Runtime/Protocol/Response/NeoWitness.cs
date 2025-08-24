using System;
using UnityEngine;
using Newtonsoft.Json;

namespace Neo.Unity.SDK.Protocol.Response
{
    /// <summary>
    /// Represents a witness in a Neo transaction.
    /// Contains invocation and verification scripts for transaction authorization.
    /// </summary>
    [System.Serializable]
    public class NeoWitness
    {
        #region Properties
        
        /// <summary>The invocation script (parameters for verification script)</summary>
        [JsonProperty("invocation")]
        public string Invocation { get; set; }
        
        /// <summary>The verification script (contract script for signature validation)</summary>
        [JsonProperty("verification")]
        public string Verification { get; set; }
        
        #endregion
        
        #region Constructors
        
        /// <summary>
        /// Default constructor for JSON deserialization.
        /// </summary>
        public NeoWitness()
        {
        }
        
        /// <summary>
        /// Creates a new witness.
        /// </summary>
        /// <param name="invocation">The invocation script</param>
        /// <param name="verification">The verification script</param>
        public NeoWitness(string invocation, string verification)
        {
            Invocation = invocation;
            Verification = verification;
        }
        
        #endregion
        
        #region Properties (Computed)
        
        /// <summary>Whether this witness has an invocation script</summary>
        [JsonIgnore]
        public bool HasInvocation => !string.IsNullOrEmpty(Invocation);
        
        /// <summary>Whether this witness has a verification script</summary>
        [JsonIgnore]
        public bool HasVerification => !string.IsNullOrEmpty(Verification);
        
        /// <summary>Whether this witness is complete (has both scripts)</summary>
        [JsonIgnore]
        public bool IsComplete => HasInvocation && HasVerification;
        
        /// <summary>Whether this appears to be an empty witness</summary>
        [JsonIgnore]
        public bool IsEmpty => string.IsNullOrEmpty(Invocation) && string.IsNullOrEmpty(Verification);
        
        #endregion
        
        #region Methods
        
        /// <summary>
        /// Gets the invocation script as decoded bytes.
        /// </summary>
        /// <returns>The invocation script bytes</returns>
        /// <exception cref="FormatException">If the script is not valid base64</exception>
        public byte[] GetInvocationBytes()
        {
            if (string.IsNullOrEmpty(Invocation))
                return new byte[0];
            
            try
            {
                return Convert.FromBase64String(Invocation);
            }
            catch (FormatException ex)
            {
                throw new FormatException($"Invalid base64 invocation script: {Invocation}", ex);
            }
        }
        
        /// <summary>
        /// Gets the verification script as decoded bytes.
        /// </summary>
        /// <returns>The verification script bytes</returns>
        /// <exception cref="FormatException">If the script is not valid base64</exception>
        public byte[] GetVerificationBytes()
        {
            if (string.IsNullOrEmpty(Verification))
                return new byte[0];
            
            try
            {
                return Convert.FromBase64String(Verification);
            }
            catch (FormatException ex)
            {
                throw new FormatException($"Invalid base64 verification script: {Verification}", ex);
            }
        }
        
        /// <summary>
        /// Tries to get the invocation script as decoded bytes.
        /// </summary>
        /// <param name="bytes">The invocation script bytes if successful</param>
        /// <returns>True if successful, false if invalid base64</returns>
        public bool TryGetInvocationBytes(out byte[] bytes)
        {
            try
            {
                bytes = GetInvocationBytes();
                return true;
            }
            catch
            {
                bytes = null;
                return false;
            }
        }
        
        /// <summary>
        /// Tries to get the verification script as decoded bytes.
        /// </summary>
        /// <param name="bytes">The verification script bytes if successful</param>
        /// <returns>True if successful, false if invalid base64</returns>
        public bool TryGetVerificationBytes(out byte[] bytes)
        {
            try
            {
                bytes = GetVerificationBytes();
                return true;
            }
            catch
            {
                bytes = null;
                return false;
            }
        }
        
        /// <summary>
        /// Validates that this witness has the required data.
        /// </summary>
        /// <param name="requireInvocation">Whether invocation script is required</param>
        /// <param name="requireVerification">Whether verification script is required</param>
        /// <exception cref="InvalidOperationException">If validation fails</exception>
        public void Validate(bool requireInvocation = true, bool requireVerification = true)
        {
            if (requireInvocation && !HasInvocation)
            {
                throw new InvalidOperationException("Witness is missing required invocation script.");
            }
            
            if (requireVerification && !HasVerification)
            {
                throw new InvalidOperationException("Witness is missing required verification script.");
            }
            
            // Validate base64 encoding
            if (HasInvocation)
            {
                try
                {
                    Convert.FromBase64String(Invocation);
                }
                catch (FormatException ex)
                {
                    throw new InvalidOperationException("Invocation script is not valid base64.", ex);
                }
            }
            
            if (HasVerification)
            {
                try
                {
                    Convert.FromBase64String(Verification);
                }
                catch (FormatException ex)
                {
                    throw new InvalidOperationException("Verification script is not valid base64.", ex);
                }
            }
        }
        
        #endregion
        
        #region Equality
        
        /// <summary>
        /// Determines whether the specified object is equal to the current witness.
        /// </summary>
        /// <param name="obj">The object to compare</param>
        /// <returns>True if equal, false otherwise</returns>
        public override bool Equals(object obj)
        {
            if (obj is NeoWitness other)
            {
                return Invocation == other.Invocation && Verification == other.Verification;
            }
            return false;
        }
        
        /// <summary>
        /// Returns a hash code for the current witness.
        /// </summary>
        /// <returns>Hash code</returns>
        public override int GetHashCode()
        {
            return HashCode.Combine(Invocation, Verification);
        }
        
        #endregion
        
        #region String Representation
        
        /// <summary>
        /// Returns a string representation of this witness.
        /// </summary>
        /// <returns>String representation</returns>
        public override string ToString()
        {
            var invLen = HasInvocation ? Invocation.Length : 0;
            var verLen = HasVerification ? Verification.Length : 0;
            return $"NeoWitness(Invocation: {invLen} chars, Verification: {verLen} chars)";
        }
        
        #endregion
    }
}
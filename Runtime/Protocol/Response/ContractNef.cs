using System;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

namespace Neo.Unity.SDK.Protocol.Response
{
    /// <summary>
    /// Represents a Neo Executable Format (NEF) file containing compiled smart contract bytecode.
    /// NEF files contain the executable code, metadata, and method tokens for Neo smart contracts.
    /// </summary>
    [System.Serializable]
    public class ContractNef
    {
        #region Properties
        
        /// <summary>The NEF magic number for format identification</summary>
        [JsonProperty("magic")]
        public long Magic { get; set; }
        
        /// <summary>The compiler used to create this NEF file</summary>
        [JsonProperty("compiler")]
        public string Compiler { get; set; }
        
        /// <summary>The source file or URL (optional)</summary>
        [JsonProperty("source")]
        public string Source { get; set; }
        
        /// <summary>List of method tokens used by this contract</summary>
        [JsonProperty("tokens")]
        public List<ContractMethodToken> Tokens { get; set; }
        
        /// <summary>The compiled script/bytecode (base64 encoded)</summary>
        [JsonProperty("script")]
        public string Script { get; set; }
        
        /// <summary>The checksum for integrity verification</summary>
        [JsonProperty("checksum")]
        public long Checksum { get; set; }
        
        #endregion
        
        #region Constants
        
        /// <summary>The expected NEF magic number</summary>
        public const long NEF_MAGIC = 0x3346454E; // "NEF3" in little-endian
        
        #endregion
        
        #region Constructors
        
        /// <summary>
        /// Default constructor for JSON deserialization.
        /// </summary>
        public ContractNef()
        {
            Tokens = new List<ContractMethodToken>();
        }
        
        /// <summary>
        /// Creates a new contract NEF.
        /// </summary>
        /// <param name="magic">NEF magic number</param>
        /// <param name="compiler">Compiler information</param>
        /// <param name="source">Source information</param>
        /// <param name="tokens">Method tokens</param>
        /// <param name="script">Compiled script</param>
        /// <param name="checksum">File checksum</param>
        public ContractNef(long magic, string compiler, string source, List<ContractMethodToken> tokens, string script, long checksum)
        {
            Magic = magic;
            Compiler = compiler;
            Source = source;
            Tokens = tokens ?? new List<ContractMethodToken>();
            Script = script;
            Checksum = checksum;
        }
        
        #endregion
        
        #region Properties (Computed)
        
        /// <summary>Whether this NEF has a valid magic number</summary>
        [JsonIgnore]
        public bool HasValidMagic => Magic == NEF_MAGIC;
        
        /// <summary>Whether this NEF has compiler information</summary>
        [JsonIgnore]
        public bool HasCompiler => !string.IsNullOrEmpty(Compiler);
        
        /// <summary>Whether this NEF has source information</summary>
        [JsonIgnore]
        public bool HasSource => !string.IsNullOrEmpty(Source);
        
        /// <summary>Whether this NEF has method tokens</summary>
        [JsonIgnore]
        public bool HasTokens => Tokens != null && Tokens.Count > 0;
        
        /// <summary>Whether this NEF has script data</summary>
        [JsonIgnore]
        public bool HasScript => !string.IsNullOrEmpty(Script);
        
        /// <summary>Number of method tokens</summary>
        [JsonIgnore]
        public int TokenCount => Tokens?.Count ?? 0;
        
        /// <summary>Script size in bytes (estimated from base64)</summary>
        [JsonIgnore]
        public int ScriptSize => HasScript ? GetScriptBytes().Length : 0;
        
        #endregion
        
        #region Script Methods
        
        /// <summary>
        /// Gets the script as decoded bytes.
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
                throw new FormatException($"Invalid base64 NEF script: {Script}", ex);
            }
        }
        
        /// <summary>
        /// Tries to get the script as decoded bytes.
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
        
        /// <summary>
        /// Gets the script as a hex string.
        /// </summary>
        /// <returns>Script as hex string</returns>
        public string GetScriptHex()
        {
            var bytes = GetScriptBytes();
            return bytes.Length > 0 ? Convert.ToHexString(bytes).ToLower() : string.Empty;
        }
        
        #endregion
        
        #region Token Methods
        
        /// <summary>
        /// Gets a method token by hash and method.
        /// </summary>
        /// <param name="hash">The contract hash</param>
        /// <param name="method">The method name</param>
        /// <returns>The method token or null if not found</returns>
        public ContractMethodToken GetToken(string hash, string method)
        {
            if (!HasTokens || string.IsNullOrEmpty(hash) || string.IsNullOrEmpty(method))
                return null;
            
            foreach (var token in Tokens)
            {
                if (token.Hash == hash && token.Method == method)
                    return token;
            }
            
            return null;
        }
        
        /// <summary>
        /// Gets all tokens for a specific contract.
        /// </summary>
        /// <param name="contractHash">The contract hash</param>
        /// <returns>List of tokens for the contract</returns>
        public List<ContractMethodToken> GetTokensForContract(string contractHash)
        {
            if (!HasTokens || string.IsNullOrEmpty(contractHash))
                return new List<ContractMethodToken>();
            
            var result = new List<ContractMethodToken>();
            foreach (var token in Tokens)
            {
                if (token.Hash == contractHash)
                    result.Add(token);
            }
            
            return result;
        }
        
        /// <summary>
        /// Gets all unique contract hashes referenced by tokens.
        /// </summary>
        /// <returns>List of unique contract hashes</returns>
        public List<string> GetReferencedContracts()
        {
            if (!HasTokens)
                return new List<string>();
            
            var contracts = new HashSet<string>();
            foreach (var token in Tokens)
            {
                if (!string.IsNullOrEmpty(token.Hash))
                    contracts.Add(token.Hash);
            }
            
            return new List<string>(contracts);
        }
        
        #endregion
        
        #region Validation Methods
        
        /// <summary>
        /// Validates this NEF file.
        /// </summary>
        /// <exception cref="InvalidOperationException">If validation fails</exception>
        public void Validate()
        {
            if (!HasValidMagic)
                throw new InvalidOperationException($"Invalid NEF magic number. Expected {NEF_MAGIC:X8}, got {Magic:X8}.");
            
            if (!HasCompiler)
                throw new InvalidOperationException("NEF compiler information is required.");
            
            if (!HasScript)
                throw new InvalidOperationException("NEF script is required.");
            
            // Validate script is valid base64
            try
            {
                GetScriptBytes();
            }
            catch (FormatException ex)
            {
                throw new InvalidOperationException("NEF script is not valid base64.", ex);
            }
            
            // Validate tokens
            if (Tokens != null)
            {
                foreach (var token in Tokens)
                {
                    token.Validate();
                }
            }
        }
        
        /// <summary>
        /// Verifies the checksum of this NEF file.
        /// </summary>
        /// <returns>True if checksum is valid</returns>
        public bool VerifyChecksum()
        {
            try
            {
                var calculatedChecksum = CalculateChecksum();
                return calculatedChecksum == Checksum;
            }
            catch
            {
                return false;
            }
        }
        
        /// <summary>
        /// Calculates the checksum for this NEF file.
        /// Note: This is a simplified calculation and may not match Neo's exact algorithm.
        /// </summary>
        /// <returns>Calculated checksum</returns>
        private long CalculateChecksum()
        {
            // Simplified checksum calculation
            // In a real implementation, this would follow Neo's exact checksum algorithm
            var scriptBytes = GetScriptBytes();
            long checksum = Magic;
            
            foreach (var b in scriptBytes)
            {
                checksum ^= b;
            }
            
            return checksum;
        }
        
        #endregion
        
        #region Compiler Information
        
        /// <summary>
        /// Gets the compiler name from the compiler string.
        /// </summary>
        /// <returns>Compiler name or empty string</returns>
        public string GetCompilerName()
        {
            if (!HasCompiler)
                return string.Empty;
            
            // Extract compiler name (before version)
            var parts = Compiler.Split('-', ' ');
            return parts.Length > 0 ? parts[0] : Compiler;
        }
        
        /// <summary>
        /// Gets the compiler version from the compiler string.
        /// </summary>
        /// <returns>Compiler version or empty string</returns>
        public string GetCompilerVersion()
        {
            if (!HasCompiler)
                return string.Empty;
            
            // Try to extract version from compiler string
            var parts = Compiler.Split('-');
            if (parts.Length > 1)
                return parts[1];
            
            // Try space separator
            parts = Compiler.Split(' ');
            if (parts.Length > 1)
                return parts[1];
            
            return string.Empty;
        }
        
        /// <summary>
        /// Checks if this NEF was compiled with a specific compiler.
        /// </summary>
        /// <param name="compilerName">The compiler name to check</param>
        /// <returns>True if compiled with the specified compiler</returns>
        public bool IsCompiledWith(string compilerName)
        {
            if (string.IsNullOrEmpty(compilerName) || !HasCompiler)
                return false;
            
            return Compiler.StartsWith(compilerName, StringComparison.OrdinalIgnoreCase);
        }
        
        #endregion
        
        #region Statistics
        
        /// <summary>
        /// Gets statistics about this NEF file.
        /// </summary>
        /// <returns>NEF statistics</returns>
        public NefStatistics GetStatistics()
        {
            return new NefStatistics
            {
                ScriptSize = ScriptSize,
                TokenCount = TokenCount,
                ReferencedContractCount = GetReferencedContracts().Count,
                CompilerName = GetCompilerName(),
                CompilerVersion = GetCompilerVersion(),
                HasSource = HasSource,
                IsValid = HasValidMagic && HasScript,
                ChecksumValid = VerifyChecksum()
            };
        }
        
        #endregion
        
        #region String Representation
        
        /// <summary>
        /// Returns a detailed string representation of this NEF.
        /// </summary>
        /// <returns>Detailed string representation</returns>
        public string ToDetailedString()
        {
            var result = $"ContractNef:\n";
            result += $"  Magic: 0x{Magic:X8} {(HasValidMagic ? "(Valid)" : "(Invalid)")}\n";
            result += $"  Compiler: {Compiler}\n";
            result += $"  Script Size: {ScriptSize} bytes\n";
            result += $"  Tokens: {TokenCount}\n";
            result += $"  Checksum: 0x{Checksum:X8} {(VerifyChecksum() ? "(Valid)" : "(Invalid)")}\n";
            
            if (HasSource)
            {
                result += $"  Source: {Source}\n";
            }
            
            if (HasTokens)
            {
                result += $"  Referenced Contracts: {GetReferencedContracts().Count}\n";
            }
            
            return result;
        }
        
        /// <summary>
        /// Returns a string representation of this NEF.
        /// </summary>
        /// <returns>String representation</returns>
        public override string ToString()
        {
            var compilerName = GetCompilerName();
            var validStr = HasValidMagic ? "Valid" : "Invalid";
            return $"ContractNef(Compiler: {compilerName}, Script: {ScriptSize}B, Tokens: {TokenCount}, {validStr})";
        }
        
        #endregion
    }
    
    /// <summary>
    /// Represents a method token used by a NEF contract for calling other contracts.
    /// </summary>
    [System.Serializable]
    public class ContractMethodToken
    {
        /// <summary>The hash of the contract to call</summary>
        [JsonProperty("hash")]
        public string Hash { get; set; }
        
        /// <summary>The method name to call</summary>
        [JsonProperty("method")]
        public string Method { get; set; }
        
        /// <summary>The number of parameters the method expects</summary>
        [JsonProperty("paramcount")]
        public int ParameterCount { get; set; }
        
        /// <summary>Whether the method has a return value</summary>
        [JsonProperty("hasreturnvalue")]
        public bool HasReturnValue { get; set; }
        
        /// <summary>Call flags for the method invocation</summary>
        [JsonProperty("callflags")]
        public int CallFlags { get; set; }
        
        /// <summary>
        /// Default constructor for JSON deserialization.
        /// </summary>
        public ContractMethodToken()
        {
        }
        
        /// <summary>
        /// Creates a new contract method token.
        /// </summary>
        /// <param name="hash">Contract hash</param>
        /// <param name="method">Method name</param>
        /// <param name="parameterCount">Parameter count</param>
        /// <param name="hasReturnValue">Has return value</param>
        /// <param name="callFlags">Call flags</param>
        public ContractMethodToken(string hash, string method, int parameterCount, bool hasReturnValue, int callFlags)
        {
            Hash = hash;
            Method = method;
            ParameterCount = parameterCount;
            HasReturnValue = hasReturnValue;
            CallFlags = callFlags;
        }
        
        /// <summary>
        /// Validates this method token.
        /// </summary>
        /// <exception cref="InvalidOperationException">If validation fails</exception>
        public void Validate()
        {
            if (string.IsNullOrEmpty(Hash))
                throw new InvalidOperationException("Method token contract hash cannot be null or empty.");
            
            if (string.IsNullOrEmpty(Method))
                throw new InvalidOperationException("Method token method name cannot be null or empty.");
            
            if (ParameterCount < 0)
                throw new InvalidOperationException("Method token parameter count cannot be negative.");
        }
        
        /// <summary>
        /// Returns a string representation of this method token.
        /// </summary>
        /// <returns>String representation</returns>
        public override string ToString()
        {
            var hashPreview = Hash?.Length > 8 ? Hash.Substring(0, 8) + "..." : Hash;
            var returnStr = HasReturnValue ? "with return" : "void";
            return $"MethodToken({hashPreview}::{Method}({ParameterCount} params, {returnStr}))";
        }
    }
    
    /// <summary>
    /// Contains statistical information about a NEF file.
    /// </summary>
    [System.Serializable]
    public class NefStatistics
    {
        /// <summary>Size of the script in bytes</summary>
        public int ScriptSize { get; set; }
        
        /// <summary>Number of method tokens</summary>
        public int TokenCount { get; set; }
        
        /// <summary>Number of unique referenced contracts</summary>
        public int ReferencedContractCount { get; set; }
        
        /// <summary>Name of the compiler used</summary>
        public string CompilerName { get; set; }
        
        /// <summary>Version of the compiler used</summary>
        public string CompilerVersion { get; set; }
        
        /// <summary>Whether source information is available</summary>
        public bool HasSource { get; set; }
        
        /// <summary>Whether the NEF file is valid</summary>
        public bool IsValid { get; set; }
        
        /// <summary>Whether the checksum is valid</summary>
        public bool ChecksumValid { get; set; }
        
        /// <summary>
        /// Returns a string representation of these statistics.
        /// </summary>
        /// <returns>String representation</returns>
        public override string ToString()
        {
            var validStr = IsValid ? "Valid" : "Invalid";
            var checksumStr = ChecksumValid ? "OK" : "Failed";
            return $"NefStatistics({CompilerName}, {ScriptSize}B, Tokens: {TokenCount}, {validStr}, Checksum: {checksumStr})";
        }
    }
}
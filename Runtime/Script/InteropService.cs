using System;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

namespace Neo.Unity.SDK.Script
{
    /// <summary>
    /// Defines system calls (interop services) that can be invoked from Neo VM scripts.
    /// These services provide access to blockchain functionality like cryptography, storage, and runtime information.
    /// </summary>
    public enum InteropService
    {
        // Crypto Services
        SystemCryptoCheckSig,
        SystemCryptoCheckMultisig,
        
        // Contract Services
        SystemContractCall,
        SystemContractCallNative,
        SystemContractGetCallFlags,
        SystemContractCreateStandardAccount,
        SystemContractCreateMultisigAccount,
        SystemContractNativeOnPersist,
        SystemContractNativePostPersist,
        
        // Iterator Services
        SystemIteratorNext,
        SystemIteratorValue,
        
        // Runtime Services
        SystemRuntimePlatform,
        SystemRuntimeGetTrigger,
        SystemRuntimeGetTime,
        SystemRuntimeGetScriptContainer,
        SystemRuntimeGetExecutingScriptHash,
        SystemRuntimeGetCallingScriptHash,
        SystemRuntimeGetEntryScriptHash,
        SystemRuntimeCheckWitness,
        SystemRuntimeGetInvocationCounter,
        SystemRuntimeLog,
        SystemRuntimeNotify,
        SystemRuntimeGetNotifications,
        SystemRuntimeGasLeft,
        SystemRuntimeBurnGas,
        SystemRuntimeGetNetwork,
        SystemRuntimeGetRandom,
        
        // Storage Services
        SystemStorageGetContext,
        SystemStorageGetReadOnlyContext,
        SystemStorageAsReadOnly,
        SystemStorageGet,
        SystemStorageFind,
        SystemStoragePut,
        SystemStorageDelete
    }
    
    /// <summary>
    /// Extension methods for InteropService operations and metadata.
    /// </summary>
    public static class InteropServiceExtensions
    {
        /// <summary>
        /// Gets the string identifier for the interop service.
        /// </summary>
        /// <param name="service">The interop service</param>
        /// <returns>The service identifier string</returns>
        public static string GetServiceName(this InteropService service)
        {
            return service switch
            {
                // Crypto Services
                InteropService.SystemCryptoCheckSig => "System.Crypto.CheckSig",
                InteropService.SystemCryptoCheckMultisig => "System.Crypto.CheckMultisig",
                
                // Contract Services
                InteropService.SystemContractCall => "System.Contract.Call",
                InteropService.SystemContractCallNative => "System.Contract.CallNative",
                InteropService.SystemContractGetCallFlags => "System.Contract.GetCallFlags",
                InteropService.SystemContractCreateStandardAccount => "System.Contract.CreateStandardAccount",
                InteropService.SystemContractCreateMultisigAccount => "System.Contract.CreateMultisigAccount",
                InteropService.SystemContractNativeOnPersist => "System.Contract.NativeOnPersist",
                InteropService.SystemContractNativePostPersist => "System.Contract.NativePostPersist",
                
                // Iterator Services
                InteropService.SystemIteratorNext => "System.Iterator.Next",
                InteropService.SystemIteratorValue => "System.Iterator.Value",
                
                // Runtime Services
                InteropService.SystemRuntimePlatform => "System.Runtime.Platform",
                InteropService.SystemRuntimeGetTrigger => "System.Runtime.GetTrigger",
                InteropService.SystemRuntimeGetTime => "System.Runtime.GetTime",
                InteropService.SystemRuntimeGetScriptContainer => "System.Runtime.GetScriptContainer",
                InteropService.SystemRuntimeGetExecutingScriptHash => "System.Runtime.GetExecutingScriptHash",
                InteropService.SystemRuntimeGetCallingScriptHash => "System.Runtime.GetCallingScriptHash",
                InteropService.SystemRuntimeGetEntryScriptHash => "System.Runtime.GetEntryScriptHash",
                InteropService.SystemRuntimeCheckWitness => "System.Runtime.CheckWitness",
                InteropService.SystemRuntimeGetInvocationCounter => "System.Runtime.GetInvocationCounter",
                InteropService.SystemRuntimeLog => "System.Runtime.Log",
                InteropService.SystemRuntimeNotify => "System.Runtime.Notify",
                InteropService.SystemRuntimeGetNotifications => "System.Runtime.GetNotifications",
                InteropService.SystemRuntimeGasLeft => "System.Runtime.GasLeft",
                InteropService.SystemRuntimeBurnGas => "System.Runtime.BurnGas",
                InteropService.SystemRuntimeGetNetwork => "System.Runtime.GetNetwork",
                InteropService.SystemRuntimeGetRandom => "System.Runtime.GetRandom",
                
                // Storage Services
                InteropService.SystemStorageGetContext => "System.Storage.GetContext",
                InteropService.SystemStorageGetReadOnlyContext => "System.Storage.GetReadOnlyContext",
                InteropService.SystemStorageAsReadOnly => "System.Storage.AsReadOnly",
                InteropService.SystemStorageGet => "System.Storage.Get",
                InteropService.SystemStorageFind => "System.Storage.Find",
                InteropService.SystemStoragePut => "System.Storage.Put",
                InteropService.SystemStorageDelete => "System.Storage.Delete",
                
                _ => throw new ArgumentException($"Unknown interop service: {service}")
            };
        }
        
        /// <summary>
        /// Gets the 4-byte hash of the service name used in syscalls.
        /// </summary>
        /// <param name="service">The interop service</param>
        /// <returns>The 4-byte hash</returns>
        public static byte[] GetHash(this InteropService service)
        {
            var serviceName = service.GetServiceName();
            var nameBytes = Encoding.ASCII.GetBytes(serviceName);
            
            using (var sha256 = SHA256.Create())
            {
                var hash = sha256.ComputeHash(nameBytes);
                var result = new byte[4];
                Array.Copy(hash, 0, result, 0, 4);
                return result;
            }
        }
        
        /// <summary>
        /// Gets the hash as a hex string for debugging and comparison.
        /// </summary>
        /// <param name="service">The interop service</param>
        /// <returns>The hex string representation of the hash</returns>
        public static string GetHashString(this InteropService service)
        {
            var hash = service.GetHash();
            return Convert.ToHexString(hash).ToLowerInvariant();
        }
        
        /// <summary>
        /// Gets the base GAS cost for calling this interop service.
        /// These are the minimum costs; actual costs may be higher based on complexity.
        /// </summary>
        /// <param name="service">The interop service</param>
        /// <returns>The base GAS cost in fractions</returns>
        public static long GetPrice(this InteropService service)
        {
            return service switch
            {
                // 1 << 3 = 8 fractions
                InteropService.SystemRuntimePlatform or 
                InteropService.SystemRuntimeGetTrigger or 
                InteropService.SystemRuntimeGetTime or
                InteropService.SystemRuntimeGetScriptContainer or 
                InteropService.SystemRuntimeGetNetwork => 1 << 3,
                
                // 1 << 4 = 16 fractions
                InteropService.SystemIteratorValue or 
                InteropService.SystemRuntimeGetExecutingScriptHash or 
                InteropService.SystemRuntimeGetCallingScriptHash or
                InteropService.SystemRuntimeGetEntryScriptHash or 
                InteropService.SystemRuntimeGetInvocationCounter or 
                InteropService.SystemRuntimeGasLeft or
                InteropService.SystemRuntimeBurnGas or 
                InteropService.SystemRuntimeGetRandom or 
                InteropService.SystemStorageGetContext or
                InteropService.SystemStorageGetReadOnlyContext or 
                InteropService.SystemStorageAsReadOnly => 1 << 4,
                
                // 1 << 10 = 1024 fractions
                InteropService.SystemContractGetCallFlags or 
                InteropService.SystemRuntimeCheckWitness => 1 << 10,
                
                // 1 << 12 = 4096 fractions
                InteropService.SystemRuntimeGetNotifications => 1 << 12,
                
                // 1 << 15 = 32768 fractions
                InteropService.SystemCryptoCheckSig or 
                InteropService.SystemContractCall or 
                InteropService.SystemContractCreateStandardAccount or
                InteropService.SystemIteratorNext or 
                InteropService.SystemRuntimeLog or 
                InteropService.SystemRuntimeNotify or 
                InteropService.SystemStorageGet or
                InteropService.SystemStorageFind or 
                InteropService.SystemStoragePut or 
                InteropService.SystemStorageDelete => 1 << 15,
                
                // Variable cost services - return 0 as they depend on context
                InteropService.SystemCryptoCheckMultisig or
                InteropService.SystemContractCallNative or
                InteropService.SystemContractCreateMultisigAccount or
                InteropService.SystemContractNativeOnPersist or
                InteropService.SystemContractNativePostPersist => 0,
                
                _ => 1 << 15 // Default to high cost for unknown services
            };
        }
        
        /// <summary>
        /// Gets a description of what this interop service does.
        /// Useful for debugging and documentation.
        /// </summary>
        /// <param name="service">The interop service</param>
        /// <returns>A human-readable description</returns>
        public static string GetDescription(this InteropService service)
        {
            return service switch
            {
                // Crypto Services
                InteropService.SystemCryptoCheckSig => "Verifies a signature against a public key",
                InteropService.SystemCryptoCheckMultisig => "Verifies multiple signatures against multiple public keys",
                
                // Contract Services
                InteropService.SystemContractCall => "Calls a smart contract method",
                InteropService.SystemContractCallNative => "Calls a native contract method",
                InteropService.SystemContractGetCallFlags => "Gets the current contract call flags",
                InteropService.SystemContractCreateStandardAccount => "Creates a standard account script hash",
                InteropService.SystemContractCreateMultisigAccount => "Creates a multi-signature account script hash",
                InteropService.SystemContractNativeOnPersist => "Native contract on-persist callback",
                InteropService.SystemContractNativePostPersist => "Native contract post-persist callback",
                
                // Iterator Services
                InteropService.SystemIteratorNext => "Moves iterator to next item",
                InteropService.SystemIteratorValue => "Gets current iterator value",
                
                // Runtime Services
                InteropService.SystemRuntimePlatform => "Gets the blockchain platform name",
                InteropService.SystemRuntimeGetTrigger => "Gets the trigger type for script execution",
                InteropService.SystemRuntimeGetTime => "Gets current block timestamp",
                InteropService.SystemRuntimeGetScriptContainer => "Gets the script container (transaction/block)",
                InteropService.SystemRuntimeGetExecutingScriptHash => "Gets currently executing script hash",
                InteropService.SystemRuntimeGetCallingScriptHash => "Gets calling script hash",
                InteropService.SystemRuntimeGetEntryScriptHash => "Gets entry point script hash",
                InteropService.SystemRuntimeCheckWitness => "Verifies witness for an account",
                InteropService.SystemRuntimeGetInvocationCounter => "Gets invocation counter",
                InteropService.SystemRuntimeLog => "Emits a log message",
                InteropService.SystemRuntimeNotify => "Emits a notification event",
                InteropService.SystemRuntimeGetNotifications => "Gets all notifications",
                InteropService.SystemRuntimeGasLeft => "Gets remaining GAS for execution",
                InteropService.SystemRuntimeBurnGas => "Burns/consumes GAS",
                InteropService.SystemRuntimeGetNetwork => "Gets network magic number",
                InteropService.SystemRuntimeGetRandom => "Gets a random number",
                
                // Storage Services
                InteropService.SystemStorageGetContext => "Gets storage context for current contract",
                InteropService.SystemStorageGetReadOnlyContext => "Gets read-only storage context",
                InteropService.SystemStorageAsReadOnly => "Converts context to read-only",
                InteropService.SystemStorageGet => "Reads value from storage",
                InteropService.SystemStorageFind => "Finds values in storage",
                InteropService.SystemStoragePut => "Writes value to storage",
                InteropService.SystemStorageDelete => "Deletes value from storage",
                
                _ => $"Unknown interop service: {service}"
            };
        }
        
        /// <summary>
        /// Finds an interop service by its hash.
        /// </summary>
        /// <param name="hash">The 4-byte service hash</param>
        /// <returns>The matching interop service, or null if not found</returns>
        public static InteropService? GetServiceByHash(byte[] hash)
        {
            if (hash == null || hash.Length != 4)
                return null;
            
            var hashString = Convert.ToHexString(hash).ToLowerInvariant();
            return GetServiceByHash(hashString);
        }
        
        /// <summary>
        /// Finds an interop service by its hash string.
        /// </summary>
        /// <param name="hashString">The hex string representation of the hash</param>
        /// <returns>The matching interop service, or null if not found</returns>
        public static InteropService? GetServiceByHash(string hashString)
        {
            if (string.IsNullOrEmpty(hashString))
                return null;
            
            hashString = hashString.ToLowerInvariant();
            
            // Check all services to find a match
            foreach (InteropService service in Enum.GetValues<InteropService>())
            {
                if (service.GetHashString() == hashString)
                    return service;
            }
            
            return null;
        }
        
        /// <summary>
        /// Checks if the service is related to cryptographic operations.
        /// </summary>
        /// <param name="service">The interop service</param>
        /// <returns>True if it's a crypto service</returns>
        public static bool IsCryptoService(this InteropService service)
        {
            return service == InteropService.SystemCryptoCheckSig ||
                   service == InteropService.SystemCryptoCheckMultisig;
        }
        
        /// <summary>
        /// Checks if the service is related to contract operations.
        /// </summary>
        /// <param name="service">The interop service</param>
        /// <returns>True if it's a contract service</returns>
        public static bool IsContractService(this InteropService service)
        {
            return service.GetServiceName().StartsWith("System.Contract.");
        }
        
        /// <summary>
        /// Checks if the service is related to runtime operations.
        /// </summary>
        /// <param name="service">The interop service</param>
        /// <returns>True if it's a runtime service</returns>
        public static bool IsRuntimeService(this InteropService service)
        {
            return service.GetServiceName().StartsWith("System.Runtime.");
        }
        
        /// <summary>
        /// Checks if the service is related to storage operations.
        /// </summary>
        /// <param name="service">The interop service</param>
        /// <returns>True if it's a storage service</returns>
        public static bool IsStorageService(this InteropService service)
        {
            return service.GetServiceName().StartsWith("System.Storage.");
        }
        
        /// <summary>
        /// Checks if the service is related to iterator operations.
        /// </summary>
        /// <param name="service">The interop service</param>
        /// <returns>True if it's an iterator service</returns>
        public static bool IsIteratorService(this InteropService service)
        {
            return service.GetServiceName().StartsWith("System.Iterator.");
        }
    }
    
    /// <summary>
    /// Utility class for working with interop services in Unity.
    /// </summary>
    public static class InteropServiceUtils
    {
        /// <summary>
        /// Logs information about an interop service for debugging.
        /// </summary>
        /// <param name="service">The interop service to log</param>
        public static void LogServiceInfo(InteropService service)
        {
            Debug.Log($"Interop Service: {service.GetServiceName()}\n" +
                     $"Hash: {service.GetHashString()}\n" +
                     $"Price: {service.GetPrice()} GAS fractions\n" +
                     $"Description: {service.GetDescription()}");
        }
        
        /// <summary>
        /// Gets all services of a specific category.
        /// </summary>
        /// <param name="category">The service category (Crypto, Contract, Runtime, Storage, Iterator)</param>
        /// <returns>Array of services in the category</returns>
        public static InteropService[] GetServicesByCategory(string category)
        {
            var services = new System.Collections.Generic.List<InteropService>();
            
            foreach (InteropService service in Enum.GetValues<InteropService>())
            {
                var serviceName = service.GetServiceName();
                if (serviceName.StartsWith($"System.{category}.", StringComparison.OrdinalIgnoreCase))
                {
                    services.Add(service);
                }
            }
            
            return services.ToArray();
        }
        
        /// <summary>
        /// Validates that a hash matches the expected format for interop services.
        /// </summary>
        /// <param name="hash">The hash to validate</param>
        /// <returns>True if the hash is valid</returns>
        public static bool IsValidServiceHash(byte[] hash)
        {
            return hash != null && hash.Length == 4;
        }
        
        /// <summary>
        /// Creates a debug-friendly representation of all interop services.
        /// </summary>
        /// <returns>Formatted string with all service information</returns>
        public static string GetAllServicesInfo()
        {
            var info = new StringBuilder();
            info.AppendLine("Neo Interop Services:");
            info.AppendLine("=====================");
            
            var categories = new[] { "Crypto", "Contract", "Iterator", "Runtime", "Storage" };
            
            foreach (var category in categories)
            {
                info.AppendLine($"\n{category} Services:");
                info.AppendLine(new string('-', category.Length + 10));
                
                var services = GetServicesByCategory(category);
                foreach (var service in services)
                {
                    info.AppendLine($"  {service.GetServiceName()}");
                    info.AppendLine($"    Hash: {service.GetHashString()}");
                    info.AppendLine($"    Cost: {service.GetPrice()} fractions");
                    info.AppendLine($"    Desc: {service.GetDescription()}");
                    info.AppendLine();
                }
            }
            
            return info.ToString();
        }
    }
}
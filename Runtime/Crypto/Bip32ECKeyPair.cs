using System;
using System.Collections.Generic;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Neo.Unity.SDK.Types;
using Neo.Unity.SDK.Utils;

namespace Neo.Unity.SDK.Crypto
{
    /// <summary>
    /// BIP-32 Hierarchical Deterministic (HD) key pair implementation for Neo blockchain.
    /// Enables deterministic key derivation from a master seed for wallet hierarchies.
    /// Production-ready implementation following BIP-32 specification.
    /// </summary>
    [System.Serializable]
    public class Bip32ECKeyPair : ECKeyPair
    {
        #region Constants
        
        /// <summary>Hardened derivation flag (0x80000000)</summary>
        public const uint HARDENED_BIT = 0x80000000;
        
        /// <summary>Bitcoin seed string for HMAC key</summary>
        private const string BITCOIN_SEED = "Bitcoin seed";
        
        /// <summary>Maximum valid child index</summary>
        public const uint MAX_CHILD_INDEX = 0xFFFFFFFF;
        
        #endregion
        
        #region Properties
        
        /// <summary>Whether the parent key had a private key available</summary>
        [SerializeField]
        public bool ParentHasPrivate { get; private set; }
        
        /// <summary>The child number used to derive this key</summary>
        [SerializeField]
        public uint ChildNumber { get; private set; }
        
        /// <summary>The depth in the key derivation hierarchy (0 = master)</summary>
        [SerializeField]
        public int Depth { get; private set; }
        
        /// <summary>The chain code for this key pair</summary>
        [SerializeField]
        public byte[] ChainCode { get; private set; }
        
        /// <summary>The fingerprint of the parent key</summary>
        [SerializeField]
        public uint ParentFingerprint { get; private set; }
        
        /// <summary>The identifier for this key (first 4 bytes of Hash160)</summary>
        [SerializeField]
        public byte[] Identifier { get; private set; }
        
        /// <summary>The fingerprint for this key (first 4 bytes of identifier)</summary>
        public uint Fingerprint { get; private set; }
        
        /// <summary>Whether this is a hardened derivation</summary>
        public bool IsHardened => IsHardenedDerivation(ChildNumber);
        
        /// <summary>The derivation path as a string (e.g., "m/44'/888'/0'/0/0")</summary>
        public string DerivationPath { get; private set; }
        
        #endregion
        
        #region Constructors
        
        /// <summary>
        /// Creates a BIP-32 key pair with the specified parameters.
        /// </summary>
        /// <param name="privateKey">The private key</param>
        /// <param name="publicKey">The public key</param>
        /// <param name="childNumber">The child number</param>
        /// <param name="chainCode">The chain code</param>
        /// <param name="parent">The parent key pair (null for master)</param>
        private Bip32ECKeyPair(BigInteger privateKey, ECPublicKey publicKey, uint childNumber, 
                              byte[] chainCode, Bip32ECKeyPair parent = null)
            : base(privateKey, publicKey)
        {
            ParentHasPrivate = parent != null;
            ChildNumber = childNumber;
            Depth = parent?.Depth + 1 ?? 0;
            ChainCode = (byte[])chainCode.Clone();
            ParentFingerprint = parent?.Fingerprint ?? 0;
            
            // Calculate identifier (Hash160 of compressed public key)
            var compressedPubKey = publicKey.GetEncoded(true);
            Identifier = compressedPubKey.Hash160();
            
            // Calculate fingerprint (first 4 bytes of identifier)
            Fingerprint = BitConverter.ToUInt32(Identifier, 0);
            
            // Build derivation path
            DerivationPath = BuildDerivationPath(parent);
        }
        
        /// <summary>
        /// Private constructor for Unity serialization.
        /// </summary>
        private Bip32ECKeyPair() : base()
        {
        }
        
        #endregion
        
        #region Static Factory Methods
        
        /// <summary>
        /// Creates a master BIP-32 key pair from a private key and chain code.
        /// </summary>
        /// <param name="privateKey">The master private key</param>
        /// <param name="chainCode">The master chain code</param>
        /// <returns>Master BIP-32 key pair</returns>
        public static async Task<Bip32ECKeyPair> Create(BigInteger privateKey, byte[] chainCode)
        {
            if (privateKey <= 0)
                throw new ArgumentException("Private key must be positive", nameof(privateKey));
            
            if (chainCode == null || chainCode.Length != 32)
                throw new ArgumentException("Chain code must be exactly 32 bytes", nameof(chainCode));
            
            var keyPair = await ECKeyPair.Create(privateKey);
            
            return new Bip32ECKeyPair(privateKey, keyPair.PublicKey, 0, chainCode);
        }
        
        /// <summary>
        /// Creates a master BIP-32 key pair from byte arrays.
        /// </summary>
        /// <param name="privateKeyBytes">32-byte private key</param>
        /// <param name="chainCode">32-byte chain code</param>
        /// <returns>Master BIP-32 key pair</returns>
        public static async Task<Bip32ECKeyPair> Create(byte[] privateKeyBytes, byte[] chainCode)
        {
            if (privateKeyBytes == null || privateKeyBytes.Length != 32)
                throw new ArgumentException("Private key must be exactly 32 bytes", nameof(privateKeyBytes));
            
            var privateKey = new BigInteger(privateKeyBytes, isUnsigned: true, isBigEndian: false);
            return await Create(privateKey, chainCode);
        }
        
        /// <summary>
        /// Generates a master BIP-32 key pair from a seed.
        /// </summary>
        /// <param name="seed">Seed bytes (typically 64 bytes from BIP-39 mnemonic)</param>
        /// <returns>Master BIP-32 key pair</returns>
        public static async Task<Bip32ECKeyPair> GenerateKeyPair(byte[] seed)
        {
            if (seed == null || seed.Length < 16 || seed.Length > 64)
                throw new ArgumentException("Seed must be between 16 and 64 bytes", nameof(seed));
            
            // HMAC-SHA512 with "Bitcoin seed" as key
            var seedKey = Encoding.UTF8.GetBytes(BITCOIN_SEED);
            
            using (var hmac = new HMACSHA512(seedKey))
            {
                var hash = hmac.ComputeHash(seed);
                
                var masterPrivateKey = hash.Take(32).ToArray();
                var masterChainCode = hash.Skip(32).Take(32).ToArray();
                
                return await Create(masterPrivateKey, masterChainCode);
            }
        }
        
        #endregion
        
        #region Key Derivation
        
        /// <summary>
        /// Derives a child key pair from a derivation path.
        /// </summary>
        /// <param name="master">The master key pair</param>
        /// <param name="path">Derivation path (e.g., [44, 888, 0, 0, 0] for "m/44'/888'/0'/0/0")</param>
        /// <returns>Derived child key pair</returns>
        public static async Task<Bip32ECKeyPair> DeriveKeyPair(Bip32ECKeyPair master, uint[] path)
        {
            if (master == null)
                throw new ArgumentNullException(nameof(master));
            
            if (path == null)
                throw new ArgumentNullException(nameof(path));
            
            var currentKeyPair = master;
            
            foreach (var childNumber in path)
            {
                currentKeyPair = await currentKeyPair.DeriveChildKey(childNumber);
            }
            
            return currentKeyPair;
        }
        
        /// <summary>
        /// Derives a child key pair from a BIP-44 path string.
        /// </summary>
        /// <param name="master">The master key pair</param>
        /// <param name="pathString">Path string (e.g., "m/44'/888'/0'/0/0")</param>
        /// <returns>Derived child key pair</returns>
        public static async Task<Bip32ECKeyPair> DeriveKeyPair(Bip32ECKeyPair master, string pathString)
        {
            var path = ParseDerivationPath(pathString);
            return await DeriveKeyPair(master, path);
        }
        
        /// <summary>
        /// Derives a child key at the specified index.
        /// </summary>
        /// <param name="childNumber">Child index (use HARDENED_BIT for hardened derivation)</param>
        /// <returns>Derived child key pair</returns>
        public async Task<Bip32ECKeyPair> DeriveChildKey(uint childNumber)
        {
            return await Task.Run(() =>
            {
                try
                {
                    var data = new List<byte>();
                    
                    // For hardened derivation, use private key; for normal, use public key
                    if (IsHardenedDerivation(childNumber))
                    {
                        if (!CanSign)
                            throw new InvalidOperationException("Cannot derive hardened child from public key only");
                        
                        // 0x00 + private key (33 bytes total)
                        data.Add(0x00);
                        var privateKeyBytes = PrivateKey.ToByteArray();
                        if (privateKeyBytes.Length > 32)
                        {
                            data.AddRange(privateKeyBytes.Take(32));
                        }
                        else
                        {
                            data.AddRange(new byte[32 - privateKeyBytes.Length]); // Pad with zeros
                            data.AddRange(privateKeyBytes);
                        }
                    }
                    else
                    {
                        // Compressed public key (33 bytes)
                        data.AddRange(PublicKey.GetEncoded(true));
                    }
                    
                    // Add child number (4 bytes, big-endian)
                    var childBytes = BitConverter.GetBytes(childNumber);
                    if (BitConverter.IsLittleEndian)
                    {
                        Array.Reverse(childBytes);
                    }
                    data.AddRange(childBytes);
                    
                    // HMAC-SHA512 with chain code as key
                    using (var hmac = new HMACSHA512(ChainCode))
                    {
                        var hash = hmac.ComputeHash(data.ToArray());
                        
                        var leftHalf = hash.Take(32).ToArray();
                        var rightHalf = hash.Skip(32).Take(32).ToArray();
                        
                        // Parse left half as private key
                        var leftBigInt = new BigInteger(leftHalf, isUnsigned: true, isBigEndian: false);
                        
                        // Validate that left half is valid private key
                        var curveOrder = GetCurveOrder();
                        if (leftBigInt >= curveOrder)
                            throw new CryptographicException("Invalid child key derivation - left half too large");
                        
                        // Calculate child private key
                        var childPrivateKey = (PrivateKey + leftBigInt) % curveOrder;
                        if (childPrivateKey == 0)
                            throw new CryptographicException("Invalid child key derivation - zero private key");
                        
                        // Create child key pair
                        var childECKeyPair = await ECKeyPair.Create(childPrivateKey);
                        
                        return new Bip32ECKeyPair(childPrivateKey, childECKeyPair.PublicKey, 
                                                 childNumber, rightHalf, this);
                    }
                }
                catch (Exception ex)
                {
                    throw new CryptographicException($"Failed to derive child key: {ex.Message}", ex);
                }
            });
        }
        
        #endregion
        
        #region Utility Methods
        
        /// <summary>
        /// Checks if a child number represents hardened derivation.
        /// </summary>
        /// <param name="childNumber">The child number</param>
        /// <returns>True if hardened derivation</returns>
        public static bool IsHardenedDerivation(uint childNumber)
        {
            return (childNumber & HARDENED_BIT) != 0;
        }
        
        /// <summary>
        /// Converts a child number to hardened derivation.
        /// </summary>
        /// <param name="childNumber">The child number</param>
        /// <returns>Hardened child number</returns>
        public static uint MakeHardened(uint childNumber)
        {
            return childNumber | HARDENED_BIT;
        }
        
        /// <summary>
        /// Removes hardened bit from child number.
        /// </summary>
        /// <param name="childNumber">The child number</param>
        /// <returns>Non-hardened child number</returns>
        public static uint RemoveHardened(uint childNumber)
        {
            return childNumber & ~HARDENED_BIT;
        }
        
        /// <summary>
        /// Parses a BIP-44 derivation path string.
        /// </summary>
        /// <param name="pathString">Path string (e.g., "m/44'/888'/0'/0/0")</param>
        /// <returns>Array of child numbers</returns>
        public static uint[] ParseDerivationPath(string pathString)
        {
            if (string.IsNullOrEmpty(pathString))
                throw new ArgumentException("Path string cannot be null or empty", nameof(pathString));
            
            // Remove 'm/' prefix if present
            var path = pathString.StartsWith("m/") ? pathString.Substring(2) : pathString;
            
            if (string.IsNullOrEmpty(path))
                return new uint[0];
            
            var segments = path.Split('/');
            var result = new uint[segments.Length];
            
            for (int i = 0; i < segments.Length; i++)
            {
                var segment = segments[i];
                bool isHardened = segment.EndsWith("'") || segment.EndsWith("h");
                
                if (isHardened)
                {
                    segment = segment.Substring(0, segment.Length - 1);
                }
                
                if (!uint.TryParse(segment, out var childNumber))
                    throw new ArgumentException($"Invalid path segment: {segments[i]}", nameof(pathString));
                
                if (isHardened)
                {
                    childNumber = MakeHardened(childNumber);
                }
                
                result[i] = childNumber;
            }
            
            return result;
        }
        
        /// <summary>
        /// Gets the secp256r1 curve order.
        /// </summary>
        /// <returns>Curve order as BigInteger</returns>
        private static BigInteger GetCurveOrder()
        {
            // secp256r1 curve order
            return BigInteger.Parse("115792089210356248762697446949407573529996955224135760342422259061068512044369");
        }
        
        /// <summary>
        /// Builds the derivation path string for this key.
        /// </summary>
        /// <param name="parent">The parent key pair</param>
        /// <returns>Derivation path string</returns>
        private string BuildDerivationPath(Bip32ECKeyPair parent)
        {
            if (parent == null)
                return "m"; // Master key
            
            var parentPath = parent.DerivationPath;
            var childStr = IsHardened ? $"{RemoveHardened(ChildNumber)}'" : ChildNumber.ToString();
            
            return $"{parentPath}/{childStr}";
        }
        
        #endregion
        
        #region Standard Derivation Paths
        
        /// <summary>
        /// Derives a Neo account using standard BIP-44 path (m/44'/888'/0'/0/index).
        /// </summary>
        /// <param name="accountIndex">The account index</param>
        /// <returns>Derived account key pair</returns>
        public async Task<Bip32ECKeyPair> DeriveNeoAccount(uint accountIndex = 0)
        {
            // BIP-44 path for Neo: m/44'/888'/account'/0/0
            var path = new uint[]
            {
                MakeHardened(44),   // Purpose (BIP-44)
                MakeHardened(888),  // Coin type (Neo)
                MakeHardened(accountIndex), // Account
                0,                  // Change (0 = external)
                0                   // Address index
            };
            
            return await DeriveKeyPair(this, path);
        }
        
        /// <summary>
        /// Derives multiple Neo accounts for a wallet.
        /// </summary>
        /// <param name="numberOfAccounts">Number of accounts to derive</param>
        /// <param name="startIndex">Starting account index</param>
        /// <returns>List of derived account key pairs</returns>
        public async Task<List<Bip32ECKeyPair>> DeriveNeoAccounts(int numberOfAccounts, uint startIndex = 0)
        {
            if (numberOfAccounts <= 0)
                throw new ArgumentException("Number of accounts must be positive", nameof(numberOfAccounts));
            
            var accounts = new List<Bip32ECKeyPair>(numberOfAccounts);
            
            for (uint i = 0; i < numberOfAccounts; i++)
            {
                var account = await DeriveNeoAccount(startIndex + i);
                accounts.Add(account);
            }
            
            return accounts;
        }
        
        #endregion
        
        #region Extended Keys
        
        /// <summary>
        /// Exports the extended private key (xprv format).
        /// </summary>
        /// <returns>Base58Check encoded extended private key</returns>
        public string ExportExtendedPrivateKey()
        {
            if (!CanSign)
                throw new InvalidOperationException("Cannot export extended private key without private key");
            
            var data = new List<byte>();
            
            // Version (4 bytes) - mainnet private key
            data.AddRange(BitConverter.GetBytes(0x0488ADE4u)); // xprv version
            
            // Depth (1 byte)
            data.Add((byte)Depth);
            
            // Parent fingerprint (4 bytes)
            data.AddRange(BitConverter.GetBytes(ParentFingerprint));
            
            // Child number (4 bytes)
            data.AddRange(BitConverter.GetBytes(ChildNumber));
            
            // Chain code (32 bytes)
            data.AddRange(ChainCode);
            
            // Private key (33 bytes: 0x00 + 32-byte private key)
            data.Add(0x00);
            var privateKeyBytes = PrivateKey.ToByteArray();
            if (privateKeyBytes.Length > 32)
            {
                data.AddRange(privateKeyBytes.Take(32));
            }
            else
            {
                data.AddRange(new byte[32 - privateKeyBytes.Length]); // Pad with zeros
                data.AddRange(privateKeyBytes);
            }
            
            return data.ToArray().ToBase58CheckEncoded();
        }
        
        /// <summary>
        /// Exports the extended public key (xpub format).
        /// </summary>
        /// <returns>Base58Check encoded extended public key</returns>
        public string ExportExtendedPublicKey()
        {
            var data = new List<byte>();
            
            // Version (4 bytes) - mainnet public key
            data.AddRange(BitConverter.GetBytes(0x0488B21Eu)); // xpub version
            
            // Depth (1 byte)
            data.Add((byte)Depth);
            
            // Parent fingerprint (4 bytes)
            data.AddRange(BitConverter.GetBytes(ParentFingerprint));
            
            // Child number (4 bytes)
            data.AddRange(BitConverter.GetBytes(ChildNumber));
            
            // Chain code (32 bytes)
            data.AddRange(ChainCode);
            
            // Public key (33 bytes compressed)
            data.AddRange(PublicKey.GetEncoded(true));
            
            return data.ToArray().ToBase58CheckEncoded();
        }
        
        #endregion
        
        #region Validation
        
        /// <summary>
        /// Validates that this key pair is properly formed.
        /// </summary>
        /// <exception cref="InvalidOperationException">If validation fails</exception>
        public void Validate()
        {
            if (ChainCode == null || ChainCode.Length != 32)
                throw new InvalidOperationException("Invalid chain code length");
            
            if (Identifier == null || Identifier.Length != 20)
                throw new InvalidOperationException("Invalid identifier length");
            
            if (Depth < 0 || Depth > 255)
                throw new InvalidOperationException("Invalid depth value");
            
            // Validate derivation path format
            if (string.IsNullOrEmpty(DerivationPath))
                throw new InvalidOperationException("Invalid derivation path");
        }
        
        #endregion
        
        #region String Representation
        
        /// <summary>
        /// Returns a string representation of this BIP-32 key pair.
        /// </summary>
        /// <returns>String representation</returns>
        public override string ToString()
        {
            var hardened = IsHardened ? " (Hardened)" : "";
            return $"Bip32ECKeyPair(Path: {DerivationPath}, Address: {GetAddress()}{hardened})";
        }
        
        #endregion
        
        #region Disposal Override
        
        /// <summary>
        /// Securely disposes of the BIP-32 key pair.
        /// </summary>
        public new void Dispose()
        {
            if (ChainCode != null)
            {
                Array.Clear(ChainCode, 0, ChainCode.Length);
                ChainCode = null;
            }
            
            if (Identifier != null)
            {
                Array.Clear(Identifier, 0, Identifier.Length);
                Identifier = null;
            }
            
            base.Dispose();
        }
        
        #endregion
    }
}
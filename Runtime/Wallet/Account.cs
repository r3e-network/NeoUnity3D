using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using Neo.Unity.SDK.Core;
using Neo.Unity.SDK.Types;
using Neo.Unity.SDK.Crypto;
using Neo.Unity.SDK.Script;
using Neo.Unity.SDK.Utils;
using System.Numerics;

namespace Neo.Unity.SDK.Wallet
{
    /// <summary>
    /// Represents a Neo account which can be a single-signature or multi-signature account.
    /// Unity-optimized implementation with proper serialization and async support.
    /// </summary>
    [System.Serializable]
    public class Account
    {
        #region Private Fields
        
        [SerializeField]
        private ECKeyPair keyPair;
        
        [SerializeField]
        private string address;
        
        [SerializeField]
        private string label;
        
        [SerializeField]
        private VerificationScript verificationScript;
        
        [SerializeField]
        private bool isLocked;
        
        [SerializeField]
        private string encryptedPrivateKey;
        
        [SerializeField]
        private int? signingThreshold;
        
        [SerializeField]
        private int? numberOfParticipants;
        
        // Non-serialized reference to parent wallet
        private NeoWallet wallet;
        
        #endregion
        
        #region Properties
        
        /// <summary>This account's EC key pair if available. Null if the key pair is not available (e.g., encrypted).</summary>
        public ECKeyPair KeyPair => keyPair;
        
        /// <summary>The Neo address of this account</summary>
        public string Address => address;
        
        /// <summary>The label/name for this account</summary>
        public string Label => label ?? address;
        
        /// <summary>The verification script for this account</summary>
        public VerificationScript VerificationScript => verificationScript;
        
        /// <summary>Whether this account is locked</summary>
        public bool IsLocked => isLocked;
        
        /// <summary>The encrypted private key (NEP-2 format) if available</summary>
        public string EncryptedPrivateKey => encryptedPrivateKey;
        
        /// <summary>The wallet that contains this account</summary>
        public NeoWallet Wallet => wallet;
        
        /// <summary>The signing threshold for multi-sig accounts (null for single-sig)</summary>
        public int? SigningThreshold => signingThreshold;
        
        /// <summary>The number of participants for multi-sig accounts (null for single-sig)</summary>
        public int? NumberOfParticipants => numberOfParticipants;
        
        /// <summary>Whether this account is the default account in its wallet</summary>
        public bool IsDefault => wallet?.IsDefault(this) ?? false;
        
        /// <summary>Whether this account is a multi-signature account</summary>
        public bool IsMultiSig => signingThreshold.HasValue && numberOfParticipants.HasValue;
        
        /// <summary>The script hash of this account</summary>
        public Hash160 ScriptHash
        {
            get
            {
                try
                {
                    return GetScriptHash();
                }
                catch
                {
                    return null;
                }
            }
        }
        
        /// <summary>Whether this account has a private key available for signing</summary>
        public bool CanSign => keyPair != null;
        
        /// <summary>Whether this account has an encrypted private key</summary>
        public bool HasEncryptedKey => !string.IsNullOrEmpty(encryptedPrivateKey);
        
        #endregion
        
        #region Constructors
        
        /// <summary>
        /// Constructs a new single-signature account with the given EC key pair.
        /// </summary>
        /// <param name="keyPair">The key pair of the account</param>
        public Account(ECKeyPair keyPair)
        {
            this.keyPair = keyPair ?? throw new ArgumentNullException(nameof(keyPair));
            this.address = keyPair.GetAddress();
            this.label = address;
            this.verificationScript = new VerificationScript(keyPair.PublicKey);
            this.isLocked = false;
        }
        
        /// <summary>
        /// Constructs a new multi-signature account with the given key pair and multi-sig parameters.
        /// </summary>
        /// <param name="keyPair">The key pair of the account</param>
        /// <param name="signingThreshold">The signing threshold</param>
        /// <param name="numberOfParticipants">The number of participants</param>
        public Account(ECKeyPair keyPair, int signingThreshold, int numberOfParticipants) : this(keyPair)
        {
            if (signingThreshold <= 0 || signingThreshold > numberOfParticipants)
                throw new ArgumentException("Invalid signing threshold.");
                
            if (numberOfParticipants <= 0)
                throw new ArgumentException("Number of participants must be positive.");
            
            this.signingThreshold = signingThreshold;
            this.numberOfParticipants = numberOfParticipants;
        }
        
        /// <summary>
        /// Constructs an account with the given address and optional parameters.
        /// </summary>
        /// <param name="address">The Neo address</param>
        /// <param name="label">The label for the account</param>
        /// <param name="verificationScript">The verification script</param>
        /// <param name="signingThreshold">The signing threshold for multi-sig</param>
        /// <param name="numberOfParticipants">The number of participants for multi-sig</param>
        public Account(string address, string label = null, VerificationScript verificationScript = null, 
                      int? signingThreshold = null, int? numberOfParticipants = null)
        {
            if (string.IsNullOrEmpty(address))
                throw new ArgumentException("Address cannot be null or empty.", nameof(address));
            
            if (!address.IsValidAddress())
                throw new ArgumentException("Invalid Neo address format.", nameof(address));
            
            this.address = address;
            this.label = label ?? address;
            this.verificationScript = verificationScript;
            this.signingThreshold = signingThreshold;
            this.numberOfParticipants = numberOfParticipants;
            this.isLocked = false;
        }
        
        /// <summary>
        /// Full constructor for internal use (deserialization, etc.).
        /// </summary>
        internal Account(ECKeyPair keyPair, string address, string label, VerificationScript verificationScript,
                        bool isLocked, string encryptedPrivateKey, NeoWallet wallet, 
                        int? signingThreshold, int? numberOfParticipants)
        {
            this.keyPair = keyPair;
            this.address = address ?? throw new ArgumentException("Address cannot be null.", nameof(address));
            this.label = label ?? address;
            this.verificationScript = verificationScript;
            this.isLocked = isLocked;
            this.encryptedPrivateKey = encryptedPrivateKey;
            this.wallet = wallet;
            this.signingThreshold = signingThreshold;
            this.numberOfParticipants = numberOfParticipants;
        }
        
        /// <summary>
        /// Default constructor for Unity serialization.
        /// </summary>
        private Account()
        {
        }
        
        #endregion
        
        #region Configuration Methods
        
        /// <summary>
        /// Sets the label for this account.
        /// </summary>
        /// <param name="label">The new label</param>
        /// <returns>This account for method chaining</returns>
        public Account SetLabel(string label)
        {
            this.label = label ?? address;
            return this;
        }
        
        /// <summary>
        /// Sets the wallet that contains this account (internal use).
        /// </summary>
        /// <param name="wallet">The wallet</param>
        /// <returns>This account for method chaining</returns>
        internal Account SetWallet(NeoWallet wallet)
        {
            this.wallet = wallet;
            return this;
        }
        
        /// <summary>
        /// Locks this account to prevent its use.
        /// </summary>
        /// <returns>This account for method chaining</returns>
        public Account Lock()
        {
            isLocked = true;
            return this;
        }
        
        /// <summary>
        /// Unlocks this account for use.
        /// </summary>
        public void Unlock()
        {
            isLocked = false;
        }
        
        #endregion
        
        #region Cryptographic Operations
        
        /// <summary>
        /// Decrypts this account's private key according to the NEP-2 standard, if not already decrypted.
        /// </summary>
        /// <param name="password">The passphrase used to decrypt the private key</param>
        /// <param name="scryptParams">The Scrypt parameters used for decryption</param>
        public async Task DecryptPrivateKey(string password, ScryptParams scryptParams = null)
        {
            if (keyPair != null)
                return; // Already decrypted
                
            if (string.IsNullOrEmpty(encryptedPrivateKey))
            {
                throw new WalletException("The account does not hold an encrypted private key.");
            }
            
            if (string.IsNullOrEmpty(password))
            {
                throw new ArgumentException("Password cannot be null or empty.", nameof(password));
            }
            
            try
            {
                scryptParams = scryptParams ?? ScryptParams.Default;
                keyPair = await NEP2.Decrypt(password, encryptedPrivateKey, scryptParams);
                
                if (NeoUnity.Instance.Config.EnableDebugLogging)
                {
                    Debug.Log($"[Account] Successfully decrypted private key for {address}");
                }
            }
            catch (Exception ex)
            {
                throw new WalletException($"Failed to decrypt private key for account {address}: {ex.Message}", ex);
            }
        }
        
        /// <summary>
        /// Encrypts this account's private key according to the NEP-2 standard.
        /// </summary>
        /// <param name="password">The passphrase used to encrypt the private key</param>
        /// <param name="scryptParams">The Scrypt parameters used for encryption</param>
        public async Task EncryptPrivateKey(string password, ScryptParams scryptParams = null)
        {
            if (keyPair == null)
            {
                throw new WalletException("The account does not hold a decrypted private key.");
            }
            
            if (string.IsNullOrEmpty(password))
            {
                throw new ArgumentException("Password cannot be null or empty.", nameof(password));
            }
            
            try
            {
                scryptParams = scryptParams ?? ScryptParams.Default;
                encryptedPrivateKey = await NEP2.Encrypt(password, keyPair, scryptParams);
                keyPair = null; // Clear the unencrypted key
                
                if (NeoUnity.Instance.Config.EnableDebugLogging)
                {
                    Debug.Log($"[Account] Successfully encrypted private key for {address}");
                }
            }
            catch (Exception ex)
            {
                throw new WalletException($"Failed to encrypt private key for account {address}: {ex.Message}", ex);
            }
        }
        
        #endregion
        
        #region Hash and Address Operations
        
        /// <summary>
        /// Gets the script hash of this account.
        /// </summary>
        /// <returns>The script hash</returns>
        public Hash160 GetScriptHash()
        {
            return Hash160.FromAddress(address);
        }
        
        /// <summary>
        /// Gets the signing threshold for multi-signature accounts.
        /// </summary>
        /// <returns>The signing threshold</returns>
        /// <exception cref="InvalidOperationException">If the account is not multi-sig</exception>
        public int GetSigningThreshold()
        {
            if (!IsMultiSig || !signingThreshold.HasValue)
            {
                throw new InvalidOperationException($"Cannot get signing threshold from account {address}, because it is not multi-sig.");
            }
            
            return signingThreshold.Value;
        }
        
        /// <summary>
        /// Gets the number of participants for multi-signature accounts.
        /// </summary>
        /// <returns>The number of participants</returns>
        /// <exception cref="InvalidOperationException">If the account is not multi-sig</exception>
        public int GetNumberOfParticipants()
        {
            if (!IsMultiSig || !numberOfParticipants.HasValue)
            {
                throw new InvalidOperationException($"Cannot get number of participants from account {address}, because it is not multi-sig.");
            }
            
            return numberOfParticipants.Value;
        }
        
        #endregion
        
        #region Balance Operations
        
        /// <summary>
        /// Gets the balances of all NEP-17 tokens that this account owns.
        /// The token amounts are returned in token fractions.
        /// Requires a Neo node with the RpcNep17Tracker plugin installed.
        /// </summary>
        /// <param name="neoUnity">The NeoUnity instance to use for RPC calls</param>
        /// <returns>Dictionary mapping token script hashes to token amounts</returns>
        public async Task<Dictionary<Hash160, long>> GetNep17Balances(NeoUnity neoUnity = null)
        {
            neoUnity = neoUnity ?? NeoUnity.Instance;
            
            try
            {
                var response = await neoUnity.GetNep17Balances(GetScriptHash()).SendAsync();
                var result = response.GetResult();
                
                var balances = new Dictionary<Hash160, long>();
                
                foreach (var balance in result.Balances)
                {
                    balances[balance.AssetHash] = long.Parse(balance.Amount);
                }
                
                return balances;
            }
            catch (Exception ex)
            {
                throw new WalletException($"Failed to get NEP-17 balances for account {address}: {ex.Message}", ex);
            }
        }
        
        /// <summary>
        /// Gets the balance of a specific NEP-17 token for this account.
        /// </summary>
        /// <param name="tokenHash">The token contract hash</param>
        /// <param name="neoUnity">The NeoUnity instance to use for RPC calls</param>
        /// <returns>The token balance in fractions</returns>
        public async Task<long> GetTokenBalance(Hash160 tokenHash, NeoUnity neoUnity = null)
        {
            if (tokenHash == null)
                throw new ArgumentNullException(nameof(tokenHash));
            
            var balances = await GetNep17Balances(neoUnity);
            return balances.TryGetValue(tokenHash, out var balance) ? balance : 0;
        }
        
        #endregion
        
        #region NEP-6 Conversion
        
        /// <summary>
        /// Converts this account to a NEP-6 account format.
        /// </summary>
        /// <returns>The NEP-6 account representation</returns>
        public async Task<NEP6Account> ToNEP6Account()
        {
            if (keyPair != null && string.IsNullOrEmpty(encryptedPrivateKey))
            {
                throw new WalletException("Account private key is available but not encrypted.");
            }
            
            NEP6Contract contract = null;
            
            if (verificationScript != null)
            {
                var parameters = new List<NEP6Parameter>();
                
                if (verificationScript.IsMultiSigScript())
                {
                    var numberOfAccounts = verificationScript.GetNumberOfAccounts();
                    for (int i = 0; i < numberOfAccounts; i++)
                    {
                        parameters.Add(new NEP6Parameter($"signature{i}", ContractParameterType.Signature));
                    }
                }
                else if (verificationScript.IsSingleSigScript())
                {
                    parameters.Add(new NEP6Parameter("signature", ContractParameterType.Signature));
                }
                
                var scriptBase64 = Convert.ToBase64String(verificationScript.Script);
                contract = new NEP6Contract(scriptBase64, parameters, false);
            }
            
            return new NEP6Account(address, label, IsDefault, isLocked, encryptedPrivateKey, contract, null);
        }
        
        #endregion
        
        #region Static Factory Methods
        
        /// <summary>
        /// Creates an account from the given verification script.
        /// </summary>
        /// <param name="script">The verification script</param>
        /// <returns>The account</returns>
        public static Account FromVerificationScript(VerificationScript script)
        {
            if (script == null)
                throw new ArgumentNullException(nameof(script));
            
            var address = Hash160.FromScript(script.Script).ToAddress();
            int? signingThreshold = null;
            int? numberOfParticipants = null;
            
            if (script.IsMultiSigScript())
            {
                signingThreshold = script.GetSigningThreshold();
                numberOfParticipants = script.GetNumberOfAccounts();
            }
            
            return new Account(address, address, script, signingThreshold, numberOfParticipants);
        }
        
        /// <summary>
        /// Creates an account from the given public key.
        /// </summary>
        /// <param name="publicKey">The public key</param>
        /// <returns>The account</returns>
        public static Account FromPublicKey(ECPublicKey publicKey)
        {
            if (publicKey == null)
                throw new ArgumentNullException(nameof(publicKey));
            
            var script = new VerificationScript(publicKey);
            var address = Hash160.FromScript(script.Script).ToAddress();
            
            return new Account(address, address, script);
        }
        
        /// <summary>
        /// Creates a multi-signature account from the given public keys.
        /// </summary>
        /// <param name="publicKeys">The public keys</param>
        /// <param name="signingThreshold">The signing threshold</param>
        /// <returns>The multi-sig account</returns>
        public static Account CreateMultiSigAccount(List<ECPublicKey> publicKeys, int signingThreshold)
        {
            if (publicKeys == null || publicKeys.Count == 0)
                throw new ArgumentException("Public keys list cannot be null or empty.", nameof(publicKeys));
            
            if (signingThreshold <= 0 || signingThreshold > publicKeys.Count)
                throw new ArgumentException("Invalid signing threshold.", nameof(signingThreshold));
            
            var script = new VerificationScript(publicKeys, signingThreshold);
            var address = Hash160.FromScript(script.Script).ToAddress();
            
            return new Account(address, address, script, signingThreshold, publicKeys.Count);
        }
        
        /// <summary>
        /// Creates a multi-signature account with the given parameters.
        /// </summary>
        /// <param name="address">The address of the multi-sig account</param>
        /// <param name="signingThreshold">The signing threshold</param>
        /// <param name="numberOfParticipants">The number of participants</param>
        /// <returns>The multi-sig account</returns>
        public static Account CreateMultiSigAccount(string address, int signingThreshold, int numberOfParticipants)
        {
            if (string.IsNullOrEmpty(address))
                throw new ArgumentException("Address cannot be null or empty.", nameof(address));
            
            if (signingThreshold <= 0 || signingThreshold > numberOfParticipants)
                throw new ArgumentException("Invalid signing threshold.", nameof(signingThreshold));
            
            if (numberOfParticipants <= 0)
                throw new ArgumentException("Number of participants must be positive.", nameof(numberOfParticipants));
            
            return new Account(address, address, null, signingThreshold, numberOfParticipants);
        }
        
        /// <summary>
        /// Creates an account from the given WIF (Wallet Import Format) private key.
        /// </summary>
        /// <param name="wif">The WIF private key</param>
        /// <returns>The account</returns>
        public static async Task<Account> FromWIF(string wif)
        {
            if (string.IsNullOrEmpty(wif))
                throw new ArgumentException("WIF cannot be null or empty.", nameof(wif));
            
            try
            {
                var privateKeyBytes = wif.PrivateKeyFromWIF();
                var privateKey = new BigInteger(privateKeyBytes, isUnsigned: true, isBigEndian: true);
                var keyPair = await ECKeyPair.Create(privateKey);
                
                return new Account(keyPair);
            }
            catch (Exception ex)
            {
                throw new WalletException($"Failed to create account from WIF: {ex.Message}", ex);
            }
        }
        
        /// <summary>
        /// Creates an account from a NEP-6 account.
        /// </summary>
        /// <param name="nep6Account">The NEP-6 account</param>
        /// <returns>The account</returns>
        public static async Task<Account> FromNEP6Account(NEP6Account nep6Account)
        {
            if (nep6Account == null)
                throw new ArgumentNullException(nameof(nep6Account));
            
            VerificationScript verificationScript = null;
            int? signingThreshold = null;
            int? numberOfParticipants = null;
            
            if (nep6Account.Contract?.Script != null && !string.IsNullOrEmpty(nep6Account.Contract.Script))
            {
                var scriptBytes = Convert.FromBase64String(nep6Account.Contract.Script);
                verificationScript = new VerificationScript(scriptBytes);
                
                if (verificationScript.IsMultiSigScript())
                {
                    signingThreshold = verificationScript.GetSigningThreshold();
                    numberOfParticipants = verificationScript.GetNumberOfAccounts();
                }
            }
            
            return new Account(null, nep6Account.Address, nep6Account.Label, verificationScript,
                              nep6Account.Lock, nep6Account.Key, null, signingThreshold, numberOfParticipants);
        }
        
        /// <summary>
        /// Creates an account from the given address.
        /// Note: An account created this way cannot be used for transaction signing.
        /// </summary>
        /// <param name="address">The Neo address</param>
        /// <returns>The account</returns>
        public static Account FromAddress(string address)
        {
            if (string.IsNullOrEmpty(address))
                throw new ArgumentException("Address cannot be null or empty.", nameof(address));
            
            if (!address.IsValidAddress())
                throw new ArgumentException("Invalid Neo address format.", nameof(address));
            
            return new Account(address, address);
        }
        
        /// <summary>
        /// Creates an account from the given script hash.
        /// Note: An account created this way cannot be used for transaction signing.
        /// </summary>
        /// <param name="scriptHash">The script hash</param>
        /// <returns>The account</returns>
        public static Account FromScriptHash(Hash160 scriptHash)
        {
            if (scriptHash == null)
                throw new ArgumentNullException(nameof(scriptHash));
            
            return FromAddress(scriptHash.ToAddress());
        }
        
        /// <summary>
        /// Creates a new account with a fresh key pair.
        /// </summary>
        /// <returns>The new account</returns>
        public static async Task<Account> Create()
        {
            try
            {
                var keyPair = await ECKeyPair.CreateEcKeyPair();
                return new Account(keyPair);
            }
            catch (Exception ex)
            {
                throw new WalletException($"Failed to create a new account: {ex.Message}", ex);
            }
        }
        
        #endregion
        
        #region String Representation
        
        /// <summary>
        /// Returns a string representation of this account.
        /// </summary>
        /// <returns>String representation</returns>
        public override string ToString()
        {
            var type = IsMultiSig ? $"MultiSig({signingThreshold}/{numberOfParticipants})" : "SingleSig";
            var lockStatus = isLocked ? "Locked" : "Unlocked";
            var keyStatus = keyPair != null ? "Has Key" : (HasEncryptedKey ? "Encrypted" : "No Key");
            
            return $"Account(Address: {address}, Type: {type}, {lockStatus}, {keyStatus})";
        }
        
        #endregion
    }
    
    /// <summary>
    /// Exception thrown when wallet or account operations fail.
    /// </summary>
    public class WalletException : NeoUnityException
    {
        /// <summary>
        /// Creates a new wallet exception.
        /// </summary>
        /// <param name="message">The exception message</param>
        public WalletException(string message) : base(message)
        {
        }
        
        /// <summary>
        /// Creates a new wallet exception with an inner exception.
        /// </summary>
        /// <param name="message">The exception message</param>
        /// <param name="innerException">The inner exception</param>
        public WalletException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
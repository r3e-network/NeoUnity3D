using System;
using Neo.Unity.SDK.Types;
using Neo.Unity.SDK.Wallet;

namespace Neo.Unity.SDK.Transaction
{
    /// <summary>
    /// A signer of a transaction that represents an account. It defines the scope in which 
    /// the witness/signature of an account is valid, i.e., which contracts can use the witness in an invocation.
    /// </summary>
    [System.Serializable]
    public class AccountSigner : Signer
    {
        #region Properties
        
        /// <summary>The account associated with this signer</summary>
        public Account Account { get; private set; }
        
        #endregion
        
        #region Constructor
        
        /// <summary>
        /// Creates an account signer with the specified account and witness scope.
        /// </summary>
        /// <param name="account">The account</param>
        /// <param name="scope">The witness scope</param>
        private AccountSigner(Account account, WitnessScope scope) : base(account.GetScriptHash(), scope)
        {
            Account = account ?? throw new ArgumentNullException(nameof(account));
        }
        
        /// <summary>
        /// Default constructor for Unity serialization.
        /// </summary>
        private AccountSigner() : base()
        {
        }
        
        #endregion
        
        #region Static Factory Methods
        
        /// <summary>
        /// Creates a signer for the given account with no witness scope (WitnessScope.None).
        /// The signature of this signer is only used for transactions and is disabled in contracts.
        /// This is typically used for fee-only signers.
        /// </summary>
        /// <param name="account">The signer account</param>
        /// <returns>The account signer</returns>
        public static AccountSigner None(Account account)
        {
            if (account == null)
                throw new ArgumentNullException(nameof(account));
                
            return new AccountSigner(account, WitnessScope.None);
        }
        
        /// <summary>
        /// Creates a signer for the given account hash with no witness scope (WitnessScope.None).
        /// The signature of this signer is only used for transactions and is disabled in contracts.
        /// This is typically used for fee-only signers.
        /// </summary>
        /// <param name="accountHash">The script hash of the signer account</param>
        /// <returns>The account signer</returns>
        public static AccountSigner None(Hash160 accountHash)
        {
            if (accountHash == null)
                throw new ArgumentNullException(nameof(accountHash));
                
            var account = Account.FromAddress(accountHash.ToAddress());
            return new AccountSigner(account, WitnessScope.None);
        }
        
        /// <summary>
        /// Creates a signer for the given account with CalledByEntry scope.
        /// This scope only allows the entry point contract to use this signer's witness.
        /// This is the most commonly used scope for regular transactions.
        /// </summary>
        /// <param name="account">The signer account</param>
        /// <returns>The account signer</returns>
        public static AccountSigner CalledByEntry(Account account)
        {
            if (account == null)
                throw new ArgumentNullException(nameof(account));
                
            return new AccountSigner(account, WitnessScope.CalledByEntry);
        }
        
        /// <summary>
        /// Creates a signer for the given account hash with CalledByEntry scope.
        /// This scope only allows the entry point contract to use this signer's witness.
        /// This is the most commonly used scope for regular transactions.
        /// </summary>
        /// <param name="accountHash">The script hash of the signer account</param>
        /// <returns>The account signer</returns>
        public static AccountSigner CalledByEntry(Hash160 accountHash)
        {
            if (accountHash == null)
                throw new ArgumentNullException(nameof(accountHash));
                
            var account = Account.FromAddress(accountHash.ToAddress());
            return new AccountSigner(account, WitnessScope.CalledByEntry);
        }
        
        /// <summary>
        /// Creates a signer for the given account with global witness scope (WitnessScope.Global).
        /// This allows any contract to use this signer's witness.
        /// WARNING: Use with extreme caution as this gives unrestricted access to the signature.
        /// </summary>
        /// <param name="account">The signer account</param>
        /// <returns>The account signer</returns>
        public static AccountSigner Global(Account account)
        {
            if (account == null)
                throw new ArgumentNullException(nameof(account));
                
            return new AccountSigner(account, WitnessScope.Global);
        }
        
        /// <summary>
        /// Creates a signer for the given account hash with global witness scope (WitnessScope.Global).
        /// This allows any contract to use this signer's witness.
        /// WARNING: Use with extreme caution as this gives unrestricted access to the signature.
        /// </summary>
        /// <param name="accountHash">The script hash of the signer account</param>
        /// <returns>The account signer</returns>
        public static AccountSigner Global(Hash160 accountHash)
        {
            if (accountHash == null)
                throw new ArgumentNullException(nameof(accountHash));
                
            var account = Account.FromAddress(accountHash.ToAddress());
            return new AccountSigner(account, WitnessScope.Global);
        }
        
        /// <summary>
        /// Creates a signer for the given account with custom witness scope.
        /// This allows fine-grained control over which contracts can use the witness.
        /// </summary>
        /// <param name="account">The signer account</param>
        /// <param name="scope">The custom witness scope</param>
        /// <returns>The account signer</returns>
        public static AccountSigner Custom(Account account, WitnessScope scope)
        {
            if (account == null)
                throw new ArgumentNullException(nameof(account));
                
            return new AccountSigner(account, scope);
        }
        
        #endregion
        
        #region Unity Integration Methods
        
        /// <summary>
        /// Gets the address of the signer account.
        /// </summary>
        /// <returns>The Neo address string</returns>
        public string GetAddress()
        {
            return Account?.Address ?? SignerHash?.ToAddress();
        }
        
        /// <summary>
        /// Checks if this signer can be used for fee payment.
        /// </summary>
        /// <returns>True if this signer can pay fees</returns>
        public bool CanPayFees()
        {
            return Scopes.Contains(WitnessScope.None) || !Scopes.Contains(WitnessScope.None);
        }
        
        /// <summary>
        /// Checks if this signer allows global contract access.
        /// </summary>
        /// <returns>True if global access is allowed</returns>
        public bool HasGlobalAccess()
        {
            return Scopes.Contains(WitnessScope.Global);
        }
        
        /// <summary>
        /// Checks if the account has a private key available for signing.
        /// </summary>
        /// <returns>True if the account can sign transactions</returns>
        public bool CanSign()
        {
            return Account?.KeyPair != null;
        }
        
        /// <summary>
        /// Gets the verification script for this account if available.
        /// </summary>
        /// <returns>The verification script or null if not available</returns>
        public VerificationScript GetVerificationScript()
        {
            return Account?.VerificationScript;
        }
        
        #endregion
        
        #region Validation Methods
        
        /// <summary>
        /// Validates the signer configuration.
        /// </summary>
        /// <exception cref="InvalidOperationException">If configuration is invalid</exception>
        public void Validate()
        {
            if (Account == null)
            {
                throw new InvalidOperationException("Account signer must have a valid account.");
            }
            
            if (SignerHash == null || !SignerHash.Equals(Account.GetScriptHash()))
            {
                throw new InvalidOperationException("Signer hash must match the account script hash.");
            }
            
            if (Scopes == null || Scopes.Count == 0)
            {
                throw new InvalidOperationException("Account signer must have at least one witness scope.");
            }
            
            // Validate scope combinations
            if (Scopes.Contains(WitnessScope.Global) && Scopes.Count > 1)
            {
                throw new InvalidOperationException("Global witness scope cannot be combined with other scopes.");
            }
            
            if (Scopes.Contains(WitnessScope.None) && Scopes.Count > 1)
            {
                throw new InvalidOperationException("None witness scope cannot be combined with other scopes.");
            }
        }
        
        #endregion
        
        #region String Representation
        
        /// <summary>
        /// Returns a string representation of this account signer.
        /// </summary>
        /// <returns>String representation</returns>
        public override string ToString()
        {
            var scopeStr = string.Join(", ", Scopes);
            var address = GetAddress();
            var canSign = CanSign() ? "Can Sign" : "Cannot Sign";
            
            return $"AccountSigner(Address: {address}, Scopes: {scopeStr}, {canSign})";
        }
        
        #endregion
        
        #region Equality Override
        
        /// <summary>
        /// Determines whether the specified object is equal to the current account signer.
        /// </summary>
        /// <param name="obj">The object to compare</param>
        /// <returns>True if equal, false otherwise</returns>
        public override bool Equals(object obj)
        {
            if (obj is AccountSigner other)
            {
                return base.Equals(other) && 
                       ((Account == null && other.Account == null) || 
                        (Account != null && Account.Equals(other.Account)));
            }
            return false;
        }
        
        /// <summary>
        /// Returns a hash code for the current account signer.
        /// </summary>
        /// <returns>Hash code</returns>
        public override int GetHashCode()
        {
            var hashCode = base.GetHashCode();
            if (Account != null)
            {
                hashCode = HashCode.Combine(hashCode, Account.GetHashCode());
            }
            return hashCode;
        }
        
        #endregion
    }
}
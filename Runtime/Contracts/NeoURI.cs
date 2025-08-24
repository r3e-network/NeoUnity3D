using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Neo.Unity.SDK.Types;
using Neo.Unity.SDK.Core;
using Neo.Unity.SDK.Contracts.Native;
using Neo.Unity.SDK.Wallet;
using Neo.Unity.SDK.Transaction;
using UnityEngine;

namespace Neo.Unity.SDK.Contracts
{
    /// <summary>
    /// Wrapper class to generate NEP-9 compatible URI schemes for NEP-17 Token transfers.
    /// NEP-9 defines a URI scheme to enable payments in Neo ecosystem applications.
    /// </summary>
    public class NeoURI
    {
        #region Constants
        
        private const string NEO_SCHEME = "neo";
        private const int MIN_NEP9_URI_LENGTH = 38;
        private const string NEO_TOKEN_STRING = "neo";
        private const string GAS_TOKEN_STRING = "gas";
        
        #endregion
        
        #region Properties
        
        /// <summary>
        /// The NEP-9 URI of this NeoURI.
        /// </summary>
        public Uri Uri { get; private set; }
        
        /// <summary>
        /// The NeoUnity instance for blockchain operations.
        /// </summary>
        public INeo NeoUnity { get; private set; }
        
        /// <summary>
        /// The script hash of the recipient address.
        /// </summary>
        public Hash160 Recipient { get; private set; }
        
        /// <summary>
        /// The token script hash.
        /// </summary>
        public Hash160 Token { get; private set; }
        
        /// <summary>
        /// The transfer amount.
        /// </summary>
        public decimal? Amount { get; private set; }
        
        /// <summary>
        /// The NEP-9 URI of this NeoURI as string.
        /// </summary>
        public string UriString => Uri?.ToString();
        
        /// <summary>
        /// The recipient address as a Neo address string.
        /// </summary>
        public string RecipientAddress => Recipient?.ToAddress();
        
        /// <summary>
        /// The token as a string (returns 'neo', 'gas', or script hash).
        /// </summary>
        public string TokenString
        {
            get
            {
                if (Token == null) return null;
                
                if (Token == NeoToken.SCRIPT_HASH) return NEO_TOKEN_STRING;
                if (Token == GasToken.SCRIPT_HASH) return GAS_TOKEN_STRING;
                return Token.ToString();
            }
        }
        
        /// <summary>
        /// The token as an address string.
        /// </summary>
        public string TokenAddress => Token?.ToAddress();
        
        /// <summary>
        /// The amount as a string.
        /// </summary>
        public string AmountString => Amount?.ToString();
        
        #endregion
        
        #region Constructors
        
        /// <summary>
        /// Creates a new NeoURI instance.
        /// </summary>
        public NeoURI()
        {
        }
        
        /// <summary>
        /// Creates a new NeoURI instance with a NeoUnity reference.
        /// </summary>
        /// <param name="neoUnity">The NeoUnity instance for blockchain operations</param>
        public NeoURI(INeo neoUnity)
        {
            NeoUnity = neoUnity;
        }
        
        #endregion
        
        #region Static Factory Methods
        
        /// <summary>
        /// Creates a NeoURI from a NEP-9 URI String.
        /// </summary>
        /// <param name="uriString">A NEP-9 URI String</param>
        /// <returns>A NeoURI object</returns>
        /// <exception cref="ArgumentException">Thrown when the URI string is invalid</exception>
        public static NeoURI FromUri(string uriString)
        {
            if (string.IsNullOrEmpty(uriString))
                throw new ArgumentException("URI string cannot be null or empty", nameof(uriString));
            
            if (uriString.Length < MIN_NEP9_URI_LENGTH)
                throw new ArgumentException("The provided string does not conform to the NEP-9 standard.", nameof(uriString));
            
            var baseAndQuery = uriString.Split('?');
            var schemeParts = baseAndQuery[0].Split(':');
            
            if (schemeParts.Length != 2 || schemeParts[0] != NEO_SCHEME)
                throw new ArgumentException("The provided string does not conform to the NEP-9 standard.", nameof(uriString));
            
            var neoURI = new NeoURI().To(Hash160.FromAddress(schemeParts[1]));
            
            if (baseAndQuery.Length == 2)
            {
                var queries = baseAndQuery[1].Split('&');
                
                foreach (var query in queries)
                {
                    var parts = query.Split('=');
                    if (parts.Length != 2)
                        throw new ArgumentException("This URI contains invalid queries.", nameof(uriString));
                    
                    switch (parts[0])
                    {
                        case "asset" when neoURI.Token == null:
                            neoURI.SetToken(parts[1]);
                            break;
                        case "amount" when neoURI.Amount == null:
                            if (decimal.TryParse(parts[1], out var amount))
                                neoURI.Amount = amount;
                            break;
                    }
                }
            }
            
            return neoURI;
        }
        
        #endregion
        
        #region Builder Methods
        
        /// <summary>
        /// Sets the recipient's script hash.
        /// </summary>
        /// <param name="recipient">The recipient's script hash</param>
        /// <returns>This NeoURI object for method chaining</returns>
        public NeoURI To(Hash160 recipient)
        {
            Recipient = recipient ?? throw new ArgumentNullException(nameof(recipient));
            return this;
        }
        
        /// <summary>
        /// Sets the token script hash.
        /// </summary>
        /// <param name="token">The token hash</param>
        /// <returns>This NeoURI object for method chaining</returns>
        public NeoURI SetToken(Hash160 token)
        {
            Token = token ?? throw new ArgumentNullException(nameof(token));
            return this;
        }
        
        /// <summary>
        /// Sets the token from a string (script hash, 'neo', or 'gas').
        /// </summary>
        /// <param name="token">The token hash, 'neo' or 'gas'</param>
        /// <returns>This NeoURI object for method chaining</returns>
        /// <exception cref="ArgumentException">Thrown when the token string is invalid</exception>
        public NeoURI SetToken(string token)
        {
            if (string.IsNullOrEmpty(token))
                throw new ArgumentException("Token cannot be null or empty", nameof(token));
            
            switch (token.ToLowerInvariant())
            {
                case NEO_TOKEN_STRING:
                    Token = NeoToken.SCRIPT_HASH;
                    break;
                case GAS_TOKEN_STRING:
                    Token = GasToken.SCRIPT_HASH;
                    break;
                default:
                    Token = new Hash160(token);
                    break;
            }
            
            return this;
        }
        
        /// <summary>
        /// Sets the transfer amount.
        /// Make sure to use decimals and not token fractions. E.g. for GAS use 1.5 instead of 150_000_000.
        /// </summary>
        /// <param name="amount">The amount in token units (not fractions)</param>
        /// <returns>This NeoURI object for method chaining</returns>
        public NeoURI SetAmount(decimal amount)
        {
            if (amount < 0)
                throw new ArgumentException("Amount cannot be negative", nameof(amount));
            
            Amount = amount;
            return this;
        }
        
        /// <summary>
        /// Sets the NeoUnity instance for blockchain operations.
        /// </summary>
        /// <param name="neoUnity">The NeoUnity instance</param>
        /// <returns>This NeoURI object for method chaining</returns>
        public NeoURI SetNeoUnity(INeo neoUnity)
        {
            NeoUnity = neoUnity ?? throw new ArgumentNullException(nameof(neoUnity));
            return this;
        }
        
        #endregion
        
        #region URI Building
        
        /// <summary>
        /// Builds a NEP-9 URI from the set variables and stores its value in the Uri property.
        /// </summary>
        /// <returns>This NeoURI object for method chaining</returns>
        /// <exception cref="InvalidOperationException">Thrown when required properties are not set</exception>
        public NeoURI BuildUri()
        {
            if (Recipient == null)
                throw new InvalidOperationException("Could not create a NEP-9 URI without a recipient address.");
            
            var baseUri = $"{NEO_SCHEME}:{Recipient.ToAddress()}";
            var queryPart = BuildQueryPart();
            var fullUri = baseUri + (string.IsNullOrEmpty(queryPart) ? "" : $"?{queryPart}");
            
            if (!System.Uri.TryCreate(fullUri, UriKind.Absolute, out var uri))
                throw new InvalidOperationException("Failed to create valid URI from the provided parameters.");
            
            Uri = uri;
            return this;
        }
        
        private string BuildQueryPart()
        {
            var queryParams = new List<string>();
            
            if (Token != null)
            {
                if (Token == NeoToken.SCRIPT_HASH)
                    queryParams.Add($"asset={NEO_TOKEN_STRING}");
                else if (Token == GasToken.SCRIPT_HASH)
                    queryParams.Add($"asset={GAS_TOKEN_STRING}");
                else
                    queryParams.Add($"asset={Token}");
            }
            
            if (Amount.HasValue)
                queryParams.Add($"amount={Amount.Value}");
            
            return string.Join("&", queryParams);
        }
        
        #endregion
        
        #region Transaction Building
        
        /// <summary>
        /// Creates a transaction script to transfer and initializes a TransactionBuilder 
        /// based on this script which is ready to be signed and sent.
        /// </summary>
        /// <param name="sender">The sender account</param>
        /// <returns>A transaction builder ready for signing</returns>
        /// <exception cref="InvalidOperationException">Thrown when required properties are not set</exception>
        /// <exception cref="ArgumentException">Thrown when decimal precision is invalid for the token</exception>
        public async Task<TransactionBuilder> BuildTransferFromAsync(Account sender)
        {
            if (NeoUnity == null)
                throw new InvalidOperationException("NeoUnity instance is not set.");
            if (Recipient == null)
                throw new InvalidOperationException("Recipient is not set.");
            if (!Amount.HasValue)
                throw new InvalidOperationException("Amount is not set.");
            if (Token == null)
                throw new InvalidOperationException("Token is not set.");
            
            var token = new FungibleToken(Token, NeoUnity);
            
            // Validate decimal places for known tokens
            var decimalPlaces = GetDecimalPlaces(Amount.Value);
            
            if (IsNeoToken(Token) && decimalPlaces > NeoToken.DECIMALS)
                throw new ArgumentException("The NEO token does not support any decimal places.");
            
            if (IsGasToken(Token) && decimalPlaces > GasToken.DECIMALS)
                throw new ArgumentException($"The GAS token does not support more than {GasToken.DECIMALS} decimal places.");
            
            // For other tokens, check their actual decimals
            if (!IsNeoToken(Token) && !IsGasToken(Token))
            {
                var tokenDecimals = await token.GetDecimalsAsync();
                if (decimalPlaces > tokenDecimals)
                    throw new ArgumentException($"The {Token} token does not support more than {tokenDecimals} decimal places.");
            }
            
            var tokenFractions = token.ToFractions(Amount.Value);
            return await token.TransferAsync(sender, Recipient, tokenFractions);
        }
        
        #endregion
        
        #region Helper Methods
        
        private static int GetDecimalPlaces(decimal value)
        {
            var bits = decimal.GetBits(value);
            return (bits[3] >> 16) & 0xFF;
        }
        
        private bool IsNeoToken(Hash160 tokenHash)
        {
            return tokenHash == NeoToken.SCRIPT_HASH;
        }
        
        private bool IsGasToken(Hash160 tokenHash)
        {
            return tokenHash == GasToken.SCRIPT_HASH;
        }
        
        #endregion
        
        #region Overrides
        
        /// <summary>
        /// Returns the string representation of this NeoURI.
        /// </summary>
        /// <returns>The URI string or empty string if not built</returns>
        public override string ToString()
        {
            return UriString ?? string.Empty;
        }
        
        /// <summary>
        /// Determines whether the specified object is equal to the current NeoURI.
        /// </summary>
        /// <param name="obj">The object to compare</param>
        /// <returns>True if the objects are equal, false otherwise</returns>
        public override bool Equals(object obj)
        {
            if (obj is NeoURI other)
            {
                return Equals(Recipient, other.Recipient) &&
                       Equals(Token, other.Token) &&
                       Amount == other.Amount;
            }
            return false;
        }
        
        /// <summary>
        /// Returns a hash code for this NeoURI.
        /// </summary>
        /// <returns>A hash code value</returns>
        public override int GetHashCode()
        {
            return HashCode.Combine(Recipient, Token, Amount);
        }
        
        #endregion
    }
}
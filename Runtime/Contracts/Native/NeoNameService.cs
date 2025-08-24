using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Neo.Unity.SDK.Core;
using Neo.Unity.SDK.Types;
using Neo.Unity.SDK.Transaction;
using Neo.Unity.SDK.Wallet;

namespace Neo.Unity.SDK.Contracts.Native
{
    /// <summary>
    /// Represents the official NeoNameService contract and provides methods to invoke its functions.
    /// The Neo Name Service allows users to register human-readable domain names that resolve to Neo addresses and other data.
    /// </summary>
    [System.Serializable]
    public class NeoNameService : NonFungibleToken
    {
        #region Constants

        public const string NAME = "NameService";
        public const string SYMBOL = "NNS";
        public const int DECIMALS = 0;

        // Default NNS contract hash on mainnet
        private static readonly Hash160 DEFAULT_NNS_HASH = new Hash160("0x50ac1c37690cc2cfc594472833cf57505d5f46de");

        // Contract methods
        private const string ADD_ROOT = "addRoot";
        private const string ROOTS = "roots";
        private const string SET_PRICE = "setPrice";
        private const string GET_PRICE = "getPrice";
        private const string IS_AVAILABLE = "isAvailable";
        private const string REGISTER = "register";
        private const string RENEW = "renew";
        private const string SET_ADMIN = "setAdmin";
        private const string SET_RECORD = "setRecord";
        private const string GET_RECORD = "getRecord";
        private const string GET_ALL_RECORDS = "getAllRecords";
        private const string DELETE_RECORD = "deleteRecord";
        private const string RESOLVE = "resolve";

        // Property keys
        private const string NAME_PROPERTY = "name";
        private const string EXPIRATION_PROPERTY = "expiration";
        private const string ADMIN_PROPERTY = "admin";

        #endregion

        #region Constructor

        /// <summary>
        /// Constructs a new NeoNameService instance that uses the given NeoUnity instance for invocations.
        /// Uses the NNS script hash specified in the NeoUnity configuration, or the default mainnet NNS contract.
        /// </summary>
        /// <param name="neoUnity">The NeoUnity instance to use for invocations</param>
        public NeoNameService(NeoUnity neoUnity) : base(GetNNSHash(neoUnity), neoUnity)
        {
        }

        /// <summary>
        /// Constructs a new NeoNameService instance using the singleton NeoUnity instance.
        /// Uses the NNS script hash specified in the configuration, or the default mainnet NNS contract.
        /// </summary>
        public NeoNameService() : base(GetNNSHash(Core.NeoUnity.Instance))
        {
        }

        /// <summary>
        /// Gets the NNS contract hash from configuration or default.
        /// </summary>
        /// <param name="neoUnity">The NeoUnity instance</param>
        /// <returns>The NNS contract hash</returns>
        private static Hash160 GetNNSHash(NeoUnity neoUnity)
        {
            // In a full implementation, this would check NeoUnity configuration for custom NNS hash
            return DEFAULT_NNS_HASH;
        }

        #endregion

        #region Token Metadata Overrides

        /// <summary>
        /// Returns the name of the NeoNameService contract.
        /// Doesn't require a call to the Neo node.
        /// </summary>
        /// <returns>The name</returns>
        public async Task<string> GetName()
        {
            return NAME;
        }

        /// <summary>
        /// Returns the symbol of the NeoNameService contract.
        /// Doesn't require a call to the Neo node.
        /// </summary>
        /// <returns>The symbol</returns>
        public override async Task<string> GetSymbol()
        {
            return SYMBOL;
        }

        /// <summary>
        /// Returns the decimals of the NeoNameService contract.
        /// Doesn't require a call to the Neo node.
        /// </summary>
        /// <returns>The decimals</returns>
        public override async Task<int> GetDecimals()
        {
            return DECIMALS;
        }

        #endregion

        #region NEP-11 Overrides for Domain Names

        /// <summary>
        /// Gets the owner of the domain name.
        /// </summary>
        /// <param name="domainName">The domain name</param>
        /// <returns>The owner of the domain name</returns>
        public async Task<Hash160> GetOwnerOf(string domainName)
        {
            if (string.IsNullOrEmpty(domainName))
            {
                throw new ArgumentException("Domain name cannot be null or empty.", nameof(domainName));
            }

            var domainBytes = System.Text.Encoding.UTF8.GetBytes(domainName);
            return await GetOwnerOf(domainBytes);
        }

        /// <summary>
        /// Gets the properties of the domain name.
        /// </summary>
        /// <param name="domainName">The domain name</param>
        /// <returns>The properties of the domain name</returns>
        public async Task<Dictionary<string, string>> GetProperties(string domainName)
        {
            if (string.IsNullOrEmpty(domainName))
            {
                throw new ArgumentException("Domain name cannot be null or empty.", nameof(domainName));
            }

            var domainBytes = System.Text.Encoding.UTF8.GetBytes(domainName);
            return await GetProperties(domainBytes);
        }

        /// <summary>
        /// Creates a transaction script to transfer a domain name and initializes a TransactionBuilder based on this script.
        /// </summary>
        /// <param name="from">The current owner of the domain name</param>
        /// <param name="to">The recipient of the domain name</param>
        /// <param name="domainName">The domain name to transfer</param>
        /// <param name="data">Optional data for the transfer</param>
        /// <returns>A transaction builder ready for signing</returns>
        public async Task<TransactionBuilder> TransferDomain(Account from, Hash160 to, string domainName, ContractParameter data = null)
        {
            await CheckDomainAvailability(domainName, false);
            return await Transfer(from, to, domainName, data);
        }

        #endregion

        #region Root Domain Management

        /// <summary>
        /// Creates a transaction script to add a root domain (like .neo) and initializes a TransactionBuilder based on this script.
        /// Only the committee is allowed to add a new root domain.
        /// Requires to be signed by the committee.
        /// </summary>
        /// <param name="rootDomain">The new root domain</param>
        /// <returns>A transaction builder ready for committee signing</returns>
        public async Task<TransactionBuilder> AddRoot(string rootDomain)
        {
            if (string.IsNullOrEmpty(rootDomain))
            {
                throw new ArgumentException("Root domain cannot be null or empty.", nameof(rootDomain));
            }

            if (NeoUnity.Config.EnableDebugLogging)
            {
                Debug.Log($"[NeoNameService] Adding root domain: {rootDomain}");
            }

            return await InvokeFunction(ADD_ROOT, ContractParameter.String(rootDomain));
        }

        /// <summary>
        /// Gets all existing root domains using an iterator.
        /// This method requires sessions to be enabled on the Neo node.
        /// </summary>
        /// <returns>An iterator of root domain strings</returns>
        public async Task<Iterator<string>> GetRoots()
        {
            return await CallFunctionReturningIterator<string>(ROOTS, null, item => item.GetString());
        }

        /// <summary>
        /// Gets all existing root domains without using sessions.
        /// Use this method if sessions are disabled on the Neo node.
        /// </summary>
        /// <param name="count">Maximum number of roots to retrieve</param>
        /// <returns>List of root domain strings</returns>
        public async Task<List<string>> GetRootsUnwrapped(int count = DEFAULT_ITERATOR_COUNT)
        {
            var stackItems = await CallFunctionAndUnwrapIterator(ROOTS, new ContractParameter[0], count);
            var roots = new List<string>();

            foreach (var item in stackItems)
            {
                roots.Add(item.GetString());
            }

            if (NeoUnity.Config.EnableDebugLogging)
            {
                Debug.Log($"[NeoNameService] Retrieved {roots.Count} root domains");
            }

            return roots;
        }

        #endregion

        #region Price Management

        /// <summary>
        /// Creates a transaction script to set the prices for registering domains and initializes a TransactionBuilder based on this script.
        /// Only the committee is allowed to set prices.
        /// </summary>
        /// <param name="priceList">The prices for registering domains. Index refers to domain name length.</param>
        /// <returns>A transaction builder ready for committee signing</returns>
        public async Task<TransactionBuilder> SetPrice(List<long> priceList)
        {
            if (priceList == null || priceList.Count == 0)
            {
                throw new ArgumentException("Price list cannot be null or empty.", nameof(priceList));
            }

            var priceParameters = new List<ContractParameter>();
            foreach (var price in priceList)
            {
                priceParameters.Add(ContractParameter.Integer(price));
            }

            return await InvokeFunction(SET_PRICE, ContractParameter.Array(priceParameters.ToArray()));
        }

        /// <summary>
        /// Gets the price to register a domain name of a certain length.
        /// </summary>
        /// <param name="domainNameLength">The length of the domain name</param>
        /// <returns>The price to register a domain in GAS fractions</returns>
        public async Task<long> GetPrice(int domainNameLength)
        {
            if (domainNameLength <= 0)
            {
                throw new ArgumentException("Domain name length must be positive.", nameof(domainNameLength));
            }

            var result = await CallFunctionReturningInt(GET_PRICE, ContractParameter.Integer(domainNameLength));

            if (NeoUnity.Config.EnableDebugLogging)
            {
                Debug.Log($"[NeoNameService] Price for {domainNameLength}-character domain: {result} GAS fractions");
            }

            return result;
        }

        #endregion

        #region Domain Registration & Management

        /// <summary>
        /// Checks if the specified domain name is available for registration.
        /// </summary>
        /// <param name="domainName">The domain name to check</param>
        /// <returns>True if the domain name is available, otherwise false</returns>
        public async Task<bool> IsAvailable(string domainName)
        {
            if (string.IsNullOrEmpty(domainName))
            {
                throw new ArgumentException("Domain name cannot be null or empty.", nameof(domainName));
            }

            var result = await CallFunctionReturningBool(IS_AVAILABLE, ContractParameter.String(domainName));

            if (NeoUnity.Config.EnableDebugLogging)
            {
                Debug.Log($"[NeoNameService] Domain '{domainName}' availability: {result}");
            }

            return result;
        }

        /// <summary>
        /// Creates a transaction script to register a new domain name and initializes a TransactionBuilder based on this script.
        /// </summary>
        /// <param name="domainName">The domain name to register</param>
        /// <param name="owner">The script hash of the domain owner</param>
        /// <returns>A transaction builder ready for signing</returns>
        public async Task<TransactionBuilder> Register(string domainName, Hash160 owner)
        {
            if (string.IsNullOrEmpty(domainName))
            {
                throw new ArgumentException("Domain name cannot be null or empty.", nameof(domainName));
            }

            if (owner == null)
            {
                throw new ArgumentNullException(nameof(owner));
            }

            await CheckDomainAvailability(domainName, true);

            if (NeoUnity.Config.EnableDebugLogging)
            {
                Debug.Log($"[NeoNameService] Registering domain '{domainName}' for owner: {owner}");
            }

            return await InvokeFunction(REGISTER, 
                ContractParameter.String(domainName), 
                ContractParameter.Hash160(owner));
        }

        /// <summary>
        /// Creates a transaction script to register a new domain name for an account.
        /// </summary>
        /// <param name="domainName">The domain name to register</param>
        /// <param name="owner">The account that will own the domain</param>
        /// <returns>A transaction builder ready for signing</returns>
        public async Task<TransactionBuilder> Register(string domainName, Account owner)
        {
            if (owner == null)
            {
                throw new ArgumentNullException(nameof(owner));
            }

            return await Register(domainName, owner.GetScriptHash());
        }

        /// <summary>
        /// Creates a transaction script to renew a domain name for one year and initializes a TransactionBuilder based on this script.
        /// </summary>
        /// <param name="domainName">The domain name to renew</param>
        /// <returns>A transaction builder ready for signing</returns>
        public async Task<TransactionBuilder> Renew(string domainName)
        {
            await CheckDomainAvailability(domainName, false);
            return await InvokeFunction(RENEW, ContractParameter.String(domainName));
        }

        /// <summary>
        /// Creates a transaction script to renew a domain name for a specified number of years.
        /// </summary>
        /// <param name="domainName">The domain name to renew</param>
        /// <param name="years">The number of years to renew (1-10)</param>
        /// <returns>A transaction builder ready for signing</returns>
        public async Task<TransactionBuilder> Renew(string domainName, int years)
        {
            if (years <= 0 || years > 10)
            {
                throw new ArgumentException("Domain names can only be renewed by at least 1, and at most 10 years.", nameof(years));
            }

            await CheckDomainAvailability(domainName, false);
            return await InvokeFunction(RENEW, 
                ContractParameter.String(domainName), 
                ContractParameter.Integer(years));
        }

        /// <summary>
        /// Creates a transaction script to set the admin for a domain name.
        /// Requires to be signed by the current owner and the new admin.
        /// </summary>
        /// <param name="domainName">The domain name</param>
        /// <param name="admin">The script hash of the new admin</param>
        /// <returns>A transaction builder ready for signing</returns>
        public async Task<TransactionBuilder> SetAdmin(string domainName, Hash160 admin)
        {
            if (string.IsNullOrEmpty(domainName))
            {
                throw new ArgumentException("Domain name cannot be null or empty.", nameof(domainName));
            }

            if (admin == null)
            {
                throw new ArgumentNullException(nameof(admin));
            }

            await CheckDomainAvailability(domainName, false);
            return await InvokeFunction(SET_ADMIN, 
                ContractParameter.String(domainName), 
                ContractParameter.Hash160(admin));
        }

        #endregion

        #region Record Management

        /// <summary>
        /// Creates a transaction script to set a DNS record for a domain name.
        /// Requires to be signed by the domain owner or admin.
        /// </summary>
        /// <param name="domainName">The domain name</param>
        /// <param name="recordType">The DNS record type</param>
        /// <param name="data">The record data</param>
        /// <returns>A transaction builder ready for signing</returns>
        public async Task<TransactionBuilder> SetRecord(string domainName, RecordType recordType, string data)
        {
            if (string.IsNullOrEmpty(domainName))
            {
                throw new ArgumentException("Domain name cannot be null or empty.", nameof(domainName));
            }

            if (string.IsNullOrEmpty(data))
            {
                throw new ArgumentException("Record data cannot be null or empty.", nameof(data));
            }

            return await InvokeFunction(SET_RECORD,
                ContractParameter.String(domainName),
                ContractParameter.Integer((int)recordType),
                ContractParameter.String(data));
        }

        /// <summary>
        /// Gets a DNS record for a domain name.
        /// </summary>
        /// <param name="domainName">The domain name</param>
        /// <param name="recordType">The DNS record type</param>
        /// <returns>The record data</returns>
        public async Task<string> GetRecord(string domainName, RecordType recordType)
        {
            if (string.IsNullOrEmpty(domainName))
            {
                throw new ArgumentException("Domain name cannot be null or empty.", nameof(domainName));
            }

            try
            {
                var result = await CallFunctionReturningString(GET_RECORD,
                    ContractParameter.String(domainName),
                    ContractParameter.Integer((int)recordType));

                if (NeoUnity.Config.EnableDebugLogging)
                {
                    Debug.Log($"[NeoNameService] Record {recordType} for '{domainName}': {result}");
                }

                return result;
            }
            catch (Exception ex)
            {
                throw new ContractException($"Could not get a record of type '{recordType}' for the domain name '{domainName}': {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Gets all DNS records for a domain name using an iterator.
        /// </summary>
        /// <param name="domainName">The domain name</param>
        /// <returns>An iterator of record states</returns>
        public async Task<Iterator<RecordState>> GetAllRecords(string domainName)
        {
            if (string.IsNullOrEmpty(domainName))
            {
                throw new ArgumentException("Domain name cannot be null or empty.", nameof(domainName));
            }

            return await CallFunctionReturningIterator<RecordState>(GET_ALL_RECORDS,
                new[] { ContractParameter.String(domainName) },
                MapRecordState);
        }

        /// <summary>
        /// Gets all DNS records for a domain name without using sessions.
        /// </summary>
        /// <param name="domainName">The domain name</param>
        /// <param name="count">Maximum number of records to retrieve</param>
        /// <returns>List of record states</returns>
        public async Task<List<RecordState>> GetAllRecordsUnwrapped(string domainName, int count = DEFAULT_ITERATOR_COUNT)
        {
            if (string.IsNullOrEmpty(domainName))
            {
                throw new ArgumentException("Domain name cannot be null or empty.", nameof(domainName));
            }

            var stackItems = await CallFunctionAndUnwrapIterator(GET_ALL_RECORDS,
                new[] { ContractParameter.String(domainName) }, count);

            var records = new List<RecordState>();
            foreach (var item in stackItems)
            {
                records.Add(MapRecordState(item));
            }

            return records;
        }

        /// <summary>
        /// Maps a stack item to a RecordState object.
        /// </summary>
        /// <param name="stackItem">The stack item containing record data</param>
        /// <returns>The mapped record state</returns>
        private RecordState MapRecordState(StackItem stackItem)
        {
            if (!stackItem.IsArray())
            {
                throw new ContractException("Record state must be an array");
            }

            var array = stackItem.GetList();
            if (array.Count < 2)
            {
                throw new ContractException("Record state array must have at least 2 elements");
            }

            var type = (RecordType)array[0].GetInteger();
            var data = array[1].GetString();

            return new RecordState(type, data);
        }

        /// <summary>
        /// Creates a transaction script to delete a DNS record for a domain name.
        /// Requires to be signed by the domain owner or admin.
        /// </summary>
        /// <param name="domainName">The domain name</param>
        /// <param name="recordType">The DNS record type to delete</param>
        /// <returns>A transaction builder ready for signing</returns>
        public async Task<TransactionBuilder> DeleteRecord(string domainName, RecordType recordType)
        {
            if (string.IsNullOrEmpty(domainName))
            {
                throw new ArgumentException("Domain name cannot be null or empty.", nameof(domainName));
            }

            return await InvokeFunction(DELETE_RECORD,
                ContractParameter.String(domainName),
                ContractParameter.Integer((int)recordType));
        }

        /// <summary>
        /// Resolves a domain name to get its record data.
        /// </summary>
        /// <param name="domainName">The domain name to resolve</param>
        /// <param name="recordType">The DNS record type to resolve</param>
        /// <returns>The resolved data</returns>
        public async Task<string> Resolve(string domainName, RecordType recordType)
        {
            if (string.IsNullOrEmpty(domainName))
            {
                throw new ArgumentException("Domain name cannot be null or empty.", nameof(domainName));
            }

            try
            {
                var result = await CallFunctionReturningString(RESOLVE,
                    ContractParameter.String(domainName),
                    ContractParameter.Integer((int)recordType));

                if (NeoUnity.Config.EnableDebugLogging)
                {
                    Debug.Log($"[NeoNameService] Resolved '{domainName}' ({recordType}): {result}");
                }

                return result;
            }
            catch (Exception ex)
            {
                throw new ContractException($"Unresolvable domain name '{domainName}': {ex.Message}", ex);
            }
        }

        #endregion

        #region Domain State

        /// <summary>
        /// Gets the state of a domain name (includes name, expiration, admin info).
        /// </summary>
        /// <param name="domainName">The domain name</param>
        /// <returns>The domain name state</returns>
        public async Task<NameState> GetNameState(string domainName)
        {
            if (string.IsNullOrEmpty(domainName))
            {
                throw new ArgumentException("Domain name cannot be null or empty.", nameof(domainName));
            }

            var properties = await GetProperties(domainName);

            if (!properties.TryGetValue(NAME_PROPERTY, out var name))
            {
                throw new ContractException("'name' property not found in domain state");
            }

            if (!properties.TryGetValue(EXPIRATION_PROPERTY, out var expirationString) ||
                !long.TryParse(expirationString, out var expiration))
            {
                throw new ContractException("'expiration' property not found or invalid in domain state");
            }

            Hash160 admin = null;
            if (properties.TryGetValue(ADMIN_PROPERTY, out var adminString) && !string.IsNullOrEmpty(adminString))
            {
                try
                {
                    admin = Hash160.FromAddress(adminString);
                }
                catch
                {
                    // Admin might be stored as hex, try that
                    try
                    {
                        admin = new Hash160(adminString);
                    }
                    catch
                    {
                        // Ignore if admin can't be parsed
                    }
                }
            }

            return new NameState(name, expiration, admin);
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Checks domain availability and throws appropriate exceptions.
        /// </summary>
        /// <param name="domainName">The domain name to check</param>
        /// <param name="shouldBeAvailable">Whether the domain should be available</param>
        /// <exception cref="ContractException">Thrown if availability doesn't match expectation</exception>
        private async Task CheckDomainAvailability(string domainName, bool shouldBeAvailable)
        {
            var isAvailable = await IsAvailable(domainName);
            
            if (shouldBeAvailable && !isAvailable)
            {
                throw new ContractException($"The domain name '{domainName}' is already taken.");
            }
            else if (!shouldBeAvailable && isAvailable)
            {
                throw new ContractException($"The domain name '{domainName}' is not registered.");
            }
        }

        #endregion

        #region Unity Integration Helpers

        /// <summary>
        /// Estimates the cost to register a domain name.
        /// </summary>
        /// <param name="domainName">The domain name to register</param>
        /// <returns>The estimated registration cost in GAS fractions</returns>
        public async Task<long> EstimateRegistrationCost(string domainName)
        {
            if (string.IsNullOrEmpty(domainName))
            {
                throw new ArgumentException("Domain name cannot be null or empty.", nameof(domainName));
            }

            return await GetPrice(domainName.Length);
        }

        /// <summary>
        /// Gets a user-friendly domain status for display.
        /// </summary>
        /// <param name="domainName">The domain name to check</param>
        /// <returns>Domain status information</returns>
        public async Task<DomainStatus> GetDomainStatus(string domainName)
        {
            if (string.IsNullOrEmpty(domainName))
            {
                throw new ArgumentException("Domain name cannot be null or empty.", nameof(domainName));
            }

            var status = new DomainStatus { DomainName = domainName };

            try
            {
                status.IsAvailable = await IsAvailable(domainName);
                
                if (!status.IsAvailable)
                {
                    status.Owner = await GetOwnerOf(domainName);
                    status.NameState = await GetNameState(domainName);
                }

                status.RegistrationCost = await EstimateRegistrationCost(domainName);
            }
            catch (Exception ex)
            {
                status.ErrorMessage = ex.Message;
            }

            return status;
        }

        #endregion
    }

    /// <summary>
    /// Represents the state of a domain name record.
    /// </summary>
    [System.Serializable]
    public class RecordState
    {
        /// <summary>The record type</summary>
        public RecordType Type { get; private set; }

        /// <summary>The record data</summary>
        public string Data { get; private set; }

        /// <summary>
        /// Creates a new record state.
        /// </summary>
        /// <param name="type">The record type</param>
        /// <param name="data">The record data</param>
        public RecordState(RecordType type, string data)
        {
            Type = type;
            Data = data ?? throw new ArgumentNullException(nameof(data));
        }

        public override string ToString()
        {
            return $"RecordState({Type}: {Data})";
        }
    }

    /// <summary>
    /// Represents the state of a domain name.
    /// </summary>
    [System.Serializable]
    public class NameState
    {
        /// <summary>The domain name</summary>
        public string Name { get; private set; }

        /// <summary>The expiration timestamp</summary>
        public long Expiration { get; private set; }

        /// <summary>The admin script hash (null if no admin)</summary>
        public Hash160 Admin { get; private set; }

        /// <summary>Whether the domain has an admin</summary>
        public bool HasAdmin => Admin != null;

        /// <summary>Whether the domain is expired</summary>
        public bool IsExpired => DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() > Expiration;

        /// <summary>
        /// Creates a new name state.
        /// </summary>
        /// <param name="name">The domain name</param>
        /// <param name="expiration">The expiration timestamp</param>
        /// <param name="admin">The admin script hash (optional)</param>
        public NameState(string name, long expiration, Hash160 admin = null)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Expiration = expiration;
            Admin = admin;
        }

        /// <summary>
        /// Gets the expiration date as DateTime.
        /// </summary>
        /// <returns>The expiration date</returns>
        public DateTime GetExpirationDate()
        {
            return DateTimeOffset.FromUnixTimeMilliseconds(Expiration).DateTime;
        }

        /// <summary>
        /// Gets the time remaining until expiration.
        /// </summary>
        /// <returns>Time remaining, or TimeSpan.Zero if expired</returns>
        public TimeSpan GetTimeUntilExpiration()
        {
            var expiration = GetExpirationDate();
            var remaining = expiration - DateTime.UtcNow;
            return remaining > TimeSpan.Zero ? remaining : TimeSpan.Zero;
        }

        public override string ToString()
        {
            var adminInfo = HasAdmin ? $", Admin: {Admin}" : "";
            var expirationInfo = IsExpired ? "EXPIRED" : $"Expires: {GetExpirationDate():yyyy-MM-dd}";
            return $"NameState({Name}, {expirationInfo}{adminInfo})";
        }
    }

    /// <summary>
    /// Represents the status information for a domain name.
    /// </summary>
    [System.Serializable]
    public class DomainStatus
    {
        /// <summary>The domain name</summary>
        public string DomainName { get; set; }

        /// <summary>Whether the domain is available for registration</summary>
        public bool IsAvailable { get; set; }

        /// <summary>The owner of the domain (if not available)</summary>
        public Hash160 Owner { get; set; }

        /// <summary>The domain name state (if not available)</summary>
        public NameState NameState { get; set; }

        /// <summary>The cost to register this domain</summary>
        public long RegistrationCost { get; set; }

        /// <summary>Any error message during status retrieval</summary>
        public string ErrorMessage { get; set; }

        /// <summary>Whether there was an error getting the status</summary>
        public bool HasError => !string.IsNullOrEmpty(ErrorMessage);

        /// <summary>
        /// Gets the registration cost in decimal GAS format.
        /// </summary>
        /// <returns>Registration cost in decimal GAS</returns>
        public decimal GetRegistrationCostGas()
        {
            return GasToken.FromGasFractions(RegistrationCost);
        }

        /// <summary>
        /// Gets a user-friendly status description.
        /// </summary>
        /// <returns>Status description</returns>
        public string GetStatusDescription()
        {
            if (HasError) return $"Error: {ErrorMessage}";
            if (IsAvailable) return $"Available (Cost: {GetRegistrationCostGas():F8} GAS)";
            
            var ownerInfo = Owner != null ? $"Owner: {Owner.ToAddress()}" : "Owned";
            var expirationInfo = NameState?.IsExpired == true ? " (EXPIRED)" : "";
            return $"Registered ({ownerInfo}){expirationInfo}";
        }

        public override string ToString()
        {
            return $"DomainStatus({DomainName}: {GetStatusDescription()})";
        }
    }
}
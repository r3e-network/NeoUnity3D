using System;
using System.Numerics;
using UnityEngine;

namespace Neo.Unity.SDK.Types
{
    /// <summary>
    /// Contains fundamental constants used throughout the Neo blockchain ecosystem.
    /// These values are derived from the Neo protocol specification and core implementations.
    /// </summary>
    public static class NeoConstants
    {
        // MARK: Accounts, Addresses, Keys

        /// <summary>
        /// The maximum number of public keys that can take part in a multi-signature address.
        /// Taken from Neo.SmartContract.Contract.CreateMultiSigRedeemScript(...) in the C# neo repo.
        /// </summary>
        public const int MAX_PUBLIC_KEYS_PER_MULTISIG_ACCOUNT = 1024;

        /// <summary>
        /// The byte size of a Hash160 hash.
        /// </summary>
        public const int HASH160_SIZE = 20;

        /// <summary>
        /// The byte size of a Hash256 hash.
        /// </summary>
        public const int HASH256_SIZE = 32;

        /// <summary>
        /// Size of a private key in bytes.
        /// </summary>
        public const int PRIVATE_KEY_SIZE = 32;

        /// <summary>
        /// Size of a compressed public key in bytes.
        /// </summary>
        public const int PUBLIC_KEY_SIZE_COMPRESSED = 33;

        /// <summary>
        /// Size of an uncompressed public key in bytes.
        /// </summary>
        public const int PUBLIC_KEY_SIZE_UNCOMPRESSED = 65;

        /// <summary>
        /// Size of a signature in bytes.
        /// </summary>
        public const int SIGNATURE_SIZE = 64;

        /// <summary>
        /// Size of a single signature verification script in bytes.
        /// 1 (PUSHDATA OpCode) + 1 (byte for data length) + 33 (public key) + 1 (SYSCALL Opcode) + 4 (InteropServiceCode) = 40
        /// </summary>
        public const int VERIFICATION_SCRIPT_SIZE = 40;

        /// <summary>
        /// Standard address version for Neo addresses.
        /// </summary>
        public const byte ADDRESS_VERSION = 0x35;

        // MARK: Transactions & Contracts

        /// <summary>
        /// The current version used for Neo transactions.
        /// </summary>
        public const byte CURRENT_TX_VERSION = 0;

        /// <summary>
        /// The maximum size of a transaction in bytes.
        /// </summary>
        public const int MAX_TRANSACTION_SIZE = 102400;

        /// <summary>
        /// The maximum number of attributes that a transaction can have.
        /// </summary>
        public const int MAX_TRANSACTION_ATTRIBUTES = 16;

        /// <summary>
        /// The maximum number of contracts or groups a signer scope can contain.
        /// </summary>
        public const int MAX_SIGNER_SUBITEMS = 16;

        /// <summary>
        /// The maximum byte length for a valid contract manifest.
        /// </summary>
        public const int MAX_MANIFEST_SIZE = 0xFFFF;

        /// <summary>
        /// The default maximum number of iterator items returned in an RPC response.
        /// </summary>
        public const int MAX_ITERATOR_ITEMS_DEFAULT = 100;

        /// <summary>
        /// The maximum number of contract invocation stack items.
        /// </summary>
        public const int MAX_STACK_SIZE = 2048;

        /// <summary>
        /// The maximum size of a script in bytes.
        /// </summary>
        public const int MAX_SCRIPT_LENGTH = 1024 * 1024; // 1MB

        /// <summary>
        /// The default network fee per byte.
        /// </summary>
        public const long DEFAULT_NETWORK_FEE_PER_BYTE = 1000;

        // MARK: Blockchain & Network

        /// <summary>
        /// The maximum number of transactions per block.
        /// </summary>
        public const int MAX_TRANSACTIONS_PER_BLOCK = 512;

        /// <summary>
        /// The target time between blocks in milliseconds (15 seconds).
        /// </summary>
        public const int BLOCK_TIME_MS = 15000;

        /// <summary>
        /// The number of blocks before a transaction expires if not included.
        /// </summary>
        public const int MAX_VALID_UNTIL_BLOCK_INCREMENT = 86400; // Roughly 15 days at 15s per block

        /// <summary>
        /// The maximum number of validators that can be active at once.
        /// </summary>
        public const int MAX_VALIDATORS_COUNT = 1024;

        /// <summary>
        /// The minimum number of committee members.
        /// </summary>
        public const int MIN_COMMITTEE_SIZE = 4;

        /// <summary>
        /// The maximum number of committee members.
        /// </summary>
        public const int MAX_COMMITTEE_SIZE = 1024;

        // MARK: Native Contracts

        /// <summary>
        /// The decimals for the NEO token (NEO is indivisible).
        /// </summary>
        public const int NEO_TOKEN_DECIMALS = 0;

        /// <summary>
        /// The decimals for the GAS token.
        /// </summary>
        public const int GAS_TOKEN_DECIMALS = 8;

        /// <summary>
        /// The initial supply of NEO tokens.
        /// </summary>
        public const long NEO_TOKEN_TOTAL_SUPPLY = 100_000_000;

        /// <summary>
        /// The symbol for the NEO token.
        /// </summary>
        public const string NEO_TOKEN_SYMBOL = "NEO";

        /// <summary>
        /// The symbol for the GAS token.
        /// </summary>
        public const string GAS_TOKEN_SYMBOL = "GAS";

        // MARK: Cryptography

        /// <summary>
        /// The curve used by Neo for elliptic curve cryptography.
        /// </summary>
        public const string SECP256R1_CURVE_NAME = "secp256r1";

        /// <summary>
        /// Half of the curve order for secp256r1, used in signature validation.
        /// </summary>
        public static readonly BigInteger SECP256R1_HALF_CURVE_ORDER = 
            BigInteger.Parse("57896044618658097711785492504343953926418782139537452191302581570759080747169");

        /// <summary>
        /// The full curve order for secp256r1.
        /// </summary>
        public static readonly BigInteger SECP256R1_CURVE_ORDER = 
            BigInteger.Parse("115792089237316195423570985008687907852837564279074904382605163141518161494337");

        // MARK: Storage & Database

        /// <summary>
        /// The maximum key length for storage operations.
        /// </summary>
        public const int MAX_STORAGE_KEY_SIZE = 64;

        /// <summary>
        /// The maximum value length for storage operations.
        /// </summary>
        public const int MAX_STORAGE_VALUE_SIZE = 65535;

        /// <summary>
        /// The cost per byte of storage in GAS.
        /// </summary>
        public const long STORAGE_PRICE_PER_BYTE = 100000; // 0.001 GAS

        // MARK: Virtual Machine

        /// <summary>
        /// The maximum number of items that can be on the evaluation stack.
        /// </summary>
        public const int MAX_STACK_DEPTH = 2048;

        /// <summary>
        /// The maximum number of items in an array or map.
        /// </summary>
        public const int MAX_ARRAY_SIZE = 1024;

        /// <summary>
        /// The maximum size of a string or byte array item.
        /// </summary>
        public const int MAX_ITEM_SIZE = 1024 * 1024; // 1MB

        /// <summary>
        /// The default system fee for contract deployment.
        /// </summary>
        public const long DEFAULT_CONTRACT_DEPLOYMENT_FEE = 1000_00000000; // 1000 GAS

        // MARK: RPC & API

        /// <summary>
        /// The default RPC timeout in milliseconds.
        /// </summary>
        public const int DEFAULT_RPC_TIMEOUT_MS = 30000;

        /// <summary>
        /// The default maximum number of connections to RPC server.
        /// </summary>
        public const int DEFAULT_MAX_RPC_CONNECTIONS = 100;

        /// <summary>
        /// The default RPC port for MainNet.
        /// </summary>
        public const int DEFAULT_RPC_PORT_MAINNET = 10332;

        /// <summary>
        /// The default RPC port for TestNet.
        /// </summary>
        public const int DEFAULT_RPC_PORT_TESTNET = 20332;

        // MARK: Encoding & Serialization

        /// <summary>
        /// The prefix used for Base58Check encoding of Neo addresses.
        /// </summary>
        public const string BASE58_ADDRESS_PREFIX = "A";

        /// <summary>
        /// The maximum number of bytes that can be pushed onto the stack at once.
        /// </summary>
        public const int MAX_PUSH_DATA_SIZE = 520;

        // MARK: Economic Parameters

        /// <summary>
        /// The minimum GAS required to keep a transaction alive in the memory pool.
        /// </summary>
        public const long MIN_TRANSACTION_FEE = 1000_000; // 0.01 GAS

        /// <summary>
        /// The fee per byte for priority transactions.
        /// </summary>
        public const long HIGH_PRIORITY_FEE_PER_BYTE = 50000; // 0.0005 GAS

        /// <summary>
        /// The maximum fee per byte allowed.
        /// </summary>
        public const long MAX_FEE_PER_BYTE = 100000; // 0.001 GAS

        // MARK: Time & Timestamps

        /// <summary>
        /// The Unix timestamp of the Neo Genesis block.
        /// </summary>
        public const long GENESIS_TIMESTAMP = 1468595301000; // July 15, 2016

        /// <summary>
        /// The maximum allowed deviation of block timestamp from system time (in milliseconds).
        /// </summary>
        public const long MAX_BLOCK_TIMESTAMP_DEVIATION = 900000; // 15 minutes

        // MARK: Protocol Versions

        /// <summary>
        /// The current protocol version of Neo.
        /// </summary>
        public const int PROTOCOL_VERSION = 0;

        /// <summary>
        /// The minimum protocol version supported.
        /// </summary>
        public const int MIN_PROTOCOL_VERSION = 0;

        // MARK: Unity-Specific Constants

        /// <summary>
        /// The default timeout for Unity coroutines in seconds.
        /// </summary>
        public const float UNITY_DEFAULT_TIMEOUT = 30.0f;

        /// <summary>
        /// The maximum number of concurrent operations in Unity.
        /// </summary>
        public const int UNITY_MAX_CONCURRENT_OPERATIONS = 10;

        /// <summary>
        /// The default polling interval for Unity updates in seconds.
        /// </summary>
        public const float UNITY_DEFAULT_POLLING_INTERVAL = 1.0f;

        /// <summary>
        /// Converts GAS fraction to the smallest unit (10^-8 GAS).
        /// </summary>
        /// <param name="gas">The GAS amount.</param>
        /// <returns>The amount in the smallest unit.</returns>
        public static long GasToFixedPoint(decimal gas)
        {
            return (long)(gas * (decimal)Math.Pow(10, GAS_TOKEN_DECIMALS));
        }

        /// <summary>
        /// Converts the smallest GAS unit to decimal GAS.
        /// </summary>
        /// <param name="fixedPointGas">The amount in the smallest unit.</param>
        /// <returns>The GAS amount as decimal.</returns>
        public static decimal FixedPointToGas(long fixedPointGas)
        {
            return (decimal)fixedPointGas / (decimal)Math.Pow(10, GAS_TOKEN_DECIMALS);
        }

        /// <summary>
        /// Calculates the system fee for a transaction based on its size and complexity.
        /// </summary>
        /// <param name="transactionSize">The size of the transaction in bytes.</param>
        /// <param name="executionCost">The execution cost in GAS units.</param>
        /// <returns>The system fee in the smallest GAS unit.</returns>
        public static long CalculateSystemFee(int transactionSize, long executionCost)
        {
            return Math.Max(executionCost, MIN_TRANSACTION_FEE);
        }

        /// <summary>
        /// Calculates the network fee for a transaction based on its size.
        /// </summary>
        /// <param name="transactionSize">The size of the transaction in bytes.</param>
        /// <param name="feePerByte">The fee per byte (optional, uses default if not specified).</param>
        /// <returns>The network fee in the smallest GAS unit.</returns>
        public static long CalculateNetworkFee(int transactionSize, long feePerByte = DEFAULT_NETWORK_FEE_PER_BYTE)
        {
            return transactionSize * feePerByte;
        }
    }
}
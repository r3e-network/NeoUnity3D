using System;
using System.Threading.Tasks;
using UnityEngine;
using Neo.Unity.SDK.Core;
using Neo.Unity.SDK.Types;

namespace Neo.Unity.SDK.Contracts.Native
{
    /// <summary>
    /// Represents the GasToken native contract and provides methods to invoke its functions.
    /// The GAS token is used to pay for transaction fees and contract execution on the Neo blockchain.
    /// </summary>
    [System.Serializable]
    public class GasToken : FungibleToken
    {
        #region Constants

        public const string NAME = "GasToken";
        public static readonly Hash160 SCRIPT_HASH = SmartContract.CalcNativeContractHash(NAME);
        public const int DECIMALS = 8;
        public const string SYMBOL = "GAS";

        #endregion

        #region Constructor

        /// <summary>
        /// Constructs a new GasToken instance that uses the given NeoUnity instance for invocations.
        /// </summary>
        /// <param name="neoUnity">The NeoUnity instance to use for invocations</param>
        public GasToken(NeoUnity neoUnity) : base(SCRIPT_HASH, neoUnity)
        {
        }

        /// <summary>
        /// Constructs a new GasToken instance using the singleton NeoUnity instance.
        /// </summary>
        public GasToken() : base(SCRIPT_HASH)
        {
        }

        #endregion

        #region Token Metadata Overrides

        /// <summary>
        /// Returns the name of the GasToken contract.
        /// Doesn't require a call to the Neo node.
        /// </summary>
        /// <returns>The name</returns>
        public async Task<string> GetName()
        {
            return NAME;
        }

        /// <summary>
        /// Returns the symbol of the GasToken contract.
        /// Doesn't require a call to the Neo node.
        /// </summary>
        /// <returns>The symbol</returns>
        public override async Task<string> GetSymbol()
        {
            return SYMBOL;
        }

        /// <summary>
        /// Returns the number of decimals of the GAS token.
        /// Doesn't require a call to the Neo node.
        /// </summary>
        /// <returns>The number of decimals</returns>
        public override async Task<int> GetDecimals()
        {
            return DECIMALS;
        }

        #endregion

        #region GAS Token Utilities

        /// <summary>
        /// Converts a decimal GAS amount to GAS fractions.
        /// </summary>
        /// <param name="gasAmount">The GAS amount in decimal form</param>
        /// <returns>The GAS amount in fractions</returns>
        public static long ToGasFractions(decimal gasAmount)
        {
            return ToFractions(gasAmount, DECIMALS);
        }

        /// <summary>
        /// Converts GAS fractions to decimal GAS amount.
        /// </summary>
        /// <param name="gasFractions">The GAS amount in fractions</param>
        /// <returns>The GAS amount in decimal form</returns>
        public static decimal FromGasFractions(long gasFractions)
        {
            return ToDecimals(gasFractions, DECIMALS);
        }

        /// <summary>
        /// Gets the GAS balance formatted for display with proper decimal places.
        /// </summary>
        /// <param name="scriptHash">The script hash to get balance for</param>
        /// <returns>Formatted GAS balance string (e.g., "1.50000000 GAS")</returns>
        public async Task<string> GetFormattedGasBalance(Hash160 scriptHash)
        {
            var balance = await GetBalanceOf(scriptHash);
            var decimalAmount = FromGasFractions(balance);
            return $"{decimalAmount:F8} {SYMBOL}";
        }

        /// <summary>
        /// Gets the GAS balance formatted for display with customizable decimal places.
        /// </summary>
        /// <param name="scriptHash">The script hash to get balance for</param>
        /// <param name="decimalPlaces">Number of decimal places to display (0-8)</param>
        /// <param name="includeSymbol">Whether to include the GAS symbol</param>
        /// <returns>Formatted GAS balance string</returns>
        public async Task<string> GetFormattedGasBalance(Hash160 scriptHash, int decimalPlaces = 8, bool includeSymbol = true)
        {
            if (decimalPlaces < 0 || decimalPlaces > DECIMALS)
            {
                throw new ArgumentException($"Decimal places must be between 0 and {DECIMALS}.", nameof(decimalPlaces));
            }

            var balance = await GetBalanceOf(scriptHash);
            var decimalAmount = FromGasFractions(balance);
            var formatString = $"F{decimalPlaces}";
            var formattedAmount = decimalAmount.ToString(formatString);

            return includeSymbol ? $"{formattedAmount} {SYMBOL}" : formattedAmount;
        }

        /// <summary>
        /// Checks if an account has sufficient GAS balance for a transaction or operation.
        /// </summary>
        /// <param name="scriptHash">The account script hash to check</param>
        /// <param name="requiredGasFractions">The required GAS amount in fractions</param>
        /// <returns>True if the account has sufficient GAS balance</returns>
        public async Task<bool> HasSufficientGas(Hash160 scriptHash, long requiredGasFractions)
        {
            if (scriptHash == null)
            {
                throw new ArgumentNullException(nameof(scriptHash));
            }

            if (requiredGasFractions < 0)
            {
                throw new ArgumentException("Required GAS amount cannot be negative.", nameof(requiredGasFractions));
            }

            var balance = await GetBalanceOf(scriptHash);
            var hasSufficientGas = balance >= requiredGasFractions;

            if (NeoUnity.Config.EnableDebugLogging)
            {
                var balanceDecimal = FromGasFractions(balance);
                var requiredDecimal = FromGasFractions(requiredGasFractions);
                Debug.Log($"[GasToken] GAS check for {scriptHash}: Balance={balanceDecimal:F8} GAS, Required={requiredDecimal:F8} GAS, Sufficient={hasSufficientGas}");
            }

            return hasSufficientGas;
        }

        /// <summary>
        /// Calculates the remaining GAS balance after a hypothetical transaction.
        /// </summary>
        /// <param name="scriptHash">The account script hash</param>
        /// <param name="transactionCost">The transaction cost in GAS fractions</param>
        /// <returns>The remaining GAS balance in fractions, or -1 if insufficient</returns>
        public async Task<long> GetRemainingGasAfterTransaction(Hash160 scriptHash, long transactionCost)
        {
            if (scriptHash == null)
            {
                throw new ArgumentNullException(nameof(scriptHash));
            }

            if (transactionCost < 0)
            {
                throw new ArgumentException("Transaction cost cannot be negative.", nameof(transactionCost));
            }

            var balance = await GetBalanceOf(scriptHash);
            return balance >= transactionCost ? balance - transactionCost : -1;
        }

        /// <summary>
        /// Gets the GAS balance in a more user-friendly decimal format.
        /// </summary>
        /// <param name="scriptHash">The script hash to get balance for</param>
        /// <returns>The GAS balance in decimal form</returns>
        public async Task<decimal> GetGasBalanceDecimal(Hash160 scriptHash)
        {
            var balance = await GetBalanceOf(scriptHash);
            return FromGasFractions(balance);
        }

        #endregion

        #region Unity Integration Helpers

        /// <summary>
        /// Estimates the GAS cost for a simple NEP-17 token transfer.
        /// This is a rough estimate based on typical network fees.
        /// </summary>
        /// <returns>Estimated GAS cost in fractions for a simple transfer</returns>
        public static long EstimateTransferGasCost()
        {
            // Conservative estimate: 0.01 GAS for network fee + 0.005 GAS for system fee
            return ToGasFractions(0.015m);
        }

        /// <summary>
        /// Estimates the GAS cost for a contract invocation.
        /// This is a rough estimate and actual costs may vary significantly.
        /// </summary>
        /// <param name="complexity">The complexity level of the contract call (1-10)</param>
        /// <returns>Estimated GAS cost in fractions</returns>
        public static long EstimateContractInvocationGasCost(int complexity = 5)
        {
            if (complexity < 1 || complexity > 10)
            {
                throw new ArgumentException("Complexity must be between 1 and 10.", nameof(complexity));
            }

            // Base cost: 0.02 GAS + complexity factor
            var baseCost = 0.02m;
            var complexityFactor = complexity * 0.005m; // 0.005 GAS per complexity point
            
            return ToGasFractions(baseCost + complexityFactor);
        }

        /// <summary>
        /// Formats a GAS amount for Unity UI display with appropriate precision.
        /// </summary>
        /// <param name="gasFractions">The GAS amount in fractions</param>
        /// <param name="shortFormat">Whether to use short format (fewer decimals for small amounts)</param>
        /// <returns>User-friendly formatted GAS string</returns>
        public static string FormatGasAmountForUI(long gasFractions, bool shortFormat = true)
        {
            var gasDecimal = FromGasFractions(gasFractions);

            if (shortFormat)
            {
                // For UI display, show fewer decimals for very small amounts
                if (gasDecimal >= 1m)
                {
                    return $"{gasDecimal:F4} GAS"; // 4 decimal places for amounts >= 1 GAS
                }
                else if (gasDecimal >= 0.001m)
                {
                    return $"{gasDecimal:F6} GAS"; // 6 decimal places for amounts >= 0.001 GAS
                }
                else
                {
                    return $"{gasDecimal:F8} GAS"; // Full precision for very small amounts
                }
            }
            else
            {
                return $"{gasDecimal:F8} GAS"; // Always show full precision
            }
        }

        /// <summary>
        /// Validates that a GAS amount is within reasonable bounds for the Neo network.
        /// </summary>
        /// <param name="gasFractions">The GAS amount to validate</param>
        /// <returns>True if the amount is valid</returns>
        public static bool IsValidGasAmount(long gasFractions)
        {
            // GAS amounts must be non-negative and within the maximum supply bounds
            // Neo's maximum GAS supply is theoretically unlimited, but let's use a reasonable upper bound
            const long maxReasonableGas = 100_000_000L * 100_000_000L; // 100 million GAS in fractions
            
            return gasFractions >= 0 && gasFractions <= maxReasonableGas;
        }

        #endregion

        #region Debugging and Logging

        /// <summary>
        /// Logs detailed GAS balance information for debugging purposes.
        /// </summary>
        /// <param name="scriptHash">The script hash to log information for</param>
        /// <param name="context">Additional context for the log entry</param>
        public async Task LogGasBalance(Hash160 scriptHash, string context = null)
        {
            if (!NeoUnity.Config.EnableDebugLogging) return;

            try
            {
                var balance = await GetBalanceOf(scriptHash);
                var balanceDecimal = FromGasFractions(balance);
                var contextInfo = !string.IsNullOrEmpty(context) ? $" ({context})" : "";
                
                Debug.Log($"[GasToken] GAS Balance{contextInfo}: {scriptHash} = {balanceDecimal:F8} GAS ({balance} fractions)");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[GasToken] Failed to log GAS balance for {scriptHash}: {ex.Message}");
            }
        }

        #endregion
    }
}
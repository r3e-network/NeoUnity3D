using System;
using UnityEngine;
using Newtonsoft.Json;

namespace Neo.Unity.SDK.Protocol.Response
{
    /// <summary>
    /// Response for the calculatenetworkfee RPC call.
    /// Returns the calculated network fee required for a transaction.
    /// </summary>
    [System.Serializable]
    public class NeoNetworkFeeResponse : NeoResponse<NeoNetworkFeeResponse.NetworkFeeInfo>
    {
        /// <summary>
        /// Gets the network fee information from the response.
        /// </summary>
        public NetworkFeeInfo NetworkFeeInfo => Result;

        /// <summary>
        /// Default constructor for JSON deserialization.
        /// </summary>
        public NeoNetworkFeeResponse() : base()
        {
        }

        /// <summary>
        /// Creates a successful network fee response.
        /// </summary>
        /// <param name="networkFeeInfo">The network fee information</param>
        /// <param name="id">The request ID</param>
        public NeoNetworkFeeResponse(NetworkFeeInfo networkFeeInfo, int id = 1) : base(networkFeeInfo, id)
        {
        }

        /// <summary>
        /// Creates an error network fee response.
        /// </summary>
        /// <param name="error">The error information</param>
        /// <param name="id">The request ID</param>
        public NeoNetworkFeeResponse(ResponseError error, int id = 1) : base(error, id)
        {
        }

        /// <summary>
        /// Contains network fee calculation information.
        /// </summary>
        [System.Serializable]
        public class NetworkFeeInfo
        {
            /// <summary>The calculated network fee in the smallest unit (equivalent to satoshis)</summary>
            [JsonProperty("networkfee")]
            public string NetworkFee { get; set; }

            /// <summary>
            /// Default constructor for JSON deserialization.
            /// </summary>
            public NetworkFeeInfo()
            {
            }

            /// <summary>
            /// Creates new network fee information.
            /// </summary>
            /// <param name="networkFee">The network fee amount</param>
            public NetworkFeeInfo(string networkFee)
            {
                NetworkFee = networkFee;
            }

            /// <summary>
            /// Creates new network fee information from long value.
            /// </summary>
            /// <param name="networkFee">The network fee amount as long</param>
            public NetworkFeeInfo(long networkFee)
            {
                NetworkFee = networkFee.ToString();
            }

            /// <summary>
            /// Gets the network fee as a long value (in smallest unit).
            /// </summary>
            [JsonIgnore]
            public long NetworkFeeLong
            {
                get
                {
                    if (string.IsNullOrEmpty(NetworkFee))
                        return 0L;

                    if (long.TryParse(NetworkFee, out long result))
                        return result;

                    return 0L;
                }
            }

            /// <summary>
            /// Gets the network fee as a decimal value in GAS units.
            /// Note: Neo GAS uses 8 decimal places (1 GAS = 100,000,000 smallest units).
            /// </summary>
            [JsonIgnore]
            public decimal NetworkFeeGas
            {
                get
                {
                    try
                    {
                        return NetworkFeeLong / 100_000_000m; // Convert to GAS units
                    }
                    catch
                    {
                        return 0m;
                    }
                }
            }

            /// <summary>
            /// Gets the network fee as a double value in GAS units.
            /// Note: May lose precision for very small amounts.
            /// </summary>
            [JsonIgnore]
            public double NetworkFeeGasDouble
            {
                get
                {
                    try
                    {
                        return NetworkFeeLong / 100_000_000.0; // Convert to GAS units
                    }
                    catch
                    {
                        return 0.0;
                    }
                }
            }

            /// <summary>
            /// Checks if the network fee is greater than zero.
            /// </summary>
            [JsonIgnore]
            public bool HasFee => NetworkFeeLong > 0L;

            /// <summary>
            /// Gets a formatted string representation of the network fee in GAS.
            /// </summary>
            /// <param name="decimals">Number of decimal places to show (default: 8)</param>
            /// <returns>Formatted network fee amount</returns>
            public string GetFormattedGasAmount(int decimals = 8)
            {
                if (decimals < 0 || decimals > 8)
                    decimals = 8;

                return NetworkFeeGas.ToString($"F{decimals}");
            }

            /// <summary>
            /// Returns the network fee in scientific notation for very small amounts.
            /// </summary>
            /// <returns>Scientific notation representation</returns>
            public string GetScientificNotation()
            {
                return NetworkFeeGas.ToString("E8");
            }

            /// <summary>
            /// Compares this network fee with another.
            /// </summary>
            /// <param name="other">The other network fee to compare with</param>
            /// <returns>Comparison result</returns>
            public int CompareTo(NetworkFeeInfo other)
            {
                if (other == null)
                    return 1;

                return NetworkFeeLong.CompareTo(other.NetworkFeeLong);
            }

            /// <summary>
            /// Checks if this network fee is sufficient for a minimum required fee.
            /// </summary>
            /// <param name="minimumFee">The minimum required fee in smallest units</param>
            /// <returns>True if this fee meets the minimum requirement</returns>
            public bool IsSufficientFor(long minimumFee)
            {
                return NetworkFeeLong >= minimumFee;
            }

            /// <summary>
            /// Checks if this network fee is sufficient for a minimum required fee in GAS.
            /// </summary>
            /// <param name="minimumFeeGas">The minimum required fee in GAS units</param>
            /// <returns>True if this fee meets the minimum requirement</returns>
            public bool IsSufficientForGas(decimal minimumFeeGas)
            {
                return NetworkFeeGas >= minimumFeeGas;
            }

            /// <summary>
            /// Returns a string representation of the network fee.
            /// </summary>
            /// <returns>String representation</returns>
            public override string ToString()
            {
                return $"NetworkFee({GetFormattedGasAmount()} GAS, {NetworkFee} units)";
            }

            /// <summary>
            /// Validates the network fee information.
            /// </summary>
            /// <exception cref="ArgumentException">If the fee information is invalid</exception>
            public void Validate()
            {
                if (string.IsNullOrEmpty(NetworkFee))
                    throw new ArgumentException("Network fee cannot be null or empty");

                if (!long.TryParse(NetworkFee, out long fee))
                    throw new ArgumentException($"Invalid network fee format: {NetworkFee}");

                if (fee < 0)
                    throw new ArgumentException("Network fee cannot be negative");
            }

            /// <summary>
            /// Creates a copy of this network fee info with a new fee amount.
            /// </summary>
            /// <param name="newFee">The new fee amount in smallest units</param>
            /// <returns>New NetworkFeeInfo instance</returns>
            public NetworkFeeInfo WithFee(long newFee)
            {
                return new NetworkFeeInfo(newFee);
            }

            /// <summary>
            /// Creates a copy of this network fee info with a new fee amount in GAS.
            /// </summary>
            /// <param name="newFeeGas">The new fee amount in GAS units</param>
            /// <returns>New NetworkFeeInfo instance</returns>
            public NetworkFeeInfo WithFeeGas(decimal newFeeGas)
            {
                var feeInSmallestUnit = (long)(newFeeGas * 100_000_000m);
                return new NetworkFeeInfo(feeInSmallestUnit);
            }
        }
    }
}
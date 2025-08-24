using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Neo.Unity.SDK.Types;

namespace Neo.Unity.SDK.Transaction
{
    /// <summary>
    /// This signer represents a smart contract instead of a normal account.
    /// You can use this in transactions that require the verification of a smart contract,
    /// e.g., if you want to withdraw tokens from a contract you own.
    /// 
    /// Using such a signer will make Neo call the `verify()` method of the corresponding contract.
    /// Make sure to provide the necessary contract parameters if the contract's `verify()` method expects any.
    /// </summary>
    [Serializable]
    public class ContractSigner : Signer
    {
        [SerializeField]
        private ContractParameter[] _verifyParams;

        /// <summary>
        /// Gets the parameters that are consumed by this contract signer's verify() method.
        /// </summary>
        public ContractParameter[] VerifyParams => _verifyParams ?? Array.Empty<ContractParameter>();

        /// <summary>
        /// Initializes a new instance of the ContractSigner class.
        /// </summary>
        /// <param name="contractHash">The script hash of the contract.</param>
        /// <param name="scope">The witness scope for this signer.</param>
        /// <param name="verifyParams">The parameters to pass to the verify() method of the contract.</param>
        protected ContractSigner(Hash160 contractHash, WitnessScope scope, ContractParameter[] verifyParams = null)
            : base(contractHash, scope)
        {
            _verifyParams = verifyParams?.ToArray() ?? Array.Empty<ContractParameter>();
        }

        /// <summary>
        /// Creates a contract signer with CalledByEntry scope that only allows the entry point contract to use this signer's witness.
        /// </summary>
        /// <param name="contractHash">The script hash of the contract.</param>
        /// <param name="verifyParams">The parameters to pass to the verify() method of the contract.</param>
        /// <returns>A new ContractSigner instance.</returns>
        public static ContractSigner CalledByEntry(Hash160 contractHash, params ContractParameter[] verifyParams)
        {
            return new ContractSigner(contractHash, WitnessScope.CalledByEntry, verifyParams);
        }

        /// <summary>
        /// Creates a contract signer with Global scope that allows the witness to be used in all contexts.
        /// </summary>
        /// <param name="contractHash">The script hash of the contract.</param>
        /// <param name="verifyParams">The parameters to pass to the verify() method of the contract.</param>
        /// <returns>A new ContractSigner instance.</returns>
        public static ContractSigner Global(Hash160 contractHash, params ContractParameter[] verifyParams)
        {
            return new ContractSigner(contractHash, WitnessScope.Global, verifyParams);
        }

        /// <summary>
        /// Creates a contract signer with CustomContracts scope that allows the witness to be used in specified contracts.
        /// </summary>
        /// <param name="contractHash">The script hash of the contract.</param>
        /// <param name="allowedContracts">The contracts where this witness can be used.</param>
        /// <param name="verifyParams">The parameters to pass to the verify() method of the contract.</param>
        /// <returns>A new ContractSigner instance.</returns>
        public static ContractSigner CustomContracts(Hash160 contractHash, Hash160[] allowedContracts, params ContractParameter[] verifyParams)
        {
            var signer = new ContractSigner(contractHash, WitnessScope.CustomContracts, verifyParams);
            signer.SetAllowedContracts(allowedContracts);
            return signer;
        }

        /// <summary>
        /// Creates a contract signer with CustomGroups scope that allows the witness to be used in contracts from specified groups.
        /// </summary>
        /// <param name="contractHash">The script hash of the contract.</param>
        /// <param name="allowedGroups">The groups whose contracts can use this witness.</param>
        /// <param name="verifyParams">The parameters to pass to the verify() method of the contract.</param>
        /// <returns>A new ContractSigner instance.</returns>
        public static ContractSigner CustomGroups(Hash160 contractHash, ECPublicKey[] allowedGroups, params ContractParameter[] verifyParams)
        {
            var signer = new ContractSigner(contractHash, WitnessScope.CustomGroups, verifyParams);
            signer.SetAllowedGroups(allowedGroups);
            return signer;
        }

        /// <summary>
        /// Creates a contract signer with WitnessRules scope that uses custom rules to determine witness usage.
        /// </summary>
        /// <param name="contractHash">The script hash of the contract.</param>
        /// <param name="rules">The witness rules to apply.</param>
        /// <param name="verifyParams">The parameters to pass to the verify() method of the contract.</param>
        /// <returns>A new ContractSigner instance.</returns>
        public static ContractSigner WithRules(Hash160 contractHash, WitnessRule[] rules, params ContractParameter[] verifyParams)
        {
            var signer = new ContractSigner(contractHash, WitnessScope.WitnessRules, verifyParams);
            signer.SetRules(rules);
            return signer;
        }

        /// <summary>
        /// Creates a contract signer with None scope that only allows the witness to be used at transaction level.
        /// </summary>
        /// <param name="contractHash">The script hash of the contract.</param>
        /// <param name="verifyParams">The parameters to pass to the verify() method of the contract.</param>
        /// <returns>A new ContractSigner instance.</returns>
        public static ContractSigner None(Hash160 contractHash, params ContractParameter[] verifyParams)
        {
            return new ContractSigner(contractHash, WitnessScope.None, verifyParams);
        }

        /// <summary>
        /// Sets the parameters for the contract's verify() method.
        /// </summary>
        /// <param name="parameters">The parameters to set.</param>
        public void SetVerifyParameters(params ContractParameter[] parameters)
        {
            _verifyParams = parameters?.ToArray() ?? Array.Empty<ContractParameter>();
        }

        /// <summary>
        /// Adds a parameter to the contract's verify() method parameters.
        /// </summary>
        /// <param name="parameter">The parameter to add.</param>
        public void AddVerifyParameter(ContractParameter parameter)
        {
            if (parameter == null)
                throw new ArgumentNullException(nameof(parameter));

            var newParams = new ContractParameter[_verifyParams.Length + 1];
            Array.Copy(_verifyParams, newParams, _verifyParams.Length);
            newParams[_verifyParams.Length] = parameter;
            _verifyParams = newParams;
        }

        /// <summary>
        /// Removes all verify parameters.
        /// </summary>
        public void ClearVerifyParameters()
        {
            _verifyParams = Array.Empty<ContractParameter>();
        }

        /// <summary>
        /// Gets the witness for this contract signer.
        /// This creates a contract witness with the verify parameters.
        /// </summary>
        /// <returns>A witness suitable for contract verification.</returns>
        public override Witness GetWitness()
        {
            return Witness.CreateContractWitness(_verifyParams);
        }

        /// <summary>
        /// Validates that this contract signer is properly configured.
        /// </summary>
        /// <returns>True if the signer is valid, false otherwise.</returns>
        public override bool IsValid()
        {
            if (!base.IsValid())
                return false;

            // Validate verify parameters
            if (_verifyParams != null)
            {
                foreach (var param in _verifyParams)
                {
                    if (param == null || !param.IsValid())
                        return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Gets a description of this contract signer.
        /// </summary>
        /// <returns>A descriptive string.</returns>
        public override string GetDescription()
        {
            var baseDesc = base.GetDescription();
            var paramCount = _verifyParams?.Length ?? 0;
            return $"{baseDesc} (Contract signer with {paramCount} verify parameter{(paramCount != 1 ? "s" : "")})";
        }

        /// <summary>
        /// Gets detailed information about this contract signer including verify parameters.
        /// </summary>
        /// <returns>Detailed information string.</returns>
        public string GetDetailedInfo()
        {
            var info = GetDescription();
            
            if (_verifyParams != null && _verifyParams.Length > 0)
            {
                info += "\nVerify Parameters:";
                for (int i = 0; i < _verifyParams.Length; i++)
                {
                    info += $"\n  [{i}]: {_verifyParams[i]?.GetType()?.Name ?? "null"} - {_verifyParams[i]?.ToString() ?? "null"}";
                }
            }
            else
            {
                info += "\nNo verify parameters";
            }

            return info;
        }

        /// <summary>
        /// Determines if this contract signer has any verify parameters.
        /// </summary>
        /// <returns>True if verify parameters are present, false otherwise.</returns>
        public bool HasVerifyParameters()
        {
            return _verifyParams != null && _verifyParams.Length > 0;
        }

        /// <summary>
        /// Gets the number of verify parameters.
        /// </summary>
        /// <returns>The number of verify parameters.</returns>
        public int GetVerifyParameterCount()
        {
            return _verifyParams?.Length ?? 0;
        }

        /// <summary>
        /// Creates a copy of this contract signer with different verify parameters.
        /// </summary>
        /// <param name="newVerifyParams">The new verify parameters.</param>
        /// <returns>A new ContractSigner instance.</returns>
        public ContractSigner WithVerifyParameters(params ContractParameter[] newVerifyParams)
        {
            var signer = new ContractSigner(ScriptHash, Scopes, newVerifyParams);
            
            // Copy scope-specific settings
            if (AllowedContracts != null && AllowedContracts.Length > 0)
                signer.SetAllowedContracts(AllowedContracts);
            
            if (AllowedGroups != null && AllowedGroups.Length > 0)
                signer.SetAllowedGroups(AllowedGroups);
            
            if (Rules != null && Rules.Length > 0)
                signer.SetRules(Rules);

            return signer;
        }

        /// <summary>
        /// Determines if this contract signer is equivalent to another.
        /// </summary>
        /// <param name="other">The other contract signer to compare.</param>
        /// <returns>True if equivalent, false otherwise.</returns>
        public bool Equals(ContractSigner other)
        {
            if (other == null)
                return false;

            if (!base.Equals(other))
                return false;

            if ((_verifyParams?.Length ?? 0) != (other._verifyParams?.Length ?? 0))
                return false;

            if (_verifyParams != null && other._verifyParams != null)
            {
                for (int i = 0; i < _verifyParams.Length; i++)
                {
                    if (!_verifyParams[i].Equals(other._verifyParams[i]))
                        return false;
                }
            }

            return true;
        }

        public override bool Equals(object obj)
        {
            return obj is ContractSigner other && Equals(other);
        }

        public override int GetHashCode()
        {
            int hash = base.GetHashCode();
            if (_verifyParams != null)
            {
                foreach (var param in _verifyParams)
                {
                    hash = HashCode.Combine(hash, param);
                }
            }
            return hash;
        }

        public override string ToString()
        {
            return GetDetailedInfo();
        }
    }
}
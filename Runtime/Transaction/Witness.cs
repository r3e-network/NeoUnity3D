using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Neo.Unity.SDK.Serialization;
using Neo.Unity.SDK.Script;
using Neo.Unity.SDK.Types;
using Neo.Unity.SDK.Crypto;

namespace Neo.Unity.SDK.Transaction
{
    /// <summary>
    /// A script (invocation and verification script) used to validate a transaction.
    /// Usually, a witness is made up of a signature (invocation script) and a check-signature script (verification script)
    /// that together prove that the signer has witnessed the signed data.
    /// </summary>
    [Serializable]
    public struct Witness : INeoSerializable, IEquatable<Witness>
    {
        [SerializeField]
        private InvocationScript _invocationScript;
        
        [SerializeField]
        private VerificationScript _verificationScript;

        /// <summary>
        /// Gets the invocation script containing the signature or parameters.
        /// </summary>
        public InvocationScript InvocationScript => _invocationScript ?? new InvocationScript();

        /// <summary>
        /// Gets the verification script containing the signature checking logic.
        /// </summary>
        public VerificationScript VerificationScript => _verificationScript ?? new VerificationScript();

        /// <summary>
        /// Gets the size of this witness when serialized.
        /// </summary>
        public int Size => InvocationScript.Size + VerificationScript.Size;

        /// <summary>
        /// Gets a value indicating whether this witness is empty (no scripts).
        /// </summary>
        public bool IsEmpty => InvocationScript.IsEmpty && VerificationScript.IsEmpty;

        /// <summary>
        /// Initializes a new instance of the Witness struct with empty scripts.
        /// </summary>
        public Witness()
        {
            _invocationScript = new InvocationScript();
            _verificationScript = new VerificationScript();
        }

        /// <summary>
        /// Initializes a new instance of the Witness struct with the specified scripts.
        /// </summary>
        /// <param name="invocationScript">The invocation script.</param>
        /// <param name="verificationScript">The verification script.</param>
        public Witness(InvocationScript invocationScript, VerificationScript verificationScript)
        {
            _invocationScript = invocationScript ?? new InvocationScript();
            _verificationScript = verificationScript ?? new VerificationScript();
        }

        /// <summary>
        /// Initializes a new instance of the Witness struct with raw script bytes.
        /// </summary>
        /// <param name="invocationBytes">The invocation script bytes.</param>
        /// <param name="verificationBytes">The verification script bytes.</param>
        public Witness(byte[] invocationBytes, byte[] verificationBytes)
        {
            _invocationScript = new InvocationScript(invocationBytes);
            _verificationScript = new VerificationScript(verificationBytes);
        }

        /// <summary>
        /// Creates a witness from a message and key pair for single signature.
        /// </summary>
        /// <param name="messageToSign">The message to sign.</param>
        /// <param name="keyPair">The key pair used for signing.</param>
        /// <returns>A new witness with the signature and verification script.</returns>
        /// <exception cref="ArgumentNullException">Thrown when parameters are null.</exception>
        public static Witness Create(byte[] messageToSign, ECKeyPair keyPair)
        {
            if (messageToSign == null)
                throw new ArgumentNullException(nameof(messageToSign));
            if (keyPair == null)
                throw new ArgumentNullException(nameof(keyPair));

            var invocationScript = InvocationScript.FromMessageAndKeyPair(messageToSign, keyPair);
            var verificationScript = new VerificationScript(keyPair.PublicKey);
            
            return new Witness(invocationScript, verificationScript);
        }

        /// <summary>
        /// Creates a multi-signature witness with the specified signatures and public keys.
        /// </summary>
        /// <param name="signingThreshold">The minimum number of signatures required.</param>
        /// <param name="signatures">The signatures to include.</param>
        /// <param name="publicKeys">The public keys for verification.</param>
        /// <returns>A new multi-signature witness.</returns>
        /// <exception cref="ArgumentException">Thrown when not enough signatures are provided.</exception>
        public static Witness CreateMultiSigWitness(int signingThreshold, SignatureData[] signatures, ECPublicKey[] publicKeys)
        {
            if (signatures == null)
                throw new ArgumentNullException(nameof(signatures));
            if (publicKeys == null)
                throw new ArgumentNullException(nameof(publicKeys));
            if (signatures.Length < signingThreshold)
                throw new ArgumentException("Not enough signatures provided for the required signing threshold.");

            var verificationScript = new VerificationScript(publicKeys, signingThreshold);
            return CreateMultiSigWitness(signatures, verificationScript);
        }

        /// <summary>
        /// Creates a multi-signature witness with the specified signatures and verification script.
        /// </summary>
        /// <param name="signatures">The signatures to include.</param>
        /// <param name="verificationScript">The verification script.</param>
        /// <returns>A new multi-signature witness.</returns>
        /// <exception cref="ArgumentException">Thrown when not enough signatures are provided.</exception>
        public static Witness CreateMultiSigWitness(SignatureData[] signatures, VerificationScript verificationScript)
        {
            if (signatures == null)
                throw new ArgumentNullException(nameof(signatures));
            if (verificationScript == null)
                throw new ArgumentNullException(nameof(verificationScript));

            var threshold = verificationScript.GetSigningThreshold();
            if (signatures.Length < threshold)
                throw new ArgumentException("Not enough signatures provided for the required signing threshold.");

            var invocationScript = InvocationScript.FromSignatures(signatures.Take(threshold).ToArray());
            return new Witness(invocationScript, verificationScript);
        }

        /// <summary>
        /// Creates a contract witness with parameters for the contract's verify method.
        /// This is used when the signer is a contract rather than a regular account.
        /// </summary>
        /// <param name="verifyParams">The parameters for the contract's verify method.</param>
        /// <returns>A new contract witness.</returns>
        public static Witness CreateContractWitness(ContractParameter[] verifyParams = null)
        {
            if (verifyParams == null || verifyParams.Length == 0)
            {
                return new Witness();
            }

            var builder = new ScriptBuilder();
            foreach (var param in verifyParams)
            {
                builder.PushParam(param);
            }

            var invocationScript = new InvocationScript(builder.ToArray());
            var verificationScript = new VerificationScript(); // Empty for contract witnesses

            return new Witness(invocationScript, verificationScript);
        }

        /// <summary>
        /// Creates a witness with a single signature.
        /// </summary>
        /// <param name="signature">The signature data.</param>
        /// <param name="publicKey">The public key used for verification.</param>
        /// <returns>A new single-signature witness.</returns>
        public static Witness CreateSingleSigWitness(SignatureData signature, ECPublicKey publicKey)
        {
            if (signature == null)
                throw new ArgumentNullException(nameof(signature));
            if (publicKey == null)
                throw new ArgumentNullException(nameof(publicKey));

            var invocationScript = InvocationScript.FromSignatures(new[] { signature });
            var verificationScript = new VerificationScript(publicKey);

            return new Witness(invocationScript, verificationScript);
        }

        /// <summary>
        /// Serializes this witness to the specified writer.
        /// </summary>
        /// <param name="writer">The binary writer.</param>
        public void Serialize(BinaryWriter writer)
        {
            InvocationScript.Serialize(writer);
            VerificationScript.Serialize(writer);
        }

        /// <summary>
        /// Deserializes a witness from the specified reader.
        /// </summary>
        /// <param name="reader">The binary reader.</param>
        /// <returns>The deserialized witness.</returns>
        public static Witness Deserialize(BinaryReader reader)
        {
            var invocationScript = InvocationScript.Deserialize(reader);
            var verificationScript = VerificationScript.Deserialize(reader);
            return new Witness(invocationScript, verificationScript);
        }

        /// <summary>
        /// Validates this witness for correctness.
        /// </summary>
        /// <returns>True if the witness is valid, false otherwise.</returns>
        public bool IsValid()
        {
            try
            {
                // Basic validation - both scripts should be present for non-contract witnesses
                if (IsEmpty)
                    return false;

                // Additional validation could be added here
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Gets a hash of this witness for comparison purposes.
        /// </summary>
        /// <returns>A hash representing this witness.</returns>
        public Hash256 GetHash()
        {
            var combined = InvocationScript.Script.Concat(VerificationScript.Script).ToArray();
            return Hash256.ComputeHash(combined);
        }

        /// <summary>
        /// Determines if this witness can be used for the specified account.
        /// </summary>
        /// <param name="accountHash">The script hash of the account.</param>
        /// <returns>True if compatible, false otherwise.</returns>
        public bool IsCompatibleWith(Hash160 accountHash)
        {
            if (accountHash == null || VerificationScript.IsEmpty)
                return false;

            return VerificationScript.GetScriptHash().Equals(accountHash);
        }

        /// <summary>
        /// Gets information about the witness type and requirements.
        /// </summary>
        /// <returns>A description of the witness.</returns>
        public string GetWitnessInfo()
        {
            if (IsEmpty)
                return "Empty witness";

            if (VerificationScript.IsEmpty)
                return "Contract witness (no verification script)";

            var threshold = VerificationScript.GetSigningThreshold();
            if (threshold > 1)
                return $"Multi-signature witness (threshold: {threshold})";

            return "Single signature witness";
        }

        /// <summary>
        /// Determines if this witness is equivalent to another.
        /// </summary>
        /// <param name="other">The other witness to compare.</param>
        /// <returns>True if the witnesses are equivalent, false otherwise.</returns>
        public bool Equals(Witness other)
        {
            return InvocationScript.Equals(other.InvocationScript) &&
                   VerificationScript.Equals(other.VerificationScript);
        }

        /// <summary>
        /// Determines if this witness is equivalent to another object.
        /// </summary>
        /// <param name="obj">The object to compare.</param>
        /// <returns>True if equivalent, false otherwise.</returns>
        public override bool Equals(object obj)
        {
            return obj is Witness other && Equals(other);
        }

        /// <summary>
        /// Gets the hash code for this witness.
        /// </summary>
        /// <returns>The hash code.</returns>
        public override int GetHashCode()
        {
            return HashCode.Combine(InvocationScript, VerificationScript);
        }

        /// <summary>
        /// Gets a string representation of this witness.
        /// </summary>
        /// <returns>A string describing the witness.</returns>
        public override string ToString()
        {
            return $"Witness: {GetWitnessInfo()}, Size: {Size} bytes";
        }

        public static bool operator ==(Witness left, Witness right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Witness left, Witness right)
        {
            return !left.Equals(right);
        }
    }
}
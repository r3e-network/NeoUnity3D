using System;

namespace Neo.Unity.SDK.Transaction
{
    /// <summary>
    /// Base exception class for transaction-related errors in the Neo Unity SDK.
    /// </summary>
    public abstract class TransactionException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the TransactionException class.
        /// </summary>
        protected TransactionException() : base() { }

        /// <summary>
        /// Initializes a new instance of the TransactionException class with a specified error message.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        protected TransactionException(string message) : base(message) { }

        /// <summary>
        /// Initializes a new instance of the TransactionException class with a specified error message and inner exception.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="innerException">The exception that is the cause of the current exception.</param>
        protected TransactionException(string message, Exception innerException) : base(message, innerException) { }
    }

    /// <summary>
    /// Exception thrown when there are issues with transaction script format or structure.
    /// </summary>
    public class ScriptFormatException : TransactionException
    {
        /// <summary>
        /// Initializes a new instance of the ScriptFormatException class.
        /// </summary>
        public ScriptFormatException() : base("Invalid script format") { }

        /// <summary>
        /// Initializes a new instance of the ScriptFormatException class with a specified error message.
        /// </summary>
        /// <param name="message">The message that describes the script format error.</param>
        public ScriptFormatException(string message) : base($"Script format error: {message}") { }

        /// <summary>
        /// Initializes a new instance of the ScriptFormatException class with a specified error message and inner exception.
        /// </summary>
        /// <param name="message">The message that describes the script format error.</param>
        /// <param name="innerException">The exception that is the cause of the current exception.</param>
        public ScriptFormatException(string message, Exception innerException) : base($"Script format error: {message}", innerException) { }

        /// <summary>
        /// Creates a ScriptFormatException for invalid script length.
        /// </summary>
        /// <param name="actualLength">The actual length of the script.</param>
        /// <param name="expectedLength">The expected length of the script.</param>
        /// <returns>A new ScriptFormatException instance.</returns>
        public static ScriptFormatException InvalidLength(int actualLength, int expectedLength)
        {
            return new ScriptFormatException($"Invalid script length. Expected: {expectedLength}, Actual: {actualLength}");
        }

        /// <summary>
        /// Creates a ScriptFormatException for invalid opcodes.
        /// </summary>
        /// <param name="opcode">The invalid opcode.</param>
        /// <param name="position">The position where the invalid opcode was found.</param>
        /// <returns>A new ScriptFormatException instance.</returns>
        public static ScriptFormatException InvalidOpCode(byte opcode, int position)
        {
            return new ScriptFormatException($"Invalid opcode 0x{opcode:X2} at position {position}");
        }

        /// <summary>
        /// Creates a ScriptFormatException for malformed scripts.
        /// </summary>
        /// <param name="reason">The specific reason for the malformed script.</param>
        /// <returns>A new ScriptFormatException instance.</returns>
        public static ScriptFormatException Malformed(string reason)
        {
            return new ScriptFormatException($"Malformed script: {reason}");
        }
    }

    /// <summary>
    /// Exception thrown when there are configuration issues with transaction signers.
    /// </summary>
    public class SignerConfigurationException : TransactionException
    {
        /// <summary>
        /// Initializes a new instance of the SignerConfigurationException class.
        /// </summary>
        public SignerConfigurationException() : base("Invalid signer configuration") { }

        /// <summary>
        /// Initializes a new instance of the SignerConfigurationException class with a specified error message.
        /// </summary>
        /// <param name="message">The message that describes the signer configuration error.</param>
        public SignerConfigurationException(string message) : base($"Signer configuration error: {message}") { }

        /// <summary>
        /// Initializes a new instance of the SignerConfigurationException class with a specified error message and inner exception.
        /// </summary>
        /// <param name="message">The message that describes the signer configuration error.</param>
        /// <param name="innerException">The exception that is the cause of the current exception.</param>
        public SignerConfigurationException(string message, Exception innerException) : base($"Signer configuration error: {message}", innerException) { }

        /// <summary>
        /// Creates a SignerConfigurationException for duplicate signers.
        /// </summary>
        /// <param name="scriptHash">The script hash of the duplicate signer.</param>
        /// <returns>A new SignerConfigurationException instance.</returns>
        public static SignerConfigurationException DuplicateSigner(string scriptHash)
        {
            return new SignerConfigurationException($"Duplicate signer with script hash: {scriptHash}");
        }

        /// <summary>
        /// Creates a SignerConfigurationException for invalid witness scopes.
        /// </summary>
        /// <param name="scope">The invalid scope configuration.</param>
        /// <returns>A new SignerConfigurationException instance.</returns>
        public static SignerConfigurationException InvalidScope(string scope)
        {
            return new SignerConfigurationException($"Invalid witness scope configuration: {scope}");
        }

        /// <summary>
        /// Creates a SignerConfigurationException for missing required signers.
        /// </summary>
        /// <param name="requiredCount">The number of required signers.</param>
        /// <param name="actualCount">The actual number of signers provided.</param>
        /// <returns>A new SignerConfigurationException instance.</returns>
        public static SignerConfigurationException InsufficientSigners(int requiredCount, int actualCount)
        {
            return new SignerConfigurationException($"Insufficient signers. Required: {requiredCount}, Provided: {actualCount}");
        }

        /// <summary>
        /// Creates a SignerConfigurationException for incompatible signer types.
        /// </summary>
        /// <param name="signerType">The incompatible signer type.</param>
        /// <param name="context">The context where the incompatibility occurs.</param>
        /// <returns>A new SignerConfigurationException instance.</returns>
        public static SignerConfigurationException IncompatibleSignerType(string signerType, string context)
        {
            return new SignerConfigurationException($"Signer type '{signerType}' is incompatible with {context}");
        }
    }

    /// <summary>
    /// Exception thrown when there are general transaction configuration issues.
    /// </summary>
    public class TransactionConfigurationException : TransactionException
    {
        /// <summary>
        /// Initializes a new instance of the TransactionConfigurationException class.
        /// </summary>
        public TransactionConfigurationException() : base("Invalid transaction configuration") { }

        /// <summary>
        /// Initializes a new instance of the TransactionConfigurationException class with a specified error message.
        /// </summary>
        /// <param name="message">The message that describes the transaction configuration error.</param>
        public TransactionConfigurationException(string message) : base($"Transaction configuration error: {message}") { }

        /// <summary>
        /// Initializes a new instance of the TransactionConfigurationException class with a specified error message and inner exception.
        /// </summary>
        /// <param name="message">The message that describes the transaction configuration error.</param>
        /// <param name="innerException">The exception that is the cause of the current exception.</param>
        public TransactionConfigurationException(string message, Exception innerException) : base($"Transaction configuration error: {message}", innerException) { }

        /// <summary>
        /// Creates a TransactionConfigurationException for invalid transaction size.
        /// </summary>
        /// <param name="actualSize">The actual size of the transaction.</param>
        /// <param name="maxSize">The maximum allowed size.</param>
        /// <returns>A new TransactionConfigurationException instance.</returns>
        public static TransactionConfigurationException InvalidSize(int actualSize, int maxSize)
        {
            return new TransactionConfigurationException($"Transaction size exceeds limit. Size: {actualSize}, Max: {maxSize}");
        }

        /// <summary>
        /// Creates a TransactionConfigurationException for insufficient fees.
        /// </summary>
        /// <param name="providedFee">The fee provided.</param>
        /// <param name="requiredFee">The minimum required fee.</param>
        /// <returns>A new TransactionConfigurationException instance.</returns>
        public static TransactionConfigurationException InsufficientFee(long providedFee, long requiredFee)
        {
            return new TransactionConfigurationException($"Insufficient fee. Provided: {providedFee}, Required: {requiredFee}");
        }

        /// <summary>
        /// Creates a TransactionConfigurationException for invalid network magic.
        /// </summary>
        /// <param name="networkMagic">The invalid network magic value.</param>
        /// <returns>A new TransactionConfigurationException instance.</returns>
        public static TransactionConfigurationException InvalidNetworkMagic(uint networkMagic)
        {
            return new TransactionConfigurationException($"Invalid network magic: {networkMagic}");
        }

        /// <summary>
        /// Creates a TransactionConfigurationException for invalid valid until block.
        /// </summary>
        /// <param name="validUntilBlock">The invalid valid until block value.</param>
        /// <param name="currentBlock">The current block height.</param>
        /// <returns>A new TransactionConfigurationException instance.</returns>
        public static TransactionConfigurationException InvalidValidUntilBlock(uint validUntilBlock, uint currentBlock)
        {
            return new TransactionConfigurationException($"Invalid validUntilBlock. Value: {validUntilBlock}, Current block: {currentBlock}");
        }

        /// <summary>
        /// Creates a TransactionConfigurationException for too many attributes.
        /// </summary>
        /// <param name="attributeCount">The number of attributes.</param>
        /// <param name="maxAttributes">The maximum number of allowed attributes.</param>
        /// <returns>A new TransactionConfigurationException instance.</returns>
        public static TransactionConfigurationException TooManyAttributes(int attributeCount, int maxAttributes)
        {
            return new TransactionConfigurationException($"Too many transaction attributes. Count: {attributeCount}, Max: {maxAttributes}");
        }

        /// <summary>
        /// Creates a TransactionConfigurationException for invalid attribute types.
        /// </summary>
        /// <param name="attributeType">The invalid attribute type.</param>
        /// <returns>A new TransactionConfigurationException instance.</returns>
        public static TransactionConfigurationException InvalidAttributeType(string attributeType)
        {
            return new TransactionConfigurationException($"Invalid transaction attribute type: {attributeType}");
        }

        /// <summary>
        /// Creates a TransactionConfigurationException for missing required fields.
        /// </summary>
        /// <param name="fieldName">The name of the missing field.</param>
        /// <returns>A new TransactionConfigurationException instance.</returns>
        public static TransactionConfigurationException MissingRequiredField(string fieldName)
        {
            return new TransactionConfigurationException($"Missing required field: {fieldName}");
        }
    }

    /// <summary>
    /// Exception thrown when witness validation fails.
    /// </summary>
    public class WitnessValidationException : TransactionException
    {
        /// <summary>
        /// Initializes a new instance of the WitnessValidationException class.
        /// </summary>
        public WitnessValidationException() : base("Witness validation failed") { }

        /// <summary>
        /// Initializes a new instance of the WitnessValidationException class with a specified error message.
        /// </summary>
        /// <param name="message">The message that describes the witness validation error.</param>
        public WitnessValidationException(string message) : base($"Witness validation error: {message}") { }

        /// <summary>
        /// Initializes a new instance of the WitnessValidationException class with a specified error message and inner exception.
        /// </summary>
        /// <param name="message">The message that describes the witness validation error.</param>
        /// <param name="innerException">The exception that is the cause of the current exception.</param>
        public WitnessValidationException(string message, Exception innerException) : base($"Witness validation error: {message}", innerException) { }

        /// <summary>
        /// Creates a WitnessValidationException for signature verification failure.
        /// </summary>
        /// <param name="signerHash">The hash of the signer whose signature failed.</param>
        /// <returns>A new WitnessValidationException instance.</returns>
        public static WitnessValidationException SignatureVerificationFailed(string signerHash)
        {
            return new WitnessValidationException($"Signature verification failed for signer: {signerHash}");
        }

        /// <summary>
        /// Creates a WitnessValidationException for contract verification failure.
        /// </summary>
        /// <param name="contractHash">The hash of the contract whose verification failed.</param>
        /// <param name="reason">The specific reason for the failure.</param>
        /// <returns>A new WitnessValidationException instance.</returns>
        public static WitnessValidationException ContractVerificationFailed(string contractHash, string reason)
        {
            return new WitnessValidationException($"Contract verification failed for {contractHash}: {reason}");
        }
    }
}
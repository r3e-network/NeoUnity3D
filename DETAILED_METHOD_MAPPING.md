# 🔍 DETAILED TEST METHOD MAPPING ANALYSIS
## Method-by-Method Swift → C# Test Comparison

## ✅ GasTokenTests
**Files**: `GasTokenTests.swift` → `GasTokenTests.cs`

### 📊 Coverage Statistics
- **Swift Methods**: 4
- **C# Methods**: 14
- **Coverage**: 100.0% (4/4)
- **Enhancement Factor**: 3.5x
- **Exact Matches**: 4
- **Partial Matches**: 0
- **Missing Methods**: 0
- **Enhancement Methods**: 10

### ✅ Matched Methods
- 🎯 **Exact Match**
  - Swift: testName (L9, 4 lines)
  - C#: TestName (L38, 16 lines)
- 🎯 **Exact Match**
  - Swift: testSymbol (L14, 4 lines)
  - C#: TestSymbol (L55, 16 lines)
- 🎯 **Exact Match**
  - Swift: testDecimals (L19, 4 lines)
  - C#: TestDecimals (L72, 16 lines)
- 🎯 **Exact Match**
  - Swift: testScriptHash (L24, 3 lines)
  - C#: TestScriptHash (L89, 6 lines)

### 🚀 C# Enhancement Methods
- `TestGetTotalSupply` (async) - 16 lines
- `TestGetBalance` (async) - 17 lines
- `TestTransfer` (async) - 35 lines
- `TestMemoryUsage_CreateGasToken` - 13 lines
- `TestUnityCoroutineCompatibility` (Unity) - 26 lines
- `TestSerializationCompatibility` - 28 lines
- `TestErrorHandling_InvalidAddress` (async) - 18 lines
- `TestConstants` - 12 lines
- `TestMultipleInstances` - 10 lines
- `TestThreadSafety` - 36 lines

---

## ✅ NeoTokenTests
**Files**: `NeoTokenTests.swift` → `NeoTokenTests.cs`

### 📊 Coverage Statistics
- **Swift Methods**: 18
- **C# Methods**: 20
- **Coverage**: 100.0% (18/18)
- **Enhancement Factor**: 1.11x
- **Exact Matches**: 18
- **Partial Matches**: 0
- **Missing Methods**: 0
- **Enhancement Methods**: 2

### ✅ Matched Methods
- 🎯 **Exact Match**
  - Swift: testConstants (L27, 13 lines)
  - C#: TestConstants (L49, 26 lines)
- 🎯 **Exact Match**
  - Swift: testRegisterCandidate (L41, 18 lines)
  - C#: TestRegisterCandidate (L76, 31 lines)
- 🎯 **Exact Match**
  - Swift: testUnregisterCandidate (L60, 18 lines)
  - C#: TestUnregisterCandidate (L108, 31 lines)
- 🎯 **Exact Match**
  - Swift: testGetCandidates (L79, 11 lines)
  - C#: TestGetCandidates (L140, 21 lines)
- 🎯 **Exact Match**
  - Swift: testIsCandidate (L91, 7 lines)
  - C#: TestIsCandidate (L162, 18 lines)
- 🎯 **Exact Match**
  - Swift: testGetAllCandidatesIterator (L99, 17 lines)
  - C#: TestGetAllCandidatesIterator (L181, 40 lines)
- 🎯 **Exact Match**
  - Swift: testGetCandidateVotes (L117, 5 lines)
  - C#: TestGetCandidateVotes (L222, 17 lines)
- 🎯 **Exact Match**
  - Swift: testVote (L123, 17 lines)
  - C#: TestVote (L240, 34 lines)
- 🎯 **Exact Match**
  - Swift: testCancelVote (L141, 16 lines)
  - C#: TestCancelVote (L275, 33 lines)
- 🎯 **Exact Match**
  - Swift: testBuildVoteScript (L158, 12 lines)
  - C#: TestBuildVoteScript (L309, 19 lines)
- 🎯 **Exact Match**
  - Swift: testBuildCancelVoteScript (L171, 9 lines)
  - C#: TestBuildCancelVoteScript (L329, 18 lines)
- 🎯 **Exact Match**
  - Swift: testGetGasPerBlock (L181, 5 lines)
  - C#: TestGetGasPerBlock (L348, 16 lines)
- 🎯 **Exact Match**
  - Swift: testSetGasPerBlock (L187, 17 lines)
  - C#: TestSetGasPerBlock (L365, 27 lines)
- 🎯 **Exact Match**
  - Swift: testGetRegisterPrice (L205, 5 lines)
  - C#: TestGetRegisterPrice (L393, 16 lines)
- 🎯 **Exact Match**
  - Swift: testSetRegisterPrice (L211, 17 lines)
  - C#: TestSetRegisterPrice (L410, 27 lines)
- 🎯 **Exact Match**
  - Swift: testGetAccountState (L229, 12 lines)
  - C#: TestGetAccountState (L438, 20 lines)
- 🎯 **Exact Match**
  - Swift: testGetAccountState_noVote (L242, 9 lines)
  - C#: TestGetAccountState_NoVote (L459, 18 lines)
- 🎯 **Exact Match**
  - Swift: testGetAccountState_noBalance (L252, 9 lines)
  - C#: TestGetAccountState_NoBalance (L478, 18 lines)

### 🚀 C# Enhancement Methods
- `TestMemoryUsage_CreateNeoToken` - 14 lines
- `TestSerializationCompatibility` - 12 lines

---

## ⚠️ NefFileTests
**Files**: `NefFileTests.swift` → `NefFileTests.cs`

### 📊 Coverage Statistics
- **Swift Methods**: 16
- **C# Methods**: 17
- **Coverage**: 68.8% (11/16)
- **Enhancement Factor**: 1.06x
- **Exact Matches**: 8
- **Partial Matches**: 3
- **Missing Methods**: 5
- **Enhancement Methods**: 7

### ✅ Matched Methods
- 🎯 **Exact Match**
  - Swift: testNewNefFile (L31, 9 lines)
  - C#: TestNewNefFile (L30, 15 lines)
- 🎯 **Exact Match**
  - Swift: testNewNefFileWithMethodTokens (L41, 9 lines)
  - C#: TestNewNefFileWithMethodTokens (L46, 24 lines)
- 🎯 **Exact Match**
  - Swift: testFailConstructorWithTooLongCompilerName (L51, 6 lines)
  - C#: TestFailConstructorWithTooLongCompilerName (L71, 13 lines)
- 🔗 **Partial Match**
  - Swift: testReadFromFileShouldProduceCorrectNefFileWhenReadingValidFile (L58, 5 lines)
  - C#: TestReadFromFile (L85, 27 lines)
- 🎯 **Exact Match**
  - Swift: testReadFromFileThatIsTooLarge (L64, 5 lines)
  - C#: TestReadFromFileThatIsTooLarge (L113, 22 lines)
- 🔗 **Partial Match**
  - Swift: testDeserializeAndSerialize_ContractWithMethodTokens (L70, 10 lines)
  - C#: TestDeserializeAndSerialize (L136, 17 lines)
- 🔗 **Partial Match**
  - Swift: testDeserializeAndSerialize_ContractWithoutMethodTokens (L81, 10 lines)
  - C#: TestDeserializeAndSerialize (L136, 17 lines)
- 🎯 **Exact Match**
  - Swift: testDeserializeWithWrongMagicNumber (L92, 13 lines)
  - C#: TestDeserializeWithWrongMagicNumber (L154, 18 lines)
- 🎯 **Exact Match**
  - Swift: testDeserializeWithWrongChecksum (L106, 13 lines)
  - C#: TestDeserializeWithWrongChecksum (L173, 19 lines)
- 🎯 **Exact Match**
  - Swift: testDeserializeWithEmptyScript (L120, 13 lines)
  - C#: TestDeserializeWithEmptyScript (L193, 18 lines)
- 🎯 **Exact Match**
  - Swift: testGetSize (L134, 6 lines)
  - C#: TestGetSize (L212, 10 lines)

### ❌ Missing C# Methods
- `testDeserializeNeoTokenNefFile` - 9 lines
- `testDeserializeNefFileFromStackItem` - 10 lines
- `testSerializeDeserializeNefFileWithSourceUrl` - 12 lines
- `testFailDeserializationWithTooLongSourceUrl` (error test) - 14 lines
- `testFailConstructingWithTooLongSourceUrl` (error test) - 10 lines

### 🚀 C# Enhancement Methods
- `TestNullInputValidation` - 21 lines
- `TestEmptyInputValidation` - 14 lines
- `TestChecksumCalculation` - 16 lines
- `TestMemoryUsage_NefFileOperations` - 17 lines
- `TestUnityCoroutineCompatibility` (Unity) (async) - 22 lines
- `TestSerializationForUnityInspector` - 23 lines
- `TestThreadSafety_NefFileOperations` - 41 lines

---

## ✅ ECKeyPairTests
**Files**: `ECKeyPairTests.swift` → `ECKeyPairTests.cs`

### 📊 Coverage Statistics
- **Swift Methods**: 9
- **C# Methods**: 21
- **Coverage**: 100.0% (9/9)
- **Enhancement Factor**: 2.33x
- **Exact Matches**: 9
- **Partial Matches**: 0
- **Missing Methods**: 0
- **Enhancement Methods**: 12

### ✅ Matched Methods
- 🎯 **Exact Match**
  - Swift: testNewPublicKeyFromPoint (L10, 6 lines)
  - C#: TestNewPublicKeyFromPoint (L24, 13 lines)
- 🎯 **Exact Match**
  - Swift: testNewPublicKeyFromUncompressedPoint (L17, 5 lines)
  - C#: TestNewPublicKeyFromUncompressedPoint (L38, 9 lines)
- 🎯 **Exact Match**
  - Swift: testNewPublicKeyFromStringWithInvalidSize (L23, 4 lines)
  - C#: TestNewPublicKeyFromStringWithInvalidSize (L48, 9 lines)
- 🎯 **Exact Match**
  - Swift: testNewPublicKeyFromPointWithHexPrefix (L28, 4 lines)
  - C#: TestNewPublicKeyFromPointWithHexPrefix (L58, 12 lines)
- 🎯 **Exact Match**
  - Swift: testSerializePublicKey (L33, 7 lines)
  - C#: TestSerializePublicKey (L71, 13 lines)
- 🎯 **Exact Match**
  - Swift: testDeserializePublicKey (L41, 8 lines)
  - C#: TestDeserializePublicKey (L85, 12 lines)
- 🎯 **Exact Match**
  - Swift: testPublicKeySize (L50, 4 lines)
  - C#: TestPublicKeySize (L98, 9 lines)
- 🎯 **Exact Match**
  - Swift: testPublicKeyWif (L55, 8 lines)
  - C#: TestPublicKeyWif (L108, 13 lines)
- 🎯 **Exact Match**
  - Swift: testPublicKeyComparable (L64, 14 lines)
  - C#: TestPublicKeyComparable (L122, 16 lines)

### 🚀 C# Enhancement Methods
- `TestKeyPairCreation` - 13 lines
- `TestKeyPairFromPrivateKey` - 14 lines
- `TestSignAndVerify` - 15 lines
- `TestSignWithDifferentKeyVerifyFails` - 15 lines
- `TestInvalidSignatureVerification` - 14 lines
- `TestKeyPairEquality` - 12 lines
- `TestPublicKeyEquality` - 13 lines
- `TestMemoryUsage_KeyPairCreation` - 20 lines
- `TestUnityCoroutineCompatibility` (Unity) (async) - 20 lines
- `TestSerializationForUnityInspector` - 27 lines
- `TestThreadSafety` - 46 lines
- `TestEdgeCases_EmptyAndNullInputs` - 17 lines

---

## ✅ NEP2Tests
**Files**: `NEP2Tests.swift` → `NEP2Tests.cs`

### 📊 Coverage Statistics
- **Swift Methods**: 4
- **C# Methods**: 18
- **Coverage**: 100.0% (4/4)
- **Enhancement Factor**: 4.5x
- **Exact Matches**: 4
- **Partial Matches**: 0
- **Missing Methods**: 0
- **Enhancement Methods**: 14

### ✅ Matched Methods
- 🎯 **Exact Match**
  - Swift: testDecryptWithDefaultScryptParams (L7, 6 lines)
  - C#: TestDecryptWithDefaultScryptParams (L24, 11 lines)
- 🎯 **Exact Match**
  - Swift: testDecryptWithNonDefaultScryptParams (L14, 8 lines)
  - C#: TestDecryptWithNonDefaultScryptParams (L36, 15 lines)
- 🎯 **Exact Match**
  - Swift: testEncryptWithDefaultScryptParams (L23, 7 lines)
  - C#: TestEncryptWithDefaultScryptParams (L52, 13 lines)
- 🎯 **Exact Match**
  - Swift: testEncryptWithNonDefaultScryptParams (L31, 9 lines)
  - C#: TestEncryptWithNonDefaultScryptParams (L66, 15 lines)

### 🚀 C# Enhancement Methods
- `TestEncryptDecryptRoundTrip` - 17 lines
- `TestDecryptWithWrongPassword` - 7 lines
- `TestDecryptWithInvalidFormat` - 15 lines
- `TestNullInputValidation` - 17 lines
- `TestEmptyPasswordHandling` - 15 lines
- `TestDifferentScryptParameters` - 29 lines
- `TestLongPasswordHandling` - 16 lines
- `TestUnicodePasswordHandling` - 16 lines
- `TestConsistentEncryption` - 19 lines
- `TestMemoryUsage_NEP2Operations` - 30 lines
- `TestUnityCoroutineCompatibility` (Unity) (async) - 24 lines
- `TestSerializationForUnityInspector` - 25 lines
- `TestThreadSafety_NEP2Operations` - 45 lines
- `TestEdgeCases_SpecialCharacterPasswords` - 30 lines

---

## ❌ AccountTests
**Files**: `AccountTests.swift` → `AccountTests.cs`

### 📊 Coverage Statistics
- **Swift Methods**: 25
- **C# Methods**: 30
- **Coverage**: 8.0% (2/25)
- **Enhancement Factor**: 1.2x
- **Exact Matches**: 0
- **Partial Matches**: 2
- **Missing Methods**: 23
- **Enhancement Methods**: 28

### ✅ Matched Methods
- 🔗 **Partial Match**
  - Swift: testFromPublicKey (L35, 6 lines)
  - C#: TestFromPublicKey_ShouldCreateValidAccount (L169, 14 lines)
- 🔗 **Partial Match**
  - Swift: testUnlock (L223, 7 lines)
  - C#: TestLockUnlock_ShouldToggleLockState (L111, 15 lines)

### ❌ Missing C# Methods
- `testCreateGenericAccount` - 11 lines
- `testInitAccountFromExistingKeyPair` - 8 lines
- `testFromVerificationScript` - 6 lines
- `testCreateMultiSigAccountFromPublicKeys` - 8 lines
- `testCreateMultiSigAccountWithAddress` - 9 lines
- `testCreateMultiSigAccountFromVerificationScript` - 7 lines
- `testEncryptPublicKey` - 6 lines
- `testFailEncryptAccountWithoutPrivateKey` (error test) - 6 lines
- `testDecryptWithStandardScryptParams` - 9 lines
- `testFailDecryptingAccountWithoutDecryptedPrivateKey` (error test) - 6 lines
- `testLoadAccountFromNEP6` - 10 lines
- `testLoadMultiSigAccountFromNEP6` - 12 lines
- `testToNep6AccountWithOnlyAnAddress` - 10 lines
- `testFailToNep6AccountWithUnencryptedPrivateKey` (error test) - 6 lines
- `testToNep6AccountWithEncryptedPrivateKey` - 11 lines
- `testToNep6AccountWithMultiSigAccount` - 12 lines
- `testCreateAccountFromWIF` - 12 lines
- `testCreateAccountFromAddress` - 9 lines
- `testGetNep17Balances` (async) - 15 lines
- `testIsMultiSig` - 16 lines
- `testIsDefault` - 8 lines
- `testWalletLink` - 9 lines
- `testNilValuesWhenNotMultiSig` - 5 lines

### 🚀 C# Enhancement Methods
- `TestAccountConstructor_WithKeyPair_ShouldCreateValidAccount` - 15 lines
- `TestAccountConstructor_WithAddress_ShouldCreateValidAccount` - 13 lines
- `TestAccountConstructor_WithInvalidAddress_ShouldThrow` - 7 lines
- `TestAccountConstructor_WithNullKeyPair_ShouldThrow` - 7 lines
- `TestGetScriptHash_ShouldReturnCorrectHash` - 13 lines
- `TestSetLabel_ShouldUpdateLabel` - 13 lines
- `TestCreate_ShouldCreateValidAccount` (async) - 13 lines
- `TestFromAddress_ShouldCreateValidAccount` - 11 lines
- `TestFromScriptHash_ShouldCreateValidAccount` - 11 lines
- `TestFromWIF_ShouldCreateValidAccount` (async) - 14 lines
- `TestCreateMultiSigAccount_WithPublicKeys_ShouldCreateValidAccount` (async) - 20 lines
- `TestCreateMultiSigAccount_WithParameters_ShouldCreateValidAccount` - 17 lines
- `TestCreateMultiSigAccount_WithInvalidThreshold_ShouldThrow` - 12 lines
- `TestGetSigningThreshold_OnSingleSig_ShouldThrow` - 10 lines
- `TestGetNumberOfParticipants_OnSingleSig_ShouldThrow` - 10 lines
- `TestEncryptDecryptPrivateKey_ShouldMaintainFunctionality` (async) - 24 lines
- `TestEncryptPrivateKey_WithoutKeyPair_ShouldThrow` (async) - 13 lines
- `TestDecryptPrivateKey_WithoutEncryptedKey_ShouldThrow` (async) - 13 lines
- `TestToString_SingleSig_ShouldReturnCorrectFormat` - 15 lines
- `TestToString_MultiSig_ShouldReturnCorrectFormat` - 14 lines
- `TestToNEP6Account_WithEncryption_ShouldCreateValidNEP6` (async) - 17 lines
- `TestFromNEP6Account_ShouldCreateValidAccount` (async) - 17 lines
- `TestValidAddress_ShouldPass` - 17 lines
- `TestInvalidAddress_ShouldThrow` - 20 lines
- `TestAccountEquality_SameAddress_ShouldBeEqual` - 10 lines
- `TestAccountEquality_DifferentAccounts_ShouldNotBeEqual` (async) - 10 lines
- `TestEncryptPrivateKey_WithEmptyPassword_ShouldThrow` (async) - 15 lines
- `TestDecryptPrivateKey_WithWrongPassword_ShouldThrow` (async) - 15 lines

---

## ❌ TransactionBuilderTests
**Files**: `TransactionBuilderTests.swift` → `TransactionBuilderTests.cs`

### 📊 Coverage Statistics
- **Swift Methods**: 49
- **C# Methods**: 30
- **Coverage**: 49.0% (24/49)
- **Enhancement Factor**: 0.61x
- **Exact Matches**: 24
- **Partial Matches**: 0
- **Missing Methods**: 25
- **Enhancement Methods**: 6

### ✅ Matched Methods
- 🎯 **Exact Match**
  - Swift: testBuildTransactionWithCorrectNonce (L26, 25 lines)
  - C#: TestBuildTransactionWithCorrectNonce (L53, 36 lines)
- 🎯 **Exact Match**
  - Swift: testFailBuildingTransactionWithIncorrectNonce (L52, 10 lines)
  - C#: TestFailBuildingTransactionWithIncorrectNonce (L90, 15 lines)
- 🎯 **Exact Match**
  - Swift: testFailBuildingTransactionWithInvalidBlockNumber (L63, 14 lines)
  - C#: TestFailBuildingTransactionWithInvalidBlockNumber (L106, 20 lines)
- 🎯 **Exact Match**
  - Swift: testAutomaticallySetNonce (L78, 11 lines)
  - C#: TestAutomaticallySetNonce (L127, 22 lines)
- 🎯 **Exact Match**
  - Swift: testFailBuildingTxWithoutAnySigner (L90, 13 lines)
  - C#: TestFailBuildingTxWithoutAnySigner (L150, 25 lines)
- 🎯 **Exact Match**
  - Swift: testOverrideSigner (L104, 8 lines)
  - C#: TestOverrideSigner (L176, 13 lines)
- 🎯 **Exact Match**
  - Swift: testAttributesHighPriority (L113, 11 lines)
  - C#: TestAttributesHighPriority (L190, 23 lines)
- 🎯 **Exact Match**
  - Swift: testAttributesHighPriorityCommittee (L125, 12 lines)
  - C#: TestAttributesHighPriorityCommittee (L214, 26 lines)
- 🎯 **Exact Match**
  - Swift: testAttributesHighPriorityNotCommitteeMember (L138, 15 lines)
  - C#: TestAttributesHighPriorityNotCommitteeMember (L241, 24 lines)
- 🎯 **Exact Match**
  - Swift: testAttributesHighPriorityOnlyAddedOnce (L154, 12 lines)
  - C#: TestAttributesHighPriorityOnlyAddedOnce (L266, 24 lines)
- 🎯 **Exact Match**
  - Swift: testFailAddingMoreThanMaxAttributesToTx_justAttributes (L167, 6 lines)
  - C#: TestFailAddingMoreThanMaxAttributesToTx_JustAttributes (L291, 16 lines)
- 🎯 **Exact Match**
  - Swift: testFailAddingMoreThanMaxAttributesToTx_attributesAndSigners (L174, 9 lines)
  - C#: TestFailAddingMoreThanMaxAttributesToTx_AttributesAndSigners (L308, 24 lines)
- 🎯 **Exact Match**
  - Swift: testAutomaticSettingOfValidUntilBlockVariable (L192, 10 lines)
  - C#: TestAutomaticSettingOfValidUntilBlockVariable (L333, 22 lines)
- 🎯 **Exact Match**
  - Swift: testAutomaticSettingOfSystemFeeAndNetworkFee (L203, 10 lines)
  - C#: TestAutomaticSettingOfSystemFeeAndNetworkFee (L356, 22 lines)
- 🎯 **Exact Match**
  - Swift: testFailTryingToSignTransactionWithAccountMissingAPrivateKey (L214, 15 lines)
  - C#: TestFailTryingToSignTransactionWithAccountMissingAPrivateKey (L379, 24 lines)
- 🎯 **Exact Match**
  - Swift: testFailAutomaticallySigningWithMultiSigAccountSigner (L230, 15 lines)
  - C#: TestFailAutomaticallySigningWithMultiSigAccountSigner (L404, 27 lines)
- 🎯 **Exact Match**
  - Swift: testSignTransactionWithAdditionalSigners (L278, 14 lines)
  - C#: TestSignTransactionWithAdditionalSigners (L432, 38 lines)
- 🎯 **Exact Match**
  - Swift: testSendInvokeFunction (L321, 19 lines)
  - C#: TestSendInvokeFunction (L471, 35 lines)
- 🎯 **Exact Match**
  - Swift: testTransferNeoFromNormalAccount (L341, 17 lines)
  - C#: TestTransferNeoFromNormalAccount (L507, 34 lines)
- 🎯 **Exact Match**
  - Swift: testExtendScript (L359, 15 lines)
  - C#: TestExtendScript (L542, 36 lines)
- 🎯 **Exact Match**
  - Swift: testGetUnsignedTransaction (L513, 15 lines)
  - C#: TestGetUnsignedTransaction (L579, 23 lines)
- 🎯 **Exact Match**
  - Swift: testVersion (L529, 14 lines)
  - C#: TestVersion (L603, 22 lines)
- 🎯 **Exact Match**
  - Swift: testAdditionalNetworkFee (L544, 21 lines)
  - C#: TestAdditionalNetworkFee (L626, 30 lines)
- 🎯 **Exact Match**
  - Swift: testAdditionalSystemFee (L566, 21 lines)
  - C#: TestAdditionalSystemFee (L657, 30 lines)

### ❌ Missing C# Methods
- `testFailAddingMoreThanMaxAttributesToTx_signers` (error test) - 7 lines
- `testFailWithNoSigningAccount` (async) - 14 lines
- `testFailSigningWithAccountWithoutECKeyPair` (async) - 16 lines
- `testFailSendingTransactionBecauseItDoesntContainTheRightNumberOfWitnesses` (async) - 14 lines
- `testContractWitness` (async) - 12 lines
- `testInvokingWithParamsShouldProduceTheCorrectRequest` (async) - 11 lines
- `testDoIfSenderCannotCoverFees` (async) - 26 lines
- `testDoIfSenderCannotCoverFees_alreadySpecifiedASupplier` (error test) - 6 lines
- `testThrowIfSenderCannotCoverFees` (async) - 21 lines
- `testThrowIfSenderCannotCoverFees_alreadySpecifiedAConsumer` (error test) - 6 lines
- `testInvokeScript` (async) - 7 lines
- `testInvokeScriptWithoutSettingScript` (async) - 11 lines
- `testBuildWithoutSettingScript` (async) - 8 lines
- `testBuildWithInvalidScript` (async) - 16 lines
- `testBuildWithScript_vmFaults` (async) - 16 lines
- `testSetFirstSigner` - 15 lines
- `testSetFirstSigner_feeOnlyPresent` (error test) - 12 lines
- `testSetFirstSigner_notPresent` (error test) - 11 lines
- `testTrackingTransactionShouldReturnCorrectBlock` (async) - 38 lines
- `testTrackingTransaction_txNotSent` (async) - 23 lines
- `testGetAppliationLog` (async) - 23 lines
- `testGetApplicationLog_txNotSent` (async) - 25 lines
- `testGetApplicationLog_notExisting` (async) - 27 lines
- `testTransmissionOnFault` (async) - 24 lines
- `testPreventTransmissionOnFault` (async) - 22 lines

### 🚀 C# Enhancement Methods
- `TestMemoryUsage_TransactionBuilder` - 22 lines
- `TestUnityCoroutineCompatibility` (Unity) - 33 lines
- `TestSerializationCompatibility` - 27 lines
- `TestThreadSafety` - 47 lines
- `TestEdgeCases_InvalidInputs` - 27 lines
- `TestTransactionSerialization` (async) - 29 lines

---

## ⚠️ ScriptBuilderTests
**Files**: `ScriptBuilderTests.swift` → `ScriptBuilderTests.cs`

### 📊 Coverage Statistics
- **Swift Methods**: 9
- **C# Methods**: 22
- **Coverage**: 55.6% (5/9)
- **Enhancement Factor**: 2.44x
- **Exact Matches**: 5
- **Partial Matches**: 0
- **Missing Methods**: 4
- **Enhancement Methods**: 17

### ✅ Matched Methods
- 🎯 **Exact Match**
  - Swift: testPushArrayEmpty (L10, 4 lines)
  - C#: TestPushArrayEmpty (L29, 9 lines)
- 🎯 **Exact Match**
  - Swift: testPushParamEmptyArray (L15, 4 lines)
  - C#: TestPushParamEmptyArray (L39, 9 lines)
- 🎯 **Exact Match**
  - Swift: testPushByteArray (L20, 13 lines)
  - C#: TestPushByteArray (L49, 19 lines)
- 🎯 **Exact Match**
  - Swift: testPushString (L34, 10 lines)
  - C#: TestPushString (L69, 15 lines)
- 🎯 **Exact Match**
  - Swift: testPushInteger (L45, 37 lines)
  - C#: TestPushInteger (L85, 27 lines)

### ❌ Missing C# Methods
- `testVerificationScriptFromPublicKeys` - 13 lines
- `testVerificationScriptFromPublicKey` - 6 lines
- `testMap` - 26 lines
- `testMapNested` - 34 lines

### 🚀 C# Enhancement Methods
- `TestPushBigInteger` - 19 lines
- `TestPushBoolean` - 11 lines
- `TestPushParam` - 21 lines
- `TestOpCode` - 10 lines
- `TestRawBytes` - 8 lines
- `TestContractCall` - 22 lines
- `TestSysCall` - 12 lines
- `TestJump` - 9 lines
- `TestComplexScript` - 22 lines
- `TestScriptSize` - 12 lines
- `TestClear` - 12 lines
- `TestMemoryUsage_ScriptBuilder` - 23 lines
- `TestUnityCoroutineCompatibility` (Unity) (async) - 23 lines
- `TestSerializationForUnityInspector` - 28 lines
- `TestThreadSafety` - 40 lines
- `TestEdgeCases_InvalidInputs` - 17 lines
- `TestDataIntegrity_RoundTripSerialization` - 27 lines

---

## ✅ BinaryReaderTests
**Files**: `BinaryReaderTests.swift` → `BinaryReaderTests.cs`

### 📊 Coverage Statistics
- **Swift Methods**: 6
- **C# Methods**: 21
- **Coverage**: 100.0% (6/6)
- **Enhancement Factor**: 3.5x
- **Exact Matches**: 6
- **Partial Matches**: 0
- **Missing Methods**: 0
- **Enhancement Methods**: 15

### ✅ Matched Methods
- 🎯 **Exact Match**
  - Swift: testReadPushDataBytes (L8, 13 lines)
  - C#: TestReadPushDataBytes (L22, 31 lines)
- 🎯 **Exact Match**
  - Swift: testFailReadPushData (L22, 6 lines)
  - C#: TestFailReadPushData (L54, 16 lines)
- 🎯 **Exact Match**
  - Swift: testReadPushDataString (L29, 7 lines)
  - C#: TestReadPushDataString (L71, 15 lines)
- 🎯 **Exact Match**
  - Swift: testReadPushDataBigInteger (L37, 6 lines)
  - C#: TestReadPushDataBigInteger (L87, 9 lines)
- 🎯 **Exact Match**
  - Swift: testReadUInt32 (L44, 6 lines)
  - C#: TestReadUInt32 (L97, 9 lines)
- 🎯 **Exact Match**
  - Swift: testReadInt64 (L51, 6 lines)
  - C#: TestReadInt64 (L107, 9 lines)

### 🚀 C# Enhancement Methods
- `TestReadByte` - 13 lines
- `TestReadBytes` - 15 lines
- `TestReadUInt16` - 9 lines
- `TestReadVarInt` - 9 lines
- `TestReadVarString` - 14 lines
- `TestReadBoolean` - 13 lines
- `TestPosition` - 19 lines
- `TestAvailable` - 19 lines
- `TestEndOfStream` - 16 lines
- `TestMemoryUsage_BinaryReader` - 22 lines
- `TestUnityCoroutineCompatibility` (Unity) (async) - 21 lines
- `TestSerializationForUnityInspector` - 24 lines
- `TestThreadSafety` - 42 lines
- `TestEdgeCases_BoundaryConditions` - 17 lines
- `TestDataIntegrity_RoundTripSerialization` - 39 lines

---

## 🏁 DETAILED MAPPING SUMMARY
**Analyzed Files**: 9 file pairs
**Total Swift Methods Analyzed**: 140
**Total Matched Methods**: 83
**Total Enhancement Methods**: 111
**Overall Coverage**: 59.3%
**Overall Enhancement Factor**: 1.39x
using System;
using System.Threading.Tasks;
using NUnit.Framework;
using UnityEngine;
using Neo.Unity.SDK.Wallet;
using Neo.Unity.SDK.Types;
using Neo.Unity.SDK.Crypto;

namespace Neo.Unity.SDK.Tests.Wallet
{
    /// <summary>
    /// Unit tests for the Account class.
    /// Tests account creation, key management, and wallet operations.
    /// </summary>
    [TestFixture]
    public class AccountTests
    {
        private ECKeyPair testKeyPair;
        private string testAddress;
        private Hash160 testScriptHash;
        
        [SetUp]
        public async Task SetUp()
        {
            // Create test key pair
            testKeyPair = await ECKeyPair.CreateEcKeyPair();
            testAddress = testKeyPair.GetAddress();
            testScriptHash = Hash160.FromAddress(testAddress);
        }
        
        #region Constructor Tests
        
        [Test]
        public void TestAccountConstructor_WithKeyPair_ShouldCreateValidAccount()
        {
            // Act
            var account = new Account(testKeyPair);
            
            // Assert
            Assert.IsNotNull(account, "Account should not be null");
            Assert.AreEqual(testAddress, account.Address, "Address should match key pair");
            Assert.AreEqual(testAddress, account.Label, "Label should default to address");
            Assert.IsNotNull(account.VerificationScript, "Verification script should not be null");
            Assert.IsFalse(account.IsLocked, "Account should not be locked by default");
            Assert.IsFalse(account.IsMultiSig, "Single-sig account should not be multi-sig");
            Assert.IsTrue(account.CanSign, "Account with key pair should be able to sign");
        }
        
        [Test]
        public void TestAccountConstructor_WithAddress_ShouldCreateValidAccount()
        {
            // Act
            var account = new Account(testAddress);
            
            // Assert
            Assert.IsNotNull(account, "Account should not be null");
            Assert.AreEqual(testAddress, account.Address, "Address should match");
            Assert.AreEqual(testAddress, account.Label, "Label should default to address");
            Assert.IsFalse(account.IsLocked, "Account should not be locked by default");
            Assert.IsFalse(account.CanSign, "Account without key pair should not be able to sign");
        }
        
        [Test]
        public void TestAccountConstructor_WithInvalidAddress_ShouldThrow()
        {
            // Act & Assert
            Assert.Throws<ArgumentException>(() => new Account("invalid_address"), 
                "Should throw exception for invalid address");
        }
        
        [Test]
        public void TestAccountConstructor_WithNullKeyPair_ShouldThrow()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new Account((ECKeyPair)null), 
                "Should throw exception for null key pair");
        }
        
        #endregion
        
        #region Property Tests
        
        [Test]
        public void TestGetScriptHash_ShouldReturnCorrectHash()
        {
            // Arrange
            var account = new Account(testKeyPair);
            
            // Act
            var scriptHash = account.GetScriptHash();
            
            // Assert
            Assert.IsNotNull(scriptHash, "Script hash should not be null");
            Assert.AreEqual(testScriptHash, scriptHash, "Script hash should match expected value");
        }
        
        [Test]
        public void TestSetLabel_ShouldUpdateLabel()
        {
            // Arrange
            var account = new Account(testKeyPair);
            var newLabel = "Test Account";
            
            // Act
            account.SetLabel(newLabel);
            
            // Assert
            Assert.AreEqual(newLabel, account.Label, "Label should be updated");
        }
        
        [Test]
        public void TestLockUnlock_ShouldToggleLockState()
        {
            // Arrange
            var account = new Account(testKeyPair);
            
            // Act & Assert
            Assert.IsFalse(account.IsLocked, "Account should start unlocked");
            
            account.Lock();
            Assert.IsTrue(account.IsLocked, "Account should be locked after Lock()");
            
            account.Unlock();
            Assert.IsFalse(account.IsLocked, "Account should be unlocked after Unlock()");
        }
        
        #endregion
        
        #region Static Factory Method Tests
        
        [Test]
        public async Task TestCreate_ShouldCreateValidAccount()
        {
            // Act
            var account = await Account.Create();
            
            // Assert
            Assert.IsNotNull(account, "Created account should not be null");
            Assert.IsNotNull(account.Address, "Address should not be null");
            Assert.IsNotNull(account.KeyPair, "Key pair should not be null");
            Assert.IsTrue(account.CanSign, "Created account should be able to sign");
            Assert.IsFalse(account.IsMultiSig, "Single account should not be multi-sig");
        }
        
        [Test]
        public void TestFromAddress_ShouldCreateValidAccount()
        {
            // Act
            var account = Account.FromAddress(testAddress);
            
            // Assert
            Assert.IsNotNull(account, "Account should not be null");
            Assert.AreEqual(testAddress, account.Address, "Address should match");
            Assert.IsFalse(account.CanSign, "Account from address should not be able to sign");
        }
        
        [Test]
        public void TestFromScriptHash_ShouldCreateValidAccount()
        {
            // Act
            var account = Account.FromScriptHash(testScriptHash);
            
            // Assert
            Assert.IsNotNull(account, "Account should not be null");
            Assert.AreEqual(testAddress, account.Address, "Address should match script hash");
            Assert.AreEqual(testScriptHash, account.GetScriptHash(), "Script hash should match");
        }
        
        [Test]
        public void TestFromPublicKey_ShouldCreateValidAccount()
        {
            // Arrange
            var publicKey = testKeyPair.PublicKey;
            
            // Act
            var account = Account.FromPublicKey(publicKey);
            
            // Assert
            Assert.IsNotNull(account, "Account should not be null");
            Assert.AreEqual(testAddress, account.Address, "Address should match public key");
            Assert.IsNotNull(account.VerificationScript, "Verification script should not be null");
        }
        
        [Test]
        public async Task TestFromWIF_ShouldCreateValidAccount()
        {
            // Arrange
            var wif = testKeyPair.ExportAsWIF();
            
            // Act
            var account = await Account.FromWIF(wif);
            
            // Assert
            Assert.IsNotNull(account, "Account should not be null");
            Assert.AreEqual(testAddress, account.Address, "Address should match WIF");
            Assert.IsTrue(account.CanSign, "Account from WIF should be able to sign");
        }
        
        #endregion
        
        #region Multi-Signature Tests
        
        [Test]
        public async Task TestCreateMultiSigAccount_WithPublicKeys_ShouldCreateValidAccount()
        {
            // Arrange
            var keyPair1 = await ECKeyPair.CreateEcKeyPair();
            var keyPair2 = await ECKeyPair.CreateEcKeyPair();
            var keyPair3 = await ECKeyPair.CreateEcKeyPair();
            var publicKeys = new[] { keyPair1.PublicKey, keyPair2.PublicKey, keyPair3.PublicKey };
            var signingThreshold = 2;
            
            // Act
            var account = Account.CreateMultiSigAccount(publicKeys.ToList(), signingThreshold);
            
            // Assert
            Assert.IsNotNull(account, "Multi-sig account should not be null");
            Assert.IsTrue(account.IsMultiSig, "Account should be multi-sig");
            Assert.AreEqual(signingThreshold, account.GetSigningThreshold(), "Signing threshold should match");
            Assert.AreEqual(3, account.GetNumberOfParticipants(), "Number of participants should match");
            Assert.IsNotNull(account.VerificationScript, "Verification script should not be null");
        }
        
        [Test]
        public void TestCreateMultiSigAccount_WithParameters_ShouldCreateValidAccount()
        {
            // Arrange
            var signingThreshold = 2;
            var numberOfParticipants = 3;
            
            // Act
            var account = Account.CreateMultiSigAccount(testAddress, signingThreshold, numberOfParticipants);
            
            // Assert
            Assert.IsNotNull(account, "Multi-sig account should not be null");
            Assert.IsTrue(account.IsMultiSig, "Account should be multi-sig");
            Assert.AreEqual(testAddress, account.Address, "Address should match");
            Assert.AreEqual(signingThreshold, account.GetSigningThreshold(), "Signing threshold should match");
            Assert.AreEqual(numberOfParticipants, account.GetNumberOfParticipants(), "Number of participants should match");
        }
        
        [Test]
        public void TestCreateMultiSigAccount_WithInvalidThreshold_ShouldThrow()
        {
            // Act & Assert
            Assert.Throws<ArgumentException>(() => 
                Account.CreateMultiSigAccount(testAddress, 0, 3), 
                "Should throw for zero signing threshold");
                
            Assert.Throws<ArgumentException>(() => 
                Account.CreateMultiSigAccount(testAddress, 4, 3), 
                "Should throw for threshold greater than participants");
        }
        
        [Test]
        public void TestGetSigningThreshold_OnSingleSig_ShouldThrow()
        {
            // Arrange
            var account = new Account(testKeyPair);
            
            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => account.GetSigningThreshold(), 
                "Should throw when getting signing threshold from single-sig account");
        }
        
        [Test]
        public void TestGetNumberOfParticipants_OnSingleSig_ShouldThrow()
        {
            // Arrange
            var account = new Account(testKeyPair);
            
            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => account.GetNumberOfParticipants(), 
                "Should throw when getting number of participants from single-sig account");
        }
        
        #endregion
        
        #region Encryption Tests
        
        [Test]
        public async Task TestEncryptDecryptPrivateKey_ShouldMaintainFunctionality()
        {
            // Arrange
            var account = new Account(testKeyPair);
            var password = "test_password_123";
            var originalAddress = account.Address;
            
            // Act - Encrypt
            await account.EncryptPrivateKey(password);
            
            // Assert - After encryption
            Assert.IsNull(account.KeyPair, "Key pair should be null after encryption");
            Assert.IsTrue(account.HasEncryptedKey, "Should have encrypted key");
            Assert.IsFalse(account.CanSign, "Should not be able to sign when encrypted");
            
            // Act - Decrypt
            await account.DecryptPrivateKey(password);
            
            // Assert - After decryption
            Assert.IsNotNull(account.KeyPair, "Key pair should be restored after decryption");
            Assert.AreEqual(originalAddress, account.Address, "Address should remain the same");
            Assert.IsTrue(account.CanSign, "Should be able to sign after decryption");
        }
        
        [Test]
        public async Task TestEncryptPrivateKey_WithoutKeyPair_ShouldThrow()
        {
            // Arrange
            var account = Account.FromAddress(testAddress);
            var password = "test_password";
            
            // Act & Assert
            var ex = Assert.ThrowsAsync<WalletException>(async () => 
                await account.EncryptPrivateKey(password));
                
            Assert.IsNotNull(ex, "Should throw when trying to encrypt without key pair");
        }
        
        [Test]
        public async Task TestDecryptPrivateKey_WithoutEncryptedKey_ShouldThrow()
        {
            // Arrange
            var account = Account.FromAddress(testAddress);
            var password = "test_password";
            
            // Act & Assert
            var ex = Assert.ThrowsAsync<WalletException>(async () => 
                await account.DecryptPrivateKey(password));
                
            Assert.IsNotNull(ex, "Should throw when trying to decrypt without encrypted key");
        }
        
        #endregion
        
        #region String Representation Tests
        
        [Test]
        public void TestToString_SingleSig_ShouldReturnCorrectFormat()
        {
            // Arrange
            var account = new Account(testKeyPair);
            
            // Act
            var result = account.ToString();
            
            // Assert
            Assert.IsNotNull(result, "ToString should not return null");
            Assert.Contains("SingleSig", result, "Should indicate single-sig account");
            Assert.Contains(testAddress, result, "Should contain the address");
            Assert.Contains("Has Key", result, "Should indicate key availability");
        }
        
        [Test]
        public void TestToString_MultiSig_ShouldReturnCorrectFormat()
        {
            // Arrange
            var account = Account.CreateMultiSigAccount(testAddress, 2, 3);
            
            // Act
            var result = account.ToString();
            
            // Assert
            Assert.IsNotNull(result, "ToString should not return null");
            Assert.Contains("MultiSig(2/3)", result, "Should indicate multi-sig configuration");
            Assert.Contains(testAddress, result, "Should contain the address");
        }
        
        #endregion
        
        #region NEP-6 Conversion Tests
        
        [Test]
        public async Task TestToNEP6Account_WithEncryption_ShouldCreateValidNEP6()
        {
            // Arrange
            var account = new Account(testKeyPair);
            var password = "test_password";
            await account.EncryptPrivateKey(password);
            
            // Act
            var nep6Account = await account.ToNEP6Account();
            
            // Assert
            Assert.IsNotNull(nep6Account, "NEP-6 account should not be null");
            Assert.AreEqual(testAddress, nep6Account.Address, "Address should match");
            Assert.IsNotNull(nep6Account.Key, "Encrypted key should not be null");
            Assert.IsNotNull(nep6Account.Contract, "Contract should not be null");
        }
        
        [Test]
        public async Task TestFromNEP6Account_ShouldCreateValidAccount()
        {
            // Arrange
            var originalAccount = new Account(testKeyPair);
            var password = "test_password";
            await originalAccount.EncryptPrivateKey(password);
            var nep6Account = await originalAccount.ToNEP6Account();
            
            // Act
            var restoredAccount = await Account.FromNEP6Account(nep6Account);
            
            // Assert
            Assert.IsNotNull(restoredAccount, "Restored account should not be null");
            Assert.AreEqual(originalAccount.Address, restoredAccount.Address, "Address should match");
            Assert.AreEqual(originalAccount.HasEncryptedKey, restoredAccount.HasEncryptedKey, "Encryption state should match");
        }
        
        #endregion
        
        #region Validation Tests
        
        [Test]
        public void TestValidAddress_ShouldPass()
        {
            // Known valid addresses for different networks
            var validAddresses = new[]
            {
                "NbnjKGMBJzJ6j5PHeYhjJDaQ5Vy5UYu4Fv", // Testnet
                "NQRLhCpAru9BjGsMwk67vdMwmzKMRgsWpx"  // Mainnet
            };
            
            foreach (var address in validAddresses)
            {
                // Act & Assert
                Assert.DoesNotThrow(() => new Account(address), 
                    $"Should not throw for valid address: {address}");
            }
        }
        
        [Test]
        public void TestInvalidAddress_ShouldThrow()
        {
            // Invalid address formats
            var invalidAddresses = new[]
            {
                "",
                "invalid",
                "1234567890",
                "0x1234567890abcdef",
                "NbnjKGMBJzJ6j5PHeYhjJDaQ5Vy5UYu4Fx" // Invalid checksum
            };
            
            foreach (var address in invalidAddresses)
            {
                // Act & Assert
                Assert.Throws<ArgumentException>(() => new Account(address), 
                    $"Should throw for invalid address: {address}");
            }
        }
        
        #endregion
        
        #region Equality Tests
        
        [Test]
        public void TestAccountEquality_SameAddress_ShouldBeEqual()
        {
            // Arrange
            var account1 = new Account(testAddress);
            var account2 = new Account(testAddress);
            
            // Act & Assert
            Assert.AreEqual(account1.GetScriptHash(), account2.GetScriptHash(), "Accounts with same address should have same script hash");
        }
        
        [Test]
        public async Task TestAccountEquality_DifferentAccounts_ShouldNotBeEqual()
        {
            // Arrange
            var account1 = new Account(testKeyPair);
            var account2 = await Account.Create();
            
            // Act & Assert
            Assert.AreNotEqual(account1.GetScriptHash(), account2.GetScriptHash(), "Different accounts should have different script hashes");
        }
        
        #endregion
        
        #region Error Handling Tests
        
        [Test]
        public async Task TestEncryptPrivateKey_WithEmptyPassword_ShouldThrow()
        {
            // Arrange
            var account = new Account(testKeyPair);
            
            // Act & Assert
            Assert.ThrowsAsync<ArgumentException>(async () => 
                await account.EncryptPrivateKey(""), 
                "Should throw for empty password");
                
            Assert.ThrowsAsync<ArgumentException>(async () => 
                await account.EncryptPrivateKey(null), 
                "Should throw for null password");
        }
        
        [Test]
        public async Task TestDecryptPrivateKey_WithWrongPassword_ShouldThrow()
        {
            // Arrange
            var account = new Account(testKeyPair);
            var correctPassword = "correct_password";
            var wrongPassword = "wrong_password";
            
            await account.EncryptPrivateKey(correctPassword);
            
            // Act & Assert
            Assert.ThrowsAsync<WalletException>(async () => 
                await account.DecryptPrivateKey(wrongPassword), 
                "Should throw for wrong password");
        }
        
        #endregion
    }
}
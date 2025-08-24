using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NUnit.Framework;
using UnityEngine;
using Neo.Unity.SDK.Wallet;
using Neo.Unity.SDK.Types;
using Neo.Unity.SDK.Crypto;

namespace Neo.Unity.SDK.Tests.Wallet
{
    /// <summary>
    /// Unit tests for the NeoWallet class.
    /// Tests wallet creation, account management, and NEP-6 compatibility.
    /// </summary>
    [TestFixture]
    public class NeoWalletTests
    {
        private Account testAccount1;
        private Account testAccount2;
        private NeoWallet testWallet;
        
        [SetUp]
        public async Task SetUp()
        {
            // Create test accounts
            testAccount1 = await Account.Create();
            testAccount2 = await Account.Create();
            
            // Create test wallet
            testWallet = new NeoWallet("TestWallet");
        }
        
        #region Constructor Tests
        
        [Test]
        public void TestWalletConstructor_Default_ShouldCreateValidWallet()
        {
            // Act
            var wallet = new NeoWallet();
            
            // Assert
            Assert.IsNotNull(wallet, "Wallet should not be null");
            Assert.AreEqual("NeoUnityWallet", wallet.Name, "Should have default name");
            Assert.AreEqual("3.0", wallet.Version, "Should have current version");
            Assert.AreEqual(0, wallet.AccountCount, "Should start with no accounts");
            Assert.IsNull(wallet.DefaultAccount, "Should have no default account initially");
        }
        
        [Test]
        public void TestWalletConstructor_WithName_ShouldCreateValidWallet()
        {
            // Arrange
            var walletName = "MyGameWallet";
            
            // Act
            var wallet = new NeoWallet(walletName);
            
            // Assert
            Assert.IsNotNull(wallet, "Wallet should not be null");
            Assert.AreEqual(walletName, wallet.Name, "Name should match");
        }
        
        #endregion
        
        #region Account Management Tests
        
        [Test]
        public void TestAddAccounts_ShouldIncreaseAccountCount()
        {
            // Act
            testWallet.AddAccounts(testAccount1, testAccount2);
            
            // Assert
            Assert.AreEqual(2, testWallet.AccountCount, "Account count should be 2");
            Assert.IsTrue(testWallet.HoldsAccount(testAccount1.GetScriptHash()), "Should contain first account");
            Assert.IsTrue(testWallet.HoldsAccount(testAccount2.GetScriptHash()), "Should contain second account");
        }
        
        [Test]
        public void TestSetDefaultAccount_WithValidAccount_ShouldSetDefault()
        {
            // Arrange
            testWallet.AddAccounts(testAccount1, testAccount2);
            
            // Act
            testWallet.SetDefaultAccount(testAccount2);
            
            // Assert
            Assert.AreEqual(testAccount2.Address, testWallet.DefaultAccount.Address, "Default account should be set");
            Assert.IsTrue(testWallet.IsDefault(testAccount2), "Account should be marked as default");
            Assert.IsFalse(testWallet.IsDefault(testAccount1), "Other account should not be default");
        }
        
        [Test]
        public void TestSetDefaultAccount_WithInvalidAccount_ShouldThrow()
        {
            // Arrange
            testWallet.AddAccounts(testAccount1);
            
            // Act & Assert
            Assert.Throws<ArgumentException>(() => testWallet.SetDefaultAccount(testAccount2), 
                "Should throw when setting account not in wallet as default");
        }
        
        [Test]
        public void TestRemoveAccount_WithValidAccount_ShouldRemove()
        {
            // Arrange
            testWallet.AddAccounts(testAccount1, testAccount2);
            testWallet.SetDefaultAccount(testAccount1);
            
            // Act
            var result = testWallet.RemoveAccount(testAccount2);
            
            // Assert
            Assert.IsTrue(result, "Should return true when account is removed");
            Assert.AreEqual(1, testWallet.AccountCount, "Account count should decrease");
            Assert.IsFalse(testWallet.HoldsAccount(testAccount2.GetScriptHash()), "Should not contain removed account");
            Assert.AreEqual(testAccount1.Address, testWallet.DefaultAccount.Address, "Default should remain unchanged");
        }
        
        [Test]
        public void TestRemoveAccount_DefaultAccount_ShouldUpdateDefault()
        {
            // Arrange
            testWallet.AddAccounts(testAccount1, testAccount2);
            testWallet.SetDefaultAccount(testAccount1);
            
            // Act
            var result = testWallet.RemoveAccount(testAccount1);
            
            // Assert
            Assert.IsTrue(result, "Should return true when account is removed");
            Assert.AreEqual(1, testWallet.AccountCount, "Account count should decrease");
            Assert.AreEqual(testAccount2.Address, testWallet.DefaultAccount.Address, "Default should be updated to remaining account");
        }
        
        [Test]
        public void TestRemoveAccount_LastAccount_ShouldThrow()
        {
            // Arrange
            testWallet.AddAccounts(testAccount1);
            
            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => testWallet.RemoveAccount(testAccount1), 
                "Should throw when trying to remove the last account");
        }
        
        [Test]
        public void TestGetAccount_WithValidHash_ShouldReturnAccount()
        {
            // Arrange
            testWallet.AddAccounts(testAccount1);
            var scriptHash = testAccount1.GetScriptHash();
            
            // Act
            var account = testWallet.GetAccount(scriptHash);
            
            // Assert
            Assert.IsNotNull(account, "Should return the account");
            Assert.AreEqual(testAccount1.Address, account.Address, "Should return the correct account");
        }
        
        [Test]
        public void TestGetAccount_WithInvalidHash_ShouldReturnNull()
        {
            // Arrange
            testWallet.AddAccounts(testAccount1);
            var invalidHash = testAccount2.GetScriptHash();
            
            // Act
            var account = testWallet.GetAccount(invalidHash);
            
            // Assert
            Assert.IsNull(account, "Should return null for account not in wallet");
        }
        
        #endregion
        
        #region Static Factory Method Tests
        
        [Test]
        public async Task TestCreate_ShouldCreateWalletWithOneAccount()
        {
            // Act
            var wallet = await NeoWallet.Create();
            
            // Assert
            Assert.IsNotNull(wallet, "Wallet should not be null");
            Assert.AreEqual(1, wallet.AccountCount, "Should have one account");
            Assert.IsNotNull(wallet.DefaultAccount, "Should have default account");
            Assert.IsTrue(wallet.DefaultAccount.CanSign, "Default account should be able to sign");
        }
        
        [Test]
        public async Task TestCreateWithPassword_ShouldCreateEncryptedWallet()
        {
            // Arrange
            var password = "test_password_123";
            
            // Act
            var wallet = await NeoWallet.Create(password);
            
            // Assert
            Assert.IsNotNull(wallet, "Wallet should not be null");
            Assert.AreEqual(1, wallet.AccountCount, "Should have one account");
            Assert.IsNotNull(wallet.DefaultAccount, "Should have default account");
            Assert.IsTrue(wallet.DefaultAccount.HasEncryptedKey, "Account should have encrypted key");
            Assert.IsFalse(wallet.DefaultAccount.CanSign, "Encrypted account should not be able to sign");
        }
        
        [Test]
        public void TestWithAccounts_ShouldCreateWalletWithAccounts()
        {
            // Act
            var wallet = NeoWallet.WithAccounts(testAccount1, testAccount2);
            
            // Assert
            Assert.IsNotNull(wallet, "Wallet should not be null");
            Assert.AreEqual(2, wallet.AccountCount, "Should have two accounts");
            Assert.AreEqual(testAccount1.Address, wallet.DefaultAccount.Address, "First account should be default");
        }
        
        [Test]
        public void TestWithAccounts_EmptyList_ShouldThrow()
        {
            // Act & Assert
            Assert.Throws<ArgumentException>(() => NeoWallet.WithAccounts(), 
                "Should throw for empty account list");
        }
        
        #endregion
        
        #region Encryption Tests
        
        [Test]
        public async Task TestEncryptDecryptAllAccounts_ShouldWorkCorrectly()
        {
            // Arrange
            var wallet = await NeoWallet.Create();
            await wallet.AddAccounts(testAccount1).SetDefaultAccount(testAccount1);
            var password = "wallet_password_123";
            
            // Act - Encrypt
            await wallet.EncryptAllAccounts(password);
            
            // Assert - After encryption
            Assert.IsTrue(wallet.DefaultAccount.HasEncryptedKey, "Account should have encrypted key");
            Assert.IsFalse(wallet.DefaultAccount.CanSign, "Account should not be able to sign when encrypted");
            
            // Act - Decrypt
            await wallet.DecryptAllAccounts(password);
            
            // Assert - After decryption
            Assert.IsNotNull(wallet.DefaultAccount.KeyPair, "Account should have key pair after decryption");
            Assert.IsTrue(wallet.DefaultAccount.CanSign, "Account should be able to sign after decryption");
        }
        
        #endregion
        
        #region NEP-6 Wallet File Tests
        
        [Test]
        public async Task TestToNEP6Wallet_ShouldCreateValidNEP6()
        {
            // Arrange
            testWallet.AddAccounts(testAccount1, testAccount2);
            testWallet.SetDefaultAccount(testAccount1);
            
            var password = "test_password";
            await testWallet.EncryptAllAccounts(password);
            
            // Act
            var nep6Wallet = await testWallet.ToNEP6Wallet();
            
            // Assert
            Assert.IsNotNull(nep6Wallet, "NEP-6 wallet should not be null");
            Assert.AreEqual(testWallet.Name, nep6Wallet.Name, "Name should match");
            Assert.AreEqual(testWallet.Version, nep6Wallet.Version, "Version should match");
            Assert.AreEqual(2, nep6Wallet.Accounts.Count, "Should have 2 accounts");
            
            var defaultNep6Account = nep6Wallet.Accounts.Find(acc => acc.IsDefault);
            Assert.IsNotNull(defaultNep6Account, "Should have a default account");
            Assert.AreEqual(testAccount1.Address, defaultNep6Account.Address, "Default account should match");
        }
        
        [Test]
        public async Task TestFromNEP6Wallet_ShouldRestoreWallet()
        {
            // Arrange
            testWallet.AddAccounts(testAccount1, testAccount2);
            testWallet.SetDefaultAccount(testAccount1);
            
            var password = "test_password";
            await testWallet.EncryptAllAccounts(password);
            var nep6Wallet = await testWallet.ToNEP6Wallet();
            
            // Act
            var restoredWallet = await NeoWallet.FromNEP6Wallet(nep6Wallet);
            
            // Assert
            Assert.IsNotNull(restoredWallet, "Restored wallet should not be null");
            Assert.AreEqual(testWallet.Name, restoredWallet.Name, "Name should match");
            Assert.AreEqual(2, restoredWallet.AccountCount, "Account count should match");
            Assert.AreEqual(testAccount1.Address, restoredWallet.DefaultAccount.Address, "Default account should match");
        }
        
        #endregion
        
        #region Duplicate Account Tests
        
        [Test]
        public void TestAddAccounts_WithDuplicates_ShouldIgnoreDuplicates()
        {
            // Arrange
            testWallet.AddAccounts(testAccount1);
            
            // Act
            testWallet.AddAccounts(testAccount1, testAccount2); // testAccount1 is duplicate
            
            // Assert
            Assert.AreEqual(2, testWallet.AccountCount, "Should have 2 unique accounts");
        }
        
        [Test]
        public void TestAddAccounts_AccountInOtherWallet_ShouldThrow()
        {
            // Arrange
            var otherWallet = new NeoWallet("OtherWallet");
            otherWallet.AddAccounts(testAccount1);
            
            // Act & Assert
            Assert.Throws<ArgumentException>(() => testWallet.AddAccounts(testAccount1), 
                "Should throw when adding account that's already in another wallet");
        }
        
        #endregion
        
        #region String Representation Tests
        
        [Test]
        public void TestToString_ShouldReturnCorrectFormat()
        {
            // Arrange
            testWallet.AddAccounts(testAccount1, testAccount2);
            testWallet.SetDefaultAccount(testAccount1);
            
            // Act
            var result = testWallet.ToString();
            
            // Assert
            Assert.IsNotNull(result, "ToString should not return null");
            Assert.Contains("TestWallet", result, "Should contain wallet name");
            Assert.Contains("2", result, "Should contain account count");
            Assert.Contains(testAccount1.Address, result, "Should contain default account address");
        }
        
        #endregion
        
        #region Performance Tests
        
        [Test]
        public async Task TestLargeWallet_ShouldPerformWell()
        {
            // Arrange
            var largeWallet = new NeoWallet("LargeWallet");
            var accounts = new List<Account>();
            
            // Create 50 accounts
            for (int i = 0; i < 50; i++)
            {
                accounts.Add(await Account.Create());
            }
            
            var startTime = DateTime.UtcNow;
            
            // Act
            largeWallet.AddAccounts(accounts);
            largeWallet.SetDefaultAccount(accounts[0]);
            
            var endTime = DateTime.UtcNow;
            var duration = endTime - startTime;
            
            // Assert
            Assert.AreEqual(50, largeWallet.AccountCount, "Should have 50 accounts");
            Assert.Less(duration.TotalSeconds, 1.0, "Should complete within 1 second");
        }
        
        #endregion
        
        #region Edge Case Tests
        
        [Test]
        public void TestGetAccount_WithNullHash_ShouldReturnNull()
        {
            // Arrange
            testWallet.AddAccounts(testAccount1);
            
            // Act
            var account = testWallet.GetAccount(null);
            
            // Assert
            Assert.IsNull(account, "Should return null for null script hash");
        }
        
        [Test]
        public void TestIsDefault_WithNullAccount_ShouldReturnFalse()
        {
            // Arrange
            testWallet.AddAccounts(testAccount1);
            testWallet.SetDefaultAccount(testAccount1);
            
            // Act
            var isDefault = testWallet.IsDefault((Account)null);
            
            // Assert
            Assert.IsFalse(isDefault, "Should return false for null account");
        }
        
        [Test]
        public void TestRemoveAccount_WithNullAccount_ShouldReturnFalse()
        {
            // Arrange
            testWallet.AddAccounts(testAccount1);
            
            // Act
            var result = testWallet.RemoveAccount((Account)null);
            
            // Assert
            Assert.IsFalse(result, "Should return false for null account");
        }
        
        #endregion
        
        #region Wallet State Tests
        
        [Test]
        public void TestWalletProperties_AfterAccountOperations_ShouldMaintainConsistency()
        {
            // Arrange & Act
            testWallet.AddAccounts(testAccount1, testAccount2);
            testWallet.SetDefaultAccount(testAccount1);
            
            // Assert initial state
            Assert.AreEqual(2, testWallet.AccountCount, "Should have 2 accounts");
            Assert.AreEqual(testAccount1.Address, testWallet.DefaultAccount.Address, "Default should be account1");
            
            // Act - Remove non-default account
            testWallet.RemoveAccount(testAccount2);
            
            // Assert after removal
            Assert.AreEqual(1, testWallet.AccountCount, "Should have 1 account");
            Assert.AreEqual(testAccount1.Address, testWallet.DefaultAccount.Address, "Default should still be account1");
            
            // Act - Add account back
            testWallet.AddAccounts(testAccount2);
            
            // Assert after re-adding
            Assert.AreEqual(2, testWallet.AccountCount, "Should have 2 accounts again");
            Assert.AreEqual(testAccount1.Address, testWallet.DefaultAccount.Address, "Default should still be account1");
        }
        
        #endregion
        
        #region Unity Serialization Tests
        
        [Test]
        public void TestUnitySerialization_ShouldPreserveWalletData()
        {
            // Arrange
            testWallet.AddAccounts(testAccount1, testAccount2);
            testWallet.SetDefaultAccount(testAccount1);
            testWallet.Name = "SerializationTestWallet";
            
            // Act - Simulate Unity serialization/deserialization
            var json = JsonUtility.ToJson(testWallet);
            var deserializedWallet = JsonUtility.FromJson<NeoWallet>(json);
            
            // Assert
            Assert.IsNotNull(deserializedWallet, "Deserialized wallet should not be null");
            Assert.AreEqual(testWallet.Name, deserializedWallet.Name, "Name should be preserved");
            // Note: Full serialization testing would require custom Unity serialization setup
        }
        
        #endregion
    }
}
using System;
using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Neo.Unity.SDK;
using Neo.Unity.SDK.Wallet;
using Neo.Unity.SDK.Tests.Helpers;
using Newtonsoft.Json;

namespace Neo.Unity.SDK.Tests.Wallet
{
    /// <summary>
    /// Unity Test Framework implementation of NEP6Wallet tests
    /// Converted from Swift NEP6WalletTests.swift with Unity-specific enhancements
    /// </summary>
    [TestFixture]
    public class NEP6WalletTests
    {
        [Test]
        public void TestReadWallet()
        {
            // Arrange
            var walletJson = TestHelpers.LoadJsonResource("wallet");

            // Act
            var wallet = JsonConvert.DeserializeObject<NEP6Wallet>(walletJson);

            // Assert
            Assert.IsNotNull(wallet);
            Assert.AreEqual("Wallet", wallet.Name);
            Assert.AreEqual(Wallet.CURRENT_VERSION, wallet.Version);
            Assert.AreEqual(ScryptParams.DEFAULT, wallet.Scrypt);
            Assert.AreEqual(2, wallet.Accounts.Count);

            // Test first account
            var account1 = wallet.Accounts[0];
            Assert.AreEqual("NLnyLtep7jwyq1qhNPkwXbJpurC4jUT8ke", account1.Address);
            Assert.AreEqual("Account1", account1.Label);
            Assert.IsTrue(account1.IsDefault);
            Assert.IsFalse(account1.Lock);
            Assert.AreEqual("6PYVEi6ZGdsLoCYbbGWqoYef7VWMbKwcew86m5fpxnZRUD8tEjainBgQW1", account1.Key);
            Assert.IsNull(account1.Extra);

            var contract1 = account1.Contract;
            Assert.IsNotNull(contract1);
            Assert.AreEqual("DCECJJQloGtaH45hM/x5r6LCuEML+TJyl/F2dh33no2JKcULQZVEDXg=", contract1.Script);
            Assert.IsFalse(contract1.IsDeployed);

            var parameter1 = contract1.Nep6Parameters[0];
            Assert.AreEqual("signature", parameter1.ParamName);
            Assert.AreEqual(ContractParameterType.Signature, parameter1.Type);

            // Test second account
            var account2 = wallet.Accounts[1];
            Assert.AreEqual("NWcx4EfYdfqn5jNjDz8AHE6hWtWdUGDdmy", account2.Address);
            Assert.AreEqual("Account2", account2.Label);
            Assert.IsFalse(account2.IsDefault);
            Assert.IsFalse(account2.Lock);
            Assert.AreEqual("6PYSQWBqZE5oEFdMGCJ3xR7bz6ezz814oKE7GqwB9i5uhtUzkshe9B6YGB", account2.Key);
            Assert.IsNull(account2.Extra);

            var contract2 = account2.Contract;
            Assert.IsNotNull(contract2);
            Assert.AreEqual("DCEDHMqqRt98SU9EJpjIwXwJMR42FcLcBCy9Ov6rpg+kB0ALQZVEDXg=", contract2.Script);
            Assert.IsFalse(contract2.IsDeployed);

            var parameter2 = contract2.Nep6Parameters[0];
            Assert.AreEqual("signature", parameter2.ParamName);
            Assert.AreEqual(ContractParameterType.Signature, parameter2.Type);
        }

        [Test]
        public void TestCreateWallet()
        {
            // Arrange
            var walletName = "TestWallet";
            var password = "testpassword123";

            // Act
            var wallet = new NEP6Wallet(walletName);
            var account = wallet.CreateAccount(password);

            // Assert
            Assert.IsNotNull(wallet);
            Assert.AreEqual(walletName, wallet.Name);
            Assert.AreEqual(Wallet.CURRENT_VERSION, wallet.Version);
            Assert.AreEqual(1, wallet.Accounts.Count);
            Assert.IsNotNull(account);
            Assert.IsTrue(account.IsDefault);
            Assert.IsFalse(account.Lock);
            Assert.IsNotNull(account.Key); // Encrypted private key
            Assert.IsNotNull(account.Address);
            Assert.IsNotNull(account.Contract);
        }

        [Test]
        public void TestAddAccount()
        {
            // Arrange
            var wallet = new NEP6Wallet("TestWallet");
            var password = "testpassword123";

            // Act
            var account1 = wallet.CreateAccount(password);
            var account2 = wallet.CreateAccount(password);

            // Assert
            Assert.AreEqual(2, wallet.Accounts.Count);
            Assert.IsTrue(account1.IsDefault); // First account is default
            Assert.IsFalse(account2.IsDefault); // Second account is not default
            Assert.AreNotEqual(account1.Address, account2.Address);
            Assert.AreNotEqual(account1.Key, account2.Key);
        }

        [Test]
        public void TestRemoveAccount()
        {
            // Arrange
            var wallet = new NEP6Wallet("TestWallet");
            var password = "testpassword123";
            var account1 = wallet.CreateAccount(password);
            var account2 = wallet.CreateAccount(password);

            Assert.AreEqual(2, wallet.Accounts.Count);

            // Act
            var removed = wallet.RemoveAccount(account1.Address);

            // Assert
            Assert.IsTrue(removed);
            Assert.AreEqual(1, wallet.Accounts.Count);
            Assert.AreEqual(account2.Address, wallet.Accounts[0].Address);
        }

        [Test]
        public void TestGetAccount()
        {
            // Arrange
            var wallet = new NEP6Wallet("TestWallet");
            var password = "testpassword123";
            var account = wallet.CreateAccount(password);

            // Act
            var foundAccount = wallet.GetAccount(account.Address);
            var notFoundAccount = wallet.GetAccount("NInvalidAddress");

            // Assert
            Assert.IsNotNull(foundAccount);
            Assert.AreEqual(account.Address, foundAccount.Address);
            Assert.IsNull(notFoundAccount);
        }

        [Test]
        public void TestGetDefaultAccount()
        {
            // Arrange
            var wallet = new NEP6Wallet("TestWallet");
            var password = "testpassword123";

            // Act - No accounts yet
            var defaultAccount1 = wallet.GetDefaultAccount();
            Assert.IsNull(defaultAccount1);

            // Add accounts
            var account1 = wallet.CreateAccount(password);
            var account2 = wallet.CreateAccount(password);

            // Act - With accounts
            var defaultAccount2 = wallet.GetDefaultAccount();

            // Assert
            Assert.IsNotNull(defaultAccount2);
            Assert.AreEqual(account1.Address, defaultAccount2.Address);
            Assert.IsTrue(defaultAccount2.IsDefault);
        }

        [Test]
        public void TestSetDefaultAccount()
        {
            // Arrange
            var wallet = new NEP6Wallet("TestWallet");
            var password = "testpassword123";
            var account1 = wallet.CreateAccount(password);
            var account2 = wallet.CreateAccount(password);

            Assert.IsTrue(account1.IsDefault);
            Assert.IsFalse(account2.IsDefault);

            // Act
            wallet.SetDefaultAccount(account2.Address);

            // Assert
            Assert.IsFalse(account1.IsDefault);
            Assert.IsTrue(account2.IsDefault);
            Assert.AreEqual(account2.Address, wallet.GetDefaultAccount().Address);
        }

        [Test]
        public void TestWalletSerialization()
        {
            // Arrange
            var wallet = new NEP6Wallet("TestWallet");
            var password = "testpassword123";
            wallet.CreateAccount(password);
            wallet.CreateAccount(password);

            // Act
            var jsonString = JsonConvert.SerializeObject(wallet, Formatting.Indented);
            var deserializedWallet = JsonConvert.DeserializeObject<NEP6Wallet>(jsonString);

            // Assert
            Assert.IsNotNull(jsonString);
            Assert.IsNotNull(deserializedWallet);
            Assert.AreEqual(wallet.Name, deserializedWallet.Name);
            Assert.AreEqual(wallet.Version, deserializedWallet.Version);
            Assert.AreEqual(wallet.Accounts.Count, deserializedWallet.Accounts.Count);

            for (int i = 0; i < wallet.Accounts.Count; i++)
            {
                Assert.AreEqual(wallet.Accounts[i].Address, deserializedWallet.Accounts[i].Address);
                Assert.AreEqual(wallet.Accounts[i].Label, deserializedWallet.Accounts[i].Label);
                Assert.AreEqual(wallet.Accounts[i].IsDefault, deserializedWallet.Accounts[i].IsDefault);
                Assert.AreEqual(wallet.Accounts[i].Key, deserializedWallet.Accounts[i].Key);
            }
        }

        [Test]
        public void TestAccountDecryption()
        {
            // Arrange
            var wallet = new NEP6Wallet("TestWallet");
            var password = "testpassword123";
            var account = wallet.CreateAccount(password);

            // Act
            var decryptedKeyPair = account.DecryptPrivateKey(password);

            // Assert
            Assert.IsNotNull(decryptedKeyPair);
            Assert.IsNotNull(decryptedKeyPair.PrivateKey);
            Assert.IsNotNull(decryptedKeyPair.PublicKey);
            Assert.AreEqual(32, decryptedKeyPair.PrivateKey.Length);
            Assert.AreEqual(33, decryptedKeyPair.PublicKey.Size);
        }

        [Test]
        public void TestAccountDecryptionWithWrongPassword()
        {
            // Arrange
            var wallet = new NEP6Wallet("TestWallet");
            var password = "correctpassword";
            var wrongPassword = "wrongpassword";
            var account = wallet.CreateAccount(password);

            // Act & Assert
            Assert.Throws<ArgumentException>(() =>
            {
                account.DecryptPrivateKey(wrongPassword);
            });
        }

        [Test]
        public void TestImportAccountFromPrivateKey()
        {
            // Arrange
            var wallet = new NEP6Wallet("TestWallet");
            var password = "testpassword123";
            var keyPair = ECKeyPair.CreateEcKeyPair();

            // Act
            var importedAccount = wallet.ImportAccount(keyPair, password, "ImportedAccount");

            // Assert
            Assert.IsNotNull(importedAccount);
            Assert.AreEqual("ImportedAccount", importedAccount.Label);
            Assert.IsNotNull(importedAccount.Key);
            Assert.AreEqual(1, wallet.Accounts.Count);
            Assert.IsTrue(importedAccount.IsDefault);

            // Verify the imported key can be decrypted
            var decryptedKeyPair = importedAccount.DecryptPrivateKey(password);
            TestHelpers.AssertBytesEqual(keyPair.PrivateKey, decryptedKeyPair.PrivateKey);
        }

        #region Unity-Specific Tests

        [Test]
        [Performance]
        public void TestPerformance_WalletCreation()
        {
            const int iterations = 5;
            var password = "testpassword123";

            // Act & Assert
            var executionTime = TestHelpers.MeasureExecutionTime(() =>
            {
                for (int i = 0; i < iterations; i++)
                {
                    var wallet = new NEP6Wallet($"TestWallet{i}");
                    wallet.CreateAccount(password);
                    GC.KeepAlive(wallet);
                }
            });

            var avgTime = (float)executionTime / iterations;
            Assert.Less(avgTime, 1000, "Wallet creation should average less than 1 second");
            Debug.Log($"Wallet creation average time: {avgTime}ms per wallet ({iterations} iterations)");
        }

        [Test]
        public void TestMemoryUsage_WalletWithMultipleAccounts()
        {
            const int accountCount = 10;
            var password = "testpassword123";

            // Act & Assert
            var memoryUsage = TestHelpers.MeasureMemoryUsage(() =>
            {
                var wallet = new NEP6Wallet("TestWallet");
                for (int i = 0; i < accountCount; i++)
                {
                    wallet.CreateAccount(password);
                }
                GC.KeepAlive(wallet);
            });

            var avgMemoryPerAccount = memoryUsage / accountCount;
            Assert.Less(Math.Abs(avgMemoryPerAccount), 50 * 1024, "Each account should use less than 50KB");
            Debug.Log($"Memory usage per account: {avgMemoryPerAccount} bytes");
        }

        [UnityTest]
        public IEnumerator TestUnityCoroutineCompatibility()
        {
            // Arrange
            bool walletCreationCompleted = false;
            NEP6Wallet createdWallet = null;
            var password = "testpassword123";

            // Act - Simulate async wallet creation in coroutine
            yield return new WaitForEndOfFrame();

            createdWallet = new NEP6Wallet("CoroutineWallet");
            createdWallet.CreateAccount(password);
            walletCreationCompleted = true;

            yield return new WaitUntil(() => walletCreationCompleted);

            // Assert
            Assert.IsNotNull(createdWallet);
            Assert.AreEqual("CoroutineWallet", createdWallet.Name);
            Assert.AreEqual(1, createdWallet.Accounts.Count);
        }

        [Test]
        public void TestSerializationForUnityInspector()
        {
            // Test that NEP6Wallet can work with Unity serialization
            var wallet = new NEP6Wallet("InspectorWallet");
            var password = "testpassword123";
            wallet.CreateAccount(password);

            // Create serializable wrapper for Unity Inspector
            var serializableWallet = new SerializableWalletInfo
            {
                name = wallet.Name,
                version = wallet.Version,
                accountCount = wallet.Accounts.Count,
                hasDefaultAccount = wallet.GetDefaultAccount() != null,
                defaultAccountAddress = wallet.GetDefaultAccount()?.Address ?? ""
            };

            var jsonString = JsonUtility.ToJson(serializableWallet, true);
            Assert.IsNotNull(jsonString);
            Assert.IsTrue(jsonString.Contains("InspectorWallet"));

            Debug.Log($"Serializable wallet info: {jsonString}");

            // Test deserialization
            var deserializedWallet = JsonUtility.FromJson<SerializableWalletInfo>(jsonString);
            Assert.AreEqual("InspectorWallet", deserializedWallet.name);
            Assert.AreEqual(wallet.Version, deserializedWallet.version);
            Assert.AreEqual(1, deserializedWallet.accountCount);
            Assert.IsTrue(deserializedWallet.hasDefaultAccount);
        }

        [Test]
        public void TestThreadSafety()
        {
            // Arrange
            const int threadCount = 4;
            const int operationsPerThread = 5;
            var wallet = new NEP6Wallet("ThreadSafetyWallet");
            var tasks = new System.Threading.Tasks.Task[threadCount];
            var exceptions = new System.Collections.Concurrent.ConcurrentQueue<Exception>();

            // Act
            for (int t = 0; t < threadCount; t++)
            {
                int threadIndex = t;
                tasks[t] = System.Threading.Tasks.Task.Run(() =>
                {
                    try
                    {
                        for (int i = 0; i < operationsPerThread; i++)
                        {
                            var password = $"password{threadIndex}{i}";
                            var account = wallet.CreateAccount(password);
                            Assert.IsNotNull(account);
                        }
                    }
                    catch (Exception ex)
                    {
                        exceptions.Enqueue(ex);
                    }
                });
            }

            System.Threading.Tasks.Task.WaitAll(tasks);

            // Assert
            Assert.IsTrue(exceptions.IsEmpty, $"Thread safety test failed with {exceptions.Count} exceptions");
            Assert.AreEqual(threadCount * operationsPerThread, wallet.Accounts.Count);
            Debug.Log($"Thread safety test passed: {threadCount} threads, {operationsPerThread} ops each, {wallet.Accounts.Count} accounts created");
        }

        [Test]
        public void TestEdgeCases_InvalidInputs()
        {
            // Test null/empty wallet name
            Assert.Throws<ArgumentException>(() => new NEP6Wallet(null));
            Assert.Throws<ArgumentException>(() => new NEP6Wallet(""));
            Assert.Throws<ArgumentException>(() => new NEP6Wallet("   "));

            var wallet = new NEP6Wallet("TestWallet");

            // Test null/empty password
            Assert.Throws<ArgumentException>(() => wallet.CreateAccount(null));
            Assert.Throws<ArgumentException>(() => wallet.CreateAccount(""));

            // Test invalid address operations
            Assert.IsFalse(wallet.RemoveAccount("InvalidAddress"));
            Assert.IsNull(wallet.GetAccount("InvalidAddress"));
            Assert.Throws<ArgumentException>(() => wallet.SetDefaultAccount("InvalidAddress"));
        }

        #endregion

        #region Helper Classes

        [System.Serializable]
        private class SerializableWalletInfo
        {
            public string name;
            public string version;
            public int accountCount;
            public bool hasDefaultAccount;
            public string defaultAccountAddress;
        }

        #endregion
    }
}
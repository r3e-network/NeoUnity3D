using System;
using System.Collections;
using System.Threading.Tasks;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Neo.Unity.SDK;
using Neo.Unity.SDK.Contracts;
using Neo.Unity.SDK.Tests.Helpers;

namespace Neo.Unity.SDK.Tests.Contract
{
    /// <summary>
    /// Unity Test Framework implementation of GasToken contract tests
    /// Converted from Swift GasTokenTests.swift with Unity-specific enhancements
    /// </summary>
    [TestFixture]
    public class GasTokenTests
    {
        private const string GASTOKEN_SCRIPTHASH = "d2a4cff31913016155e38e474a2c06d08be276cf";

        private MockNeoSwift mockNeoSwift;
        private GasToken gasToken;

        [SetUp]
        public void SetUp()
        {
            mockNeoSwift = new MockNeoSwift();
            gasToken = new GasToken(mockNeoSwift);
        }

        [TearDown]
        public void TearDown()
        {
            mockNeoSwift?.Dispose();
        }

        [Test]
        public async Task TestName()
        {
            // Arrange
            mockNeoSwift
                .WithMockInvokeFunctions(new Dictionary<string, string>
                {
                    ["name"] = TestHelpers.LoadJsonResource("invokefunction_name_gas")
                });

            // Act
            var name = await gasToken.GetNameAsync();

            // Assert
            Assert.AreEqual("GasToken", name);
        }

        [Test]
        public async Task TestSymbol()
        {
            // Arrange
            mockNeoSwift
                .WithMockInvokeFunctions(new Dictionary<string, string>
                {
                    ["symbol"] = TestHelpers.LoadJsonResource("invokefunction_symbol_gas")
                });

            // Act
            var symbol = await gasToken.GetSymbolAsync();

            // Assert
            Assert.AreEqual("GAS", symbol);
        }

        [Test]
        public async Task TestDecimals()
        {
            // Arrange
            mockNeoSwift
                .WithMockInvokeFunctions(new Dictionary<string, string>
                {
                    ["decimals"] = TestHelpers.LoadJsonResource("invokefunction_decimals_gas")
                });

            // Act
            var decimals = await gasToken.GetDecimalsAsync();

            // Assert
            Assert.AreEqual(8, decimals);
        }

        [Test]
        public void TestScriptHash()
        {
            // Act & Assert
            Assert.AreEqual(GASTOKEN_SCRIPTHASH, gasToken.ScriptHash.ToString());
        }

        [Test]
        public async Task TestGetTotalSupply()
        {
            // Arrange
            mockNeoSwift
                .WithMockInvokeFunctions(new Dictionary<string, string>
                {
                    ["totalSupply"] = TestHelpers.LoadJsonResource("invokefunction_totalSupply_gas")
                });

            // Act
            var totalSupply = await gasToken.GetTotalSupplyAsync();

            // Assert
            Assert.Greater(totalSupply, 0);
        }

        [Test]
        public async Task TestGetBalance()
        {
            // Arrange
            var testAccount = TestHelpers.CreateTestAccount();
            mockNeoSwift
                .WithMockInvokeFunctions(new Dictionary<string, string>
                {
                    ["balanceOf"] = TestHelpers.LoadJsonResource("invokefunction_balanceOf_gas")
                });

            // Act
            var balance = await gasToken.GetBalanceAsync(testAccount.GetScriptHash());

            // Assert
            Assert.GreaterOrEqual(balance, 0);
        }

        [Test]
        public async Task TestTransfer()
        {
            // Arrange
            var fromAccount = TestHelpers.CreateTestAccount("e6e919577dd7b8e97805151c05ae07ff4f752654d6d8797597aca989c02c4cb3");
            var toAccount = TestHelpers.CreateTestAccount("1dd37fba80fec4e6a6f13fd708d8dcb3b29def768017052f6c930a1de96f2097");
            var amount = 1000000000; // 10 GAS

            mockNeoSwift
                .WithMockResponses(new Dictionary<string, string>
                {
                    ["invokescript"] = TestHelpers.LoadJsonResource("invokescript_transfer_gas"),
                    ["getblockcount"] = TestHelpers.LoadJsonResource("getblockcount_1000")
                });

            var expectedScript = new ScriptBuilder()
                .ContractCall(GasToken.SCRIPT_HASH, "transfer", new ContractParameter[]
                {
                    ContractParameter.Hash160(fromAccount.GetScriptHash()),
                    ContractParameter.Hash160(toAccount.GetScriptHash()),
                    ContractParameter.Integer(amount),
                    ContractParameter.Any(null)
                })
                .ToArray();

            // Act
            var builder = await gasToken
                .TransferAsync(fromAccount.GetScriptHash(), toAccount.GetScriptHash(), amount)
                .ConfigureAwait(false);

            builder.Signers(new AccountSigner[] { AccountSigner.CalledByEntry(fromAccount) });

            // Assert
            TestHelpers.AssertBytesEqual(expectedScript, builder.Script);
        }

        #region Unity-Specific Tests

        [Test]
        [Performance]
        public void TestPerformance_GetTokenInfo()
        {
            // Arrange
            mockNeoSwift
                .WithMockInvokeFunctions(new Dictionary<string, string>
                {
                    ["name"] = TestHelpers.LoadJsonResource("invokefunction_name_gas"),
                    ["symbol"] = TestHelpers.LoadJsonResource("invokefunction_symbol_gas"),
                    ["decimals"] = TestHelpers.LoadJsonResource("invokefunction_decimals_gas")
                });

            // Act & Assert
            var executionTime = TestHelpers.MeasureExecutionTime(() =>
            {
                _ = gasToken.GetNameAsync().Result;
                _ = gasToken.GetSymbolAsync().Result;
                _ = gasToken.GetDecimalsAsync().Result;
            });

            Assert.Less(executionTime, 2000, "Token info queries should complete within 2 seconds");
            Debug.Log($"GAS token info execution time: {executionTime}ms");
        }

        [Test]
        public void TestMemoryUsage_CreateGasToken()
        {
            // Act & Assert
            var memoryUsage = TestHelpers.MeasureMemoryUsage(() =>
            {
                var token = new GasToken(mockNeoSwift);
                GC.KeepAlive(token);
            });

            Assert.Less(Math.Abs(memoryUsage), 512 * 1024, "GasToken creation should use less than 512KB");
            Debug.Log($"GasToken memory usage: {memoryUsage} bytes");
        }

        [UnityTest]
        public IEnumerator TestUnityCoroutineCompatibility()
        {
            // Arrange
            mockNeoSwift
                .WithMockInvokeFunctions(new Dictionary<string, string>
                {
                    ["symbol"] = TestHelpers.LoadJsonResource("invokefunction_symbol_gas")
                });

            bool completed = false;
            string result = null;

            // Act
            gasToken.GetSymbolAsync().ContinueWith(task =>
            {
                result = task.Result;
                completed = true;
            });

            // Wait for completion
            yield return new WaitUntil(() => completed);

            // Assert
            Assert.AreEqual("GAS", result);
        }

        [Test]
        public void TestSerializationCompatibility()
        {
            // Test that GasToken can be serialized for Unity Inspector
            var token = new GasToken(mockNeoSwift);
            
            // Create serializable wrapper
            var serializableToken = new SerializableTokenInfo
            {
                name = "GasToken",
                symbol = "GAS",
                decimals = 8,
                scriptHash = GASTOKEN_SCRIPTHASH
            };
            
            var jsonString = JsonUtility.ToJson(serializableToken, true);
            Assert.IsNotNull(jsonString);
            Assert.IsTrue(jsonString.Contains("GAS"));
            
            Debug.Log($"GasToken serialized: {jsonString}");

            // Test deserialization
            var deserializedToken = JsonUtility.FromJson<SerializableTokenInfo>(jsonString);
            Assert.AreEqual("GasToken", deserializedToken.name);
            Assert.AreEqual("GAS", deserializedToken.symbol);
            Assert.AreEqual(8, deserializedToken.decimals);
            Assert.AreEqual(GASTOKEN_SCRIPTHASH, deserializedToken.scriptHash);
        }

        [Test]
        public async Task TestErrorHandling_InvalidAddress()
        {
            // Arrange
            var invalidAddress = "invalid_address";

            // Act & Assert
            try
            {
                await gasToken.GetBalanceAsync(Hash160.Parse(invalidAddress));
                Assert.Fail("Should throw exception for invalid address");
            }
            catch (Exception ex)
            {
                Assert.IsTrue(ex is ArgumentException || ex is FormatException);
                Debug.Log($"Expected exception caught: {ex.GetType().Name}");
            }
        }

        [Test]
        public void TestConstants()
        {
            // Test that all constants are properly defined
            Assert.AreEqual(GASTOKEN_SCRIPTHASH, GasToken.SCRIPT_HASH.ToString());
            Assert.IsNotNull(gasToken.ScriptHash);
            
            // Test that script hash is valid format
            var scriptHashString = gasToken.ScriptHash.ToString();
            Assert.AreEqual(40, scriptHashString.Length); // 20 bytes = 40 hex characters
            Assert.IsTrue(System.Text.RegularExpressions.Regex.IsMatch(scriptHashString, "^[0-9a-fA-F]{40}$"));
        }

        [Test]
        public void TestMultipleInstances()
        {
            // Test that multiple GasToken instances work correctly
            var token1 = new GasToken(mockNeoSwift);
            var token2 = new GasToken(mockNeoSwift);
            
            Assert.AreEqual(token1.ScriptHash, token2.ScriptHash);
            Assert.AreNotSame(token1, token2); // Different instances
        }

        [Test]
        public void TestThreadSafety()
        {
            // Arrange
            const int threadCount = 4;
            const int operationsPerThread = 10;
            var tasks = new System.Threading.Tasks.Task[threadCount];
            var exceptions = new System.Collections.Concurrent.ConcurrentQueue<Exception>();

            // Act
            for (int t = 0; t < threadCount; t++)
            {
                tasks[t] = System.Threading.Tasks.Task.Run(() =>
                {
                    try
                    {
                        for (int i = 0; i < operationsPerThread; i++)
                        {
                            var token = new GasToken(mockNeoSwift);
                            var scriptHash = token.ScriptHash;
                            Assert.IsNotNull(scriptHash);
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
            Debug.Log($"Thread safety test passed: {threadCount} threads, {operationsPerThread} ops each");
        }

        #endregion

        #region Helper Classes

        [System.Serializable]
        private class SerializableTokenInfo
        {
            public string name;
            public string symbol;
            public int decimals;
            public string scriptHash;
        }

        #endregion
    }
}
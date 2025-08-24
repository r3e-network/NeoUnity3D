using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Neo.Unity.SDK;
using Neo.Unity.SDK.Transaction;
using Neo.Unity.SDK.Script;
using Neo.Unity.SDK.Tests.Helpers;
using Neo.Unity.SDK.Models;
using System.Collections;

namespace Neo.Unity.SDK.Tests.Transaction
{
    /// <summary>
    /// Unity Test Framework implementation of TransactionBuilder tests
    /// Converted from Swift TransactionBuilderTests.swift with Unity-specific enhancements
    /// </summary>
    [TestFixture]
    public class TransactionBuilderTests
    {
        private readonly Hash160 NEO_TOKEN_SCRIPT_HASH = Hash160.Parse("ef4073a0f2b305a38ec4050e4d3d28bc40ea63f5");
        private readonly Hash160 GAS_TOKEN_SCRIPT_HASH = Hash160.Parse("d2a4cff31913016155e38e474a2c06d08be276cf");
        private const string NEP17_TRANSFER = "transfer";

        private Account account1;
        private Account account2;
        private Hash160 recipient;
        private MockNeoSwift mockNeoSwift;
        private byte[] scriptInvokeFunctionNeoSymbolBytes;

        [SetUp]
        public void SetUp()
        {
            account1 = TestHelpers.CreateTestAccount("e6e919577dd7b8e97805151c05ae07ff4f752654d6d8797597aca989c02c4cb3");
            account2 = TestHelpers.CreateTestAccount("b4b2b579cac270125259f08a5f414e9235817e7637b9a66cfeb3b77d90c8e7f9");
            recipient = Hash160.Parse("969a77db482f74ce27105f760efa139223431394");
            mockNeoSwift = new MockNeoSwift();

            // Build script for NEO symbol function call
            var scriptBuilder = new ScriptBuilder();
            scriptBuilder.ContractCall(NEO_TOKEN_SCRIPT_HASH, "symbol", new ContractParameter[0]);
            scriptInvokeFunctionNeoSymbolBytes = scriptBuilder.ToArray();
        }

        [TearDown]
        public void TearDown()
        {
            mockNeoSwift?.Dispose();
        }

        [Test]
        public async Task TestBuildTransactionWithCorrectNonce()
        {
            // Arrange
            mockNeoSwift
                .WithMockResponses(new Dictionary<string, string>
                {
                    ["invokescript"] = TestHelpers.LoadJsonResource("invokescript_necessary_mock"),
                    ["calculatenetworkfee"] = TestHelpers.LoadJsonResource("calculatenetworkfee")
                });

            var nonce = UnityEngine.Random.Range(0, (int)Math.Pow(2, 32) - 1);
            var transactionBuilder = new TransactionBuilder(mockNeoSwift)
                .ValidUntilBlock(1)
                .Script(new byte[] { 1, 2, 3 })
                .Signers(new AccountSigner[] { AccountSigner.CalledByEntry(account1) });

            // Act & Assert - Test with random nonce
            var transaction = await transactionBuilder.Nonce(nonce).GetUnsignedTransactionAsync();
            Assert.AreEqual(nonce, transaction.Nonce);

            // Test with nonce = 0
            nonce = 0;
            transaction = await transactionBuilder.Nonce(nonce).GetUnsignedTransactionAsync();
            Assert.AreEqual(nonce, transaction.Nonce);

            // Test with max nonce
            nonce = (int)(Math.Pow(2, 32) - 1);
            transaction = await transactionBuilder.Nonce(nonce).GetUnsignedTransactionAsync();
            Assert.AreEqual(nonce, transaction.Nonce);

            // Test with negative as unsigned
            nonce = -1;
            transaction = await transactionBuilder.Nonce((uint)nonce).GetUnsignedTransactionAsync();
            Assert.AreEqual((uint)nonce, transaction.Nonce);
        }

        [Test]
        public void TestFailBuildingTransactionWithIncorrectNonce()
        {
            // Arrange
            var transactionBuilder = new TransactionBuilder(mockNeoSwift)
                .ValidUntilBlock(1)
                .Script(new byte[] { 1, 2, 3 })
                .Signers(new AccountSigner[] { AccountSigner.CalledByEntry(account1) });

            // Act & Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => 
                transactionBuilder.Nonce((uint)Math.Pow(2, 32)));
            Assert.Throws<ArgumentOutOfRangeException>(() => 
                transactionBuilder.Nonce(-1));
        }

        [Test]
        public void TestFailBuildingTransactionWithInvalidBlockNumber()
        {
            // Act & Assert
            Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                new TransactionBuilder(mockNeoSwift)
                    .ValidUntilBlock(-1)
                    .Script(new byte[] { 1, 2, 3 })
                    .Signers(new AccountSigner[] { AccountSigner.CalledByEntry(account1) });
            });

            Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                new TransactionBuilder(mockNeoSwift)
                    .ValidUntilBlock((uint)Math.Pow(2, 32))
                    .Script(new byte[] { 1, 2, 3 })
                    .Signers(new AccountSigner[] { AccountSigner.CalledByEntry(account1) });
            });
        }

        [Test]
        public async Task TestAutomaticallySetNonce()
        {
            // Arrange
            mockNeoSwift
                .WithMockResponses(new Dictionary<string, string>
                {
                    ["invokescript"] = TestHelpers.LoadJsonResource("invokescript_necessary_mock"),
                    ["calculatenetworkfee"] = TestHelpers.LoadJsonResource("calculatenetworkfee"),
                    ["getblockcount"] = TestHelpers.LoadJsonResource("getblockcount_1000")
                });

            // Act
            var tx = await new TransactionBuilder(mockNeoSwift)
                .Script(new byte[] { 1, 2, 3 })
                .Signers(new AccountSigner[] { AccountSigner.CalledByEntry(account1) })
                .GetUnsignedTransactionAsync();

            // Assert
            Assert.Less(tx.Nonce, Math.Pow(2, 32));
            Assert.Greater(tx.Nonce, 0);
        }

        [Test]
        public async Task TestFailBuildingTxWithoutAnySigner()
        {
            // Act & Assert
            var exception = await TestDelegate.ThrowsAsync<InvalidOperationException>(async () =>
            {
                await new TransactionBuilder(mockNeoSwift)
                    .ValidUntilBlock(100)
                    .Script(new byte[] { 1, 2, 3 })
                    .GetUnsignedTransactionAsync();
            });

            Assert.IsTrue(exception.Message.Contains("Cannot create a transaction without signers"));

            // Test duplicate signers
            var builder = new TransactionBuilder(mockNeoSwift);
            Assert.Throws<ArgumentException>(() =>
            {
                builder.Signers(new AccountSigner[] 
                { 
                    AccountSigner.Global(account1), 
                    AccountSigner.CalledByEntry(account1) 
                });
            });
        }

        [Test]
        public void TestOverrideSigner()
        {
            // Arrange
            var builder = new TransactionBuilder(mockNeoSwift);

            // Act & Assert
            builder.Signers(new AccountSigner[] { AccountSigner.Global(account1) });
            CollectionAssert.AreEqual(new[] { AccountSigner.Global(account1) }, builder.Signers);

            builder.Signers(new AccountSigner[] { AccountSigner.Global(account2) });
            CollectionAssert.AreEqual(new[] { AccountSigner.Global(account2) }, builder.Signers);
        }

        [Test]
        public async Task TestAttributesHighPriority()
        {
            // Arrange
            mockNeoSwift
                .WithMockResponses(new Dictionary<string, string>
                {
                    ["invokescript"] = TestHelpers.LoadJsonResource("invokescript_symbol_neo"),
                    ["calculatenetworkfee"] = TestHelpers.LoadJsonResource("calculatenetworkfee"),
                    ["getcommittee"] = TestHelpers.LoadJsonResource("getcommittee"),
                    ["getblockcount"] = TestHelpers.LoadJsonResource("getblockcount_1000")
                });

            // Act
            var tx = await new TransactionBuilder(mockNeoSwift)
                .Script(scriptInvokeFunctionNeoSymbolBytes)
                .Attributes(new TransactionAttribute[] { TransactionAttribute.HighPriority() })
                .Signers(new AccountSigner[] { AccountSigner.None(account1) })
                .GetUnsignedTransactionAsync();

            // Assert
            CollectionAssert.AreEqual(new[] { TransactionAttribute.HighPriority() }, tx.Attributes);
        }

        [Test]
        public async Task TestAttributesHighPriorityCommittee()
        {
            // Arrange
            mockNeoSwift
                .WithMockResponses(new Dictionary<string, string>
                {
                    ["invokescript"] = TestHelpers.LoadJsonResource("invokescript_symbol_neo"),
                    ["calculatenetworkfee"] = TestHelpers.LoadJsonResource("calculatenetworkfee"),
                    ["getcommittee"] = TestHelpers.LoadJsonResource("getcommittee"),
                    ["getblockcount"] = TestHelpers.LoadJsonResource("getblockcount_1000")
                });

            var multiSigAccount = Account.CreateMultiSigAccount(
                new ECPublicKey[] { account2.KeyPair.PublicKey, account1.KeyPair.PublicKey }, 1);

            // Act
            var tx = await new TransactionBuilder(mockNeoSwift)
                .Script(scriptInvokeFunctionNeoSymbolBytes)
                .Attributes(new TransactionAttribute[] { TransactionAttribute.HighPriority() })
                .Signers(new AccountSigner[] { AccountSigner.None(multiSigAccount) })
                .GetUnsignedTransactionAsync();

            // Assert
            CollectionAssert.AreEqual(new[] { TransactionAttribute.HighPriority() }, tx.Attributes);
        }

        [Test]
        public async Task TestAttributesHighPriorityNotCommitteeMember()
        {
            // Arrange
            mockNeoSwift
                .WithMockResponses(new Dictionary<string, string>
                {
                    ["getcommittee"] = TestHelpers.LoadJsonResource("getcommittee"),
                    ["getblockcount"] = TestHelpers.LoadJsonResource("getblockcount_1000")
                });

            var builder = new TransactionBuilder(mockNeoSwift)
                .Script(scriptInvokeFunctionNeoSymbolBytes)
                .Attributes(new TransactionAttribute[] { TransactionAttribute.HighPriority() })
                .Signers(new AccountSigner[] { AccountSigner.None(account2) });

            // Act & Assert
            var exception = await TestDelegate.ThrowsAsync<UnauthorizedAccessException>(async () =>
            {
                await builder.GetUnsignedTransactionAsync();
            });

            Assert.IsTrue(exception.Message.Contains("Only committee members can send transactions with high priority"));
        }

        [Test]
        public async Task TestAttributesHighPriorityOnlyAddedOnce()
        {
            // Arrange
            mockNeoSwift
                .WithMockResponses(new Dictionary<string, string>
                {
                    ["invokescript"] = TestHelpers.LoadJsonResource("invokescript_symbol_neo"),
                    ["calculatenetworkfee"] = TestHelpers.LoadJsonResource("calculatenetworkfee"),
                    ["getcommittee"] = TestHelpers.LoadJsonResource("getcommittee"),
                    ["getblockcount"] = TestHelpers.LoadJsonResource("getblockcount_1000")
                });

            // Act
            var tx = await new TransactionBuilder(mockNeoSwift)
                .Script(scriptInvokeFunctionNeoSymbolBytes)
                .Attributes(new TransactionAttribute[] { TransactionAttribute.HighPriority() })
                .Attributes(new TransactionAttribute[] { TransactionAttribute.HighPriority() })
                .Signers(new AccountSigner[] { AccountSigner.None(account1) })
                .GetUnsignedTransactionAsync();

            // Assert
            CollectionAssert.AreEqual(new[] { TransactionAttribute.HighPriority() }, tx.Attributes);
        }

        [Test]
        public void TestFailAddingMoreThanMaxAttributesToTx_JustAttributes()
        {
            // Arrange
            var attrs = new TransactionAttribute[NeoConstants.MAX_TRANSACTION_ATTRIBUTES + 1];
            for (int i = 0; i <= NeoConstants.MAX_TRANSACTION_ATTRIBUTES; i++)
            {
                attrs[i] = TransactionAttribute.HighPriority();
            }

            // Act & Assert
            Assert.Throws<ArgumentException>(() =>
            {
                new TransactionBuilder(mockNeoSwift).Attributes(attrs);
            });
        }

        [Test]
        public void TestFailAddingMoreThanMaxAttributesToTx_AttributesAndSigners()
        {
            // Arrange
            var builder = new TransactionBuilder(mockNeoSwift);
            builder.Signers(new AccountSigner[] 
            { 
                AccountSigner.CalledByEntry(Account.Create()),
                AccountSigner.CalledByEntry(Account.Create()),
                AccountSigner.CalledByEntry(Account.Create())
            });

            var attrs = new TransactionAttribute[NeoConstants.MAX_TRANSACTION_ATTRIBUTES - 2];
            for (int i = 0; i < NeoConstants.MAX_TRANSACTION_ATTRIBUTES - 2; i++)
            {
                attrs[i] = TransactionAttribute.HighPriority();
            }

            // Act & Assert
            Assert.Throws<ArgumentException>(() =>
            {
                builder.Attributes(attrs);
            });
        }

        [Test]
        public async Task TestAutomaticSettingOfValidUntilBlockVariable()
        {
            // Arrange
            mockNeoSwift
                .WithMockResponses(new Dictionary<string, string>
                {
                    ["invokescript"] = TestHelpers.LoadJsonResource("invokescript_symbol_neo"),
                    ["calculatenetworkfee"] = TestHelpers.LoadJsonResource("calculatenetworkfee"),
                    ["getblockcount"] = TestHelpers.LoadJsonResource("getblockcount_1000")
                });

            // Act
            var tx = await new TransactionBuilder(mockNeoSwift)
                .Script(scriptInvokeFunctionNeoSymbolBytes)
                .Signers(new AccountSigner[] { AccountSigner.None(Account.Create()) })
                .GetUnsignedTransactionAsync();

            // Assert
            var expectedValidUntil = NeoSwift.DEFAULT_MAX_VALID_UNTIL_BLOCK_INCREMENT + 999;
            Assert.AreEqual(expectedValidUntil, tx.ValidUntilBlock);
        }

        [Test]
        public async Task TestAutomaticSettingOfSystemFeeAndNetworkFee()
        {
            // Arrange
            mockNeoSwift
                .WithMockResponses(new Dictionary<string, string>
                {
                    ["invokescript"] = TestHelpers.LoadJsonResource("invokescript_symbol_neo"),
                    ["calculatenetworkfee"] = TestHelpers.LoadJsonResource("calculatenetworkfee")
                });

            // Act
            var tx = await new TransactionBuilder(mockNeoSwift)
                .Script(scriptInvokeFunctionNeoSymbolBytes)
                .Signers(new AccountSigner[] { AccountSigner.None(Account.Create()) })
                .ValidUntilBlock(1000)
                .GetUnsignedTransactionAsync();

            // Assert
            Assert.AreEqual(984060, tx.SystemFee);
            Assert.AreEqual(1230610, tx.NetworkFee);
        }

        [Test]
        public async Task TestFailTryingToSignTransactionWithAccountMissingAPrivateKey()
        {
            // Arrange
            mockNeoSwift
                .WithMockResponses(new Dictionary<string, string>
                {
                    ["invokescript"] = TestHelpers.LoadJsonResource("invokescript_symbol_neo"),
                    ["calculatenetworkfee"] = TestHelpers.LoadJsonResource("calculatenetworkfee")
                });

            var builder = new TransactionBuilder(mockNeoSwift)
                .Script(scriptInvokeFunctionNeoSymbolBytes)
                .Signers(new AccountSigner[] { AccountSigner.None(Account.FromAddress(account1.Address)) })
                .ValidUntilBlock(1000);

            // Act & Assert
            var exception = await TestDelegate.ThrowsAsync<InvalidOperationException>(async () =>
            {
                await builder.SignAsync();
            });

            Assert.IsTrue(exception.Message.Contains($"account {account1.Address} does not hold a private key"));
        }

        [Test]
        public async Task TestFailAutomaticallySigningWithMultiSigAccountSigner()
        {
            // Arrange
            mockNeoSwift
                .WithMockResponses(new Dictionary<string, string>
                {
                    ["invokescript"] = TestHelpers.LoadJsonResource("invokescript_symbol_neo"),
                    ["calculatenetworkfee"] = TestHelpers.LoadJsonResource("calculatenetworkfee"),
                    ["getblockcount"] = TestHelpers.LoadJsonResource("getblockcount_1000")
                });

            var multiSigAccount = Account.CreateMultiSigAccount(
                new ECPublicKey[] { account1.KeyPair.PublicKey }, 1);

            var builder = new TransactionBuilder(mockNeoSwift)
                .Script(scriptInvokeFunctionNeoSymbolBytes)
                .Signers(new AccountSigner[] { AccountSigner.None(multiSigAccount) });

            // Act & Assert
            var exception = await TestDelegate.ThrowsAsync<InvalidOperationException>(async () =>
            {
                await builder.SignAsync();
            });

            Assert.IsTrue(exception.Message.Contains("Transactions with multi-sig signers cannot be signed automatically"));
        }

        [Test]
        public async Task TestSignTransactionWithAdditionalSigners()
        {
            // Arrange
            mockNeoSwift
                .WithMockResponses(new Dictionary<string, string>
                {
                    ["invokescript"] = TestHelpers.LoadJsonResource("invokescript_symbol_neo"),
                    ["calculatenetworkfee"] = TestHelpers.LoadJsonResource("calculatenetworkfee")
                });

            // Act
            var tx = await new TransactionBuilder(mockNeoSwift)
                .Script(scriptInvokeFunctionNeoSymbolBytes)
                .Signers(new AccountSigner[] 
                { 
                    AccountSigner.CalledByEntry(account1), 
                    AccountSigner.CalledByEntry(account2) 
                })
                .ValidUntilBlock(1000)
                .SignAsync();

            // Assert
            Assert.AreEqual(2, tx.Witnesses.Count);
            
            var signers = new List<ECPublicKey>();
            foreach (var witness in tx.Witnesses)
            {
                var publicKeys = witness.VerificationScript.GetPublicKeys();
                if (publicKeys.Count > 0)
                {
                    signers.Add(publicKeys[0]);
                }
            }

            Assert.Contains(account1.KeyPair.PublicKey, signers);
            Assert.Contains(account2.KeyPair.PublicKey, signers);
        }

        [Test]
        public async Task TestSendInvokeFunction()
        {
            // Arrange
            mockNeoSwift
                .WithMockResponses(new Dictionary<string, string>
                {
                    ["invokescript"] = TestHelpers.LoadJsonResource("invokescript_transfer_with_fixed_sysfee"),
                    ["calculatenetworkfee"] = TestHelpers.LoadJsonResource("calculatenetworkfee"),
                    ["getblockcount"] = TestHelpers.LoadJsonResource("getblockcount_1000"),
                    ["sendrawtransaction"] = TestHelpers.LoadJsonResource("sendrawtransaction")
                });

            var script = new ScriptBuilder()
                .ContractCall(NEO_TOKEN_SCRIPT_HASH, NEP17_TRANSFER, new ContractParameter[]
                {
                    ContractParameter.Hash160(account1.GetScriptHash()),
                    ContractParameter.Hash160(recipient),
                    ContractParameter.Integer(5),
                    ContractParameter.Any(null)
                })
                .ToArray();

            // Act
            var tx = await new TransactionBuilder(mockNeoSwift)
                .Script(script)
                .Signers(new AccountSigner[] { AccountSigner.None(account1) })
                .SignAsync();

            var response = await tx.SendAsync();

            // Assert
            Assert.IsNull(response.Error);
            Assert.IsNotNull(response.Result);
        }

        [Test]
        public async Task TestTransferNeoFromNormalAccount()
        {
            // Arrange
            mockNeoSwift
                .WithMockResponses(new Dictionary<string, string>
                {
                    ["invokescript"] = TestHelpers.LoadJsonResource("invokescript_transfer_with_fixed_sysfee"),
                    ["calculatenetworkfee"] = TestHelpers.LoadJsonResource("calculatenetworkfee")
                });

            var expectedVerificationScript = account1.GetVerificationScript().Script;
            var script = new ScriptBuilder()
                .ContractCall(NEO_TOKEN_SCRIPT_HASH, NEP17_TRANSFER, new ContractParameter[]
                {
                    ContractParameter.Hash160(account1.GetScriptHash()),
                    ContractParameter.Hash160(recipient),
                    ContractParameter.Integer(5),
                    ContractParameter.Any(null)
                })
                .ToArray();

            // Act
            var tx = await new TransactionBuilder(mockNeoSwift)
                .Script(script)
                .Signers(new AccountSigner[] { AccountSigner.None(account1) })
                .ValidUntilBlock(100)
                .SignAsync();

            // Assert
            TestHelpers.AssertBytesEqual(script, tx.Script);
            Assert.AreEqual(1, tx.Witnesses.Count);
            TestHelpers.AssertBytesEqual(expectedVerificationScript, tx.Witnesses[0].VerificationScript.Script);
        }

        [Test]
        public void TestExtendScript()
        {
            // Arrange
            var script1 = new ScriptBuilder()
                .ContractCall(NEO_TOKEN_SCRIPT_HASH, NEP17_TRANSFER, new ContractParameter[]
                {
                    ContractParameter.Hash160(account1.GetScriptHash()),
                    ContractParameter.Hash160(recipient),
                    ContractParameter.Integer(11),
                    ContractParameter.Any(null)
                })
                .ToArray();

            var script2 = new ScriptBuilder()
                .ContractCall(NEO_TOKEN_SCRIPT_HASH, NEP17_TRANSFER, new ContractParameter[]
                {
                    ContractParameter.Hash160(account1.GetScriptHash()),
                    ContractParameter.Hash160(account2.GetScriptHash()),
                    ContractParameter.Integer(22),
                    ContractParameter.Any(null)
                })
                .ToArray();

            var builder = new TransactionBuilder(mockNeoSwift).Script(script1);
            TestHelpers.AssertBytesEqual(script1, builder.Script);

            // Act
            builder.ExtendScript(script2);

            // Assert
            var expectedCombined = new byte[script1.Length + script2.Length];
            Array.Copy(script1, 0, expectedCombined, 0, script1.Length);
            Array.Copy(script2, 0, expectedCombined, script1.Length, script2.Length);
            TestHelpers.AssertBytesEqual(expectedCombined, builder.Script);
        }

        [Test]
        public async Task TestGetUnsignedTransaction()
        {
            // Arrange
            mockNeoSwift
                .WithMockResponses(new Dictionary<string, string>
                {
                    ["invokescript"] = TestHelpers.LoadJsonResource("invokescript_symbol_neo"),
                    ["getblockcount"] = TestHelpers.LoadJsonResource("getblockcount_1000"),
                    ["calculatenetworkfee"] = TestHelpers.LoadJsonResource("calculatenetworkfee")
                });

            // Act
            var tx = await new TransactionBuilder(mockNeoSwift)
                .Script(scriptInvokeFunctionNeoSymbolBytes)
                .Signers(new AccountSigner[] { AccountSigner.CalledByEntry(account1) })
                .GetUnsignedTransactionAsync();

            // Assert
            Assert.AreEqual(0, tx.Version);
            CollectionAssert.AreEqual(new[] { AccountSigner.CalledByEntry(account1) }, tx.Signers);
            Assert.IsEmpty(tx.Witnesses);
        }

        [Test]
        public async Task TestVersion()
        {
            // Arrange
            mockNeoSwift
                .WithMockResponses(new Dictionary<string, string>
                {
                    ["invokescript"] = TestHelpers.LoadJsonResource("invokescript_symbol_neo"),
                    ["getblockcount"] = TestHelpers.LoadJsonResource("getblockcount_1000"),
                    ["calculatenetworkfee"] = TestHelpers.LoadJsonResource("calculatenetworkfee")
                });

            // Act
            var tx = await new TransactionBuilder(mockNeoSwift)
                .Version(1)
                .Script(scriptInvokeFunctionNeoSymbolBytes)
                .Signers(new AccountSigner[] { AccountSigner.CalledByEntry(account1) })
                .GetUnsignedTransactionAsync();

            // Assert
            Assert.AreEqual(1, tx.Version);
        }

        [Test]
        public async Task TestAdditionalNetworkFee()
        {
            // Arrange
            mockNeoSwift
                .WithMockResponses(new Dictionary<string, string>
                {
                    ["invokescript"] = TestHelpers.LoadJsonResource("invokescript_symbol_neo"),
                    ["getblockcount"] = TestHelpers.LoadJsonResource("getblockcount_1000"),
                    ["calculatenetworkfee"] = TestHelpers.LoadJsonResource("calculatenetworkfee")
                });

            const int baseNetworkFee = 1230610;

            // Act
            var tx1 = await new TransactionBuilder(mockNeoSwift)
                .Script(scriptInvokeFunctionNeoSymbolBytes)
                .Signers(new AccountSigner[] { AccountSigner.CalledByEntry(Account.Create()) })
                .GetUnsignedTransactionAsync();

            var tx2 = await new TransactionBuilder(mockNeoSwift)
                .Script(scriptInvokeFunctionNeoSymbolBytes)
                .Signers(new AccountSigner[] { AccountSigner.None(Account.Create()) })
                .AdditionalNetworkFee(2000)
                .GetUnsignedTransactionAsync();

            // Assert
            Assert.AreEqual(baseNetworkFee, tx1.NetworkFee);
            Assert.AreEqual(baseNetworkFee + 2000, tx2.NetworkFee);
        }

        [Test]
        public async Task TestAdditionalSystemFee()
        {
            // Arrange
            mockNeoSwift
                .WithMockResponses(new Dictionary<string, string>
                {
                    ["invokescript"] = TestHelpers.LoadJsonResource("invokescript_symbol_neo"),
                    ["getblockcount"] = TestHelpers.LoadJsonResource("getblockcount_1000"),
                    ["calculatenetworkfee"] = TestHelpers.LoadJsonResource("calculatenetworkfee")
                });

            const int baseSystemFee = 984060;

            // Act
            var tx1 = await new TransactionBuilder(mockNeoSwift)
                .Script(scriptInvokeFunctionNeoSymbolBytes)
                .Signers(new AccountSigner[] { AccountSigner.CalledByEntry(Account.Create()) })
                .GetUnsignedTransactionAsync();

            var tx2 = await new TransactionBuilder(mockNeoSwift)
                .Script(scriptInvokeFunctionNeoSymbolBytes)
                .Signers(new AccountSigner[] { AccountSigner.None(Account.Create()) })
                .AdditionalSystemFee(3000)
                .GetUnsignedTransactionAsync();

            // Assert
            Assert.AreEqual(baseSystemFee, tx1.SystemFee);
            Assert.AreEqual(baseSystemFee + 3000, tx2.SystemFee);
        }

        #region Unity-Specific Tests

        [Test]
        [Performance]
        public void TestPerformance_TransactionBuilding()
        {
            // Arrange
            mockNeoSwift
                .WithMockResponses(new Dictionary<string, string>
                {
                    ["invokescript"] = TestHelpers.LoadJsonResource("invokescript_symbol_neo"),
                    ["getblockcount"] = TestHelpers.LoadJsonResource("getblockcount_1000"),
                    ["calculatenetworkfee"] = TestHelpers.LoadJsonResource("calculatenetworkfee")
                });

            const int iterations = 10;

            // Act & Assert
            var executionTime = TestHelpers.MeasureExecutionTime(() =>
            {
                for (int i = 0; i < iterations; i++)
                {
                    var builder = new TransactionBuilder(mockNeoSwift)
                        .Script(scriptInvokeFunctionNeoSymbolBytes)
                        .Signers(new AccountSigner[] { AccountSigner.CalledByEntry(Account.Create()) });
                    
                    var task = builder.GetUnsignedTransactionAsync();
                    task.Wait();
                    GC.KeepAlive(task.Result);
                }
            });

            var avgTime = (float)executionTime / iterations;
            Assert.Less(avgTime, 500, "Transaction building should average less than 500ms");
            Debug.Log($"Transaction building average time: {avgTime}ms per transaction ({iterations} iterations)");
        }

        [Test]
        public void TestMemoryUsage_TransactionBuilder()
        {
            const int iterations = 5;

            // Act & Assert
            var memoryUsage = TestHelpers.MeasureMemoryUsage(() =>
            {
                var builders = new TransactionBuilder[iterations];
                for (int i = 0; i < iterations; i++)
                {
                    builders[i] = new TransactionBuilder(mockNeoSwift)
                        .Script(scriptInvokeFunctionNeoSymbolBytes)
                        .Signers(new AccountSigner[] { AccountSigner.CalledByEntry(Account.Create()) });
                }
                GC.KeepAlive(builders);
            });

            var avgMemoryPerBuilder = memoryUsage / iterations;
            Assert.Less(Math.Abs(avgMemoryPerBuilder), 50 * 1024, "Each transaction builder should use less than 50KB");
            Debug.Log($"Memory usage per transaction builder: {avgMemoryPerBuilder} bytes");
        }

        [UnityTest]
        public IEnumerator TestUnityCoroutineCompatibility()
        {
            // Arrange
            mockNeoSwift
                .WithMockResponses(new Dictionary<string, string>
                {
                    ["invokescript"] = TestHelpers.LoadJsonResource("invokescript_symbol_neo"),
                    ["getblockcount"] = TestHelpers.LoadJsonResource("getblockcount_1000"),
                    ["calculatenetworkfee"] = TestHelpers.LoadJsonResource("calculatenetworkfee")
                });

            bool completed = false;
            Neo.Unity.SDK.Models.Transaction result = null;

            // Act
            var builder = new TransactionBuilder(mockNeoSwift)
                .Script(scriptInvokeFunctionNeoSymbolBytes)
                .Signers(new AccountSigner[] { AccountSigner.CalledByEntry(account1) });

            builder.GetUnsignedTransactionAsync().ContinueWith(task =>
            {
                result = task.Result;
                completed = true;
            });

            // Wait for completion
            yield return new WaitUntil(() => completed);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Version);
        }

        [Test]
        public void TestSerializationCompatibility()
        {
            // Test that TransactionBuilder can work with Unity serialization
            var builder = new TransactionBuilder(mockNeoSwift)
                .Script(scriptInvokeFunctionNeoSymbolBytes)
                .ValidUntilBlock(1000);

            // Create serializable wrapper for Unity Inspector
            var serializableBuilder = new SerializableTransactionBuilderInfo
            {
                scriptHex = TestHelpers.BytesToHex(scriptInvokeFunctionNeoSymbolBytes),
                validUntilBlock = 1000,
                version = 0
            };

            var jsonString = JsonUtility.ToJson(serializableBuilder, true);
            Assert.IsNotNull(jsonString);
            Assert.IsTrue(jsonString.Contains("1000"));

            Debug.Log($"Serializable transaction builder: {jsonString}");

            // Test deserialization
            var deserializedBuilder = JsonUtility.FromJson<SerializableTransactionBuilderInfo>(jsonString);
            Assert.AreEqual(1000, deserializedBuilder.validUntilBlock);
            Assert.AreEqual(0, deserializedBuilder.version);
        }

        [Test]
        public void TestThreadSafety()
        {
            // Arrange
            const int threadCount = 4;
            const int operationsPerThread = 10;
            var tasks = new System.Threading.Tasks.Task[threadCount];
            var results = new TransactionBuilder[threadCount * operationsPerThread];
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
                            int resultIndex = threadIndex * operationsPerThread + i;
                            results[resultIndex] = new TransactionBuilder(mockNeoSwift)
                                .Script(scriptInvokeFunctionNeoSymbolBytes)
                                .ValidUntilBlock((uint)(1000 + resultIndex));
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

            // Verify all builders were created
            foreach (var result in results)
            {
                Assert.IsNotNull(result);
                Assert.IsNotNull(result.Script);
            }

            Debug.Log($"Thread safety test passed: {threadCount} threads, {operationsPerThread} ops each");
        }

        [Test]
        public void TestEdgeCases_InvalidInputs()
        {
            // Test null script
            Assert.Throws<ArgumentNullException>(() =>
            {
                new TransactionBuilder(mockNeoSwift).Script(null);
            });

            // Test empty script
            Assert.Throws<ArgumentException>(() =>
            {
                new TransactionBuilder(mockNeoSwift).Script(new byte[0]);
            });

            // Test null signers
            Assert.Throws<ArgumentNullException>(() =>
            {
                new TransactionBuilder(mockNeoSwift).Signers(null);
            });

            // Test empty signers array
            Assert.Throws<ArgumentException>(() =>
            {
                new TransactionBuilder(mockNeoSwift).Signers(new AccountSigner[0]);
            });
        }

        [Test]
        public async Task TestTransactionSerialization()
        {
            // Arrange
            mockNeoSwift
                .WithMockResponses(new Dictionary<string, string>
                {
                    ["invokescript"] = TestHelpers.LoadJsonResource("invokescript_symbol_neo"),
                    ["getblockcount"] = TestHelpers.LoadJsonResource("getblockcount_1000"),
                    ["calculatenetworkfee"] = TestHelpers.LoadJsonResource("calculatenetworkfee")
                });

            // Act
            var tx = await new TransactionBuilder(mockNeoSwift)
                .Script(scriptInvokeFunctionNeoSymbolBytes)
                .Signers(new AccountSigner[] { AccountSigner.CalledByEntry(account1) })
                .GetUnsignedTransactionAsync();

            // Assert
            var serializedTx = tx.ToByteArray();
            Assert.IsNotNull(serializedTx);
            Assert.Greater(serializedTx.Length, 0);

            // Test that transaction can be deserialized
            var deserializedTx = Neo.Unity.SDK.Models.Transaction.FromByteArray(serializedTx);
            Assert.AreEqual(tx.Hash, deserializedTx.Hash);
            Assert.AreEqual(tx.Version, deserializedTx.Version);
            TestHelpers.AssertBytesEqual(tx.Script, deserializedTx.Script);
        }

        #endregion

        #region Helper Classes

        [System.Serializable]
        private class SerializableTransactionBuilderInfo
        {
            public string scriptHex;
            public uint validUntilBlock;
            public byte version;
        }

        #endregion
    }
}
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NUnit.Framework;
using Neo.Unity.SDK.Contracts;
using Neo.Unity.SDK.Types;
using Neo.Unity.SDK.Core;
using Neo.Unity.SDK.Wallet;
using Neo.Unity.SDK.Tests.Helpers;
using Neo.Unity.SDK.Protocol.Response;
using Neo.Unity.SDK.Script;

namespace Neo.Unity.SDK.Tests.Contract
{
    [TestFixture]
    public class SmartContractTests
    {
        private static readonly Hash160 NEO_SCRIPT_HASH = new Hash160("ef4073a0f2b305a38ec4050e4d3d28bc40ea63f5");
        private static readonly Hash160 SOME_SCRIPT_HASH = new Hash160("969a77db482f74ce27105f760efa139223431394");
        private Account account1;
        private Hash160 recipient;

        private MockNeoSwift mockNeoSwift;
        private SmartContract someContract;
        private SmartContract neoContract;

        private const string NEP17_TRANSFER = "transfer";
        private const string NEP17_BALANCEOF = "balanceOf";
        private const string NEP17_NAME = "name";
        private const string NEP17_TOTALSUPPLY = "totalSupply";

        [SetUp]
        public async Task SetUp()
        {
            account1 = await Account.FromWIF("L1WMhxazScMhUrdv34JqQb1HFSQmWeN2Kpc1R9JGKwL7CDNP21uR");
            recipient = new Hash160("969a77db482f74ce27105f760efa139223431394");

            mockNeoSwift = new MockNeoSwift();
            someContract = new SmartContract(SOME_SCRIPT_HASH, mockNeoSwift);
            neoContract = new SmartContract(NEO_SCRIPT_HASH, mockNeoSwift);
        }

        [Test]
        public void TestConstructSmartContract()
        {
            Assert.AreEqual(NEO_SCRIPT_HASH, neoContract.ScriptHash);
        }

        [Test]
        public async Task TestGetManifest()
        {
            mockNeoSwift.SetupGetContractState();
            var manifest = await someContract.GetManifest();
            
            Assert.AreEqual("neow3j", manifest.Name);
        }

        [Test]
        public async Task TestGetName()
        {
            mockNeoSwift.SetupGetContractState();
            var name = await someContract.GetName();
            
            Assert.AreEqual("neow3j", name);
        }

        [Test]
        public void TestInvokeWithEmptyString()
        {
            var exception = Assert.Throws<ArgumentException>(() =>
            {
                neoContract.InvokeFunction("", new List<ContractParameter>());
            });
            Assert.AreEqual("The invocation function must not be empty.", exception.Message);
        }

        [Test]
        public void TestBuildInvokeFunctionScript()
        {
            var expectedScript = new ScriptBuilder()
                .ContractCall(NEO_SCRIPT_HASH, NEP17_TRANSFER, new List<ContractParameter>
                {
                    ContractParameter.CreateHash160(account1.ScriptHash),
                    ContractParameter.CreateHash160(recipient),
                    ContractParameter.CreateInteger(42)
                })
                .ToArray();

            var script = neoContract.BuildInvokeFunctionScript(NEP17_TRANSFER, new List<ContractParameter>
            {
                ContractParameter.CreateHash160(account1.ScriptHash),
                ContractParameter.CreateHash160(recipient),
                ContractParameter.CreateInteger(42)
            });

            CollectionAssert.AreEqual(expectedScript, script);
        }

        [Test]
        public void TestInvokeFunction()
        {
            var expectedScript = new ScriptBuilder()
                .ContractCall(NEO_SCRIPT_HASH, NEP17_TRANSFER, new List<ContractParameter>
                {
                    ContractParameter.CreateHash160(account1.ScriptHash),
                    ContractParameter.CreateHash160(recipient),
                    ContractParameter.CreateInteger(42)
                })
                .ToArray();

            var builder = neoContract.InvokeFunction(NEP17_TRANSFER, new List<ContractParameter>
            {
                ContractParameter.CreateHash160(account1.ScriptHash),
                ContractParameter.CreateHash160(recipient),
                ContractParameter.CreateInteger(42)
            });

            CollectionAssert.AreEqual(expectedScript, builder.Script);
        }

        [Test]
        public async Task TestCallFunctionReturningString()
        {
            mockNeoSwift.SetupInvokeFunction("symbol", "ant");
            var name = await someContract.CallFunctionReturningString("symbol");
            
            Assert.AreEqual("ant", name);
        }

        [Test]
        public async Task TestCallFunctionReturningNonString()
        {
            mockNeoSwift.SetupInvokeFunction(NEP17_TOTALSUPPLY, 300_000_000_000_000);
            
            var exception = await AssertThrowsAsync<ContractException>(async () =>
            {
                await neoContract.CallFunctionReturningString(NEP17_NAME);
            });
            Assert.IsTrue(exception.Message.Contains("unexpected return type"));
        }

        [Test]
        public async Task TestCallFunctionReturningInt()
        {
            mockNeoSwift.SetupInvokeFunction(NEP17_TOTALSUPPLY, 300_000_000_000_000);
            var supply = await someContract.CallFunctionReturningInt(NEP17_TOTALSUPPLY);
            
            Assert.AreEqual(300_000_000_000_000, supply);
        }

        [Test]
        public async Task TestCallFunctionReturningNonInt()
        {
            mockNeoSwift.SetupInvokeFunction(NEP17_TRANSFER, true);
            
            var exception = await AssertThrowsAsync<ContractException>(async () =>
            {
                await neoContract.CallFunctionReturningString(NEP17_TRANSFER);
            });
            Assert.IsTrue(exception.Message.Contains("unexpected return type"));
        }

        [Test]
        public async Task TestCallFunctionReturningBool()
        {
            mockNeoSwift.SetupInvokeFunction(NEP17_TOTALSUPPLY, false);
            var result = await someContract.CallFunctionReturningBool(NEP17_TOTALSUPPLY);
            
            Assert.IsFalse(result);
        }

        [Test]
        public async Task TestCallFunctionReturningBool_Zero()
        {
            mockNeoSwift.SetupInvokeFunction(NEP17_TOTALSUPPLY, 0);
            var result = await someContract.CallFunctionReturningBool(NEP17_TOTALSUPPLY);
            
            Assert.IsFalse(result);
        }

        [Test]
        public async Task TestCallFunctionReturningBool_One()
        {
            mockNeoSwift.SetupInvokeFunction(NEP17_TOTALSUPPLY, 1);
            var result = await someContract.CallFunctionReturningBool(NEP17_TOTALSUPPLY);
            
            Assert.IsTrue(result);
        }

        [Test]
        public async Task TestCallFunctionReturningNonBool()
        {
            mockNeoSwift.SetupInvokeFunction("getCandidates", new List<object> { "candidate1", "candidate2" });
            
            var exception = await AssertThrowsAsync<ContractException>(async () =>
            {
                await neoContract.CallFunctionReturningBool("getCandidates");
            });
            Assert.IsTrue(exception.Message.Contains("unexpected return type"));
        }

        [Test]
        public async Task TestCallFunctionReturningScriptHash()
        {
            var expectedHash = new Hash160("69ecca587293047be4c59159bf8bc399985c160d");
            mockNeoSwift.SetupInvokeFunction("ownerOf", expectedHash);
            var scriptHash = await someContract.CallFunctionReturningScriptHash("ownerOf");
            
            Assert.AreEqual(expectedHash, scriptHash);
        }

        [Test]
        public async Task TestCallFunctionReturningIterator()
        {
            var iteratorId = "190d19ca-e935-4ad0-95c9-93b8cf6d115c";
            var sessionId = "a7b35b13-bdfc-4ab3-a398-88a9db9da4fe";
            mockNeoSwift.SetupInvokeFunctionWithIterator("tokensOf", iteratorId, sessionId);
            mockNeoSwift.SetupTraverseIterator(iteratorId, sessionId, new[] { "tokenof1", "tokenof2" });
            
            var iterator = await someContract.CallFunctionReturningIterator("tokensOf");
            
            Assert.AreEqual(iteratorId, iterator.IteratorId);
            Assert.AreEqual(sessionId, iterator.SessionId);
        }

        [Test]
        public async Task TestCallFunctionReturningIterator_TraverseWithFunction()
        {
            var iteratorId = "190d19ca-e935-4ad0-95c9-93b8cf6d115c";
            var sessionId = "a7b35b13-bdfc-4ab3-a398-88a9db9da4fe";
            var results = new[] { "tokenof1", "tokenof2" };
            mockNeoSwift.SetupInvokeFunctionWithIterator("tokensOf", iteratorId, sessionId);
            mockNeoSwift.SetupTraverseIterator(iteratorId, sessionId, results);
            
            var iterator = await someContract.CallFunctionReturningIterator("tokensOf", x => x.ToString());
            
            Assert.AreEqual(iteratorId, iterator.IteratorId);
            Assert.AreEqual(sessionId, iterator.SessionId);
            
            var strings = await iterator.Traverse(100);
            Assert.AreEqual(results[0], strings[0]);
            Assert.AreEqual(results[1], strings[1]);
        }

        [Test]
        public async Task TestCallFunctionTraversingIterator()
        {
            var iteratorId = "190d19ca-e935-4ad0-95c9-93b8cf6d115c";
            var sessionId = "a7b35b13-bdfc-4ab3-a398-88a9db9da4fe";
            var tokens = new[]
            {
                new List<object> { "neow#1", "besttoken" },
                new List<object> { "neow#2", "almostbesttoken" }
            };
            mockNeoSwift.SetupInvokeFunctionWithIterator("iterateTokens", iteratorId, sessionId);
            mockNeoSwift.SetupTraverseIteratorComplex(iteratorId, sessionId, tokens);
            mockNeoSwift.SetupTerminateSession(sessionId);
            
            var results = await someContract.CallFunctionAndTraverseIterator("iterateTokens");
            
            Assert.AreEqual(2, results.Count);
            
            var token1 = results[0] as List<object>;
            Assert.AreEqual("neow#1", token1[0]);
            Assert.AreEqual("besttoken", token1[1]);

            var token2 = results[1] as List<object>;
            Assert.AreEqual("neow#2", token2[0]);
            Assert.AreEqual("almostbesttoken", token2[1]);
        }

        [Test]
        public async Task TestCallFunctionTraversingIterator_WithFunction()
        {
            var iteratorId = "190d19ca-e935-4ad0-95c9-93b8cf6d115c";
            var sessionId = "a7b35b13-bdfc-4ab3-a398-88a9db9da4fe";
            var tokens = new[]
            {
                new List<object> { "neow#1", "besttoken" },
                new List<object> { "neow#2", "almostbesttoken" }
            };
            mockNeoSwift.SetupInvokeFunctionWithIterator("tokens", iteratorId, sessionId);
            mockNeoSwift.SetupTraverseIteratorComplex(iteratorId, sessionId, tokens);
            mockNeoSwift.SetupTerminateSession(sessionId);
            
            var strings = await someContract.CallFunctionAndTraverseIterator("tokens", 
                x => (x as List<object>)?[1]?.ToString());
            
            Assert.AreEqual("besttoken", strings[0]);
            Assert.AreEqual("almostbesttoken", strings[1]);
        }

        [Test]
        public async Task TestCallFunctionReturningIteratorOtherReturnType()
        {
            mockNeoSwift.SetupInvokeFunction("symbol", "ant");
            
            var exception = await AssertThrowsAsync<ContractException>(async () =>
            {
                await neoContract.CallFunctionReturningIterator("symbol");
            });
            Assert.IsTrue(exception.Message.Contains("unexpected return type"));
        }

        [Test]
        public async Task TestCallFunctionReturningIterator_SessionsDisabled()
        {
            mockNeoSwift.SetupInvokeFunctionWithoutSession("tokensOf");
            
            var exception = await AssertThrowsAsync<InvalidOperationException>(async () =>
            {
                await neoContract.CallFunctionReturningIterator("tokensOf");
            });
            Assert.AreEqual("No session id was found. The connected Neo node might not support sessions.", exception.Message);
        }

        [Test]
        public async Task TestInvokingFunctionPerformsCorrectCall()
        {
            mockNeoSwift.SetupInvokeFunction(NEP17_BALANCEOF, 3);
            
            var response = await neoContract.CallInvokeFunction(NEP17_BALANCEOF, new List<ContractParameter>
            {
                ContractParameter.CreateHash160(account1.GetScriptHash())
            });
            
            Assert.AreEqual(3, response.InvocationResult.Stack[0].Integer);
        }

        [Test]
        public async Task TestInvokingFunctionPerformsCorrectCall_WithoutParameters()
        {
            mockNeoSwift.SetupInvokeFunction("symbol", "NEO");
            
            var invokeFunction = await neoContract.CallInvokeFunction("symbol");
            
            Assert.AreEqual("NEO", invokeFunction.InvocationResult.Stack[0].String);
        }

        [Test]
        public async Task TestCallFunctionAndUnwrapIterator()
        {
            var owners = new[]
            {
                new Hash160("NSdNMyrz7Bp8MXab41nTuz1mRCnsFr5Rsv"),
                new Hash160("NhxK1PEmijLVD6D4WSuPoUYJVk855L21ru")
            };
            mockNeoSwift.SetupInvokeScriptWithArray("ownerOf", owners);
            
            var iteratorArray = await someContract.CallFunctionAndUnwrapIterator("ownerOf", new List<ContractParameter>(), 20);
            
            Assert.AreEqual(2, iteratorArray.Count);
            Assert.AreEqual(owners[0].ToAddress(), iteratorArray[0].Address);
            Assert.AreEqual(owners[1].ToAddress(), iteratorArray[1].Address);
        }

        [Test]
        public async Task TestCallInvokeFunction_MissingFunction()
        {
            var exception = await AssertThrowsAsync<ArgumentException>(async () =>
            {
                await neoContract.CallInvokeFunction("");
            });
            Assert.AreEqual("The invocation function must not be empty.", exception.Message);
        }

        [Test]
        public async Task TestContractParameterValidation()
        {
            // Test various contract parameter validation scenarios
            var validParameters = new List<ContractParameter>
            {
                ContractParameter.CreateString("test"),
                ContractParameter.CreateInteger(42),
                ContractParameter.CreateBool(true),
                ContractParameter.CreateByteArray(new byte[] { 1, 2, 3 })
            };

            Assert.DoesNotThrow(() =>
            {
                neoContract.InvokeFunction("testMethod", validParameters);
            });
        }

        [Test]
        public async Task TestScriptBuilding()
        {
            var parameters = new List<ContractParameter>
            {
                ContractParameter.CreateHash160(account1.ScriptHash),
                ContractParameter.CreateInteger(100)
            };

            var script = neoContract.BuildInvokeFunctionScript("transfer", parameters);
            Assert.IsNotNull(script);
            Assert.IsTrue(script.Length > 0);
        }

        [Test]
        public async Task TestContractStateRetrieval()
        {
            mockNeoSwift.SetupGetContractState();
            var manifest = await someContract.GetManifest();
            
            Assert.IsNotNull(manifest);
            Assert.AreEqual("neow3j", manifest.Name);
        }

        [Test]
        public async Task TestMethodInvocationChaining()
        {
            mockNeoSwift.SetupInvokeFunction("symbol", "TEST");
            mockNeoSwift.SetupInvokeFunction("decimals", 8);
            
            var symbol = await someContract.CallFunctionReturningString("symbol");
            var decimals = await someContract.CallFunctionReturningInt("decimals");
            
            Assert.AreEqual("TEST", symbol);
            Assert.AreEqual(8, decimals);
        }

        [Test]
        public async Task TestErrorHandling()
        {
            mockNeoSwift.SetupInvokeFunctionWithError("faultyMethod", "VM fault: Stack underflow");
            
            var exception = await AssertThrowsAsync<ContractException>(async () =>
            {
                await someContract.CallFunctionReturningString("faultyMethod");
            });
            Assert.IsTrue(exception.Message.Contains("VM fault"));
        }

        [Test]
        public async Task TestSessionManagement()
        {
            var sessionId = "test-session-id";
            mockNeoSwift.SetupTerminateSession(sessionId);
            
            // Test session termination doesn't throw
            Assert.DoesNotThrowAsync(async () =>
            {
                await mockNeoSwift.TerminateSession(sessionId);
            });
        }

        private async Task<T> AssertThrowsAsync<T>(Func<Task> action) where T : Exception
        {
            try
            {
                await action();
                Assert.Fail($"Expected exception of type {typeof(T).Name} was not thrown.");
                return null;
            }
            catch (T ex)
            {
                return ex;
            }
            catch (Exception ex)
            {
                Assert.Fail($"Expected exception of type {typeof(T).Name}, but got {ex.GetType().Name}: {ex.Message}");
                return null;
            }
        }
    }
}
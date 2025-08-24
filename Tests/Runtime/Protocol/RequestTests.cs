using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NUnit.Framework;
using Neo.Unity.SDK.Core;
using Neo.Unity.SDK.Types;
using Neo.Unity.SDK.Transaction;
using Neo.Unity.SDK.Tests.Helpers;
using Neo.Unity.SDK.Protocol.Response;
using Newtonsoft.Json;

namespace Neo.Unity.SDK.Tests.Protocol
{
    [TestFixture]
    public class RequestTests
    {
        private MockNeoSwift mockNeoSwift;
        private const string DefaultAccountAddress = "NM7Aky765FG8NhhwtxjXRx7jEL1cnw7PBP";
        private const string DefaultAccountPublicKey = "033a4d051b04b7fc0230d2b1aaedfd5a84be279a5361a7358db665ad7857787f1b";
        private const string NeoTokenHash = "ef4073a0f2b305a38ec4050e4d3d28bc40ea63f5";
        private const string CommitteeAccountScriptHash = "05859de95ccbbd5668e0f055b208273634d4657f";

        [SetUp]
        public void SetUp()
        {
            mockNeoSwift = new MockNeoSwift();
        }

        // MARK: Blockchain Methods

        [Test]
        public async Task TestGetBestBlockHash()
        {
            var expectedJson = @"{""jsonrpc"":""2.0"",""method"":""getbestblockhash"",""id"":1,""params"":[]}";
            
            await VerifyRequest(expectedJson, async neoSwift =>
            {
                return await neoSwift.GetBestBlockHash();
            });
        }

        [Test]
        public async Task TestGetBlockHash()
        {
            var expectedJson = @"{""jsonrpc"":""2.0"",""method"":""getblockhash"",""id"":1,""params"":[16293]}";
            
            await VerifyRequest(expectedJson, async neoSwift =>
            {
                return await neoSwift.GetBlockHash(16293);
            });
        }

        [Test]
        public async Task TestGetBlock_Index()
        {
            var expectedJson = @"{""jsonrpc"":""2.0"",""method"":""getblock"",""id"":1,""params"":[12345,1]}";
            
            await VerifyRequest(expectedJson, async neoSwift =>
            {
                return await neoSwift.GetBlock(12345, true);
            });
        }

        [Test]
        public async Task TestGetBlock_Index_OnlyHeader()
        {
            var expectedJson = @"{""jsonrpc"":""2.0"",""method"":""getblockheader"",""id"":1,""params"":[12345,1]}";
            
            await VerifyRequest(expectedJson, async neoSwift =>
            {
                return await neoSwift.GetBlock(12345, false);
            });
        }

        [Test]
        public async Task TestGetBlock_Hash()
        {
            var expectedJson = @"{""jsonrpc"":""2.0"",""method"":""getblock"",""id"":1,""params"":[""2240b34669038f82ac492150d391dfc3d7fe5e3c1d34e5b547d50e99c09b468d"",1]}";
            
            await VerifyRequest(expectedJson, async neoSwift =>
            {
                var hash = new Hash256("0x2240b34669038f82ac492150d391dfc3d7fe5e3c1d34e5b547d50e99c09b468d");
                return await neoSwift.GetBlock(hash, true);
            });
        }

        [Test]
        public async Task TestGetBlock_NotFullTxObjects()
        {
            var expectedJson = @"{""jsonrpc"":""2.0"",""method"":""getblockheader"",""id"":1,""params"":[""2240b34669038f82ac492150d391dfc3d7fe5e3c1d34e5b547d50e99c09b468d"",1]}";
            
            await VerifyRequest(expectedJson, async neoSwift =>
            {
                var hash = new Hash256("0x2240b34669038f82ac492150d391dfc3d7fe5e3c1d34e5b547d50e99c09b468d");
                return await neoSwift.GetBlock(hash, false);
            });
        }

        [Test]
        public async Task TestGetRawBlock_Index()
        {
            var expectedJson = @"{""jsonrpc"":""2.0"",""method"":""getblock"",""id"":1,""params"":[12345,0]}";
            
            await VerifyRequest(expectedJson, async neoSwift =>
            {
                return await neoSwift.GetRawBlock(12345);
            });
        }

        [Test]
        public async Task TestGetBlockHeaderCount()
        {
            var expectedJson = @"{""jsonrpc"":""2.0"",""method"":""getblockheadercount"",""id"":1,""params"":[]}";
            
            await VerifyRequest(expectedJson, async neoSwift =>
            {
                return await neoSwift.GetBlockHeaderCount();
            });
        }

        [Test]
        public async Task TestGetBlockCount()
        {
            var expectedJson = @"{""jsonrpc"":""2.0"",""method"":""getblockcount"",""id"":1,""params"":[]}";
            
            await VerifyRequest(expectedJson, async neoSwift =>
            {
                return await neoSwift.GetBlockCount();
            });
        }

        [Test]
        public async Task TestGetNativeContracts()
        {
            var expectedJson = @"{""jsonrpc"":""2.0"",""method"":""getnativecontracts"",""id"":1,""params"":[]}";
            
            await VerifyRequest(expectedJson, async neoSwift =>
            {
                return await neoSwift.GetNativeContracts();
            });
        }

        [Test]
        public async Task TestGetBlockHeader_Index()
        {
            var expectedJson = @"{""jsonrpc"":""2.0"",""method"":""getblockheader"",""id"":1,""params"":[12345,1]}";
            
            await VerifyRequest(expectedJson, async neoSwift =>
            {
                return await neoSwift.GetBlockHeader(12345);
            });
        }

        [Test]
        public async Task TestGetRawBlockHeader_Index()
        {
            var expectedJson = @"{""jsonrpc"":""2.0"",""method"":""getblockheader"",""id"":1,""params"":[12345,0]}";
            
            await VerifyRequest(expectedJson, async neoSwift =>
            {
                return await neoSwift.GetRawBlockHeader(12345);
            });
        }

        [Test]
        public async Task TestGetContractState()
        {
            var expectedJson = @"{""jsonrpc"":""2.0"",""method"":""getcontractstate"",""id"":1,""params"":[""dc675afc61a7c0f7b3d2682bf6e1d8ed865a0e5f""]}";
            
            await VerifyRequest(expectedJson, async neoSwift =>
            {
                var hash = new Hash160("dc675afc61a7c0f7b3d2682bf6e1d8ed865a0e5f");
                return await neoSwift.GetContractState(hash);
            });
        }

        [Test]
        public async Task TestGetContractState_ByName()
        {
            var expectedJson = @"{""jsonrpc"":""2.0"",""method"":""getcontractstate"",""id"":1,""params"":[""NeoToken""]}";
            
            await VerifyRequest(expectedJson, async neoSwift =>
            {
                return await neoSwift.GetNativeContractState("NeoToken");
            });
        }

        [Test]
        public async Task TestGetMemPool()
        {
            var expectedJson = @"{""jsonrpc"":""2.0"",""method"":""getrawmempool"",""id"":1,""params"":[1]}";
            
            await VerifyRequest(expectedJson, async neoSwift =>
            {
                return await neoSwift.GetMemPool();
            });
        }

        [Test]
        public async Task TestGetRawMemPool()
        {
            var expectedJson = @"{""jsonrpc"":""2.0"",""method"":""getrawmempool"",""id"":1,""params"":[]}";
            
            await VerifyRequest(expectedJson, async neoSwift =>
            {
                return await neoSwift.GetRawMemPool();
            });
        }

        [Test]
        public async Task TestGetTransaction()
        {
            var expectedJson = @"{""jsonrpc"":""2.0"",""method"":""getrawtransaction"",""id"":1,""params"":[""1f31821787b0a53df0ff7d6e0e7ecba3ac19dd517d6d2ea5aaf00432c20831d6"",1]}";
            
            await VerifyRequest(expectedJson, async neoSwift =>
            {
                var hash = new Hash256("0x1f31821787b0a53df0ff7d6e0e7ecba3ac19dd517d6d2ea5aaf00432c20831d6");
                return await neoSwift.GetTransaction(hash);
            });
        }

        [Test]
        public async Task TestGetRawTransaction()
        {
            var expectedJson = @"{""jsonrpc"":""2.0"",""method"":""getrawtransaction"",""id"":1,""params"":[""1f31821787b0a53df0ff7d6e0e7ecba3ac19dd517d6d2ea5aaf00432c20831d6"",0]}";
            
            await VerifyRequest(expectedJson, async neoSwift =>
            {
                var hash = new Hash256("0x1f31821787b0a53df0ff7d6e0e7ecba3ac19dd517d6d2ea5aaf00432c20831d6");
                return await neoSwift.GetRawTransaction(hash);
            });
        }

        [Test]
        public async Task TestGetStorage()
        {
            var key = "616e797468696e67";
            var hash = new Hash160("03febccf81ac85e3d795bc5cbd4e84e907812aa3");
            var expectedJson = $@"{{""jsonrpc"":""2.0"",""method"":""getstorage"",""id"":1,""params"":[""03febccf81ac85e3d795bc5cbd4e84e907812aa3"",""YW55dGhpbmc="""]}}";
            
            await VerifyRequest(expectedJson, async neoSwift =>
            {
                return await neoSwift.GetStorage(hash, key);
            });
        }

        [Test]
        public async Task TestGetTransactionHeight()
        {
            var hash = new Hash256("0x793f560ae7058a50c672890e69c9292391dd159ce963a33462059d03b9573d6a");
            var expectedJson = @"{""jsonrpc"":""2.0"",""method"":""gettransactionheight"",""id"":1,""params"":[""793f560ae7058a50c672890e69c9292391dd159ce963a33462059d03b9573d6a""]}";
            
            await VerifyRequest(expectedJson, async neoSwift =>
            {
                return await neoSwift.GetTransactionHeight(hash);
            });
        }

        [Test]
        public async Task TestGetNextBlockValidators()
        {
            var expectedJson = @"{""jsonrpc"":""2.0"",""method"":""getnextblockvalidators"",""id"":1,""params"":[]}";
            
            await VerifyRequest(expectedJson, async neoSwift =>
            {
                return await neoSwift.GetNextBlockValidators();
            });
        }

        [Test]
        public async Task TestGetCommittee()
        {
            var expectedJson = @"{""jsonrpc"":""2.0"",""method"":""getcommittee"",""id"":1,""params"":[]}";
            
            await VerifyRequest(expectedJson, async neoSwift =>
            {
                return await neoSwift.GetCommittee();
            });
        }

        // MARK: Node Methods

        [Test]
        public async Task TestGetConnectionCount()
        {
            var expectedJson = @"{""jsonrpc"":""2.0"",""method"":""getconnectioncount"",""id"":1,""params"":[]}";
            
            await VerifyRequest(expectedJson, async neoSwift =>
            {
                return await neoSwift.GetConnectionCount();
            });
        }

        [Test]
        public async Task TestGetPeers()
        {
            var expectedJson = @"{""jsonrpc"":""2.0"",""method"":""getpeers"",""id"":1,""params"":[]}";
            
            await VerifyRequest(expectedJson, async neoSwift =>
            {
                return await neoSwift.GetPeers();
            });
        }

        [Test]
        public async Task TestGetVersion()
        {
            var expectedJson = @"{""jsonrpc"":""2.0"",""method"":""getversion"",""id"":1,""params"":[]}";
            
            await VerifyRequest(expectedJson, async neoSwift =>
            {
                return await neoSwift.GetVersion();
            });
        }

        [Test]
        public async Task TestSendRawTransaction()
        {
            var rawTx = "gAAAAdQFqwPnNqAconfZSxN3ETx+lhu0VQUR/h1AjzDHeoJlAAACm3z/2qZ0vq4Pkw6+YIWvkJPl/lazSlwiDM3Pbvwzb8UAypo7AAAAACO6JwPFMmPo1uUi3DIgMznc2O7pm3z/2qZ0vq4Pkw6+YIWvkJPl/lazSlwiDM3Pbvwzb8UAGnEYAgAAAClfg/g/xDn1bm4fsGLYnG9TgmPXAUFANxHjZvyZ53oRC2yWtfiCjvlWptXPpctjJzQZFJARsPMNxUWPqlnkhn0Kx1N+MkyYEku2kf7KXF3fbtIPStt3giMhAmW/kGvzhfvz93eDLlWoeZG8++GbCX+3xcouQCWk1eXWrA==";
            var expectedJson = $@"{{""jsonrpc"":""2.0"",""method"":""sendrawtransaction"",""id"":1,""params"":[""{rawTx}"""]}}";
            
            await VerifyRequest(expectedJson, async neoSwift =>
            {
                return await neoSwift.SendRawTransaction("80000001d405ab03e736a01ca277d94b1377113c7e961bb4550511fe1d408f30c77a82650000029b7cffdaa674beae0f930ebe6085af9093e5fe56b34a5c220ccdcf6efc336fc500ca9a3b0000000023ba2703c53263e8d6e522dc32203339dcd8eee99b7cffdaa674beae0f930ebe6085af9093e5fe56b34a5c220ccdcf6efc336fc5001a711802000000295f83f83fc439f56e6e1fb062d89c6f538263d70141403711e366fc99e77a110b6c96b5f8828ef956a6d5cfa5cb63273419149011b0f30dc5458faa59e4867d0ac7537e324c98124bb691feca5c5ddf6ed20f4adb778223210265bf906bf385fbf3f777832e55a87991bcfbe19b097fb7c5ca2e4025a4d5e5d6ac");
            });
        }

        [Test]
        public async Task TestSubmitBlock()
        {
            var expectedJson = @"{""jsonrpc"":""2.0"",""method"":""submitblock"",""id"":1,""params"":[""00000000000000000000000000000000""]}";
            
            await VerifyRequest(expectedJson, async neoSwift =>
            {
                return await neoSwift.SubmitBlock("00000000000000000000000000000000");
            });
        }

        // MARK: Smart Contract Methods

        [Test]
        public async Task TestInvokeFunction()
        {
            var contractHash = new Hash160("af7c7328eee5a275a3bcaee2bf0cf662b5e739be");
            var accountHash = new Hash160("91b83e96f2a7c4fdf0c1688441ec61986c7cae26");
            var signerHash = new Hash160("0xcadb3dc2faa3ef14a13b619c9a43124755aa2569");
            var neoTokenHashObj = new Hash160(NeoTokenHash);
            
            var expectedJson = $@"{{""jsonrpc"":""2.0"",""method"":""invokefunction"",""id"":1,""params"":[""af7c7328eee5a275a3bcaee2bf0cf662b5e739be"",""balanceOf"",[{{""type"":""Hash160"",""value"":""91b83e96f2a7c4fdf0c1688441ec61986c7cae26""}}],[{{""allowedcontracts"":[""ef4073a0f2b305a38ec4050e4d3d28bc40ea63f5""],""account"":""cadb3dc2faa3ef14a13b619c9a43124755aa2569"",""rules"":[{{""condition"":{{""type"":""CalledByContract"",""hash"":""{NeoTokenHash}""}},""action"":""Allow""}}],""allowedgroups"":[""033a4d051b04b7fc0230d2b1aaedfd5a84be279a5361a7358db665ad7857787f1b""],""scopes"":""CalledByEntry,CustomContracts,CustomGroups,WitnessRules""}}]}}";
            
            await VerifyRequest(expectedJson, async neoSwift =>
            {
                var parameters = new List<ContractParameter>
                {
                    ContractParameter.CreateHash160(accountHash)
                };
                
                var signers = new List<AccountSigner>
                {
                    new AccountSigner(signerHash, WitnessScope.CalledByEntry | WitnessScope.CustomContracts | WitnessScope.CustomGroups | WitnessScope.WitnessRules)
                        .SetAllowedContracts(new[] { neoTokenHashObj })
                        .SetAllowedGroups(new[] { new Crypto.ECPublicKey(DefaultAccountPublicKey) })
                        .SetRules(new[] { new WitnessRule(WitnessAction.Allow, WitnessCondition.CalledByContract(neoTokenHashObj)) })
                };
                
                return await neoSwift.InvokeFunction(contractHash, "balanceOf", parameters, signers);
            });
        }

        [Test]
        public async Task TestInvokeFunctionDiagnostics()
        {
            var contractHash = new Hash160("af7c7328eee5a275a3bcaee2bf0cf662b5e739be");
            var accountHash = new Hash160("91b83e96f2a7c4fdf0c1688441ec61986c7cae26");
            var expectedJson = @"{""jsonrpc"":""2.0"",""method"":""invokefunction"",""id"":1,""params"":[""af7c7328eee5a275a3bcaee2bf0cf662b5e739be"",""balanceOf"",[{""type"":""Hash160"",""value"":""91b83e96f2a7c4fdf0c1688441ec61986c7cae26""}],[],1]}";
            
            await VerifyRequest(expectedJson, async neoSwift =>
            {
                var parameters = new List<ContractParameter>
                {
                    ContractParameter.CreateHash160(accountHash)
                };
                return await neoSwift.InvokeFunctionDiagnostics(contractHash, "balanceOf", parameters);
            });
        }

        [Test]
        public async Task TestInvokeScript()
        {
            var script = "10c00c08646563696d616c730c1425059ecb4878d3a875f91c51ceded330d4575fde41627d5b52";
            var expectedJson = $@"{{""jsonrpc"":""2.0"",""method"":""invokescript"",""id"":1,""params"":[""{Convert.ToBase64String(Neo.Unity.SDK.Utils.ByteExtensions.HexToBytes(script))}"",[]}}]}";
            
            await VerifyRequest(expectedJson, async neoSwift =>
            {
                return await neoSwift.InvokeScript(script);
            });
        }

        [Test]
        public async Task TestInvokeScriptDiagnostics()
        {
            var script = "10c00c08646563696d616c730c1425059ecb4878d3a875f91c51ceded330d4575fde41627d5b52";
            var expectedJson = $@"{{""jsonrpc"":""2.0"",""method"":""invokescript"",""id"":1,""params"":[""{Convert.ToBase64String(Neo.Unity.SDK.Utils.ByteExtensions.HexToBytes(script))}"",[], 1]}}";
            
            await VerifyRequest(expectedJson, async neoSwift =>
            {
                return await neoSwift.InvokeScriptDiagnostics(script);
            });
        }

        [Test]
        public async Task TestTraverseIterator()
        {
            var sessionId = "127d3320-db35-48d5-b6d3-ca22dca4a370";
            var iteratorId = "cb7ef774-1ade-4a83-914b-94373ca92010";
            var count = 100;
            var expectedJson = $@"{{""jsonrpc"":""2.0"",""method"":""traverseiterator"",""id"":1,""params"":[""{sessionId}"",""{iteratorId}"",{count}]}}";
            
            await VerifyRequest(expectedJson, async neoSwift =>
            {
                return await neoSwift.TraverseIterator(sessionId, iteratorId, count);
            });
        }

        [Test]
        public async Task TestTerminateSession()
        {
            var sessionId = "127d3320-db35-48d5-b6d3-ca22dca4a370";
            var expectedJson = $@"{{""jsonrpc"":""2.0"",""method"":""terminatesession"",""id"":1,""params"":[""{sessionId}""]}}";
            
            await VerifyRequest(expectedJson, async neoSwift =>
            {
                return await neoSwift.TerminateSession(sessionId);
            });
        }

        // MARK: Utilities Methods

        [Test]
        public async Task TestListPlugins()
        {
            var expectedJson = @"{""jsonrpc"":""2.0"",""method"":""listplugins"",""id"":1,""params"":[]}";
            
            await VerifyRequest(expectedJson, async neoSwift =>
            {
                return await neoSwift.ListPlugins();
            });
        }

        [Test]
        public async Task TestValidateAddress()
        {
            var address = "NTzVAPBpnUUCvrA6tFPxBHGge8Kyw8igxX";
            var expectedJson = $@"{{""jsonrpc"":""2.0"",""method"":""validateaddress"",""id"":1,""params"":[""{address}""]}}";
            
            await VerifyRequest(expectedJson, async neoSwift =>
            {
                return await neoSwift.ValidateAddress(address);
            });
        }

        // MARK: NEP17 Methods

        [Test]
        public async Task TestGetNep17Transfers()
        {
            var accountHash = new Hash160("04457ce4219e462146ac00b09793f81bc5bca2ce");
            var expectedJson = @"{""jsonrpc"":""2.0"",""method"":""getnep17transfers"",""id"":1,""params"":[""NekZLTu93WgrdFHxzBEJUYgLTQMAT85GLi""]}";
            
            await VerifyRequest(expectedJson, async neoSwift =>
            {
                return await neoSwift.GetNep17Transfers(accountHash);
            });
        }

        [Test]
        public async Task TestGetNep17Transfers_Date()
        {
            var accountHash = new Hash160("04457ce4219e462146ac00b09793f81bc5bca2ce");
            var fromDate = new DateTime(2019, 3, 20, 17, 30, 30, DateTimeKind.Utc);
            var expectedJson = @"{""jsonrpc"":""2.0"",""method"":""getnep17transfers"",""id"":1,""params"":[""NekZLTu93WgrdFHxzBEJUYgLTQMAT85GLi"",1553105830]}";
            
            await VerifyRequest(expectedJson, async neoSwift =>
            {
                return await neoSwift.GetNep17Transfers(accountHash, fromDate);
            });
        }

        [Test]
        public async Task TestGetNep17Balances()
        {
            var accountHash = new Hash160("5d75775015b024970bfeacf7c6ab1b0ade974886");
            var expectedJson = @"{""jsonrpc"":""2.0"",""method"":""getnep17balances"",""id"":1,""params"":[""NY9zhKwcmht5cQJ3oRqjJGo3QuVLwXwTzL""]}";
            
            await VerifyRequest(expectedJson, async neoSwift =>
            {
                return await neoSwift.GetNep17Balances(accountHash);
            });
        }

        // MARK: Application Log Methods

        [Test]
        public async Task TestGetApplicationLog()
        {
            var txHash = new Hash256("420d1eb458c707d698c6d2ba0f91327918ddb3b7bae2944df070f3f4e579078b");
            var expectedJson = @"{""jsonrpc"":""2.0"",""method"":""getapplicationlog"",""id"":1,""params"":[""420d1eb458c707d698c6d2ba0f91327918ddb3b7bae2944df070f3f4e579078b""]}";
            
            await VerifyRequest(expectedJson, async neoSwift =>
            {
                return await neoSwift.GetApplicationLog(txHash);
            });
        }

        // MARK: State Service Methods

        [Test]
        public async Task TestGetStateRoot()
        {
            var blockIndex = 52;
            var expectedJson = $@"{{""jsonrpc"":""2.0"",""method"":""getstateroot"",""id"":1,""params"":[{blockIndex}]}}";
            
            await VerifyRequest(expectedJson, async neoSwift =>
            {
                return await neoSwift.GetStateRoot(blockIndex);
            });
        }

        [Test]
        public async Task TestGetStateHeight()
        {
            var expectedJson = @"{""jsonrpc"":""2.0"",""method"":""getstateheight"",""id"":1,""params"":[]}";
            
            await VerifyRequest(expectedJson, async neoSwift =>
            {
                return await neoSwift.GetStateHeight();
            });
        }

        // Helper Methods

        private async Task VerifyRequest<T>(string expectedJson, Func<INeoUnity, Task<T>> makeRequest)
        {
            mockNeoSwift.SetupRequestInterceptor(request =>
            {
                var actualJson = request.ToString();
                // Normalize JSON for comparison
                var expectedObj = JsonConvert.DeserializeObject(expectedJson);
                var actualObj = JsonConvert.DeserializeObject(actualJson);
                var normalizedExpected = JsonConvert.SerializeObject(expectedObj, Formatting.None);
                var normalizedActual = JsonConvert.SerializeObject(actualObj, Formatting.None);
                Assert.AreEqual(normalizedExpected, normalizedActual);
            });

            var result = await makeRequest(mockNeoSwift);
            Assert.IsNotNull(result);
        }

        [Test]
        public async Task TestRequestIdIncrement()
        {
            var requests = new List<string>();
            mockNeoSwift.SetupRequestInterceptor(request =>
            {
                requests.Add(request.ToString());
            });

            await mockNeoSwift.GetBlockCount();
            await mockNeoSwift.GetBlockCount();
            await mockNeoSwift.GetBlockCount();

            Assert.AreEqual(3, requests.Count);
            
            // Verify ID increments
            for (int i = 0; i < requests.Count; i++)
            {
                var requestObj = JsonConvert.DeserializeObject<dynamic>(requests[i]);
                Assert.AreEqual(i + 1, (int)requestObj.id);
            }
        }

        [Test]
        public async Task TestJsonRpcVersion()
        {
            mockNeoSwift.SetupRequestInterceptor(request =>
            {
                var requestObj = JsonConvert.DeserializeObject<dynamic>(request.ToString());
                Assert.AreEqual("2.0", (string)requestObj.jsonrpc);
            });

            await mockNeoSwift.GetBlockCount();
        }

        [Test]
        public async Task TestParameterSerialization()
        {
            var contractHash = new Hash160("af7c7328eee5a275a3bcaee2bf0cf662b5e739be");
            var accountHash = new Hash160("91b83e96f2a7c4fdf0c1688441ec61986c7cae26");
            
            mockNeoSwift.SetupRequestInterceptor(request =>
            {
                var requestObj = JsonConvert.DeserializeObject<dynamic>(request.ToString());
                var parameters = requestObj.@params;
                
                // Verify contract hash serialization
                Assert.AreEqual("af7c7328eee5a275a3bcaee2bf0cf662b5e739be", (string)parameters[0]);
                
                // Verify method name
                Assert.AreEqual("balanceOf", (string)parameters[1]);
                
                // Verify parameter array
                var paramArray = parameters[2];
                Assert.AreEqual(1, paramArray.Count);
                Assert.AreEqual("Hash160", (string)paramArray[0].type);
                Assert.AreEqual("91b83e96f2a7c4fdf0c1688441ec61986c7cae26", (string)paramArray[0].value);
            });

            var contractParameters = new List<ContractParameter>
            {
                ContractParameter.CreateHash160(accountHash)
            };
            
            await mockNeoSwift.InvokeFunction(contractHash, "balanceOf", contractParameters);
        }

        [Test]
        public async Task TestErrorHandling()
        {
            mockNeoSwift.SetupErrorResponse("Invalid method", -32601);
            
            var exception = await AssertThrowsAsync<NeoUnityException>(async () =>
            {
                await mockNeoSwift.GetBlockCount();
            });
            
            Assert.IsTrue(exception.Message.Contains("Invalid method"));
        }

        [Test]
        public async Task TestTimeoutHandling()
        {
            mockNeoSwift.SetupTimeout(TimeSpan.FromSeconds(1));
            
            var exception = await AssertThrowsAsync<TaskCanceledException>(async () =>
            {
                await mockNeoSwift.GetBlockCount();
            });
            
            Assert.IsNotNull(exception);
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
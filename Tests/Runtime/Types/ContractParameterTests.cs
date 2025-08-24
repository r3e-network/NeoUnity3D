using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using NUnit.Framework;
using Neo.Unity.SDK.Types;
using Neo.Unity.SDK.Crypto;
using Neo.Unity.SDK.Wallet;
using Newtonsoft.Json;
using System.Threading.Tasks;

namespace Neo.Unity.SDK.Tests.Types
{
    [TestFixture]
    public class ContractParameterTests
    {
        private ContractParameter contractParameter;
        
        [SetUp]
        public void SetUp()
        {
            contractParameter = ContractParameter.CreateString("value");
        }

        [Test]
        public void TestStringFromString()
        {
            var result = ContractParameter.CreateString("value");
            AssertContractParameter(result, "value", ContractParameterType.String);
        }

        [Test]
        public void TestBytesFromBytes()
        {
            var bytes = new byte[] { 0x01, 0x01 };
            var p = ContractParameter.CreateByteArray(bytes);
            AssertContractParameter(p, bytes, ContractParameterType.ByteArray);
        }

        [Test]
        public void TestBytesFromBytesString()
        {
            var p = ContractParameter.CreateByteArray("0xa602");
            var expected = new byte[] { 0xa6, 0x02 };
            AssertContractParameter(p, expected, ContractParameterType.ByteArray);
        }

        [Test]
        public void TestBytesEquals()
        {
            var p1 = ContractParameter.CreateByteArray("0x796573");
            var p2 = ContractParameter.CreateByteArray(new byte[] { 0x79, 0x65, 0x73 });
            Assert.AreEqual(p1, p2);
        }

        [Test]
        public void TestBytesFromString()
        {
            var p = ContractParameter.CreateByteArrayFromString("Neo");
            var expected = new byte[] { 0x4e, 0x65, 0x6f };
            AssertContractParameter(p, expected, ContractParameterType.ByteArray);
        }

        [Test]
        public void TestBytesFromInvalidBytesString()
        {
            Assert.Throws<ArgumentException>(() =>
            {
                ContractParameter.CreateByteArray("value");
            }, "Argument is not a valid hex number.");
        }

        [Test]
        public void TestArrayFromArray()
        {
            var parameters = new List<ContractParameter>
            {
                ContractParameter.CreateString("value"),
                ContractParameter.CreateByteArray("0x0101")
            };
            var p = ContractParameter.CreateArray(parameters);
            AssertContractParameter(p, parameters, ContractParameterType.Array);
        }

        [Test]
        public void TestArrayFromEmpty()
        {
            var p = ContractParameter.CreateArray(new List<ContractParameter>());
            var arrayValue = p.Value as List<ContractParameter>;
            Assert.IsNotNull(arrayValue);
            Assert.AreEqual(0, arrayValue.Count);
        }

        [Test]
        public void TestNestedArray()
        {
            var p1 = "value";
            var p2 = "0x0101";
            var p3 = new BigInteger(420);
            var p4_1 = 1024;
            var p4_2 = "neow3j:)";
            var p4_3_1 = BigInteger.Parse("10");
            var p4_3 = new List<object> { p4_3_1 };
            var p4 = new List<object> { p4_1, p4_2, p4_3 };
            var p5 = 55;
            var parameters = new List<object> { p1, p2, p3, p4, p5 };

            var p = ContractParameter.CreateArray(parameters);
            Assert.AreEqual(ContractParameterType.Array, p.Type);

            var array = p.Value as List<ContractParameter>;
            Assert.IsNotNull(array);

            Assert.AreEqual(ContractParameter.CreateString(p1), array[0]);
            Assert.AreEqual(ContractParameter.CreateString(p2), array[1]);
            Assert.AreEqual(ContractParameter.CreateInteger(p3), array[2]);
            
            var expectedSubArray = new List<ContractParameter>
            {
                ContractParameter.CreateInteger(p4_1),
                ContractParameter.CreateString(p4_2),
                ContractParameter.CreateArray(new List<ContractParameter> { ContractParameter.CreateInteger(p4_3_1) })
            };
            Assert.AreEqual(ContractParameter.CreateArray(expectedSubArray), array[3]);
            Assert.AreEqual(ContractParameter.CreateInteger(p5), array[4]);
        }

        [Test]
        public void TestArrayWithInvalidType()
        {
            Assert.Throws<ArgumentException>(() =>
            {
                ContractParameter.CreateArray(new List<object> { 6.4 });
            }, "The provided object could not be casted into a supported contract parameter type.");
        }

        [Test]
        public void TestSignatureFromString()
        {
            var sig = "d8485d4771e9112cca6ac7e6b75fc52585a2e7ee9a702db4a39dfad0f888ea6c22b6185ceab38d8322b67737a5574d8b63f4e27b0d208f3f9efcdbf56093f213";
            var p = ContractParameter.CreateSignature(sig);
            var expectedBytes = ByteExtensions.HexToBytes(sig);
            AssertContractParameter(p, expectedBytes, ContractParameterType.Signature);
        }

        [Test]
        public void TestSignatureFromStringWith0x()
        {
            var sig = "d8485d4771e9112cca6ac7e6b75fc52585a2e7ee9a702db4a39dfad0f888ea6c22b6185ceab38d8322b67737a5574d8b63f4e27b0d208f3f9efcdbf56093f213";
            var p = ContractParameter.CreateSignature("0x" + sig);
            var expectedBytes = ByteExtensions.HexToBytes(sig);
            AssertContractParameter(p, expectedBytes, ContractParameterType.Signature);
        }

        [Test]
        public void TestSignatureFromBytes()
        {
            var sig = "d8485d4771e9112cca6ac7e6b75fc52585a2e7ee9a702db4a39dfad0f888ea6c22b6185ceab38d8322b67737a5574d8b63f4e27b0d208f3f9efcdbf56093f213";
            var bytes = ByteExtensions.HexToBytes(sig);
            var p = ContractParameter.CreateSignature(bytes);
            AssertContractParameter(p, bytes, ContractParameterType.Signature);
        }

        [Test]
        public void TestSignatureFromSignatureData()
        {
            var sig = "598235b9c5495cced03e41c0e4e0f7c4e3b8df3a190d33a76d764c5a6eb7581e8875976f63c1848cccc0822d8b8a534537da56a9b41f5e03977f83aae33d3558";
            var signatureBytes = ByteExtensions.HexToBytes(sig);
            var signatureData = new ECDSASignature(signatureBytes.Take(32).ToArray(), signatureBytes.Skip(32).ToArray());
            var p = ContractParameter.CreateSignature(signatureData);
            AssertContractParameter(p, signatureBytes, ContractParameterType.Signature);
        }

        [Test]
        public void TestSignatureFromTooShortString()
        {
            var sig = "d8485d4771e9112cca6ac7e6b75fc52585a2e7ee9a702db4a39dfad0f888ea6c22b6185ceab38d8322b67737a5574d8b63f4e27b0d208f3f9efcdbf56093f2";
            Assert.Throws<ArgumentException>(() =>
            {
                ContractParameter.CreateSignature(sig);
            }, "Signature is expected to have a length of 64 bytes, but had 63.");
        }

        [Test]
        public void TestSignatureFromTooLongString()
        {
            var sig = "d8485d4771e9112cca6ac7e6b75fc52585a2e7ee9a702db4a39dfad0f888ea6c22b6185ceab38d8322b67737a5574d8b63f4e27b0d208f3f9efcdbf56093f213ff";
            Assert.Throws<ArgumentException>(() =>
            {
                ContractParameter.CreateSignature(sig);
            }, "Signature is expected to have a length of 64 bytes, but had 65.");
        }

        [Test]
        public void TestSignatureFromInvalidHexString()
        {
            var sig = "d8485d4771e9112cca6ac7e6b75fc52585t2e7ee9a702db4a39dfad0f888ea6c22b6185ceab38d8322b67737a5574d8b63f4e27b0d208f3f9efcdbf56093f213";
            Assert.Throws<ArgumentException>(() =>
            {
                ContractParameter.CreateSignature(sig);
            }, "Argument is not a valid hex number.");
        }

        [Test]
        public void TestBool()
        {
            var p = ContractParameter.CreateBool(false);
            AssertContractParameter(p, false, ContractParameterType.Boolean);
            var p1 = ContractParameter.CreateBool(true);
            AssertContractParameter(p1, true, ContractParameterType.Boolean);
        }

        [Test]
        public void TestInt()
        {
            var p = ContractParameter.CreateInteger(10);
            AssertContractParameter(p, new BigInteger(10), ContractParameterType.Integer);
            var p1 = ContractParameter.CreateInteger(-1);
            AssertContractParameter(p1, new BigInteger(-1), ContractParameterType.Integer);
            var p2 = ContractParameter.CreateInteger(BigInteger.Parse("10"));
            AssertContractParameter(p2, BigInteger.Parse("10"), ContractParameterType.Integer);
        }

        [Test]
        public void TestHash160()
        {
            var hash = new Hash160("576f6f6c6f576f6f6c6f576f6f6c6f576f6f6c6f");
            var p = ContractParameter.CreateHash160(hash);
            AssertContractParameter(p, hash, ContractParameterType.Hash160);
        }

        [Test]
        public async Task TestHash160FromAccount()
        {
            var account = await Account.Create();
            var p = ContractParameter.CreateHash160(account);
            AssertContractParameter(p, account.ScriptHash, ContractParameterType.Hash160);
        }

        [Test]
        public void TestHash256()
        {
            var hash = new Hash256("576f6f6c6f576f6f6c6f576f6f6c6f576f6f6c6ff6c6f576f6f6c6f576f6f6cf");
            var p = ContractParameter.CreateHash256(hash);
            AssertContractParameter(p, hash, ContractParameterType.Hash256);
        }

        [Test]
        public void TestHash256FromString()
        {
            var hashString = "576f6f6c6f576f6f6c6f576f6f6c6f576f6f6c6ff6c6f576f6f6c6f576f6f6cf";
            var hash = new Hash256(hashString);
            var p = ContractParameter.CreateHash256(hashString);
            AssertContractParameter(p, hash, ContractParameterType.Hash256);
        }

        [Test]
        public void TestHash256FromTooShortString()
        {
            Assert.Throws<ArgumentException>(() =>
            {
                ContractParameter.CreateHash256("576f6f6c6f576f6f6c6f576f6f6c6f576f6f6c6ff6c6f576f6f6c6f576f6f6");
            }, "Hash must be 32 bytes long but was 31 bytes.");
        }

        [Test]
        public void TestHash256FromTooLongString()
        {
            Assert.Throws<ArgumentException>(() =>
            {
                ContractParameter.CreateHash256("576f6f6c6f576f6f6c6f576f6f6c6f576f6f6c6ff6c6f576f6f6c6f576f6f6cfaa");
            }, "Hash must be 32 bytes long but was 33 bytes.");
        }

        [Test]
        public void TestHash256FromInvalidHexString()
        {
            Assert.Throws<ArgumentException>(() =>
            {
                ContractParameter.CreateHash256("576f6f6c6f576f6f6c6f576f6f6c6f576f6f6c6ff6c6f576f6f6c6f576f6f6cg");
            }, "String argument is not hexadecimal.");
        }

        [Test]
        public void TestPublicKeyFromPublicKey()
        {
            var key = new ECPublicKey("03b4af8efe55d98b44eedfcfaa39642fd5d53ad543d18d3cc2db5880970a4654f6");
            var p = ContractParameter.CreatePublicKey(key);
            AssertContractParameter(p, key.GetEncodedBytes(true), ContractParameterType.PublicKey);
        }

        [Test]
        public void TestPublicKeyFromBytes()
        {
            var bytes = ByteExtensions.HexToBytes("03b4af8efe55d98b44eedfcfaa39642fd5d53ad543d18d3cc2db5880970a4654f6");
            var p = ContractParameter.CreatePublicKey(bytes);
            AssertContractParameter(p, bytes, ContractParameterType.PublicKey);
        }

        [Test]
        public void TestPublicKeyFromString()
        {
            var keyString = "03b4af8efe55d98b44eedfcfaa39642fd5d53ad543d18d3cc2db5880970a4654f6";
            var p = ContractParameter.CreatePublicKey(keyString);
            var expectedBytes = ByteExtensions.HexToBytes(keyString);
            AssertContractParameter(p, expectedBytes, ContractParameterType.PublicKey);
        }

        [Test]
        public void TestPublicKeyFromInvalidBytes()
        {
            var bytes = ByteExtensions.HexToBytes("03b4af8d061b6b320cce6c63bc4ec7894dce107bfc5f5ef5c68a93b4ad1e1368");
            Assert.Throws<ArgumentException>(() =>
            {
                ContractParameter.CreatePublicKey(bytes);
            }, "Public key argument must be 33 bytes but was 32 bytes.");
        }

        [Test]
        public void TestPublicKeyFromInvalidString()
        {
            var keyString = "03b4af8d061b6b320cce6c63bc4ec7894dce107bfc5f5ef5c68a93b4ad1e1368";
            Assert.Throws<ArgumentException>(() =>
            {
                ContractParameter.CreatePublicKey(keyString);
            }, "Public key argument must be 33 bytes but was 32 bytes.");
        }

        [Test]
        public void TestMap()
        {
            var map = new Dictionary<ContractParameter, ContractParameter>
            {
                { ContractParameter.CreateInteger(1), ContractParameter.CreateString("first") },
                { ContractParameter.CreateInteger(2), ContractParameter.CreateString("second") }
            };
            var p = ContractParameter.CreateMap(map);
            AssertContractParameter(p, map, ContractParameterType.Map);
        }

        [Test]
        public void TestMapWithObjects()
        {
            var map = new Dictionary<object, object>
            {
                { "one", "first" },
                { "two", 2 }
            };
            var p = ContractParameter.CreateMap(map);
            var resultMap = p.Value as Dictionary<ContractParameter, ContractParameter>;
            Assert.IsNotNull(resultMap);
            Assert.AreEqual(2, resultMap.Count);
            Assert.IsTrue(resultMap.ContainsKey(ContractParameter.CreateString("one")));
            Assert.IsTrue(resultMap.ContainsKey(ContractParameter.CreateString("two")));
            Assert.IsTrue(resultMap.ContainsValue(ContractParameter.CreateString("first")));
            Assert.IsTrue(resultMap.ContainsValue(ContractParameter.CreateInteger(2)));
        }

        [Test]
        public void TestMapNested()
        {
            var map1Key = 5;
            var map1 = new Dictionary<object, object> { { "hello", 1234 } };
            var map = new Dictionary<object, object>
            {
                { "one", "first" },
                { "two", 2 },
                { map1Key, map1 }
            };
            
            var p = ContractParameter.CreateMap(map);
            var resultMap = p.Value as Dictionary<ContractParameter, ContractParameter>;
            Assert.IsNotNull(resultMap);
            Assert.AreEqual(3, resultMap.Count);
            Assert.IsTrue(resultMap.ContainsKey(ContractParameter.CreateString("one")));
            Assert.IsTrue(resultMap.ContainsKey(ContractParameter.CreateString("two")));
            Assert.IsTrue(resultMap.ContainsValue(ContractParameter.CreateString("first")));
            Assert.IsTrue(resultMap.ContainsValue(ContractParameter.CreateInteger(2)));
            
            var expectedNestedMap = ContractParameter.CreateMap(map1);
            Assert.AreEqual(expectedNestedMap, resultMap[ContractParameter.CreateInteger(map1Key)]);
        }

        [Test]
        public void TestMapInvalidKey()
        {
            var map = new Dictionary<ContractParameter, ContractParameter>
            {
                { ContractParameter.CreateArray(new List<object> { 1, "test" }), ContractParameter.CreateString("first") }
            };
            Assert.Throws<ArgumentException>(() =>
            {
                ContractParameter.CreateMap(map);
            }, "The provided map contains an invalid key. The keys cannot be of type array or map.");
        }

        [Test]
        public void TestMapEmpty()
        {
            Assert.Throws<ArgumentException>(() =>
            {
                ContractParameter.CreateMap(new Dictionary<object, object>());
            }, "At least one map entry is required to create a map contract parameter.");
        }

        [Test]
        public void TestEquals()
        {
            Assert.AreEqual(contractParameter, ContractParameter.CreateString("value"));
            Assert.AreNotEqual(contractParameter, ContractParameter.CreateString("test"));
            Assert.AreNotEqual(contractParameter, ContractParameter.CreateInteger(1));

            var p1 = ContractParameter.CreateHash160(Hash160.Zero);
            var p2 = ContractParameter.CreateHash160(Hash160.Zero);
            Assert.AreEqual(p1, p2);

            p2 = new ContractParameter { Type = ContractParameterType.Any };
            Assert.AreNotEqual(p1, p2);

            p2 = new ContractParameter { Type = ContractParameterType.Hash160, Value = null };
            Assert.AreNotEqual(p1, p2);

            p1 = new ContractParameter { Type = ContractParameterType.Hash160, Value = null };
            p2 = new ContractParameter { Type = ContractParameterType.Hash160, Value = null };
            Assert.AreEqual(p1, p2);
        }

        [Test]
        public void TestDeserializeAndSerialize()
        {
            var json = GetTestJson();
            var contractParameter = JsonConvert.DeserializeObject<ContractParameter>(json);
            var array = contractParameter.Value as List<ContractParameter>;
            Assert.IsNotNull(array);

            AssertValue(array[0], new BigInteger(1000));
            AssertValue(array[1], new BigInteger(1000));

            var array2 = array[2].Value as List<ContractParameter>;
            Assert.IsNotNull(array2);

            AssertValue(array2[0], "hello, world!");
            AssertValue(array2[1], new byte[] { 0x01, 0x02, 0x03 });
            AssertValue(array2[2], new byte[] { 0x01, 0x02, 0x03 });
            AssertValue(array2[3], new byte[] { 0x01, 0x02, 0x03 });
            AssertValue(array2[4], true);
            AssertValue(array2[5], true);
            AssertValue(array2[6], new Hash160("69ecca587293047be4c59159bf8bc399985c160d"));
            AssertValue(array2[7], new Hash256("fe26f525c17b58f63a4d106fba973ec34cc99bfe2501c9f672cc145b483e398b"));
            Assert.IsNull(array2[8].Value);

            var map = array2[9].Value as Dictionary<ContractParameter, ContractParameter>;
            Assert.IsNotNull(map);

            var keys = new List<object>();
            foreach (var key in map.Keys)
                keys.Add(key.Value);
            Assert.IsTrue(keys.Contains(new BigInteger(5)));
            Assert.IsTrue(keys.Contains(new byte[] { 0x01, 0x02, 0x03 }));

            var values = new List<object>();
            foreach (var value in map.Values)
                values.Add(value.Value);
            Assert.IsTrue(values.Contains("value"));
            Assert.IsTrue(values.Contains(new BigInteger(5)));

            var reencodedJson = JsonConvert.SerializeObject(contractParameter);
            var reencodedContractParameter = JsonConvert.DeserializeObject<ContractParameter>(reencodedJson);
            Assert.AreEqual(reencodedContractParameter, contractParameter);
        }

        private void AssertValue<T>(ContractParameter parameter, T expected)
        {
            if (parameter.Value is T actual)
            {
                if (typeof(T) == typeof(byte[]))
                {
                    CollectionAssert.AreEqual(expected as byte[], actual as byte[]);
                }
                else
                {
                    Assert.AreEqual(expected, actual);
                }
            }
            else
            {
                Assert.Fail($"Expected value of type {typeof(T)}, but got {parameter.Value?.GetType()}");
            }
        }

        private void AssertContractParameter(ContractParameter parameter, object expectedValue, ContractParameterType expectedType)
        {
            Assert.AreEqual(expectedType, parameter.Type);
            
            if (expectedValue is byte[] expectedBytes && parameter.Value is byte[] actualBytes)
            {
                CollectionAssert.AreEqual(expectedBytes, actualBytes);
            }
            else if (expectedValue is List<ContractParameter> expectedList && parameter.Value is List<ContractParameter> actualList)
            {
                CollectionAssert.AreEqual(expectedList, actualList);
            }
            else if (expectedValue is Dictionary<ContractParameter, ContractParameter> expectedDict && parameter.Value is Dictionary<ContractParameter, ContractParameter> actualDict)
            {
                CollectionAssert.AreEqual(expectedDict, actualDict);
            }
            else
            {
                Assert.AreEqual(expectedValue, parameter.Value);
            }
        }

        private string GetTestJson()
        {
            return @"{
                ""type"":""Array"",
                ""value"": [
                    {
                        ""type"":""Integer"",
                        ""value"":1000
                    },
                    {
                        ""type"":""Integer"",
                        ""value"":""1000""
                    },
                    {
                        ""type"":""Array"",
                        ""value"":[
                            {
                                ""type"":""String"",
                                ""value"":""hello, world!""
                            },
                            {
                                ""type"":""ByteArray"",
                                ""value"":""AQID""
                            },
                            {
                                ""type"":""Signature"",
                                ""value"":""AQID""
                            },
                            {
                                ""type"":""PublicKey"",
                                ""value"":""010203""
                            },
                            {
                                ""type"":""Boolean"",
                                ""value"":true
                            },
                            {
                                ""type"":""Boolean"",
                                ""value"":""true""
                            },
                            {
                                ""type"":""Hash160"",
                                ""value"":""69ecca587293047be4c59159bf8bc399985c160d""
                            },
                            {
                                ""type"":""Hash256"",
                                ""value"":""fe26f525c17b58f63a4d106fba973ec34cc99bfe2501c9f672cc145b483e398b""
                            },
                            {
                                ""type"":""Any"",
                                ""value"":""""
                            },
                           {
                               ""type"": ""Map"",
                               ""value"": [
                               {
                                   ""key"":
                                   {
                                       ""type"": ""Integer"",
                                       ""value"": ""5""
                                   },
                                   ""value"":
                                   {
                                       ""type"": ""String"",
                                       ""value"": ""value""
                                   }
                               },
                               {
                                   ""key"":
                                   {
                                       ""type"": ""ByteArray"",
                                       ""value"":""AQID""
                                   },
                                   ""value"":
                                   {
                                       ""type"": ""Integer"",
                                       ""value"": ""5""
                                   }
                               }
                           ]
                           }
                        ]
                    }
                ]
            }";
        }
    }
}
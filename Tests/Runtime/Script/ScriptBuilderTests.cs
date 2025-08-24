using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Neo.Unity.SDK;
using Neo.Unity.SDK.Script;
using Neo.Unity.SDK.Tests.Helpers;

namespace Neo.Unity.SDK.Tests.Script
{
    /// <summary>
    /// Unity Test Framework implementation of ScriptBuilder tests
    /// Converted from Swift ScriptBuilderTests.swift with Unity-specific enhancements
    /// </summary>
    [TestFixture]
    public class ScriptBuilderTests
    {
        private ScriptBuilder builder;

        [SetUp]
        public void SetUp()
        {
            builder = new ScriptBuilder();
        }

        [Test]
        public void TestPushArrayEmpty()
        {
            // Act
            builder.PushArray(new object[0]);

            // Assert
            AssertBuilder(new byte[] { (byte)OpCode.NEWARRAY0 });
        }

        [Test]
        public void TestPushParamEmptyArray()
        {
            // Act
            builder.PushParam(ContractParameter.Array(new ContractParameter[0]));

            // Assert
            AssertBuilder(new byte[] { (byte)OpCode.NEWARRAY0 });
        }

        [Test]
        public void TestPushByteArray()
        {
            // Test small array (1 byte)
            builder.PushData(CreateByteArray(1));
            AssertBuilder(TestHelpers.HexToBytes("0c01"), firstN: 2);

            builder = new ScriptBuilder();
            builder.PushData(CreateByteArray(75));
            AssertBuilder(TestHelpers.HexToBytes("0c4b"), firstN: 2);

            builder = new ScriptBuilder();
            builder.PushData(CreateByteArray(256));
            AssertBuilder(TestHelpers.HexToBytes("0d0001"), firstN: 3);

            builder = new ScriptBuilder();
            builder.PushData(CreateByteArray(65536));
            AssertBuilder(TestHelpers.HexToBytes("0e00000100"), firstN: 5);
        }

        [Test]
        public void TestPushString()
        {
            // Test empty string
            builder.PushData("");
            AssertBuilder(TestHelpers.HexToBytes("0c00"), firstN: 2);

            builder = new ScriptBuilder();
            builder.PushData("a");
            AssertBuilder(TestHelpers.HexToBytes("0c0161"), firstN: 3);

            builder = new ScriptBuilder();
            builder.PushData(new string('a', 10000));
            AssertBuilder(TestHelpers.HexToBytes("0e1027"), firstN: 3);
        }

        [Test]
        public void TestPushInteger()
        {
            // Test push zero
            builder.PushInteger(0);
            AssertBuilder(new byte[] { (byte)OpCode.PUSH0 }, firstN: 1);

            builder = new ScriptBuilder();
            builder.PushInteger(1);
            AssertBuilder(new byte[] { (byte)OpCode.PUSH1 }, firstN: 1);

            builder = new ScriptBuilder();
            builder.PushInteger(-1);
            AssertBuilder(new byte[] { (byte)OpCode.PUSHM1 }, firstN: 1);

            builder = new ScriptBuilder();
            builder.PushInteger(16);
            AssertBuilder(new byte[] { (byte)OpCode.PUSH16 }, firstN: 1);

            builder = new ScriptBuilder();
            builder.PushInteger(17);
            AssertBuilder(new byte[] { (byte)OpCode.PUSHINT8, 17 }, firstN: 2);

            builder = new ScriptBuilder();
            builder.PushInteger(256);
            AssertBuilder(new byte[] { (byte)OpCode.PUSHINT16, 0x00, 0x01 }, firstN: 3);
        }

        [Test]
        public void TestPushBigInteger()
        {
            // Test BigInteger values
            builder.PushBigInteger(BigInteger.Zero);
            AssertBuilder(new byte[] { (byte)OpCode.PUSH0 }, firstN: 1);

            builder = new ScriptBuilder();
            builder.PushBigInteger(BigInteger.One);
            AssertBuilder(new byte[] { (byte)OpCode.PUSH1 }, firstN: 1);

            builder = new ScriptBuilder();
            builder.PushBigInteger(BigInteger.MinusOne);
            AssertBuilder(new byte[] { (byte)OpCode.PUSHM1 }, firstN: 1);

            builder = new ScriptBuilder();
            builder.PushBigInteger(new BigInteger(100));
            AssertBuilder(new byte[] { (byte)OpCode.PUSHINT8, 100 }, firstN: 2);
        }

        [Test]
        public void TestPushBoolean()
        {
            // Test true
            builder.PushBoolean(true);
            AssertBuilder(new byte[] { (byte)OpCode.PUSHT }, firstN: 1);

            builder = new ScriptBuilder();
            builder.PushBoolean(false);
            AssertBuilder(new byte[] { (byte)OpCode.PUSHF }, firstN: 1);
        }

        [Test]
        public void TestPushParam()
        {
            // Test various parameter types
            builder.PushParam(ContractParameter.Boolean(true));
            AssertBuilder(new byte[] { (byte)OpCode.PUSHT }, firstN: 1);

            builder = new ScriptBuilder();
            builder.PushParam(ContractParameter.Integer(42));
            AssertBuilder(new byte[] { (byte)OpCode.PUSHINT8, 42 }, firstN: 2);

            builder = new ScriptBuilder();
            builder.PushParam(ContractParameter.String("hello"));
            var expected = new List<byte> { (byte)OpCode.PUSHDATA1, 5 };
            expected.AddRange(System.Text.Encoding.UTF8.GetBytes("hello"));
            AssertBuilder(expected.ToArray());

            builder = new ScriptBuilder();
            builder.PushParam(ContractParameter.ByteArray(new byte[] { 0x01, 0x02, 0x03 }));
            AssertBuilder(new byte[] { (byte)OpCode.PUSHDATA1, 3, 0x01, 0x02, 0x03 });
        }

        [Test]
        public void TestOpCode()
        {
            // Test raw opcode insertion
            builder.OpCode(OpCode.NOP);
            AssertBuilder(new byte[] { (byte)OpCode.NOP }, firstN: 1);

            builder.OpCode(OpCode.PUSH0);
            AssertBuilder(new byte[] { (byte)OpCode.NOP, (byte)OpCode.PUSH0 }, firstN: 2);
        }

        [Test]
        public void TestRawBytes()
        {
            // Test raw byte insertion
            var rawBytes = new byte[] { 0x01, 0x02, 0x03, 0x04 };
            builder.RawBytes(rawBytes);
            AssertBuilder(rawBytes);
        }

        [Test]
        public void TestContractCall()
        {
            // Arrange
            var contractHash = Hash160.Parse("ef4073a0f2b305a38ec4050e4d3d28bc40ea63f5");
            var method = "balanceOf";
            var parameters = new ContractParameter[]
            {
                ContractParameter.Hash160(Hash160.Parse("969a77db482f74ce27105f760efa139223431394"))
            };

            // Act
            builder.ContractCall(contractHash, method, parameters);

            // Assert
            var script = builder.ToArray();
            Assert.IsNotNull(script);
            Assert.Greater(script.Length, 0);

            // Verify the script contains the expected components
            Assert.IsTrue(ContainsBytes(script, System.Text.Encoding.UTF8.GetBytes(method)));
        }

        [Test]
        public void TestSysCall()
        {
            // Test system call
            var sysCallName = "System.Runtime.CheckWitness";
            builder.SysCall(sysCallName);

            var script = builder.ToArray();
            Assert.IsNotNull(script);
            Assert.Greater(script.Length, 0);
            Assert.IsTrue(ContainsBytes(script, System.Text.Encoding.UTF8.GetBytes(sysCallName)));
        }

        [Test]
        public void TestJump()
        {
            // Test unconditional jump
            builder.Jump(OpCode.JMP, 10);
            var script = builder.ToArray();
            Assert.IsNotNull(script);
            Assert.AreEqual((byte)OpCode.JMP, script[0]);
        }

        [Test]
        public void TestComplexScript()
        {
            // Build a complex script with multiple operations
            var contractHash = Hash160.Parse("ef4073a0f2b305a38ec4050e4d3d28bc40ea63f5");
            var recipient = Hash160.Parse("969a77db482f74ce27105f760efa139223431394");

            builder
                .PushParam(ContractParameter.Hash160(contractHash))
                .PushParam(ContractParameter.Hash160(recipient))
                .PushParam(ContractParameter.Integer(100))
                .PushParam(ContractParameter.Any(null))
                .PushInteger(4)
                .OpCode(OpCode.PACK)
                .PushData("transfer")
                .PushParam(ContractParameter.Hash160(contractHash))
                .SysCall("System.Contract.Call");

            var script = builder.ToArray();
            Assert.IsNotNull(script);
            Assert.Greater(script.Length, 50); // Complex script should be substantial
        }

        [Test]
        public void TestScriptSize()
        {
            // Test that script size is calculated correctly
            Assert.AreEqual(0, builder.Size);

            builder.PushInteger(1);
            Assert.AreEqual(1, builder.Size);

            builder.PushData("hello");
            Assert.AreEqual(8, builder.Size); // 1 (previous) + 1 (PUSHDATA1) + 1 (length) + 5 (data)
        }

        [Test]
        public void TestClear()
        {
            // Build some script
            builder.PushInteger(1).PushInteger(2).OpCode(OpCode.ADD);
            Assert.Greater(builder.Size, 0);

            // Clear and verify
            builder.Clear();
            Assert.AreEqual(0, builder.Size);
            Assert.AreEqual(0, builder.ToArray().Length);
        }

        #region Unity-Specific Tests

        [Test]
        [Performance]
        public void TestPerformance_LargeScriptBuilding()
        {
            const int iterations = 1000;

            // Act & Assert
            var executionTime = TestHelpers.MeasureExecutionTime(() =>
            {
                var testBuilder = new ScriptBuilder();
                for (int i = 0; i < iterations; i++)
                {
                    testBuilder.PushInteger(i);
                }
                var script = testBuilder.ToArray();
                GC.KeepAlive(script);
            });

            Assert.Less(executionTime, 1000, "Large script building should complete within 1 second");
            Debug.Log($"Large script building time: {executionTime}ms for {iterations} operations");
        }

        [Test]
        public void TestMemoryUsage_ScriptBuilder()
        {
            const int iterations = 100;

            // Act & Assert
            var memoryUsage = TestHelpers.MeasureMemoryUsage(() =>
            {
                var builders = new ScriptBuilder[iterations];
                for (int i = 0; i < iterations; i++)
                {
                    builders[i] = new ScriptBuilder();
                    builders[i].PushInteger(i);
                    builders[i].PushData($"test{i}");
                    builders[i].OpCode(OpCode.NOP);
                }
                GC.KeepAlive(builders);
            });

            var avgMemoryPerBuilder = memoryUsage / iterations;
            Assert.Less(Math.Abs(avgMemoryPerBuilder), 10 * 1024, "Each ScriptBuilder should use less than 10KB");
            Debug.Log($"Memory usage per ScriptBuilder: {avgMemoryPerBuilder} bytes");
        }

        [UnityTest]
        public IEnumerator TestUnityCoroutineCompatibility()
        {
            // Arrange
            bool scriptBuildingCompleted = false;
            byte[] result = null;

            // Act - Simulate async script building in coroutine
            yield return new WaitForEndOfFrame();

            var testBuilder = new ScriptBuilder();
            testBuilder.PushInteger(42);
            testBuilder.PushData("unity");
            testBuilder.OpCode(OpCode.NOP);
            result = testBuilder.ToArray();
            scriptBuildingCompleted = true;

            yield return new WaitUntil(() => scriptBuildingCompleted);

            // Assert
            Assert.IsNotNull(result);
            Assert.Greater(result.Length, 0);
        }

        [Test]
        public void TestSerializationForUnityInspector()
        {
            // Test that ScriptBuilder results can work with Unity serialization
            builder.PushInteger(42);
            builder.PushData("test");
            builder.OpCode(OpCode.NOP);

            var script = builder.ToArray();
            var serializableScript = new SerializableScriptInfo
            {
                scriptHex = TestHelpers.BytesToHex(script),
                scriptSize = script.Length,
                operationCount = 3
            };

            var jsonString = JsonUtility.ToJson(serializableScript, true);
            Assert.IsNotNull(jsonString);
            Assert.IsTrue(jsonString.Contains(TestHelpers.BytesToHex(script)));

            Debug.Log($"Serializable script info: {jsonString}");

            // Test deserialization
            var deserializedScript = JsonUtility.FromJson<SerializableScriptInfo>(jsonString);
            Assert.AreEqual(TestHelpers.BytesToHex(script), deserializedScript.scriptHex);
            Assert.AreEqual(script.Length, deserializedScript.scriptSize);
            Assert.AreEqual(3, deserializedScript.operationCount);
        }

        [Test]
        public void TestThreadSafety()
        {
            // Arrange
            const int threadCount = 4;
            const int operationsPerThread = 100;
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
                        var threadBuilder = new ScriptBuilder();
                        for (int i = 0; i < operationsPerThread; i++)
                        {
                            threadBuilder.PushInteger(threadIndex * 1000 + i);
                            threadBuilder.PushData($"thread{threadIndex}op{i}");
                            threadBuilder.OpCode(OpCode.NOP);
                        }
                        var script = threadBuilder.ToArray();
                        Assert.Greater(script.Length, 0);
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

        [Test]
        public void TestEdgeCases_InvalidInputs()
        {
            // Test null inputs
            Assert.Throws<ArgumentNullException>(() => builder.PushData((byte[])null));
            Assert.Throws<ArgumentNullException>(() => builder.PushData((string)null));
            Assert.Throws<ArgumentNullException>(() => builder.PushParam(null));
            Assert.Throws<ArgumentNullException>(() => builder.RawBytes(null));

            // Test very large integers
            var largeInt = BigInteger.Pow(2, 256);
            Assert.DoesNotThrow(() => builder.PushBigInteger(largeInt));

            // Test empty operations
            Assert.DoesNotThrow(() => builder.ToArray()); // Should return empty array
            Assert.AreEqual(0, builder.ToArray().Length);
        }

        [Test]
        public void TestDataIntegrity_RoundTripSerialization()
        {
            // Build a script with various data types
            var originalHash = Hash160.Parse("ef4073a0f2b305a38ec4050e4d3d28bc40ea63f5");
            var originalString = "Hello, Neo Unity SDK!";
            var originalInt = 12345;
            var originalBytes = new byte[] { 0x01, 0x02, 0x03, 0x04 };

            builder
                .PushParam(ContractParameter.Hash160(originalHash))
                .PushData(originalString)
                .PushInteger(originalInt)
                .PushData(originalBytes)
                .OpCode(OpCode.NOP);

            var script = builder.ToArray();

            // Verify script can be read back
            var reader = new BinaryReader(script);
            Assert.Greater(reader.Available, 0);

            // The exact format depends on how parameters are encoded,
            // but we can verify the script contains our data
            var scriptHex = TestHelpers.BytesToHex(script);
            Assert.IsTrue(scriptHex.Contains(TestHelpers.BytesToHex(originalBytes)));
        }

        #endregion

        #region Helper Methods

        private void AssertBuilder(byte[] expected, int firstN = -1)
        {
            var actual = builder.ToArray();
            if (firstN > 0)
            {
                var truncated = new byte[firstN];
                Array.Copy(actual, 0, truncated, 0, Math.Min(firstN, actual.Length));
                TestHelpers.AssertBytesEqual(expected, truncated);
            }
            else
            {
                TestHelpers.AssertBytesEqual(expected, actual);
            }
        }

        private byte[] CreateByteArray(int length)
        {
            var result = new byte[length];
            for (int i = 0; i < length; i++)
            {
                result[i] = 0x01;
            }
            return result;
        }

        private bool ContainsBytes(byte[] haystack, byte[] needle)
        {
            if (needle.Length > haystack.Length) return false;

            for (int i = 0; i <= haystack.Length - needle.Length; i++)
            {
                bool found = true;
                for (int j = 0; j < needle.Length; j++)
                {
                    if (haystack[i + j] != needle[j])
                    {
                        found = false;
                        break;
                    }
                }
                if (found) return true;
            }
            return false;
        }

        #endregion

        #region Helper Classes

        [System.Serializable]
        private class SerializableScriptInfo
        {
            public string scriptHex;
            public int scriptSize;
            public int operationCount;
        }

        #endregion
    }
}
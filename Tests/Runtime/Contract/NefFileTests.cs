using System;
using System.Collections;
using System.IO;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Neo.Unity.SDK;
using Neo.Unity.SDK.Contracts;
using Neo.Unity.SDK.Tests.Helpers;

namespace Neo.Unity.SDK.Tests.Contract
{
    /// <summary>
    /// Unity Test Framework implementation of NEF file handling tests
    /// Converted from Swift NefFileTests.swift with Unity-specific enhancements
    /// </summary>
    [TestFixture]
    public class NefFileTests
    {
        private const string MAGIC = "4e454633"; // NEF3 in hex (reversed)
        private const string TESTCONTRACT_COMPILER = "neon-3.0.0.0";
        private const string TESTCONTRACT_COMPILER_HEX = "6e656f77336a2d332e302e3000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000";
        private const string RESERVED_BYTES = "0000";
        private const string TESTCONTRACT_SCRIPT_SIZE = "05";
        private const string TESTCONTRACT_SCRIPT = "5700017840";
        private const string TESTCONTRACT_CHECKSUM = "760f39a0";
        private const string TESTCONTRACT_WITH_TOKENS_SCRIPT = "213701004021370000405700017840";
        private const string TESTCONTRACT_WITH_TOKENS_CHECKSUM = "b559a069";

        [Test]
        public void TestNewNefFile()
        {
            // Arrange
            var script = TestHelpers.HexToBytes(TESTCONTRACT_SCRIPT);

            // Act
            var nef = new NefFile(TESTCONTRACT_COMPILER, new NefFile.MethodToken[0], script);

            // Assert
            Assert.AreEqual(TESTCONTRACT_COMPILER, nef.Compiler);
            TestHelpers.AssertBytesEqual(script, nef.Script);
            Assert.AreEqual(TESTCONTRACT_CHECKSUM, TestHelpers.BytesToHex(nef.Checksum));
            Assert.AreEqual(0, nef.MethodTokens.Length);
        }

        [Test]
        public void TestNewNefFileWithMethodTokens()
        {
            // Arrange
            var script = TestHelpers.HexToBytes(TESTCONTRACT_WITH_TOKENS_SCRIPT);
            var methodTokens = new[]
            {
                new NefFile.MethodToken(
                    Hash160.Parse("f61eebf573ea36593fd43aa150c055ad7906ab83"),
                    "getGasPerBlock", 0, true, CallFlags.All),
                new NefFile.MethodToken(
                    Hash160.Parse("70e2301955bf1e74cbb31d18c2f96972abadb328"),
                    "totalSupply", 0, true, CallFlags.All)
            };

            // Act
            var nef = new NefFile(TESTCONTRACT_COMPILER, methodTokens, script);

            // Assert
            Assert.AreEqual(TESTCONTRACT_COMPILER, nef.Compiler);
            TestHelpers.AssertBytesEqual(script, nef.Script);
            Assert.AreEqual(methodTokens.Length, nef.MethodTokens.Length);
            Assert.AreEqual(TESTCONTRACT_WITH_TOKENS_CHECKSUM, TestHelpers.BytesToHex(nef.Checksum));
        }

        [Test]
        public void TestFailConstructorWithTooLongCompilerName()
        {
            // Arrange
            var tooLongCompiler = new string('a', 65); // Too long compiler name
            var script = TestHelpers.HexToBytes(TESTCONTRACT_SCRIPT);

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => 
                new NefFile(tooLongCompiler, new NefFile.MethodToken[0], script));
            
            Assert.That(exception.Message, Does.Contain("compiler name").IgnoreCase);
        }

        [Test]
        public void TestReadFromFile()
        {
            // Create a test NEF file in memory
            var expectedNef = CreateTestNefFile();
            var nefBytes = expectedNef.ToByteArray();

            // Write to temporary file
            var tempFilePath = Path.GetTempFileName();
            try
            {
                File.WriteAllBytes(tempFilePath, nefBytes);

                // Act
                var nef = NefFile.ReadFromFile(tempFilePath);

                // Assert
                Assert.AreEqual(expectedNef.Compiler, nef.Compiler);
                TestHelpers.AssertBytesEqual(expectedNef.Script, nef.Script);
                TestHelpers.AssertBytesEqual(expectedNef.Checksum, nef.Checksum);
            }
            finally
            {
                if (File.Exists(tempFilePath))
                    File.Delete(tempFilePath);
            }
        }

        [Test]
        public void TestReadFromFileThatIsTooLarge()
        {
            // Create a file that's too large
            var tempFilePath = Path.GetTempFileName();
            try
            {
                var tooLargeFile = new byte[NefFile.MAX_NEF_FILE_SIZE + 1];
                File.WriteAllBytes(tempFilePath, tooLargeFile);

                // Act & Assert
                var exception = Assert.Throws<ArgumentException>(() => 
                    NefFile.ReadFromFile(tempFilePath));
                
                Assert.That(exception.Message, Does.Contain("too large").IgnoreCase);
            }
            finally
            {
                if (File.Exists(tempFilePath))
                    File.Delete(tempFilePath);
            }
        }

        [Test]
        public void TestDeserializeAndSerialize()
        {
            // Arrange
            var originalNef = CreateTestNefFile();
            var bytes = originalNef.ToByteArray();

            // Act
            var deserializedNef = NefFile.FromByteArray(bytes);
            var reserializedBytes = deserializedNef.ToByteArray();

            // Assert
            Assert.AreEqual(originalNef.Compiler, deserializedNef.Compiler);
            TestHelpers.AssertBytesEqual(originalNef.Script, deserializedNef.Script);
            TestHelpers.AssertBytesEqual(originalNef.Checksum, deserializedNef.Checksum);
            TestHelpers.AssertBytesEqual(bytes, reserializedBytes);
        }

        [Test]
        public void TestDeserializeWithWrongMagicNumber()
        {
            // Arrange
            var nefHex = "00000000" + // Wrong magic number
                        TESTCONTRACT_COMPILER_HEX +
                        RESERVED_BYTES + "00" + // no tokens
                        RESERVED_BYTES +
                        TESTCONTRACT_SCRIPT_SIZE +
                        TESTCONTRACT_SCRIPT +
                        TESTCONTRACT_CHECKSUM;

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => 
                NefFile.FromByteArray(TestHelpers.HexToBytes(nefHex)));
            
            Assert.That(exception.Message, Does.Contain("magic number").IgnoreCase);
        }

        [Test]
        public void TestDeserializeWithWrongChecksum()
        {
            // Arrange
            var nefHex = MAGIC +
                        TESTCONTRACT_COMPILER_HEX +
                        RESERVED_BYTES +
                        "00" + // no tokens
                        RESERVED_BYTES +
                        TESTCONTRACT_SCRIPT_SIZE +
                        TESTCONTRACT_SCRIPT +
                        "00000000"; // Wrong checksum

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => 
                NefFile.FromByteArray(TestHelpers.HexToBytes(nefHex)));
            
            Assert.That(exception.Message, Does.Contain("checksum").IgnoreCase);
        }

        [Test]
        public void TestDeserializeWithEmptyScript()
        {
            // Arrange
            var nefHex = MAGIC +
                        TESTCONTRACT_COMPILER_HEX +
                        RESERVED_BYTES +
                        "00" + // no tokens
                        RESERVED_BYTES +
                        "00" + // empty script
                        TESTCONTRACT_CHECKSUM;

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => 
                NefFile.FromByteArray(TestHelpers.HexToBytes(nefHex)));
            
            Assert.That(exception.Message, Does.Contain("script").And.Contain("empty").IgnoreCase);
        }

        [Test]
        public void TestGetSize()
        {
            // Arrange
            var nef = CreateTestNefFile();
            var bytes = nef.ToByteArray();

            // Act & Assert
            Assert.AreEqual(bytes.Length, nef.Size);
        }

        [Test]
        public void TestNullInputValidation()
        {
            // Test null compiler
            Assert.Throws<ArgumentNullException>(() => 
                new NefFile(null, new NefFile.MethodToken[0], TestHelpers.HexToBytes(TESTCONTRACT_SCRIPT)));

            // Test null script
            Assert.Throws<ArgumentNullException>(() => 
                new NefFile(TESTCONTRACT_COMPILER, new NefFile.MethodToken[0], null));

            // Test null method tokens
            Assert.Throws<ArgumentNullException>(() => 
                new NefFile(TESTCONTRACT_COMPILER, null, TestHelpers.HexToBytes(TESTCONTRACT_SCRIPT)));

            // Test null byte array for deserialization
            Assert.Throws<ArgumentNullException>(() => NefFile.FromByteArray(null));

            // Test null file path
            Assert.Throws<ArgumentNullException>(() => NefFile.ReadFromFile(null));
        }

        [Test]
        public void TestEmptyInputValidation()
        {
            // Test empty compiler
            Assert.Throws<ArgumentException>(() => 
                new NefFile("", new NefFile.MethodToken[0], TestHelpers.HexToBytes(TESTCONTRACT_SCRIPT)));

            // Test empty script
            Assert.Throws<ArgumentException>(() => 
                new NefFile(TESTCONTRACT_COMPILER, new NefFile.MethodToken[0], new byte[0]));

            // Test empty file path
            Assert.Throws<ArgumentException>(() => NefFile.ReadFromFile(""));
        }

        [Test]
        public void TestChecksumCalculation()
        {
            // Create two identical NEF files
            var nef1 = CreateTestNefFile();
            var nef2 = CreateTestNefFile();

            // Should have identical checksums
            TestHelpers.AssertBytesEqual(nef1.Checksum, nef2.Checksum);

            // Different scripts should have different checksums
            var differentScript = TestHelpers.HexToBytes("FF00FF00FF");
            var nef3 = new NefFile(TESTCONTRACT_COMPILER, new NefFile.MethodToken[0], differentScript);
            
            Assert.AreNotEqual(TestHelpers.BytesToHex(nef1.Checksum), TestHelpers.BytesToHex(nef3.Checksum));
        }

        private NefFile CreateTestNefFile()
        {
            var script = TestHelpers.HexToBytes(TESTCONTRACT_SCRIPT);
            return new NefFile(TESTCONTRACT_COMPILER, new NefFile.MethodToken[0], script);
        }

        #region Unity-Specific Tests

        [Test]
        [Performance]
        public void TestPerformance_NefFileOperations()
        {
            // Arrange
            const int iterations = 50;
            var nef = CreateTestNefFile();

            // Test serialization performance
            var serializationTime = TestHelpers.MeasureExecutionTime(() =>
            {
                for (int i = 0; i < iterations; i++)
                {
                    var bytes = nef.ToByteArray();
                    GC.KeepAlive(bytes);
                }
            });

            // Test deserialization performance
            var bytes = nef.ToByteArray();
            var deserializationTime = TestHelpers.MeasureExecutionTime(() =>
            {
                for (int i = 0; i < iterations; i++)
                {
                    var deserializedNef = NefFile.FromByteArray(bytes);
                    GC.KeepAlive(deserializedNef);
                }
            });

            var avgSerializationTime = (float)serializationTime / iterations;
            var avgDeserializationTime = (float)deserializationTime / iterations;

            // Assert reasonable performance
            Assert.Less(avgSerializationTime, 10f, "NEF serialization should average less than 10ms");
            Assert.Less(avgDeserializationTime, 10f, "NEF deserialization should average less than 10ms");
            
            Debug.Log($"NEF serialization average time: {avgSerializationTime}ms per operation ({iterations} iterations)");
            Debug.Log($"NEF deserialization average time: {avgDeserializationTime}ms per operation ({iterations} iterations)");
        }

        [Test]
        public void TestMemoryUsage_NefFileOperations()
        {
            // Test memory usage of NEF file operations
            var memoryUsage = TestHelpers.MeasureMemoryUsage(() =>
            {
                var nef = CreateTestNefFile();
                var bytes = nef.ToByteArray();
                var deserializedNef = NefFile.FromByteArray(bytes);
                var reserializedBytes = deserializedNef.ToByteArray();
                GC.KeepAlive(new object[] { nef, bytes, deserializedNef, reserializedBytes });
            });

            // Assert reasonable memory usage
            Assert.Less(Math.Abs(memoryUsage), 50 * 1024, "NEF operations should use less than 50KB additional memory");
            Debug.Log($"NEF operations memory usage: {memoryUsage} bytes");
        }

        [UnityTest]
        public IEnumerator TestUnityCoroutineCompatibility()
        {
            // Arrange
            var nef = CreateTestNefFile();
            byte[] serializedBytes = null;
            NefFile deserializedNef = null;

            // Act - Simulate async NEF operations in coroutine
            yield return new WaitForEndOfFrame();
            
            serializedBytes = nef.ToByteArray();
            
            yield return new WaitForEndOfFrame();
            
            deserializedNef = NefFile.FromByteArray(serializedBytes);

            // Assert
            Assert.IsNotNull(serializedBytes);
            Assert.IsNotNull(deserializedNef);
            Assert.AreEqual(nef.Compiler, deserializedNef.Compiler);
        }

        [Test]
        public void TestSerializationForUnityInspector()
        {
            // Create a serializable wrapper for Unity Inspector
            var nef = CreateTestNefFile();
            var serializedData = new SerializableNefData
            {
                compiler = nef.Compiler,
                scriptHex = TestHelpers.BytesToHex(nef.Script),
                checksumHex = TestHelpers.BytesToHex(nef.Checksum),
                methodTokenCount = nef.MethodTokens.Length,
                sizeBytes = nef.Size
            };

            // Act
            var jsonString = JsonUtility.ToJson(serializedData, true);
            var deserializedData = JsonUtility.FromJson<SerializableNefData>(jsonString);

            // Assert
            Assert.AreEqual(nef.Compiler, deserializedData.compiler);
            Assert.AreEqual(TestHelpers.BytesToHex(nef.Script), deserializedData.scriptHex);
            Assert.AreEqual(TestHelpers.BytesToHex(nef.Checksum), deserializedData.checksumHex);
        }

        [Test]
        public void TestThreadSafety_NefFileOperations()
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
                            var nef = CreateTestNefFile();
                            var bytes = nef.ToByteArray();
                            var deserializedNef = NefFile.FromByteArray(bytes);
                            
                            if (!nef.Compiler.Equals(deserializedNef.Compiler))
                            {
                                throw new Exception("NEF serialization round-trip failed in thread");
                            }
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
        private class SerializableNefData
        {
            public string compiler;
            public string scriptHex;
            public string checksumHex;
            public int methodTokenCount;
            public int sizeBytes;
        }

        #endregion
    }
}
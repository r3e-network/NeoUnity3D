using System;
using System.Collections;
using System.Text;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Neo.Unity.SDK;
using Neo.Unity.SDK.Tests.Helpers;
using Neo.Unity.SDK.Utils;

namespace Neo.Unity.SDK.Tests.Crypto
{
    /// <summary>
    /// Unity Test Framework implementation of Base64 encoding/decoding tests
    /// Converted from Swift Base64Tests.swift with Unity-specific enhancements
    /// </summary>
    [TestFixture]
    public class Base64Tests
    {
        private const string INPUT_STRING = "150c14242dbf5e2f6ac2568b59b7822278d571b75f17be0c14242dbf5e2f6ac2568b59b7822278d571b75f17be13c00c087472616e736665720c14897720d8cd76f4f00abfa37c0edd889c208fde9b41627d5b5238";
        private const string OUTPUT_STRING = "FQwUJC2/Xi9qwlaLWbeCInjVcbdfF74MFCQtv14vasJWi1m3giJ41XG3Xxe+E8AMCHRyYW5zZmVyDBSJdyDYzXb08Aq/o3wO3YicII/em0FifVtSOA==";

        [Test]
        public void TestBase64EncodeForString()
        {
            // Act
            var encoded = Convert.ToBase64String(TestHelpers.HexToBytes(INPUT_STRING));

            // Assert
            Assert.AreEqual(OUTPUT_STRING, encoded);
        }

        [Test]
        public void TestBase64EncodeForBytes()
        {
            // Arrange
            var inputBytes = TestHelpers.HexToBytes(INPUT_STRING);

            // Act
            var encoded = Convert.ToBase64String(inputBytes);

            // Assert
            Assert.AreEqual(OUTPUT_STRING, encoded);
        }

        [Test]
        public void TestBase64Decode()
        {
            // Act
            var decoded = Convert.FromBase64String(OUTPUT_STRING);
            var decodedHex = TestHelpers.BytesToHex(decoded);

            // Assert
            Assert.AreEqual(INPUT_STRING.ToLowerInvariant(), decodedHex);
        }

        [Test]
        public void TestBase64EncodeDecodeRoundTrip()
        {
            // Arrange
            var originalBytes = TestHelpers.HexToBytes(INPUT_STRING);

            // Act
            var encoded = Convert.ToBase64String(originalBytes);
            var decoded = Convert.FromBase64String(encoded);

            // Assert
            TestHelpers.AssertBytesEqual(originalBytes, decoded);
        }

        [Test]
        public void TestBase64EncodeEmptyInput()
        {
            // Arrange
            var emptyBytes = new byte[0];

            // Act
            var encoded = Convert.ToBase64String(emptyBytes);

            // Assert
            Assert.AreEqual("", encoded);
        }

        [Test]
        public void TestBase64DecodeEmptyInput()
        {
            // Act
            var decoded = Convert.FromBase64String("");

            // Assert
            Assert.AreEqual(0, decoded.Length);
        }

        [Test]
        public void TestBase64EncodeVariousLengths()
        {
            // Test different input lengths to verify padding
            var testCases = new[]
            {
                ("41", "QQ=="),           // 1 byte
                ("4142", "QUI="),        // 2 bytes
                ("414243", "QUJD"),      // 3 bytes
                ("41424344", "QUJDRA==") // 4 bytes
            };

            foreach (var (hex, expectedBase64) in testCases)
            {
                // Arrange
                var bytes = TestHelpers.HexToBytes(hex);

                // Act
                var encoded = Convert.ToBase64String(bytes);

                // Assert
                Assert.AreEqual(expectedBase64, encoded, $"Failed for hex: {hex}");
            }
        }

        [Test]
        public void TestBase64DecodeInvalidInput()
        {
            // Test invalid characters
            Assert.Throws<FormatException>(() => Convert.FromBase64String("Invalid@Base64"));
            
            // Test invalid length (not multiple of 4)
            Assert.Throws<FormatException>(() => Convert.FromBase64String("QUI"));
        }

        [Test]
        public void TestBase64UTF8StringEncoding()
        {
            // Arrange
            var testString = "Hello, Neo Unity SDK! 🚀";
            var utf8Bytes = Encoding.UTF8.GetBytes(testString);

            // Act
            var encoded = Convert.ToBase64String(utf8Bytes);
            var decodedBytes = Convert.FromBase64String(encoded);
            var decodedString = Encoding.UTF8.GetString(decodedBytes);

            // Assert
            Assert.AreEqual(testString, decodedString);
        }

        #region Unity-Specific Tests

        [Test]
        [Performance]
        public void TestPerformance_Base64Encoding()
        {
            // Arrange
            const int iterations = 1000;
            var testData = TestHelpers.GenerateRandomBytes(1024); // 1KB test data

            // Act & Assert
            var executionTime = TestHelpers.MeasureExecutionTime(() =>
            {
                for (int i = 0; i < iterations; i++)
                {
                    var encoded = Convert.ToBase64String(testData);
                    GC.KeepAlive(encoded);
                }
            });

            var avgTime = (float)executionTime / iterations;
            Assert.Less(avgTime, 1f, "Base64 encoding should average less than 1ms per operation");
            Debug.Log($"Base64 encoding average time: {avgTime}ms per 1KB ({iterations} iterations)");
        }

        [Test]
        [Performance]
        public void TestPerformance_Base64Decoding()
        {
            // Arrange
            const int iterations = 1000;
            var testData = TestHelpers.GenerateRandomBytes(1024);
            var encodedData = Convert.ToBase64String(testData);

            // Act & Assert
            var executionTime = TestHelpers.MeasureExecutionTime(() =>
            {
                for (int i = 0; i < iterations; i++)
                {
                    var decoded = Convert.FromBase64String(encodedData);
                    GC.KeepAlive(decoded);
                }
            });

            var avgTime = (float)executionTime / iterations;
            Assert.Less(avgTime, 1f, "Base64 decoding should average less than 1ms per operation");
            Debug.Log($"Base64 decoding average time: {avgTime}ms per 1KB ({iterations} iterations)");
        }

        [Test]
        public void TestMemoryUsage_Base64Operations()
        {
            // Arrange
            var testData = TestHelpers.GenerateRandomBytes(1024);

            // Test encoding memory usage
            var encodingMemory = TestHelpers.MeasureMemoryUsage(() =>
            {
                var encoded = Convert.ToBase64String(testData);
                GC.KeepAlive(encoded);
            });

            // Test decoding memory usage
            var encodedData = Convert.ToBase64String(testData);
            var decodingMemory = TestHelpers.MeasureMemoryUsage(() =>
            {
                var decoded = Convert.FromBase64String(encodedData);
                GC.KeepAlive(decoded);
            });

            // Assert reasonable memory usage
            Assert.Less(Math.Abs(encodingMemory), 5 * 1024, "Base64 encoding should use less than 5KB additional memory");
            Assert.Less(Math.Abs(decodingMemory), 5 * 1024, "Base64 decoding should use less than 5KB additional memory");
            
            Debug.Log($"Base64 encoding memory usage: {encodingMemory} bytes");
            Debug.Log($"Base64 decoding memory usage: {decodingMemory} bytes");
        }

        [UnityTest]
        public IEnumerator TestUnityCoroutineCompatibility()
        {
            // Arrange
            var testData = TestHelpers.GenerateRandomBytes(512);
            string encodedResult = null;
            byte[] decodedResult = null;

            // Act - Simulate async Base64 operations in coroutine
            yield return new WaitForEndOfFrame();
            
            encodedResult = Convert.ToBase64String(testData);
            
            yield return new WaitForEndOfFrame();
            
            decodedResult = Convert.FromBase64String(encodedResult);

            // Assert
            Assert.IsNotNull(encodedResult);
            Assert.IsNotNull(decodedResult);
            TestHelpers.AssertBytesEqual(testData, decodedResult);
        }

        [Test]
        public void TestThreadSafety_Base64Operations()
        {
            // Arrange
            const int threadCount = 4;
            const int operationsPerThread = 100;
            var testData = TestHelpers.GenerateRandomBytes(256);
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
                            var encoded = Convert.ToBase64String(testData);
                            var decoded = Convert.FromBase64String(encoded);
                            
                            if (!TestHelpers.BytesToHex(decoded).Equals(TestHelpers.BytesToHex(testData)))
                            {
                                throw new Exception("Base64 round-trip failed in thread");
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

        [Test]
        public void TestEdgeCases_LargeData()
        {
            // Test with large data (1MB)
            var largeData = TestHelpers.GenerateRandomBytes(1024 * 1024);

            // Act
            var encoded = Convert.ToBase64String(largeData);
            var decoded = Convert.FromBase64String(encoded);

            // Assert
            TestHelpers.AssertBytesEqual(largeData, decoded);
            Assert.IsTrue(encoded.Length > largeData.Length, "Encoded data should be larger than original");
        }

        [Test]
        public void TestSerializationForUnityInspector()
        {
            // Create a serializable wrapper for Unity Inspector
            var testData = TestHelpers.HexToBytes(INPUT_STRING);
            var serializedData = new SerializableBase64Data
            {
                originalHex = INPUT_STRING,
                encodedBase64 = Convert.ToBase64String(testData),
                dataSize = testData.Length
            };

            // Act
            var jsonString = JsonUtility.ToJson(serializedData, true);
            var deserializedData = JsonUtility.FromJson<SerializableBase64Data>(jsonString);

            // Assert
            Assert.AreEqual(INPUT_STRING, deserializedData.originalHex);
            Assert.AreEqual(OUTPUT_STRING, deserializedData.encodedBase64);
            Assert.AreEqual(testData.Length, deserializedData.dataSize);
        }

        #endregion

        #region Helper Classes

        [System.Serializable]
        private class SerializableBase64Data
        {
            public string originalHex;
            public string encodedBase64;
            public int dataSize;
        }

        #endregion
    }
}
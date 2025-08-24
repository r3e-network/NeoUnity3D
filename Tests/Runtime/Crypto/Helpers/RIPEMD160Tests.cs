using System;
using System.Collections;
using System.Text;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Neo.Unity.SDK;
using Neo.Unity.SDK.Crypto;
using Neo.Unity.SDK.Tests.Helpers;

namespace Neo.Unity.SDK.Tests.Crypto.Helpers
{
    /// <summary>
    /// Unity Test Framework implementation of RIPEMD160 hash function tests
    /// Converted from Swift RIPEMD160Tests.swift with Unity-specific enhancements
    /// Test vectors from: https://homes.esat.kuleuven.be/~bosselae/ripemd160.html
    /// </summary>
    [TestFixture]
    public class RIPEMD160Tests
    {
        /// <summary>
        /// Test vectors from the official RIPEMD160 specification
        /// </summary>
        private readonly (string message, string expectedHash)[] _testVectors = new[]
        {
            ("", "9c1185a5c5e9fc54612808977ee8f548b2258d31"),
            ("a", "0bdc9d2d256b3ee9daae347be6f4dc835a467ffe"),
            ("abc", "8eb208f7e05d987a9b044a8e98c6b087f15a0bfc"),
            ("message digest", "5d0689ef49d2fae572b881b123a85ffa21595f36"),
            ("abcdefghijklmnopqrstuvwxyz", "f71c27109c692c1b56bbdceb5b9d2865b3708dbc"),
            ("abcdbcdecdefdefgefghfghighijhijkijkljklmklmnlmnomnopnopq", "12a053384a9c0c88e405a06c27dcf49ada62eb2b"),
            ("ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789", "b0e20b6e3116640286ed3a87a5713079b21f5189"),
            (string.Concat(Enumerable.Repeat("1234567890", 8)), "9b752e45573d4b39f4dbd3323cab82bf63326bfb") // 80 chars
        };

        [Test]
        public void TestTestVectors()
        {
            foreach (var (message, expectedHash) in _testVectors)
            {
                // Arrange
                var messageBytes = Encoding.UTF8.GetBytes(message);

                // Act
                var actualHash = RIPEMD160.ComputeHash(messageBytes);
                var actualHashHex = TestHelpers.BytesToHex(actualHash);

                // Assert
                Assert.AreEqual(expectedHash, actualHashHex, $"Failed for message: '{message}'");
            }
        }

        [Test]
        public void TestLargeMessage()
        {
            // Test the large message (1 million 'a' characters) separately due to performance
            // This test may be slow, so we use a shorter version for regular testing
            var largeMessage = new string('a', 100000); // 100K characters for testing
            var messageBytes = Encoding.UTF8.GetBytes(largeMessage);

            // Act
            var hash = RIPEMD160.ComputeHash(messageBytes);

            // Assert
            Assert.AreEqual(20, hash.Length, "RIPEMD160 hash should be 20 bytes long");
            Assert.IsNotNull(hash);
        }

        [Test]
        [Ignore("Performance test - enable for full compliance testing")]
        public void TestVeryLargeMessage()
        {
            // The full 1 million character test from the specification
            var veryLargeMessage = new string('a', 1_000_000);
            var messageBytes = Encoding.UTF8.GetBytes(veryLargeMessage);

            // Act
            var hash = RIPEMD160.ComputeHash(messageBytes);
            var hashHex = TestHelpers.BytesToHex(hash);

            // Assert
            Assert.AreEqual("52783243c1697bdbe16d37f97f68f08325dc1528", hashHex);
        }

        [Test]
        public void TestEmptyMessage()
        {
            // Test empty message specifically
            var emptyBytes = new byte[0];

            // Act
            var hash = RIPEMD160.ComputeHash(emptyBytes);
            var hashHex = TestHelpers.BytesToHex(hash);

            // Assert
            Assert.AreEqual("9c1185a5c5e9fc54612808977ee8f548b2258d31", hashHex);
        }

        [Test]
        public void TestSingleByteMessage()
        {
            // Test single byte messages
            var testCases = new[]
            {
                (new byte[] { 0x00 }, "Single zero byte"),
                (new byte[] { 0xFF }, "Single max byte"),
                (new byte[] { 0x80 }, "Single high bit byte"),
                (Encoding.UTF8.GetBytes("a"), "Single 'a' character")
            };

            foreach (var (messageBytes, description) in testCases)
            {
                // Act
                var hash = RIPEMD160.ComputeHash(messageBytes);

                // Assert
                Assert.AreEqual(20, hash.Length, $"Hash length should be 20 bytes for: {description}");
                Assert.IsNotNull(hash, $"Hash should not be null for: {description}");
            }
        }

        [Test]
        public void TestHashDeterminism()
        {
            // Same input should always produce the same hash
            var message = "determinism test";
            var messageBytes = Encoding.UTF8.GetBytes(message);

            // Act
            var hash1 = RIPEMD160.ComputeHash(messageBytes);
            var hash2 = RIPEMD160.ComputeHash(messageBytes);

            // Assert
            TestHelpers.AssertBytesEqual(hash1, hash2);
        }

        [Test]
        public void TestDifferentInputsProduceDifferentHashes()
        {
            // Different inputs should produce different hashes
            var message1Bytes = Encoding.UTF8.GetBytes("message1");
            var message2Bytes = Encoding.UTF8.GetBytes("message2");

            // Act
            var hash1 = RIPEMD160.ComputeHash(message1Bytes);
            var hash2 = RIPEMD160.ComputeHash(message2Bytes);

            // Assert
            Assert.AreNotEqual(TestHelpers.BytesToHex(hash1), TestHelpers.BytesToHex(hash2));
        }

        [Test]
        public void TestHashLength()
        {
            // RIPEMD160 should always produce 160-bit (20-byte) hashes
            var testMessages = new[]
            {
                "",
                "short",
                "medium length message",
                "a very long message that is definitely longer than the typical input size and should still produce a 160-bit hash"
            };

            foreach (var message in testMessages)
            {
                // Act
                var messageBytes = Encoding.UTF8.GetBytes(message);
                var hash = RIPEMD160.ComputeHash(messageBytes);

                // Assert
                Assert.AreEqual(20, hash.Length, $"Hash should be 20 bytes for message: '{message}'");
            }
        }

        [Test]
        public void TestNullInputValidation()
        {
            // Test null input
            Assert.Throws<ArgumentNullException>(() => RIPEMD160.ComputeHash(null));
        }

        [Test]
        public void TestBinaryDataHashing()
        {
            // Test hashing of binary data (not just text)
            var binaryData = new byte[256];
            for (int i = 0; i < 256; i++)
            {
                binaryData[i] = (byte)i;
            }

            // Act
            var hash = RIPEMD160.ComputeHash(binaryData);

            // Assert
            Assert.AreEqual(20, hash.Length);
            Assert.IsNotNull(hash);

            // Should be different from text hashes
            var textHash = RIPEMD160.ComputeHash(Encoding.UTF8.GetBytes("binary data"));
            Assert.AreNotEqual(TestHelpers.BytesToHex(hash), TestHelpers.BytesToHex(textHash));
        }

        [Test]
        public void TestIncrementalHashing()
        {
            // Test that hashing data in chunks produces the same result as hashing all at once
            var fullMessage = "This is a test message that will be hashed incrementally";
            var fullMessageBytes = Encoding.UTF8.GetBytes(fullMessage);

            // Hash all at once
            var fullHash = RIPEMD160.ComputeHash(fullMessageBytes);

            // For now, just verify the full hash works
            // TODO: Implement incremental hashing if RIPEMD160 supports it
            Assert.AreEqual(20, fullHash.Length);
        }

        [Test]
        public void TestUnicodeHandling()
        {
            // Test Unicode strings
            var unicodeMessages = new[]
            {
                "Hello, 世界!", // Chinese
                "مرحبا بالعالم", // Arabic
                "Здравствуй, мир!", // Russian
                "🌍🌎🌏", // Emojis
                "Καλημέρα κόσμε", // Greek
            };

            foreach (var message in unicodeMessages)
            {
                // Act
                var messageBytes = Encoding.UTF8.GetBytes(message);
                var hash = RIPEMD160.ComputeHash(messageBytes);

                // Assert
                Assert.AreEqual(20, hash.Length, $"Hash should be 20 bytes for Unicode message: '{message}'");
                Assert.IsNotNull(hash);

                // Same message should produce same hash
                var hash2 = RIPEMD160.ComputeHash(messageBytes);
                TestHelpers.AssertBytesEqual(hash, hash2);
            }
        }

        #region Unity-Specific Tests

        [Test]
        [Performance]
        public void TestPerformance_RIPEMD160Hashing()
        {
            // Arrange
            const int iterations = 100;
            var testMessage = Encoding.UTF8.GetBytes("Performance test message for RIPEMD160 hashing");

            // Act & Assert
            var executionTime = TestHelpers.MeasureExecutionTime(() =>
            {
                for (int i = 0; i < iterations; i++)
                {
                    var hash = RIPEMD160.ComputeHash(testMessage);
                    GC.KeepAlive(hash);
                }
            });

            var avgTime = (float)executionTime / iterations;
            Assert.Less(avgTime, 10f, "RIPEMD160 hashing should average less than 10ms per operation");
            Debug.Log($"RIPEMD160 hashing average time: {avgTime}ms per operation ({iterations} iterations)");
        }

        [Test]
        public void TestMemoryUsage_RIPEMD160Hashing()
        {
            // Arrange
            var testMessage = Encoding.UTF8.GetBytes("Memory usage test for RIPEMD160");

            // Act
            var memoryUsage = TestHelpers.MeasureMemoryUsage(() =>
            {
                var hash = RIPEMD160.ComputeHash(testMessage);
                GC.KeepAlive(hash);
            });

            // Assert reasonable memory usage
            Assert.Less(Math.Abs(memoryUsage), 10 * 1024, "RIPEMD160 hashing should use less than 10KB additional memory");
            Debug.Log($"RIPEMD160 hashing memory usage: {memoryUsage} bytes");
        }

        [UnityTest]
        public IEnumerator TestUnityCoroutineCompatibility()
        {
            // Arrange
            var testMessage = Encoding.UTF8.GetBytes("Coroutine compatibility test");
            byte[] hashResult = null;

            // Act - Simulate async hashing in coroutine
            yield return new WaitForEndOfFrame();
            
            hashResult = RIPEMD160.ComputeHash(testMessage);

            // Assert
            Assert.IsNotNull(hashResult);
            Assert.AreEqual(20, hashResult.Length);
        }

        [Test]
        public void TestSerializationForUnityInspector()
        {
            // Create a serializable wrapper for Unity Inspector
            var testMessage = "Unity Inspector test";
            var messageBytes = Encoding.UTF8.GetBytes(testMessage);
            var hash = RIPEMD160.ComputeHash(messageBytes);
            
            var serializedData = new SerializableRipemd160Data
            {
                originalMessage = testMessage,
                messageLength = messageBytes.Length,
                hashHex = TestHelpers.BytesToHex(hash),
                hashLength = hash.Length
            };

            // Act
            var jsonString = JsonUtility.ToJson(serializedData, true);
            var deserializedData = JsonUtility.FromJson<SerializableRipemd160Data>(jsonString);

            // Assert
            Assert.AreEqual(testMessage, deserializedData.originalMessage);
            Assert.AreEqual(TestHelpers.BytesToHex(hash), deserializedData.hashHex);
            Assert.AreEqual(20, deserializedData.hashLength);
        }

        [Test]
        public void TestThreadSafety_RIPEMD160Hashing()
        {
            // Arrange
            const int threadCount = 4;
            const int operationsPerThread = 25;
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
                            var message = $"Thread {threadIndex} message {i}";
                            var messageBytes = Encoding.UTF8.GetBytes(message);
                            var hash = RIPEMD160.ComputeHash(messageBytes);
                            
                            if (hash == null || hash.Length != 20)
                            {
                                throw new Exception("RIPEMD160 hashing failed in thread");
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
        public void TestEdgeCases_SpecialDataPatterns()
        {
            // Test various edge cases
            var specialPatterns = new[]
            {
                new byte[1] { 0x00 }, // Single zero
                new byte[1] { 0xFF }, // Single max value
                new byte[512], // All zeros (512 bytes)
                Enumerable.Repeat((byte)0xFF, 512).ToArray(), // All ones (512 bytes)
                Enumerable.Range(0, 256).Select(i => (byte)i).ToArray(), // All byte values
            };

            foreach (var pattern in specialPatterns)
            {
                // Act
                var hash = RIPEMD160.ComputeHash(pattern);

                // Assert
                Assert.AreEqual(20, hash.Length, "Hash should always be 20 bytes");
                Assert.IsNotNull(hash);

                // Same pattern should produce same hash
                var hash2 = RIPEMD160.ComputeHash(pattern);
                TestHelpers.AssertBytesEqual(hash, hash2);
            }
        }

        [Test]
        public void TestHashDistribution()
        {
            // Test that hashes are well distributed (no obvious patterns)
            var hashes = new HashSet<string>();
            
            for (int i = 0; i < 100; i++)
            {
                var message = $"test message {i}";
                var messageBytes = Encoding.UTF8.GetBytes(message);
                var hash = RIPEMD160.ComputeHash(messageBytes);
                var hashHex = TestHelpers.BytesToHex(hash);
                
                // All hashes should be unique
                Assert.IsTrue(hashes.Add(hashHex), $"Duplicate hash found for message: {message}");
            }
            
            Assert.AreEqual(100, hashes.Count, "All 100 hashes should be unique");
        }

        #endregion

        #region Helper Classes

        [System.Serializable]
        private class SerializableRipemd160Data
        {
            public string originalMessage;
            public int messageLength;
            public string hashHex;
            public int hashLength;
        }

        #endregion
    }
}
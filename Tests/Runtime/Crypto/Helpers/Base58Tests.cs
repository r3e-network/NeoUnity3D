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
    /// Unity Test Framework implementation of Base58 encoding/decoding tests
    /// Converted from Swift Base58Tests.swift with Unity-specific enhancements
    /// </summary>
    [TestFixture]
    public class Base58Tests
    {
        /// <summary>
        /// Tuples of arbitrary strings that are mapped to valid Base58 encodings
        /// </summary>
        private readonly (string decoded, string encoded)[] _validStringDecodedToEncodedTuples = new[]
        {
            ("", ""),
            (" ", "Z"),
            ("-", "n"),
            ("0", "q"),
            ("1", "r"),
            ("-1", "4SU"),
            ("11", "4k8"),
            ("abc", "ZiCa"),
            ("1234598760", "3mJr7AoUXx2Wqd"),
            ("abcdefghijklmnopqrstuvwxyz", "3yxU3u1igY8WkgtjK92fbJQCd4BZiiT1v25f"),
            ("00000000000000000000000000000000000000000000000000000000000000",
             "3sN2THZeE9Eh9eYrwkvZqNstbHGvrxSAM7gXUXvyFQP8XvQLUqNCS27icwUeDT7ckHm4FUHM2mTVh1vbLmk7y")
        };

        /// <summary>
        /// Invalid Base58 strings that should fail decoding
        /// </summary>
        private readonly string[] _invalidStrings = new[]
        {
            "0",
            "O",
            "I",
            "l",
            "3mJr0",
            "O3yxU",
            "3sNI",
            "4kl8",
            "0OIl",
            "!@#$%^&*()-_=+~`"
        };

        [Test]
        public void TestBase58EncodingForValidStrings()
        {
            foreach (var (decoded, encoded) in _validStringDecodedToEncodedTuples)
            {
                // Arrange
                var bytes = Encoding.UTF8.GetBytes(decoded);

                // Act
                var result = Base58.Encode(bytes);

                // Assert
                Assert.AreEqual(encoded, result, $"Failed for input: '{decoded}'");
            }
        }

        [Test]
        public void TestBase58DecodingForValidStrings()
        {
            foreach (var (decoded, encoded) in _validStringDecodedToEncodedTuples)
            {
                // Act
                var bytes = Base58.Decode(encoded);
                var result = Encoding.UTF8.GetString(bytes);

                // Assert
                Assert.AreEqual(decoded, result, $"Failed for input: '{encoded}'");
            }
        }

        [Test]
        public void TestBase58DecodingForInvalidStrings()
        {
            foreach (var invalidString in _invalidStrings)
            {
                // Act & Assert
                Assert.Throws<FormatException>(() => Base58.Decode(invalidString),
                    $"Should throw for invalid string: '{invalidString}'");
            }
        }

        [Test]
        public void TestBase58CheckEncoding()
        {
            // Arrange
            var inputData = new byte[]
            {
                6, 161, 159, 136, 34, 110, 33, 238, 14, 79, 14, 218, 133, 13, 109, 40, 194, 236, 153, 44, 61, 157, 254
            };
            var expectedOutput = "tz1Y3qqTg9HdrzZGbEjiCPmwuZ7fWVxpPtRw";

            // Act
            var actualOutput = Base58.EncodeCheck(inputData);

            // Assert
            Assert.AreEqual(expectedOutput, actualOutput);
        }

        [Test]
        public void TestBase58CheckDecoding()
        {
            // Arrange
            var inputString = "tz1Y3qqTg9HdrzZGbEjiCPmwuZ7fWVxpPtRw";
            var expectedOutputData = new byte[]
            {
                6, 161, 159, 136, 34, 110, 33, 238, 14, 79, 14, 218, 133, 13, 109, 40, 194, 236, 153, 44, 61, 157, 254
            };

            // Act
            var actualOutput = Base58.DecodeCheck(inputString);

            // Assert
            TestHelpers.AssertBytesEqual(expectedOutputData, actualOutput);
        }

        [Test]
        public void TestBase58CheckDecodingWithInvalidCharacters()
        {
            // Act & Assert
            Assert.Throws<FormatException>(() => Base58.DecodeCheck("0oO1lL"));
        }

        [Test]
        public void TestBase58CheckDecodingWithInvalidChecksum()
        {
            // Act & Assert
            Assert.Throws<FormatException>(() => Base58.DecodeCheck("tz1Y3qqTg9HdrzZGbEjiCPmwuZ7fWVxpPtrW"));
        }

        [Test]
        public void TestBase58EncodingRoundTrip()
        {
            // Test various byte arrays for round-trip encoding/decoding
            var testCases = new[]
            {
                new byte[0], // Empty
                new byte[] { 0x00 }, // Single zero
                new byte[] { 0xFF }, // Single max value
                new byte[] { 0x00, 0x00, 0x00 }, // Multiple zeros
                TestHelpers.GenerateRandomBytes(32), // Random 32 bytes
                TestHelpers.GenerateRandomBytes(64), // Random 64 bytes
                TestHelpers.HexToBytes("deadbeef"), // Known hex pattern
            };

            foreach (var testCase in testCases)
            {
                // Act
                var encoded = Base58.Encode(testCase);
                var decoded = Base58.Decode(encoded);

                // Assert
                TestHelpers.AssertBytesEqual(testCase, decoded);
            }
        }

        [Test]
        public void TestBase58CheckEncodingRoundTrip()
        {
            // Test various byte arrays for round-trip Base58Check encoding/decoding
            var testCases = new[]
            {
                new byte[0], // Empty
                new byte[] { 0x00 }, // Single zero
                new byte[] { 0xFF }, // Single max value
                TestHelpers.GenerateRandomBytes(21), // Address length
                TestHelpers.GenerateRandomBytes(32), // Hash length
                TestHelpers.GenerateRandomBytes(65), // Public key length
            };

            foreach (var testCase in testCases)
            {
                // Act
                var encoded = Base58.EncodeCheck(testCase);
                var decoded = Base58.DecodeCheck(encoded);

                // Assert
                TestHelpers.AssertBytesEqual(testCase, decoded);
            }
        }

        [Test]
        public void TestLeadingZeroHandling()
        {
            // Base58 should properly handle leading zeros
            var testCases = new[]
            {
                new byte[] { 0x00, 0x01 },
                new byte[] { 0x00, 0x00, 0x01 },
                new byte[] { 0x00, 0x00, 0x00, 0x01 },
                new byte[] { 0x00, 0x00, 0x00, 0x00, 0x01 },
            };

            foreach (var testCase in testCases)
            {
                // Act
                var encoded = Base58.Encode(testCase);
                var decoded = Base58.Decode(encoded);

                // Assert
                TestHelpers.AssertBytesEqual(testCase, decoded);
                
                // Leading zeros should be preserved as '1' characters
                var leadingZeros = 0;
                foreach (var b in testCase)
                {
                    if (b == 0x00) leadingZeros++;
                    else break;
                }
                
                var leadingOnes = 0;
                foreach (var c in encoded)
                {
                    if (c == '1') leadingOnes++;
                    else break;
                }
                
                Assert.AreEqual(leadingZeros, leadingOnes, "Leading zeros should be preserved as '1' characters");
            }
        }

        [Test]
        public void TestEmptyInputHandling()
        {
            // Empty input should produce empty output
            Assert.AreEqual("", Base58.Encode(new byte[0]));
            TestHelpers.AssertBytesEqual(new byte[0], Base58.Decode(""));
        }

        [Test]
        public void TestNullInputValidation()
        {
            // Test null inputs
            Assert.Throws<ArgumentNullException>(() => Base58.Encode(null));
            Assert.Throws<ArgumentNullException>(() => Base58.Decode(null));
            Assert.Throws<ArgumentNullException>(() => Base58.EncodeCheck(null));
            Assert.Throws<ArgumentNullException>(() => Base58.DecodeCheck(null));
        }

        [Test]
        public void TestAllBase58Characters()
        {
            // Verify the Base58 alphabet is used correctly
            var base58Alphabet = "123456789ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz";
            
            // Test that encoded output only contains valid Base58 characters
            var testData = TestHelpers.GenerateRandomBytes(100);
            var encoded = Base58.Encode(testData);

            foreach (var c in encoded)
            {
                Assert.IsTrue(base58Alphabet.Contains(c), $"Invalid character '{c}' in Base58 output");
            }
        }

        [Test]
        public void TestChecksumValidation()
        {
            // Create a valid Base58Check string and corrupt it
            var originalData = TestHelpers.GenerateRandomBytes(20);
            var validEncoded = Base58.EncodeCheck(originalData);

            // Corrupt a character in the middle
            var corrupted = validEncoded.ToCharArray();
            corrupted[corrupted.Length / 2] = corrupted[corrupted.Length / 2] == '1' ? '2' : '1';
            var corruptedString = new string(corrupted);

            // Should fail checksum validation
            Assert.Throws<FormatException>(() => Base58.DecodeCheck(corruptedString));
        }

        [Test]
        public void TestLargeDataHandling()
        {
            // Test with large data arrays
            var largeData = TestHelpers.GenerateRandomBytes(1024); // 1KB
            
            // Act
            var encoded = Base58.Encode(largeData);
            var decoded = Base58.Decode(encoded);

            // Assert
            TestHelpers.AssertBytesEqual(largeData, decoded);
            Assert.IsTrue(encoded.Length > largeData.Length, "Encoded data should be larger than original");
        }

        #region Unity-Specific Tests

        [Test]
        [Performance]
        public void TestPerformance_Base58Operations()
        {
            // Arrange
            const int iterations = 100;
            var testData = TestHelpers.GenerateRandomBytes(32);

            // Test encoding performance
            var encodingTime = TestHelpers.MeasureExecutionTime(() =>
            {
                for (int i = 0; i < iterations; i++)
                {
                    var encoded = Base58.Encode(testData);
                    GC.KeepAlive(encoded);
                }
            });

            // Test decoding performance
            var encoded = Base58.Encode(testData);
            var decodingTime = TestHelpers.MeasureExecutionTime(() =>
            {
                for (int i = 0; i < iterations; i++)
                {
                    var decoded = Base58.Decode(encoded);
                    GC.KeepAlive(decoded);
                }
            });

            var avgEncodingTime = (float)encodingTime / iterations;
            var avgDecodingTime = (float)decodingTime / iterations;

            // Assert reasonable performance
            Assert.Less(avgEncodingTime, 10f, "Base58 encoding should average less than 10ms per operation");
            Assert.Less(avgDecodingTime, 10f, "Base58 decoding should average less than 10ms per operation");
            
            Debug.Log($"Base58 encoding average time: {avgEncodingTime}ms per operation ({iterations} iterations)");
            Debug.Log($"Base58 decoding average time: {avgDecodingTime}ms per operation ({iterations} iterations)");
        }

        [Test]
        public void TestMemoryUsage_Base58Operations()
        {
            // Arrange
            var testData = TestHelpers.GenerateRandomBytes(100);

            // Test encoding memory usage
            var encodingMemory = TestHelpers.MeasureMemoryUsage(() =>
            {
                var encoded = Base58.Encode(testData);
                GC.KeepAlive(encoded);
            });

            // Test decoding memory usage
            var encoded = Base58.Encode(testData);
            var decodingMemory = TestHelpers.MeasureMemoryUsage(() =>
            {
                var decoded = Base58.Decode(encoded);
                GC.KeepAlive(decoded);
            });

            // Assert reasonable memory usage
            Assert.Less(Math.Abs(encodingMemory), 10 * 1024, "Base58 encoding should use less than 10KB additional memory");
            Assert.Less(Math.Abs(decodingMemory), 10 * 1024, "Base58 decoding should use less than 10KB additional memory");
            
            Debug.Log($"Base58 encoding memory usage: {encodingMemory} bytes");
            Debug.Log($"Base58 decoding memory usage: {decodingMemory} bytes");
        }

        [UnityTest]
        public IEnumerator TestUnityCoroutineCompatibility()
        {
            // Arrange
            var testData = TestHelpers.GenerateRandomBytes(50);
            string encodedResult = null;
            byte[] decodedResult = null;

            // Act - Simulate async Base58 operations in coroutine
            yield return new WaitForEndOfFrame();
            
            encodedResult = Base58.Encode(testData);
            
            yield return new WaitForEndOfFrame();
            
            decodedResult = Base58.Decode(encodedResult);

            // Assert
            Assert.IsNotNull(encodedResult);
            Assert.IsNotNull(decodedResult);
            TestHelpers.AssertBytesEqual(testData, decodedResult);
        }

        [Test]
        public void TestSerializationForUnityInspector()
        {
            // Create a serializable wrapper for Unity Inspector
            var testData = TestHelpers.GenerateRandomBytes(20);
            var encoded = Base58.Encode(testData);
            
            var serializedData = new SerializableBase58Data
            {
                originalDataHex = TestHelpers.BytesToHex(testData),
                base58Encoded = encoded,
                dataLength = testData.Length,
                encodedLength = encoded.Length
            };

            // Act
            var jsonString = JsonUtility.ToJson(serializedData, true);
            var deserializedData = JsonUtility.FromJson<SerializableBase58Data>(jsonString);

            // Assert
            Assert.AreEqual(TestHelpers.BytesToHex(testData), deserializedData.originalDataHex);
            Assert.AreEqual(encoded, deserializedData.base58Encoded);
            Assert.AreEqual(testData.Length, deserializedData.dataLength);
        }

        [Test]
        public void TestThreadSafety_Base58Operations()
        {
            // Arrange
            const int threadCount = 4;
            const int operationsPerThread = 50;
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
                            var testData = TestHelpers.GenerateRandomBytes(32);
                            var encoded = Base58.Encode(testData);
                            var decoded = Base58.Decode(encoded);
                            
                            if (!TestHelpers.BytesToHex(decoded).Equals(TestHelpers.BytesToHex(testData)))
                            {
                                throw new Exception("Base58 round-trip failed in thread");
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
        public void TestEdgeCases_SpecialBytePatterns()
        {
            // Test special byte patterns
            var specialPatterns = new[]
            {
                new byte[] { 0x00 }, // All zeros
                new byte[] { 0xFF }, // All ones
                new byte[] { 0x80 }, // High bit set
                new byte[] { 0x01, 0x00, 0x00, 0x00 }, // Little-endian 1
                new byte[] { 0x00, 0x00, 0x00, 0x01 }, // Big-endian 1
                Enumerable.Range(0, 256).Select(i => (byte)i).ToArray(), // All byte values
            };

            foreach (var pattern in specialPatterns)
            {
                // Act
                var encoded = Base58.Encode(pattern);
                var decoded = Base58.Decode(encoded);

                // Assert
                TestHelpers.AssertBytesEqual(pattern, decoded);
            }
        }

        #endregion

        #region Helper Classes

        [System.Serializable]
        private class SerializableBase58Data
        {
            public string originalDataHex;
            public string base58Encoded;
            public int dataLength;
            public int encodedLength;
        }

        #endregion
    }
}
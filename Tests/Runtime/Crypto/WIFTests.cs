using System;
using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Neo.Unity.SDK;
using Neo.Unity.SDK.Crypto;
using Neo.Unity.SDK.Tests.Helpers;

namespace Neo.Unity.SDK.Tests.Crypto
{
    /// <summary>
    /// Unity Test Framework implementation of WIF (Wallet Import Format) tests
    /// Converted from Swift WIFTests.swift with Unity-specific enhancements
    /// </summary>
    [TestFixture]
    public class WIFTests
    {
        private const string VALID_WIF = "L25kgAQJXNHnhc7Sx9bomxxwVSMsZdkaNQ3m2VfHrnLzKWMLP13A";
        private const string PRIVATE_KEY_HEX = "9117f4bf9be717c9a90994326897f4243503accd06712162267e77f18b49c3a3";

        [Test]
        public void TestValidWifToPrivateKey()
        {
            // Act
            var privateKeyFromWIF = WIF.PrivateKeyFromWIF(VALID_WIF);

            // Assert
            var expectedPrivateKey = TestHelpers.HexToBytes(PRIVATE_KEY_HEX);
            TestHelpers.AssertBytesEqual(expectedPrivateKey, privateKeyFromWIF);
        }

        [Test]
        public void TestWronglySizedWifs()
        {
            // Arrange
            var tooLarge = "L25kgAQJXNHnhc7Sx9bomxxwVSMsZdkaNQ3m2VfHrnLzKWMLP13Ahc7S";
            var tooSmall = "L25kgAQJXNHnhc7Sx9bomxxwVSMsZdkaNQ3m2VfHrnLzKWML";

            // Act & Assert
            AssertThrowsWrongWifFormat(tooLarge);
            AssertThrowsWrongWifFormat(tooSmall);
        }

        [Test]
        public void TestWrongFirstByteWif()
        {
            // Arrange
            var base58Bytes = Base58.Decode(VALID_WIF);
            base58Bytes[0] = 0x81; // Change first byte
            var wrongFirstByteWif = Base58.Encode(base58Bytes);

            // Act & Assert
            AssertThrowsWrongWifFormat(wrongFirstByteWif);
        }

        [Test]
        public void TestWrongByte33Wif()
        {
            // Arrange
            var base58Bytes = Base58.Decode(VALID_WIF);
            base58Bytes[33] = 0x00; // Change byte 33 (compression flag)
            var wrongByte33Wif = Base58.Encode(base58Bytes);

            // Act & Assert
            AssertThrowsWrongWifFormat(wrongByte33Wif);
        }

        [Test]
        public void TestValidPrivateKeyToWif()
        {
            // Arrange
            var privateKeyBytes = TestHelpers.HexToBytes(PRIVATE_KEY_HEX);

            // Act
            var wifFromPrivateKey = WIF.WifFromPrivateKey(privateKeyBytes);

            // Assert
            Assert.AreEqual(VALID_WIF, wifFromPrivateKey);
        }

        [Test]
        public void TestWronglySizedPrivateKey()
        {
            // Arrange - Missing one character (31 bytes instead of 32)
            var wronglySizedPrivateKey = "9117f4bf9be717c9a90994326897f4243503accd06712162267e77f18b49c3";
            var wronglySizedBytes = TestHelpers.HexToBytes(wronglySizedPrivateKey);

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => WIF.WifFromPrivateKey(wronglySizedBytes));
            Assert.That(exception.Message, Does.Contain("32 bytes"));
        }

        [Test]
        public void TestWifRoundTrip()
        {
            // Arrange
            var originalPrivateKey = TestHelpers.GenerateRandomBytes(32);

            // Act
            var wif = WIF.WifFromPrivateKey(originalPrivateKey);
            var recoveredPrivateKey = WIF.PrivateKeyFromWIF(wif);

            // Assert
            TestHelpers.AssertBytesEqual(originalPrivateKey, recoveredPrivateKey);
        }

        [Test]
        public void TestMultipleValidWifs()
        {
            // Test known valid WIF keys with their corresponding private keys
            var testCases = new[]
            {
                new { Wif = "L25kgAQJXNHnhc7Sx9bomxxwVSMsZdkaNQ3m2VfHrnLzKWMLP13A", PrivateKey = "9117f4bf9be717c9a90994326897f4243503accd06712162267e77f18b49c3a3" },
                new { Wif = "L1eV34wPoj9weqhGijdDLtVQzUpWGHszXXpdU9dPuh2nRFFzFa7E", PrivateKey = "84180ac9d6eb6fba207ea4ef9d2200102d1ebeb4b9c07e2c6a738a42742e27a5" },
                new { Wif = "L3cNMQUSrvUrHx1MzacwHiUeCWzqK2MLt5fPvJj9mz6L2rzYZpok", PrivateKey = "c7134d6fd8e73d819e82755c64c93788d8db0961929e025a53363c4cc02a6962" }
            };

            foreach (var testCase in testCases)
            {
                // Test WIF to private key
                var privateKeyFromWif = WIF.PrivateKeyFromWIF(testCase.Wif);
                var expectedPrivateKey = TestHelpers.HexToBytes(testCase.PrivateKey);
                TestHelpers.AssertBytesEqual(expectedPrivateKey, privateKeyFromWif);

                // Test private key to WIF
                var wifFromPrivateKey = WIF.WifFromPrivateKey(expectedPrivateKey);
                Assert.AreEqual(testCase.Wif, wifFromPrivateKey);
            }
        }

        [Test]
        public void TestWifFormat()
        {
            // All valid WIFs should start with 'L' or 'K' for compressed keys
            var privateKeyBytes = TestHelpers.GenerateRandomBytes(32);
            var wif = WIF.WifFromPrivateKey(privateKeyBytes);

            Assert.IsTrue(wif.StartsWith("L") || wif.StartsWith("K"), 
                "WIF should start with 'L' or 'K' for compressed keys");
            Assert.AreEqual(52, wif.Length, "WIF should be 52 characters long");
        }

        [Test]
        public void TestInvalidWifFormats()
        {
            var invalidWifs = new[]
            {
                "", // Empty string
                "invalid", // Too short
                "5HueCGU8rMjxEXxiPuD5BDku4MkFqeZyd4dZ1jvhTVqvbTLvyTJ", // Uncompressed WIF (wrong format)
                "L25kgAQJXNHnhc7Sx9bomxxwVSMsZdkaNQ3m2VfHrnLzKWMLP13", // Missing checksum
                "M25kgAQJXNHnhc7Sx9bomxxwVSMsZdkaNQ3m2VfHrnLzKWMLP13A", // Wrong first character
                "L25kgAQJXNHnhc7Sx9bomxxwVSMsZdkaNQ3m2VfHrnLzKWMLP13B" // Invalid checksum
            };

            foreach (var invalidWif in invalidWifs)
            {
                AssertThrowsWrongWifFormat(invalidWif);
            }
        }

        [Test]
        public void TestNullInputValidation()
        {
            // Test null WIF
            Assert.Throws<ArgumentNullException>(() => WIF.PrivateKeyFromWIF(null));

            // Test null private key
            Assert.Throws<ArgumentNullException>(() => WIF.WifFromPrivateKey(null));
        }

        [Test]
        public void TestEmptyInputValidation()
        {
            // Test empty WIF
            Assert.Throws<ArgumentException>(() => WIF.PrivateKeyFromWIF(""));

            // Test empty private key array
            Assert.Throws<ArgumentException>(() => WIF.WifFromPrivateKey(new byte[0]));
        }

        [Test]
        public void TestChecksumValidation()
        {
            // Create a WIF with intentionally wrong checksum
            var base58Bytes = Base58.Decode(VALID_WIF);
            base58Bytes[base58Bytes.Length - 1] ^= 0x01; // Corrupt checksum
            var corruptedWif = Base58.Encode(base58Bytes);

            // Act & Assert
            AssertThrowsWrongWifFormat(corruptedWif);
        }

        [Test]
        public void TestCompressionFlag()
        {
            // All WIFs generated should have compression flag (0x01)
            var privateKeyBytes = TestHelpers.GenerateRandomBytes(32);
            var wif = WIF.WifFromPrivateKey(privateKeyBytes);
            
            var decoded = Base58.Decode(wif);
            Assert.AreEqual(0x01, decoded[33], "WIF should have compression flag set");
        }

        [Test]
        public void TestNetworkByte()
        {
            // WIF for Neo should start with 0x80
            var privateKeyBytes = TestHelpers.GenerateRandomBytes(32);
            var wif = WIF.WifFromPrivateKey(privateKeyBytes);
            
            var decoded = Base58.Decode(wif);
            Assert.AreEqual(0x80, decoded[0], "WIF should start with network byte 0x80");
        }

        [Test]
        public void TestDeterministicGeneration()
        {
            // Same private key should always generate same WIF
            var privateKeyBytes = TestHelpers.HexToBytes(PRIVATE_KEY_HEX);
            
            var wif1 = WIF.WifFromPrivateKey(privateKeyBytes);
            var wif2 = WIF.WifFromPrivateKey(privateKeyBytes);
            
            Assert.AreEqual(wif1, wif2, "Same private key should generate same WIF");
        }

        private void AssertThrowsWrongWifFormat(string input)
        {
            var exception = Assert.Throws<ArgumentException>(() => WIF.PrivateKeyFromWIF(input));
            Assert.That(exception.Message, Does.Contain("WIF format").Or.Contain("format").IgnoreCase);
        }

        #region Unity-Specific Tests

        [Test]
        [Performance]
        public void TestPerformance_WifOperations()
        {
            // Arrange
            const int iterations = 1000;
            var privateKeyBytes = TestHelpers.GenerateRandomBytes(32);
            var wif = WIF.WifFromPrivateKey(privateKeyBytes);

            // Test WIF generation performance
            var wifGenerationTime = TestHelpers.MeasureExecutionTime(() =>
            {
                for (int i = 0; i < iterations; i++)
                {
                    var testWif = WIF.WifFromPrivateKey(privateKeyBytes);
                    GC.KeepAlive(testWif);
                }
            });

            // Test WIF parsing performance
            var wifParsingTime = TestHelpers.MeasureExecutionTime(() =>
            {
                for (int i = 0; i < iterations; i++)
                {
                    var parsedKey = WIF.PrivateKeyFromWIF(wif);
                    GC.KeepAlive(parsedKey);
                }
            });

            var avgWifGenTime = (float)wifGenerationTime / iterations;
            var avgWifParseTime = (float)wifParsingTime / iterations;

            // Assert reasonable performance
            Assert.Less(avgWifGenTime, 5f, "WIF generation should average less than 5ms per operation");
            Assert.Less(avgWifParseTime, 5f, "WIF parsing should average less than 5ms per operation");
            
            Debug.Log($"WIF generation average time: {avgWifGenTime}ms per operation ({iterations} iterations)");
            Debug.Log($"WIF parsing average time: {avgWifParseTime}ms per operation ({iterations} iterations)");
        }

        [Test]
        public void TestMemoryUsage_WifOperations()
        {
            // Arrange
            var privateKeyBytes = TestHelpers.GenerateRandomBytes(32);

            // Test WIF generation memory usage
            var wifGenMemory = TestHelpers.MeasureMemoryUsage(() =>
            {
                var wif = WIF.WifFromPrivateKey(privateKeyBytes);
                GC.KeepAlive(wif);
            });

            // Test WIF parsing memory usage
            var wif = WIF.WifFromPrivateKey(privateKeyBytes);
            var wifParseMemory = TestHelpers.MeasureMemoryUsage(() =>
            {
                var parsed = WIF.PrivateKeyFromWIF(wif);
                GC.KeepAlive(parsed);
            });

            // Assert reasonable memory usage
            Assert.Less(Math.Abs(wifGenMemory), 5 * 1024, "WIF generation should use less than 5KB additional memory");
            Assert.Less(Math.Abs(wifParseMemory), 5 * 1024, "WIF parsing should use less than 5KB additional memory");
            
            Debug.Log($"WIF generation memory usage: {wifGenMemory} bytes");
            Debug.Log($"WIF parsing memory usage: {wifParseMemory} bytes");
        }

        [UnityTest]
        public IEnumerator TestUnityCoroutineCompatibility()
        {
            // Arrange
            var privateKeyBytes = TestHelpers.GenerateRandomBytes(32);
            string wifResult = null;
            byte[] parsedResult = null;

            // Act - Simulate async WIF operations in coroutine
            yield return new WaitForEndOfFrame();
            
            wifResult = WIF.WifFromPrivateKey(privateKeyBytes);
            
            yield return new WaitForEndOfFrame();
            
            parsedResult = WIF.PrivateKeyFromWIF(wifResult);

            // Assert
            Assert.IsNotNull(wifResult);
            Assert.IsNotNull(parsedResult);
            TestHelpers.AssertBytesEqual(privateKeyBytes, parsedResult);
        }

        [Test]
        public void TestSerializationForUnityInspector()
        {
            // Arrange
            var privateKeyBytes = TestHelpers.GenerateRandomBytes(32);
            var wif = WIF.WifFromPrivateKey(privateKeyBytes);
            
            var serializedData = new SerializableWifData
            {
                wif = wif,
                privateKeyHex = TestHelpers.BytesToHex(privateKeyBytes),
                isValid = true,
                networkByte = 0x80,
                compressionFlag = 0x01
            };

            // Act
            var jsonString = JsonUtility.ToJson(serializedData, true);
            var deserializedData = JsonUtility.FromJson<SerializableWifData>(jsonString);

            // Assert
            Assert.AreEqual(wif, deserializedData.wif);
            Assert.AreEqual(TestHelpers.BytesToHex(privateKeyBytes), deserializedData.privateKeyHex);
            Assert.IsTrue(deserializedData.isValid);
        }

        [Test]
        public void TestThreadSafety_WifOperations()
        {
            // Arrange
            const int threadCount = 4;
            const int operationsPerThread = 100;
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
                            var privateKey = TestHelpers.GenerateRandomBytes(32);
                            var wif = WIF.WifFromPrivateKey(privateKey);
                            var parsed = WIF.PrivateKeyFromWIF(wif);
                            
                            if (!TestHelpers.BytesToHex(parsed).Equals(TestHelpers.BytesToHex(privateKey)))
                            {
                                throw new Exception("WIF round-trip failed in thread");
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
        public void TestEdgeCases_ExtremePrivateKeys()
        {
            // Test minimum value private key (all zeros except last byte)
            var minKey = new byte[32];
            minKey[31] = 0x01;

            // Test maximum value private key (all 0xFF)
            var maxKey = new byte[32];
            for (int i = 0; i < 32; i++) maxKey[i] = 0xFF;

            // Test both keys
            foreach (var testKey in new[] { minKey, maxKey })
            {
                var wif = WIF.WifFromPrivateKey(testKey);
                var parsed = WIF.PrivateKeyFromWIF(wif);
                TestHelpers.AssertBytesEqual(testKey, parsed);
            }
        }

        #endregion

        #region Helper Classes

        [System.Serializable]
        private class SerializableWifData
        {
            public string wif;
            public string privateKeyHex;
            public bool isValid;
            public byte networkByte;
            public byte compressionFlag;
        }

        #endregion
    }
}
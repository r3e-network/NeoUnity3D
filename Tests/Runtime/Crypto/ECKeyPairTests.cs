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
    /// Unity Test Framework implementation of ECKeyPair cryptographic tests
    /// Converted from Swift ECKeyPairTests.swift with Unity-specific enhancements
    /// </summary>
    [TestFixture]
    public class ECKeyPairTests
    {
        private const string ENCODED_POINT = "03b4af8d061b6b320cce6c63bc4ec7894dce107bfc5f5ef5c68a93b4ad1e136816";
        private const string UNCOMPRESSED_POINT = "04b4af8d061b6b320cce6c63bc4ec7894dce107bfc5f5ef5c68a93b4ad1e1368165f4f7fb1c5862465543c06dd5a2aa414f6583f92a5cc3e1d4259df79bf6839c9";
        private const string PRIVATE_KEY_HEX = "c7134d6fd8e73d819e82755c64c93788d8db0961929e025a53363c4cc02a6962";
        private const string EXPECTED_WIF = "L3tgppXLgdaeqSGSFw1Go3skBiy8vQAM7YMXvTHsKQtE16PBncSU";

        [Test]
        public void TestNewPublicKeyFromPoint()
        {
            // Act
            var publicKey = new ECPublicKey(ENCODED_POINT);

            // Assert
            var encodedBytes = publicKey.GetEncoded(compressed: true);
            var expectedBytes = TestHelpers.HexToBytes(ENCODED_POINT);
            
            TestHelpers.AssertBytesEqual(expectedBytes, encodedBytes);
            Assert.AreEqual(ENCODED_POINT, publicKey.GetEncodedCompressedHex());
        }

        [Test]
        public void TestNewPublicKeyFromUncompressedPoint()
        {
            // Act
            var publicKey = new ECPublicKey(UNCOMPRESSED_POINT);

            // Assert
            Assert.AreEqual(ENCODED_POINT, publicKey.GetEncodedCompressedHex());
        }

        [Test]
        public void TestNewPublicKeyFromStringWithInvalidSize()
        {
            // Arrange
            var tooSmall = ENCODED_POINT.Substring(0, ENCODED_POINT.Length - 2);

            // Act & Assert
            Assert.Throws<ArgumentException>(() => new ECPublicKey(tooSmall));
        }

        [Test]
        public void TestNewPublicKeyFromPointWithHexPrefix()
        {
            // Arrange
            var prefixed = "0x" + ENCODED_POINT;

            // Act
            var publicKey = new ECPublicKey(prefixed);

            // Assert
            Assert.AreEqual(ENCODED_POINT, publicKey.GetEncodedCompressedHex());
        }

        [Test]
        public void TestSerializePublicKey()
        {
            // Arrange
            var publicKey = new ECPublicKey(ENCODED_POINT);

            // Act
            var serialized = publicKey.ToArray();

            // Assert
            var expected = TestHelpers.HexToBytes(ENCODED_POINT);
            TestHelpers.AssertBytesEqual(expected, serialized);
        }

        [Test]
        public void TestDeserializePublicKey()
        {
            // Arrange
            var data = TestHelpers.HexToBytes("036b17d1f2e12c4247f8bce6e563a440f277037d812deb33a0f4a13945d898c296");

            // Act
            var publicKey = ECPublicKey.FromBytes(data);

            // Assert
            Assert.AreEqual(NeoConstants.SECP256R1_DOMAIN.G, publicKey.ECPoint);
        }

        [Test]
        public void TestPublicKeySize()
        {
            // Arrange
            var key = new ECPublicKey("036b17d1f2e12c4247f8bce6e563a440f277037d812deb33a0f4a13945d898c296");

            // Act & Assert
            Assert.AreEqual(33, key.Size);
        }

        [Test]
        public void TestPublicKeyWif()
        {
            // Arrange
            var privateKeyBytes = TestHelpers.HexToBytes(PRIVATE_KEY_HEX);
            var keyPair = ECKeyPair.CreateFromPrivateKey(privateKeyBytes);

            // Act
            var wif = keyPair.ExportAsWif();

            // Assert
            Assert.AreEqual(EXPECTED_WIF, wif);
        }

        [Test]
        public void TestPublicKeyComparable()
        {
            // Arrange
            var encodedKey2 = "036b17d1f2e12c4247f8bce6e563a440f277037d812deb33a0f4a13945d898c296";
            
            var key1 = new ECPublicKey(ENCODED_POINT);
            var key2 = new ECPublicKey(encodedKey2);
            var key1Uncompressed = new ECPublicKey(UNCOMPRESSED_POINT);

            // Act & Assert
            Assert.IsTrue(key1.CompareTo(key2) > 0);
            Assert.AreEqual(0, key1.CompareTo(key1Uncompressed));
            Assert.IsFalse(key1.CompareTo(key1Uncompressed) < 0);
            Assert.IsFalse(key1.CompareTo(key1Uncompressed) > 0);
        }

        [Test]
        public void TestKeyPairCreation()
        {
            // Act
            var keyPair = ECKeyPair.CreateEcKeyPair();

            // Assert
            Assert.IsNotNull(keyPair);
            Assert.IsNotNull(keyPair.PrivateKey);
            Assert.IsNotNull(keyPair.PublicKey);
            Assert.AreEqual(32, keyPair.PrivateKey.Length);
            Assert.AreEqual(33, keyPair.PublicKey.Size);
        }

        [Test]
        public void TestKeyPairFromPrivateKey()
        {
            // Arrange
            var privateKeyBytes = TestHelpers.HexToBytes(PRIVATE_KEY_HEX);

            // Act
            var keyPair = ECKeyPair.CreateFromPrivateKey(privateKeyBytes);

            // Assert
            Assert.IsNotNull(keyPair);
            TestHelpers.AssertBytesEqual(privateKeyBytes, keyPair.PrivateKey);
            Assert.IsNotNull(keyPair.PublicKey);
        }

        [Test]
        public void TestSignAndVerify()
        {
            // Arrange
            var keyPair = ECKeyPair.CreateEcKeyPair();
            var message = TestHelpers.HexToBytes("48656c6c6f20576f726c64"); // "Hello World"

            // Act
            var signature = keyPair.Sign(message);
            var isValid = keyPair.PublicKey.Verify(message, signature);

            // Assert
            Assert.IsNotNull(signature);
            Assert.IsTrue(isValid);
        }

        [Test]
        public void TestSignWithDifferentKeyVerifyFails()
        {
            // Arrange
            var keyPair1 = ECKeyPair.CreateEcKeyPair();
            var keyPair2 = ECKeyPair.CreateEcKeyPair();
            var message = TestHelpers.HexToBytes("48656c6c6f20576f726c64"); // "Hello World"

            // Act
            var signature = keyPair1.Sign(message);
            var isValid = keyPair2.PublicKey.Verify(message, signature);

            // Assert
            Assert.IsFalse(isValid);
        }

        [Test]
        public void TestInvalidSignatureVerification()
        {
            // Arrange
            var keyPair = ECKeyPair.CreateEcKeyPair();
            var message = TestHelpers.HexToBytes("48656c6c6f20576f726c64"); // "Hello World"
            var invalidSignature = TestHelpers.GenerateRandomBytes(64);

            // Act
            var isValid = keyPair.PublicKey.Verify(message, invalidSignature);

            // Assert
            Assert.IsFalse(isValid);
        }

        [Test]
        public void TestKeyPairEquality()
        {
            // Arrange
            var privateKeyBytes = TestHelpers.HexToBytes(PRIVATE_KEY_HEX);
            var keyPair1 = ECKeyPair.CreateFromPrivateKey(privateKeyBytes);
            var keyPair2 = ECKeyPair.CreateFromPrivateKey(privateKeyBytes);

            // Act & Assert
            Assert.AreEqual(keyPair1.PublicKey, keyPair2.PublicKey);
            TestHelpers.AssertBytesEqual(keyPair1.PrivateKey, keyPair2.PrivateKey);
        }

        [Test]
        public void TestPublicKeyEquality()
        {
            // Arrange
            var publicKey1 = new ECPublicKey(ENCODED_POINT);
            var publicKey2 = new ECPublicKey(ENCODED_POINT);
            var publicKey3 = new ECPublicKey(UNCOMPRESSED_POINT);

            // Act & Assert
            Assert.AreEqual(publicKey1, publicKey2);
            Assert.AreEqual(publicKey1, publicKey3); // Compressed and uncompressed should be equal
            Assert.AreEqual(publicKey1.GetHashCode(), publicKey2.GetHashCode());
        }

        #region Unity-Specific Tests

        [Test]
        [Performance]
        public void TestPerformance_KeyGeneration()
        {
            const int iterations = 100;

            // Act & Assert
            var executionTime = TestHelpers.MeasureExecutionTime(() =>
            {
                for (int i = 0; i < iterations; i++)
                {
                    var keyPair = ECKeyPair.CreateEcKeyPair();
                    GC.KeepAlive(keyPair);
                }
            });

            var avgTimePerKey = (float)executionTime / iterations;
            Assert.Less(avgTimePerKey, 50, "Key generation should average less than 50ms per key");
            Debug.Log($"Key generation average time: {avgTimePerKey}ms per key ({iterations} iterations)");
        }

        [Test]
        [Performance]
        public void TestPerformance_SignAndVerify()
        {
            // Arrange
            var keyPair = ECKeyPair.CreateEcKeyPair();
            var message = TestHelpers.HexToBytes("48656c6c6f20576f726c64");
            const int iterations = 50;

            // Act & Assert
            var executionTime = TestHelpers.MeasureExecutionTime(() =>
            {
                for (int i = 0; i < iterations; i++)
                {
                    var signature = keyPair.Sign(message);
                    var isValid = keyPair.PublicKey.Verify(message, signature);
                    Assert.IsTrue(isValid);
                }
            });

            var avgTime = (float)executionTime / iterations;
            Assert.Less(avgTime, 100, "Sign and verify should average less than 100ms");
            Debug.Log($"Sign and verify average time: {avgTime}ms per operation ({iterations} iterations)");
        }

        [Test]
        public void TestMemoryUsage_KeyPairCreation()
        {
            const int iterations = 10;

            // Act & Assert
            var memoryUsage = TestHelpers.MeasureMemoryUsage(() =>
            {
                var keyPairs = new ECKeyPair[iterations];
                for (int i = 0; i < iterations; i++)
                {
                    keyPairs[i] = ECKeyPair.CreateEcKeyPair();
                }
                GC.KeepAlive(keyPairs);
            });

            var avgMemoryPerKey = memoryUsage / iterations;
            Assert.Less(Math.Abs(avgMemoryPerKey), 10 * 1024, "Each key pair should use less than 10KB");
            Debug.Log($"Memory usage per key pair: {avgMemoryPerKey} bytes");
        }

        [UnityTest]
        public IEnumerator TestUnityCoroutineCompatibility()
        {
            // Arrange
            bool keyGenerationCompleted = false;
            ECKeyPair generatedKeyPair = null;

            // Act - Simulate async key generation in coroutine
            yield return new WaitForEndOfFrame();
            
            generatedKeyPair = ECKeyPair.CreateEcKeyPair();
            keyGenerationCompleted = true;

            yield return new WaitUntil(() => keyGenerationCompleted);

            // Assert
            Assert.IsNotNull(generatedKeyPair);
            Assert.IsNotNull(generatedKeyPair.PublicKey);
            Assert.IsNotNull(generatedKeyPair.PrivateKey);
        }

        [Test]
        public void TestSerializationForUnityInspector()
        {
            // Arrange
            var keyPair = ECKeyPair.CreateEcKeyPair();
            var publicKeyHex = keyPair.PublicKey.GetEncodedCompressedHex();

            // Create a serializable wrapper for Unity Inspector
            var serializableKeyData = new SerializableKeyData
            {
                publicKeyHex = publicKeyHex,
                hasPrivateKey = true
            };

            // Act
            var jsonString = JsonUtility.ToJson(serializableKeyData, true);

            // Assert
            Assert.IsNotNull(jsonString);
            Assert.IsTrue(jsonString.Contains(publicKeyHex));
            Debug.Log($"Serializable key data: {jsonString}");

            // Test deserialization
            var deserializedData = JsonUtility.FromJson<SerializableKeyData>(jsonString);
            Assert.AreEqual(publicKeyHex, deserializedData.publicKeyHex);
            Assert.IsTrue(deserializedData.hasPrivateKey);
        }

        [Test]
        public void TestThreadSafety()
        {
            // Arrange
            const int threadCount = 4;
            const int operationsPerThread = 25;
            var tasks = new System.Threading.Tasks.Task[threadCount];
            var results = new ECKeyPair[threadCount * operationsPerThread];
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
                            results[resultIndex] = ECKeyPair.CreateEcKeyPair();
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
            
            // Verify all key pairs were created
            foreach (var result in results)
            {
                Assert.IsNotNull(result);
                Assert.IsNotNull(result.PublicKey);
                Assert.IsNotNull(result.PrivateKey);
            }

            Debug.Log($"Thread safety test passed: {threadCount} threads, {operationsPerThread} ops each");
        }

        [Test]
        public void TestEdgeCases_EmptyAndNullInputs()
        {
            // Test null input handling
            Assert.Throws<ArgumentException>(() => new ECPublicKey(null));
            Assert.Throws<ArgumentException>(() => new ECPublicKey(""));
            Assert.Throws<ArgumentException>(() => new ECPublicKey("   "));
            
            // Test invalid hex characters
            Assert.Throws<ArgumentException>(() => new ECPublicKey("03g4af8d061b6b320cce6c63bc4ec7894dce107bfc5f5ef5c68a93b4ad1e136816"));
            
            // Test invalid private key
            Assert.Throws<ArgumentException>(() => ECKeyPair.CreateFromPrivateKey(null));
            Assert.Throws<ArgumentException>(() => ECKeyPair.CreateFromPrivateKey(new byte[0]));
            Assert.Throws<ArgumentException>(() => ECKeyPair.CreateFromPrivateKey(new byte[31])); // Too short
            Assert.Throws<ArgumentException>(() => ECKeyPair.CreateFromPrivateKey(new byte[33])); // Too long
        }

        #endregion

        #region Helper Classes

        [System.Serializable]
        private class SerializableKeyData
        {
            public string publicKeyHex;
            public bool hasPrivateKey;
        }

        #endregion
    }
}
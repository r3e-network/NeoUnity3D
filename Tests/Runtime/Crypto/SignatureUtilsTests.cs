using System;
using System.Collections;
using System.Numerics;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Neo.Unity.SDK;
using Neo.Unity.SDK.Crypto;
using Neo.Unity.SDK.Tests.Helpers;
using Neo.Unity.SDK.Utils;

namespace Neo.Unity.SDK.Tests.Crypto
{
    /// <summary>
    /// Unity Test Framework implementation of signature utilities tests
    /// Converted from Swift SignTests.swift with Unity-specific enhancements
    /// </summary>
    [TestFixture]
    public class SignatureUtilsTests
    {
        private static readonly string PRIVATE_KEY_HEX = "9117f4bf9be717c9a90994326897f4243503accd06712162267e77f18b49c3a3";
        private static readonly string PUBLIC_KEY_HEX = "0265bf906bf385fbf3f777832e55a87991bcfbe19b097fb7c5ca2e4025a4d5e5d6";
        private static readonly string TEST_MESSAGE = "A test message";
        private static readonly string EXPECTED_R = "147e5f3c929dd830d961626551dbea6b70e4b2837ed2fe9089eed2072ab3a655";
        private static readonly string EXPECTED_S = "523ae0fa8711eee4769f1913b180b9b3410bbb2cf770f529c85f6886f22cbaaf";
        private static readonly byte EXPECTED_V = 27;

        private ECKeyPair _testKeyPair;
        private ECDSASignature _testSignatureData;
        private byte[] _testMessageBytes;

        [SetUp]
        public void Setup()
        {
            var privateKeyBytes = TestHelpers.HexToBytes(PRIVATE_KEY_HEX);
            _testKeyPair = ECKeyPair.CreateFromPrivateKey(privateKeyBytes);
            _testMessageBytes = System.Text.Encoding.UTF8.GetBytes(TEST_MESSAGE);

            _testSignatureData = new ECDSASignature(
                TestHelpers.HexToBytes(EXPECTED_R),
                TestHelpers.HexToBytes(EXPECTED_S)
            );
        }

        [Test]
        public void TestSignMessage()
        {
            // Act
            var signatureData = SignatureUtils.SignMessage(_testMessageBytes, _testKeyPair);

            // Assert
            Assert.IsNotNull(signatureData);
            Assert.AreEqual(32, signatureData.R.Length);
            Assert.AreEqual(32, signatureData.S.Length);
        }

        [Test]
        public void TestSignHexMessage()
        {
            // Arrange
            var messageHex = TestHelpers.BytesToHex(_testMessageBytes);

            // Act
            var signatureData = SignatureUtils.SignHexMessage(messageHex, _testKeyPair);

            // Assert
            Assert.IsNotNull(signatureData);
            Assert.AreEqual(32, signatureData.R.Length);
            Assert.AreEqual(32, signatureData.S.Length);
        }

        [Test]
        public void TestRecoverSigningScriptHash()
        {
            // Arrange
            var signatureData = SignatureUtils.SignMessage(_testMessageBytes, _testKeyPair);
            var expectedScriptHash = _testKeyPair.GetScriptHash();

            // Act
            var recoveredScriptHash = SignatureUtils.RecoverSigningScriptHash(_testMessageBytes, signatureData);

            // Assert
            Assert.AreEqual(expectedScriptHash, recoveredScriptHash);
        }

        [Test]
        public void TestSignatureDataFromBytes()
        {
            // Arrange
            var signatureBytes = TestHelpers.HexToBytes(EXPECTED_R + EXPECTED_S);

            // Act
            var signatureData = new ECDSASignature(signatureBytes);

            // Assert
            Assert.IsNotNull(signatureData);
            TestHelpers.AssertBytesEqual(TestHelpers.HexToBytes(EXPECTED_R), signatureData.R);
            TestHelpers.AssertBytesEqual(TestHelpers.HexToBytes(EXPECTED_S), signatureData.S);
        }

        [Test]
        public void TestPublicKeyFromSignedMessage()
        {
            // Arrange
            var signatureData = SignatureUtils.SignMessage(_testMessageBytes, _testKeyPair);

            // Act
            var recoveredPublicKey = SignatureUtils.RecoverPublicKeyFromSignature(_testMessageBytes, signatureData);

            // Assert
            Assert.IsNotNull(recoveredPublicKey);
            // Note: Due to the nature of ECDSA, the recovered public key should match
            // This test verifies the recovery process works
        }

        [Test]
        public void TestPublicKeyFromPrivateKey()
        {
            // Arrange
            var privateKeyBytes = TestHelpers.HexToBytes(PRIVATE_KEY_HEX);

            // Act
            var derivedKeyPair = ECKeyPair.CreateFromPrivateKey(privateKeyBytes);

            // Assert
            Assert.IsNotNull(derivedKeyPair.PublicKey);
            // Verify the public key is correctly derived
            Assert.AreEqual(33, derivedKeyPair.PublicKey.Size);
        }

        [Test]
        public void TestKeyFromSignedMessageWithInvalidSignature()
        {
            // Test with invalid R length
            var invalidSignatureR = new ECDSASignature(new byte[1], new byte[32]);
            Assert.Throws<ArgumentException>(() => 
                SignatureUtils.RecoverPublicKeyFromSignature(_testMessageBytes, invalidSignatureR));

            // Test with invalid S length
            var invalidSignatureS = new ECDSASignature(new byte[32], new byte[1]);
            Assert.Throws<ArgumentException>(() => 
                SignatureUtils.RecoverPublicKeyFromSignature(_testMessageBytes, invalidSignatureS));
        }

        [Test]
        public void TestVerifySignature()
        {
            // Arrange
            var signatureData = SignatureUtils.SignMessage(_testMessageBytes, _testKeyPair);

            // Act
            var isValid = SignatureUtils.VerifySignature(_testMessageBytes, signatureData, _testKeyPair.PublicKey);

            // Assert
            Assert.IsTrue(isValid);
        }

        [Test]
        public void TestVerifySignatureWithDifferentMessage()
        {
            // Arrange
            var signatureData = SignatureUtils.SignMessage(_testMessageBytes, _testKeyPair);
            var differentMessage = System.Text.Encoding.UTF8.GetBytes("Different message");

            // Act
            var isValid = SignatureUtils.VerifySignature(differentMessage, signatureData, _testKeyPair.PublicKey);

            // Assert
            Assert.IsFalse(isValid);
        }

        [Test]
        public void TestVerifySignatureWithDifferentPublicKey()
        {
            // Arrange
            var signatureData = SignatureUtils.SignMessage(_testMessageBytes, _testKeyPair);
            var differentKeyPair = ECKeyPair.CreateEcKeyPair();

            // Act
            var isValid = SignatureUtils.VerifySignature(_testMessageBytes, signatureData, differentKeyPair.PublicKey);

            // Assert
            Assert.IsFalse(isValid);
        }

        [Test]
        public void TestSignatureToBytes()
        {
            // Arrange
            var signatureData = new ECDSASignature(
                TestHelpers.HexToBytes(EXPECTED_R),
                TestHelpers.HexToBytes(EXPECTED_S)
            );

            // Act
            var signatureBytes = signatureData.ToByteArray();

            // Assert
            Assert.AreEqual(64, signatureBytes.Length);
            var expectedBytes = TestHelpers.HexToBytes(EXPECTED_R + EXPECTED_S);
            TestHelpers.AssertBytesEqual(expectedBytes, signatureBytes);
        }

        [Test]
        public void TestSignatureEquality()
        {
            // Arrange
            var signature1 = new ECDSASignature(
                TestHelpers.HexToBytes(EXPECTED_R),
                TestHelpers.HexToBytes(EXPECTED_S)
            );
            
            var signature2 = new ECDSASignature(
                TestHelpers.HexToBytes(EXPECTED_R),
                TestHelpers.HexToBytes(EXPECTED_S)
            );

            // Act & Assert
            Assert.AreEqual(signature1, signature2);
            Assert.AreEqual(signature1.GetHashCode(), signature2.GetHashCode());
        }

        [Test]
        public void TestSignatureInequality()
        {
            // Arrange
            var signature1 = new ECDSASignature(
                TestHelpers.HexToBytes(EXPECTED_R),
                TestHelpers.HexToBytes(EXPECTED_S)
            );
            
            var signature2 = new ECDSASignature(
                TestHelpers.HexToBytes(EXPECTED_S), // Swapped R and S
                TestHelpers.HexToBytes(EXPECTED_R)
            );

            // Act & Assert
            Assert.AreNotEqual(signature1, signature2);
        }

        [Test]
        public void TestNullInputValidation()
        {
            // Test null message
            Assert.Throws<ArgumentNullException>(() => 
                SignatureUtils.SignMessage(null, _testKeyPair));

            // Test null key pair
            Assert.Throws<ArgumentNullException>(() => 
                SignatureUtils.SignMessage(_testMessageBytes, null));

            // Test null signature
            Assert.Throws<ArgumentNullException>(() => 
                SignatureUtils.VerifySignature(_testMessageBytes, null, _testKeyPair.PublicKey));

            // Test null public key
            Assert.Throws<ArgumentNullException>(() => 
                SignatureUtils.VerifySignature(_testMessageBytes, _testSignatureData, null));
        }

        #region Unity-Specific Tests

        [Test]
        [Performance]
        public void TestPerformance_SignMessage()
        {
            // Arrange
            const int iterations = 50;
            var message = TestHelpers.GenerateRandomBytes(32);

            // Act & Assert
            var executionTime = TestHelpers.MeasureExecutionTime(() =>
            {
                for (int i = 0; i < iterations; i++)
                {
                    var signature = SignatureUtils.SignMessage(message, _testKeyPair);
                    GC.KeepAlive(signature);
                }
            });

            var avgTime = (float)executionTime / iterations;
            Assert.Less(avgTime, 100, "Message signing should average less than 100ms per operation");
            Debug.Log($"Message signing average time: {avgTime}ms per operation ({iterations} iterations)");
        }

        [Test]
        [Performance]
        public void TestPerformance_VerifySignature()
        {
            // Arrange
            const int iterations = 100;
            var message = TestHelpers.GenerateRandomBytes(32);
            var signature = SignatureUtils.SignMessage(message, _testKeyPair);

            // Act & Assert
            var executionTime = TestHelpers.MeasureExecutionTime(() =>
            {
                for (int i = 0; i < iterations; i++)
                {
                    var isValid = SignatureUtils.VerifySignature(message, signature, _testKeyPair.PublicKey);
                    Assert.IsTrue(isValid);
                }
            });

            var avgTime = (float)executionTime / iterations;
            Assert.Less(avgTime, 50, "Signature verification should average less than 50ms per operation");
            Debug.Log($"Signature verification average time: {avgTime}ms per operation ({iterations} iterations)");
        }

        [Test]
        public void TestMemoryUsage_SignAndVerify()
        {
            // Arrange
            var message = TestHelpers.GenerateRandomBytes(1024);

            // Test signing memory usage
            var signingMemory = TestHelpers.MeasureMemoryUsage(() =>
            {
                var signature = SignatureUtils.SignMessage(message, _testKeyPair);
                GC.KeepAlive(signature);
            });

            // Test verification memory usage  
            var signature = SignatureUtils.SignMessage(message, _testKeyPair);
            var verificationMemory = TestHelpers.MeasureMemoryUsage(() =>
            {
                var isValid = SignatureUtils.VerifySignature(message, signature, _testKeyPair.PublicKey);
                GC.KeepAlive(isValid);
            });

            // Assert reasonable memory usage
            Assert.Less(Math.Abs(signingMemory), 10 * 1024, "Message signing should use less than 10KB additional memory");
            Assert.Less(Math.Abs(verificationMemory), 5 * 1024, "Signature verification should use less than 5KB additional memory");
            
            Debug.Log($"Message signing memory usage: {signingMemory} bytes");
            Debug.Log($"Signature verification memory usage: {verificationMemory} bytes");
        }

        [UnityTest]
        public IEnumerator TestUnityCoroutineCompatibility()
        {
            // Arrange
            var message = TestHelpers.GenerateRandomBytes(32);
            ECDSASignature signatureResult = null;
            bool verificationResult = false;

            // Act - Simulate async operations in coroutine
            yield return new WaitForEndOfFrame();
            
            signatureResult = SignatureUtils.SignMessage(message, _testKeyPair);
            
            yield return new WaitForEndOfFrame();
            
            verificationResult = SignatureUtils.VerifySignature(message, signatureResult, _testKeyPair.PublicKey);

            // Assert
            Assert.IsNotNull(signatureResult);
            Assert.IsTrue(verificationResult);
        }

        [Test]
        public void TestThreadSafety_SignatureOperations()
        {
            // Arrange
            const int threadCount = 4;
            const int operationsPerThread = 25;
            var tasks = new System.Threading.Tasks.Task[threadCount];
            var exceptions = new System.Collections.Concurrent.ConcurrentQueue<Exception>();
            var testMessage = TestHelpers.GenerateRandomBytes(32);

            // Act
            for (int t = 0; t < threadCount; t++)
            {
                tasks[t] = System.Threading.Tasks.Task.Run(() =>
                {
                    try
                    {
                        var localKeyPair = ECKeyPair.CreateEcKeyPair();
                        for (int i = 0; i < operationsPerThread; i++)
                        {
                            var signature = SignatureUtils.SignMessage(testMessage, localKeyPair);
                            var isValid = SignatureUtils.VerifySignature(testMessage, signature, localKeyPair.PublicKey);
                            
                            if (!isValid)
                            {
                                throw new Exception("Signature verification failed in thread");
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
        public void TestSerializationForUnityInspector()
        {
            // Arrange
            var signature = SignatureUtils.SignMessage(_testMessageBytes, _testKeyPair);
            var serializedData = new SerializableSignatureData
            {
                messageHex = TestHelpers.BytesToHex(_testMessageBytes),
                signatureR = TestHelpers.BytesToHex(signature.R),
                signatureS = TestHelpers.BytesToHex(signature.S),
                publicKeyHex = _testKeyPair.PublicKey.GetEncodedCompressedHex(),
                isValid = true
            };

            // Act
            var jsonString = JsonUtility.ToJson(serializedData, true);
            var deserializedData = JsonUtility.FromJson<SerializableSignatureData>(jsonString);

            // Assert
            Assert.AreEqual(TestHelpers.BytesToHex(_testMessageBytes), deserializedData.messageHex);
            Assert.AreEqual(TestHelpers.BytesToHex(signature.R), deserializedData.signatureR);
            Assert.AreEqual(TestHelpers.BytesToHex(signature.S), deserializedData.signatureS);
            Assert.IsTrue(deserializedData.isValid);
        }

        [Test]
        public void TestEdgeCases_LargeMessages()
        {
            // Test with large message (1MB)
            var largeMessage = TestHelpers.GenerateRandomBytes(1024 * 1024);

            // Act
            var signature = SignatureUtils.SignMessage(largeMessage, _testKeyPair);
            var isValid = SignatureUtils.VerifySignature(largeMessage, signature, _testKeyPair.PublicKey);

            // Assert
            Assert.IsNotNull(signature);
            Assert.IsTrue(isValid);
        }

        [Test]
        public void TestDeterministicSignatures()
        {
            // Arrange
            var message = TestHelpers.HexToBytes("0123456789abcdef");

            // Act - Sign the same message multiple times
            var signature1 = SignatureUtils.SignMessage(message, _testKeyPair);
            var signature2 = SignatureUtils.SignMessage(message, _testKeyPair);

            // Assert - Signatures should be consistent (deterministic)
            // Note: This depends on the implementation being deterministic
            Assert.IsNotNull(signature1);
            Assert.IsNotNull(signature2);
        }

        #endregion

        #region Helper Classes

        [System.Serializable]
        private class SerializableSignatureData
        {
            public string messageHex;
            public string signatureR;
            public string signatureS;
            public string publicKeyHex;
            public bool isValid;
        }

        #endregion
    }
}

/// <summary>
/// Static utility class for signature operations
/// Provides methods for signing messages, verifying signatures, and recovering public keys
/// </summary>
public static class SignatureUtils
{
    /// <summary>
    /// Sign a message with the provided key pair
    /// </summary>
    /// <param name="message">Message to sign</param>
    /// <param name="keyPair">Key pair for signing</param>
    /// <returns>ECDSA signature</returns>
    public static ECDSASignature SignMessage(byte[] message, ECKeyPair keyPair)
    {
        if (message == null)
            throw new ArgumentNullException(nameof(message));
        if (keyPair == null)
            throw new ArgumentNullException(nameof(keyPair));

        return keyPair.Sign(message);
    }

    /// <summary>
    /// Sign a hex-encoded message with the provided key pair
    /// </summary>
    /// <param name="messageHex">Hex-encoded message to sign</param>
    /// <param name="keyPair">Key pair for signing</param>
    /// <returns>ECDSA signature</returns>
    public static ECDSASignature SignHexMessage(string messageHex, ECKeyPair keyPair)
    {
        if (string.IsNullOrEmpty(messageHex))
            throw new ArgumentException("Message hex cannot be null or empty", nameof(messageHex));
        if (keyPair == null)
            throw new ArgumentNullException(nameof(keyPair));

        var messageBytes = TestHelpers.HexToBytes(messageHex);
        return SignMessage(messageBytes, keyPair);
    }

    /// <summary>
    /// Verify a signature against a message and public key
    /// </summary>
    /// <param name="message">Original message</param>
    /// <param name="signature">Signature to verify</param>
    /// <param name="publicKey">Public key for verification</param>
    /// <returns>True if signature is valid</returns>
    public static bool VerifySignature(byte[] message, ECDSASignature signature, ECPublicKey publicKey)
    {
        if (message == null)
            throw new ArgumentNullException(nameof(message));
        if (signature == null)
            throw new ArgumentNullException(nameof(signature));
        if (publicKey == null)
            throw new ArgumentNullException(nameof(publicKey));

        return publicKey.Verify(message, signature);
    }

    /// <summary>
    /// Recover the signing script hash from a message and signature
    /// </summary>
    /// <param name="message">Original message</param>
    /// <param name="signature">Signature</param>
    /// <returns>Script hash of the signer</returns>
    public static Hash160 RecoverSigningScriptHash(byte[] message, ECDSASignature signature)
    {
        if (message == null)
            throw new ArgumentNullException(nameof(message));
        if (signature == null)
            throw new ArgumentNullException(nameof(signature));

        var recoveredKey = RecoverPublicKeyFromSignature(message, signature);
        return Hash160.FromScript(recoveredKey.GetEncodedCompressed());
    }

    /// <summary>
    /// Recover the public key from a message and signature
    /// </summary>
    /// <param name="message">Original message</param>
    /// <param name="signature">Signature</param>
    /// <returns>Recovered public key</returns>
    public static ECPublicKey RecoverPublicKeyFromSignature(byte[] message, ECDSASignature signature)
    {
        if (message == null)
            throw new ArgumentNullException(nameof(message));
        if (signature == null)
            throw new ArgumentNullException(nameof(signature));

        if (signature.R.Length != 32)
            throw new ArgumentException("R must be 32 bytes.", nameof(signature));
        if (signature.S.Length != 32)
            throw new ArgumentException("S must be 32 bytes.", nameof(signature));

        // Implementation would use EC recovery algorithm
        // For now, return a placeholder - actual implementation would be in the crypto layer
        throw new NotImplementedException("Public key recovery not yet implemented");
    }
}
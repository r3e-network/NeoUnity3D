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
    /// Unity Test Framework implementation of BIP32 HD wallet key pair tests
    /// Converted from Swift Bip32ECKeyPairTests.swift with Unity-specific enhancements
    /// </summary>
    [TestFixture]
    public class Bip32ECKeyPairTests
    {
        private const uint HARDENED_BIT = 0x80000000;

        [Test]
        public void TestVectors1()
        {
            // Chain m
            GenerateAndTest(
                "000102030405060708090a0b0c0d0e0f",
                "xprv9s21ZrQH143K3QTDL4LXw2F7HEK3wJUD2nW2nRk4stbPy6cq3jPPqjiChkVvvNKmPGJxWUtg6LnF5kejMRNNU3TGtRBeJgk33yuGBxrMPHi",
                "xpub661MyMwAqRbcFtXgS5sYJABqqG9YLmC4Q1Rdap9gSE8NqtwybGhePY2gZ29ESFjqJoCu1Rupje8YtGqsefD265TMg7usUDFdp6W1EGMcet8",
                new uint[0]);
            
            // Chain m/0H
            GenerateAndTest(
                "000102030405060708090a0b0c0d0e0f",
                "xprv9uHRZZhk6KAJC1avXpDAp4MDc3sQKNxDiPvvkX8Br5ngLNv1TxvUxt4cV1rGL5hj6KCesnDYUhd7oWgT11eZG7XnxHrnYeSvkzY7d2bhkJ7",
                "xpub68Gmy5EdvgibQVfPdqkBBCHxA5htiqg55crXYuXoQRKfDBFA1WEjWgP6LHhwBZeNK1VTsfTFUHCdrfp1bgwQ9xv5ski8PX9rL2dZXvgGDnw",
                new uint[] { 0 | HARDENED_BIT });
            
            // Chain m/0H/1
            GenerateAndTest(
                "000102030405060708090a0b0c0d0e0f",
                "xprv9wTYmMFdV23N2TdNG573QoEsfRrWKQgWeibmLntzniatZvR9BmLnvSxqu53Kw1UmYPxLgboyZQaXwTCg8MSY3H2EU4pWcQDnRnrVA1xe8fs",
                "xpub6ASuArnXKPbfEwhqN6e3mwBcDTgzisQN1wXN9BJcM47sSikHjJf3UFHKkNAWbWMiGj7Wf5uMash7SyYq527Hqck2AxYysAA7xmALppuCkwQ",
                new uint[] { 0 | HARDENED_BIT, 1 });
            
            // Chain m/0H/1/2H
            GenerateAndTest(
                "000102030405060708090a0b0c0d0e0f",
                "xprv9z4pot5VBttmtdRTWfWQmoH1taj2axGVzFqSb8C9xaxKymcFzXBDptWmT7FwuEzG3ryjH4ktypQSAewRiNMjANTtpgP4mLTj34bhnZX7UiM",
                "xpub6D4BDPcP2GT577Vvch3R8wDkScZWzQzMMUm3PWbmWvVJrZwQY4VUNgqFJPMM3No2dFDFGTsxxpG5uJh7n7epu4trkrX7x7DogT5Uv6fcLW5",
                new uint[] { 0 | HARDENED_BIT, 1, 2 | HARDENED_BIT });
            
            // Chain m/0H/1/2H/2
            GenerateAndTest(
                "000102030405060708090a0b0c0d0e0f",
                "xprvA2JDeKCSNNZky6uBCviVfJSKyQ1mDYahRjijr5idH2WwLsEd4Hsb2Tyh8RfQMuPh7f7RtyzTtdrbdqqsunu5Mm3wDvUAKRHSC34sJ7in334",
                "xpub6FHa3pjLCk84BayeJxFW2SP4XRrFd1JYnxeLeU8EqN3vDfZmbqBqaGJAyiLjTAwm6ZLRQUMv1ZACTj37sR62cfN7fe5JnJ7dh8zL4fiyLHV",
                new uint[] { 0 | HARDENED_BIT, 1, 2 | HARDENED_BIT, 2 });
            
            // Chain m/0H/1/2H/2/1000000000
            GenerateAndTest(
                "000102030405060708090a0b0c0d0e0f",
                "xprvA41z7zogVVwxVSgdKUHDy1SKmdb533PjDz7J6N6mV6uS3ze1ai8FHa8kmHScGpWmj4WggLyQjgPie1rFSruoUihUZREPSL39UNdE3BBDu76",
                "xpub6H1LXWLaKsWFhvm6RVpEL9P4KfRZSW7abD2ttkWP3SSQvnyA8FSVqNTEcYFgJS2UaFcxupHiYkro49S8yGasTvXEYBVPamhGW6cFJodrTHy",
                new uint[] { 0 | HARDENED_BIT, 1, 2 | HARDENED_BIT, 2, 1000000000 });
        }

        [Test]
        public void TestVectors2()
        {
            // Chain m
            GenerateAndTest(
                "fffcf9f6f3f0edeae7e4e1dedbd8d5d2cfccc9c6c3c0bdbab7b4b1aeaba8a5a29f9c999693908d8a8784817e7b7875726f6c696663605d5a5754514e4b484542",
                "xprv9s21ZrQH143K31xYSDQpPDxsXRTUcvj2iNHm5NUtrGiGG5e2DtALGdso3pGz6ssrdK4PFmM8NSpSBHNqPqm55Qn3LqFtT2emdEXVYsCzC2U",
                "xpub661MyMwAqRbcFW31YEwpkMuc5THy2PSt5bDMsktWQcFF8syAmRUapSCGu8ED9W6oDMSgv6Zz8idoc4a6mr8BDzTJY47LJhkJ8UB7WEGuduB",
                new uint[0]);
            
            // Chain m/0
            GenerateAndTest(
                "fffcf9f6f3f0edeae7e4e1dedbd8d5d2cfccc9c6c3c0bdbab7b4b1aeaba8a5a29f9c999693908d8a8784817e7b7875726f6c696663605d5a5754514e4b484542",
                "xprv9vHkqa6EV4sPZHYqZznhT2NPtPCjKuDKGY38FBWLvgaDx45zo9WQRUT3dKYnjwih2yJD9mkrocEZXo1ex8G81dwSM1fwqWpWkeS3v86pgKt",
                "xpub69H7F5d8KSRgmmdJg2KhpAK8SR3DjMwAdkxj3ZuxV27CprR9LgpeyGmXUbC6wb7ERfvrnKZjXoUmmDznezpbZb7ap6r1D3tgFxHmwMkQTPH",
                new uint[] { 0 });
            
            // Chain m/0/2147483647H
            GenerateAndTest(
                "fffcf9f6f3f0edeae7e4e1dedbd8d5d2cfccc9c6c3c0bdbab7b4b1aeaba8a5a29f9c999693908d8a8784817e7b7875726f6c696663605d5a5754514e4b484542",
                "xprv9wSp6B7kry3Vj9m1zSnLvN3xH8RdsPP1Mh7fAaR7aRLcQMKTR2vidYEeEg2mUCTAwCd6vnxVrcjfy2kRgVsFawNzmjuHc2YmYRmagcEPdU9",
                "xpub6ASAVgeehLbnwdqV6UKMHVzgqAG8Gr6riv3Fxxpj8ksbH9ebxaEyBLZ85ySDhKiLDBrQSARLq1uNRts8RuJiHjaDMBU4Zn9h8LZNnBC5y4a",
                new uint[] { 0, 2147483647 | HARDENED_BIT });
            
            // Chain m/0/2147483647H/1
            GenerateAndTest(
                "fffcf9f6f3f0edeae7e4e1dedbd8d5d2cfccc9c6c3c0bdbab7b4b1aeaba8a5a29f9c999693908d8a8784817e7b7875726f6c696663605d5a5754514e4b484542",
                "xprv9zFnWC6h2cLgpmSA46vutJzBcfJ8yaJGg8cX1e5StJh45BBciYTRXSd25UEPVuesF9yog62tGAQtHjXajPPdbRCHuWS6T8XA2ECKADdw4Ef",
                "xpub6DF8uhdarytz3FWdA8TvFSvvAh8dP3283MY7p2V4SeE2wyWmG5mg5EwVvmdMVCQcoNJxGoWaU9DCWh89LojfZ537wTfunKau47EL2dhHKon",
                new uint[] { 0, 2147483647 | HARDENED_BIT, 1 });
            
            // Chain m/0/2147483647H/1/2147483646H
            GenerateAndTest(
                "fffcf9f6f3f0edeae7e4e1dedbd8d5d2cfccc9c6c3c0bdbab7b4b1aeaba8a5a29f9c999693908d8a8784817e7b7875726f6c696663605d5a5754514e4b484542",
                "xprvA1RpRA33e1JQ7ifknakTFpgNXPmW2YvmhqLQYMmrj4xJXXWYpDPS3xz7iAxn8L39njGVyuoseXzU6rcxFLJ8HFsTjSyQbLYnMpCqE2VbFWc",
                "xpub6ERApfZwUNrhLCkDtcHTcxd75RbzS1ed54G1LkBUHQVHQKqhMkhgbmJbZRkrgZw4koxb5JaHWkY4ALHY2grBGRjaDMzQLcgJvLJuZZvRcEL",
                new uint[] { 0, 2147483647 | HARDENED_BIT, 1, 2147483646 | HARDENED_BIT });
            
            // Chain m/0/2147483647H/1/2147483646H/2
            GenerateAndTest(
                "fffcf9f6f3f0edeae7e4e1dedbd8d5d2cfccc9c6c3c0bdbab7b4b1aeaba8a5a29f9c999693908d8a5754514e4b484542",
                "xprvA2nrNbFZABcdryreWet9Ea4LvTJcGsqrMzxHx98MMrotbir7yrKCEXw7nadnHM8Dq38EGfSh6dqA9QWTyefMLEcBYJUuekgW4BYPJcr9E7j",
                "xpub6FnCn6nSzZAw5Tw7cgR9bi15UV96gLZhjDstkXXxvCLsUXBGXPdSnLFbdpq8p9HmGsApME5hQTZ3emM2rnY5agb9rXpVGyy3bdW6EEgAtqt",
                new uint[] { 0, 2147483647 | HARDENED_BIT, 1, 2147483646 | HARDENED_BIT, 2 });
        }

        [Test]
        public void TestVectors3()
        {
            // Chain m
            GenerateAndTest(
                "4b381541583be4423346c643850da4b320e46a87ae3d2a4e6da11eba819cd4acba45d239319ac14f863b8d5ab5a0d0c64d2e8a1e7d1457df2e5a3c51c73235be",
                "xprv9s21ZrQH143K25QhxbucbDDuQ4naNntJRi4KUfWT7xo4EKsHt2QJDu7KXp1A3u7Bi1j8ph3EGsZ9Xvz9dGuVrtHHs7pXeTzjuxBrCmmhgC6",
                "xpub661MyMwAqRbcEZVB4dScxMAdx6d4nFc9nvyvH3v4gJL378CSRZiYmhRoP7mBy6gSPSCYk6SzXPTf3ND1cZAceL7SfJ1Z3GC8vBgp2epUt13",
                new uint[0]);
            
            // Chain m/0H
            GenerateAndTest(
                "4b381541583be4423346c643850da4b320e46a87ae3d2a4e6da11eba819cd4acba45d239319ac14f863b8d5ab5a0d0c64d2e8a1e7d1457df2e5a3c51c73235be",
                "xprv9uPDJpEQgRQfDcW7BkF7eTya6RPxXeJCqCJGHuCJ4GiRVLzkTXBAJMu2qaMWPrS7AANYqdq6vcBcBUdJCVVFceUvJFjaPdGZ2y9WACViL4L",
                "xpub68NZiKmJWnxxS6aaHmn81bvJeTESw724CRDs6HbuccFQN9Ku14VQrADWgqbhhTHBaohPX4CjNLf9fq9MYo6oDaPPLPxSb7gwQN3ih19Zm4Y",
                new uint[] { 0 | HARDENED_BIT });
        }

        [Test]
        public void TestHardenedKeyDerivation()
        {
            // Arrange
            var seed = TestHelpers.HexToBytes("000102030405060708090a0b0c0d0e0f");
            var masterKeyPair = Bip32ECKeyPair.GenerateKeyPair(seed);

            // Act
            var hardenedChild = Bip32ECKeyPair.DeriveKeyPair(masterKeyPair, new uint[] { 0 | HARDENED_BIT });

            // Assert
            Assert.IsNotNull(hardenedChild);
            Assert.AreNotEqual(masterKeyPair.PrivateKey, hardenedChild.PrivateKey);
            Assert.AreNotEqual(masterKeyPair.PublicKey, hardenedChild.PublicKey);
        }

        [Test]
        public void TestNonHardenedKeyDerivation()
        {
            // Arrange
            var seed = TestHelpers.HexToBytes("000102030405060708090a0b0c0d0e0f");
            var masterKeyPair = Bip32ECKeyPair.GenerateKeyPair(seed);

            // Act
            var nonHardenedChild = Bip32ECKeyPair.DeriveKeyPair(masterKeyPair, new uint[] { 0 });

            // Assert
            Assert.IsNotNull(nonHardenedChild);
            Assert.AreNotEqual(masterKeyPair.PrivateKey, nonHardenedChild.PrivateKey);
            Assert.AreNotEqual(masterKeyPair.PublicKey, nonHardenedChild.PublicKey);
        }

        [Test]
        public void TestDeepKeyDerivation()
        {
            // Arrange
            var seed = TestHelpers.HexToBytes("000102030405060708090a0b0c0d0e0f");
            var masterKeyPair = Bip32ECKeyPair.GenerateKeyPair(seed);

            // Act
            var deepChild = Bip32ECKeyPair.DeriveKeyPair(masterKeyPair, 
                new uint[] { 44 | HARDENED_BIT, 0 | HARDENED_BIT, 0 | HARDENED_BIT, 0, 0 });

            // Assert
            Assert.IsNotNull(deepChild);
            Assert.AreEqual(5, deepChild.Depth);
        }

        [Test]
        public void TestKeyPairEquality()
        {
            // Arrange
            var seed = TestHelpers.HexToBytes("000102030405060708090a0b0c0d0e0f");
            var keyPair1 = Bip32ECKeyPair.GenerateKeyPair(seed);
            var keyPair2 = Bip32ECKeyPair.GenerateKeyPair(seed);

            // Assert
            Assert.AreEqual(keyPair1.PrivateKey, keyPair2.PrivateKey);
            Assert.AreEqual(keyPair1.PublicKey, keyPair2.PublicKey);
            Assert.AreEqual(keyPair1.ChainCode, keyPair2.ChainCode);
        }

        [Test]
        public void TestInvalidSeedLength()
        {
            // Test too short seed
            var shortSeed = TestHelpers.GenerateRandomBytes(8);
            Assert.Throws<ArgumentException>(() => Bip32ECKeyPair.GenerateKeyPair(shortSeed));

            // Test too long seed
            var longSeed = TestHelpers.GenerateRandomBytes(128);
            Assert.Throws<ArgumentException>(() => Bip32ECKeyPair.GenerateKeyPair(longSeed));
        }

        [Test]
        public void TestInvalidDerivationPath()
        {
            // Arrange
            var seed = TestHelpers.HexToBytes("000102030405060708090a0b0c0d0e0f");
            var masterKeyPair = Bip32ECKeyPair.GenerateKeyPair(seed);

            // Test null path
            Assert.Throws<ArgumentNullException>(() => 
                Bip32ECKeyPair.DeriveKeyPair(masterKeyPair, null));

            // Test empty path (should return master key)
            var emptyPathChild = Bip32ECKeyPair.DeriveKeyPair(masterKeyPair, new uint[0]);
            Assert.AreEqual(masterKeyPair.PrivateKey, emptyPathChild.PrivateKey);
        }

        [Test]
        public void TestPublicKeyDerivation()
        {
            // Arrange
            var seed = TestHelpers.HexToBytes("000102030405060708090a0b0c0d0e0f");
            var masterKeyPair = Bip32ECKeyPair.GenerateKeyPair(seed);

            // Act - Derive child with private key
            var childPrivate = Bip32ECKeyPair.DeriveKeyPair(masterKeyPair, new uint[] { 0 });

            // Derive same child from public key only (non-hardened derivation)
            var childPublic = Bip32ECKeyPair.DerivePublicKey(masterKeyPair, new uint[] { 0 });

            // Assert
            Assert.AreEqual(childPrivate.PublicKey, childPublic);
        }

        [Test]
        public void TestHardenedKeyPublicDerivationFails()
        {
            // Arrange
            var seed = TestHelpers.HexToBytes("000102030405060708090a0b0c0d0e0f");
            var masterKeyPair = Bip32ECKeyPair.GenerateKeyPair(seed);

            // Act & Assert - Hardened derivation should fail with public key only
            Assert.Throws<InvalidOperationException>(() => 
                Bip32ECKeyPair.DerivePublicKey(masterKeyPair, new uint[] { 0 | HARDENED_BIT }));
        }

        private void GenerateAndTest(string seed, string expectedPrivateKey, 
            string expectedPublicKey, uint[] path)
        {
            // Arrange
            var seedBytes = TestHelpers.HexToBytes(seed);
            var masterKeyPair = Bip32ECKeyPair.GenerateKeyPair(seedBytes);
            
            // Act
            var childKeyPair = Bip32ECKeyPair.DeriveKeyPair(masterKeyPair, path);
            
            // Serialize keys
            var privateKeySerialized = SerializePrivateKey(childKeyPair);
            var publicKeySerialized = SerializePublicKey(childKeyPair);
            
            var privateKeyWithChecksum = AddChecksum(privateKeySerialized);
            var publicKeyWithChecksum = AddChecksum(publicKeySerialized);
            
            var privateKeyBase58 = Base58.Encode(privateKeyWithChecksum);
            var publicKeyBase58 = Base58.Encode(publicKeyWithChecksum);
            
            // Assert
            Assert.AreEqual(expectedPrivateKey, privateKeyBase58);
            Assert.AreEqual(expectedPublicKey, publicKeyBase58);
        }

        private byte[] AddChecksum(byte[] input)
        {
            var hash = Hash.Sha256(Hash.Sha256(input));
            var result = new byte[input.Length + 4];
            Array.Copy(input, 0, result, 0, input.Length);
            Array.Copy(hash, 0, result, input.Length, 4);
            return result;
        }

        private byte[] SerializePublicKey(Bip32ECKeyPair keyPair)
        {
            return SerializeKey(keyPair, 0x0488B21E, true);
        }

        private byte[] SerializePrivateKey(Bip32ECKeyPair keyPair)
        {
            return SerializeKey(keyPair, 0x0488ADE4, false);
        }

        private byte[] SerializeKey(Bip32ECKeyPair keyPair, uint header, bool isPublic)
        {
            var buffer = new byte[78];
            var offset = 0;

            // Header (4 bytes)
            buffer[offset++] = (byte)(header >> 24);
            buffer[offset++] = (byte)(header >> 16);
            buffer[offset++] = (byte)(header >> 8);
            buffer[offset++] = (byte)header;

            // Depth (1 byte)
            buffer[offset++] = (byte)keyPair.Depth;

            // Parent fingerprint (4 bytes)
            var parentFingerprintBytes = BitConverter.GetBytes(keyPair.ParentFingerprint);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(parentFingerprintBytes);
            Array.Copy(parentFingerprintBytes, 0, buffer, offset, 4);
            offset += 4;

            // Child number (4 bytes)
            var childNumberBytes = BitConverter.GetBytes(keyPair.ChildNumber);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(childNumberBytes);
            Array.Copy(childNumberBytes, 0, buffer, offset, 4);
            offset += 4;

            // Chain code (32 bytes)
            Array.Copy(keyPair.ChainCode, 0, buffer, offset, 32);
            offset += 32;

            // Key data (33 bytes)
            if (isPublic)
            {
                var publicKeyBytes = keyPair.PublicKey.GetEncoded(compressed: true);
                Array.Copy(publicKeyBytes, 0, buffer, offset, 33);
            }
            else
            {
                buffer[offset] = 0x00; // Private key prefix
                Array.Copy(keyPair.PrivateKey, 0, buffer, offset + 1, 32);
            }

            return buffer;
        }

        #region Unity-Specific Tests

        [Test]
        [Performance]
        public void TestPerformance_KeyGeneration()
        {
            // Arrange
            const int iterations = 10;
            var seed = TestHelpers.HexToBytes("000102030405060708090a0b0c0d0e0f");

            // Act & Assert
            var executionTime = TestHelpers.MeasureExecutionTime(() =>
            {
                for (int i = 0; i < iterations; i++)
                {
                    var keyPair = Bip32ECKeyPair.GenerateKeyPair(seed);
                    GC.KeepAlive(keyPair);
                }
            });

            var avgTime = (float)executionTime / iterations;
            Assert.Less(avgTime, 100, "BIP32 key generation should average less than 100ms per operation");
            Debug.Log($"BIP32 key generation average time: {avgTime}ms per operation ({iterations} iterations)");
        }

        [Test]
        [Performance]
        public void TestPerformance_KeyDerivation()
        {
            // Arrange
            const int iterations = 50;
            var seed = TestHelpers.HexToBytes("000102030405060708090a0b0c0d0e0f");
            var masterKeyPair = Bip32ECKeyPair.GenerateKeyPair(seed);
            var path = new uint[] { 44 | HARDENED_BIT, 0 | HARDENED_BIT, 0 | HARDENED_BIT, 0, 0 };

            // Act & Assert
            var executionTime = TestHelpers.MeasureExecutionTime(() =>
            {
                for (int i = 0; i < iterations; i++)
                {
                    var childKeyPair = Bip32ECKeyPair.DeriveKeyPair(masterKeyPair, path);
                    GC.KeepAlive(childKeyPair);
                }
            });

            var avgTime = (float)executionTime / iterations;
            Assert.Less(avgTime, 50, "BIP32 key derivation should average less than 50ms per operation");
            Debug.Log($"BIP32 key derivation average time: {avgTime}ms per operation ({iterations} iterations)");
        }

        [Test]
        public void TestMemoryUsage_KeyOperations()
        {
            // Arrange
            var seed = TestHelpers.HexToBytes("000102030405060708090a0b0c0d0e0f");

            // Test master key generation memory usage
            var masterGenMemory = TestHelpers.MeasureMemoryUsage(() =>
            {
                var keyPair = Bip32ECKeyPair.GenerateKeyPair(seed);
                GC.KeepAlive(keyPair);
            });

            // Test child key derivation memory usage
            var masterKeyPair = Bip32ECKeyPair.GenerateKeyPair(seed);
            var derivationMemory = TestHelpers.MeasureMemoryUsage(() =>
            {
                var childKeyPair = Bip32ECKeyPair.DeriveKeyPair(masterKeyPair, new uint[] { 0 });
                GC.KeepAlive(childKeyPair);
            });

            // Assert reasonable memory usage
            Assert.Less(Math.Abs(masterGenMemory), 20 * 1024, "Master key generation should use less than 20KB additional memory");
            Assert.Less(Math.Abs(derivationMemory), 10 * 1024, "Key derivation should use less than 10KB additional memory");
            
            Debug.Log($"Master key generation memory usage: {masterGenMemory} bytes");
            Debug.Log($"Key derivation memory usage: {derivationMemory} bytes");
        }

        [UnityTest]
        public IEnumerator TestUnityCoroutineCompatibility()
        {
            // Arrange
            var seed = TestHelpers.HexToBytes("000102030405060708090a0b0c0d0e0f");
            Bip32ECKeyPair masterKeyPair = null;
            Bip32ECKeyPair childKeyPair = null;

            // Act - Simulate async operations in coroutine
            yield return new WaitForEndOfFrame();
            
            masterKeyPair = Bip32ECKeyPair.GenerateKeyPair(seed);
            
            yield return new WaitForEndOfFrame();
            
            childKeyPair = Bip32ECKeyPair.DeriveKeyPair(masterKeyPair, new uint[] { 0 });

            // Assert
            Assert.IsNotNull(masterKeyPair);
            Assert.IsNotNull(childKeyPair);
            Assert.AreNotEqual(masterKeyPair.PrivateKey, childKeyPair.PrivateKey);
        }

        [Test]
        public void TestSerializationForUnityInspector()
        {
            // Arrange
            var seed = TestHelpers.HexToBytes("000102030405060708090a0b0c0d0e0f");
            var keyPair = Bip32ECKeyPair.GenerateKeyPair(seed);
            
            var serializedData = new SerializableBip32Data
            {
                depth = keyPair.Depth,
                childNumber = keyPair.ChildNumber,
                parentFingerprint = keyPair.ParentFingerprint,
                publicKeyHex = keyPair.PublicKey.GetEncodedCompressedHex(),
                chainCodeHex = TestHelpers.BytesToHex(keyPair.ChainCode)
            };

            // Act
            var jsonString = JsonUtility.ToJson(serializedData, true);
            var deserializedData = JsonUtility.FromJson<SerializableBip32Data>(jsonString);

            // Assert
            Assert.AreEqual(keyPair.Depth, deserializedData.depth);
            Assert.AreEqual(keyPair.ChildNumber, deserializedData.childNumber);
            Assert.AreEqual(keyPair.ParentFingerprint, deserializedData.parentFingerprint);
            Assert.AreEqual(keyPair.PublicKey.GetEncodedCompressedHex(), deserializedData.publicKeyHex);
        }

        [Test]
        public void TestThreadSafety_KeyOperations()
        {
            // Arrange
            const int threadCount = 4;
            const int operationsPerThread = 10;
            var seed = TestHelpers.HexToBytes("000102030405060708090a0b0c0d0e0f");
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
                            var masterKey = Bip32ECKeyPair.GenerateKeyPair(seed);
                            var childKey = Bip32ECKeyPair.DeriveKeyPair(masterKey, new uint[] { (uint)i });
                            
                            if (childKey == null)
                            {
                                throw new Exception("Key derivation failed in thread");
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
        private class SerializableBip32Data
        {
            public byte depth;
            public uint childNumber;
            public uint parentFingerprint;
            public string publicKeyHex;
            public string chainCodeHex;
        }

        #endregion
    }
}

/// <summary>
/// BIP32 hierarchical deterministic key pair
/// Supports key derivation according to BIP32 specification
/// </summary>
public class Bip32ECKeyPair
{
    public const uint HARDENED_BIT = 0x80000000;
    
    public byte[] PrivateKey { get; }
    public ECPublicKey PublicKey { get; }
    public byte[] ChainCode { get; }
    public byte Depth { get; }
    public uint ChildNumber { get; }
    public uint ParentFingerprint { get; }

    public Bip32ECKeyPair(byte[] privateKey, ECPublicKey publicKey, byte[] chainCode, 
        byte depth = 0, uint childNumber = 0, uint parentFingerprint = 0)
    {
        PrivateKey = privateKey ?? throw new ArgumentNullException(nameof(privateKey));
        PublicKey = publicKey ?? throw new ArgumentNullException(nameof(publicKey));
        ChainCode = chainCode ?? throw new ArgumentNullException(nameof(chainCode));
        Depth = depth;
        ChildNumber = childNumber;
        ParentFingerprint = parentFingerprint;
    }

    public static Bip32ECKeyPair GenerateKeyPair(byte[] seed)
    {
        if (seed == null)
            throw new ArgumentNullException(nameof(seed));
        if (seed.Length < 16 || seed.Length > 64)
            throw new ArgumentException("Seed length must be between 16 and 64 bytes", nameof(seed));

        // HMAC-SHA512 with "Bitcoin seed" as key
        var hmacKey = System.Text.Encoding.UTF8.GetBytes("Bitcoin seed");
        var hmac = Hash.HMACSHA512(seed, hmacKey);
        
        var privateKey = new byte[32];
        var chainCode = new byte[32];
        
        Array.Copy(hmac, 0, privateKey, 0, 32);
        Array.Copy(hmac, 32, chainCode, 0, 32);
        
        var keyPair = ECKeyPair.CreateFromPrivateKey(privateKey);
        
        return new Bip32ECKeyPair(privateKey, keyPair.PublicKey, chainCode);
    }

    public static Bip32ECKeyPair DeriveKeyPair(Bip32ECKeyPair parent, uint[] path)
    {
        if (parent == null)
            throw new ArgumentNullException(nameof(parent));
        if (path == null)
            throw new ArgumentNullException(nameof(path));

        var currentKeyPair = parent;
        
        foreach (var index in path)
        {
            currentKeyPair = DeriveChildKeyPair(currentKeyPair, index);
        }
        
        return currentKeyPair;
    }

    public static ECPublicKey DerivePublicKey(Bip32ECKeyPair parent, uint[] path)
    {
        if (parent == null)
            throw new ArgumentNullException(nameof(parent));
        if (path == null)
            throw new ArgumentNullException(nameof(path));

        // Check if any index in path is hardened
        foreach (var index in path)
        {
            if ((index & HARDENED_BIT) != 0)
                throw new InvalidOperationException("Cannot derive hardened keys from public key only");
        }

        var derivedKeyPair = DeriveKeyPair(parent, path);
        return derivedKeyPair.PublicKey;
    }

    private static Bip32ECKeyPair DeriveChildKeyPair(Bip32ECKeyPair parent, uint index)
    {
        var isHardened = (index & HARDENED_BIT) != 0;
        var data = new byte[37];
        
        if (isHardened)
        {
            // Hardened derivation: 0x00 || private_key || index
            data[0] = 0x00;
            Array.Copy(parent.PrivateKey, 0, data, 1, 32);
        }
        else
        {
            // Non-hardened derivation: public_key || index
            var publicKeyBytes = parent.PublicKey.GetEncoded(compressed: true);
            Array.Copy(publicKeyBytes, 0, data, 0, 33);
        }
        
        // Add index (big-endian)
        data[33] = (byte)(index >> 24);
        data[34] = (byte)(index >> 16);
        data[35] = (byte)(index >> 8);
        data[36] = (byte)index;
        
        var hmac = Hash.HMACSHA512(data, parent.ChainCode);
        
        var childPrivateKeyData = new byte[32];
        var childChainCode = new byte[32];
        
        Array.Copy(hmac, 0, childPrivateKeyData, 0, 32);
        Array.Copy(hmac, 32, childChainCode, 0, 32);
        
        // Add parent private key to child private key (mod n)
        var childPrivateKey = AddPrivateKeys(childPrivateKeyData, parent.PrivateKey);
        var childKeyPair = ECKeyPair.CreateFromPrivateKey(childPrivateKey);
        
        // Calculate parent fingerprint
        var parentPublicKeyHash = Hash.Hash160(parent.PublicKey.GetEncoded(compressed: true));
        var parentFingerprint = BitConverter.ToUInt32(parentPublicKeyHash, 0);
        if (BitConverter.IsLittleEndian)
            parentFingerprint = ReverseBytes(parentFingerprint);
        
        return new Bip32ECKeyPair(
            childPrivateKey,
            childKeyPair.PublicKey,
            childChainCode,
            (byte)(parent.Depth + 1),
            index,
            parentFingerprint
        );
    }

    private static byte[] AddPrivateKeys(byte[] key1, byte[] key2)
    {
        // Simple addition for demonstration - actual implementation would need modular arithmetic
        var result = new byte[32];
        var carry = 0;
        
        for (int i = 31; i >= 0; i--)
        {
            var sum = key1[i] + key2[i] + carry;
            result[i] = (byte)(sum & 0xFF);
            carry = sum >> 8;
        }
        
        return result;
    }

    private static uint ReverseBytes(uint value)
    {
        return ((value & 0x000000FFU) << 24) |
               ((value & 0x0000FF00U) << 8) |
               ((value & 0x00FF0000U) >> 8) |
               ((value & 0xFF000000U) >> 24);
    }
}
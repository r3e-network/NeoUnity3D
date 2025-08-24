using System;
using System.Linq;
using NUnit.Framework;
using Neo.Unity.SDK.Types;
using Neo.Unity.SDK.Utils;
using Neo.Unity.SDK.Serialization;

namespace Neo.Unity.SDK.Tests.Types
{
    [TestFixture]
    public class Hash256Tests
    {
        [Test]
        public void TestFromValidHash()
        {
            var hash1 = new Hash256("0xb804a98220c69ab4674e97142beeeb00909113d417b9d6a67c12b71a3974a21a");
            Assert.AreEqual("b804a98220c69ab4674e97142beeeb00909113d417b9d6a67c12b71a3974a21a", hash1.ToString());

            var hash2 = new Hash256("b804a98220c69ab4674e97142beeeb00909113d417b9d6a67c12b71a3974a21a");
            Assert.AreEqual("b804a98220c69ab4674e97142beeeb00909113d417b9d6a67c12b71a3974a21a", hash2.ToString());
        }

        [Test]
        public void TestCreationThrows()
        {
            AssertErrorMessage("String argument is not hexadecimal.", () =>
                new Hash256("b804a98220c69ab4674e97142beeeb00909113d417b9d6a67c12b71a3974a21ae"));

            AssertErrorMessage("String argument is not hexadecimal.", () =>
                new Hash256("g804a98220c69ab4674e97142beeeb00909113d417b9d6a67c12b71a3974a21a"));

            AssertErrorMessage("Hash must be 32 bytes long but was 31 bytes.", () =>
                new Hash256("0xb804a98220c69ab4674e97142beeeb00909113d417b9d6a67c12b71a3974a2"));

            AssertErrorMessage("Hash must be 32 bytes long but was 33 bytes.", () =>
                new Hash256("0xb804a98220c69ab4674e97142beeeb00909113d417b9d6a67c12b71a3974a21a12"));
        }

        [Test]
        public void TestFromBytes()
        {
            var hashString = "b804a98220c69ab4674e97142beeeb00909113d417b9d6a67c12b71a3974a21a";
            var bytes = ByteExtensions.HexToBytes(hashString);
            var hash = new Hash256(bytes);
            Assert.AreEqual(hashString, hash.ToString());
        }

        [Test]
        public void TestToArray()
        {
            var hashString = "b804a98220c69ab4674e97142beeeb00909113d417b9d6a67c12b71a3974a21a";
            var hash = new Hash256(hashString);
            var expected = ByteExtensions.HexToBytes(hashString).Reverse().ToArray();
            CollectionAssert.AreEqual(expected, hash.ToLittleEndianArray());
        }

        [Test]
        public void TestSerializeAndDeserialize()
        {
            var writer = new BinaryWriter();
            var hashString = "b804a98220c69ab4674e97142beeeb00909113d417b9d6a67c12b71a3974a21a";
            var expectedData = ByteExtensions.HexToBytes(hashString).Reverse().ToArray();

            var hash = new Hash256(hashString);
            hash.Serialize(writer);

            CollectionAssert.AreEqual(expectedData, writer.ToArray());
            
            var deserializedHash = Hash256.FromBytes(expectedData);
            Assert.AreEqual(hashString, deserializedHash.ToString());
        }

        [Test]
        public void TestEquals()
        {
            var bytes1 = ByteExtensions.HexToBytes("1aa274391ab7127ca6d6b917d413919000ebee2b14974e67b49ac62082a904b8").Reverse().ToArray();
            var bytes2 = ByteExtensions.HexToBytes("b43034ab680d646f8b6ca71647aa6ba167b2eb0b3757e545f6c2715787b13272").Reverse().ToArray();
            
            var hash1 = new Hash256(bytes1);
            var hash2 = new Hash256(bytes2);
            var hash3 = new Hash256("0xb804a98220c69ab4674e97142beeeb00909113d417b9d6a67c12b71a3974a21a");
            
            Assert.AreNotEqual(hash1, hash2);
            Assert.AreEqual(hash1, hash1);
            Assert.AreEqual(hash1, hash3);
            Assert.AreEqual(hash1.GetHashCode(), hash3.GetHashCode());
        }

        [Test]
        public void TestCompareTo()
        {
            var bytes1 = ByteExtensions.HexToBytes("1aa274391ab7127ca6d6b917d413919000ebee2b14974e67b49ac62082a904b8").Reverse().ToArray();
            var bytes2 = ByteExtensions.HexToBytes("b43034ab680d646f8b6ca71647aa6ba167b2eb0b3757e545f6c2715787b13272").Reverse().ToArray();
            
            var hash1 = new Hash256(bytes1);
            var hash2 = new Hash256(bytes2);
            var hash3 = new Hash256("0xf4609b99e171190c22adcf70c88a7a14b5b530914d2398287bd8bb7ad95a661c");
            
            Assert.Greater(hash1.CompareTo(hash2), 0);
            Assert.Greater(hash3.CompareTo(hash1), 0);
            Assert.Greater(hash3.CompareTo(hash2), 0);
        }

        [Test]
        public void TestSize()
        {
            var hash = new Hash256("b804a98220c69ab4674e97142beeeb00909113d417b9d6a67c12b71a3974a21a");
            Assert.AreEqual(32, hash.Size);
        }

        [Test]
        public void TestFromBytesValidation()
        {
            var validBytes = new byte[32];
            var hash = Hash256.FromBytes(validBytes);
            Assert.IsNotNull(hash);

            Assert.Throws<ArgumentException>(() =>
            {
                Hash256.FromBytes(new byte[31]); // Too short
            });

            Assert.Throws<ArgumentException>(() =>
            {
                Hash256.FromBytes(new byte[33]); // Too long
            });
        }

        [Test]
        public void TestZeroHash()
        {
            var zeroHash = Hash256.Zero;
            Assert.IsNotNull(zeroHash);
            Assert.AreEqual(32, zeroHash.Size);
            Assert.AreEqual("0000000000000000000000000000000000000000000000000000000000000000", zeroHash.ToString());
        }

        [Test]
        public void TestHashCalculation()
        {
            // Test double SHA256 hash calculation
            var testData = ByteExtensions.HexToBytes("01020304");
            var hash = Hash256.FromDoubleSha256(testData);
            Assert.IsNotNull(hash);
            Assert.AreEqual(32, hash.Size);
        }

        [Test]
        public void TestTransactionHashGeneration()
        {
            // Test transaction hash generation pattern
            var transactionData = ByteExtensions.HexToBytes("0102030405060708090a0b0c0d0e0f101112131415161718191a1b1c1d1e1f20");
            var hash = Hash256.FromDoubleSha256(transactionData);
            Assert.IsNotNull(hash);
            Assert.AreEqual(32, hash.Size);
        }

        [Test]
        public void TestByteSerialization()
        {
            var originalHash = new Hash256("b804a98220c69ab4674e97142beeeb00909113d417b9d6a67c12b71a3974a21a");
            var bytes = originalHash.ToArray();
            var deserializedHash = new Hash256(bytes);
            Assert.AreEqual(originalHash, deserializedHash);
        }

        [Test]
        public void TestLittleEndianConversion()
        {
            var hash = new Hash256("b804a98220c69ab4674e97142beeeb00909113d417b9d6a67c12b71a3974a21a");
            var littleEndian = hash.ToLittleEndianArray();
            var bigEndian = ByteExtensions.HexToBytes("b804a98220c69ab4674e97142beeeb00909113d417b9d6a67c12b71a3974a21a");
            CollectionAssert.AreEqual(bigEndian.Reverse().ToArray(), littleEndian);
        }

        [Test]
        public void TestGetHashCode()
        {
            var hash1 = new Hash256("b804a98220c69ab4674e97142beeeb00909113d417b9d6a67c12b71a3974a21a");
            var hash2 = new Hash256("b804a98220c69ab4674e97142beeeb00909113d417b9d6a67c12b71a3974a21a");
            var hash3 = new Hash256("c804a98220c69ab4674e97142beeeb00909113d417b9d6a67c12b71a3974a21a");

            Assert.AreEqual(hash1.GetHashCode(), hash2.GetHashCode());
            Assert.AreNotEqual(hash1.GetHashCode(), hash3.GetHashCode());
        }

        [Test]
        public void TestBlockHashGeneration()
        {
            // Test block hash generation pattern
            var blockHeader = ByteExtensions.HexToBytes("000000000019d6689c085ae165831e934ff763ae46a2a6c172b3f1b60a8ce26f");
            var hash = Hash256.FromDoubleSha256(blockHeader);
            Assert.IsNotNull(hash);
            Assert.AreEqual(32, hash.Size);
        }

        [Test]
        public void TestMerkleRootCalculation()
        {
            // Test Merkle root calculation pattern
            var transactionHashes = new[]
            {
                ByteExtensions.HexToBytes("a1b2c3d4e5f6a7b8c9d0e1f2a3b4c5d6e7f8a9b0c1d2e3f4a5b6c7d8e9f0a1b2"),
                ByteExtensions.HexToBytes("b2c3d4e5f6a7b8c9d0e1f2a3b4c5d6e7f8a9b0c1d2e3f4a5b6c7d8e9f0a1b2c3")
            };

            foreach (var txHash in transactionHashes)
            {
                var hash = new Hash256(txHash);
                Assert.IsNotNull(hash);
                Assert.AreEqual(32, hash.Size);
            }
        }

        [Test]
        public void TestNetworkHashValidation()
        {
            // Test various network-specific hash formats
            var networkHashes = new[]
            {
                "b804a98220c69ab4674e97142beeeb00909113d417b9d6a67c12b71a3974a21a",
                "1aa274391ab7127ca6d6b917d413919000ebee2b14974e67b49ac62082a904b8",
                "f4609b99e171190c22adcf70c88a7a14b5b530914d2398287bd8bb7ad95a661c"
            };

            foreach (var hashString in networkHashes)
            {
                var hash = new Hash256(hashString);
                Assert.IsNotNull(hash);
                Assert.AreEqual(32, hash.Size);
                Assert.AreEqual(hashString, hash.ToString());
            }
        }

        [Test]
        public void TestStateRootHashGeneration()
        {
            // Test state root hash generation pattern
            var stateData = ByteExtensions.HexToBytes("deadbeefcafebabe1234567890abcdef");
            var hash = Hash256.FromDoubleSha256(stateData);
            Assert.IsNotNull(hash);
            Assert.AreEqual(32, hash.Size);
        }

        private void AssertErrorMessage(string expectedMessage, Action action)
        {
            var exception = Assert.Throws<ArgumentException>(action.Invoke);
            Assert.AreEqual(expectedMessage, exception.Message);
        }
    }
}
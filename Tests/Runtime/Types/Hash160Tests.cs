using System;
using System.Linq;
using NUnit.Framework;
using Neo.Unity.SDK.Types;
using Neo.Unity.SDK.Crypto;
using Neo.Unity.SDK.Utils;
using Neo.Unity.SDK.Serialization;
using Neo.Unity.SDK.Script;

namespace Neo.Unity.SDK.Tests.Types
{
    [TestFixture]
    public class Hash160Tests
    {
        private const string DefaultAccountPublicKey = "033a4d051b04b7fc0230d2b1aaedfd5a84be279a5361a7358db665ad7857787f1b";
        private const string DefaultAccountAddress = "NM7Aky765FG8NhhwtxjXRx7jEL1cnw7PBP";
        private const string CommitteeAccountScriptHash = "05859de95ccbbd5668e0f055b208273634d4657f";

        [Test]
        public void TestFromValidHash()
        {
            var hash1 = new Hash160("0x23ba2703c53263e8d6e522dc32203339dcd8eee9");
            Assert.AreEqual("23ba2703c53263e8d6e522dc32203339dcd8eee9", hash1.ToString());

            var hash2 = new Hash160("23ba2703c53263e8d6e522dc32203339dcd8eee9");
            Assert.AreEqual("23ba2703c53263e8d6e522dc32203339dcd8eee9", hash2.ToString());
        }

        [Test]
        public void TestCreationThrows()
        {
            AssertErrorMessage("String argument is not hexadecimal.", () =>
                new Hash160("0x23ba2703c53263e8d6e522dc32203339dcd8eee"));

            AssertErrorMessage("String argument is not hexadecimal.", () =>
                new Hash160("g3ba2703c53263e8d6e522dc32203339dcd8eee9"));

            AssertErrorMessage("Hash must be 20 bytes long but was 19 bytes.", () =>
                new Hash160("23ba2703c53263e8d6e522dc32203339dcd8ee"));

            AssertErrorMessage("Hash must be 32 bytes long but was 32 bytes.", () =>
                new Hash160("c56f33fc6ecfcd0c225c4ab356fee59390af8560be0e930faebe74a6daff7c9b"));
        }

        [Test]
        public void TestToArray()
        {
            var hash = new Hash160("23ba2703c53263e8d6e522dc32203339dcd8eee9");
            var expected = ByteExtensions.HexToBytes("23ba2703c53263e8d6e522dc32203339dcd8eee9").Reverse().ToArray();
            CollectionAssert.AreEqual(expected, hash.ToLittleEndianArray());
        }

        [Test]
        public void TestSerializeAndDeserialize()
        {
            var writer = new BinaryWriter();
            var hashString = "23ba2703c53263e8d6e522dc32203339dcd8eee9";
            var expectedData = ByteExtensions.HexToBytes(hashString).Reverse().ToArray();

            var hash = new Hash160(hashString);
            hash.Serialize(writer);

            CollectionAssert.AreEqual(expectedData, writer.ToArray());
            
            var deserializedHash = Hash160.FromBytes(expectedData);
            Assert.AreEqual(hashString, deserializedHash.ToString());
        }

        [Test]
        public void TestEquals()
        {
            var hash1 = Hash160.FromScript(ByteExtensions.HexToBytes("01a402d8"));
            var hash2 = Hash160.FromScript(ByteExtensions.HexToBytes("d802a401"));
            Assert.AreNotEqual(hash1, hash2);
            Assert.AreEqual(hash1, hash1);
        }

        [Test]
        public void TestFromValidAddress()
        {
            var hash = Hash160.FromAddress("NLnyLtep7jwyq1qhNPkwXbJpurC4jUT8ke");
            var expectedHash = ByteExtensions.HexToBytes("09a55874c2da4b86e5d49ff530a1b153eb12c7d6");
            CollectionAssert.AreEqual(expectedHash, hash.ToLittleEndianArray());
        }

        [Test]
        public void TestFromInvalidAddress()
        {
            AssertErrorMessage("Not a valid NEO address.", () =>
                Hash160.FromAddress("NLnyLtep7jwyq1qhNPkwXbJpurC4jUT8keas"));
        }

        [Test]
        public void TestFromPublicKeyBytes()
        {
            var key = "035fdb1d1f06759547020891ae97c729327853aeb1256b6fe0473bc2e9fa42ff50";
            var script = $"{OpCode.PUSHDATA1.ToString()}21{key}{OpCode.SYSCALL.ToString()}{InteropService.SystemCryptoCheckSig.Hash}";
            var hash = Hash160.FromPublicKey(ByteExtensions.HexToBytes(key));
            var expectedHash = Hash.Sha256ThenRipemd160(ByteExtensions.HexToBytes(script));
            CollectionAssert.AreEqual(expectedHash, hash.ToLittleEndianArray());

            var publicKey = new ECPublicKey(DefaultAccountPublicKey);
            var hash2 = Hash160.FromPublicKeys(new[] { publicKey }, 1);
            Assert.AreEqual(CommitteeAccountScriptHash, hash2.ToString());
        }

        [Test]
        public void TestFromContractScript()
        {
            var scriptHex = "110c21026aa8fe6b4360a67a530e23c08c6a72525afde34719c5436f9d3ced759f939a3d110b41138defaf";
            var hash = Hash160.FromScript(ByteExtensions.HexToBytes(scriptHex));
            Assert.AreEqual("afaed076854454449770763a628f379721ea9808", hash.ToString());
            Assert.AreEqual("0898ea2197378f623a7670974454448576d0aeaf", hash.ToLittleEndianArray().ToHexString());
        }

        [Test]
        public void TestToAddress()
        {
            var hash = Hash160.FromPublicKey(ByteExtensions.HexToBytes(DefaultAccountPublicKey));
            Assert.AreEqual(DefaultAccountAddress, hash.ToAddress());
        }

        [Test]
        public void TestCompareTo()
        {
            var hash1 = Hash160.FromScript(ByteExtensions.HexToBytes("01a402d8"));
            var hash2 = Hash160.FromScript(ByteExtensions.HexToBytes("d802a401"));
            var hash3 = Hash160.FromScript(ByteExtensions.HexToBytes("a7b3a191"));
            
            Assert.Greater(hash2.CompareTo(hash1), 0);
            Assert.Greater(hash3.CompareTo(hash1), 0);
            Assert.Greater(hash2.CompareTo(hash3), 0);
        }

        [Test]
        public void TestSize()
        {
            var hash = new Hash160("23ba2703c53263e8d6e522dc32203339dcd8eee9");
            Assert.AreEqual(20, hash.Size);
        }

        [Test]
        public void TestHashCalculation()
        {
            // Test hash calculation from various sources
            var testData = ByteExtensions.HexToBytes("01020304");
            var hash = Hash160.FromScript(testData);
            Assert.IsNotNull(hash);
            Assert.AreEqual(20, hash.Size);
        }

        [Test]
        public void TestAddressConversion()
        {
            var originalAddress = "NM7Aky765FG8NhhwtxjXRx7jEL1cnw7PBP";
            var hash = Hash160.FromAddress(originalAddress);
            var convertedAddress = hash.ToAddress();
            Assert.AreEqual(originalAddress, convertedAddress);
        }

        [Test]
        public void TestScriptHashGeneration()
        {
            var publicKeyBytes = ByteExtensions.HexToBytes(DefaultAccountPublicKey);
            var hash = Hash160.FromPublicKey(publicKeyBytes);
            Assert.IsNotNull(hash);
            Assert.AreEqual(20, hash.Size);
        }

        [Test]
        public void TestMultiSigScriptHash()
        {
            var publicKeys = new[]
            {
                new ECPublicKey(DefaultAccountPublicKey)
            };
            var hash = Hash160.FromPublicKeys(publicKeys, 1);
            Assert.IsNotNull(hash);
            Assert.AreEqual(20, hash.Size);
        }

        [Test]
        public void TestByteSerialization()
        {
            var originalHash = new Hash160("23ba2703c53263e8d6e522dc32203339dcd8eee9");
            var bytes = originalHash.ToArray();
            var deserializedHash = new Hash160(bytes);
            Assert.AreEqual(originalHash, deserializedHash);
        }

        [Test]
        public void TestLittleEndianConversion()
        {
            var hash = new Hash160("23ba2703c53263e8d6e522dc32203339dcd8eee9");
            var littleEndian = hash.ToLittleEndianArray();
            var bigEndian = ByteExtensions.HexToBytes("23ba2703c53263e8d6e522dc32203339dcd8eee9");
            CollectionAssert.AreEqual(bigEndian.Reverse().ToArray(), littleEndian);
        }

        [Test]
        public void TestFromBytesValidation()
        {
            var validBytes = new byte[20];
            var hash = Hash160.FromBytes(validBytes);
            Assert.IsNotNull(hash);

            Assert.Throws<ArgumentException>(() =>
            {
                Hash160.FromBytes(new byte[19]); // Too short
            });

            Assert.Throws<ArgumentException>(() =>
            {
                Hash160.FromBytes(new byte[21]); // Too long
            });
        }

        [Test]
        public void TestGetHashCode()
        {
            var hash1 = new Hash160("23ba2703c53263e8d6e522dc32203339dcd8eee9");
            var hash2 = new Hash160("23ba2703c53263e8d6e522dc32203339dcd8eee9");
            var hash3 = new Hash160("33ba2703c53263e8d6e522dc32203339dcd8eee9");

            Assert.AreEqual(hash1.GetHashCode(), hash2.GetHashCode());
            Assert.AreNotEqual(hash1.GetHashCode(), hash3.GetHashCode());
        }

        [Test]
        public void TestZeroHash()
        {
            var zeroHash = Hash160.Zero;
            Assert.IsNotNull(zeroHash);
            Assert.AreEqual(20, zeroHash.Size);
            Assert.AreEqual("0000000000000000000000000000000000000000", zeroHash.ToString());
        }

        private void AssertErrorMessage(string expectedMessage, Action action)
        {
            var exception = Assert.Throws<ArgumentException>(action.Invoke);
            Assert.AreEqual(expectedMessage, exception.Message);
        }
    }
}
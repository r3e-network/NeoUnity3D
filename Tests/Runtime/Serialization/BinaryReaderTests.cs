using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Neo.Unity.SDK;
using Neo.Unity.SDK.Serialization;
using Neo.Unity.SDK.Tests.Helpers;
using System.Collections;

namespace Neo.Unity.SDK.Tests.Serialization
{
    /// <summary>
    /// Unity Test Framework implementation of BinaryReader tests
    /// Converted from Swift BinaryReaderTests.swift with Unity-specific enhancements
    /// </summary>
    [TestFixture]
    public class BinaryReaderTests
    {
        [Test]
        public void TestReadPushDataBytes()
        {
            // Arrange
            var prefixCountMap = new Dictionary<string, int>
            {
                ["0c01"] = 1,
                ["0cff"] = 255,
                ["0d0001"] = 256,
                ["0d0010"] = 4096,
                ["0e00000100"] = 65536
            };

            // Act & Assert
            foreach (var kvp in prefixCountMap)
            {
                var prefix = kvp.Key;
                var count = kvp.Value;
                var bytes = new byte[count];
                for (int i = 0; i < count; i++)
                {
                    bytes[i] = 0x01;
                }

                var input = new byte[TestHelpers.HexToBytes(prefix).Length + bytes.Length];
                Array.Copy(TestHelpers.HexToBytes(prefix), 0, input, 0, TestHelpers.HexToBytes(prefix).Length);
                Array.Copy(bytes, 0, input, TestHelpers.HexToBytes(prefix).Length, bytes.Length);

                ReadPushDataBytesAndAssert(input, bytes);
            }
        }

        [Test]
        public void TestFailReadPushData()
        {
            // Arrange
            var data = new List<byte>();
            data.AddRange(TestHelpers.HexToBytes("4b"));
            data.Add(0x01);
            data.AddRange(TestHelpers.HexToBytes("0000"));

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() =>
            {
                var reader = new BinaryReader(data.ToArray());
                reader.ReadPushData();
            });
        }

        [Test]
        public void TestReadPushDataString()
        {
            // Act & Assert
            ReadPushDataStringAndAssert(TestHelpers.HexToBytes("0c00"), "");
            ReadPushDataStringAndAssert(TestHelpers.HexToBytes("0c0161"), "a");

            var bytes = new byte[10000];
            var input = new byte[TestHelpers.HexToBytes("0e10270000").Length + bytes.Length];
            Array.Copy(TestHelpers.HexToBytes("0e10270000"), 0, input, 0, TestHelpers.HexToBytes("0e10270000").Length);
            Array.Copy(bytes, 0, input, TestHelpers.HexToBytes("0e10270000").Length, bytes.Length);

            var expectedString = Encoding.UTF8.GetString(bytes);
            ReadPushDataStringAndAssert(input, expectedString);
        }

        [Test]
        public void TestReadPushDataBigInteger()
        {
            // Act & Assert
            ReadPushDataIntegerAndAssert(TestHelpers.HexToBytes("10"), BigInteger.Zero);
            ReadPushDataIntegerAndAssert(TestHelpers.HexToBytes("11"), BigInteger.One);
            ReadPushDataIntegerAndAssert(TestHelpers.HexToBytes("0f"), new BigInteger(-1));
            ReadPushDataIntegerAndAssert(TestHelpers.HexToBytes("20"), new BigInteger(16));
        }

        [Test]
        public void TestReadUInt32()
        {
            // Act & Assert
            ReadUInt32AndAssert(new byte[] { 0xff, 0xff, 0xff, 0xff }, 4_294_967_295);
            ReadUInt32AndAssert(new byte[] { 0x01, 0x00, 0x00, 0x00 }, 1);
            ReadUInt32AndAssert(new byte[] { 0x00, 0x00, 0x00, 0x00 }, 0);
            ReadUInt32AndAssert(new byte[] { 0x8c, 0xae, 0x00, 0x00, 0xff }, 44_684);
        }

        [Test]
        public void TestReadInt64()
        {
            // Act & Assert
            ReadInt64AndAssert(new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x80 }, long.MinValue);
            ReadInt64AndAssert(new byte[] { 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0x7f }, long.MaxValue);
            ReadInt64AndAssert(new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 }, 0);
            ReadInt64AndAssert(new byte[] { 0x11, 0x33, 0x22, 0x8c, 0xae, 0x00, 0x00, 0x00, 0xff }, 749_675_361_041);
        }

        [Test]
        public void TestReadByte()
        {
            // Arrange
            var data = new byte[] { 0x42, 0xff, 0x00, 0x7f };
            var reader = new BinaryReader(data);

            // Act & Assert
            Assert.AreEqual(0x42, reader.ReadByte());
            Assert.AreEqual(0xff, reader.ReadByte());
            Assert.AreEqual(0x00, reader.ReadByte());
            Assert.AreEqual(0x7f, reader.ReadByte());
        }

        [Test]
        public void TestReadBytes()
        {
            // Arrange
            var data = new byte[] { 0x01, 0x02, 0x03, 0x04, 0x05 };
            var reader = new BinaryReader(data);

            // Act
            var result1 = reader.ReadBytes(3);
            var result2 = reader.ReadBytes(2);

            // Assert
            TestHelpers.AssertBytesEqual(new byte[] { 0x01, 0x02, 0x03 }, result1);
            TestHelpers.AssertBytesEqual(new byte[] { 0x04, 0x05 }, result2);
        }

        [Test]
        public void TestReadUInt16()
        {
            // Act & Assert
            ReadUInt16AndAssert(new byte[] { 0xff, 0xff }, 65535);
            ReadUInt16AndAssert(new byte[] { 0x01, 0x00 }, 1);
            ReadUInt16AndAssert(new byte[] { 0x00, 0x00 }, 0);
            ReadUInt16AndAssert(new byte[] { 0x34, 0x12 }, 0x1234);
        }

        [Test]
        public void TestReadVarInt()
        {
            // Test various VarInt encodings
            ReadVarIntAndAssert(new byte[] { 0xfc }, 252);
            ReadVarIntAndAssert(new byte[] { 0xfd, 0x00, 0x01 }, 256);
            ReadVarIntAndAssert(new byte[] { 0xfe, 0x00, 0x00, 0x01, 0x00 }, 65536);
            ReadVarIntAndAssert(new byte[] { 0xff, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00 }, 4294967296);
        }

        [Test]
        public void TestReadVarString()
        {
            // Arrange & Act & Assert
            ReadVarStringAndAssert(new byte[] { 0x00 }, "");
            ReadVarStringAndAssert(new byte[] { 0x05, 0x48, 0x65, 0x6c, 0x6c, 0x6f }, "Hello");
            
            // Test with UTF-8 encoding
            var utf8Bytes = Encoding.UTF8.GetBytes("Neo币");
            var varStringData = new byte[1 + utf8Bytes.Length];
            varStringData[0] = (byte)utf8Bytes.Length;
            Array.Copy(utf8Bytes, 0, varStringData, 1, utf8Bytes.Length);
            ReadVarStringAndAssert(varStringData, "Neo币");
        }

        [Test]
        public void TestReadBoolean()
        {
            // Arrange
            var data = new byte[] { 0x01, 0x00, 0xff, 0x42 };
            var reader = new BinaryReader(data);

            // Act & Assert
            Assert.IsTrue(reader.ReadBoolean());
            Assert.IsFalse(reader.ReadBoolean());
            Assert.IsTrue(reader.ReadBoolean()); // Non-zero is true
            Assert.IsTrue(reader.ReadBoolean()); // Non-zero is true
        }

        [Test]
        public void TestPosition()
        {
            // Arrange
            var data = new byte[] { 0x01, 0x02, 0x03, 0x04 };
            var reader = new BinaryReader(data);

            // Act & Assert
            Assert.AreEqual(0, reader.Position);
            
            reader.ReadByte();
            Assert.AreEqual(1, reader.Position);
            
            reader.ReadUInt16();
            Assert.AreEqual(3, reader.Position);
            
            reader.ReadByte();
            Assert.AreEqual(4, reader.Position);
        }

        [Test]
        public void TestAvailable()
        {
            // Arrange
            var data = new byte[] { 0x01, 0x02, 0x03, 0x04 };
            var reader = new BinaryReader(data);

            // Act & Assert
            Assert.AreEqual(4, reader.Available);
            
            reader.ReadByte();
            Assert.AreEqual(3, reader.Available);
            
            reader.ReadBytes(2);
            Assert.AreEqual(1, reader.Available);
            
            reader.ReadByte();
            Assert.AreEqual(0, reader.Available);
        }

        [Test]
        public void TestEndOfStream()
        {
            // Arrange
            var data = new byte[] { 0x01 };
            var reader = new BinaryReader(data);

            // Act & Assert
            Assert.IsFalse(reader.IsAtEnd);
            
            reader.ReadByte();
            Assert.IsTrue(reader.IsAtEnd);

            // Trying to read beyond stream should throw
            Assert.Throws<InvalidOperationException>(() => reader.ReadByte());
        }

        #region Unity-Specific Tests

        [Test]
        [Performance]
        public void TestPerformance_LargeDataReading()
        {
            // Arrange
            const int dataSize = 100000;
            var data = TestHelpers.GenerateRandomBytes(dataSize);

            // Act & Assert
            var executionTime = TestHelpers.MeasureExecutionTime(() =>
            {
                var reader = new BinaryReader(data);
                while (reader.Available > 0)
                {
                    reader.ReadByte();
                }
            });

            Assert.Less(executionTime, 1000, "Large data reading should complete within 1 second");
            Debug.Log($"Large data reading time: {executionTime}ms for {dataSize} bytes");
        }

        [Test]
        public void TestMemoryUsage_BinaryReader()
        {
            const int iterations = 100;
            const int dataSize = 1000;

            // Act & Assert
            var memoryUsage = TestHelpers.MeasureMemoryUsage(() =>
            {
                var readers = new BinaryReader[iterations];
                for (int i = 0; i < iterations; i++)
                {
                    var data = TestHelpers.GenerateRandomBytes(dataSize);
                    readers[i] = new BinaryReader(data);
                }
                GC.KeepAlive(readers);
            });

            var avgMemoryPerReader = memoryUsage / iterations;
            Assert.Less(Math.Abs(avgMemoryPerReader), 10 * 1024, "Each BinaryReader should use less than 10KB");
            Debug.Log($"Memory usage per BinaryReader: {avgMemoryPerReader} bytes");
        }

        [UnityTest]
        public IEnumerator TestUnityCoroutineCompatibility()
        {
            // Arrange
            var data = TestHelpers.GenerateRandomBytes(1000);
            bool completed = false;
            byte[] result = null;

            // Act - Simulate async reading in coroutine
            yield return new WaitForEndOfFrame();

            var reader = new BinaryReader(data);
            result = reader.ReadBytes(100);
            completed = true;

            yield return new WaitUntil(() => completed);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(100, result.Length);
        }

        [Test]
        public void TestSerializationForUnityInspector()
        {
            // Create a serializable wrapper for Unity Inspector
            var data = new byte[] { 0x01, 0x02, 0x03, 0x04 };
            var serializableReaderData = new SerializableBinaryReaderData
            {
                dataHex = TestHelpers.BytesToHex(data),
                position = 0,
                length = data.Length
            };

            var jsonString = JsonUtility.ToJson(serializableReaderData, true);
            Assert.IsNotNull(jsonString);
            Assert.IsTrue(jsonString.Contains("01020304"));

            Debug.Log($"Serializable binary reader data: {jsonString}");

            // Test deserialization
            var deserializedData = JsonUtility.FromJson<SerializableBinaryReaderData>(jsonString);
            Assert.AreEqual("01020304", deserializedData.dataHex);
            Assert.AreEqual(0, deserializedData.position);
            Assert.AreEqual(4, deserializedData.length);
        }

        [Test]
        public void TestThreadSafety()
        {
            // Arrange
            const int threadCount = 4;
            const int operationsPerThread = 100;
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
                            var data = TestHelpers.GenerateRandomBytes(100);
                            var reader = new BinaryReader(data);
                            
                            // Read various data types
                            if (reader.Available >= 4) reader.ReadUInt32();
                            if (reader.Available >= 8) reader.ReadInt64();
                            if (reader.Available >= 2) reader.ReadUInt16();
                            if (reader.Available >= 1) reader.ReadByte();
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
        public void TestEdgeCases_BoundaryConditions()
        {
            // Test null data
            Assert.Throws<ArgumentNullException>(() => new BinaryReader(null));

            // Test empty data
            var emptyReader = new BinaryReader(new byte[0]);
            Assert.AreEqual(0, emptyReader.Available);
            Assert.IsTrue(emptyReader.IsAtEnd);

            // Test reading beyond bounds
            var singleByteReader = new BinaryReader(new byte[] { 0x01 });
            singleByteReader.ReadByte();
            Assert.Throws<InvalidOperationException>(() => singleByteReader.ReadByte());
            Assert.Throws<InvalidOperationException>(() => singleByteReader.ReadBytes(1));
        }

        [Test]
        public void TestDataIntegrity_RoundTripSerialization()
        {
            // Test that data written by BinaryWriter can be read by BinaryReader
            var originalData = new object[]
            {
                (byte)42,
                (ushort)1234,
                (uint)567890,
                (long)123456789012345,
                true,
                false,
                "Hello, Neo!"
            };

            // Write data
            var writer = new BinaryWriter();
            writer.WriteByte((byte)originalData[0]);
            writer.WriteUInt16((ushort)originalData[1]);
            writer.WriteUInt32((uint)originalData[2]);
            writer.WriteInt64((long)originalData[3]);
            writer.WriteBoolean((bool)originalData[4]);
            writer.WriteBoolean((bool)originalData[5]);
            writer.WriteVarString((string)originalData[6]);

            var serializedData = writer.ToArray();

            // Read data back
            var reader = new BinaryReader(serializedData);
            Assert.AreEqual((byte)originalData[0], reader.ReadByte());
            Assert.AreEqual((ushort)originalData[1], reader.ReadUInt16());
            Assert.AreEqual((uint)originalData[2], reader.ReadUInt32());
            Assert.AreEqual((long)originalData[3], reader.ReadInt64());
            Assert.AreEqual((bool)originalData[4], reader.ReadBoolean());
            Assert.AreEqual((bool)originalData[5], reader.ReadBoolean());
            Assert.AreEqual((string)originalData[6], reader.ReadVarString());

            Assert.IsTrue(reader.IsAtEnd);
        }

        #endregion

        #region Helper Methods

        private void ReadUInt32AndAssert(byte[] input, uint expected)
        {
            var reader = new BinaryReader(input);
            Assert.AreEqual(expected, reader.ReadUInt32());
        }

        private void ReadInt64AndAssert(byte[] input, long expected)
        {
            var reader = new BinaryReader(input);
            Assert.AreEqual(expected, reader.ReadInt64());
        }

        private void ReadUInt16AndAssert(byte[] input, ushort expected)
        {
            var reader = new BinaryReader(input);
            Assert.AreEqual(expected, reader.ReadUInt16());
        }

        private void ReadVarIntAndAssert(byte[] input, ulong expected)
        {
            var reader = new BinaryReader(input);
            Assert.AreEqual(expected, reader.ReadVarInt());
        }

        private void ReadVarStringAndAssert(byte[] input, string expected)
        {
            var reader = new BinaryReader(input);
            Assert.AreEqual(expected, reader.ReadVarString());
        }

        private void ReadPushDataBytesAndAssert(byte[] input, byte[] expected)
        {
            var reader = new BinaryReader(input);
            var result = reader.ReadPushData();
            TestHelpers.AssertBytesEqual(expected, result);
        }

        private void ReadPushDataStringAndAssert(byte[] input, string expected)
        {
            var reader = new BinaryReader(input);
            Assert.AreEqual(expected, reader.ReadPushString());
        }

        private void ReadPushDataIntegerAndAssert(byte[] input, BigInteger expected)
        {
            var reader = new BinaryReader(input);
            Assert.AreEqual(expected, reader.ReadPushBigInt());
        }

        #endregion

        #region Helper Classes

        [System.Serializable]
        private class SerializableBinaryReaderData
        {
            public string dataHex;
            public int position;
            public int length;
        }

        #endregion
    }
}
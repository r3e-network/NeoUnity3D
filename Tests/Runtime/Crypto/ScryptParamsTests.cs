using System;
using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Neo.Unity.SDK;
using Neo.Unity.SDK.Crypto;
using Neo.Unity.SDK.Tests.Helpers;
using Newtonsoft.Json;

namespace Neo.Unity.SDK.Tests.Crypto
{
    /// <summary>
    /// Unity Test Framework implementation of Scrypt parameters serialization tests
    /// Converted from Swift ScryptParamsTests.swift with Unity-specific enhancements
    /// </summary>
    [TestFixture]
    public class ScryptParamsTests
    {
        private ScryptParams _testParams;

        [SetUp]
        public void Setup()
        {
            _testParams = new ScryptParams(7, 8, 9);
        }

        [Test]
        public void TestSerialize()
        {
            // Act
            var jsonString = JsonConvert.SerializeObject(_testParams);

            // Assert
            Assert.AreEqual("{\"n\":7,\"r\":8,\"p\":9}", jsonString);
        }

        [Test]
        public void TestDeserialize()
        {
            // Arrange - Various JSON format variations
            var jsonStrings = new[]
            {
                "{\"n\":7,\"r\":8,\"p\":9}",
                "{\"n\":7,\"blockSize\":8,\"p\":9}",
                "{\"n\":7,\"blockSize\":8,\"parallel\":9}",
                "{\"n\":7,\"r\":8,\"parallel\":9}",
                "{\"n\":7,\"blocksize\":8,\"p\":9}",
                "{\"n\":7,\"blocksize\":8,\"parallel\":9}",
                "{\"cost\":7,\"r\":8,\"p\":9}",
                "{\"cost\":7,\"r\":8,\"parallel\":9}",
                "{\"cost\":7,\"blockSize\":8,\"p\":9}",
                "{\"cost\":7,\"blockSize\":8,\"parallel\":9}",
                "{\"cost\":7,\"blocksize\":8,\"p\":9}",
                "{\"cost\":7,\"blocksize\":8,\"parallel\":9}"
            };

            foreach (var jsonString in jsonStrings)
            {
                // Act
                var deserializedParams = JsonConvert.DeserializeObject<ScryptParams>(jsonString);

                // Assert
                Assert.AreEqual(_testParams.N, deserializedParams.N, $"Failed for JSON: {jsonString}");
                Assert.AreEqual(_testParams.R, deserializedParams.R, $"Failed for JSON: {jsonString}");
                Assert.AreEqual(_testParams.P, deserializedParams.P, $"Failed for JSON: {jsonString}");
            }
        }

        [Test]
        public void TestEquality()
        {
            // Arrange
            var params1 = new ScryptParams(7, 8, 9);
            var params2 = new ScryptParams(7, 8, 9);
            var params3 = new ScryptParams(8, 8, 9);

            // Assert
            Assert.AreEqual(params1, params2);
            Assert.AreNotEqual(params1, params3);
            Assert.AreEqual(params1.GetHashCode(), params2.GetHashCode());
        }

        [Test]
        public void TestDefaultParameters()
        {
            // Act
            var defaultParams = ScryptParams.Default;

            // Assert
            Assert.AreEqual(16384, defaultParams.N);
            Assert.AreEqual(8, defaultParams.R);
            Assert.AreEqual(8, defaultParams.P);
        }

        [Test]
        public void TestParameterValidation()
        {
            // Test valid parameters
            Assert.DoesNotThrow(() => new ScryptParams(1024, 8, 8));
            Assert.DoesNotThrow(() => new ScryptParams(16384, 8, 8));
            Assert.DoesNotThrow(() => new ScryptParams(32768, 8, 8));

            // Test invalid parameters
            Assert.Throws<ArgumentException>(() => new ScryptParams(0, 8, 8));
            Assert.Throws<ArgumentException>(() => new ScryptParams(-1, 8, 8));
            Assert.Throws<ArgumentException>(() => new ScryptParams(1024, 0, 8));
            Assert.Throws<ArgumentException>(() => new ScryptParams(1024, 8, 0));
        }

        [Test]
        public void TestNonPowerOfTwoN()
        {
            // N parameter should typically be a power of 2
            Assert.Throws<ArgumentException>(() => new ScryptParams(1000, 8, 8));
            Assert.Throws<ArgumentException>(() => new ScryptParams(15, 8, 8));
        }

        [Test]
        public void TestToString()
        {
            // Act
            var stringRepresentation = _testParams.ToString();

            // Assert
            Assert.IsTrue(stringRepresentation.Contains("7"));
            Assert.IsTrue(stringRepresentation.Contains("8"));
            Assert.IsTrue(stringRepresentation.Contains("9"));
        }

        [Test]
        public void TestMemoryCostCalculation()
        {
            // Arrange
            var testCases = new[]
            {
                new { Params = new ScryptParams(1024, 8, 1), ExpectedMemory = 1024 * 8 * 128 },
                new { Params = new ScryptParams(16384, 8, 1), ExpectedMemory = 16384 * 8 * 128 },
                new { Params = new ScryptParams(32768, 8, 1), ExpectedMemory = 32768 * 8 * 128 }
            };

            foreach (var testCase in testCases)
            {
                // Act
                var memoryCost = testCase.Params.GetMemoryCost();

                // Assert
                Assert.AreEqual(testCase.ExpectedMemory, memoryCost);
            }
        }

        [Test]
        public void TestComputationalCostCalculation()
        {
            // Arrange
            var lowCostParams = new ScryptParams(1024, 1, 1);
            var mediumCostParams = new ScryptParams(16384, 8, 8);
            var highCostParams = new ScryptParams(32768, 16, 16);

            // Act
            var lowCost = lowCostParams.GetComputationalCost();
            var mediumCost = mediumCostParams.GetComputationalCost();
            var highCost = highCostParams.GetComputationalCost();

            // Assert
            Assert.Less(lowCost, mediumCost);
            Assert.Less(mediumCost, highCost);
        }

        [Test]
        public void TestJsonSerialization()
        {
            // Test serialization and deserialization round trip
            var originalParams = new ScryptParams(4096, 4, 2);

            // Act
            var json = JsonConvert.SerializeObject(originalParams);
            var deserializedParams = JsonConvert.DeserializeObject<ScryptParams>(json);

            // Assert
            Assert.AreEqual(originalParams, deserializedParams);
        }

        [Test]
        public void TestCommonScryptParameters()
        {
            // Test common scrypt parameter sets
            var commonParams = new[]
            {
                new ScryptParams(16384, 8, 8),  // NEP2 default
                new ScryptParams(32768, 8, 8),  // Higher security
                new ScryptParams(65536, 8, 8),  // Very high security
                new ScryptParams(1024, 1, 1),   // Fast testing
                new ScryptParams(2048, 1, 1)    // Light security
            };

            foreach (var param in commonParams)
            {
                // Act & Assert
                Assert.IsNotNull(param);
                Assert.Greater(param.N, 0);
                Assert.Greater(param.R, 0);
                Assert.Greater(param.P, 0);

                // Should be serializable
                var json = JsonConvert.SerializeObject(param);
                var deserialized = JsonConvert.DeserializeObject<ScryptParams>(json);
                Assert.AreEqual(param, deserialized);
            }
        }

        #region Unity-Specific Tests

        [Test]
        public void TestSerializationForUnityInspector()
        {
            // Create a serializable wrapper for Unity Inspector
            var serializedData = new SerializableScryptData
            {
                n = _testParams.N,
                r = _testParams.R,
                p = _testParams.P,
                memoryCost = _testParams.GetMemoryCost(),
                computationalCost = _testParams.GetComputationalCost()
            };

            // Act
            var jsonString = JsonUtility.ToJson(serializedData, true);
            var deserializedData = JsonUtility.FromJson<SerializableScryptData>(jsonString);

            // Assert
            Assert.AreEqual(_testParams.N, deserializedData.n);
            Assert.AreEqual(_testParams.R, deserializedData.r);
            Assert.AreEqual(_testParams.P, deserializedData.p);
            Assert.AreEqual(_testParams.GetMemoryCost(), deserializedData.memoryCost);
        }

        [Test]
        [Performance]
        public void TestPerformance_SerializationDeserialization()
        {
            // Arrange
            const int iterations = 1000;
            var testParams = new ScryptParams(16384, 8, 8);

            // Test serialization performance
            var serializationTime = TestHelpers.MeasureExecutionTime(() =>
            {
                for (int i = 0; i < iterations; i++)
                {
                    var json = JsonConvert.SerializeObject(testParams);
                    GC.KeepAlive(json);
                }
            });

            // Test deserialization performance
            var json = JsonConvert.SerializeObject(testParams);
            var deserializationTime = TestHelpers.MeasureExecutionTime(() =>
            {
                for (int i = 0; i < iterations; i++)
                {
                    var deserialized = JsonConvert.DeserializeObject<ScryptParams>(json);
                    GC.KeepAlive(deserialized);
                }
            });

            var avgSerializationTime = (float)serializationTime / iterations;
            var avgDeserializationTime = (float)deserializationTime / iterations;

            // Assert reasonable performance
            Assert.Less(avgSerializationTime, 1f, "Scrypt params serialization should average less than 1ms");
            Assert.Less(avgDeserializationTime, 1f, "Scrypt params deserialization should average less than 1ms");
            
            Debug.Log($"Serialization average time: {avgSerializationTime}ms per operation ({iterations} iterations)");
            Debug.Log($"Deserialization average time: {avgDeserializationTime}ms per operation ({iterations} iterations)");
        }

        [Test]
        public void TestMemoryUsage_ScryptParams()
        {
            // Test memory usage of creating scrypt params
            var memoryUsage = TestHelpers.MeasureMemoryUsage(() =>
            {
                var params1 = new ScryptParams(16384, 8, 8);
                var params2 = new ScryptParams(32768, 16, 16);
                var json = JsonConvert.SerializeObject(params1);
                var deserialized = JsonConvert.DeserializeObject<ScryptParams>(json);
                GC.KeepAlive(new[] { params1, params2, deserialized });
            });

            // Assert reasonable memory usage
            Assert.Less(Math.Abs(memoryUsage), 10 * 1024, "ScryptParams operations should use less than 10KB additional memory");
            Debug.Log($"ScryptParams memory usage: {memoryUsage} bytes");
        }

        [Test]
        public void TestThreadSafety_ScryptParams()
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
                            var scryptParams = new ScryptParams(1024 * (i % 4 + 1), 8, 8);
                            var json = JsonConvert.SerializeObject(scryptParams);
                            var deserialized = JsonConvert.DeserializeObject<ScryptParams>(json);
                            
                            if (!scryptParams.Equals(deserialized))
                            {
                                throw new Exception("ScryptParams serialization round-trip failed in thread");
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
        private class SerializableScryptData
        {
            public int n;
            public int r;
            public int p;
            public long memoryCost;
            public long computationalCost;
        }

        #endregion
    }
}

/// <summary>
/// Enhanced ScryptParams class with Unity-specific features
/// </summary>
public class ScryptParams : IEquatable<ScryptParams>
{
    [JsonProperty("n", DefaultValueHandling = DefaultValueHandling.Include)]
    [JsonAlias("cost")]
    public int N { get; }

    [JsonProperty("r", DefaultValueHandling = DefaultValueHandling.Include)]
    [JsonAlias("blockSize", "blocksize")]
    public int R { get; }

    [JsonProperty("p", DefaultValueHandling = DefaultValueHandling.Include)]
    [JsonAlias("parallel")]
    public int P { get; }

    public static readonly ScryptParams Default = new ScryptParams(16384, 8, 8);

    [JsonConstructor]
    public ScryptParams(int n, int r, int p)
    {
        if (n <= 0)
            throw new ArgumentException("N parameter must be positive", nameof(n));
        if (r <= 0)
            throw new ArgumentException("R parameter must be positive", nameof(r));
        if (p <= 0)
            throw new ArgumentException("P parameter must be positive", nameof(p));

        // Check if N is a power of 2
        if ((n & (n - 1)) != 0)
            throw new ArgumentException("N parameter must be a power of 2", nameof(n));

        N = n;
        R = r;
        P = p;
    }

    /// <summary>
    /// Calculate memory cost in bytes
    /// </summary>
    public long GetMemoryCost()
    {
        return (long)N * R * 128;
    }

    /// <summary>
    /// Calculate relative computational cost
    /// </summary>
    public long GetComputationalCost()
    {
        return (long)N * R * P;
    }

    public bool Equals(ScryptParams other)
    {
        if (other == null) return false;
        return N == other.N && R == other.R && P == other.P;
    }

    public override bool Equals(object obj)
    {
        return Equals(obj as ScryptParams);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(N, R, P);
    }

    public override string ToString()
    {
        return $"ScryptParams(N={N}, R={R}, P={P})";
    }

    public static bool operator ==(ScryptParams left, ScryptParams right)
    {
        if (ReferenceEquals(left, right)) return true;
        if (left is null || right is null) return false;
        return left.Equals(right);
    }

    public static bool operator !=(ScryptParams left, ScryptParams right)
    {
        return !(left == right);
    }
}
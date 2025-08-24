using System;
using System.IO;
using System.Text;
using UnityEngine;
using NUnit.Framework;
using Neo.Unity.SDK;

namespace Neo.Unity.SDK.Tests.Helpers
{
    /// <summary>
    /// Helper class for loading test resources and common test utilities
    /// </summary>
    public static class TestHelpers
    {
        private static string TestResourcesPath => Path.Combine(Application.streamingAssetsPath, "Tests", "Resources");

        /// <summary>
        /// Load JSON test data from resources
        /// </summary>
        /// <param name="filename">JSON filename without extension</param>
        /// <returns>JSON string content</returns>
        public static string LoadJsonResource(string filename)
        {
            string path = Path.Combine(TestResourcesPath, $"{filename}.json");
            if (!File.Exists(path))
            {
                // Try alternative paths for different resource structures
                path = Path.Combine(TestResourcesPath, "responses", $"{filename}.json");
                if (!File.Exists(path))
                {
                    path = Path.Combine(TestResourcesPath, "responses", "contract", $"{filename}.json");
                    if (!File.Exists(path))
                    {
                        path = Path.Combine(TestResourcesPath, "wallet", $"{filename}.json");
                    }
                }
            }
            
            if (File.Exists(path))
            {
                return File.ReadAllText(path, Encoding.UTF8);
            }
            
            throw new FileNotFoundException($"Test resource not found: {filename}.json");
        }

        /// <summary>
        /// Load binary test data from resources
        /// </summary>
        /// <param name="filename">Binary filename</param>
        /// <returns>Binary data as byte array</returns>
        public static byte[] LoadBinaryResource(string filename)
        {
            string path = Path.Combine(TestResourcesPath, filename);
            if (!File.Exists(path))
            {
                path = Path.Combine(TestResourcesPath, "responses", "contract", "contracts", filename);
            }
            
            if (File.Exists(path))
            {
                return File.ReadAllBytes(path);
            }
            
            throw new FileNotFoundException($"Test resource not found: {filename}");
        }

        /// <summary>
        /// Convert hex string to byte array
        /// </summary>
        /// <param name="hex">Hex string</param>
        /// <returns>Byte array</returns>
        public static byte[] HexToBytes(string hex)
        {
            if (hex.StartsWith("0x"))
                hex = hex.Substring(2);
                
            int length = hex.Length;
            byte[] bytes = new byte[length / 2];
            for (int i = 0; i < length; i += 2)
            {
                bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
            }
            return bytes;
        }

        /// <summary>
        /// Convert byte array to hex string
        /// </summary>
        /// <param name="bytes">Byte array</param>
        /// <returns>Hex string</returns>
        public static string BytesToHex(byte[] bytes)
        {
            return BitConverter.ToString(bytes).Replace("-", "").ToLowerInvariant();
        }

        /// <summary>
        /// Create a test account with known private key
        /// </summary>
        /// <param name="privateKeyHex">Private key in hex format</param>
        /// <returns>Test account</returns>
        public static Account CreateTestAccount(string privateKeyHex = "e6e919577dd7b8e97805151c05ae07ff4f752654d6d8797597aca989c02c4cb3")
        {
            var privateKey = HexToBytes(privateKeyHex);
            var keyPair = ECKeyPair.CreateFromPrivateKey(privateKey);
            return new Account(keyPair);
        }

        /// <summary>
        /// Assert that two byte arrays are equal
        /// </summary>
        /// <param name="expected">Expected byte array</param>
        /// <param name="actual">Actual byte array</param>
        public static void AssertBytesEqual(byte[] expected, byte[] actual)
        {
            Assert.AreEqual(expected.Length, actual.Length, "Byte array lengths differ");
            for (int i = 0; i < expected.Length; i++)
            {
                Assert.AreEqual(expected[i], actual[i], $"Bytes differ at index {i}");
            }
        }

        /// <summary>
        /// Assert that hex strings represent equal byte arrays
        /// </summary>
        /// <param name="expectedHex">Expected hex string</param>
        /// <param name="actualHex">Actual hex string</param>
        public static void AssertHexEqual(string expectedHex, string actualHex)
        {
            var expected = HexToBytes(expectedHex);
            var actual = HexToBytes(actualHex);
            AssertBytesEqual(expected, actual);
        }

        /// <summary>
        /// Measure execution time of an action
        /// </summary>
        /// <param name="action">Action to measure</param>
        /// <returns>Execution time in milliseconds</returns>
        public static long MeasureExecutionTime(Action action)
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            action.Invoke();
            stopwatch.Stop();
            return stopwatch.ElapsedMilliseconds;
        }

        /// <summary>
        /// Measure memory usage of an action
        /// </summary>
        /// <param name="action">Action to measure</param>
        /// <returns>Memory delta in bytes</returns>
        public static long MeasureMemoryUsage(Action action)
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            
            long memoryBefore = GC.GetTotalMemory(false);
            action.Invoke();
            long memoryAfter = GC.GetTotalMemory(false);
            
            return memoryAfter - memoryBefore;
        }

        /// <summary>
        /// Generate random bytes for testing
        /// </summary>
        /// <param name="length">Number of bytes to generate</param>
        /// <returns>Random byte array</returns>
        public static byte[] GenerateRandomBytes(int length)
        {
            var random = new System.Random();
            var bytes = new byte[length];
            random.NextBytes(bytes);
            return bytes;
        }

        /// <summary>
        /// Generate random hex string for testing
        /// </summary>
        /// <param name="length">Number of bytes (hex length will be 2x)</param>
        /// <returns>Random hex string</returns>
        public static string GenerateRandomHex(int length)
        {
            return BytesToHex(GenerateRandomBytes(length));
        }
    }
}
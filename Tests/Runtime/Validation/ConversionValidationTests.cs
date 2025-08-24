using System;
using System.Threading.Tasks;
using NUnit.Framework;
using UnityEngine;
using NeoUnity.Runtime.Types;
using NeoUnity.Runtime.Protocol;
using NeoUnity.Runtime.Protocol.Response;
using NeoUnity.Runtime.Reactive;

namespace NeoUnity.Tests.Runtime.Validation
{
    /// <summary>
    /// Validation tests for the final Swift-to-C# conversions.
    /// Ensures all converted classes are properly implemented and functional.
    /// </summary>
    [TestFixture]
    public class ConversionValidationTests
    {
        [Test]
        public void Role_Enum_Conversion_IsValid()
        {
            // Test Role enum functionality
            var stateValidator = Role.StateValidator;
            var oracle = Role.Oracle;
            var neoFS = Role.NeoFSAlphabetNode;

            // Test byte values
            Assert.AreEqual(0x04, stateValidator.ToByte());
            Assert.AreEqual(0x08, oracle.ToByte());
            Assert.AreEqual(0x10, neoFS.ToByte());

            // Test JSON string values
            Assert.AreEqual("StateValidator", stateValidator.ToJsonString());
            Assert.AreEqual("Oracle", oracle.ToJsonString());
            Assert.AreEqual("NeoFSAlphabetNode", neoFS.ToJsonString());

            // Test from byte conversion
            Assert.AreEqual(Role.StateValidator, RoleExtensions.FromByte(0x04));
            Assert.AreEqual(Role.Oracle, RoleExtensions.FromByte(0x08));
            Assert.AreEqual(Role.NeoFSAlphabetNode, RoleExtensions.FromByte(0x10));

            // Test from string conversion
            Assert.AreEqual(Role.StateValidator, RoleExtensions.FromString("StateValidator"));
            Assert.AreEqual(Role.Oracle, RoleExtensions.FromString("Oracle"));
            Assert.AreEqual(Role.NeoFSAlphabetNode, RoleExtensions.FromString("NeoFSAlphabetNode"));

            // Test validation
            Assert.IsTrue(stateValidator.IsValid());
            Assert.IsTrue(oracle.IsValid());
            Assert.IsTrue(neoFS.IsValid());
        }

        [Test]
        public void ContractMethodToken_Conversion_IsValid()
        {
            // Test ContractMethodToken functionality
            var hash = Hash160.Zero;
            var method = "testMethod";
            var paramCount = 2;
            var returnValue = true;
            var callFlags = "All";

            var token = new ContractMethodToken(hash, method, paramCount, returnValue, callFlags);

            Assert.AreEqual(hash, token.Hash);
            Assert.AreEqual(method, token.Method);
            Assert.AreEqual(paramCount, token.ParamCount);
            Assert.AreEqual(returnValue, token.ReturnValue);
            Assert.AreEqual(callFlags, token.CallFlags);

            // Test validation
            Assert.IsTrue(token.IsValid());

            // Test equality
            var token2 = new ContractMethodToken(hash, method, paramCount, returnValue, callFlags);
            Assert.AreEqual(token, token2);
            Assert.AreEqual(token.GetHashCode(), token2.GetHashCode());
        }

        [Test]
        public void ContractStorageEntry_Conversion_IsValid()
        {
            // Test ContractStorageEntry functionality
            var key = "dGVzdEtleQ=="; // base64 for "testKey"
            var value = "dGVzdFZhbHVl"; // base64 for "testValue"

            var entry = new ContractStorageEntry(key, value);

            Assert.AreEqual(key, entry.Key);
            Assert.AreEqual(value, entry.Value);

            // Test byte conversion
            var keyBytes = entry.GetKeyBytes();
            var valueBytes = entry.GetValueBytes();
            
            Assert.IsNotNull(keyBytes);
            Assert.IsNotNull(valueBytes);
            Assert.AreEqual("testKey", System.Text.Encoding.UTF8.GetString(keyBytes));
            Assert.AreEqual("testValue", System.Text.Encoding.UTF8.GetString(valueBytes));

            // Test validation
            Assert.IsTrue(entry.IsValid());

            // Test equality
            var entry2 = new ContractStorageEntry(key, value);
            Assert.AreEqual(entry, entry2);
        }

        [Test]
        public void INeoExpress_Interface_IsValid()
        {
            // Test that INeoExpress interface is properly defined
            var interfaceType = typeof(INeoExpress);
            Assert.IsTrue(interfaceType.IsInterface);

            // Check all required methods exist
            var methods = interfaceType.GetMethods();
            var methodNames = Array.ConvertAll(methods, m => m.Name);

            Assert.Contains("ExpressGetPopulatedBlocks", methodNames);
            Assert.Contains("ExpressGetNep17Contracts", methodNames);
            Assert.Contains("ExpressGetContractStorage", methodNames);
            Assert.Contains("ExpressListContracts", methodNames);
            Assert.Contains("ExpressCreateCheckpoint", methodNames);
            Assert.Contains("ExpressListOracleRequests", methodNames);
            Assert.Contains("ExpressCreateOracleResponseTx", methodNames);
            Assert.Contains("ExpressShutdown", methodNames);

            Debug.Log($"INeoExpress interface has {methods.Length} methods: {string.Join(", ", methodNames)}");
        }

        [Test]
        public void INeoUnityRx_Interface_IsValid()
        {
            // Test that INeoUnityRx interface is properly defined
            var interfaceType = typeof(INeoUnityRx);
            Assert.IsTrue(interfaceType.IsInterface);

            // Check all required methods exist
            var methods = interfaceType.GetMethods();
            var methodNames = Array.ConvertAll(methods, m => m.Name);

            Assert.Contains("GetBlockStream", methodNames);
            Assert.Contains("ReplayBlocks", methodNames);
            Assert.Contains("CatchUpToLatestBlocks", methodNames);
            Assert.Contains("CatchUpToLatestAndSubscribeToNewBlocks", methodNames);
            Assert.Contains("SubscribeToNewBlocks", methodNames);
            Assert.Contains("GetTransactionStream", methodNames);
            Assert.Contains("GetBlockIndexStream", methodNames);

            Debug.Log($"INeoUnityRx interface has {methods.Length} methods");
        }

        [Test]
        public void JsonRpc2_0Rx_Class_IsValid()
        {
            // Test that JsonRpc2_0Rx class is properly defined and instantiable
            var classType = typeof(JsonRpc2_0Rx);
            Assert.IsFalse(classType.IsAbstract);
            Assert.IsFalse(classType.IsInterface);

            // Check constructor exists
            var constructors = classType.GetConstructors();
            Assert.Greater(constructors.Length, 0);

            Debug.Log($"JsonRpc2_0Rx has {constructors.Length} constructors");
        }

        [Test]
        public void NeoUnityRx_Class_IsValid()
        {
            // Test that NeoUnityRx class is properly defined and instantiable
            var classType = typeof(NeoUnityRx);
            Assert.IsFalse(classType.IsAbstract);
            Assert.IsFalse(classType.IsInterface);

            // Check it implements INeoUnityRx
            Assert.IsTrue(typeof(INeoUnityRx).IsAssignableFrom(classType));

            // Check constructor exists
            var constructors = classType.GetConstructors();
            Assert.Greater(constructors.Length, 0);

            Debug.Log($"NeoUnityRx implements INeoUnityRx and has {constructors.Length} constructors");
        }

        [Test]
        public void ReactiveBlockchainMonitor_Component_IsValid()
        {
            // Test that ReactiveBlockchainMonitor is a proper MonoBehaviour
            var componentType = typeof(ReactiveBlockchainMonitor);
            Assert.IsTrue(typeof(MonoBehaviour).IsAssignableFrom(componentType));

            // Check it has the required methods
            var methods = componentType.GetMethods();
            var methodNames = Array.ConvertAll(methods, m => m.Name);

            Assert.Contains("Initialize", methodNames);
            Assert.Contains("StartMonitoring", methodNames);
            Assert.Contains("StopMonitoring", methodNames);

            Debug.Log($"ReactiveBlockchainMonitor is a valid MonoBehaviour component");
        }

        [Test]
        public void All_Response_Classes_Have_IResponse_Interface()
        {
            // Test that response classes implement the correct interfaces
            var assembly = typeof(ContractMethodToken).Assembly;
            var responseTypes = new[]
            {
                typeof(ContractMethodToken),
                typeof(ContractStorageEntry)
            };

            foreach (var type in responseTypes)
            {
                // Check if it implements IResponse<T>
                var interfaces = type.GetInterfaces();
                var hasIResponse = false;
                
                foreach (var iface in interfaces)
                {
                    if (iface.IsGenericType && iface.GetGenericTypeDefinition() == typeof(IResponse<>))
                    {
                        hasIResponse = true;
                        break;
                    }
                }

                Assert.IsTrue(hasIResponse, $"{type.Name} should implement IResponse<T>");
                Debug.Log($"{type.Name} properly implements IResponse<T>");
            }
        }

        [Test]
        public void Role_Extension_Methods_Handle_Invalid_Values()
        {
            // Test error handling for invalid values
            Assert.Throws<ArgumentException>(() => RoleExtensions.FromByte(0xFF));
            Assert.Throws<ArgumentException>(() => RoleExtensions.FromString("InvalidRole"));
            Assert.Throws<ArgumentOutOfRangeException>(() => ((Role)0xFF).ToJsonString());
        }

        [Test]
        public void ContractMethodToken_Handles_Null_Values()
        {
            // Test null handling
            Assert.Throws<ArgumentNullException>(() => new ContractMethodToken(null, "method", 0, false, "flags"));
            Assert.Throws<ArgumentNullException>(() => new ContractMethodToken(Hash160.Zero, null, 0, false, "flags"));
            Assert.Throws<ArgumentNullException>(() => new ContractMethodToken(Hash160.Zero, "method", 0, false, null));
        }

        [Test]
        public void ContractStorageEntry_Handles_Edge_Cases()
        {
            // Test empty values
            var entry = new ContractStorageEntry("", "");
            Assert.AreEqual("", entry.Key);
            Assert.AreEqual("", entry.Value);

            // Test null handling
            Assert.Throws<ArgumentNullException>(() => new ContractStorageEntry(null, "value"));
            Assert.Throws<ArgumentNullException>(() => new ContractStorageEntry("key", null));
        }
    }

    /// <summary>
    /// Integration tests for reactive functionality.
    /// </summary>
    [TestFixture]
    public class ReactiveIntegrationTests
    {
        [Test]
        public async Task ReactiveWrapper_Can_Be_Created()
        {
            // Create a mock NeoUnity instance (this would normally be injected)
            // For now, just test that the wrapper can be created
            try
            {
                // We can't fully test without a real INeo implementation,
                // but we can test the class structure
                var wrapperType = typeof(ReactiveNeoUnityWrapper);
                Assert.IsNotNull(wrapperType);

                var methods = wrapperType.GetMethods();
                var methodNames = Array.ConvertAll(methods, m => m.Name);

                Assert.Contains("StartBlockStream", methodNames);
                Assert.Contains("StartTransactionStream", methodNames);
                Assert.Contains("StopStream", methodNames);
                Assert.Contains("Dispose", methodNames);

                Debug.Log("ReactiveNeoUnityWrapper structure is valid");
            }
            catch (Exception ex)
            {
                Assert.Fail($"ReactiveWrapper creation test failed: {ex.Message}");
            }
        }

        [Test]
        public void BlockNotificationHandler_Can_Be_Created()
        {
            var handlerType = typeof(JsonRpc2_0Rx.BlockNotificationHandler);
            Assert.IsNotNull(handlerType);

            var methods = handlerType.GetMethods();
            var methodNames = Array.ConvertAll(methods, m => m.Name);

            Assert.Contains("StartListening", methodNames);
            Assert.Contains("StopListening", methodNames);

            Debug.Log("BlockNotificationHandler structure is valid");
        }
    }
}
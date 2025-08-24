using System;
using System.ComponentModel;
using Newtonsoft.Json;
using UnityEngine;

namespace Neo.Unity.SDK.Types
{
    /// <summary>
    /// Represents the types of plugins that can be installed on a Neo node.
    /// These plugins extend the functionality of Neo nodes with additional features.
    /// </summary>
    [Serializable]
    public enum NodePluginType
    {
        /// <summary>
        /// Plugin for tracking application logs.
        /// </summary>
        [Description("ApplicationLogs")]
        ApplicationLogs,

        /// <summary>
        /// Plugin for monitoring core metrics of the node.
        /// </summary>
        [Description("CoreMetrics")]
        CoreMetrics,

        /// <summary>
        /// Plugin for importing blocks from external sources.
        /// </summary>
        [Description("ImportBlocks")]
        ImportBlocks,

        /// <summary>
        /// Plugin using LevelDB as storage backend.
        /// </summary>
        [Description("LevelDBStore")]
        LevelDbStore,

        /// <summary>
        /// Plugin using RocksDB as storage backend.
        /// </summary>
        [Description("RocksDBStore")]
        RocksDbStore,

        /// <summary>
        /// Plugin for tracking NEP-17 token transfers via RPC.
        /// </summary>
        [Description("RpcNep17Tracker")]
        RpcNep17Tracker,

        /// <summary>
        /// Plugin providing additional RPC security features.
        /// </summary>
        [Description("RpcSecurity")]
        RpcSecurity,

        /// <summary>
        /// Core RPC server plugin providing RPC functionality.
        /// </summary>
        [Description("RpcServerPlugin")]
        RpcServerPlugin,

        /// <summary>
        /// Plugin for tracking system assets via RPC.
        /// </summary>
        [Description("RpcSystemAssetTrackerPlugin")]
        RpcSystemAssetTracker,

        /// <summary>
        /// Plugin implementing simple policy management.
        /// </summary>
        [Description("SimplePolicyPlugin")]
        SimplePolicy,

        /// <summary>
        /// Plugin for dumping blockchain states.
        /// </summary>
        [Description("StatesDumper")]
        StatesDumper,

        /// <summary>
        /// Plugin for system logging functionality.
        /// </summary>
        [Description("SystemLog")]
        SystemLog
    }

    /// <summary>
    /// Extension methods for NodePluginType enum operations.
    /// </summary>
    public static class NodePluginTypeExtensions
    {
        /// <summary>
        /// Gets the string identifier for the node plugin type.
        /// </summary>
        /// <param name="pluginType">The node plugin type.</param>
        /// <returns>The string identifier used by Neo nodes.</returns>
        public static string GetIdentifier(this NodePluginType pluginType)
        {
            return pluginType switch
            {
                NodePluginType.ApplicationLogs => "ApplicationLogs",
                NodePluginType.CoreMetrics => "CoreMetrics",
                NodePluginType.ImportBlocks => "ImportBlocks",
                NodePluginType.LevelDbStore => "LevelDBStore",
                NodePluginType.RocksDbStore => "RocksDBStore",
                NodePluginType.RpcNep17Tracker => "RpcNep17Tracker",
                NodePluginType.RpcSecurity => "RpcSecurity",
                NodePluginType.RpcServerPlugin => "RpcServerPlugin",
                NodePluginType.RpcSystemAssetTracker => "RpcSystemAssetTrackerPlugin",
                NodePluginType.SimplePolicy => "SimplePolicyPlugin",
                NodePluginType.StatesDumper => "StatesDumper",
                NodePluginType.SystemLog => "SystemLog",
                _ => throw new ArgumentOutOfRangeException(nameof(pluginType), pluginType, "Invalid node plugin type")
            };
        }

        /// <summary>
        /// Parses a node plugin type from its string identifier.
        /// </summary>
        /// <param name="identifier">The string identifier.</param>
        /// <returns>The corresponding node plugin type.</returns>
        /// <exception cref="ArgumentException">Thrown when the identifier is invalid.</exception>
        public static NodePluginType FromIdentifier(string identifier)
        {
            if (string.IsNullOrEmpty(identifier))
                throw new ArgumentException("Plugin identifier cannot be null or empty", nameof(identifier));

            return identifier switch
            {
                "ApplicationLogs" => NodePluginType.ApplicationLogs,
                "CoreMetrics" => NodePluginType.CoreMetrics,
                "ImportBlocks" => NodePluginType.ImportBlocks,
                "LevelDBStore" => NodePluginType.LevelDbStore,
                "RocksDBStore" => NodePluginType.RocksDbStore,
                "RpcNep17Tracker" => NodePluginType.RpcNep17Tracker,
                "RpcSecurity" => NodePluginType.RpcSecurity,
                "RpcServerPlugin" => NodePluginType.RpcServerPlugin,
                "RpcSystemAssetTrackerPlugin" => NodePluginType.RpcSystemAssetTracker,
                "SimplePolicyPlugin" => NodePluginType.SimplePolicy,
                "StatesDumper" => NodePluginType.StatesDumper,
                "SystemLog" => NodePluginType.SystemLog,
                _ => throw new ArgumentException($"Invalid node plugin identifier: {identifier}", nameof(identifier))
            };
        }

        /// <summary>
        /// Gets a human-readable description of the plugin type.
        /// </summary>
        /// <param name="pluginType">The node plugin type.</param>
        /// <returns>A descriptive string explaining the plugin's purpose.</returns>
        public static string GetDescription(this NodePluginType pluginType)
        {
            return pluginType switch
            {
                NodePluginType.ApplicationLogs => "Tracks and provides access to smart contract application logs",
                NodePluginType.CoreMetrics => "Monitors and exposes core node performance metrics",
                NodePluginType.ImportBlocks => "Enables importing blockchain data from external sources",
                NodePluginType.LevelDbStore => "Provides LevelDB storage backend for blockchain data",
                NodePluginType.RocksDbStore => "Provides RocksDB storage backend for blockchain data",
                NodePluginType.RpcNep17Tracker => "Tracks NEP-17 token transfers and balances via RPC",
                NodePluginType.RpcSecurity => "Adds security enhancements to RPC server functionality",
                NodePluginType.RpcServerPlugin => "Core RPC server providing JSON-RPC API functionality",
                NodePluginType.RpcSystemAssetTracker => "Tracks system assets and their movements via RPC",
                NodePluginType.SimplePolicy => "Implements basic policy management for the node",
                NodePluginType.StatesDumper => "Exports blockchain state data for analysis or backup",
                NodePluginType.SystemLog => "Provides comprehensive system logging capabilities",
                _ => "Unknown plugin type"
            };
        }

        /// <summary>
        /// Determines if the plugin type is related to RPC functionality.
        /// </summary>
        /// <param name="pluginType">The node plugin type.</param>
        /// <returns>True if the plugin provides RPC-related functionality, false otherwise.</returns>
        public static bool IsRpcRelated(this NodePluginType pluginType)
        {
            return pluginType switch
            {
                NodePluginType.RpcNep17Tracker or
                NodePluginType.RpcSecurity or
                NodePluginType.RpcServerPlugin or
                NodePluginType.RpcSystemAssetTracker => true,
                _ => false
            };
        }

        /// <summary>
        /// Determines if the plugin type is related to storage functionality.
        /// </summary>
        /// <param name="pluginType">The node plugin type.</param>
        /// <returns>True if the plugin provides storage-related functionality, false otherwise.</returns>
        public static bool IsStorageRelated(this NodePluginType pluginType)
        {
            return pluginType switch
            {
                NodePluginType.LevelDbStore or
                NodePluginType.RocksDbStore => true,
                _ => false
            };
        }

        /// <summary>
        /// Determines if the plugin type is related to logging or monitoring.
        /// </summary>
        /// <param name="pluginType">The node plugin type.</param>
        /// <returns>True if the plugin provides logging or monitoring functionality, false otherwise.</returns>
        public static bool IsMonitoringRelated(this NodePluginType pluginType)
        {
            return pluginType switch
            {
                NodePluginType.ApplicationLogs or
                NodePluginType.CoreMetrics or
                NodePluginType.SystemLog => true,
                _ => false
            };
        }
    }

    /// <summary>
    /// JSON converter for NodePluginType.
    /// </summary>
    public class NodePluginTypeJsonConverter : JsonConverter<NodePluginType>
    {
        public override void WriteJson(JsonWriter writer, NodePluginType value, JsonSerializer serializer)
        {
            writer.WriteValue(value.GetIdentifier());
        }

        public override NodePluginType ReadJson(JsonReader reader, Type objectType, NodePluginType existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            if (reader.Value is not string stringValue)
                throw new JsonException($"Expected string value for NodePluginType, got {reader.Value?.GetType().Name ?? "null"}");

            return NodePluginTypeExtensions.FromIdentifier(stringValue);
        }
    }
}
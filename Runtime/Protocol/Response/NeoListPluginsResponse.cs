using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Newtonsoft.Json;

namespace Neo.Unity.SDK.Protocol.Response
{
    /// <summary>
    /// Response for the listplugins RPC call.
    /// Returns a list of plugins loaded by the Neo node.
    /// </summary>
    [System.Serializable]
    public class NeoListPluginsResponse : NeoResponse<List<NeoListPluginsResponse.Plugin>>
    {
        /// <summary>
        /// Gets the list of plugins from the response.
        /// </summary>
        public List<Plugin> Plugins => Result;

        /// <summary>
        /// Default constructor for JSON deserialization.
        /// </summary>
        public NeoListPluginsResponse() : base()
        {
        }

        /// <summary>
        /// Creates a successful list plugins response.
        /// </summary>
        /// <param name="plugins">The list of plugins</param>
        /// <param name="id">The request ID</param>
        public NeoListPluginsResponse(List<Plugin> plugins, int id = 1) : base(plugins, id)
        {
        }

        /// <summary>
        /// Creates an error list plugins response.
        /// </summary>
        /// <param name="error">The error information</param>
        /// <param name="id">The request ID</param>
        public NeoListPluginsResponse(ResponseError error, int id = 1) : base(error, id)
        {
        }

        /// <summary>
        /// Represents a Neo node plugin with its metadata.
        /// </summary>
        [System.Serializable]
        public class Plugin
        {
            /// <summary>The name of the plugin</summary>
            [JsonProperty("name")]
            public string Name { get; set; }

            /// <summary>The version of the plugin</summary>
            [JsonProperty("version")]
            public string Version { get; set; }

            /// <summary>The list of interfaces implemented by the plugin</summary>
            [JsonProperty("interfaces")]
            public List<string> Interfaces { get; set; } = new List<string>();

            /// <summary>
            /// Default constructor for JSON deserialization.
            /// </summary>
            public Plugin()
            {
            }

            /// <summary>
            /// Creates a new plugin information object.
            /// </summary>
            /// <param name="name">The plugin name</param>
            /// <param name="version">The plugin version</param>
            /// <param name="interfaces">The list of interfaces</param>
            public Plugin(string name, string version, List<string> interfaces)
            {
                Name = name;
                Version = version;
                Interfaces = interfaces ?? new List<string>();
            }

            /// <summary>
            /// Gets the number of interfaces implemented by this plugin.
            /// </summary>
            [JsonIgnore]
            public int InterfaceCount => Interfaces?.Count ?? 0;

            /// <summary>
            /// Checks if the plugin implements a specific interface.
            /// </summary>
            /// <param name="interfaceName">The interface name to check</param>
            /// <returns>True if the plugin implements the interface</returns>
            public bool ImplementsInterface(string interfaceName)
            {
                if (string.IsNullOrEmpty(interfaceName) || Interfaces == null)
                    return false;

                return Interfaces.Contains(interfaceName, StringComparer.OrdinalIgnoreCase);
            }

            /// <summary>
            /// Gets all RPC-related interfaces implemented by this plugin.
            /// </summary>
            [JsonIgnore]
            public List<string> RpcInterfaces
            {
                get
                {
                    if (Interfaces == null)
                        return new List<string>();

                    return Interfaces.Where(i => i.ToLower().Contains("rpc")).ToList();
                }
            }

            /// <summary>
            /// Checks if this is an RPC plugin (implements RPC interfaces).
            /// </summary>
            [JsonIgnore]
            public bool IsRpcPlugin => RpcInterfaces.Count > 0;

            /// <summary>
            /// Gets version information as a System.Version object.
            /// </summary>
            [JsonIgnore]
            public System.Version VersionInfo
            {
                get
                {
                    try
                    {
                        if (string.IsNullOrEmpty(Version))
                            return new System.Version(0, 0, 0, 0);

                        return new System.Version(Version);
                    }
                    catch
                    {
                        return new System.Version(0, 0, 0, 0);
                    }
                }
            }

            /// <summary>
            /// Returns a string representation of the plugin.
            /// </summary>
            /// <returns>String representation</returns>
            public override string ToString()
            {
                return $"Plugin(Name: {Name}, Version: {Version}, Interfaces: {InterfaceCount})";
            }

            /// <summary>
            /// Gets a detailed description of the plugin including all interfaces.
            /// </summary>
            /// <returns>Detailed description</returns>
            public string GetDetailedInfo()
            {
                var interfacesStr = Interfaces != null && Interfaces.Count > 0 
                    ? string.Join(", ", Interfaces) 
                    : "None";
                
                return $"Plugin '{Name}' v{Version}\nInterfaces: {interfacesStr}";
            }

            /// <summary>
            /// Validates the plugin information.
            /// </summary>
            /// <exception cref="ArgumentException">If the plugin information is invalid</exception>
            public void Validate()
            {
                if (string.IsNullOrEmpty(Name))
                    throw new ArgumentException("Plugin name cannot be null or empty");

                if (string.IsNullOrEmpty(Version))
                    throw new ArgumentException("Plugin version cannot be null or empty");

                if (Interfaces == null)
                    throw new ArgumentException("Interfaces list cannot be null");

                // Validate version format
                try
                {
                    var _ = new System.Version(Version);
                }
                catch
                {
                    throw new ArgumentException($"Invalid version format: {Version}");
                }
            }
        }

        /// <summary>
        /// Gets all plugins that implement a specific interface.
        /// </summary>
        /// <param name="interfaceName">The interface name to search for</param>
        /// <returns>List of plugins implementing the interface</returns>
        public List<Plugin> GetPluginsByInterface(string interfaceName)
        {
            if (string.IsNullOrEmpty(interfaceName) || Plugins == null)
                return new List<Plugin>();

            return Plugins.Where(p => p.ImplementsInterface(interfaceName)).ToList();
        }

        /// <summary>
        /// Gets all RPC plugins from the list.
        /// </summary>
        /// <returns>List of RPC plugins</returns>
        public List<Plugin> GetRpcPlugins()
        {
            if (Plugins == null)
                return new List<Plugin>();

            return Plugins.Where(p => p.IsRpcPlugin).ToList();
        }

        /// <summary>
        /// Finds a plugin by name.
        /// </summary>
        /// <param name="name">The plugin name</param>
        /// <returns>The plugin if found, null otherwise</returns>
        public Plugin FindPluginByName(string name)
        {
            if (string.IsNullOrEmpty(name) || Plugins == null)
                return null;

            return Plugins.FirstOrDefault(p => 
                string.Equals(p.Name, name, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Gets the total number of plugins.
        /// </summary>
        [JsonIgnore]
        public int PluginCount => Plugins?.Count ?? 0;

        /// <summary>
        /// Gets all unique interfaces across all plugins.
        /// </summary>
        [JsonIgnore]
        public List<string> AllInterfaces
        {
            get
            {
                if (Plugins == null)
                    return new List<string>();

                var allInterfaces = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                foreach (var plugin in Plugins)
                {
                    if (plugin.Interfaces != null)
                    {
                        foreach (var interfaceName in plugin.Interfaces)
                        {
                            allInterfaces.Add(interfaceName);
                        }
                    }
                }

                return allInterfaces.ToList();
            }
        }

        /// <summary>
        /// Returns a summary string of all plugins.
        /// </summary>
        /// <returns>Summary string</returns>
        public string GetSummary()
        {
            if (Plugins == null || Plugins.Count == 0)
                return "No plugins loaded";

            var rpcCount = GetRpcPlugins().Count;
            var totalInterfaces = AllInterfaces.Count;

            return $"Total Plugins: {PluginCount}, RPC Plugins: {rpcCount}, Total Interfaces: {totalInterfaces}";
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Newtonsoft.Json;

namespace Neo.Unity.SDK.Protocol.Response
{
    /// <summary>
    /// Response for the traverseiterator RPC call.
    /// Returns a list of stack items from iterator traversal.
    /// </summary>
    [System.Serializable]
    public class NeoTraverseIteratorResponse : NeoResponse<List<object>>
    {
        /// <summary>
        /// Gets the traverse iterator results from the response.
        /// </summary>
        public List<object> TraverseResults => Result;

        /// <summary>
        /// Default constructor for JSON deserialization.
        /// </summary>
        public NeoTraverseIteratorResponse() : base()
        {
        }

        /// <summary>
        /// Creates a successful traverse iterator response.
        /// </summary>
        /// <param name="results">The iterator results</param>
        /// <param name="id">The request ID</param>
        public NeoTraverseIteratorResponse(List<object> results, int id = 1) : base(results, id)
        {
        }

        /// <summary>
        /// Creates an error traverse iterator response.
        /// </summary>
        /// <param name="error">The error information</param>
        /// <param name="id">The request ID</param>
        public NeoTraverseIteratorResponse(ResponseError error, int id = 1) : base(error, id)
        {
        }

        /// <summary>
        /// Gets the number of items returned from the iterator.
        /// </summary>
        [JsonIgnore]
        public int ItemCount => TraverseResults?.Count ?? 0;

        /// <summary>
        /// Checks if the iterator returned any items.
        /// </summary>
        [JsonIgnore]
        public bool HasItems => ItemCount > 0;

        /// <summary>
        /// Gets all string values from the results (filters out non-string types).
        /// </summary>
        [JsonIgnore]
        public List<string> StringValues
        {
            get
            {
                if (TraverseResults == null)
                    return new List<string>();

                return TraverseResults
                    .Where(item => item is string)
                    .Cast<string>()
                    .ToList();
            }
        }

        /// <summary>
        /// Gets all numeric values from the results (filters out non-numeric types).
        /// </summary>
        [JsonIgnore]
        public List<decimal> NumericValues
        {
            get
            {
                if (TraverseResults == null)
                    return new List<decimal>();

                var numbers = new List<decimal>();
                foreach (var item in TraverseResults)
                {
                    if (item == null) continue;

                    if (decimal.TryParse(item.ToString(), out decimal value))
                    {
                        numbers.Add(value);
                    }
                }
                return numbers;
            }
        }

        /// <summary>
        /// Gets all boolean values from the results (filters out non-boolean types).
        /// </summary>
        [JsonIgnore]
        public List<bool> BooleanValues
        {
            get
            {
                if (TraverseResults == null)
                    return new List<bool>();

                return TraverseResults
                    .Where(item => item is bool)
                    .Cast<bool>()
                    .ToList();
            }
        }

        /// <summary>
        /// Gets the types of all items in the results.
        /// </summary>
        [JsonIgnore]
        public List<string> ItemTypes
        {
            get
            {
                if (TraverseResults == null)
                    return new List<string>();

                return TraverseResults
                    .Select(item => item?.GetType().Name ?? "null")
                    .ToList();
            }
        }

        /// <summary>
        /// Gets unique types present in the results.
        /// </summary>
        [JsonIgnore]
        public List<string> UniqueItemTypes
        {
            get
            {
                return ItemTypes.Distinct().ToList();
            }
        }

        /// <summary>
        /// Gets an item at a specific index with type checking.
        /// </summary>
        /// <typeparam name="T">The expected type of the item</typeparam>
        /// <param name="index">The index of the item</param>
        /// <returns>The item cast to type T, or default(T) if not found or wrong type</returns>
        public T GetItemAs<T>(int index)
        {
            if (TraverseResults == null || index < 0 || index >= TraverseResults.Count)
                return default(T);

            var item = TraverseResults[index];
            if (item is T)
                return (T)item;

            // Try to convert string representations
            if (typeof(T) == typeof(string) && item != null)
                return (T)(object)item.ToString();

            // Try to parse numeric types from strings
            if (item is string str && !string.IsNullOrEmpty(str))
            {
                if (typeof(T) == typeof(int) && int.TryParse(str, out int intVal))
                    return (T)(object)intVal;

                if (typeof(T) == typeof(long) && long.TryParse(str, out long longVal))
                    return (T)(object)longVal;

                if (typeof(T) == typeof(decimal) && decimal.TryParse(str, out decimal decVal))
                    return (T)(object)decVal;

                if (typeof(T) == typeof(double) && double.TryParse(str, out double doubleVal))
                    return (T)(object)doubleVal;

                if (typeof(T) == typeof(bool) && bool.TryParse(str, out bool boolVal))
                    return (T)(object)boolVal;
            }

            return default(T);
        }

        /// <summary>
        /// Gets all items of a specific type.
        /// </summary>
        /// <typeparam name="T">The type to filter for</typeparam>
        /// <returns>List of items of type T</returns>
        public List<T> GetItemsAs<T>()
        {
            if (TraverseResults == null)
                return new List<T>();

            var items = new List<T>();
            for (int i = 0; i < TraverseResults.Count; i++)
            {
                var item = GetItemAs<T>(i);
                if (!EqualityComparer<T>.Default.Equals(item, default(T)))
                {
                    items.Add(item);
                }
            }
            return items;
        }

        /// <summary>
        /// Checks if the iterator results contain a specific value.
        /// </summary>
        /// <param name="value">The value to search for</param>
        /// <returns>True if the value is found</returns>
        public bool ContainsValue(object value)
        {
            if (TraverseResults == null || value == null)
                return false;

            foreach (var item in TraverseResults)
            {
                if (item == null && value == null)
                    return true;

                if (item != null && item.Equals(value))
                    return true;

                // Try string comparison
                if (item != null && value != null && 
                    string.Equals(item.ToString(), value.ToString(), StringComparison.OrdinalIgnoreCase))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Finds the index of a specific value in the results.
        /// </summary>
        /// <param name="value">The value to search for</param>
        /// <returns>The index of the value, or -1 if not found</returns>
        public int IndexOf(object value)
        {
            if (TraverseResults == null)
                return -1;

            for (int i = 0; i < TraverseResults.Count; i++)
            {
                var item = TraverseResults[i];

                if (item == null && value == null)
                    return i;

                if (item != null && value != null)
                {
                    if (item.Equals(value))
                        return i;

                    // Try string comparison
                    if (string.Equals(item.ToString(), value.ToString(), StringComparison.OrdinalIgnoreCase))
                        return i;
                }
            }

            return -1;
        }

        /// <summary>
        /// Gets a summary of the item types and their counts.
        /// </summary>
        /// <returns>Dictionary with type names as keys and counts as values</returns>
        public Dictionary<string, int> GetItemTypeSummary()
        {
            var summary = new Dictionary<string, int>();

            if (TraverseResults == null)
                return summary;

            foreach (var item in TraverseResults)
            {
                var typeName = item?.GetType().Name ?? "null";
                
                if (summary.ContainsKey(typeName))
                    summary[typeName]++;
                else
                    summary[typeName] = 1;
            }

            return summary;
        }

        /// <summary>
        /// Returns a string representation of the traverse iterator response.
        /// </summary>
        /// <returns>String representation</returns>
        public override string ToString()
        {
            if (HasError)
                return base.ToString();

            var typeSummary = GetItemTypeSummary();
            var typeInfo = string.Join(", ", typeSummary.Select(kv => $"{kv.Key}: {kv.Value}"));

            return $"NeoTraverseIteratorResponse(Items: {ItemCount}, Types: [{typeInfo}])";
        }

        /// <summary>
        /// Gets detailed information about the iterator results.
        /// </summary>
        /// <returns>Detailed description</returns>
        public string GetDetailedInfo()
        {
            if (!HasItems)
                return "No items returned from iterator";

            var typeSummary = GetItemTypeSummary();
            var info = $"Iterator Results:\n" +
                      $"  Total Items: {ItemCount}\n" +
                      $"  Unique Types: {UniqueItemTypes.Count}\n" +
                      $"  Type Summary:\n";

            foreach (var kv in typeSummary.OrderByDescending(x => x.Value))
            {
                info += $"    {kv.Key}: {kv.Value} items\n";
            }

            if (ItemCount <= 10)
            {
                info += "  Sample Items:\n";
                for (int i = 0; i < Math.Min(ItemCount, 10); i++)
                {
                    var item = TraverseResults[i];
                    var itemStr = item?.ToString() ?? "null";
                    if (itemStr.Length > 50)
                        itemStr = itemStr.Substring(0, 50) + "...";
                    
                    info += $"    [{i}] {item?.GetType().Name ?? "null"}: {itemStr}\n";
                }
            }

            return info.TrimEnd('\n');
        }

        /// <summary>
        /// Validates the traverse iterator results.
        /// </summary>
        /// <exception cref="ArgumentException">If the results are invalid</exception>
        public void ValidateResults()
        {
            // Iterator results can be empty or null (valid states)
            // No specific validation required unless there are business rules
        }

        /// <summary>
        /// Creates a copy of this response with new results.
        /// </summary>
        /// <param name="newResults">The new results list</param>
        /// <returns>New NeoTraverseIteratorResponse instance</returns>
        public NeoTraverseIteratorResponse WithResults(List<object> newResults)
        {
            return new NeoTraverseIteratorResponse(newResults, Id);
        }

        /// <summary>
        /// Filters the results to only include items of a specific type.
        /// </summary>
        /// <typeparam name="T">The type to filter for</typeparam>
        /// <returns>New response with filtered results</returns>
        public NeoTraverseIteratorResponse FilterByType<T>()
        {
            var filteredResults = GetItemsAs<T>().Cast<object>().ToList();
            return WithResults(filteredResults);
        }

        /// <summary>
        /// Gets the first item of a specific type.
        /// </summary>
        /// <typeparam name="T">The expected type</typeparam>
        /// <returns>The first item of type T, or default(T) if not found</returns>
        public T GetFirstItemAs<T>()
        {
            if (TraverseResults == null || TraverseResults.Count == 0)
                return default(T);

            return GetItemAs<T>(0);
        }

        /// <summary>
        /// Gets the last item of a specific type.
        /// </summary>
        /// <typeparam name="T">The expected type</typeparam>
        /// <returns>The last item of type T, or default(T) if not found</returns>
        public T GetLastItemAs<T>()
        {
            if (TraverseResults == null || TraverseResults.Count == 0)
                return default(T);

            return GetItemAs<T>(TraverseResults.Count - 1);
        }

        /// <summary>
        /// Checks if all items in the results are of the same type.
        /// </summary>
        [JsonIgnore]
        public bool AllItemsSameType
        {
            get
            {
                var uniqueTypes = UniqueItemTypes;
                return uniqueTypes.Count <= 1;
            }
        }

        /// <summary>
        /// Gets the most common type in the results.
        /// </summary>
        [JsonIgnore]
        public string MostCommonType
        {
            get
            {
                var typeSummary = GetItemTypeSummary();
                if (typeSummary.Count == 0)
                    return "unknown";

                return typeSummary.OrderByDescending(kv => kv.Value).First().Key;
            }
        }
    }
}
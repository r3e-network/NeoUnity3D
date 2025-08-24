using System;
using System.Collections.Generic;
using System.Linq;

namespace Neo.Unity.SDK.Utils
{
    /// <summary>
    /// Extension methods for arrays and collections that provide convenience operations
    /// commonly used in Neo blockchain operations.
    /// </summary>
    public static class ArrayExtensions
    {
        /// <summary>
        /// Appends an element to the end of an array, creating a new array.
        /// </summary>
        /// <typeparam name="T">The type of elements in the array.</typeparam>
        /// <param name="array">The source array.</param>
        /// <param name="element">The element to append.</param>
        /// <returns>A new array containing all elements from the source array plus the new element.</returns>
        public static T[] Append<T>(this T[] array, T element)
        {
            if (array == null)
                throw new ArgumentNullException(nameof(array));

            var result = new T[array.Length + 1];
            Array.Copy(array, result, array.Length);
            result[array.Length] = element;
            return result;
        }

        /// <summary>
        /// Prepends an element to the beginning of an array, creating a new array.
        /// </summary>
        /// <typeparam name="T">The type of elements in the array.</typeparam>
        /// <param name="array">The source array.</param>
        /// <param name="element">The element to prepend.</param>
        /// <returns>A new array containing the new element followed by all elements from the source array.</returns>
        public static T[] Prepend<T>(this T[] array, T element)
        {
            if (array == null)
                throw new ArgumentNullException(nameof(array));

            var result = new T[array.Length + 1];
            result[0] = element;
            Array.Copy(array, 0, result, 1, array.Length);
            return result;
        }

        /// <summary>
        /// Concatenates two arrays, creating a new array.
        /// </summary>
        /// <typeparam name="T">The type of elements in the arrays.</typeparam>
        /// <param name="first">The first array.</param>
        /// <param name="second">The second array.</param>
        /// <returns>A new array containing all elements from both arrays.</returns>
        public static T[] Concat<T>(this T[] first, T[] second)
        {
            if (first == null)
                throw new ArgumentNullException(nameof(first));
            if (second == null)
                throw new ArgumentNullException(nameof(second));

            var result = new T[first.Length + second.Length];
            Array.Copy(first, 0, result, 0, first.Length);
            Array.Copy(second, 0, result, first.Length, second.Length);
            return result;
        }

        /// <summary>
        /// Concatenates multiple arrays, creating a new array.
        /// </summary>
        /// <typeparam name="T">The type of elements in the arrays.</typeparam>
        /// <param name="arrays">The arrays to concatenate.</param>
        /// <returns>A new array containing all elements from all input arrays.</returns>
        public static T[] ConcatAll<T>(params T[][] arrays)
        {
            if (arrays == null)
                throw new ArgumentNullException(nameof(arrays));

            int totalLength = arrays.Sum(arr => arr?.Length ?? 0);
            var result = new T[totalLength];
            int offset = 0;

            foreach (var array in arrays)
            {
                if (array != null && array.Length > 0)
                {
                    Array.Copy(array, 0, result, offset, array.Length);
                    offset += array.Length;
                }
            }

            return result;
        }

        /// <summary>
        /// Creates a slice of the array from the specified start index with the specified length.
        /// </summary>
        /// <typeparam name="T">The type of elements in the array.</typeparam>
        /// <param name="array">The source array.</param>
        /// <param name="start">The starting index.</param>
        /// <param name="length">The number of elements to include.</param>
        /// <returns>A new array containing the sliced elements.</returns>
        public static T[] Slice<T>(this T[] array, int start, int length)
        {
            if (array == null)
                throw new ArgumentNullException(nameof(array));
            if (start < 0 || start >= array.Length)
                throw new ArgumentOutOfRangeException(nameof(start));
            if (length < 0 || start + length > array.Length)
                throw new ArgumentOutOfRangeException(nameof(length));

            var result = new T[length];
            Array.Copy(array, start, result, 0, length);
            return result;
        }

        /// <summary>
        /// Creates a slice of the array from the specified start index to the end.
        /// </summary>
        /// <typeparam name="T">The type of elements in the array.</typeparam>
        /// <param name="array">The source array.</param>
        /// <param name="start">The starting index.</param>
        /// <returns>A new array containing elements from the start index to the end.</returns>
        public static T[] Slice<T>(this T[] array, int start)
        {
            if (array == null)
                throw new ArgumentNullException(nameof(array));
            if (start < 0 || start > array.Length)
                throw new ArgumentOutOfRangeException(nameof(start));

            return Slice(array, start, array.Length - start);
        }

        /// <summary>
        /// Reverses the elements in an array, creating a new array.
        /// </summary>
        /// <typeparam name="T">The type of elements in the array.</typeparam>
        /// <param name="array">The source array.</param>
        /// <returns>A new array with elements in reverse order.</returns>
        public static T[] Reverse<T>(this T[] array)
        {
            if (array == null)
                throw new ArgumentNullException(nameof(array));

            var result = new T[array.Length];
            for (int i = 0; i < array.Length; i++)
            {
                result[i] = array[array.Length - 1 - i];
            }
            return result;
        }

        /// <summary>
        /// Checks if the array is null or empty.
        /// </summary>
        /// <typeparam name="T">The type of elements in the array.</typeparam>
        /// <param name="array">The array to check.</param>
        /// <returns>True if the array is null or has no elements, false otherwise.</returns>
        public static bool IsNullOrEmpty<T>(this T[] array)
        {
            return array == null || array.Length == 0;
        }

        /// <summary>
        /// Safely gets an element at the specified index, returning a default value if the index is out of bounds.
        /// </summary>
        /// <typeparam name="T">The type of elements in the array.</typeparam>
        /// <param name="array">The source array.</param>
        /// <param name="index">The index to access.</param>
        /// <param name="defaultValue">The default value to return if index is out of bounds.</param>
        /// <returns>The element at the specified index, or the default value if out of bounds.</returns>
        public static T SafeGet<T>(this T[] array, int index, T defaultValue = default)
        {
            if (array == null || index < 0 || index >= array.Length)
                return defaultValue;

            return array[index];
        }

        /// <summary>
        /// Finds the index of the first occurrence of the specified element.
        /// </summary>
        /// <typeparam name="T">The type of elements in the array.</typeparam>
        /// <param name="array">The source array.</param>
        /// <param name="element">The element to find.</param>
        /// <returns>The index of the element if found, -1 otherwise.</returns>
        public static int IndexOf<T>(this T[] array, T element) where T : IEquatable<T>
        {
            if (array == null)
                throw new ArgumentNullException(nameof(array));

            for (int i = 0; i < array.Length; i++)
            {
                if (array[i]?.Equals(element) == true)
                    return i;
            }
            return -1;
        }

        /// <summary>
        /// Determines whether the array contains the specified element.
        /// </summary>
        /// <typeparam name="T">The type of elements in the array.</typeparam>
        /// <param name="array">The source array.</param>
        /// <param name="element">The element to search for.</param>
        /// <returns>True if the element is found, false otherwise.</returns>
        public static bool Contains<T>(this T[] array, T element) where T : IEquatable<T>
        {
            return IndexOf(array, element) >= 0;
        }

        /// <summary>
        /// Removes all occurrences of the specified element from the array.
        /// </summary>
        /// <typeparam name="T">The type of elements in the array.</typeparam>
        /// <param name="array">The source array.</param>
        /// <param name="element">The element to remove.</param>
        /// <returns>A new array with all occurrences of the element removed.</returns>
        public static T[] Remove<T>(this T[] array, T element) where T : IEquatable<T>
        {
            if (array == null)
                throw new ArgumentNullException(nameof(array));

            return array.Where(x => !element?.Equals(x) == true).ToArray();
        }

        /// <summary>
        /// Removes the element at the specified index from the array.
        /// </summary>
        /// <typeparam name="T">The type of elements in the array.</typeparam>
        /// <param name="array">The source array.</param>
        /// <param name="index">The index of the element to remove.</param>
        /// <returns>A new array with the element at the specified index removed.</returns>
        public static T[] RemoveAt<T>(this T[] array, int index)
        {
            if (array == null)
                throw new ArgumentNullException(nameof(array));
            if (index < 0 || index >= array.Length)
                throw new ArgumentOutOfRangeException(nameof(index));

            var result = new T[array.Length - 1];
            if (index > 0)
                Array.Copy(array, 0, result, 0, index);
            if (index < array.Length - 1)
                Array.Copy(array, index + 1, result, index, array.Length - index - 1);

            return result;
        }

        /// <summary>
        /// Inserts an element at the specified index in the array.
        /// </summary>
        /// <typeparam name="T">The type of elements in the array.</typeparam>
        /// <param name="array">The source array.</param>
        /// <param name="index">The index at which to insert the element.</param>
        /// <param name="element">The element to insert.</param>
        /// <returns>A new array with the element inserted at the specified index.</returns>
        public static T[] Insert<T>(this T[] array, int index, T element)
        {
            if (array == null)
                throw new ArgumentNullException(nameof(array));
            if (index < 0 || index > array.Length)
                throw new ArgumentOutOfRangeException(nameof(index));

            var result = new T[array.Length + 1];
            if (index > 0)
                Array.Copy(array, 0, result, 0, index);
            result[index] = element;
            if (index < array.Length)
                Array.Copy(array, index, result, index + 1, array.Length - index);

            return result;
        }

        /// <summary>
        /// Converts an array to a hexadecimal string representation.
        /// Only works with byte arrays.
        /// </summary>
        /// <param name="bytes">The byte array.</param>
        /// <param name="prefix">Whether to include the "0x" prefix.</param>
        /// <returns>The hexadecimal string representation.</returns>
        public static string ToHex(this byte[] bytes, bool prefix = false)
        {
            if (bytes == null)
                throw new ArgumentNullException(nameof(bytes));

            if (bytes.Length == 0)
                return prefix ? "0x" : "";

            var hex = BitConverter.ToString(bytes).Replace("-", "").ToLowerInvariant();
            return prefix ? "0x" + hex : hex;
        }

        /// <summary>
        /// Pads a byte array to the specified length with leading zeros.
        /// </summary>
        /// <param name="bytes">The byte array to pad.</param>
        /// <param name="length">The desired length.</param>
        /// <returns>A new byte array padded to the specified length.</returns>
        public static byte[] PadLeft(this byte[] bytes, int length)
        {
            if (bytes == null)
                throw new ArgumentNullException(nameof(bytes));
            if (length < 0)
                throw new ArgumentOutOfRangeException(nameof(length));
            if (bytes.Length >= length)
                return (byte[])bytes.Clone();

            var result = new byte[length];
            Array.Copy(bytes, 0, result, length - bytes.Length, bytes.Length);
            return result;
        }

        /// <summary>
        /// Pads a byte array to the specified length with trailing zeros.
        /// </summary>
        /// <param name="bytes">The byte array to pad.</param>
        /// <param name="length">The desired length.</param>
        /// <returns>A new byte array padded to the specified length.</returns>
        public static byte[] PadRight(this byte[] bytes, int length)
        {
            if (bytes == null)
                throw new ArgumentNullException(nameof(bytes));
            if (length < 0)
                throw new ArgumentOutOfRangeException(nameof(length));
            if (bytes.Length >= length)
                return (byte[])bytes.Clone();

            var result = new byte[length];
            Array.Copy(bytes, 0, result, 0, bytes.Length);
            return result;
        }

        /// <summary>
        /// Determines if two byte arrays are equal.
        /// </summary>
        /// <param name="first">The first byte array.</param>
        /// <param name="second">The second byte array.</param>
        /// <returns>True if the arrays are equal, false otherwise.</returns>
        public static bool SequenceEqual(this byte[] first, byte[] second)
        {
            if (ReferenceEquals(first, second))
                return true;
            if (first == null || second == null)
                return false;
            if (first.Length != second.Length)
                return false;

            for (int i = 0; i < first.Length; i++)
            {
                if (first[i] != second[i])
                    return false;
            }

            return true;
        }
    }
}
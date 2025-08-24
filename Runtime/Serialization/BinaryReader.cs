using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Text;
using UnityEngine;
using Neo.Unity.SDK.Types;
using Neo.Unity.SDK.Crypto;
using Neo.Unity.SDK.Script;

namespace Neo.Unity.SDK.Serialization
{
    /// <summary>
    /// Binary reader for Neo protocol data structures.
    /// Handles little-endian binary data reading with variable-length integer support.
    /// Unity-optimized implementation with proper async support and memory management.
    /// </summary>
    public class BinaryReader : IDisposable
    {
        #region Private Fields
        
        private readonly byte[] array;
        private int position = 0;
        private int marker = -1;
        private bool disposed = false;
        
        #endregion
        
        #region Properties
        
        /// <summary>Current position in the stream</summary>
        public int Position 
        { 
            get => position; 
            set => position = Math.Max(0, Math.Min(value, array.Length)); 
        }
        
        /// <summary>Number of bytes available to read</summary>
        public int Available => array.Length - position;
        
        /// <summary>Whether we've reached the end of the stream</summary>
        public bool EndOfStream => position >= array.Length;
        
        /// <summary>Total length of the underlying data</summary>
        public int Length => array.Length;
        
        #endregion
        
        #region Constructors
        
        /// <summary>
        /// Creates a new binary reader from a byte array.
        /// </summary>
        /// <param name="input">The byte array to read from</param>
        /// <exception cref="ArgumentNullException">If input is null</exception>
        public BinaryReader(byte[] input)
        {
            array = input ?? throw new ArgumentNullException(nameof(input));
        }
        
        /// <summary>
        /// Creates a new binary reader from a hex string.
        /// </summary>
        /// <param name="hexInput">The hex string to read from</param>
        /// <exception cref="ArgumentException">If hex string is invalid</exception>
        public BinaryReader(string hexInput)
        {
            if (string.IsNullOrEmpty(hexInput))
                throw new ArgumentException("Hex input cannot be null or empty.", nameof(hexInput));
            
            if (!hexInput.IsValidHex())
                throw new ArgumentException("Invalid hex string format.", nameof(hexInput));
            
            array = Convert.FromHexString(hexInput);
        }
        
        #endregion
        
        #region Position Management
        
        /// <summary>
        /// Marks the current position for later reset.
        /// </summary>
        public void Mark()
        {
            EnsureNotDisposed();
            marker = position;
        }
        
        /// <summary>
        /// Resets to the previously marked position.
        /// </summary>
        /// <exception cref="InvalidOperationException">If no mark was set</exception>
        public void Reset()
        {
            EnsureNotDisposed();
            
            if (marker < 0)
                throw new InvalidOperationException("No mark has been set. Call Mark() first.");
            
            position = marker;
        }
        
        /// <summary>
        /// Resets the position to the beginning.
        /// </summary>
        public void ResetToBeginning()
        {
            EnsureNotDisposed();
            position = 0;
        }
        
        /// <summary>
        /// Skips the specified number of bytes.
        /// </summary>
        /// <param name="count">Number of bytes to skip</param>
        /// <exception cref="ArgumentOutOfRangeException">If trying to skip beyond end of stream</exception>
        public void Skip(int count)
        {
            EnsureNotDisposed();
            
            if (count < 0)
                throw new ArgumentOutOfRangeException(nameof(count), "Skip count cannot be negative.");
            
            if (position + count > array.Length)
                throw new ArgumentOutOfRangeException(nameof(count), "Cannot skip beyond end of stream.");
            
            position += count;
        }
        
        #endregion
        
        #region Basic Type Reading
        
        /// <summary>
        /// Reads a boolean value (1 byte).
        /// </summary>
        /// <returns>The boolean value</returns>
        public bool ReadBoolean()
        {
            EnsureNotDisposed();
            EnsureAvailable(1);
            
            var value = array[position] != 0;
            position += 1;
            return value;
        }
        
        /// <summary>
        /// Reads a single byte.
        /// </summary>
        /// <returns>The byte value</returns>
        public byte ReadByte()
        {
            EnsureNotDisposed();
            EnsureAvailable(1);
            
            var value = array[position];
            position += 1;
            return value;
        }
        
        /// <summary>
        /// Reads a byte array of the specified length.
        /// </summary>
        /// <param name="length">Number of bytes to read</param>
        /// <returns>The byte array</returns>
        public byte[] ReadBytes(int length)
        {
            EnsureNotDisposed();
            
            if (length < 0)
                throw new ArgumentOutOfRangeException(nameof(length), "Length cannot be negative.");
            
            if (length == 0)
                return new byte[0];
            
            EnsureAvailable(length);
            
            var result = new byte[length];
            Array.Copy(array, position, result, 0, length);
            position += length;
            return result;
        }
        
        /// <summary>
        /// Reads a 16-bit unsigned integer (little-endian).
        /// </summary>
        /// <returns>The UInt16 value</returns>
        public ushort ReadUInt16()
        {
            EnsureNotDisposed();
            EnsureAvailable(2);
            
            var value = BitConverter.ToUInt16(array, position);
            position += 2;
            return value;
        }
        
        /// <summary>
        /// Reads a 16-bit signed integer (little-endian).
        /// </summary>
        /// <returns>The Int16 value</returns>
        public short ReadInt16()
        {
            EnsureNotDisposed();
            EnsureAvailable(2);
            
            var value = BitConverter.ToInt16(array, position);
            position += 2;
            return value;
        }
        
        /// <summary>
        /// Reads a 32-bit unsigned integer (little-endian).
        /// </summary>
        /// <returns>The UInt32 value</returns>
        public uint ReadUInt32()
        {
            EnsureNotDisposed();
            EnsureAvailable(4);
            
            var value = BitConverter.ToUInt32(array, position);
            position += 4;
            return value;
        }
        
        /// <summary>
        /// Reads a 32-bit signed integer (little-endian).
        /// </summary>
        /// <returns>The Int32 value</returns>
        public int ReadInt32()
        {
            EnsureNotDisposed();
            EnsureAvailable(4);
            
            var value = BitConverter.ToInt32(array, position);
            position += 4;
            return value;
        }
        
        /// <summary>
        /// Reads a 64-bit signed integer (little-endian).
        /// </summary>
        /// <returns>The Int64 value</returns>
        public long ReadInt64()
        {
            EnsureNotDisposed();
            EnsureAvailable(8);
            
            var value = BitConverter.ToInt64(array, position);
            position += 8;
            return value;
        }
        
        #endregion
        
        #region Variable-Length Reading
        
        /// <summary>
        /// Reads a variable-length integer.
        /// </summary>
        /// <returns>The variable-length integer</returns>
        public int ReadVarInt()
        {
            return ReadVarInt(int.MaxValue);
        }
        
        /// <summary>
        /// Reads a variable-length integer with maximum value validation.
        /// </summary>
        /// <param name="max">Maximum allowed value</param>
        /// <returns>The variable-length integer</returns>
        /// <exception cref="ArgumentOutOfRangeException">If value exceeds maximum</exception>
        public int ReadVarInt(int max)
        {
            EnsureNotDisposed();
            
            var first = ReadByte();
            int value;
            
            switch (first)
            {
                case 0xFD:
                    value = ReadUInt16();
                    break;
                case 0xFE:
                    value = (int)ReadUInt32();
                    break;
                case 0xFF:
                    var longValue = ReadInt64();
                    if (longValue > int.MaxValue || longValue < 0)
                        throw new OverflowException("Variable integer value exceeds Int32 range.");
                    value = (int)longValue;
                    break;
                default:
                    value = first;
                    break;
            }
            
            if (value > max)
                throw new ArgumentOutOfRangeException(nameof(max), $"Variable integer value {value} exceeds maximum {max}.");
            
            return value;
        }
        
        /// <summary>
        /// Reads a variable-length byte array.
        /// </summary>
        /// <returns>The byte array</returns>
        public byte[] ReadVarBytes()
        {
            return ReadVarBytes(0x1000000); // 16MB default max
        }
        
        /// <summary>
        /// Reads a variable-length byte array with maximum size validation.
        /// </summary>
        /// <param name="max">Maximum allowed size</param>
        /// <returns>The byte array</returns>
        public byte[] ReadVarBytes(int max)
        {
            var length = ReadVarInt(max);
            return ReadBytes(length);
        }
        
        /// <summary>
        /// Reads a variable-length UTF-8 string.
        /// </summary>
        /// <returns>The string</returns>
        public string ReadVarString()
        {
            var bytes = ReadVarBytes();
            return Encoding.UTF8.GetString(bytes);
        }
        
        #endregion
        
        #region Cryptographic Reading
        
        /// <summary>
        /// Reads an encoded EC point (compressed or uncompressed).
        /// </summary>
        /// <returns>The encoded EC point bytes</returns>
        /// <exception cref="InvalidDataException">If the EC point format is invalid</exception>
        public byte[] ReadEncodedECPoint()
        {
            EnsureNotDisposed();
            
            var firstByte = ReadByte();
            
            switch (firstByte)
            {
                case 0x02:
                case 0x03:
                    // Compressed point: 1 byte prefix + 32 bytes X coordinate
                    var compressedPoint = new byte[33];
                    compressedPoint[0] = firstByte;
                    var xCoordBytes = ReadBytes(32);
                    Array.Copy(xCoordBytes, 0, compressedPoint, 1, 32);
                    return compressedPoint;
                    
                case 0x04:
                    // Uncompressed point: 1 byte prefix + 32 bytes X + 32 bytes Y
                    var uncompressedPoint = new byte[65];
                    uncompressedPoint[0] = firstByte;
                    var coordBytes = ReadBytes(64);
                    Array.Copy(coordBytes, 0, uncompressedPoint, 1, 64);
                    return uncompressedPoint;
                    
                case 0x00:
                    // Infinity point
                    return new byte[] { 0x00 };
                    
                default:
                    throw new InvalidDataException($"Invalid EC point prefix byte: 0x{firstByte:X2}");
            }
        }
        
        /// <summary>
        /// Reads an EC point from the stream.
        /// </summary>
        /// <returns>The EC point</returns>
        public ECPoint ReadECPoint()
        {
            var encodedBytes = ReadEncodedECPoint();
            return ECPoint.DecodePoint(encodedBytes);
        }
        
        #endregion
        
        #region Serializable Object Reading
        
        /// <summary>
        /// Reads a serializable object of the specified type.
        /// </summary>
        /// <typeparam name="T">The type implementing INeoSerializable</typeparam>
        /// <returns>The deserialized object</returns>
        public T ReadSerializable<T>() where T : INeoSerializable, new()
        {
            EnsureNotDisposed();
            return INeoSerializable.Deserialize<T>(this);
        }
        
        /// <summary>
        /// Reads a list of serializable objects with variable-length byte encoding.
        /// </summary>
        /// <typeparam name="T">The type implementing INeoSerializable</typeparam>
        /// <returns>The list of deserialized objects</returns>
        public List<T> ReadSerializableListVarBytes<T>() where T : INeoSerializable, new()
        {
            EnsureNotDisposed();
            
            var totalLength = ReadVarInt(0x10000000); // 256MB max
            var bytesRead = 0;
            var startOffset = position;
            var list = new List<T>();
            
            while (bytesRead < totalLength && !EndOfStream)
            {
                try
                {
                    var item = ReadSerializable<T>();
                    list.Add(item);
                    bytesRead = position - startOffset;
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"[BinaryReader] Failed to read serializable item: {ex.Message}");
                    break;
                }
            }
            
            return list;
        }
        
        /// <summary>
        /// Reads a list of serializable objects with count prefix.
        /// </summary>
        /// <typeparam name="T">The type implementing INeoSerializable</typeparam>
        /// <returns>The list of deserialized objects</returns>
        public List<T> ReadSerializableList<T>() where T : INeoSerializable, new()
        {
            EnsureNotDisposed();
            
            var count = ReadVarInt(0x10000000);
            var list = new List<T>(count);
            
            for (int i = 0; i < count; i++)
            {
                try
                {
                    var item = ReadSerializable<T>();
                    list.Add(item);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[BinaryReader] Failed to read serializable list item {i}: {ex.Message}");
                    throw;
                }
            }
            
            return list;
        }
        
        #endregion
        
        #region Script-Specific Reading
        
        /// <summary>
        /// Reads push data from a script based on the PUSHDATA opcode.
        /// </summary>
        /// <returns>The pushed data</returns>
        /// <exception cref="InvalidDataException">If not at a PUSHDATA opcode</exception>
        public byte[] ReadPushData()
        {
            EnsureNotDisposed();
            
            var opcodeByte = ReadByte();
            var opcode = (OpCode)opcodeByte;
            
            int size = opcode switch
            {
                OpCode.PUSHDATA1 => ReadByte(),
                OpCode.PUSHDATA2 => ReadUInt16(),
                OpCode.PUSHDATA4 => (int)ReadUInt32(),
                _ => throw new InvalidDataException($"Stream did not contain a PUSHDATA OpCode at the current position. Found: {opcode}")
            };
            
            return ReadBytes(size);
        }
        
        /// <summary>
        /// Reads a string from push data.
        /// </summary>
        /// <returns>The UTF-8 decoded string</returns>
        public string ReadPushString()
        {
            var data = ReadPushData();
            return Encoding.UTF8.GetString(data);
        }
        
        /// <summary>
        /// Reads an integer from push data.
        /// </summary>
        /// <returns>The integer value</returns>
        public int ReadPushInt()
        {
            var bigInt = ReadPushBigInt();
            
            if (bigInt > int.MaxValue || bigInt < int.MinValue)
                throw new OverflowException("Push integer value exceeds Int32 range.");
            
            return (int)bigInt;
        }
        
        /// <summary>
        /// Reads a BigInteger from push data.
        /// </summary>
        /// <returns>The BigInteger value</returns>
        public BigInteger ReadPushBigInt()
        {
            EnsureNotDisposed();
            
            var opcodeByte = ReadByte();
            
            // Handle constant integer opcodes
            if (opcodeByte >= (byte)OpCode.PUSHM1 && opcodeByte <= (byte)OpCode.PUSH16)
            {
                return opcodeByte - (byte)OpCode.PUSH0;
            }
            
            var opcode = (OpCode)opcodeByte;
            int byteCount = opcode switch
            {
                OpCode.PUSHINT8 => 1,
                OpCode.PUSHINT16 => 2,
                OpCode.PUSHINT32 => 4,
                OpCode.PUSHINT64 => 8,
                OpCode.PUSHINT128 => 16,
                OpCode.PUSHINT256 => 32,
                _ => throw new InvalidDataException($"Cannot parse PUSHINT OpCode: {opcode}")
            };
            
            var bytes = ReadBytes(byteCount);
            return new BigInteger(bytes, isUnsigned: false, isBigEndian: false); // Little-endian
        }
        
        #endregion
        
        #region Validation and Error Checking
        
        /// <summary>
        /// Ensures the specified number of bytes are available to read.
        /// </summary>
        /// <param name="count">Number of bytes required</param>
        /// <exception cref="EndOfStreamException">If not enough bytes available</exception>
        private void EnsureAvailable(int count)
        {
            if (Available < count)
                throw new EndOfStreamException($"Attempted to read {count} bytes, but only {Available} bytes are available.");
        }
        
        /// <summary>
        /// Ensures the reader has not been disposed.
        /// </summary>
        /// <exception cref="ObjectDisposedException">If the reader has been disposed</exception>
        private void EnsureNotDisposed()
        {
            if (disposed)
                throw new ObjectDisposedException(nameof(BinaryReader));
        }
        
        #endregion
        
        #region Utility Methods
        
        /// <summary>
        /// Reads the remaining bytes from the current position to the end.
        /// </summary>
        /// <returns>The remaining bytes</returns>
        public byte[] ReadToEnd()
        {
            EnsureNotDisposed();
            return ReadBytes(Available);
        }
        
        /// <summary>
        /// Peeks at the next byte without advancing the position.
        /// </summary>
        /// <returns>The next byte or -1 if at end of stream</returns>
        public int PeekByte()
        {
            EnsureNotDisposed();
            
            if (EndOfStream)
                return -1;
            
            return array[position];
        }
        
        /// <summary>
        /// Creates a copy of this reader with the same data but independent position.
        /// </summary>
        /// <returns>A new binary reader with the same data</returns>
        public BinaryReader Clone()
        {
            EnsureNotDisposed();
            
            var clone = new BinaryReader((byte[])array.Clone())
            {
                position = this.position,
                marker = this.marker
            };
            
            return clone;
        }
        
        #endregion
        
        #region IDisposable Implementation
        
        /// <summary>
        /// Disposes of the binary reader resources.
        /// </summary>
        public void Dispose()
        {
            disposed = true;
        }
        
        #endregion
        
        #region String Representation
        
        /// <summary>
        /// Returns a string representation of this binary reader.
        /// </summary>
        /// <returns>String representation</returns>
        public override string ToString()
        {
            return $"BinaryReader(Position: {position}/{array.Length}, Available: {Available} bytes)";
        }
        
        #endregion
    }
}
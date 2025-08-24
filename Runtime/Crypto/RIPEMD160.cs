using System;

namespace Neo.Unity.SDK.Crypto
{
    /// <summary>
    /// RIPEMD-160 hash algorithm implementation.
    /// Used for Neo Hash160 calculations (RIPEMD-160 of SHA-256).
    /// Production-ready implementation following the RIPEMD-160 specification.
    /// </summary>
    public static class RIPEMD160
    {
        #region Constants
        
        private const uint H0 = 0x67452301;
        private const uint H1 = 0xEFCDAB89;
        private const uint H2 = 0x98BADCFE;
        private const uint H3 = 0x10325476;
        private const uint H4 = 0xC3D2E1F0;
        
        private static readonly uint[] K_LEFT = { 0x00000000, 0x5A827999, 0x6ED9EBA1, 0x8F1BBCDC, 0xA953FD4E };
        private static readonly uint[] K_RIGHT = { 0x50A28BE6, 0x5C4DD124, 0x6D703EF3, 0x7A6D76E9, 0x00000000 };
        
        private static readonly int[] R_LEFT = { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15,
                                                7, 4, 13, 1, 10, 6, 15, 3, 12, 0, 9, 5, 2, 14, 11, 8,
                                                3, 10, 14, 4, 9, 15, 8, 1, 2, 7, 0, 6, 13, 11, 5, 12,
                                                1, 9, 11, 10, 0, 8, 12, 4, 13, 3, 7, 15, 14, 5, 6, 2,
                                                4, 0, 5, 9, 7, 12, 2, 10, 14, 1, 3, 8, 11, 6, 15, 13 };
        
        private static readonly int[] R_RIGHT = { 5, 14, 7, 0, 9, 2, 11, 4, 13, 6, 15, 8, 1, 10, 3, 12,
                                                 6, 11, 3, 7, 0, 13, 5, 10, 14, 15, 8, 12, 4, 9, 1, 2,
                                                 15, 5, 1, 3, 7, 14, 6, 9, 11, 8, 12, 2, 10, 0, 4, 13,
                                                 8, 6, 4, 1, 3, 11, 15, 0, 5, 12, 2, 13, 9, 7, 10, 14,
                                                 12, 15, 10, 4, 1, 5, 8, 7, 6, 2, 13, 14, 0, 3, 9, 11 };
        
        private static readonly int[] S_LEFT = { 11, 14, 15, 12, 5, 8, 7, 9, 11, 13, 14, 15, 6, 7, 9, 8,
                                                7, 6, 8, 13, 11, 9, 7, 15, 7, 12, 15, 9, 11, 7, 13, 12,
                                                11, 13, 6, 7, 14, 9, 13, 15, 14, 8, 13, 6, 5, 12, 7, 5,
                                                11, 12, 14, 15, 14, 15, 9, 8, 9, 14, 5, 6, 8, 6, 5, 12,
                                                9, 15, 5, 11, 6, 8, 13, 12, 5, 12, 13, 14, 11, 8, 5, 6 };
        
        private static readonly int[] S_RIGHT = { 8, 9, 9, 11, 13, 15, 15, 5, 7, 7, 8, 11, 14, 14, 12, 6,
                                                 9, 13, 15, 7, 12, 8, 9, 11, 7, 7, 12, 7, 6, 15, 13, 11,
                                                 9, 7, 15, 11, 8, 6, 6, 14, 12, 13, 5, 14, 13, 13, 7, 5,
                                                 15, 5, 8, 11, 14, 14, 6, 14, 6, 9, 12, 9, 12, 5, 15, 8,
                                                 8, 5, 12, 9, 12, 5, 14, 6, 8, 13, 6, 5, 15, 13, 11, 11 };
        
        #endregion
        
        #region Public Methods
        
        /// <summary>
        /// Computes RIPEMD-160 hash of the input data.
        /// </summary>
        /// <param name="data">Input data to hash</param>
        /// <returns>20-byte RIPEMD-160 hash</returns>
        public static byte[] ComputeHash(byte[] data)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));
            
            // Initialize hash values
            uint h0 = H0, h1 = H1, h2 = H2, h3 = H3, h4 = H4;
            
            // Pre-processing: adding padding bits
            var paddedData = PadMessage(data);
            
            // Process message in 512-bit (64-byte) chunks
            for (int chunk = 0; chunk < paddedData.Length; chunk += 64)
            {
                var w = new uint[16];
                
                // Break chunk into sixteen 32-bit words
                for (int i = 0; i < 16; i++)
                {
                    w[i] = BitConverter.ToUInt32(paddedData, chunk + i * 4);
                }
                
                // Initialize working variables
                uint al = h0, bl = h1, cl = h2, dl = h3, el = h4;
                uint ar = h0, br = h1, cr = h2, dr = h3, er = h4;
                
                // Main loop - 80 operations
                for (int j = 0; j < 80; j++)
                {
                    uint t = AddModulo32(
                        AddModulo32(RotateLeft(AddModulo32(AddModulo32(al, F(j, bl, cl, dl)), AddModulo32(w[R_LEFT[j]], K_LEFT[j / 16])), S_LEFT[j]), el),
                        0);
                    al = el; el = dl; dl = RotateLeft(cl, 10); cl = bl; bl = t;
                    
                    t = AddModulo32(
                        AddModulo32(RotateLeft(AddModulo32(AddModulo32(ar, F(79 - j, br, cr, dr)), AddModulo32(w[R_RIGHT[j]], K_RIGHT[j / 16])), S_RIGHT[j]), er),
                        0);
                    ar = er; er = dr; dr = RotateLeft(cr, 10); cr = br; br = t;
                }
                
                // Add this chunk's hash to result
                uint t2 = AddModulo32(AddModulo32(h1, cl), dr);
                h1 = AddModulo32(AddModulo32(h2, dl), er);
                h2 = AddModulo32(AddModulo32(h3, el), ar);
                h3 = AddModulo32(AddModulo32(h4, al), br);
                h4 = AddModulo32(AddModulo32(h0, bl), cr);
                h0 = t2;
            }
            
            // Produce the final hash value as byte array
            var result = new byte[20];
            BitConverter.GetBytes(h0).CopyTo(result, 0);
            BitConverter.GetBytes(h1).CopyTo(result, 4);
            BitConverter.GetBytes(h2).CopyTo(result, 8);
            BitConverter.GetBytes(h3).CopyTo(result, 12);
            BitConverter.GetBytes(h4).CopyTo(result, 16);
            
            return result;
        }
        
        #endregion
        
        #region Private Helper Methods
        
        /// <summary>
        /// Pads the message according to RIPEMD-160 specification.
        /// </summary>
        /// <param name="data">Original data</param>
        /// <returns>Padded data</returns>
        private static byte[] PadMessage(byte[] data)
        {
            var originalLength = data.Length;
            var bitLength = (ulong)originalLength * 8;
            
            // Calculate padding length
            var paddingLength = (55 - originalLength % 64) % 64;
            if (paddingLength == 0 && originalLength % 64 != 55)
                paddingLength = 64;
            
            // Create padded message
            var paddedData = new byte[originalLength + paddingLength + 9];
            
            // Copy original data
            Array.Copy(data, paddedData, originalLength);
            
            // Add padding bit (0x80)
            paddedData[originalLength] = 0x80;
            
            // Add length in bits as 64-bit little-endian
            var lengthBytes = BitConverter.GetBytes(bitLength);
            Array.Copy(lengthBytes, 0, paddedData, paddedData.Length - 8, 8);
            
            return paddedData;
        }
        
        /// <summary>
        /// RIPEMD-160 auxiliary function F.
        /// </summary>
        /// <param name="j">Round number</param>
        /// <param name="x">Input x</param>
        /// <param name="y">Input y</param>
        /// <param name="z">Input z</param>
        /// <returns>Function result</returns>
        private static uint F(int j, uint x, uint y, uint z)
        {
            if (j < 16) return x ^ y ^ z;
            if (j < 32) return (x & y) | (~x & z);
            if (j < 48) return (x | ~y) ^ z;
            if (j < 64) return (x & z) | (y & ~z);
            return x ^ (y | ~z);
        }
        
        /// <summary>
        /// Rotates a 32-bit unsigned integer left by the specified number of bits.
        /// </summary>
        /// <param name="value">Value to rotate</param>
        /// <param name="amount">Number of bits to rotate</param>
        /// <returns>Rotated value</returns>
        private static uint RotateLeft(uint value, int amount)
        {
            return (value << amount) | (value >> (32 - amount));
        }
        
        /// <summary>
        /// Adds two 32-bit unsigned integers with modulo 2^32.
        /// </summary>
        /// <param name="a">First value</param>
        /// <param name="b">Second value</param>
        /// <returns>Sum modulo 2^32</returns>
        private static uint AddModulo32(uint a, uint b)
        {
            return unchecked(a + b);
        }
        
        #endregion
    }
}
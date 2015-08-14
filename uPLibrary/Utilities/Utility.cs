using System;
using Microsoft.SPOT;

namespace uPLibrary.Utilities
{
    /// <summary>
    /// Utility class
    /// </summary>
    public static class Utility
    {
        /// <summary>
        /// Reverse bits order inside a byte (MSB to LSB and viceversa)
        /// </summary>
        /// <param name="value">Byte value to reverse</param>
        /// <returns>Byte value after reverse</returns>
        public static byte ReverseBits(byte value)
        {
            byte result = 0x00;

            int i = 7, j = 0;

            while (i >= 0)
            {
                result |= (byte)(((value >> i) & 0x01) << j);
                i--;
                j++;
            }
            return result;
        }
    }
}

using System;
using Microsoft.SPOT;

namespace MaCRo.Tools
{
    public class BitConverter
    {
        public static byte[] GetBytes(ushort value)
        {
            return new byte[] { (byte)(value & 255), (byte)((value >> 8) & 255) };
        }

        public static byte[] GetBytes(uint value)
        {
            return new byte[] { (byte)(value & 255), (byte)((value >> 8) & 255), (byte)((value >> 16) & 255), (byte)((value >> 24) & 255) };
        }

        public static byte[] GetBytes(int value)
        {
            return new byte[] { (byte)(value & 255), (byte)((value >> 8) & 255), (byte)((value >> 16) & 255), (byte)((value >> 24) & 255) };
        }

        public static ushort ToUInt16(byte[] value, int startIndex)
        {
            // From http://msdn.microsoft.com/en-us/library/system.bitconverter.touint16(VS.96).aspx
            if (value == null) throw new ArgumentNullException();
            else if (startIndex == value.Length - 1) throw new ArgumentException();
            else if (startIndex < 0 || startIndex >= value.Length) throw new ArgumentOutOfRangeException();

            return (ushort)(
               (value[startIndex] & 0x000000FF) |
               (value[startIndex + 1] << 8 & 0x0000FF00)
            );
        }

        public static int ToInt32(byte[] value, int startIndex)
        {
            // From http://msdn.microsoft.com/en-us/library/system.bitconverter.touint32(VS.96).aspx
            if (value == null) throw new ArgumentNullException();
            else if (startIndex < 0 || startIndex >= value.Length) throw new ArgumentOutOfRangeException();
            else if (startIndex >= value.Length - 3) throw new ArgumentException();

            return (int)(
               (value[startIndex] & 0x000000FF) |
               (value[startIndex + 1] << 8 & 0x0000FF00) |
               (value[startIndex + 2] << 16 & 0x00FF0000) |
               (value[startIndex + 3] << 24)
            );
        }

        public static uint ToUInt32(byte[] value, int startIndex)
        {
            // From http://msdn.microsoft.com/en-us/library/system.bitconverter.touint32(VS.96).aspx
            if (value == null) throw new ArgumentNullException();
            else if (startIndex < 0 || startIndex >= value.Length) throw new ArgumentOutOfRangeException();
            else if (startIndex >= value.Length - 3) throw new ArgumentException();

            return (uint)(
               (value[startIndex] & 0x000000FF) |
               (value[startIndex + 1] << 8 & 0x0000FF00) |
               (value[startIndex + 2] << 16 & 0x00FF0000) |
               (value[startIndex + 3] << 24)
            );
        }
    }
}

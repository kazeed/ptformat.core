using System;

namespace Ptformat.Core.Readers
{
    public static class EndianReaderUtils
    {
        public static int Read2(byte[] buf, bool bigendian)
        {
            if (buf is null || buf.Length < 2)
            {
                throw new ArgumentNullException(nameof(buf));
            }

            return bigendian ? (buf[0] << 8) | buf[1] : (buf[1] << 8) | buf[0];
        }

        public static int Read3(byte[] buf, bool bigendian)
        {
            if (buf is null || buf.Length < 3)
            {
                throw new ArgumentNullException(nameof(buf));
            }

            return bigendian ? (buf[0] << 16) | (buf[1] << 8) | buf[2] : (buf[2] << 16) | (buf[1] << 8) | buf[0];
        }

        public static int Read4(byte[] buf, bool bigendian)
        {
            if (buf is null || buf.Length < 4)
            {
                throw new ArgumentNullException(nameof(buf));
            }

            return bigendian
                ? (buf[0] << 24) | (buf[1] << 16) | (buf[2] << 8) | buf[3]
                : (buf[3] << 24) | (buf[2] << 16) | (buf[1] << 8) | buf[0];
        }

        public static long Read5(byte[] buf, bool bigendian)
        {
            if (buf is null || buf.Length < 5)
            {
                throw new ArgumentNullException(nameof(buf));
            }

            return bigendian
                ? ((long)buf[0] << 32) | ((long)buf[1] << 24) | ((long)buf[2] << 16) | ((long)buf[3] << 8) | buf[4]
                : ((long)buf[4] << 32) | ((long)buf[3] << 24) | ((long)buf[2] << 16) | ((long)buf[1] << 8) | buf[0];
        }

        public static long Read8(byte[] buf, bool bigendian)
        {
            if (buf is null || buf.Length < 8)
            {
                throw new ArgumentNullException(nameof(buf));
            }

            return bigendian
                ? ((long)buf[0] << 56)
                  | ((long)buf[1] << 48)
                  | ((long)buf[2] << 40)
                  | ((long)buf[3] << 32)
                  | ((long)buf[4] << 24)
                  | ((long)buf[5] << 16)
                  | ((long)buf[6] << 8)
                  | buf[7]
                : ((long)buf[7] << 56)
                  | ((long)buf[6] << 48)
                  | ((long)buf[5] << 40)
                  | ((long)buf[4] << 32)
                  | ((long)buf[3] << 24)
                  | ((long)buf[2] << 16)
                  | ((long)buf[1] << 8)
                  | buf[0];
        }
    }
}

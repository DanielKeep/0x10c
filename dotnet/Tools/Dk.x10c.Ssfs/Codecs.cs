using System;
using System.Collections.Generic;

namespace Dk.x10c.Ssfs
{
    public static class Codecs
    {
        public static int PackInto(this string s, ushort[] dest, int offset = 0, bool littleEndian = false, byte fill = 0)
        {
            int lo = 0, ho = 1;
            if (!littleEndian)
            {
                lo = 1;
                ho = 0;
            }

            var fullWordChars = s.Length >> 1;
            var stop = offset + fullWordChars;
            for (int i = offset, j = 0; i < stop; ++i, j += 2)
            {
                var lc = s[j + lo] & 0x7F;
                var hc = s[j + ho] & 0x7F;
                dest[i] = (ushort)(lc + (hc << 8));
            }
            if ((s.Length & 1) == 1)
            {
                var lc = s[s.Length - 1] & 0x7F;
                var hc = fill;
                dest[offset + fullWordChars] = (ushort)(lc + (hc << 8));
            }

            return fullWordChars;
        }

        public static int PackInto(this string s, ArraySegment<ushort> dest, int offset = 0, bool littleEndian = false, byte fill = 0)
        {
            return s.PackInto(dest.Array, offset + dest.Offset, littleEndian, fill);
        }

        public static string UnpackString(this ushort[] src, int offset = 0, int limit = -1, bool littleEndian = false, byte term = 0)
        {
            if (limit < 0)
                limit = src.Length - offset;

            int lo = 0, ho = 1;
            if (!littleEndian)
            {
                lo = 1;
                ho = 0;
            }

            var bytes = new List<char>();
            var chs = new byte[2];
            var stop = offset + limit;
            for (int i = offset; i < stop; ++i)
            {
                var w = src[i];
                chs[lo] = (byte)(w);
                chs[ho] = (byte)(w >> 8);

                if (chs[lo] == term)
                    break;
                bytes.Add((char)chs[lo]);

                if (chs[ho] == term)
                    break;
                bytes.Add((char)chs[ho]);
            }

            return new String(bytes.ToArray());
        }

        public static string UnpackString(this ArraySegment<ushort> src, int offset = 0, int limit = -1, bool littleEndian = false, byte term = 0)
        {
            return src.Array.UnpackString(offset + src.Offset, limit, littleEndian, term);
        }
    }
}

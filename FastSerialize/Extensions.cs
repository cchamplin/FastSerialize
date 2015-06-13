using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FastSerialize
{
    public static class Extensions
    {
        public static IEnumerable<byte> AsEnumerableByte(this Stream stream)
        {

            if (stream != null)
            {

                for (int x = stream.ReadByte(); x != -1; x = stream.ReadByte())
                    yield return (byte)x;
            }
        }
        public static IEnumerable<char> AsEnumerableChar(this Stream stream)
        {

            if (stream != null)
            {
                for (int x = stream.ReadByte(); x != -1; x = stream.ReadByte())
                    yield return (char)x;
            }
        }
    }
}

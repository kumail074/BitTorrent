using System;
using System.Text;
using System.Linq;
using System.Collections.Generic;

namespace Torrent
{
    public static class BEncoding
    {
        private static byte DictionaryStart = System.Text.Encoding.UTF8.GetBytes("d")[0];
        private static byte DictionaryEnd = System.Text.Encoding.UTF8.GetBytes("e")[0];
        private static byte ListStart = System.Text.Encoding.UTF8.GetBytes("l")[0];
        private static byte ListEnd = System.Text.Encoding.UTF8.GetBytes("e")[0];
        private static byte NumberStart = System.Text.Encoding.UTF8.GetBytes("i")[0];
        private static byte NumberEnd = System.Text.Encoding.UTF8.GetBytes("e")[0];
        private static byte ByteArrayDivider = System.Text.Encoding.UTF8.GetBytes(":")[0];

        public static object Decode(byte[] bytes)
        {
            IEnumerator<byte> enumerator = ((IEnumerable<byte>)bytes).GetEnumerator();
            enumerator.MoveNext();
            return DecodeNextObject(enumerator);
        }
    }
}
    
    
    
    
    
public class Class1
{
}
using System;
using System.Collections;
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

        public static object DecodeNextObject(IEnumerator<byte> enumerator)
        {
            if (enumerator.Current == DictionaryStart)
                return DecodeDictionary(enumerator);

            if (enumerator.Current == ListStart)
                return DecodeList(enumerator);
            
            if (enumerator.Current == NumberStart)
                return DecodeNumber(enumerator);

            return DecodeByteArray(enumerator);
        }

        public static object DecodeFile(string file)
        {
            if(!File.Exists(file))
                throw new FileNotFoundException("File not found" + file);

            byte[] bytes = File.ReadAllBytes(file);
            return BEncoding.Decode(bytes);
        }

        public static long DecodeNumber(IEnumerator enumerator)
        {
            List<byte> bytes = new List<byte>();
            while (enumerator.MoveNext())
            {
                if (enumerator.Current == NumberEnd)
                    break;

                bytes.Add(enumerator.Current);
            }
            string numAsString = Encoding.UTF8.GetString(bytes.ToArray());
            return Int64.Parse(numAsString);
        }
    }
}
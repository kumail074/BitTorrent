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

        private static object DecodeNextObject(IEnumerator<byte> enumerator)
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

        private static byte[] DecodeByteArray(IEnumerator<byte> enumerator)
        {
            List<byte> lengthBytes = new List<byte>();

            do
            {
                if (enumerator.Current == ByteArrayDivider)
                    break;
                lengthBytes.Add(enumerator.Current);
            } while (enumerator.MoveNext());
            
            string lengthString = System.Text.Encoding.UTF8.GetString(lengthBytes.ToArray());
            int length;
            if (!Int32.TryParse(lengthString, out length))
                throw new Exception("unable to parse length of byte array");
            
            byte[] bytes = new byte[length];

            for (int i = 0; i < length; i++)
            {
                enumerator.MoveNext();
                bytes[i] = enumerator.Current;
            }

            return bytes;
        }

        private static List<object> DecodeList(IEnumerator<byte> enumerator)
        {
            List<object> list = new List<object>();

            while (enumerator.MoveNext())
            {
                if (enumerator.Current == ListEnd)
                    break;
                list.Add(DecodeNextObject(enumerator));
            }

            return list;
        }

        private static Dictionary<string, object> DecodeDictionary(IEnumerator<byte> enumerator)
        {
            Dictionary<string, object> dict = new Dictionary<string, object>();
            List<string> keys = new List<string>();

            while (enumerator.MoveNext())
            {
                if (enumerator.Current == DictionaryEnd)
                    break;

                string key = Encoding.UTF8.GetString(DecodeByteArray(enumerator));
                enumerator.MoveNext();
                object value = DecodeNextObject(enumerator);

                keys.Add(key);
                dict.Add(key, value)
            }

            var sortedkeys = keys.OrderBy(x => BitConverter.ToString(Encoding.UTF8.GetBytes(x)));
            if (!keys.SequenceEqual(sortedkeys))
                throw new Exception("Error leading dictionary: keys not sorted");

            return dict;
        }
    }
    
    
}
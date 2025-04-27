using System;
using System.Collections;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using System.Net.Http.Headers;

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

        /*  Decoding Process */
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

        private static long DecodeNumber(IEnumerator<byte> enumerator)
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
                dict.Add(key, value);
            }

            var sortedkeys = keys.OrderBy(x => BitConverter.ToString(Encoding.UTF8.GetBytes(x)));
            if (!keys.SequenceEqual(sortedkeys))
                throw new Exception("Error leading dictionary: keys not sorted");

            return dict;
        }
        
        /* Encoding Process */
    
        public static byte[] Encode(object obj)
        {
            MemoryStream buffer = new MemoryStream();
            EncodeNextObject(buffer, obj);
            return buffer.ToArray();
        }

        public static void EncodeToFile(object obj, string path)
        {
            File.WriteAllBytes(path, Encode(obj));
        }

        private static void EncodeNextObject(MemoryStream buffer, object obj)
        {
            if (obj is byte[])
                EncodeByteArray(buffer, (byte[])obj);
        
            else if (obj is string)
                EncodeString(buffer, (string)obj);
        
            else if (obj is long)
                EncodeNumber(buffer, ((long)obj));
        
            else if (obj.GetType() == typeof(List<object>))
                EncodeList(buffer, ((List<object>)obj));
        
            else if (obj.GetType() == typeof(Dictionary<string, object>))
                EncodeDictionary(buffer, ((Dictionary<string, object>)obj));
        
            else
                throw new Exception("Unable to encode type: " + obj.GetType());
        }

        private static void EncodeNumber(MemoryStream buffer, long input)
        {
            buffer.Append(NumberStart);
            buffer.Append(Encoding.UTF8.GetString(Convert.ToString(input)));
            buffer.Append(NumberEnd);
        }

        private static void EncodeByteArray(MemoryStream buffer, byte[] body)
        {
            buffer.Append(Encoding.UTF8.GetBytes(Convert.ToString(body) ?? throw new InvalidOperationException()));
            buffer.Append(ByteArrayDivider);
            buffer.Append(body);
        }

        private static void EncodeString(MemoryStream buffer, string input)
        {
            EncodeByteArray(buffer, Encoding.UTF8.GetBytes(input));
        }

        private static void EncodeList(MemoryStream buffer, List<object> input)
        {
            buffer.Append(ListStart);
            foreach (var item in input)
                EncodeNextObject(buffer, item);
            buffer.Append(ListEnd);
        }

        private static void EncodeDictionary(MemoryStream buffer, Dictionary<string, object> input)
        {
            buffer.Append(DictionaryStart);
            var sortedkeys = input.Keys.ToList().OrderBy(x => BitConverter.ToString(Encoding.UTF8.GetBytes(x)));
            foreach (var key in sortedkeys)
            {
                EncodeString(buffer, key);
                EncodeNextObject(buffer, input[key]);
            }
            buffer.Append(DictionaryEnd);
        }
    }   
    
    public static class MemoryStreamExtensions
    {
        public static void Append(this MemoryStream stream, byte value)
        {
            stream.Append(new[] { value });
        }

        public static void Append(this MemoryStream stream, byte[] values)
        {
            stream.Write(values, 0, values.Length);
        }
    }
    
    
    
}
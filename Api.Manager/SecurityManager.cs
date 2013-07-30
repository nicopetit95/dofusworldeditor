using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;

namespace Api.Manager
{
    class SecurityManager
    {
        public static string CryptMD5(string original)
        {
            Byte[] originalBytes;
            Byte[] encodedBytes;
            MD5 md5;

            md5 = new MD5CryptoServiceProvider();
            originalBytes = ASCIIEncoding.Default.GetBytes(original);
            encodedBytes = md5.ComputeHash(originalBytes);

            return BitConverter.ToString(encodedBytes);
        }

        public static string EncodeTo64(string toEncode)
        {
            byte[] toEncodeAsBytes = System.Text.Encoding.Unicode.GetBytes(toEncode);
            string returnValue = System.Convert.ToBase64String(toEncodeAsBytes);

            return returnValue;
        }

        public static string CryptWithKey(string original, string key)
        {
            var _Crypted = string.Empty;

            for (var i = 0; i < original.Length; i++)
            {
                var PPass = original[i];
                var PKey = key[i];
                var APass = (int)PPass / 16;
                var AKey = (int)PPass % 16;
                var ANB = (APass + (int)PKey) % Utilities.Hash.Length;
                var ANB2 = (AKey + (int)PKey) % Utilities.Hash.Length;

                _Crypted += Utilities.Hash[ANB];
                _Crypted += Utilities.Hash[ANB2];
            }

            return _Crypted;
        }

        public static string DecodeFrom64(string encodedData)
        {
            byte[] encodedDataAsBytes = System.Convert.FromBase64String(encodedData);
            string returnValue = System.Text.Encoding.Unicode.GetString(encodedDataAsBytes);

            return returnValue;
        }
    }
}

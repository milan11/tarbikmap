namespace TarbikMap.Common
{
    using System;
    using System.Security.Cryptography;
    using System.Text;
    using System.Text.RegularExpressions;

    public static class NameGenerator
    {
        public static string CreateName(string origName)
        {
            return (CreateShort(Regex.Replace(origName, "[^a-zA-Z0-9_]", "_")) + "," + Sha256(origName)).ToUpperInvariant();
        }

        private static string Sha256(string str)
        {
            using var crypt = new SHA256Managed();
            return Convert.ToBase64String(crypt.ComputeHash(Encoding.UTF8.GetBytes(str))).Replace('+', '-').Replace('/', '_');
        }

        private static string CreateShort(string orig)
        {
            if (orig.Length > 128)
            {
                return orig.Substring(0, 128);
            }
            else
            {
                return orig;
            }
        }
    }
}
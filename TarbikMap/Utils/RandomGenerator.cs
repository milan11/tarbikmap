namespace TarbikMap.Utils
{
    using System;
    using System.Security.Cryptography;
    using System.Text;

    public class RandomGenerator
    {
        public string UppercaseString(int length)
        {
            StringBuilder sb = new StringBuilder(length);

            for (int i = 0; i < length; ++i)
            {
                using (RandomNumberGenerator g = RandomNumberGenerator.Create())
                {
                    byte[] randomBytes = new byte[4];
                    g.GetBytes(randomBytes);
                    sb.Append((char)('A' + (BitConverter.ToUInt32(randomBytes) % 26)));
                }
            }

            return sb.ToString();
        }
    }
}

using System;
using System.Security.Cryptography;
using System.Text;

namespace Timeline.Helpers
{
    public static class SecureRandomExtensions
    {
        private static readonly char[] AlphaDigitString = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();

        public static string GenerateAlphaDigitString(this RandomNumberGenerator randomNumberGenerator, int length)
        {
            if (length <= 0) throw new ArgumentOutOfRangeException(nameof(length));

            var buffer = new byte[length];
            randomNumberGenerator.GetBytes(buffer);

            StringBuilder stringBuilder = new();

            foreach (byte b in buffer)
            {
                stringBuilder.Append(AlphaDigitString[b % AlphaDigitString.Length]);
            }

            return stringBuilder.ToString();
        }
    }
}


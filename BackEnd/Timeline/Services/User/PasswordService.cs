using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using System;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;

namespace Timeline.Services.User
{
    /// <summary>
    /// Hashed password is of bad format.
    /// </summary>
    /// <seealso cref="IPasswordService.VerifyPassword(string, string)"/>
    [Serializable]
    public class HashedPasswordBadFromatException : Exception
    {
        public HashedPasswordBadFromatException() : base(Resource.ExceptionHashedPasswordBadFormat) { }

        public HashedPasswordBadFromatException(string message) : base(message) { }
        public HashedPasswordBadFromatException(string message, Exception inner) : base(message, inner) { }

        public HashedPasswordBadFromatException(string hashedPassword, string reason, Exception? inner = null) 
            : base(string.Format(CultureInfo.InvariantCulture, Resource.ExceptionHashedPasswordBadFormat, reason), inner) { HashedPassword = hashedPassword; }
        protected HashedPasswordBadFromatException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }

        public string? HashedPassword { get; set; }
    }

    public interface IPasswordService
    {
        /// <summary>
        /// Hash a password.
        /// </summary>
        /// <param name="password">The password to hash.</param>
        /// <returns>A hashed representation of the supplied <paramref name="password"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="password"/> is null.</exception>
        string HashPassword(string password);

        /// <summary>
        /// Verify whether the password fits into the hashed one.
        /// 
        /// Usually you only need to check the returned bool value.
        /// Catching <see cref="HashedPasswordBadFromatException"/> usually is not necessary.
        /// Because if your program logic is right and always call <see cref="HashPassword(string)"/>
        /// and <see cref="VerifyPassword(string, string)"/> in pair, this exception will never be thrown.
        /// A thrown one usually means the data you saved is corupted, which is a critical problem.
        /// </summary>
        /// <param name="hashedPassword">The hashed password.</param>
        /// <param name="providedPassword">The password supplied for comparison.</param>
        /// <returns>True indicating password is right. Otherwise false.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="hashedPassword"/> or <paramref name="providedPassword"/> is null.</exception>
        /// <exception cref="HashedPasswordBadFromatException">Thrown when the hashed password is of bad format.</exception>
        bool VerifyPassword(string hashedPassword, string providedPassword);
    }

    /// <summary>
    /// Copied from https://github.com/aspnet/AspNetCore/blob/master/src/Identity/Extensions.Core/src/PasswordHasher.cs
    /// Remove V2 format and unnecessary format version check.
    /// Remove configuration options.
    /// Remove user related parts.
    /// Change the exceptions.
    /// </summary>
    public class PasswordService : IPasswordService
    {
        /* =======================
        * HASHED PASSWORD FORMATS
        * =======================
        * 
        * Version 3:
        * PBKDF2 with HMAC-SHA256, 128-bit salt, 256-bit subkey, 10000 iterations.
        * Format: { 0x01, prf (UInt32), iter count (UInt32), salt length (UInt32), salt, subkey }
        * (All UInt32s are stored big-endian.)
        */

        private readonly RandomNumberGenerator _rng = RandomNumberGenerator.Create();

        public PasswordService()
        {
        }

        // Compares two byte arrays for equality. The method is specifically written so that the loop is not optimized.
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        private static bool ByteArraysEqual(byte[] a, byte[] b)
        {
            if (a == null && b == null)
            {
                return true;
            }
            if (a == null || b == null || a.Length != b.Length)
            {
                return false;
            }
            var areSame = true;
            for (var i = 0; i < a.Length; i++)
            {
                areSame &= (a[i] == b[i]);
            }
            return areSame;
        }

        public string HashPassword(string password)
        {
            if (password == null)
                throw new ArgumentNullException(nameof(password));
            return Convert.ToBase64String(HashPasswordV3(password, _rng));
        }

        private static byte[] HashPasswordV3(string password, RandomNumberGenerator rng)
        {
            return HashPasswordV3(password, rng,
                prf: KeyDerivationPrf.HMACSHA256,
                iterCount: 10000,
                saltSize: 128 / 8,
                numBytesRequested: 256 / 8);
        }

        private static byte[] HashPasswordV3(string password, RandomNumberGenerator rng, KeyDerivationPrf prf, int iterCount, int saltSize, int numBytesRequested)
        {
            // Produce a version 3 (see comment above) text hash.
            byte[] salt = new byte[saltSize];
            rng.GetBytes(salt);
            byte[] subkey = KeyDerivation.Pbkdf2(password, salt, prf, iterCount, numBytesRequested);

            var outputBytes = new byte[13 + salt.Length + subkey.Length];
            outputBytes[0] = 0x01; // format marker
            WriteNetworkByteOrder(outputBytes, 1, (uint)prf);
            WriteNetworkByteOrder(outputBytes, 5, (uint)iterCount);
            WriteNetworkByteOrder(outputBytes, 9, (uint)saltSize);
            Buffer.BlockCopy(salt, 0, outputBytes, 13, salt.Length);
            Buffer.BlockCopy(subkey, 0, outputBytes, 13 + saltSize, subkey.Length);
            return outputBytes;
        }

        public bool VerifyPassword(string hashedPassword, string providedPassword)
        {
            if (hashedPassword == null)
                throw new ArgumentNullException(nameof(hashedPassword));
            if (providedPassword == null)
                throw new ArgumentNullException(nameof(providedPassword));

            byte[] decodedHashedPassword;
            try
            {
                decodedHashedPassword = Convert.FromBase64String(hashedPassword);
            }
            catch (FormatException e)
            {
                throw new HashedPasswordBadFromatException(hashedPassword, Resource.ExceptionHashedPasswordBadFormatReasonNotBase64, e);
            }

            // read the format marker from the hashed password
            if (decodedHashedPassword.Length == 0)
            {
                throw new HashedPasswordBadFromatException(hashedPassword, Resource.ExceptionHashedPasswordBadFormatReasonLength0);
            }

            return (decodedHashedPassword[0]) switch
            {
                0x01 => VerifyHashedPasswordV3(decodedHashedPassword, providedPassword, hashedPassword),
                _ => throw new HashedPasswordBadFromatException(hashedPassword, Resource.ExceptionHashedPasswordBadFormatReasonUnknownMarker),
            };
        }

        private static bool VerifyHashedPasswordV3(byte[] hashedPassword, string password, string hashedPasswordString)
        {
            try
            {
                // Read header information
                KeyDerivationPrf prf = (KeyDerivationPrf)ReadNetworkByteOrder(hashedPassword, 1);
                int iterCount = (int)ReadNetworkByteOrder(hashedPassword, 5);
                int saltLength = (int)ReadNetworkByteOrder(hashedPassword, 9);

                // Read the salt: must be >= 128 bits
                if (saltLength < 128 / 8)
                {
                    throw new HashedPasswordBadFromatException(hashedPasswordString, Resource.ExceptionHashedPasswordBadFormatReasonSaltTooShort);
                }
                byte[] salt = new byte[saltLength];
                Buffer.BlockCopy(hashedPassword, 13, salt, 0, salt.Length);

                // Read the subkey (the rest of the payload): must be >= 128 bits
                int subkeyLength = hashedPassword.Length - 13 - salt.Length;
                if (subkeyLength < 128 / 8)
                {
                    throw new HashedPasswordBadFromatException(hashedPasswordString, Resource.ExceptionHashedPasswordBadFormatReasonSubkeyTooShort);
                }
                byte[] expectedSubkey = new byte[subkeyLength];
                Buffer.BlockCopy(hashedPassword, 13 + salt.Length, expectedSubkey, 0, expectedSubkey.Length);

                // Hash the incoming password and verify it
                byte[] actualSubkey = KeyDerivation.Pbkdf2(password, salt, prf, iterCount, subkeyLength);
                return ByteArraysEqual(actualSubkey, expectedSubkey);
            }
            catch (Exception e)
            {
                // This should never occur except in the case of a malformed payload, where
                // we might go off the end of the array. Regardless, a malformed payload
                // implies verification failed.
                throw new HashedPasswordBadFromatException(hashedPasswordString, Resource.ExceptionHashedPasswordBadFormatReasonOthers, e);
            }
        }

        private static uint ReadNetworkByteOrder(byte[] buffer, int offset)
        {
            return ((uint)(buffer[offset + 0]) << 24)
                | ((uint)(buffer[offset + 1]) << 16)
                | ((uint)(buffer[offset + 2]) << 8)
                | ((uint)(buffer[offset + 3]));
        }

        private static void WriteNetworkByteOrder(byte[] buffer, int offset, uint value)
        {
            buffer[offset + 0] = (byte)(value >> 24);
            buffer[offset + 1] = (byte)(value >> 16);
            buffer[offset + 2] = (byte)(value >> 8);
            buffer[offset + 3] = (byte)(value >> 0);
        }
    }
}

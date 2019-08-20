using System;
using System.Security.Cryptography;

namespace Timeline.Services
{
    public interface IETagGenerator
    {
        string Generate(byte[] source);
    }

    public class ETagGenerator : IETagGenerator, IDisposable
    {
        private readonly SHA1 _sha1;

        public ETagGenerator()
        {
            _sha1 = SHA1.Create();
        }

        public string Generate(byte[] source)
        {
            if (source == null || source.Length == 0)
                throw new ArgumentException("Source is null or empty.", nameof(source));

            return Convert.ToBase64String(_sha1.ComputeHash(source));
        }

        public void Dispose()
        {
            _sha1.Dispose();
        }
    }
}

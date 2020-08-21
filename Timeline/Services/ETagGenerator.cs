using System;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace Timeline.Services
{
    public interface IETagGenerator
    {
        /// <summary>
        /// Generate a etag for given source.
        /// </summary>
        /// <param name="source">The source data.</param>
        /// <returns>The generated etag.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="source"/> is null.</exception>
        Task<string> Generate(byte[] source);
    }

    public sealed class ETagGenerator : IETagGenerator, IDisposable
    {
        private readonly SHA1 _sha1;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Security", "CA5350:Do Not Use Weak Cryptographic Algorithms", Justification = "Sha1 is enough ??? I don't know.")]
        public ETagGenerator()
        {
            _sha1 = SHA1.Create();
        }

        public Task<string> Generate(byte[] source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            return Task.Run(() => Convert.ToBase64String(_sha1.ComputeHash(source)));
        }

        private bool _disposed; // To detect redundant calls

        public void Dispose()
        {
            if (_disposed) return;
            _sha1.Dispose();
            _disposed = true;
        }
    }
}

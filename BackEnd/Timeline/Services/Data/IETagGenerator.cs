using System;
using System.Threading;
using System.Threading.Tasks;

namespace Timeline.Services.Data
{
    public interface IETagGenerator
    {
        /// <summary>
        /// Generate a etag for given source.
        /// </summary>
        /// <param name="source">The source data.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The generated etag.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="source"/> is null.</exception>
        /// <remarks>This function must guarantee the same result with equal given source.</remarks>
        Task<string> GenerateETagAsync(byte[] source, CancellationToken cancellationToken = default);
    }
}

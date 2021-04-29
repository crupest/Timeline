using System;
using System.Threading.Tasks;
using Timeline.Entities;

namespace Timeline.Services.Api
{
    public interface ISearchService
    {
        /// <summary>
        /// Search timelines whose name or title contains query string.
        /// </summary>
        /// <param name="query">String to contain.</param>
        /// <returns>Search results.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="query"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="query"/> is empty.</exception>
        /// <remarks>
        /// Implementation should promise high score is at first.
        /// </remarks>
        Task<SearchResult<TimelineEntity>> SearchTimelineAsync(string query);

        /// <summary>
        /// Search users whose username or nickname contains query string.
        /// </summary>
        /// <param name="query">String to contain.</param>
        /// <returns>Search results.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="query"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="query"/> is empty.</exception>
        /// <remarks>
        /// Implementation should promise high score is at first.
        /// </remarks>
        Task<SearchResult<UserEntity>> SearchUserAsync(string query);
    }
}

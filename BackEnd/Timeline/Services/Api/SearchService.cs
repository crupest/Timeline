using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Timeline.Entities;

namespace Timeline.Services.Api
{
    public class SearchResultItem<TItem>
    {
        public SearchResultItem(TItem item, int score)
        {
            Item = item;
            Score = score;
        }

        public TItem Item { get; set; } = default!;

        /// <summary>
        /// Bigger is better.
        /// </summary>
        public int Score { get; set; }
    }

    public class SearchResult<TItem>
    {
#pragma warning disable CA2227 // Collection properties should be read only
        public List<SearchResultItem<TItem>> Items { get; set; } = new();
#pragma warning restore CA2227 // Collection properties should be read only
    }

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
        Task<SearchResult<TimelineEntity>> SearchTimeline(string query);

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
        Task<SearchResult<UserEntity>> SearchUser(string query);
    }

    public class SearchService : ISearchService
    {
        private readonly DatabaseContext _database;

        public SearchService(DatabaseContext database)
        {
            _database = database;
        }

        public async Task<SearchResult<TimelineEntity>> SearchTimeline(string query)
        {
            if (query is null)
                throw new ArgumentNullException(nameof(query));
            if (query.Length == 0)
                throw new ArgumentException("Query string can't be empty.", nameof(query));

            var nameLikeTimelines = await _database.Timelines.Include(t => t.Owner).Where(t => t.Name == null ? t.Owner.Username.Contains(query) : t.Name.Contains(query)).ToListAsync();
            var titleLikeTimelines = await _database.Timelines.Where(t => t.Title != null && t.Title.Contains(query)).ToListAsync();

            var searchResult = new SearchResult<TimelineEntity>();
            searchResult.Items.AddRange(nameLikeTimelines.Select(t => new SearchResultItem<TimelineEntity>(t, 2)));
            searchResult.Items.AddRange(titleLikeTimelines.Select(t => new SearchResultItem<TimelineEntity>(t, 1)));

            return searchResult;
        }

        public async Task<SearchResult<UserEntity>> SearchUser(string query)
        {
            if (query is null)
                throw new ArgumentNullException(nameof(query));
            if (query.Length == 0)
                throw new ArgumentException("Query string can't be empty.", nameof(query));

            var usernameLikeUsers = await _database.Users.Where(u => u.Username.Contains(query)).ToListAsync();
            var nicknameLikeUsers = await _database.Users.Where(u => u.Nickname != null && u.Nickname.Contains(query)).ToListAsync();

            var searchResult = new SearchResult<UserEntity>();
            searchResult.Items.AddRange(usernameLikeUsers.Select(u => new SearchResultItem<UserEntity>(u, 2)));
            searchResult.Items.AddRange(nicknameLikeUsers.Select(u => new SearchResultItem<UserEntity>(u, 1)));

            return searchResult;

        }
    }
}

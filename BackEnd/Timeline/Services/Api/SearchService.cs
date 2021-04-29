using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using Timeline.Entities;

namespace Timeline.Services.Api
{
    public class SearchService : ISearchService
    {
        private readonly DatabaseContext _database;

        public SearchService(DatabaseContext database)
        {
            _database = database;
        }

        public async Task<SearchResult<TimelineEntity>> SearchTimelineAsync(string query)
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

        public async Task<SearchResult<UserEntity>> SearchUserAsync(string query)
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

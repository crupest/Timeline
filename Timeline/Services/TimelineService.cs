using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Timeline.Entities;
using Timeline.Models;

namespace Timeline.Services
{
    /// <summary>
    /// This define the common interface of both personal timeline
    /// and normal timeline.
    /// </summary>
    /// <remarks>
    /// The "name" parameter in method means name of timeline in
    /// <see cref="ITimelineService"/> while username of the owner
    /// of the personal timeline in <see cref="IPersonalTimelineService"/>.
    /// </remarks>
    public interface IBaseTimelineService
    {
        /// <summary>
        /// Get all the posts in the timeline.
        /// </summary>
        /// <param name="name">Username or the timeline name. See remarks of <see cref="IBaseTimelineService"/>.</param>
        /// <returns>A list of all posts.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="name"/> is null.</exception>
        /// <exception cref="TimelineNameBadFormatException">
        /// Thrown when timeline name is of bad format.
        /// For normal timeline, it means name is an empty string.
        /// For personal timeline, it means the username is of bad format,
        /// the inner exception should be a <see cref="UsernameBadFormatException"/>.
        /// </exception>
        /// <exception cref="TimelineNotExistException">
        /// Thrown when timeline does not exist.
        /// For normal timeline, it means the name does not exist.
        /// For personal timeline, it means the user of that username does not exist
        /// and the inner exception should be a <see cref="UserNotExistException"/>.
        /// </exception>
        Task<List<TimelinePostInfo>> GetPosts(string name);

        /// <summary>
        /// Create a new post in timeline.
        /// </summary>
        /// <param name="name">Username or the timeline name. See remarks of <ssee cref="IBaseTimelineService"/>.</param>
        /// <param name="author">The author's username.</param>
        /// <param name="content">The content.</param>
        /// <param name="time">The time of the post. If null, then use current time.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="name"/> or <paramref name="author"/> or <paramref name="content"/> is null.</exception>
        /// <exception cref="TimelineNameBadFormatException">
        /// Thrown when timeline name is of bad format.
        /// For normal timeline, it means name is an empty string.
        /// For personal timeline, it means the username is of bad format,
        /// the inner exception should be a <see cref="UsernameBadFormatException"/>.
        /// </exception>
        /// <exception cref="TimelineNotExistException">
        /// Thrown when timeline does not exist.
        /// For normal timeline, it means the name does not exist.
        /// For personal timeline, it means the user of that username does not exist
        /// and the inner exception should be a <see cref="UserNotExistException"/>.
        /// </exception>
        /// <exception cref="UsernameBadFormatException">Thrown if <paramref name="author"/> is of bad format.</exception>
        /// <exception cref="UserNotExistException">Thrown if <paramref name="author"/> does not exist.</exception>
        Task<long> CreatePost(string name, string author, string content, DateTime? time);

        /// <summary>
        /// Set the visibility permission of a timeline. 
        /// </summary>
        /// <param name="name">Username or the timeline name. See remarks of <see cref="IBaseTimelineService"/>.</param>
        /// <param name="visibility">The new visibility.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="name"/> is null.</exception>
        /// <exception cref="TimelineNameBadFormatException">
        /// Thrown when timeline name is of bad format.
        /// For normal timeline, it means name is an empty string.
        /// For personal timeline, it means the username is of bad format,
        /// the inner exception should be a <see cref="UsernameBadFormatException"/>.
        /// </exception>
        /// <exception cref="TimelineNotExistException">
        /// Thrown when timeline does not exist.
        /// For normal timeline, it means the name does not exist.
        /// For personal timeline, it means the user of that username does not exist
        /// and the inner exception should be a <see cref="UserNotExistException"/>.
        /// </exception>
        Task SetVisibility(string name, TimelineVisibility visibility);

        /// <summary>
        /// Set the description of a timeline.
        /// </summary>
        /// <param name="name">Username or the timeline name. See remarks of <see cref="IBaseTimelineService"/>.</param>
        /// <param name="description">The new description.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="name"/> or <paramref name="description"/> is null.</exception>
        /// <exception cref="TimelineNameBadFormatException">
        /// Thrown when timeline name is of bad format.
        /// For normal timeline, it means name is an empty string.
        /// For personal timeline, it means the username is of bad format,
        /// the inner exception should be a <see cref="UsernameBadFormatException"/>.
        /// </exception>
        /// <exception cref="TimelineNotExistException">
        /// Thrown when timeline does not exist.
        /// For normal timeline, it means the name does not exist.
        /// For personal timeline, it means the user of that username does not exist
        /// and the inner exception should be a <see cref="UserNotExistException"/>.
        /// </exception>
        Task SetDescription(string name, string description);

        /// <summary>
        /// Remove members to a timeline.
        /// </summary>
        /// <param name="name">Username or the timeline name. See remarks of <see cref="IBaseTimelineService"/>.</param>
        /// <param name="add">A list of usernames of members to add. May be null.</param>
        /// <param name="remove">A list of usernames of members to remove. May be null.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="name"/> is null.</exception>
        /// <exception cref="TimelineNameBadFormatException">
        /// Thrown when timeline name is of bad format.
        /// For normal timeline, it means name is an empty string.
        /// For personal timeline, it means the username is of bad format,
        /// the inner exception should be a <see cref="UsernameBadFormatException"/>.
        /// </exception>
        /// <exception cref="TimelineNotExistException">
        /// Thrown when timeline does not exist.
        /// For normal timeline, it means the name does not exist.
        /// For personal timeline, it means the user of that username does not exist
        /// and the inner exception should be a <see cref="UserNotExistException"/>.
        /// </exception>
        /// <exception cref="TimelineMemberOperationUserException">
        /// Thrown when an exception occurs on the user list.
        /// The inner exception is <see cref="UsernameBadFormatException"/>
        /// when one of the username is invalid.
        /// The inner exception is <see cref="UserNotExistException"/>
        /// when one of the user to add does not exist.
        /// </exception>
        /// <remarks>
        /// Operating on a username that is of bad format always throws.
        /// Add a user that already is a member has no effects.
        /// Remove a user that is not a member also has not effects.
        /// Add a user that does not exist will throw <see cref="TimelineMemberOperationUserException"/>.
        /// But remove one does not throw.
        /// </remarks>
        Task ChangeMember(string name, IList<string>? add, IList<string>? remove);

        /// <summary>
        /// Verify whether a visitor has the permission to read a timeline.
        /// </summary>
        /// <param name="name">Username or the timeline name. See remarks of <see cref="IBaseTimelineService"/>.</param>
        /// <param name="username">The user to check on. Null means visitor without account.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="name"/> is null.</exception>
        /// <exception cref="TimelineNameBadFormatException">
        /// Thrown when timeline name is of bad format.
        /// For normal timeline, it means name is an empty string.
        /// For personal timeline, it means the username is of bad format,
        /// the inner exception should be a <see cref="UsernameBadFormatException"/>.
        /// </exception>
        /// <exception cref="TimelineNotExistException">
        /// Thrown when timeline does not exist.
        /// For normal timeline, it means the name does not exist.
        /// For personal timeline, it means the user of that username does not exist
        /// and the inner exception should be a <see cref="UserNotExistException"/>.
        /// </exception>
        /// <returns>True if can read, false if can't read.</returns>
        Task<bool> HasReadPermission(string name, string? username);

        /// <summary>
        /// Verify whether a user is member of a timeline.
        /// </summary>
        /// <param name="name">Username or the timeline name. See remarks of <see cref="IBaseTimelineService"/>.</param>
        /// <param name="username">The user to check on.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="name"/> or <paramref name="username"/> is null.</exception>
        /// <exception cref="TimelineNameBadFormatException">
        /// Thrown when timeline name is of bad format.
        /// For normal timeline, it means name is an empty string.
        /// For personal timeline, it means the username is of bad format,
        /// the inner exception should be a <see cref="UsernameBadFormatException"/>.
        /// </exception>
        /// <exception cref="TimelineNotExistException">
        /// Thrown when timeline does not exist.
        /// For normal timeline, it means the name does not exist.
        /// For personal timeline, it means the user of that username does not exist
        /// and the inner exception should be a <see cref="UserNotExistException"/>.
        /// </exception>
        /// <exception cref="UsernameBadFormatException">
        /// Thrown when <paramref name="username"/> is not a valid username.
        /// </exception>
        /// <exception cref="UserNotExistException">
        /// Thrown when user <paramref name="username"/> does not exist.</exception>
        /// <returns>True if it is a member, false if not.</returns>
        Task<bool> IsMemberOf(string name, string username);
    }

    /// <summary>
    /// Service for normal timeline.
    /// </summary>
    public interface ITimelineService : IBaseTimelineService
    {
        /// <summary>
        /// Get the timeline info.
        /// </summary>
        /// <param name="name">The name of the timeline.</param>
        /// <returns>The timeline info.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="name"/> is null.</exception>
        /// <exception cref="TimelineNameBadFormatException">
        /// Thrown when timeline name is invalid. Currently it means it is an empty string.
        /// </exception>
        /// <exception cref="TimelineNotExistException">
        /// Thrown when timeline with the name does not exist.
        /// </exception>
        Task<TimelineInfo> GetTimeline(string name);

        /// <summary>
        /// Create a timeline.
        /// </summary>
        /// <param name="name">The name of the timeline.</param>
        /// <param name="owner">The owner of the timeline.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="name"/> or <paramref name="owner"/> is null.</exception>
        /// <exception cref="TimelineNameBadFormatException">
        /// Thrown when timeline name is invalid. Currently it means it is an empty string.
        /// </exception>
        /// <exception cref="TimelineAlreadyExistException">
        /// Thrown when the timeline already exists.
        /// </exception>
        /// <exception cref="UsernameBadFormatException">
        /// Thrown when the username of the owner is not valid.
        /// </exception>
        /// <exception cref="UserNotExistException">
        /// Thrown when the owner user does not exist.</exception>
        Task CreateTimeline(string name, string owner);
    }

    public interface IPersonalTimelineService : IBaseTimelineService
    {
        /// <summary>
        /// Get the timeline info.
        /// </summary>
        /// <param name="username">The username of the owner of the personal timeline.</param>
        /// <returns>The timeline info.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="username"/> is null.
        /// </exception>
        /// <exception cref="TimelineNameBadFormatException">
        /// Thrown when <paramref name="username"/> is of bad format. Inner exception MUST be <see cref="UsernameBadFormatException"/>.
        /// </exception>
        /// <exception cref="TimelineNotExistException">
        /// Thrown when the user does not exist. Inner exception MUST be <see cref="UserNotExistException"/>.
        /// </exception>
        Task<BaseTimelineInfo> GetTimeline(string username);
    }
}

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Timeline.Entities;
using Timeline.Helpers.Cache;
using Timeline.Models;
using Timeline.Services.Imaging;

namespace Timeline.Services.Timeline
{
    public interface ITimelinePostService
    {
        /// <summary>
        /// Get all the posts in the timeline.
        /// </summary>
        /// <param name="timelineId">The id of the timeline.</param>
        /// <param name="modifiedSince">The time that posts have been modified since.</param>
        /// <param name="includeDeleted">Whether include deleted posts.</param>
        /// <returns>A list of all posts.</returns>
        /// <exception cref="EntityNotExistException">Thrown when timeline does not exist.</exception>
        Task<List<TimelinePostEntity>> GetPostsAsync(long timelineId, DateTime? modifiedSince = null, bool includeDeleted = false);

        /// <summary>
        /// Get a post of a timeline.
        /// </summary>
        /// <param name="timelineId">The id of the timeline of the post.</param>
        /// <param name="postId">The id of the post.</param>
        /// <param name="includeDeleted">If true, return the entity even if it is deleted.</param>
        /// <returns>The post.</returns>
        /// <exception cref="EntityNotExistException">Thrown when timeline does not exist.</exception>
        /// <exception cref="EntityNotExistException">Thrown when post of <paramref name="postId"/> does not exist or has been deleted.</exception>
        Task<TimelinePostEntity> GetPostAsync(long timelineId, long postId, bool includeDeleted = false);

        /// <summary>
        /// Get the data digest of a post.
        /// </summary>
        /// <param name="timelineId">The timeline id.</param>
        /// <param name="postId">The post id.</param>
        /// <param name="dataIndex">The index of the data.</param>
        /// <returns>The data digest.</returns>
        /// <exception cref="EntityNotExistException">Thrown when timeline does not exist.</exception>
        /// <exception cref="EntityNotExistException">Thrown when post of <paramref name="postId"/> does not exist or has been deleted.</exception>
        /// <exception cref="EntityNotExistException">Thrown when data of that index does not exist.</exception>
        Task<ICacheableDataDigest> GetPostDataDigestAsync(long timelineId, long postId, long dataIndex);

        /// <summary>
        /// Get the data of a post.
        /// </summary>
        /// <param name="timelineId">The timeline id.</param>
        /// <param name="postId">The post id.</param>
        /// <param name="dataIndex">The index of the data.</param>
        /// <returns>The data.</returns>
        /// <exception cref="EntityNotExistException">Thrown when timeline does not exist.</exception>
        /// <exception cref="EntityNotExistException">Thrown when post of <paramref name="postId"/> does not exist or has been deleted.</exception>
        /// <exception cref="EntityNotExistException">Thrown when data of that index does not exist.</exception>
        Task<ByteData> GetPostDataAsync(long timelineId, long postId, long dataIndex);

        /// <summary>
        /// Create a new post in timeline.
        /// </summary>
        /// <param name="timelineId">The id of the timeline to create post against.</param>
        /// <param name="authorId">The author's user id.</param>
        /// <param name="request">Info about the post.</param>
        /// <returns>The entity of the created post.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="request"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="request"/> is of invalid format.</exception>
        /// <exception cref="EntityNotExistException">Thrown when timeline does not exist.</exception>
        /// <exception cref="EntityNotExistException">Thrown if user of <paramref name="authorId"/> does not exist.</exception>
        /// <exception cref="ImageException">Thrown if data is not a image. Validated by <see cref="ImageService"/>.</exception>
        Task<TimelinePostEntity> CreatePostAsync(long timelineId, long authorId, TimelinePostCreateRequest request);

        /// <summary>
        /// Modify a post. Change its properties or replace its content.
        /// </summary>
        /// <param name="timelineId">The timeline id.</param>
        /// <param name="postId">The post id.</param>
        /// <param name="request">The request.</param>
        /// <returns>The entity of the patched post.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="request"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="request"/> is of invalid format.</exception>
        /// <exception cref="EntityNotExistException">Thrown when timeline does not exist.</exception>
        /// <exception cref="EntityNotExistException">Thrown when post does not exist.</exception>
        Task<TimelinePostEntity> PatchPostAsync(long timelineId, long postId, TimelinePostPatchRequest request);

        /// <summary>
        /// Delete a post.
        /// </summary>
        /// <param name="timelineId">The id of the timeline to delete post against.</param>
        /// <param name="postId">The id of the post to delete.</param>
        /// <exception cref="EntityNotExistException">Thrown when timeline does not exist.</exception>
        /// <exception cref="EntityNotExistException">Thrown when the post with given id does not exist or is deleted already.</exception>
        /// <remarks>
        /// First use <see cref="HasPostModifyPermissionAsync(long, long, long, bool)"/> to check the permission.
        /// </remarks>
        Task DeletePostAsync(long timelineId, long postId);

        /// <summary>
        /// Delete all posts of the given user. Used when delete a user.
        /// </summary>
        /// <param name="userId">The id of the user.</param>
        Task DeleteAllPostsOfUserAsync(long userId);

        /// <summary>
        /// Verify whether a user has the permission to modify a post.
        /// </summary>
        /// <param name="timelineId">The id of the timeline.</param>
        /// <param name="postId">The id of the post.</param>
        /// <param name="modifierId">The id of the user to check on.</param>
        /// <param name="throwOnPostNotExist">True if you want it to throw <see cref="EntityNotExistException"/>. Default false.</param>
        /// <returns>True if can modify, false if can't modify.</returns>
        /// <exception cref="EntityNotExistException">Thrown when timeline does not exist.</exception>
        /// <exception cref="EntityNotExistException">Thrown when the post with given id does not exist or is deleted already and <paramref name="throwOnPostNotExist"/> is true.</exception>
        /// <remarks>
        /// Unless <paramref name="throwOnPostNotExist"/> is true, this method should return true if the post does not exist.
        /// If the post is deleted, its author info still exists, so it is checked as the post is not deleted unless <paramref name="throwOnPostNotExist"/> is true.
        /// This method does not check whether the user is administrator.
        /// It only checks whether he is the author of the post or the owner of the timeline.
        /// Return false when user with modifier id does not exist.
        /// </remarks>
        Task<bool> HasPostModifyPermissionAsync(long timelineId, long postId, long modifierId, bool throwOnPostNotExist = false);
    }
}

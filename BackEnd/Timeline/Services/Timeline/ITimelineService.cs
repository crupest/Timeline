using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Timeline.Entities;
using Timeline.Models;

namespace Timeline.Services.Timeline
{
    /// <summary>
    /// This define the interface of both personal timeline and ordinary timeline.
    /// </summary>
    public interface ITimelineService
    {
        /// <summary>
        /// Check whether a timeline with given id exists without getting full info.
        /// </summary>
        /// <param name="id">The timeline id.</param>
        /// <returns>True if exist. Otherwise false.</returns>
        Task<bool> CheckTimelineExistenceAsync(long id);

        /// <summary>
        /// Get the timeline id by name. Deprecated now because different users can have timeline with the same name now.
        /// </summary>
        /// <param name="timelineName">Timeline name.</param>
        /// <returns>Id of the timeline.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="timelineName"/> is null.</exception>
        /// <exception cref="ArgumentException">Throw when <paramref name="timelineName"/> is of bad format.</exception>
        /// <exception cref="EntityNotExistException">Thrown when timeline with name <paramref name="timelineName"/> does not exist.</exception>
        /// <exception cref="MultipleTimelineException">Thrown when multiple timelines have that name.</exception>
        Task<long> GetTimelineIdByNameAsync(string timelineName);

        /// <summary>
        /// Get timeline id by owner id and timeline name.
        /// </summary>
        /// <param name="ownerId">The timeline owner id.</param>
        /// <param name="timelineName">The timeline name.</param>
        /// <returns>A task contains timeline id.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="timelineName"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="timelineName"/> is not a valid name.</exception>
        Task<long> GetTimelineIdAsync(long ownerId, string timelineName);

        /// <summary>
        /// Get timeline id by owner username and timeline name.
        /// </summary>
        /// <param name="ownerUsername">The timeline owner id.</param>
        /// <param name="timelineName">The timeline name.</param>
        /// <returns>A task contains timeline id.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="ownerUsername"/> is null or <paramref name="timelineName"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="ownerUsername"/> is not a valid username or <paramref name="timelineName"/> is not a valid timeline name.</exception>
        /// <exception cref="EntityNotExistException">Thrown when user with given username does not exist.</exception>
        /// <exception cref="EntityNotExistException">Thrown when timeline with given name does not exist.</exception>
        Task<long> GetTimelineIdAsync(string ownerUsername, string timelineName);

        /// <summary>
        /// Get the timeline info.
        /// </summary>
        /// <param name="id">Id of timeline.</param>
        /// <returns>The timeline info.</returns>
        /// <exception cref="EntityNotExistException">Thrown when timeline does not exist.</exception>
        Task<TimelineEntity> GetTimelineAsync(long id);

        /// <summary>
        /// Set the properties of a timeline. 
        /// </summary>
        /// <param name="id">The id of the timeline.</param>
        /// <param name="newProperties">The new properties. Null member means not to change.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="newProperties"/> is null.</exception>
        /// <exception cref="EntityNotExistException">Thrown when timeline with given id does not exist.</exception>
        /// <exception cref="EntityAlreadyExistException">Thrown when a timeline with new name already exists.</exception>
        Task ChangePropertyAsync(long id, TimelineChangePropertyParams newProperties);

        /// <summary>
        /// Add a member to timeline.
        /// </summary>
        /// <param name="timelineId">Timeline id.</param>
        /// <param name="userId">User id.</param>
        /// <returns>True if the memeber was added. False if it is already a member.</returns>
        /// <exception cref="EntityNotExistException">Thrown when timeline does not exist.</exception>
        /// <exception cref="EntityNotExistException">Thrown when the user does not exist.</exception>
        Task<bool> AddMemberAsync(long timelineId, long userId);

        /// <summary>
        /// Remove a member from timeline.
        /// </summary>
        /// <param name="timelineId">Timeline id.</param>
        /// <param name="userId">User id.</param>
        /// <returns>True if the memeber was removed. False if it was not a member before.</returns>
        /// <exception cref="EntityNotExistException">Thrown when timeline does not exist.</exception>
        /// <exception cref="EntityNotExistException">Thrown when the user does not exist.</exception>
        Task<bool> RemoveMemberAsync(long timelineId, long userId);

        /// <summary>
        /// Check whether a user can manage(change timeline info, member, ...) a timeline.
        /// </summary>
        /// <param name="timelineId">The id of the timeline.</param>
        /// <param name="userId">The id of the user to check on.</param>
        /// <returns>True if the user can manage the timeline, otherwise false.</returns>
        /// <exception cref="EntityNotExistException">Thrown when timeline does not exist.</exception>
        /// <remarks>
        /// This method does not check whether visitor is administrator.
        /// Return false if user with user id does not exist.
        /// </remarks>
        Task<bool> HasManagePermissionAsync(long timelineId, long userId);

        /// <summary>
        /// Verify whether a visitor has the permission to read a timeline.
        /// </summary>
        /// <param name="timelineId">The id of the timeline.</param>
        /// <param name="visitorId">The id of the user to check on. Null means visitor without account.</param>
        /// <returns>True if can read, false if can't read.</returns>
        /// <exception cref="EntityNotExistException">Thrown when timeline does not exist.</exception>
        /// <remarks>
        /// This method does not check whether visitor is administrator.
        /// Return false if user with visitor id does not exist.
        /// </remarks>
        Task<bool> HasReadPermissionAsync(long timelineId, long? visitorId);

        /// <summary>
        /// Verify whether a user is member of a timeline.
        /// </summary>
        /// <param name="timelineId">The id of the timeline.</param>
        /// <param name="userId">The id of user to check on.</param>
        /// <returns>True if it is a member, false if not.</returns>
        /// <exception cref="EntityNotExistException">Thrown when timeline does not exist.</exception>
        /// <remarks>
        /// Timeline owner is also considered as a member.
        /// Return false when user with user id does not exist.
        /// </remarks>
        Task<bool> IsMemberOfAsync(long timelineId, long userId);

        /// <summary>
        /// Get all timelines including personal and ordinary timelines.
        /// </summary>
        /// <param name="relate">Filter timelines related (own or is a member) to specific user.</param>
        /// <param name="visibility">Filter timelines with given visibility. If null or empty, all visibilities are returned. Duplicate value are ignored.</param>
        /// <returns>The list of timelines.</returns>
        /// <remarks>
        /// If user with related user id does not exist, empty list will be returned.
        /// </remarks>
        Task<List<TimelineEntity>> GetTimelinesAsync(TimelineUserRelationship? relate = null, List<TimelineVisibility>? visibility = null);

        /// <summary>
        /// Create a timeline.
        /// </summary>
        /// <param name="ownerId">The id of owner of the timeline.</param>
        /// <param name="timelineName">The name of the timeline.</param>
        /// <returns>The info of the new timeline.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="timelineName"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when timeline name is invalid.</exception>
        /// <exception cref="EntityAlreadyExistException">Thrown when the timeline already exists.</exception>
        /// <exception cref="EntityNotExistException">Thrown when the owner user does not exist.</exception>
        Task<TimelineEntity> CreateTimelineAsync(long ownerId, string timelineName);

        /// <summary>
        /// Delete a timeline.
        /// </summary>
        /// <param name="id">The id of the timeline to delete.</param>
        /// <exception cref="EntityNotExistException">Thrown when the timeline does not exist.</exception>
        Task DeleteTimelineAsync(long id);
    }
}

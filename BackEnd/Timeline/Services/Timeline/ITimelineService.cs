using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Timeline.Entities;
using Timeline.Models;
using Timeline.Services.User;

namespace Timeline.Services.Timeline
{
    /// <summary>
    /// This define the interface of both personal timeline and ordinary timeline.
    /// </summary>
    public interface ITimelineService : IBasicTimelineService
    {
        /// <summary>
        /// Get the timeline info.
        /// </summary>
        /// <param name="id">Id of timeline.</param>
        /// <returns>The timeline info.</returns>
        /// <exception cref="TimelineNotExistException">Thrown when timeline does not exist.</exception>
        Task<TimelineEntity> GetTimelineAsync(long id);

        /// <summary>
        /// Set the properties of a timeline. 
        /// </summary>
        /// <param name="id">The id of the timeline.</param>
        /// <param name="newProperties">The new properties. Null member means not to change.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="newProperties"/> is null.</exception>
        /// <exception cref="TimelineNotExistException">Thrown when timeline with given id does not exist.</exception>
        /// <exception cref="EntityAlreadyExistException">Thrown when a timeline with new name already exists.</exception>
        Task ChangePropertyAsync(long id, TimelineChangePropertyParams newProperties);

        /// <summary>
        /// Add a member to timeline.
        /// </summary>
        /// <param name="timelineId">Timeline id.</param>
        /// <param name="userId">User id.</param>
        /// <returns>True if the memeber was added. False if it is already a member.</returns>
        /// <exception cref="TimelineNotExistException">Thrown when timeline does not exist.</exception>
        /// <exception cref="UserNotExistException">Thrown when the user does not exist.</exception>
        Task<bool> AddMemberAsync(long timelineId, long userId);

        /// <summary>
        /// Remove a member from timeline.
        /// </summary>
        /// <param name="timelineId">Timeline id.</param>
        /// <param name="userId">User id.</param>
        /// <returns>True if the memeber was removed. False if it was not a member before.</returns>
        /// <exception cref="TimelineNotExistException">Thrown when timeline does not exist.</exception>
        /// <exception cref="UserNotExistException">Thrown when the user does not exist.</exception>
        Task<bool> RemoveMemberAsync(long timelineId, long userId);

        /// <summary>
        /// Check whether a user can manage(change timeline info, member, ...) a timeline.
        /// </summary>
        /// <param name="timelineId">The id of the timeline.</param>
        /// <param name="userId">The id of the user to check on.</param>
        /// <returns>True if the user can manage the timeline, otherwise false.</returns>
        /// <exception cref="TimelineNotExistException">Thrown when timeline does not exist.</exception>
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
        /// <exception cref="TimelineNotExistException">Thrown when timeline does not exist.</exception>
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
        /// <exception cref="TimelineNotExistException">Thrown when timeline does not exist.</exception>
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
        /// <param name="timelineName">The name of the timeline.</param>
        /// <param name="ownerId">The id of owner of the timeline.</param>
        /// <returns>The info of the new timeline.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="timelineName"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when timeline name is invalid.</exception>
        /// <exception cref="EntityAlreadyExistException">Thrown when the timeline already exists.</exception>
        /// <exception cref="UserNotExistException">Thrown when the owner user does not exist.</exception>
        Task<TimelineEntity> CreateTimelineAsync(string timelineName, long ownerId);

        /// <summary>
        /// Delete a timeline.
        /// </summary>
        /// <param name="id">The id of the timeline to delete.</param>
        /// <exception cref="TimelineNotExistException">Thrown when the timeline does not exist.</exception>
        Task DeleteTimelineAsync(long id);
    }
}

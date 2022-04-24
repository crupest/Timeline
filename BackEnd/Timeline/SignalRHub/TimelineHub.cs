using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using Timeline.Auth;
using Timeline.Services;
using Timeline.Services.Timeline;
using Timeline.Services.User;

namespace Timeline.SignalRHub
{
    public class TimelineHub : Hub<ITimelineClient>
    {
        private readonly ILogger<TimelineHub> _logger;
        private readonly ITimelineService _timelineService;

        public TimelineHub(ILogger<TimelineHub> logger, ITimelineService timelineService)
        {
            _logger = logger;
            _timelineService = timelineService;
        }

        [Obsolete("Use overload with owner.")]
        public static string GenerateTimelinePostChangeListeningGroupName(string timelineName)
        {
            return $"timeline-post-change-{timelineName}";
        }

        public static string GenerateTimelinePostChangeListeningGroupName(string owner, string timeline)
        {
            return $"v2-timeline-post-change-{owner}/{timeline}";
        }

        [Obsolete("Use v2.")]
        public async Task SubscribeTimelinePostChange(string timelineName)
        {
            try
            {
                var timelineId = await _timelineService.GetTimelineIdByNameAsync(timelineName);
                var user = Context.User;
                if (!user.HasPermission(UserPermission.AllTimelineManagement) && !await _timelineService.HasReadPermissionAsync(timelineId, user.GetOptionalUserId()))
                    throw new HubException(Resource.MessageForbidden);

                var group = GenerateTimelinePostChangeListeningGroupName(timelineName);
                await Groups.AddToGroupAsync(Context.ConnectionId, group);
                _logger.LogInformation(Resource.LogSubscribeTimelinePostChange, Context.ConnectionId, group);
            }
            catch (ArgumentException)
            {
                throw new HubException(Resource.MessageTimelineNameInvalid);
            }
            catch (EntityNotExistException)
            {
                throw new HubException(Resource.MessageTimelineNotExist);
            }
        }

        public async Task SubscribeTimelinePostChangeV2(string owner, string timeline)
        {
            try
            {
                var timelineId = await _timelineService.GetTimelineIdAsync(owner, timeline);
                var user = Context.User;
                if (!user.HasPermission(UserPermission.AllTimelineManagement) && !await _timelineService.HasReadPermissionAsync(timelineId, user.GetOptionalUserId()))
                    throw new HubException(Resource.MessageForbidden);

                var group = GenerateTimelinePostChangeListeningGroupName(owner, timeline);
                await Groups.AddToGroupAsync(Context.ConnectionId, group);
                _logger.LogInformation(Resource.LogSubscribeTimelinePostChange, Context.ConnectionId, group);
            }
            catch (ArgumentException)
            {
                throw new HubException(Resource.MessageTimelineNameInvalid);
            }
            catch (EntityNotExistException)
            {
                throw new HubException(Resource.MessageTimelineNotExist);
            }
        }

        [Obsolete("Use v2.")]
        public async Task UnsubscribeTimelinePostChange(string timelineName)
        {
            var group = GenerateTimelinePostChangeListeningGroupName(timelineName);
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, group);
            _logger.LogInformation(Resource.LogUnsubscribeTimelinePostChange, Context.ConnectionId, group);
        }

        public async Task UnsubscribeTimelinePostChangeV2(string owner, string timeline)
        {
            var group = GenerateTimelinePostChangeListeningGroupName(owner, timeline);
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, group);
            _logger.LogInformation(Resource.LogUnsubscribeTimelinePostChange, Context.ConnectionId, group);
        }
    }
}

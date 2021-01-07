using System.Collections.Generic;

namespace Timeline.Models.Http
{
    /// <summary>
    /// Info of a user.
    /// </summary>
    public class HttpUser
    {
        public HttpUser() { }

        public HttpUser(string uniqueId, string username, string nickname, List<string> permissions, HttpUserLinks links)
        {
            UniqueId = uniqueId;
            Username = username;
            Nickname = nickname;
            Permissions = permissions;
            _links = links;
        }

        /// <summary>
        /// Unique id.
        /// </summary>
        public string UniqueId { get; set; } = default!;
        /// <summary>
        /// Username.
        /// </summary>
        public string Username { get; set; } = default!;
        /// <summary>
        /// Nickname.
        /// </summary>
        public string Nickname { get; set; } = default!;
#pragma warning disable CA2227 // Collection properties should be read only
        /// <summary>
        /// The permissions of the user.
        /// </summary>
        public List<string> Permissions { get; set; } = default!;
#pragma warning restore CA2227 // Collection properties should be read only
#pragma warning disable CA1707 // Identifiers should not contain underscores
        /// <summary>
        /// Related links.
        /// </summary>
        public HttpUserLinks _links { get; set; } = default!;
#pragma warning restore CA1707 // Identifiers should not contain underscores
    }

    /// <summary>
    /// Related links for user.
    /// </summary>
    public class HttpUserLinks
    {
        public HttpUserLinks() { }

        public HttpUserLinks(string self, string avatar, string timeline)
        {
            Self = self;
            Avatar = avatar;
            Timeline = timeline;
        }

        /// <summary>
        /// Self.
        /// </summary>
        public string Self { get; set; } = default!;
        /// <summary>
        /// Avatar url.
        /// </summary>
        public string Avatar { get; set; } = default!;
        /// <summary>
        /// Personal timeline url.
        /// </summary>
        public string Timeline { get; set; } = default!;
    }
}

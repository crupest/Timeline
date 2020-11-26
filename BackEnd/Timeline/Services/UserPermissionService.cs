using Microsoft.EntityFrameworkCore;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Timeline.Entities;
using Timeline.Services.Exceptions;

namespace Timeline.Services
{
    public enum UserPermission
    {
        /// <summary>
        /// This permission allows to manage user (creating, deleting or modifying).
        /// </summary>
        UserManagement,
        /// <summary>
        /// This permission allows to view and modify all timelines.
        /// </summary>
        AllTimelineManagement,
        /// <summary>
        /// This permission allow to add or remove highlight timelines.
        /// </summary>
        HighlightTimelineManagement
    }

    /// <summary>
    /// Represents a user's permissions.
    /// </summary>
    public class UserPermissions : IEnumerable<UserPermission>, IEquatable<UserPermissions>
    {
        public static UserPermissions AllPermissions { get; } = new UserPermissions(Enum.GetValues<UserPermission>());

        /// <summary>
        /// Create an instance containing given permissions.
        /// </summary>
        /// <param name="permissions">Permission list.</param>
        public UserPermissions(params UserPermission[] permissions) : this(permissions as IEnumerable<UserPermission>)
        {

        }

        /// <summary>
        /// Create an instance containing given permissions.
        /// </summary>
        /// <param name="permissions">Permission list.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="permissions"/> is null.</exception>
        public UserPermissions(IEnumerable<UserPermission> permissions)
        {
            if (permissions == null) throw new ArgumentNullException(nameof(permissions));
            _permissions = new SortedSet<UserPermission>(permissions);
        }

        private readonly SortedSet<UserPermission> _permissions = new();

        /// <summary>
        /// Check if a permission is contained in the list.
        /// </summary>
        /// <param name="permission">The permission to check.</param>
        /// <returns>True if contains. Otherwise false.</returns>
        public bool Contains(UserPermission permission)
        {
            return _permissions.Contains(permission);
        }

        /// <summary>
        /// To a serializable string list.
        /// </summary>
        /// <returns>A string list.</returns>
        public List<string> ToStringList()
        {
            return _permissions.Select(p => p.ToString()).ToList();
        }

        /// <summary>
        /// Convert a string list to user permissions.
        /// </summary>
        /// <param name="list">The string list.</param>
        /// <returns>An instance.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="list"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when there is unknown permission name.</exception>
        public static UserPermissions FromStringList(IEnumerable<string> list)
        {
            List<UserPermission> permissions = new();

            foreach (var value in list)
            {
                if (Enum.TryParse<UserPermission>(value, false, out var result))
                {
                    permissions.Add(result);
                }
                else
                {
                    throw new ArgumentException("Unknown permission name.", nameof(list));
                }
            }

            return new UserPermissions(permissions);
        }

        public IEnumerator<UserPermission> GetEnumerator()
        {
            return _permissions.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)_permissions).GetEnumerator();
        }

        public bool Equals(UserPermissions? other)
        {
            if (other == null)
                return false;

            return _permissions.SequenceEqual(other._permissions);
        }

        public override bool Equals(object? obj)
        {
            return Equals(obj as UserPermissions);
        }

        public override int GetHashCode()
        {
            int result = 0;
            foreach (var permission in Enum.GetValues<UserPermission>())
            {
                if (_permissions.Contains(permission))
                {
                    result += 1;
                }
                result <<= 1;
            }
            return result;
        }
    }

    public interface IUserPermissionService
    {
        /// <summary>
        /// Get permissions of a user.
        /// </summary>
        /// <param name="userId">The id of the user.</param>
        /// <param name="checkUserExistence">Whether check the user's existence.</param>
        /// <returns>The permission list.</returns>
        /// <exception cref="UserNotExistException">Thrown when <paramref name="checkUserExistence"/> is true and user does not exist.</exception>
        Task<UserPermissions> GetPermissionsOfUserAsync(long userId, bool checkUserExistence = true);

        /// <summary>
        /// Add a permission to user.
        /// </summary>
        /// <param name="userId">The id of the user.</param>
        /// <param name="permission">The new permission.</param>
        /// <exception cref="UserNotExistException">Thrown when user does not exist.</exception>
        /// <exception cref="InvalidOperationOnRootUserException">Thrown when change root user's permission.</exception>
        Task AddPermissionToUserAsync(long userId, UserPermission permission);

        /// <summary>
        /// Remove a permission from user.
        /// </summary>
        /// <param name="userId">The id of the user.</param>
        /// <param name="permission">The permission.</param>
        /// <param name="checkUserExistence">Whether check the user's existence.</param>
        /// <exception cref="UserNotExistException">Thrown when <paramref name="checkUserExistence"/> is true and user does not exist.</exception>
        /// <exception cref="InvalidOperationOnRootUserException">Thrown when change root user's permission.</exception>
        Task RemovePermissionFromUserAsync(long userId, UserPermission permission, bool checkUserExistence = true);
    }

    public class UserPermissionService : IUserPermissionService
    {
        private readonly DatabaseContext _database;

        public UserPermissionService(DatabaseContext database)
        {
            _database = database;
        }

        private async Task CheckUserExistence(long userId, bool checkUserExistence)
        {
            if (checkUserExistence)
            {
                var existence = await _database.Users.AnyAsync(u => u.Id == userId);
                if (!existence)
                {
                    throw new UserNotExistException(userId);
                }
            }
        }

        public async Task<UserPermissions> GetPermissionsOfUserAsync(long userId, bool checkUserExistence = true)
        {
            if (userId == 1) // The init administrator account.
            {
                return UserPermissions.AllPermissions;
            }

            await CheckUserExistence(userId, checkUserExistence);

            var permissionNameList = await _database.UserPermission.Where(e => e.UserId == userId).Select(e => e.Permission).ToListAsync();

            return UserPermissions.FromStringList(permissionNameList);
        }

        public async Task AddPermissionToUserAsync(long userId, UserPermission permission)
        {
            if (userId == 1)
                throw new InvalidOperationOnRootUserException("Can't change root user's permission.");

            await CheckUserExistence(userId, true);

            var alreadyHas = await _database.UserPermission
                .AnyAsync(e => e.UserId == userId && e.Permission == permission.ToString());

            if (alreadyHas) return;

            _database.UserPermission.Add(new UserPermissionEntity { UserId = userId, Permission = permission.ToString() });

            await _database.SaveChangesAsync();
        }

        public async Task RemovePermissionFromUserAsync(long userId, UserPermission permission, bool checkUserExistence = true)
        {
            if (userId == 1)
                throw new InvalidOperationOnRootUserException("Can't change root user's permission.");

            await CheckUserExistence(userId, checkUserExistence);

            var entity = await _database.UserPermission
                .Where(e => e.UserId == userId && e.Permission == permission.ToString())
                .SingleOrDefaultAsync();

            if (entity == null) return;

            _database.UserPermission.Remove(entity);

            await _database.SaveChangesAsync();
        }
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Timeline.Services.User
{
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
}

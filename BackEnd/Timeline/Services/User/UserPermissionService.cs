using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using Timeline.Entities;

namespace Timeline.Services.User
{
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
                throw new InvalidOperationOnRootUserException(Resource.ExceptionChangeRootUserPermission);

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
                throw new InvalidOperationOnRootUserException(Resource.ExceptionChangeRootUserPermission);

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

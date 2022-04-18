using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Timeline.Entities;

namespace Timeline.Services.User.RegisterCode
{
    public interface IRegisterCodeService
    {
        /// <summary>
        /// Get the owner of a register code or null if the code does not exist or is not enabled.
        /// </summary>
        /// <param name="code">The register code.</param>
        /// <param name="onlyEnabled">If true, only when code is enabled the owner id is returned.</param>
        /// <returns>A task contains the owner of the register code. Null of the code does not exist or is not enabled.</returns>
        Task<long?> GetCodeOwnerAsync(string code, bool onlyEnabled = true);

        /// <summary>
        /// Get the current enabled register code of the user or null if there is none.
        /// </summary>
        /// <param name="userId">The user id.</param>
        /// <returns>A task contains current enabled register code or null if there is none.</returns>
        Task<string?> GetCurrentCodeAsync(long userId);

        /// <summary>
        /// Create a new register code for a user, enable it and disable the previous one if there is a previous one.
        /// </summary>
        /// <param name="userId">The user id.</param>
        /// <returns>A task contains the new register code.</returns>
        Task<string> CreateNewCodeAsync(long userId);

        /// <summary>
        /// Record a register info for a user.
        /// </summary>
        /// <param name="userId">The newly-registered user.</param>
        /// <param name="registerCode">The register code.</param>
        /// <param name="registerTime">The register time.</param>
        /// <returns>The created register info.</returns>
        Task<UserRegisterInfo> CreateRegisterInfoAsync(long userId, string registerCode, DateTime registerTime);

        /// <summary>
        /// Get register info of a user if there is one.
        /// </summary>
        /// <param name="userId">The user id.</param>
        /// <returns>The user register info if there is one. Or null if there is not.</returns>
        Task<UserRegisterInfo?> GetUserRegisterInfoAsync(long userId);

        /// <summary>
        /// Create a user with register code.
        /// </summary>
        /// <param name="userParams">The params to create user with.</param>
        /// <param name="registerCode">The user code.</param>
        /// <returns>The created user.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="userParams"/> or <paramref name="registerCode"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="userParams"/> is invalid.</exception>
        /// <exception cref="EntityAlreadyExistException">Thrown when username already exist.</exception>
        /// <exception cref="InvalidRegisterCodeException">Thrown when register code is invalid.</exception>
        Task<UserEntity> RegisterUserWithCode(CreateUserParams userParams, string registerCode);

        /// <summary>
        /// Get the list of user register info of the specified introducer.
        /// </summary>
        /// <param name="introducerId"></param>
        /// <returns>The list of user register info.</returns>
        Task<List<UserRegisterInfo>> GetUserRegisterInfoOfIntroducerAsync(long introducerId);
    }
}

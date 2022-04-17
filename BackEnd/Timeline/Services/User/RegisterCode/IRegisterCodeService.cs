﻿using System;
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
        Task<long?> GetCodeOwner(string code, bool onlyEnabled = true);

        /// <summary>
        /// Get the current enabled register code of the user or null if there is none.
        /// </summary>
        /// <param name="userId">The user id.</param>
        /// <returns>A task contains current enabled register code or null if there is none.</returns>
        Task<string?> GetCurrentCode(long userId);

        /// <summary>
        /// Create a new register code for a user, enable it and disable the previous one if there is a previous one.
        /// </summary>
        /// <param name="userId">The user id.</param>
        /// <returns>A task contains the new register code.</returns>
        Task<string> CreateNewCode(long userId);

        /// <summary>
        /// Record a register info for a user.
        /// </summary>
        /// <param name="userId">The newly-registered user.</param>
        /// <param name="introducerId">The introducer user id.</param>
        /// <param name="registerCode">The register code.</param>
        /// <param name="registerTime">The register time.</param>
        /// <returns>The created register info.</returns>
        Task<UserRegisterInfo> CreateRegisterInfo(long userId, long introducerId, string registerCode, DateTime registerTime);

        /// <summary>
        /// Get register info of a user if there is one.
        /// </summary>
        /// <param name="userId">The user id.</param>
        /// <returns>The user register info if there is one. Or null if there is not.</returns>
        Task<UserRegisterInfo?> GetUserRegisterInfo(long userId);

        /// <summary>
        /// Get the list of user register info of the specified introducer.
        /// </summary>
        /// <param name="introducerId"></param>
        /// <returns>The list of user register info.</returns>
        Task<List<UserRegisterInfo>> GetUserRegisterInfoOfIntroducer(long introducerId);
    }
}
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using Timeline.Auth;
using Timeline.Models.Http;
using Timeline.Services.User;

namespace Timeline.Controllers
{
    public class MyControllerBase : ControllerBase
    {
        #region auth
        protected bool UserHasPermission(UserPermission permission)
        {
            return User.HasPermission(permission);
        }

        protected string? GetOptionalUsername()
        {
            return User.GetOptionalName();
        }

        protected string GetUsername()
        {
            return GetOptionalUsername() ?? throw new InvalidOperationException(Resource.ExceptionNoUsername);
        }

        protected long? GetOptionalUserId()
        {
            return User.GetOptionalUserId();
        }

        protected long GetUserId()
        {
            return GetOptionalUserId() ?? throw new InvalidOperationException(Resource.ExceptionNoUserId);
        }
        #endregion auth

        #region action result
        protected ObjectResult StatusCodeWithCommonResponse(int statusCode, int code, string message)
        {
            return StatusCode(statusCode, new CommonResponse(code, message));
        }

        protected ObjectResult OkWithCommonResponse(int statusCode = 0, string? message = null)
        {
            return Ok(new CommonResponse(statusCode, message ?? Resource.MessageOperationSucceeded));
        }

        protected ObjectResult OkWithCommonResponse(string? message)
        {
            return OkWithCommonResponse(message: message);
        }

        protected ObjectResult ForbidWithCommonResponse(string? message = null)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new CommonResponse(ErrorCodes.Common.Forbid, message ?? Resource.MessageForbid));
        }

        protected ObjectResult ForbidWithCommonResponse(int code, string? message = null)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new CommonResponse(code, message ?? Resource.MessageForbid));
        }

        protected ObjectResult DeleteWithCommonDeleteResponse(bool delete = true)
        {
            return StatusCode(StatusCodes.Status200OK, CommonDeleteResponse.Create(delete));
        }

        protected BadRequestObjectResult BadRequestWithCommonResponse(int code, string message)
        {
            return BadRequest(new CommonResponse(code, message));
        }
        #endregion action result
    }
}

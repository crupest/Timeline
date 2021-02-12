using static Timeline.Resources.Messages;

namespace Timeline.Models.Http
{
    public static class ErrorResponse
    {
        public static class Common
        {
            public static CommonResponse InvalidModel(params object?[] formatArgs)
            {
                return new CommonResponse(ErrorCodes.Common.InvalidModel, string.Format(Common_InvalidModel, formatArgs));
            }

            public static CommonResponse CustomMessage_InvalidModel(string message, params object?[] formatArgs)
            {
                return new CommonResponse(ErrorCodes.Common.InvalidModel, string.Format(message, formatArgs));
            }

            public static CommonResponse Forbid(params object?[] formatArgs)
            {
                return new CommonResponse(ErrorCodes.Common.Forbid, string.Format(Common_Forbid, formatArgs));
            }

            public static CommonResponse CustomMessage_Forbid(string message, params object?[] formatArgs)
            {
                return new CommonResponse(ErrorCodes.Common.Forbid, string.Format(message, formatArgs));
            }

            public static CommonResponse UnknownEndpoint(params object?[] formatArgs)
            {
                return new CommonResponse(ErrorCodes.Common.UnknownEndpoint, string.Format(Common_UnknownEndpoint, formatArgs));
            }

            public static CommonResponse CustomMessage_UnknownEndpoint(string message, params object?[] formatArgs)
            {
                return new CommonResponse(ErrorCodes.Common.UnknownEndpoint, string.Format(message, formatArgs));
            }

            public static class Header
            {
                public static CommonResponse IfNonMatch_BadFormat(params object?[] formatArgs)
                {
                    return new CommonResponse(ErrorCodes.Common.Header.IfNonMatch_BadFormat, string.Format(Common_Header_IfNonMatch_BadFormat, formatArgs));
                }

                public static CommonResponse CustomMessage_IfNonMatch_BadFormat(string message, params object?[] formatArgs)
                {
                    return new CommonResponse(ErrorCodes.Common.Header.IfNonMatch_BadFormat, string.Format(message, formatArgs));
                }

            }

            public static class Content
            {
                public static CommonResponse TooBig(params object?[] formatArgs)
                {
                    return new CommonResponse(ErrorCodes.Common.Content.TooBig, string.Format(Common_Content_TooBig, formatArgs));
                }

                public static CommonResponse CustomMessage_TooBig(string message, params object?[] formatArgs)
                {
                    return new CommonResponse(ErrorCodes.Common.Content.TooBig, string.Format(message, formatArgs));
                }

            }

        }

        public static class UserCommon
        {
            public static CommonResponse NotExist(params object?[] formatArgs)
            {
                return new CommonResponse(ErrorCodes.UserCommon.NotExist, string.Format(UserCommon_NotExist, formatArgs));
            }

            public static CommonResponse CustomMessage_NotExist(string message, params object?[] formatArgs)
            {
                return new CommonResponse(ErrorCodes.UserCommon.NotExist, string.Format(message, formatArgs));
            }

        }

        public static class TokenController
        {
            public static CommonResponse Create_BadCredential(params object?[] formatArgs)
            {
                return new CommonResponse(ErrorCodes.TokenController.Create_BadCredential, string.Format(TokenController_Create_BadCredential, formatArgs));
            }

            public static CommonResponse CustomMessage_Create_BadCredential(string message, params object?[] formatArgs)
            {
                return new CommonResponse(ErrorCodes.TokenController.Create_BadCredential, string.Format(message, formatArgs));
            }

            public static CommonResponse Verify_BadFormat(params object?[] formatArgs)
            {
                return new CommonResponse(ErrorCodes.TokenController.Verify_BadFormat, string.Format(TokenController_Verify_BadFormat, formatArgs));
            }

            public static CommonResponse CustomMessage_Verify_BadFormat(string message, params object?[] formatArgs)
            {
                return new CommonResponse(ErrorCodes.TokenController.Verify_BadFormat, string.Format(message, formatArgs));
            }

            public static CommonResponse Verify_UserNotExist(params object?[] formatArgs)
            {
                return new CommonResponse(ErrorCodes.TokenController.Verify_UserNotExist, string.Format(TokenController_Verify_UserNotExist, formatArgs));
            }

            public static CommonResponse CustomMessage_Verify_UserNotExist(string message, params object?[] formatArgs)
            {
                return new CommonResponse(ErrorCodes.TokenController.Verify_UserNotExist, string.Format(message, formatArgs));
            }

            public static CommonResponse Verify_OldVersion(params object?[] formatArgs)
            {
                return new CommonResponse(ErrorCodes.TokenController.Verify_OldVersion, string.Format(TokenController_Verify_OldVersion, formatArgs));
            }

            public static CommonResponse CustomMessage_Verify_OldVersion(string message, params object?[] formatArgs)
            {
                return new CommonResponse(ErrorCodes.TokenController.Verify_OldVersion, string.Format(message, formatArgs));
            }

            public static CommonResponse Verify_TimeExpired(params object?[] formatArgs)
            {
                return new CommonResponse(ErrorCodes.TokenController.Verify_TimeExpired, string.Format(TokenController_Verify_TimeExpired, formatArgs));
            }

            public static CommonResponse CustomMessage_Verify_TimeExpired(string message, params object?[] formatArgs)
            {
                return new CommonResponse(ErrorCodes.TokenController.Verify_TimeExpired, string.Format(message, formatArgs));
            }

        }

        public static class UserController
        {
            public static CommonResponse UsernameConflict(params object?[] formatArgs)
            {
                return new CommonResponse(ErrorCodes.UserController.UsernameConflict, string.Format(UserController_UsernameConflict, formatArgs));
            }

            public static CommonResponse CustomMessage_UsernameConflict(string message, params object?[] formatArgs)
            {
                return new CommonResponse(ErrorCodes.UserController.UsernameConflict, string.Format(message, formatArgs));
            }

            public static CommonResponse ChangePassword_BadOldPassword(params object?[] formatArgs)
            {
                return new CommonResponse(ErrorCodes.UserController.ChangePassword_BadOldPassword, string.Format(UserController_ChangePassword_BadOldPassword, formatArgs));
            }

            public static CommonResponse CustomMessage_ChangePassword_BadOldPassword(string message, params object?[] formatArgs)
            {
                return new CommonResponse(ErrorCodes.UserController.ChangePassword_BadOldPassword, string.Format(message, formatArgs));
            }

            public static CommonResponse ChangePermission_RootUser(params object?[] formatArgs)
            {
                return new CommonResponse(ErrorCodes.UserController.ChangePermission_RootUser, string.Format(UserController_ChangePermission_RootUser, formatArgs));
            }

            public static CommonResponse CustomMessage_ChangePermission_RootUser(string message, params object?[] formatArgs)
            {
                return new CommonResponse(ErrorCodes.UserController.ChangePermission_RootUser, string.Format(message, formatArgs));
            }

            public static CommonResponse Delete_RootUser(params object?[] formatArgs)
            {
                return new CommonResponse(ErrorCodes.UserController.Delete_RootUser, string.Format(UserController_Delete_RootUser, formatArgs));
            }

            public static CommonResponse CustomMessage_Delete_RootUser(string message, params object?[] formatArgs)
            {
                return new CommonResponse(ErrorCodes.UserController.Delete_RootUser, string.Format(message, formatArgs));
            }

        }

        public static class UserAvatar
        {
            public static CommonResponse BadFormat_CantDecode(params object?[] formatArgs)
            {
                return new CommonResponse(ErrorCodes.UserAvatar.BadFormat_CantDecode, string.Format(UserAvatar_BadFormat_CantDecode, formatArgs));
            }

            public static CommonResponse CustomMessage_BadFormat_CantDecode(string message, params object?[] formatArgs)
            {
                return new CommonResponse(ErrorCodes.UserAvatar.BadFormat_CantDecode, string.Format(message, formatArgs));
            }

            public static CommonResponse BadFormat_UnmatchedFormat(params object?[] formatArgs)
            {
                return new CommonResponse(ErrorCodes.UserAvatar.BadFormat_UnmatchedFormat, string.Format(UserAvatar_BadFormat_UnmatchedFormat, formatArgs));
            }

            public static CommonResponse CustomMessage_BadFormat_UnmatchedFormat(string message, params object?[] formatArgs)
            {
                return new CommonResponse(ErrorCodes.UserAvatar.BadFormat_UnmatchedFormat, string.Format(message, formatArgs));
            }

            public static CommonResponse BadFormat_BadSize(params object?[] formatArgs)
            {
                return new CommonResponse(ErrorCodes.UserAvatar.BadFormat_BadSize, string.Format(UserAvatar_BadFormat_BadSize, formatArgs));
            }

            public static CommonResponse CustomMessage_BadFormat_BadSize(string message, params object?[] formatArgs)
            {
                return new CommonResponse(ErrorCodes.UserAvatar.BadFormat_BadSize, string.Format(message, formatArgs));
            }

        }

        public static class TimelineController
        {
            public static CommonResponse NameConflict(params object?[] formatArgs)
            {
                return new CommonResponse(ErrorCodes.TimelineController.NameConflict, string.Format(TimelineController_NameConflict, formatArgs));
            }

            public static CommonResponse CustomMessage_NameConflict(string message, params object?[] formatArgs)
            {
                return new CommonResponse(ErrorCodes.TimelineController.NameConflict, string.Format(message, formatArgs));
            }

            public static CommonResponse NotExist(params object?[] formatArgs)
            {
                return new CommonResponse(ErrorCodes.TimelineController.NotExist, string.Format(TimelineController_NotExist, formatArgs));
            }

            public static CommonResponse CustomMessage_NotExist(string message, params object?[] formatArgs)
            {
                return new CommonResponse(ErrorCodes.TimelineController.NotExist, string.Format(message, formatArgs));
            }

            public static CommonResponse QueryRelateNotExist(params object?[] formatArgs)
            {
                return new CommonResponse(ErrorCodes.TimelineController.QueryRelateNotExist, string.Format(TimelineController_QueryRelateNotExist, formatArgs));
            }

            public static CommonResponse CustomMessage_QueryRelateNotExist(string message, params object?[] formatArgs)
            {
                return new CommonResponse(ErrorCodes.TimelineController.QueryRelateNotExist, string.Format(message, formatArgs));
            }

            public static CommonResponse PostNotExist(params object?[] formatArgs)
            {
                return new CommonResponse(ErrorCodes.TimelineController.PostNotExist, string.Format(TimelineController_PostNotExist, formatArgs));
            }

            public static CommonResponse CustomMessage_PostNotExist(string message, params object?[] formatArgs)
            {
                return new CommonResponse(ErrorCodes.TimelineController.PostNotExist, string.Format(message, formatArgs));
            }
        }

    }

}


using static TimelineApp.Resources.Messages;

namespace TimelineApp.Models.Http
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

                public static CommonResponse ContentType_Missing(params object?[] formatArgs)
                {
                    return new CommonResponse(ErrorCodes.Common.Header.ContentType_Missing, string.Format(Common_Header_ContentType_Missing, formatArgs));
                }

                public static CommonResponse CustomMessage_ContentType_Missing(string message, params object?[] formatArgs)
                {
                    return new CommonResponse(ErrorCodes.Common.Header.ContentType_Missing, string.Format(message, formatArgs));
                }

                public static CommonResponse ContentLength_Missing(params object?[] formatArgs)
                {
                    return new CommonResponse(ErrorCodes.Common.Header.ContentLength_Missing, string.Format(Common_Header_ContentLength_Missing, formatArgs));
                }

                public static CommonResponse CustomMessage_ContentLength_Missing(string message, params object?[] formatArgs)
                {
                    return new CommonResponse(ErrorCodes.Common.Header.ContentLength_Missing, string.Format(message, formatArgs));
                }

                public static CommonResponse ContentLength_Zero(params object?[] formatArgs)
                {
                    return new CommonResponse(ErrorCodes.Common.Header.ContentLength_Zero, string.Format(Common_Header_ContentLength_Zero, formatArgs));
                }

                public static CommonResponse CustomMessage_ContentLength_Zero(string message, params object?[] formatArgs)
                {
                    return new CommonResponse(ErrorCodes.Common.Header.ContentLength_Zero, string.Format(message, formatArgs));
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

                public static CommonResponse UnmatchedLength_Smaller(params object?[] formatArgs)
                {
                    return new CommonResponse(ErrorCodes.Common.Content.UnmatchedLength_Smaller, string.Format(Common_Content_UnmatchedLength_Smaller, formatArgs));
                }

                public static CommonResponse CustomMessage_UnmatchedLength_Smaller(string message, params object?[] formatArgs)
                {
                    return new CommonResponse(ErrorCodes.Common.Content.UnmatchedLength_Smaller, string.Format(message, formatArgs));
                }

                public static CommonResponse UnmatchedLength_Bigger(params object?[] formatArgs)
                {
                    return new CommonResponse(ErrorCodes.Common.Content.UnmatchedLength_Bigger, string.Format(Common_Content_UnmatchedLength_Bigger, formatArgs));
                }

                public static CommonResponse CustomMessage_UnmatchedLength_Bigger(string message, params object?[] formatArgs)
                {
                    return new CommonResponse(ErrorCodes.Common.Content.UnmatchedLength_Bigger, string.Format(message, formatArgs));
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

        public static class TimelineCommon
        {

            public static CommonResponse NameConflict(params object?[] formatArgs)
            {
                return new CommonResponse(ErrorCodes.TimelineCommon.NameConflict, string.Format(TimelineCommon_NameConflict, formatArgs));
            }

            public static CommonResponse CustomMessage_NameConflict(string message, params object?[] formatArgs)
            {
                return new CommonResponse(ErrorCodes.TimelineCommon.NameConflict, string.Format(message, formatArgs));
            }

            public static CommonResponse NotExist(params object?[] formatArgs)
            {
                return new CommonResponse(ErrorCodes.TimelineCommon.NotExist, string.Format(TimelineCommon_NotExist, formatArgs));
            }

            public static CommonResponse CustomMessage_NotExist(string message, params object?[] formatArgs)
            {
                return new CommonResponse(ErrorCodes.TimelineCommon.NotExist, string.Format(message, formatArgs));
            }

            public static CommonResponse MemberPut_NotExist(params object?[] formatArgs)
            {
                return new CommonResponse(ErrorCodes.TimelineCommon.MemberPut_NotExist, string.Format(TimelineCommon_MemberPut_NotExist, formatArgs));
            }

            public static CommonResponse CustomMessage_MemberPut_NotExist(string message, params object?[] formatArgs)
            {
                return new CommonResponse(ErrorCodes.TimelineCommon.MemberPut_NotExist, string.Format(message, formatArgs));
            }

        }

        public static class TimelineController
        {

            public static CommonResponse QueryRelateNotExist(params object?[] formatArgs)
            {
                return new CommonResponse(ErrorCodes.TimelineController.QueryRelateNotExist, string.Format(TimelineController_QueryRelateNotExist, formatArgs));
            }

            public static CommonResponse CustomMessage_QueryRelateNotExist(string message, params object?[] formatArgs)
            {
                return new CommonResponse(ErrorCodes.TimelineController.QueryRelateNotExist, string.Format(message, formatArgs));
            }

        }

    }

}

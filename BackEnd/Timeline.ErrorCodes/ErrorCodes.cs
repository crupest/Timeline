namespace Timeline.Models.Http
{
    /// <summary>
    /// All error code constants.
    /// </summary>
    /// <remarks>
    /// Format: 1bbbccdd
    /// </remarks>
    public static class ErrorCodes
    {
        public static class Common
        {
            public const int InvalidModel = 1_000_0001;
            public const int Forbid = 1_000_0002;
            public const int UnknownEndpoint = 1_000_0003;

            public static class Header
            {
                public const int IfNonMatch_BadFormat = 1_000_01_01;
                public const int IfModifiedSince_BadFormat = 1_000_01_02;
            }

            public static class Content
            {
                public const int TooBig = 1_000_11_01;
            }
        }

        public static class UserCommon
        {
            public const int NotExist = 1_001_0001;
        }

        public static class TokenController
        {
            public const int Create_BadCredential = 1_101_01_01;
            public const int Verify_BadFormat = 1_101_02_01;
            public const int Verify_UserNotExist = 1_101_02_02;
            public const int Verify_OldVersion = 1_101_02_03;
            public const int Verify_TimeExpired = 1_101_02_04;
        }

        public static class UserController
        {
            public const int UsernameConflict = 1_102_01_01;
            public const int ChangePassword_BadOldPassword = 1_102_02_01;
            public const int ChangePermission_RootUser = 1_102_03_01;
            public const int Delete_RootUser = 1_102_04_01;
        }

        public static class UserAvatar
        {
            public const int BadFormat_CantDecode = 1_103_00_01;
            public const int BadFormat_UnmatchedFormat = 1_103_00_02;
            public const int BadFormat_BadSize = 1_103_00_03;
        }

        public static class TimelineController
        {
            public const int NameConflict = 1_104_01_01;
            public const int NotExist = 1_104_02_01;
            public const int QueryRelateNotExist = 1_104_04_01;
            public const int PostNotExist = 1_104_05_01;
            public const int PostDataNotExist = 1_104_05_02;
        }

        public static class HighlightTimelineController
        {
            public const int NonHighlight = 1_105_01_01;
        }

        public static class BookmarkTimelineController
        {
            public const int NonBookmark = 1_106_01_01;
        }
    }
}


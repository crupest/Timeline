namespace Timeline
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
            public const int Unauthorized = 1_000_0004;

            public static class Header
            {
                public const int IfNonMatch_BadFormat = 1_000_01_01;
                public const int IfModifiedSince_BadFormat = 1_000_01_02;
            }

            public static class Content
            {
                public const int TooBig = 1_000_11_01;
            }

            public static class Token
            {
                public const int TimeExpired = 1_000_21_01;
                public const int VersionExpired = 1_000_21_02;
                public const int BadFormat = 1_000_21_03;
                public const int UserNotExist = 1_000_21_04;
                public const int Unknown = 1_000_21_05;
            }
        }

        public static class NotExist
        {
            public const int Default = 2_001_00_00;
            public const int User = 2_001_00_01;
            public const int Timeline = 2_001_00_02;
            public const int TimelinePost = 2_001_00_03;
            public const int TimelinePostData = 2_001_00_04;
        }

        public static class Conflict
        {
            public const int Default = 2_002_00_00;
            public const int User = 2_002_00_01;
            public const int Timeline = 2_002_00_02;
            public const int TimelinePost = 2_002_00_03;
            public const int TimelinePostData = 2_002_00_04;
        }

        public static class TokenController
        {
            public const int CreateBadCredential = 1_101_01_01;
            public const int VerifyBadFormat = 1_101_02_01;
            public const int VerifyUserNotExist = 1_101_02_02;
            public const int VerifyOldVersion = 1_101_02_03;
            public const int VerifyTimeExpired = 1_101_02_04;
        }

        public static class UserController
        {
            public const int ChangePasswordBadOldPassword = 1_102_01_01;
            public const int InvalidOperationOnRootUser = 1_102_02_01;
        }

        public static class Image
        {
            public const int CantDecode = 1_103_00_01;
            public const int UnmatchedFormat = 1_103_00_02;
            public const int BadSize = 1_103_00_03;
            public const int Unknown = 1_103_00_04;
        }

        public static class TimelineController
        {
            public const int QueryRelateNotExist = 1_104_04_01;
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


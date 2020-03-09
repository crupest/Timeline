namespace TimelineApp.Models.Http
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

            public static class Header
            {
                public const int IfNonMatch_BadFormat = 1_000_01_01;
                public const int ContentType_Missing = 1_000_02_01;
                public const int ContentLength_Missing = 1_000_03_01;
                public const int ContentLength_Zero = 1_000_03_02;
            }

            public static class Content
            {
                public const int TooBig = 1_000_11_01;
                public const int UnmatchedLength_Smaller = 1_000_11_02;
                public const int UnmatchedLength_Bigger = 1_000_11_03;
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
        }

        public static class UserAvatar
        {
            public const int BadFormat_CantDecode = 1_103_00_01;
            public const int BadFormat_UnmatchedFormat = 1_103_00_02;
            public const int BadFormat_BadSize = 1_103_00_03;
        }

        public static class TimelineCommon
        {
            public const int NameConflict = 1_104_01_01;
            public const int NotExist = 1_104_02_01;
            public const int MemberPut_NotExist = 1_104_03_01;
        }

        public static class TimelineController
        {
            public const int QueryRelateNotExist = 1_105_01_01;
        }
    }
}


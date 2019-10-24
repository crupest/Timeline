namespace Timeline
{
    /// <summary>
    /// All error code constants.
    /// </summary>
    /// <remarks>
    /// Scheme:
    /// abbbccdd
    /// </remarks>
    public static partial class ErrorCodes
    {
        public static partial class Http // a = 1 
        {
            public static class Common // bbb = 000
            {
                public const int InvalidModel = 10000000;

                public static class Header // cc = 01
                {
                    public const int Missing_ContentType = 10000101; // dd = 01
                    public const int Missing_ContentLength = 10000102; // dd = 02
                    public const int Zero_ContentLength = 10000103; // dd = 03
                    public const int BadFormat_IfNonMatch = 10000104; // dd = 04
                }

                public static class Content // cc = 02
                {
                    public const int TooBig = 1000201;
                    public const int UnmatchedLength_Smaller = 10030202;
                    public const int UnmatchedLength_Bigger = 10030203;
                }
            }
        }

    }
}

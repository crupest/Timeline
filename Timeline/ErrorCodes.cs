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
                    public const int Missing_ContentType = 10010101; // dd = 01
                    public const int Missing_ContentLength = 10010102; // dd = 02
                    public const int Zero_ContentLength = 10010103; // dd = 03
                    public const int BadFormat_IfNonMatch = 10010104; // dd = 04
                }
            }
        }

    }
}

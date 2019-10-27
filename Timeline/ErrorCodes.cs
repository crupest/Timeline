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

                public static class Header // cc = 0x
                {
                    public static class IfNonMatch // cc = 01
                    {
                        public const int BadFormat = 10000101;
                    }
                }

                public static class Content // cc = 11
                {
                    public const int TooBig = 10001101;
                    public const int UnmatchedLength_Smaller = 10001102;
                    public const int UnmatchedLength_Bigger = 10001103;
                }
            }
        }
    }
}

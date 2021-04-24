namespace Timeline.Configs
{
    public class TokenOptions
    {
        /// <summary>
        /// Set the default value of expire offset of jwt token.
        /// Unit is second. Default is 3600 * 24 seconds, aka 1 day.
        /// </summary>
        public long DefaultExpireSeconds { get; set; } = 3600 * 24;
    }
}

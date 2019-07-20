namespace Timeline.Configs
{
    public class JwtConfig
    {
        public string Issuer { get; set; }
        public string Audience { get; set; }
        public string SigningKey { get; set; }

        /// <summary>
        /// Set the default value of expire offset of jwt token.
        /// Unit is second. Default is 3600 seconds, aka 1 hour.
        /// </summary>
        public long DefaultExpireOffset { get; set; } = 3600;
    }
}

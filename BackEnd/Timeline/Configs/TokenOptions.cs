namespace Timeline.Configs
{
    public class TokenOptions
    {
        /// <summary>
        /// The length of the generated secure random token counted in byte.
        /// Note the byte will be converted to hex form when used.
        /// Default is 32 byte long.
        /// </summary>
        public long? TokenLength { get; set; }
    }
}

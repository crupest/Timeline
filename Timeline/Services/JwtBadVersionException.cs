using System;
using Timeline.Helpers;

namespace Timeline.Services
{
    [Serializable]
    public class JwtBadVersionException : Exception
    {
        public JwtBadVersionException() : base(Resources.Services.Exception.JwtBadVersionException) { }
        public JwtBadVersionException(string message) : base(message) { }
        public JwtBadVersionException(string message, Exception inner) : base(message, inner) { }

        public JwtBadVersionException(long tokenVersion, long requiredVersion)
            : base(Log.Format(Resources.Services.Exception.JwtBadVersionException,
                ("Token Version", tokenVersion),
                ("Required Version", requiredVersion)))
        {
            TokenVersion = tokenVersion;
            RequiredVersion = requiredVersion;
        }

        protected JwtBadVersionException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }

        /// <summary>
        /// The version in the token.
        /// </summary>
        public long? TokenVersion { get; set; }

        /// <summary>
        /// The version required.
        /// </summary>
        public long? RequiredVersion { get; set; }
    }
}

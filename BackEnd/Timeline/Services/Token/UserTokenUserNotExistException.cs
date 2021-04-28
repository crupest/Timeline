using System;

namespace Timeline.Services.Token
{
    [Serializable]
    public class UserTokenUserNotExistException : UserTokenException
    {
        public UserTokenUserNotExistException() : base(Resource.ExceptionUserTokenUserNotExist) { }
        public UserTokenUserNotExistException(string token) : base(token, Resource.ExceptionUserTokenUserNotExist) { }
        public UserTokenUserNotExistException(string token, Exception inner) : base(token, Resource.ExceptionUserTokenUserNotExist, inner) { }

        protected UserTokenUserNotExistException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}

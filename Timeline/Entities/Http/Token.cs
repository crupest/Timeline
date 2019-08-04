namespace Timeline.Entities.Http
{
    public class CreateTokenRequest
    {
        public string Username { get; set; }
        public string Password { get; set; }
        // in day
        public double? ExpireOffset { get; set; }
    }

    public class CreateTokenResponse
    {
        public string Token { get; set; }
        public UserInfo User { get; set; }
    }

    public class VerifyTokenRequest
    {
        public string Token { get; set; }
    }

    public class VerifyTokenResponse
    {
        public UserInfo User { get; set; }
    }
}

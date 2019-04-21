namespace Timeline.Entities
{
    public class CreateTokenRequest
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }

    public class CreateTokenResponse
    {
        public bool Success { get; set; }
        public string Token { get; set; }
        public UserInfo UserInfo { get; set; }
    }

    public class VerifyTokenRequest
    {
        public string Token { get; set; }
    }

    public class VerifyTokenResponse
    {
        public bool IsValid { get; set; }
        public UserInfo UserInfo { get; set; }
    }
}

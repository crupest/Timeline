namespace Timeline.Entities.Http
{
    public class UserPutRequest
    {
        public string Password { get; set; }
        public bool IsAdmin { get; set; }
    }

    public class UserPatchRequest
    {
        public string Password { get; set; }
        public bool? IsAdmin { get; set; }
    }

    public class ChangePasswordRequest
    {
        public string OldPassword { get; set; }
        public string NewPassword { get; set; }
    }
}

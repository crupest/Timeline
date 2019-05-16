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

    public static class UserPutResponse
    {
        public const int CreatedCode = 0;
        public const int ModifiedCode = 1;

        public static ReturnCodeMessageResponse Created { get; } = new ReturnCodeMessageResponse(CreatedCode, "A new user is created.");
        public static ReturnCodeMessageResponse Modified { get; } = new ReturnCodeMessageResponse(ModifiedCode, "A existing user is modified.");
    }

    public static class UserDeleteResponse
    {
        public const int DeletedCode = 0;
        public const int NotExistsCode = 1;

        public static ReturnCodeMessageResponse Deleted { get; } = new ReturnCodeMessageResponse(DeletedCode, "A existing user is deleted.");
        public static ReturnCodeMessageResponse NotExists { get; } = new ReturnCodeMessageResponse(NotExistsCode, "User with given name does not exists.");
    }

    public class ChangePasswordRequest
    {
        public string OldPassword { get; set; }
        public string NewPassword { get; set; }
    }

    public static class ChangePasswordResponse
    {
        public const int SuccessCode = 0;
        public const int BadOldPasswordCode = 1;
        public const int NotExistsCode = 2;

        public static ReturnCodeMessageResponse Success { get; } = new ReturnCodeMessageResponse(SuccessCode, "Success to change password.");
        public static ReturnCodeMessageResponse BadOldPassword { get; } = new ReturnCodeMessageResponse(BadOldPasswordCode, "Old password is wrong.");
        public static ReturnCodeMessageResponse NotExists { get; } = new ReturnCodeMessageResponse(NotExistsCode, "Username does not exists, please update token.");
    }

    public static class PutAvatarResponse
    {
        public const int SuccessCode = 0;
        public const int ForbiddenCode = 1;
        public const int NotExistsCode = 2;

        public static ReturnCodeMessageResponse Success { get; } = new ReturnCodeMessageResponse(SuccessCode, "Success to upload avatar.");
        public static ReturnCodeMessageResponse Forbidden { get; } = new ReturnCodeMessageResponse(ForbiddenCode, "You are not allowed to upload the user's avatar.");
        public static ReturnCodeMessageResponse NotExists { get; } = new ReturnCodeMessageResponse(NotExistsCode, "The username does not exists. If you are a user, try update your token.");
    }
}

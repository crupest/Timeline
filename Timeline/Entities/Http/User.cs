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

        public static CommonErrorResponse Created { get; } = new CommonErrorResponse(CreatedCode, "A new user is created.");
        public static CommonErrorResponse Modified { get; } = new CommonErrorResponse(ModifiedCode, "A existing user is modified.");
    }

    public static class UserDeleteResponse
    {
        public const int DeletedCode = 0;
        public const int NotExistsCode = 1;

        public static CommonErrorResponse Deleted { get; } = new CommonErrorResponse(DeletedCode, "A existing user is deleted.");
        public static CommonErrorResponse NotExists { get; } = new CommonErrorResponse(NotExistsCode, "User with given name does not exists.");
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

        public static CommonErrorResponse Success { get; } = new CommonErrorResponse(SuccessCode, "Success to change password.");
        public static CommonErrorResponse BadOldPassword { get; } = new CommonErrorResponse(BadOldPasswordCode, "Old password is wrong.");
        public static CommonErrorResponse NotExists { get; } = new CommonErrorResponse(NotExistsCode, "Username does not exists, please update token.");
    }

    public static class PutAvatarResponse
    {
        public const int SuccessCode = 0;
        public const int ForbiddenCode = 1;
        public const int NotExistsCode = 2;

        public static CommonErrorResponse Success { get; } = new CommonErrorResponse(SuccessCode, "Success to upload avatar.");
        public static CommonErrorResponse Forbidden { get; } = new CommonErrorResponse(ForbiddenCode, "You are not allowed to upload the user's avatar.");
        public static CommonErrorResponse NotExists { get; } = new CommonErrorResponse(NotExistsCode, "The username does not exists. If you are a user, try update your token.");
    }
}

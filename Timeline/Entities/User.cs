namespace Timeline.Entities
{
    public class UserModifyRequest
    {
        public string Password { get; set; }
        public string[] Roles { get; set; }
    }

    public class UserPutResponse
    {
        public const int CreatedCode = 0;
        public const int ModifiedCode = 1;

        public static UserPutResponse Created { get; } = new UserPutResponse { ReturnCode = CreatedCode };
        public static UserPutResponse Modified { get; } = new UserPutResponse { ReturnCode = ModifiedCode };

        public int ReturnCode { get; set; }
    }

    public class UserDeleteResponse
    {
        public const int SuccessCode = 0;
        public const int NotExistsCode = 1;

        public static UserDeleteResponse Success { get; } = new UserDeleteResponse { ReturnCode = SuccessCode };
        public static UserDeleteResponse NotExists { get; } = new UserDeleteResponse { ReturnCode = NotExistsCode };

        public int ReturnCode { get; set; }
    }
}

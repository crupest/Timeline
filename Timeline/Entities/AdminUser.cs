namespace Timeline.Entities
{
    public class AdminUserEntityRequest
    {
        public string Password { get; set; }
        public string[] Roles { get; set; }
    }

    public class AdminUserPutResponse
    {
        public const int CreatedCode = 0;
        public const int ModifiedCode = 1;

        public static AdminUserPutResponse Created { get; } = new AdminUserPutResponse { ReturnCode = CreatedCode };
        public static AdminUserPutResponse Modified { get; } = new AdminUserPutResponse { ReturnCode = ModifiedCode };

        public int ReturnCode { get; set; }
    }

    public class AdminUserDeleteResponse
    {
        public const int SuccessCode = 0;
        public const int NotExistsCode = 1;

        public static AdminUserDeleteResponse Success { get; } = new AdminUserDeleteResponse { ReturnCode = SuccessCode };
        public static AdminUserDeleteResponse NotExists { get; } = new AdminUserDeleteResponse { ReturnCode = NotExistsCode };

        public int ReturnCode { get; set; }
    }
}

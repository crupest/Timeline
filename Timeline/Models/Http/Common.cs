namespace Timeline.Models.Http
{
    public class CommonResponse
    {
        public static class ErrorCodes
        {
            /// <summary>
            /// Used when the model is invaid.
            /// For example a required field is null.
            /// </summary>
            public const int InvalidModel = -100;
        }

        public static CommonResponse InvalidModel(string message)
        {
            return new CommonResponse(ErrorCodes.InvalidModel, message);
        }

        public CommonResponse()
        {

        }

        public CommonResponse(int code, string message)
        {
            Code = code;
            Message = message;
        }

        public int Code { get; set; }
        public string Message { get; set; }
    }

    public static class CommonPutResponse
    {
        public const int CreatedCode = 0;
        public const int ModifiedCode = 1;

        public static CommonResponse Created { get; } = new CommonResponse(CreatedCode, "A new item is created.");
        public static CommonResponse Modified { get; } = new CommonResponse(ModifiedCode, "An existent item is modified.");
    }

    public static class CommonDeleteResponse
    {
        public const int DeletedCode = 0;
        public const int NotExistsCode = 1;

        public static CommonResponse Deleted { get; } = new CommonResponse(DeletedCode, "An existent item is deleted.");
        public static CommonResponse NotExists { get; } = new CommonResponse(NotExistsCode, "The item does not exist.");
    }
}

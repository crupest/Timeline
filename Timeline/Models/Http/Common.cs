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

            public const int Header_Missing_ContentType = -111;
            public const int Header_Missing_ContentLength = -112;
            public const int Header_Zero_ContentLength = -113;
        }

        public static CommonResponse InvalidModel(string message)
        {
            return new CommonResponse(ErrorCodes.InvalidModel, message);
        }

        public static CommonResponse MissingContentType()
        {
            return new CommonResponse(ErrorCodes.Header_Missing_ContentType, "Header Content-Type is required.");
        }

        public static CommonResponse MissingContentLength()
        {
            return new CommonResponse(ErrorCodes.Header_Missing_ContentLength, "Header Content-Length is missing or of bad format.");
        }

        public static CommonResponse ZeroContentLength()
        {
            return new CommonResponse(ErrorCodes.Header_Zero_ContentLength, "Header Content-Length must not be 0.");
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

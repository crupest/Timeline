namespace Timeline.Entities.Http
{
    public class CommonResponse
    {
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

namespace Timeline.Entities.Http
{
    public class ReturnCodeMessageResponse
    {
        public ReturnCodeMessageResponse()
        {

        }

        public ReturnCodeMessageResponse(int code)
        {
            ReturnCode = code;
        }

        public ReturnCodeMessageResponse(string message)
        {
            Message = message;
        }

        public ReturnCodeMessageResponse(int code, string message)
        {
            ReturnCode = code;
            Message = message;
        }

        public int? ReturnCode { get; set; } = null;
        public string Message { get; set; } = null;
    }
}

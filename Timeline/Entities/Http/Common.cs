namespace Timeline.Entities.Http
{
    public class CommonErrorResponse
    {
        public CommonErrorResponse()
        {

        }

        public CommonErrorResponse(int code, string message)
        {
            Code = code;
            Message = message;
        }

        public int Code { get; set; }
        public string Message { get; set; }
    }
}

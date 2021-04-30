namespace Timeline.Models.Http
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
        public string? Message { get; set; }
    }
}

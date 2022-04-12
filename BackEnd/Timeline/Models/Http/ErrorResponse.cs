namespace Timeline.Models.Http
{
    public class ErrorResponse
    {
        public const string InvalidRequest = "INVALID_REQUEST";
        public const string EntityExist = "ENTITY_EXIST";
        public const string InvalidOperation = "INVALID_OPERATION";

        public ErrorResponse(string error, string message)
        {
            Error = error;
            Message = message;
        }

        public string Error { get; set; }

        public string Message { get; set; }
    }
}

namespace Timeline.Services.Exceptions
{
    public static class ExceptionMessageHelper
    {
        public static string AppendAdditionalMessage(this string origin, string? message)
        {
            if (message == null)
                return origin;
            else
                return origin + " " + message;
        }
    }
}

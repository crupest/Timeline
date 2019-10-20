using Microsoft.Extensions.Localization;
using Timeline.Helpers;

namespace Timeline.Models.Http
{
    public class CommonResponse
    {
        public static CommonResponse InvalidModel(string message)
        {
            return new CommonResponse(ErrorCodes.Http.Common.InvalidModel, message);
        }

        public static CommonResponse MissingContentType()
        {
            return new CommonResponse(ErrorCodes.Http.Common.Header.Missing_ContentType, "Header Content-Type is required.");
        }

        public static CommonResponse MissingContentLength()
        {
            return new CommonResponse(ErrorCodes.Http.Common.Header.Missing_ContentLength, "Header Content-Length is missing or of bad format.");
        }

        public static CommonResponse ZeroContentLength()
        {
            return new CommonResponse(ErrorCodes.Http.Common.Header.Zero_ContentLength, "Header Content-Length must not be 0.");
        }

        public static CommonResponse BadIfNonMatch()
        {
            return new CommonResponse(ErrorCodes.Http.Common.Header.BadFormat_IfNonMatch, "Header If-Non-Match is of bad format.");
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
        public string? Message { get; set; }
    }

    public class CommonDataResponse<T> : CommonResponse
    {
        public CommonDataResponse()
        {

        }

        public CommonDataResponse(int code, string message, T data)
            : base(code, message)
        {
            Data = data;
        }

        public T Data { get; set; } = default!;
    }

    public static class CommonPutResponse
    {
        public class ResponseData
        {
            public ResponseData(bool create)
            {
                Create = create;
            }

            public bool Create { get; set; }
        }

        internal static CommonDataResponse<ResponseData> Create(IStringLocalizerFactory localizerFactory)
        {
            var localizer = localizerFactory.Create("Http.Common");
            return new CommonDataResponse<ResponseData>(0, localizer["ResponsePutCreate"], new ResponseData(true));
        }

        internal static CommonDataResponse<ResponseData> Modify(IStringLocalizerFactory localizerFactory)
        {
            var localizer = localizerFactory.Create("Http.Common");
            return new CommonDataResponse<ResponseData>(0, localizer["ResponsePutModify"], new ResponseData(false));

        }
    }

    public static class CommonDeleteResponse
    {
        public class ResponseData
        {
            public ResponseData(bool delete)
            {
                Delete = delete;
            }

            public bool Delete { get; set; }
        }

        internal static CommonDataResponse<ResponseData> Delete(IStringLocalizerFactory localizerFactory)
        {
            var localizer = localizerFactory.Create("Http.Common");
            return new CommonDataResponse<ResponseData>(0, localizer["ResponseDeleteDelete"], new ResponseData(true));
        }

        internal static CommonDataResponse<ResponseData> NotExist(IStringLocalizerFactory localizerFactory)
        {
            var localizer = localizerFactory.Create("Http.Common");
            return new CommonDataResponse<ResponseData>(0, localizer["ResponseDeleteNotExist"], new ResponseData(false));
        }
    }
}

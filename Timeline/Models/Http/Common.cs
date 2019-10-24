using Microsoft.Extensions.Localization;
using Timeline.Helpers;

namespace Timeline.Models.Http
{
    public class CommonResponse
    {
        internal static CommonResponse InvalidModel(string message)
        {
            return new CommonResponse(ErrorCodes.Http.Common.InvalidModel, message);
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

    internal static class HeaderErrorResponse
    {
        internal static CommonResponse MissingContentType(IStringLocalizerFactory localizerFactory)
        {
            var localizer = localizerFactory.Create("Models.Http.Common");
            return new CommonResponse(ErrorCodes.Http.Common.Header.Missing_ContentType, localizer["HeaderMissingContentType"]);
        }

        internal static CommonResponse MissingContentLength(IStringLocalizerFactory localizerFactory)
        {
            var localizer = localizerFactory.Create("Models.Http.Common");
            return new CommonResponse(ErrorCodes.Http.Common.Header.Missing_ContentLength, localizer["HeaderMissingContentLength"]);
        }

        internal static CommonResponse ZeroContentLength(IStringLocalizerFactory localizerFactory)
        {
            var localizer = localizerFactory.Create("Models.Http.Common");
            return new CommonResponse(ErrorCodes.Http.Common.Header.Zero_ContentLength, localizer["HeaderZeroContentLength"]);
        }

        internal static CommonResponse BadIfNonMatch(IStringLocalizerFactory localizerFactory)
        {
            var localizer = localizerFactory.Create("Models.Http.Common");
            return new CommonResponse(ErrorCodes.Http.Common.Header.BadFormat_IfNonMatch, localizer["HeaderBadIfNonMatch"]);
        }
    }

    internal static class ContentErrorResponse
    {
        internal static CommonResponse TooBig(IStringLocalizerFactory localizerFactory, string maxLength)
        {
            var localizer = localizerFactory.Create("Models.Http.Common");
            return new CommonResponse(ErrorCodes.Http.Common.Content.TooBig, localizer["ContentTooBig", maxLength]);
        }

        internal static CommonResponse UnmatchedLength_Smaller(IStringLocalizerFactory localizerFactory)
        {
            var localizer = localizerFactory.Create("Models.Http.Common");
            return new CommonResponse(ErrorCodes.Http.Common.Content.UnmatchedLength_Smaller, localizer["ContentUnmatchedLengthSmaller"]);
        }
        internal static CommonResponse UnmatchedLength_Bigger(IStringLocalizerFactory localizerFactory)
        {
            var localizer = localizerFactory.Create("Models.Http.Common");
            return new CommonResponse(ErrorCodes.Http.Common.Content.UnmatchedLength_Bigger, localizer["ContentUnmatchedLengthBigger"]);
        }
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

    public class CommonPutResponse : CommonDataResponse<CommonPutResponse.ResponseData>
    {
        public class ResponseData
        {
            public ResponseData(bool create)
            {
                Create = create;
            }

            public bool Create { get; set; }
        }

        public CommonPutResponse()
        {

        }

        public CommonPutResponse(int code, string message, bool create)
            : base(code, message, new ResponseData(create))
        {

        }

        internal static CommonPutResponse Create(IStringLocalizerFactory localizerFactory)
        {
            var localizer = localizerFactory.Create("Models.Http.Common");
            return new CommonPutResponse(0, localizer["PutCreate"], true);
        }

        internal static CommonPutResponse Modify(IStringLocalizerFactory localizerFactory)
        {
            var localizer = localizerFactory.Create("Models.Http.Common");
            return new CommonPutResponse(0, localizer["PutModify"], false);

        }
    }

    public class CommonDeleteResponse : CommonDataResponse<CommonDeleteResponse.ResponseData>
    {
        public class ResponseData
        {
            public ResponseData(bool delete)
            {
                Delete = delete;
            }

            public bool Delete { get; set; }
        }

        public CommonDeleteResponse()
        {

        }

        public CommonDeleteResponse(int code, string message, bool delete)
            : base(code, message, new ResponseData(delete))
        {

        }

        internal static CommonDeleteResponse Delete(IStringLocalizerFactory localizerFactory)
        {
            var localizer = localizerFactory.Create("Models.Http.Common");
            return new CommonDeleteResponse(0, localizer["DeleteDelete"], true);
        }

        internal static CommonDeleteResponse NotExist(IStringLocalizerFactory localizerFactory)
        {
            var localizer = localizerFactory.Create("Models.Models.Http.Common");
            return new CommonDeleteResponse(0, localizer["DeleteNotExist"], false);
        }
    }
}

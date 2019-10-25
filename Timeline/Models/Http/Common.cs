using System.Globalization;
using static Timeline.Resources.Models.Http.Common;

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
        internal static CommonResponse MissingContentType()
        {
            return new CommonResponse(ErrorCodes.Http.Common.Header.Missing_ContentType, MessageHeaderMissingContentType);
        }

        internal static CommonResponse MissingContentLength()
        {
            return new CommonResponse(ErrorCodes.Http.Common.Header.Missing_ContentLength, MessageHeaderMissingContentLength);
        }

        internal static CommonResponse ZeroContentLength()
        {
            return new CommonResponse(ErrorCodes.Http.Common.Header.Zero_ContentLength, MessageHeaderZeroContentLength);
        }

        internal static CommonResponse BadIfNonMatch()
        {
            return new CommonResponse(ErrorCodes.Http.Common.Header.BadFormat_IfNonMatch, MessageHeaderBadIfNonMatch);
        }
    }

    internal static class ContentErrorResponse
    {
        internal static CommonResponse TooBig(string maxLength)
        {
            return new CommonResponse(ErrorCodes.Http.Common.Content.TooBig,
                string.Format(CultureInfo.CurrentCulture, MessageContentTooBig, maxLength));
        }

        internal static CommonResponse UnmatchedLength_Smaller()
        {
            return new CommonResponse(ErrorCodes.Http.Common.Content.UnmatchedLength_Smaller, MessageContentUnmatchedLengthSmaller);
        }
        internal static CommonResponse UnmatchedLength_Bigger()
        {
            return new CommonResponse(ErrorCodes.Http.Common.Content.UnmatchedLength_Bigger, MessageContentUnmatchedLengthBigger);
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

        internal static CommonPutResponse Create()
        {
            return new CommonPutResponse(0, MessagePutCreate, true);
        }

        internal static CommonPutResponse Modify()
        {
            return new CommonPutResponse(0, MessagePutModify, false);
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

        internal static CommonDeleteResponse Delete()
        {
            return new CommonDeleteResponse(0, MessageDeleteDelete, true);
        }

        internal static CommonDeleteResponse NotExist()
        {
            return new CommonDeleteResponse(0, MessageDeleteNotExist, false);
        }
    }
}

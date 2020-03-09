using static TimelineApp.Resources.Models.Http.Common;

namespace TimelineApp.Models.Http
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
            public ResponseData() { }

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
            public ResponseData() { }

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

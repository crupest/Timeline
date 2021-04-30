namespace Timeline.Models.Http
{
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

        internal static CommonPutResponse Create(bool create)
        {
            return create ? Create() : Modify();
        }

        internal static CommonPutResponse Create()
        {
            return new CommonPutResponse(0, Resource.MessagePutCreate, true);
        }

        internal static CommonPutResponse Modify()
        {
            return new CommonPutResponse(0, Resource.MessagePutModify, false);
        }
    }
}

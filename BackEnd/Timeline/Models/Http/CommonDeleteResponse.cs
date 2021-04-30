namespace Timeline.Models.Http
{
    /// <summary>
    /// Common response for delete method.
    /// </summary>
    public class CommonDeleteResponse : CommonDataResponse<CommonDeleteResponse.ResponseData>
    {
        /// <summary></summary>
        public class ResponseData
        {
            /// <summary></summary>
            public ResponseData() { }

            /// <summary></summary>
            public ResponseData(bool delete)
            {
                Delete = delete;
            }

            /// <summary>
            /// True if the entry is deleted. False if the entry does not exist.
            /// </summary>
            public bool Delete { get; set; }
        }

        /// <summary></summary>
        public CommonDeleteResponse()
        {

        }

        /// <summary></summary>
        public CommonDeleteResponse(int code, string message, bool delete)
            : base(code, message, new ResponseData(delete))
        {

        }

        internal static CommonDeleteResponse Create(bool delete)
        {
            return delete ? Delete() : NotExist();
        }

        internal static CommonDeleteResponse Delete()
        {
            return new CommonDeleteResponse(0, Resource.MessageDeleteDelete, true);
        }

        internal static CommonDeleteResponse NotExist()
        {
            return new CommonDeleteResponse(0, Resource.MessageDeleteNotExist, false);
        }
    }
}

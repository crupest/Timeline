namespace Timeline.Services.Timeline
{
    public class TimelinePostCreateRequestData
    {
        public TimelinePostCreateRequestData(string contentType, byte[] data)
        {
            ContentType = contentType;
            Data = data;
        }

        public string ContentType { get; set; }
#pragma warning disable CA1819 // Properties should not return arrays
        public byte[] Data { get; set; }
#pragma warning restore CA1819 // Properties should not return arrays
    }
}

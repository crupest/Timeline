using System;

namespace Timeline.Models.Validation
{
    public class TimelinePostContentTypeValidator : StringSetValidator
    {
        public TimelinePostContentTypeValidator() : base(TimelinePostContentTypes.AllTypes) { }
    }

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
    public class TimelinePostContentTypeAttribute : ValidateWithAttribute
    {
        public TimelinePostContentTypeAttribute() : base(typeof(TimelinePostContentTypeValidator))
        {

        }
    }
}

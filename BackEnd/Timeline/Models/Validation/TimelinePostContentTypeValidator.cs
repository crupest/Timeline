using System;
using System.Linq;

namespace Timeline.Models.Validation
{
    public class TimelinePostDataKindValidator : StringSetValidator
    {
        public TimelinePostDataKindValidator() : base(TimelinePostDataKind.AllTypes.ToArray()) { }
    }

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
    public class TimelinePostDataKindAttribute : ValidateWithAttribute
    {
        public TimelinePostDataKindAttribute() : base(typeof(TimelinePostDataKindValidator))
        {

        }
    }
}

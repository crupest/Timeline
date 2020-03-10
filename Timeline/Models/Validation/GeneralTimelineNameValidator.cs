using System;

namespace Timeline.Models.Validation
{
    public class GeneralTimelineNameValidator : Validator<string>
    {
        private readonly UsernameValidator _usernameValidator = new UsernameValidator();
        private readonly TimelineNameValidator _timelineNameValidator = new TimelineNameValidator();

        protected override (bool, string) DoValidate(string value)
        {
            if (value.StartsWith('@'))
            {
                return _usernameValidator.Validate(value.Substring(1));
            }
            else
            {
                return _timelineNameValidator.Validate(value);
            }
        }
    }

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter,
        AllowMultiple = false)]
    public class GeneralTimelineNameAttribute : ValidateWithAttribute
    {
        public GeneralTimelineNameAttribute()
            : base(typeof(GeneralTimelineNameValidator))
        {

        }
    }
}

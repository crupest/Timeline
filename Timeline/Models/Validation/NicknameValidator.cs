using System;
using static Timeline.Resources.Models.Validation.NicknameValidator;

namespace Timeline.Models.Validation
{
    public class NicknameValidator : Validator<string>
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1062:Validate arguments of public methods", Justification = "Already checked in base.")]
        protected override (bool, string) DoValidate(string value)
        {
            if (value.Length > 10)
                return (false, MessageTooLong);

            return (true, GetSuccessMessage());
        }
    }

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
    public class NicknameAttribute : ValidateWithAttribute
    {
        public NicknameAttribute() : base(typeof(NicknameValidator))
        {

        }
    }
}

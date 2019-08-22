using System.ComponentModel.DataAnnotations;
using Timeline.Entities;
using Timeline.Models.Validation;

namespace Timeline.Models
{
    public class UserDetail
    {
        [MaxLength(10)]
        public string Nickname { get; set; }

        [ValidateWith(typeof(UserDetailValidators.QQValidator))]
        public string QQ { get; set; }

        [ValidateWith(typeof(UserDetailValidators.EMailValidator))]
        public string EMail { get; set; }

        [ValidateWith(typeof(UserDetailValidators.PhoneNumberValidator))]
        public string PhoneNumber { get; set; }

        public string Description { get; set; }

        private static string CoerceEmptyToNull(string value)
        {
            if (string.IsNullOrEmpty(value))
                return null;
            else
                return value;
        }

        public static UserDetail From(UserDetailEntity entity)
        {
            return new UserDetail
            {
                Nickname = CoerceEmptyToNull(entity.Nickname),
                QQ = CoerceEmptyToNull(entity.QQ),
                EMail = CoerceEmptyToNull(entity.EMail),
                PhoneNumber = CoerceEmptyToNull(entity.PhoneNumber),
                Description = CoerceEmptyToNull(entity.Description)
            };
        }
    }
}

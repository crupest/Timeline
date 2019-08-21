using Timeline.Entities;

namespace Timeline.Models
{
    public class UserDetail
    {
        public string QQ { get; set; }
        public string EMail { get; set; }
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
                QQ = CoerceEmptyToNull(entity.QQ),
                EMail = CoerceEmptyToNull(entity.EMail),
                PhoneNumber = CoerceEmptyToNull(entity.PhoneNumber),
                Description = CoerceEmptyToNull(entity.Description)
            };
        }
    }
}

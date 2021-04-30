using System.Reflection;

namespace Timeline.Services
{
    public class EntityType
    {
        public EntityType(string name)
        {
            Name = name;
        }

        public string Name { get; }

        public int NotExistErrorCode
        {
            get
            {
                var field = typeof(ErrorCodes.NotExist).GetField(Name, BindingFlags.Public | BindingFlags.Static);
                if (field is not null) return (int)field.GetRawConstantValue()!;
                return ErrorCodes.NotExist.Default;
            }
        }

        public int ConflictErrorCode
        {
            get
            {
                var field = typeof(ErrorCodes.Conflict).GetField(Name, BindingFlags.Public | BindingFlags.Static);
                if (field is not null) return (int)field.GetRawConstantValue()!;
                return ErrorCodes.Conflict.Default;
            }
        }
    }
}

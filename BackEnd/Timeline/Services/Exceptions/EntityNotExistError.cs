using System;
using System.Globalization;
using System.Text;

namespace Timeline.Services.Exceptions
{
    /// <summary>
    /// Thrown when you want to get an entity that does not exist.
    /// </summary>
    /// <example>
    /// For example, you want to get a timeline with given name but it does not exist.
    /// </example>
    [Serializable]
    public class EntityNotExistException : Exception
    {
        public EntityNotExistException() : this(null, null, null, null) { }
        public EntityNotExistException(string? entityName) : this(entityName, null, null, null) { }
        public EntityNotExistException(Type? entityType) : this(null, entityType, null, null) { }
        public EntityNotExistException(string? entityName, Exception? inner) : this(entityName, null, null, inner) { }
        public EntityNotExistException(Type? entityType, Exception? inner) : this(null, entityType, null, inner) { }
        public EntityNotExistException(string? entityName, Type? entityType, string? message = null, Exception? inner = null) : base(MakeMessage(entityName, entityType, message), inner)
        {
            EntityName = entityName;
            EntityType = entityType;
        }

        private static string MakeMessage(string? entityName, Type? entityType, string? message)
        {
            string? name = entityName ?? (entityType?.Name);

            var result = new StringBuilder();

            if (name == null)
                result.Append(Resources.Services.Exceptions.EntityNotExistErrorDefault);
            else
                result.AppendFormat(CultureInfo.InvariantCulture, Resources.Services.Exceptions.EntityNotExistError, name);

            if (message != null)
            {
                result.Append(' ');
                result.Append(message);
            }

            return result.ToString();
        }

        protected EntityNotExistException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }

        public string? EntityName { get; }

        public Type? EntityType { get; }
    }
}

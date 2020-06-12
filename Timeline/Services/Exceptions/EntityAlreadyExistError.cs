using System;
using System.Globalization;
using System.Text;

namespace Timeline.Services.Exceptions
{
    /// <summary>
    /// Thrown when an entity is already exists.
    /// </summary>
    /// <remarks>
    /// For example, want to create a timeline but a timeline with the same name already exists.
    /// </remarks>
    [Serializable]
    public class EntityAlreadyExistException : Exception
    {
        private readonly string? _entityName;

        public EntityAlreadyExistException() : this(null, null, null, null) { }

        public EntityAlreadyExistException(string? entityName) : this(entityName, null) { }

        public EntityAlreadyExistException(string? entityName, Exception? inner) : this(entityName, null, null, null, inner) { }

        public EntityAlreadyExistException(string? entityName, object? entity = null) : this(entityName, null, entity, null, null) { }
        public EntityAlreadyExistException(Type? entityType, object? entity = null) : this(null, entityType, entity, null, null) { }
        public EntityAlreadyExistException(string? entityName, Type? entityType, object? entity = null, string? message = null, Exception? inner = null) : base(MakeMessage(entityName, entityType, message), inner)
        {
            _entityName = entityName;
            EntityType = entityType;
            Entity = entity;
        }

        private static string MakeMessage(string? entityName, Type? entityType, string? message)
        {
            string? name = entityName ?? (entityType?.Name);

            var result = new StringBuilder();

            if (name == null)
                result.Append(Resources.Services.Exceptions.EntityAlreadyExistErrorDefault);
            else
                result.AppendFormat(CultureInfo.InvariantCulture, Resources.Services.Exceptions.EntityAlreadyExistError, name);

            if (message != null)
            {
                result.Append(' ');
                result.Append(message);
            }

            return result.ToString();
        }

        protected EntityAlreadyExistException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }

        public string? EntityName => _entityName ?? (EntityType?.Name);

        public Type? EntityType { get; }

        public object? Entity { get; }
    }
}

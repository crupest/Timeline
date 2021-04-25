using System;

namespace Timeline.Services
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
        public EntityAlreadyExistException() : this(null, null, null, null) { }
        public EntityAlreadyExistException(string? entityName) : this(entityName, null, null, null) { }
        public EntityAlreadyExistException(string? entityName, Exception? inner) : this(entityName, null, null, inner) { }
        public EntityAlreadyExistException(string? entityName, object? entity, Exception inner) : this(entityName, entity, null, inner) { }
        public EntityAlreadyExistException(string? entityName, object? entity, string? message, Exception? inner) : base(message ?? Resource.ExceptionEntityAlreadyExist, inner)
        {
            EntityName = entityName;
            Entity = entity;
        }

        protected EntityAlreadyExistException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }

        public string? EntityName { get; }

        public object? Entity { get; }
    }
}

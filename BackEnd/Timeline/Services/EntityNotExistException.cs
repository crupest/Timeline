using System;

namespace Timeline.Services
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
        public EntityNotExistException() : this(null, null) { }
        public EntityNotExistException(string? entityName) : this(entityName, null) { }
        public EntityNotExistException(string? entityName, Exception? inner) : this(entityName, null, inner) { }
        public EntityNotExistException(string? entityName, string? message, Exception? inner) : base(message ?? Resource.ExceptionEntityNotExist, inner)
        {
            EntityName = entityName;
        }
        protected EntityNotExistException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }

        public string? EntityName { get; }
    }
}

using System;
using System.Collections.Generic;

namespace Timeline.Services
{
    /// <summary>
    /// Thrown when you want to get an entity that does not exist.
    /// </summary>
    /// <example>
    /// For example, you want to get a timeline with given name but it does not exist.
    /// </example>
    [Serializable]
    public class EntityNotExistException : EntityException
    {
        public EntityNotExistException() : base() { }
        public EntityNotExistException(string? message) : base(message) { }
        public EntityNotExistException(string? message, Exception? inner) : base(message, inner) { }
        public EntityNotExistException(EntityType entityType, IDictionary<string, object>? constraints = null, string? message = null, Exception? inner = null)
            : base(entityType, constraints, message ?? Resource.ExceptionEntityNotExist, inner)
        {

        }
        protected EntityNotExistException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}

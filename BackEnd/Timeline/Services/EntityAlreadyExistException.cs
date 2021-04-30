using System;
using System.Collections.Generic;

namespace Timeline.Services
{
    /// <summary>
    /// Thrown when an entity is already exists.
    /// </summary>
    /// <remarks>
    /// For example, want to create a timeline but a timeline with the same name already exists.
    /// </remarks>
    [Serializable]
    public class EntityAlreadyExistException : EntityException
    {
        public EntityAlreadyExistException() : base() { }
        public EntityAlreadyExistException(string? message) : base(message) { }
        public EntityAlreadyExistException(string? message, Exception? inner) : base(message, inner) { }
        public EntityAlreadyExistException(EntityType entityType, IDictionary<string, object> constraints, string? message = null, Exception? inner = null)
            : base(entityType, constraints, message ?? Resource.ExceptionEntityNotExist, inner)
        {

        }
        protected EntityAlreadyExistException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}

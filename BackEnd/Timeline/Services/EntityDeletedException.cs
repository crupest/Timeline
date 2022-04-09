using System;
using System.Collections.Generic;

namespace Timeline.Services
{
    /// <summary>
    /// Thrown when an entity is deleted.
    /// </summary>
    [Serializable]
    public class EntityDeletedException : EntityException
    {
        public EntityDeletedException() : base() { }
        public EntityDeletedException(string? message) : base(message) { }
        public EntityDeletedException(string? message, Exception? inner) : base(message, inner) { }
        public EntityDeletedException(EntityType entityType, IDictionary<string, object> constraints, string? message = null, Exception? inner = null)
            : base(entityType, constraints, message ?? Resource.ExceptionEntityNotExist, inner)
        {

        }
        protected EntityDeletedException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}

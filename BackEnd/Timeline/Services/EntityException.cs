using System;
using System.Collections.Generic;
using System.Linq;

namespace Timeline.Services
{
    [Serializable]
    public class EntityException : Exception
    {
        public EntityException() { }
        public EntityException(string? message) : base(message) { }
        public EntityException(string? message, Exception? inner) : base(message, inner) { }
        public EntityException(EntityType entityType, IDictionary<string, object>? constraints = null, string? message = null, Exception? inner = null)
            : base(message, inner)
        {
            EntityType = entityType;
            if (constraints is not null)
                Constraints = constraints;
        }
        protected EntityException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }

        public EntityType EntityType { get; } = EntityTypes.Default;
        public IDictionary<string, object> Constraints { get; } = new Dictionary<string, object>();

        public string GenerateConstraintString()
        {
            return string.Join(' ', Constraints.Select(c => $"[{c.Key} = {c.Value}]"));
        }
    }
}

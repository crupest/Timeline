using NJsonSchema;
using NSwag;
using NSwag.Generation.Processors;
using NSwag.Generation.Processors.Contexts;
using System.Linq;
using Timeline.Models;

namespace Timeline.Swagger
{
    /// <summary>
    /// Coerce ByteData body type into the right one.
    /// </summary>
    public class ByteDataRequestOperationProcessor : IOperationProcessor
    {
        /// <inheritdoc/>
        public bool Process(OperationProcessorContext context)
        {
            var hasByteDataBody = context.MethodInfo.GetParameters().Where(p => p.ParameterType == typeof(ByteData)).Any();
            if (hasByteDataBody)
            {
                var bodyParameter = context.OperationDescription.Operation.Parameters.Where(p => p.Kind == OpenApiParameterKind.Body).Single();
                bodyParameter.Schema = JsonSchema.FromType<byte[]>();
            }
            return true;
        }
    }
}

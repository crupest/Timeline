using NSwag.Generation.Processors;
using NSwag.Generation.Processors.Contexts;
using System.Collections.Generic;

namespace Timeline.Swagger
{
    /// <summary>
    /// Swagger operation processor that adds default description to response.
    /// </summary>
    public class DefaultDescriptionOperationProcessor : IOperationProcessor
    {
        private readonly Dictionary<string, string> defaultDescriptionMap = new Dictionary<string, string>
        {
            ["200"] = "Succeeded to perform the operation.",
            ["304"] = "Item does not change.",
            ["400"] = "See code and message for error info.",
            ["401"] = "You need to log in to perform this operation.",
            ["403"] = "You have no permission to perform the operation.",
            ["404"] = "Item does not exist. See code and message for error info."
        };

        /// <inheritdoc/>
        public bool Process(OperationProcessorContext context)
        {
            var responses = context.OperationDescription.Operation.Responses;

            foreach (var (httpStatusCode, res) in responses)
            {
                if (!string.IsNullOrEmpty(res.Description)) continue;
                if (defaultDescriptionMap.ContainsKey(httpStatusCode))
                {
                    res.Description = defaultDescriptionMap[httpStatusCode];
                }
            }

            return true;
        }
    }
}

using NSwag.Generation.Processors;
using NSwag.Generation.Processors.Contexts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Timeline.Swagger
{
    public class DocumentDescriptionDocumentProcessor : IDocumentProcessor
    {
        private static Dictionary<string, int> GetAllErrorCodes()
        {
            var errorCodes = new Dictionary<string, int>();

            void RecursiveCheckErrorCode(Type type)
            {
                foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
                    .Where(fi => fi.IsLiteral && !fi.IsInitOnly && fi.FieldType == typeof(int)))
                {
                    var name = (type.FullName + "." + field.Name).Remove(0, typeof(ErrorCodes).FullName!.Length + 1).Replace("+", ".", StringComparison.OrdinalIgnoreCase);
                    int value = (int)field.GetRawConstantValue()!;
                    errorCodes.Add(name, value);
                }

                foreach (var nestedType in type.GetNestedTypes())
                {
                    RecursiveCheckErrorCode(nestedType);
                }
            }

            RecursiveCheckErrorCode(typeof(ErrorCodes));

            return errorCodes;
        }

        public void Process(DocumentProcessorContext context)
        {
            StringBuilder description = new StringBuilder();
            description.AppendLine("# Error Codes");
            description.AppendLine("name | value");
            description.AppendLine("---- | -----");
            foreach (var (name, value) in GetAllErrorCodes())
            {
                description.AppendLine($"`{name}` | `{value}`");
            }

            context.Document.Info.Description = description.ToString();
        }
    }
}

using System;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Timeline.ErrorCodes.CodeGenerator
{
    class Program
    {
        static void Main(string[] args)
        {
            string Indent(int n)
            {
                const string indent = "    ";
                return string.Concat(Enumerable.Repeat(indent, n));
            }

            StringBuilder code = new StringBuilder();

            code.AppendLine("using static Timeline.Resources.Messages;");
            code.AppendLine();
            code.AppendLine("namespace Timeline.Models.Http");
            code.AppendLine("{");

            int depth = 1;

            void RecursiveAddErrorCode(Type type, bool root)
            {
                code.AppendLine($"{Indent(depth)}public static class {(root ? "ErrorResponse" : type.Name)}");
                code.AppendLine($"{Indent(depth)}{{");

                foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
                    .Where(fi => fi.IsLiteral && !fi.IsInitOnly && fi.FieldType == typeof(int)))
                {
                    var path = type.FullName.Replace("+", ".").Replace("Timeline.Models.Http.ErrorCodes.", "") + "." + field.Name;

                    code.AppendLine($"{Indent(depth + 1)}public static CommonResponse {field.Name}(params object?[] formatArgs)");
                    code.AppendLine($"{Indent(depth + 1)}{{");
                    code.AppendLine($"{Indent(depth + 2)}return new CommonResponse({"ErrorCodes." + path}, string.Format({path.Replace(".", "_")}, formatArgs));");
                    code.AppendLine($"{Indent(depth + 1)}}}");
                    code.AppendLine();
                    code.AppendLine($"{Indent(depth + 1)}public static CommonResponse CustomMessage_{field.Name}(string message, params object?[] formatArgs)");
                    code.AppendLine($"{Indent(depth + 1)}{{");
                    code.AppendLine($"{Indent(depth + 2)}return new CommonResponse({"ErrorCodes." + path}, string.Format(message, formatArgs));");
                    code.AppendLine($"{Indent(depth + 1)}}}");
                    code.AppendLine();
                }

                depth += 1;

                foreach (var nestedType in type.GetNestedTypes())
                {
                    RecursiveAddErrorCode(nestedType, false);
                }

                depth -= 1;

                code.AppendLine($"{Indent(depth)}}}");
                code.AppendLine();
            }

            RecursiveAddErrorCode(typeof(Timeline.Models.Http.ErrorCodes), true);

            code.AppendLine("}");

            var generatedCode = code.ToString();

            Console.WriteLine(generatedCode);

            TextCopy.ClipboardService.SetText(generatedCode);
            var oldColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Code has copied to clipboard!");
            Console.ForegroundColor = oldColor;
        }
    }
}

using System;
using System.Linq;
using System.Reflection;

namespace Timeline.ErrorCodes.CodeGenerator
{
    class Program
    {
        static void Main(string[] args)
        {
            var code = "";

            void RecursiveAddErrorCode(Type type, bool root)
            {
                code += $@"
        public static class {(root ? "ErrorResponse" : type.Name)}
        {{
";

                foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
                    .Where(fi => fi.IsLiteral && !fi.IsInitOnly && fi.FieldType == typeof(int)))
                {
                    var path = type.FullName.Replace("+", ".").Replace("Timeline.Models.Http.ErrorCodes.", "") + "." + field.Name;

                    code += $@"
            public static CommonResponse {field.Name}(params object?[] formatArgs)
            {{
                return new CommonResponse({"ErrorCodes." + path}, string.Format({path.Replace(".", "_")}, formatArgs));
            }}

            public static CommonResponse CustomMessage_{field.Name}(string message, params object?[] formatArgs)
            {{
                return new CommonResponse({"ErrorCodes." + path}, string.Format(message, formatArgs));
            }}
";
                }

                foreach (var nestedType in type.GetNestedTypes())
                {
                    RecursiveAddErrorCode(nestedType, false);
                }

                code += @"
        }
";
            }

            RecursiveAddErrorCode(typeof(Timeline.Models.Http.ErrorCodes), true);

            code = @"
using static Timeline.Resources.Messages;

namespace Timeline.Models.Http
{
$
}
".Replace("$", code);

            Console.WriteLine(code);

            TextCopy.ClipboardService.SetText(code);
            var oldColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Code has copied to clipboard!");
            Console.ForegroundColor = oldColor;
        }
    }
}

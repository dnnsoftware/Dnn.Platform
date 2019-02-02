using System;
using System.Globalization;
using System.Text;

namespace ClientDependency.TypeScript
{
    /// <summary>
    /// An exception object that is thrown when there is a TypeScript compilation error
    /// </summary>
    public sealed class TypeScriptCompilationException : Exception
    {
        public TypeScriptCompilationError[] Errors { get; private set; }

        internal TypeScriptCompilationException(params TypeScriptCompilationError[] errors)
        {
            Errors = errors;
        }
        
        public override string Message
        {
            get { return string.Format("There were {0} TypeScript compilation errors", Errors.Length); }
        }

        public override string StackTrace
        {
            get
            {
                var builder = new StringBuilder();
                foreach (var e in Errors)
                {
                    builder.Append(e.Message);
                    builder.Append(", Code Block: ");
                    builder.Append(e.CodeBlock);
                    builder.Append(", Start Position: ");
                    builder.Append(e.StartPosition);
                    builder.Append(", Length: ");
                    builder.AppendLine(e.Length.ToString(CultureInfo.InvariantCulture));                    
                }
                return builder.ToString();
            }
        }
    }
}
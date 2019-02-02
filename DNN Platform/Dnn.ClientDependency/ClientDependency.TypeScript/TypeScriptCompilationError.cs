namespace ClientDependency.TypeScript
{
    /// <summary>
    /// Defines one compilation type script error
    /// </summary>
    public sealed class TypeScriptCompilationError
    {
        public TypeScriptCompilationError(string message, string codeBlock, int startPosition, int length)
        {
            Message = message;
            CodeBlock = codeBlock;
            StartPosition = startPosition;
            Length = length;
        }

        public string Message { get; private set; }
        public string CodeBlock { get; private set; }
        public int StartPosition { get; private set; }
        public int Length { get; private set; }
    }
}
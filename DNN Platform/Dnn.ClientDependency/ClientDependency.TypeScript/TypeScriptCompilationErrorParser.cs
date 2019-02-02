using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace ClientDependency.TypeScript
{
    /// <summary>
    /// utility to parse the string output of the type script compilation
    /// </summary>
    internal static class TypeScriptCompilationErrorParser
    {
        public static bool HasErrors(string compilationOutput)
        {
            if (compilationOutput == null) throw new ArgumentNullException("compilationOutput");
            return compilationOutput.Trim().StartsWith("TsCompiler - Compilation errors");
        }

        public static IEnumerable<TypeScriptCompilationError> Parse(string compilationOutput)
        {
            if (compilationOutput == null) throw new ArgumentNullException("compilationOutput");
            if (!HasErrors(compilationOutput))
            {
                throw new InvalidOperationException("There were no errors detected in the compilation output");
            }
            var split = Regex.Split(compilationOutput, @"(?:\n|\r\n){2,}").ToList();
            
            //remove header
            split.RemoveAt(0);
            var output = (from e in split
                          where !string.IsNullOrEmpty(e)
                          select e.Split(new[] {"\r\n", "\n"}, StringSplitOptions.None)
                          into parts
                          select new TypeScriptCompilationError(
                              parts[0],
                              parts[1],
                              int.Parse(parts[2].Split(new[]{':'})[1].Trim()),
                              int.Parse(parts[3].Split(new[]{':'})[1].Trim()))).ToList();
            return output;
        }
    }
}
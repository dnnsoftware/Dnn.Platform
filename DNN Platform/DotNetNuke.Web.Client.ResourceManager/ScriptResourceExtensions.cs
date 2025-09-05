using System;
using DotNetNuke.Abstractions.ClientResources;

namespace DotNetNuke.Web.Client.ResourceManager
{
    public static class ScriptResourceExtensions
    {
        public static IScriptResource SetAsync(this IScriptResource input)
        {
            if (input == null) throw new ArgumentNullException(nameof(input));
            input.Async = true;
            return input;
        }

        public static IScriptResource SetDefer(this IScriptResource input)
        {
            if (input == null) throw new ArgumentNullException(nameof(input));
            input.Defer = true;
            return input;
        }

        public static IScriptResource SetNoModule(this IScriptResource input)
        {
            if (input == null) throw new ArgumentNullException(nameof(input));
            input.NoModule = true;
            return input;
        }

        public static IScriptResource SetType(this IScriptResource input, string type)
        {
            if (input == null) throw new ArgumentNullException(nameof(input));
            input.Type = type;
            return input;
        }
    }
}

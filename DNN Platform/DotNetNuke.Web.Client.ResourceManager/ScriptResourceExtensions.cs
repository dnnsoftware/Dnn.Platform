using System;
using DotNetNuke.Abstractions.ClientResources;

namespace DotNetNuke.Web.Client.ResourceManager
{
    public static class ScriptResourceExtensions
    {
        public static IScriptResource FromSrc(this IScriptResource input, string scriptSrc)
        {
            if (input == null) throw new ArgumentNullException(nameof(input));
            input.FilePath = scriptSrc;
            return input;
        }
    }
}

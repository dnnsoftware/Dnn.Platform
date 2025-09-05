using System;
using DotNetNuke.Abstractions.ClientResources;

namespace DotNetNuke.Web.Client.ResourceManager
{
    public static class StylesheetResourceExtensions
    {
        public static IStylesheetResource SetDisabled(this IStylesheetResource input)
        {
            if (input == null) throw new ArgumentNullException(nameof(input));
            input.Disabled = true;
            return input;
        }
    }
}

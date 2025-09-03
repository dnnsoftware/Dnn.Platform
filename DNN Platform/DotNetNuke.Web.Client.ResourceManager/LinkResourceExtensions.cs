using System;
using DotNetNuke.Abstractions.ClientResources;

namespace DotNetNuke.Web.Client.ResourceManager
{
    public static class LinkResourceExtensions
    {
        public static ILinkResource FromSrc(this ILinkResource input, string scriptSrc)
        {
            if (input == null) throw new ArgumentNullException(nameof(input));
            input.FilePath = scriptSrc;
            return input;
        }
    }
}

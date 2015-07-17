using System;

namespace ClientDependency.Core.CompositeFiles
{
    [Obsolete("Use CssHelper instead")]
    public class CssMin
    {
        public static string CompressCSS(string body)
        {
            return CssHelper.MinifyCss(body);
        }
    }
}

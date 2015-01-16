using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

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

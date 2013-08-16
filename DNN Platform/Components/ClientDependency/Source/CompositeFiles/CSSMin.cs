using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace ClientDependency.Core.CompositeFiles
{
    public class CssMin
    {

        public static string CompressCSS(string body)
        {            
            body = Regex.Replace(body, @"[\n\r]+\s*", string.Empty);
            body = Regex.Replace(body, @"\s+", " ");
            body = Regex.Replace(body, @"\s?([:,;{}])\s?", "$1");
            body = Regex.Replace(body, @"([\s:]0)(px|pt|%|em)", "$1");
            body = Regex.Replace(body, @"/\*[\d\D]*?\*/", string.Empty);
            return body;

        }

    }

}

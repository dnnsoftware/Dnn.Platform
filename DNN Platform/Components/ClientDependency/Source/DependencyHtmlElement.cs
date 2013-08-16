using System.Collections.Generic;

namespace ClientDependency.Core
{
    /// <summary>
    ///  A simple model defining the source of the dependency and the Html Elements that need to be rendered as part of the html tag
    /// </summary>
    public class DependencyHtmlElement
    {
        public string Source { get; set; }
        public IDictionary<string, string> HtmlAttributes { get; set; }
    }
}
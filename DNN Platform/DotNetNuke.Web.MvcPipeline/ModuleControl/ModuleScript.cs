using System.Collections.Generic;
using DotNetNuke.Web.Client;

namespace DotNetNuke.Web.MvcPipeline.ModuleControl
{
    public class ModuleScript
    {
        public string FilePath { get; set; }
        public FileOrder.Js Priority { get; set; } = FileOrder.Js.DefaultPriority;
        public IDictionary<string,string> HtmlAttributes { get; set; }
    }
}

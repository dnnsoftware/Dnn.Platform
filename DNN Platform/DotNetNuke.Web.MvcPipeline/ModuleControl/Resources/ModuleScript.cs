using System.Collections.Generic;
using DotNetNuke.Abstractions.ClientResources;

namespace DotNetNuke.Web.MvcPipeline.ModuleControl.Resources
{
    public class ModuleScript
    {
        public string FilePath { get; set; }
        public FileOrder.Js Priority { get; set; } = FileOrder.Js.DefaultPriority;
        // public IDictionary<string,string> HtmlAttributes { get; set; }
    }
}

using System.Collections.Generic;
using DotNetNuke.Abstractions.ClientResources;

namespace DotNetNuke.Web.MvcPipeline.ModuleControl.Resources
{
    public class ModuleStyleSheet
    {
        public string FilePath { get; set; }
        public FileOrder.Css Priority { get; set; } = FileOrder.Css.DefaultPriority;
        // public IDictionary<string,string> HtmlAttributes { get; set; }
    }
}

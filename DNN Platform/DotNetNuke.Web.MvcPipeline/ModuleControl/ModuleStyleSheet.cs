using System.Collections.Generic;
using DotNetNuke.Web.Client;

namespace DotNetNuke.Web.MvcPipeline.ModuleControl
{
    public class ModuleStyleSheet
    {
        public string FilePath { get; set; }
        public FileOrder.Css Priority { get; set; } = FileOrder.Css.DefaultPriority;
        public IDictionary<string,string> HtmlAttributes { get; set; }
    }
}

using System.Collections.Generic;

namespace DotNetNuke.Web.MvcPipeline.ModuleControl
{
    public class ModuleResources
    {
        public List<ModuleStyleSheet> StyleSheets { get; set; } = new List<ModuleStyleSheet>();
        public List<ModuleScript> Scripts { get; set; } = new List<ModuleScript>();
        public List<string> Libraries { get; set; } = new List<string>();
        public bool AjaxScript { get; set; }
        public bool AjaxAntiForgery { get; set; }
    }
}

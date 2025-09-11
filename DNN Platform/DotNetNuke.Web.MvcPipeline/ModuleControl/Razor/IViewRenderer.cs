using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNetNuke.Web.MvcPipeline.ModuleControl.Razor
{
    public interface IViewRenderer
    {
        string RenderViewToString(string viewPath, object model = null);
    }
}

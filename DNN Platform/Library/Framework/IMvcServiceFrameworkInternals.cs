using System.Web.Mvc;

namespace DotNetNuke.Web.MvcPipeline.Integration.Framework
{
  internal interface IMvcServiceFrameworkInternals
  {
    void RegisterAjaxScript(ControllerContext page);
  }
}

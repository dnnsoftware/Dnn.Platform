using System.IO;
using System.Web.Mvc;

namespace DotNetNuke.Web.Mvc.Framework.ActionResults
{
    public interface IDnnViewResult
    {
        void ExecuteResult(ControllerContext context, TextWriter writer);
    }
}

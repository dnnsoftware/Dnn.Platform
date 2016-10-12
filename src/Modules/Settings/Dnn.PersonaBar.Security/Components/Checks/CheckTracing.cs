using System.Web;
using System.Web.UI;

namespace Dnn.PersonaBar.Security.Components.Checks
{
    public class CheckTracing : IAuditCheck
    {
        public CheckResult Execute()
        {
            var result = new CheckResult(SeverityEnum.Unverified, "CheckTracing");
            var page = HttpContext.Current.Handler as Page;

            if (page != null)
            {
                result.Severity = page.TraceEnabled ? SeverityEnum.Failure : SeverityEnum.Pass;
            }
            return result;
        }
    }
}
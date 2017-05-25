using System;
using System.Web;
using System.Web.UI;

namespace Dnn.PersonaBar.Security.Components.Checks
{
    public class CheckViewstatemac : IAuditCheck
    {
        public CheckResult Execute()
        {
            var result = new CheckResult(SeverityEnum.Unverified, "CheckViewstatemac");
            try
            {
                var page = HttpContext.Current.Handler as Page;

                if (page != null)
                {
                    if (page.EnableViewStateMac == false)
                    {
                        result.Severity = SeverityEnum.Failure;
                    }
                    else
                    {
                        result.Severity = SeverityEnum.Pass;
                    }
                }
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            return result;
        }
    }
}
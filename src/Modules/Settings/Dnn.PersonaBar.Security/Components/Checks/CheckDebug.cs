using System.Web;

namespace Dnn.PersonaBar.Security.Components.Checks
{
    public class CheckDebug : IAuditCheck
    {
        public CheckResult Execute()
        {
            var result = new CheckResult(SeverityEnum.Unverified, "CheckDebug")
            {
                Severity = HttpContext.Current.IsDebuggingEnabled
                    ? SeverityEnum.Warning
                    : SeverityEnum.Pass
            };
            return result;
        }
    }
}
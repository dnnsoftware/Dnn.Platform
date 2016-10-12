using System;
using System.Linq;

namespace Dnn.PersonaBar.Security.Components.Checks
{
    public class CheckUnexpectedExtensions : IAuditCheck
    {
        public CheckResult Execute()
        {
            var result = new CheckResult(SeverityEnum.Unverified, "CheckUnexpectedExtensions");
            try
            {
                var investigatefiles = Utility.FindUnexpectedExtensions();
                if (investigatefiles.Count() > 0)
                {
                    result.Severity = SeverityEnum.Failure;
                    foreach (var filename in investigatefiles)
                    {
                        result.Notes.Add("file:" + filename);
                    }
                }
                else
                {
                    result.Severity = SeverityEnum.Pass;
                }
            }
            catch (Exception)
            {
                throw;
            }
            return result;
        }
    }
}
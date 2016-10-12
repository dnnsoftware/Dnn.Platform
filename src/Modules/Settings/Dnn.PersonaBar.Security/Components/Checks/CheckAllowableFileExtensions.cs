using System;
using DotNetNuke.Entities.Host;

namespace Dnn.PersonaBar.Security.Components.Checks
{
    public class CheckAllowableFileExtensions : IAuditCheck
    {
        public CheckResult Execute()
        {
            var result = new CheckResult(SeverityEnum.Unverified, "CheckAllowableFileExtensions");
            try
            {
                if (Host.AllowedExtensionWhitelist.IsAllowedExtension("asp")
                        || Host.AllowedExtensionWhitelist.IsAllowedExtension("aspx")
                        || Host.AllowedExtensionWhitelist.IsAllowedExtension("php"))
                {
                    result.Severity = SeverityEnum.Failure;
                    result.Notes.Add("Extensions: " + Host.AllowedExtensionWhitelist.ToDisplayString());
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
using System;
using DotNetNuke.Security.Membership;

namespace Dnn.PersonaBar.Security.Components.Checks
{
    public class CheckPasswordFormat : IAuditCheck
    {
        public string Id => "CheckPasswordFormat";

        public bool LazyLoad => false;

        public CheckResult Execute()
        {
            var result = new CheckResult(SeverityEnum.Unverified, Id);
            try
            {
                var format = MembershipProvider.Instance().PasswordFormat;
                if (format == PasswordFormat.Hashed)
                {
                    result.Severity = SeverityEnum.Pass;
                }
                else
                {
                    result.Notes.Add("Setting:" + format.ToString());
                    result.Severity = SeverityEnum.Failure;
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
using System;
using DotNetNuke.Entities.Users;

namespace Dnn.PersonaBar.Security.Components.Checks
{
    public class CheckRarelyUsedSuperuser : IAuditCheck
    {
        public string Id => "CheckRarelyUsedSuperuser";

        public bool LazyLoad => false;

        public CheckResult Execute()
        {
            var result = new CheckResult(SeverityEnum.Unverified, Id);
            try
            {
                var totalRecords = 0;

                var superUsers = UserController.GetUsers(-1, 1, int.MaxValue, ref totalRecords, false, true);
                result.Severity = SeverityEnum.Pass;
                foreach (UserInfo user  in superUsers)
                {
                    if (DateTime.Now.AddMonths(-6) > user.Membership.LastLoginDate ||
                        DateTime.Now.AddMonths(-6) > user.Membership.LastActivityDate)
                    {
                        result.Severity = SeverityEnum.Warning;
                        result.Notes.Add("Superuser:" + user.Username);
                    }
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
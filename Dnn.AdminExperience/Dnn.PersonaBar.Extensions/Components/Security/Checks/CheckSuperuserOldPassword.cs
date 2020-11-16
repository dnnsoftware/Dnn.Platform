// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Security.Components.Checks
{
    using System;

    using DotNetNuke.Entities.Users;

    public class CheckSuperuserOldPassword : IAuditCheck
    {
        public string Id => "CheckSuperuserOldPassword";

        public bool LazyLoad => false;

        public CheckResult Execute()
        {
            var result = new CheckResult(SeverityEnum.Unverified, this.Id);
            try
            {
                var totalRecords = 0;

                var superUsers = UserController.GetUsers(-1, 1, int.MaxValue, ref totalRecords, false, true);
                result.Severity = SeverityEnum.Pass;
                foreach (UserInfo user in superUsers)
                {
                    if (DateTime.Now.AddMonths(-6) > user.Membership.LastPasswordChangeDate)
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

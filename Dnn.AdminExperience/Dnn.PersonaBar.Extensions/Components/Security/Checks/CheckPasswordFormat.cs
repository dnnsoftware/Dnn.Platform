// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Security.Components.Checks
{
    using System;

    using DotNetNuke.Security.Membership;

    public class CheckPasswordFormat : IAuditCheck
    {
        public string Id => "CheckPasswordFormat";

        public bool LazyLoad => false;

        public CheckResult Execute()
        {
            var result = new CheckResult(SeverityEnum.Unverified, this.Id);
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

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Security.Components.Checks
{
    using System;
    using System.Web;
    using System.Web.UI;

    public class CheckViewstatemac : IAuditCheck
    {
        public string Id => "CheckViewstatemac";

        public bool LazyLoad => false;

        public CheckResult Execute()
        {
            var result = new CheckResult(SeverityEnum.Unverified, this.Id);
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
            return result;
        }
    }
}

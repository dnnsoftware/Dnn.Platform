// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Security.Components.Checks
{
    using System.Web;

    public class CheckDebug : IAuditCheck
    {
        public string Id => "CheckDebug";

        public bool LazyLoad => false;

        public CheckResult Execute()
        {
            var result = new CheckResult(SeverityEnum.Unverified, this.Id)
            {
                Severity = HttpContext.Current.IsDebuggingEnabled
                    ? SeverityEnum.Warning
                    : SeverityEnum.Pass
            };
            return result;
        }
    }
}

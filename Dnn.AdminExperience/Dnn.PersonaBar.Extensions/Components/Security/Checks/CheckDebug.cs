// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using System.Web;

namespace Dnn.PersonaBar.Security.Components.Checks
{
    public class CheckDebug : IAuditCheck
    {
        public string Id => "CheckDebug";

        public bool LazyLoad => false;

        public CheckResult Execute()
        {
            var result = new CheckResult(SeverityEnum.Unverified, Id)
            {
                Severity = HttpContext.Current.IsDebuggingEnabled
                    ? SeverityEnum.Warning
                    : SeverityEnum.Pass
            };
            return result;
        }
    }
}

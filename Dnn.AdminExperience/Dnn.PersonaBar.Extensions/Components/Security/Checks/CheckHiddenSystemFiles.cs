// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Security.Components.Checks
{
    using System;
    using System.Linq;

    public class CheckHiddenSystemFiles : IAuditCheck
    {
        public string Id => "CheckHiddenSystemFiles";

        public bool LazyLoad => true;

        public CheckResult Execute()
        {
            var result = new CheckResult(SeverityEnum.Unverified, this.Id);
            try
            {
                var investigatefiles = Utility.FineHiddenSystemFiles();
                if (investigatefiles.Any())
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

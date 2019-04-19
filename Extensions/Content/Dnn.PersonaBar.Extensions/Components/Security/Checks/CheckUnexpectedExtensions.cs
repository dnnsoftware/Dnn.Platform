using System;
using System.Collections.Generic;
using System.Linq;

namespace Dnn.PersonaBar.Security.Components.Checks
{
    public class CheckUnexpectedExtensions : IAuditCheck
    {
        public string Id => "CheckUnexpectedExtensions";

        public bool LazyLoad => true;

        public CheckResult Execute()
        {
            var result = new CheckResult(SeverityEnum.Unverified, Id);
            var invalidFolders = new List<string>();
            var investigatefiles = Utility.FindUnexpectedExtensions(invalidFolders).ToList();
            if (investigatefiles.Count > 0)
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

            if (invalidFolders.Count > 0)
            {
                var folders = string.Join("", invalidFolders.Select(f => $"<p>{f}</p>").ToArray());
                result.Notes.Add($"<p>The following folders are inaccessible due to permission restrictions:</p>{folders}");
            }
            return result;
        }
    }
}
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Security.Components.Checks
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Xml;

    using DotNetNuke.Application;
    using DotNetNuke.Common;

    public class CheckDefaultPage : IAuditCheck
    {
        public string Id => "CheckDefaultPage";

        public bool LazyLoad => false;

        public CheckResult Execute()
        {
            var result = new CheckResult(SeverityEnum.Unverified, this.Id);
            try
            {
                IList<string> modifiedFiles;
                var fileModified = this.CheckDefaultPageModified(out modifiedFiles);
                if (fileModified)
                {
                    if (modifiedFiles.Count == 0)
                    {
                        if (DotNetNukeContext.Current.Application.Version.Major > 6)
                        {
                            result.Notes.Add("There is no data available about your current installation, please upgrade this module to it's latest version.");
                        }
                        else
                        {
                            fileModified = false;
                        }
                    }

                    result.Severity = SeverityEnum.Failure;
                    foreach (var filename in modifiedFiles)
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

        private bool CheckDefaultPageModified(out IList<string> modifiedFiles)
        {
            modifiedFiles = new List<string>();

            var sumData = Utility.LoadFileSumData();

            var appVersion = Utility.GetApplicationVersion();
            var appType = Utility.GetApplicationType();

            var dataNodes = sumData.SelectNodes("/checksums/sum[@version=\"" + appVersion + "\"][@type=\"" + appType + "\"]");
            if (dataNodes == null || dataNodes.Count == 0)
            {
                return true; //when no record matched, need notify user to update the module.
            }

            var fileModified = false;
            foreach (XmlNode node in dataNodes)
            {
                var fileName = node.Attributes["name"].Value;
                var sum = node.Attributes["sum"].Value;
                var file = Path.Combine(Globals.ApplicationMapPath, fileName);
                if (!File.Exists(file) || !Utility.GetFileCheckSum(file).Equals(sum, StringComparison.OrdinalIgnoreCase))
                {
                    fileModified = true;
                    modifiedFiles.Add(fileName);
                }
            }

            return fileModified;
        }
    }
}

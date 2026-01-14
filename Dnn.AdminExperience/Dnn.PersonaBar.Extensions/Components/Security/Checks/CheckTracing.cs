// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Security.Components.Checks
{
    using System;
    using System.IO;
    using System.Net.Http;
    using System.Web;
    using System.Web.Compilation;
    using System.Web.Configuration;
    using System.Web.UI;

    using DotNetNuke.Common;

    public class CheckTracing : IAuditCheck
    {
        /// <inheritdoc/>
        public string Id => "CheckTracing";

        /// <inheritdoc/>
        public bool LazyLoad => false;

        /// <inheritdoc/>
        public CheckResult Execute()
        {
            var result = new CheckResult(SeverityEnum.Unverified, this.Id);

            result.Severity = EnableTrace() ? SeverityEnum.Failure : SeverityEnum.Pass;

            return result;
        }

        private static bool EnableTrace()
        {
            return PageLevelTraceEnabled() || AppLevelTraceEnabled();
        }

        private static bool PageLevelTraceEnabled()
        {
            try
            {
                var defaultPagePath = Path.Combine(Globals.ApplicationMapPath, "Default.aspx");
                using var reader = new StreamReader(File.OpenRead(defaultPagePath));
                var pageDefine = reader.ReadLine();
                if (!string.IsNullOrEmpty(pageDefine))
                {
                    return pageDefine.ToLowerInvariant().Contains("trace=\"true\"");
                }

                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private static bool AppLevelTraceEnabled()
        {
            const string outputCacheSettingsKey = "system.web/trace";
            if (WebConfigurationManager.GetSection(outputCacheSettingsKey) is TraceSection section)
            {
                return section.Enabled;
            }

            return false;
        }
    }
}

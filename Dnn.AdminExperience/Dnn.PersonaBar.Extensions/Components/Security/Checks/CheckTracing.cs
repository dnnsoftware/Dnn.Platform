﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

using System;
using System.IO;
using System.Net.Http;
using System.Web;
using System.Web.Compilation;
using System.Web.Configuration;
using System.Web.UI;
using DotNetNuke.Common;

namespace Dnn.PersonaBar.Security.Components.Checks
{
    public class CheckTracing : IAuditCheck
    {
        public string Id => "CheckTracing";

        public bool LazyLoad => false;

        public CheckResult Execute()
        {
            var result = new CheckResult(SeverityEnum.Unverified, this.Id);

            result.Severity = this.EnableTrace() ? SeverityEnum.Failure : SeverityEnum.Pass;

            return result;
        }

        private bool EnableTrace()
        {
            return this.PageLevelTraceEnabled() || this.AppLevelTraceEnabled();
        }

        private bool PageLevelTraceEnabled()
        {
            try
            {
                var defaultPagePath = Path.Combine(Globals.ApplicationMapPath, "Default.aspx");
                using (var reader = new StreamReader(File.OpenRead(defaultPagePath)))
                {
                    var pageDefine = reader.ReadLine();
                    if (!string.IsNullOrEmpty(pageDefine))
                    {
                        return pageDefine.ToLowerInvariant().Contains("trace=\"true\"");
                    }

                    return false;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

        private bool AppLevelTraceEnabled()
        {
            const string outputCacheSettingsKey = "system.web/trace";
            var section = WebConfigurationManager.GetSection(outputCacheSettingsKey) as TraceSection;
            if (section != null)
            {
                return section.Enabled;
            }

            return false;
        }
    }
}

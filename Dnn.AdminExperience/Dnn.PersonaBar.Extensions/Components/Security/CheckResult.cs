// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Security.Components
{
    using System;
    using System.Collections.Generic;

    using DotNetNuke.Services.Localization;

    public class CheckResult
    {
        public CheckResult(SeverityEnum severity, string checkname)
        {
            this.Severity = severity;
            this.CheckName = checkname;
            this.Notes = new List<string>();
        }

        public string Reason
        {
            get
            {
                return Localization.GetString(this.CheckName + "Reason", this.LocalResourceFile);
            }
        }

        public string FailureText
        {
            get { return Localization.GetString(this.CheckName + "Failure", this.LocalResourceFile); }
        }

        public string SuccessText
        {
            get { return Localization.GetString(this.CheckName + "Success", this.LocalResourceFile); }
        }

        public string CheckNameText
        {
            get
            {

                return this.CheckName + " : " + Localization.GetString(this.CheckName + "Name", this.LocalResourceFile);
            }
        }

        public SeverityEnum Severity { get; set; }
        public string CheckName { get; set; }

        public IList<String> Notes { get; set; }

        private string LocalResourceFile
        {
            get { return "~/DesktopModules/admin/Dnn.PersonaBar/Modules/Dnn.Security/App_LocalResources/Security.resx"; }
        }
    }
}

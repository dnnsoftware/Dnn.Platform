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
        /// <summary>Initializes a new instance of the <see cref="CheckResult"/> class.</summary>
        /// <param name="severity">The result severity.</param>
        /// <param name="checkname">The check name.</param>
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
                return Localization.GetString(this.CheckName + "Reason", LocalResourceFile);
            }
        }

        public string FailureText
        {
            get { return Localization.GetString(this.CheckName + "Failure", LocalResourceFile); }
        }

        public string SuccessText
        {
            get { return Localization.GetString(this.CheckName + "Success", LocalResourceFile); }
        }

        public string CheckNameText
        {
            get
            {
                return this.CheckName + " : " + Localization.GetString(this.CheckName + "Name", LocalResourceFile);
            }
        }

        public SeverityEnum Severity { get; set; }

        public string CheckName { get; set; }

        public IList<string> Notes { get; set; }

        private static string LocalResourceFile => "~/DesktopModules/admin/Dnn.PersonaBar/Modules/Dnn.Security/App_LocalResources/Security.resx";
    }
}

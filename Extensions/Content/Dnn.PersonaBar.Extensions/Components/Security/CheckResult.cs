using System;
using System.Collections.Generic;
using DotNetNuke.Services.Localization;

namespace Dnn.PersonaBar.Security.Components
{
    public class CheckResult
    {
        public CheckResult(SeverityEnum severity, string checkname)
        {
            Severity = severity;
            CheckName = checkname;
            Notes = new List<string>();
        }

        public SeverityEnum Severity { get; set; }
        public string CheckName { get; set; }

        public string Reason
        {
            get
            {
                return Localization.GetString(CheckName + "Reason", LocalResourceFile);
            }
        }

        public string FailureText
        {
            get { return Localization.GetString(CheckName + "Failure", LocalResourceFile); }
        }

        public string SuccessText
        {
            get { return Localization.GetString(CheckName + "Success", LocalResourceFile); }
        }

        public string CheckNameText
        {
            get
            {

                return CheckName + " : " + Localization.GetString(CheckName + "Name", LocalResourceFile);
            }
        }

        public IList<String> Notes { get; set; }

        private string LocalResourceFile
        {
            get { return "~/DesktopModules/admin/Dnn.PersonaBar/Modules/Dnn.Security/App_LocalResources/Security.resx"; }
        }
    }
}
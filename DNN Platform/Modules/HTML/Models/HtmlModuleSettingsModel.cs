// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Modules.Html.Models
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Web.Mvc;

    using DotNetNuke.Modules.Html;
    using DotNetNuke.Web.MvcPipeline.Models;

    public class HtmlModuleSettingsModel : ModuleSettingsModel
    {
        [Display(Name = "plReplaceTokens")] // ID de Settings.ascx
        public bool ReplaceTokens { get; set; }

        [Display(Name = "plDecorate")] // ID de Settings.ascx
        public bool UseDecorate { get; set; }

        [Display(Name = "plSearchDescLength")] // ID de Settings.ascx
        public int SearchDescLength { get; set; }

        public List<WorkflowStateInfo> Workflows { get; set; }

        [Display(Name = "plWorkflow")] // ID de Settings.ascx
        public string SelectedWorkflow { get; set; }

        [Display(Name = "plApplyTo")] // ID de Settings.ascx
        public string ApplyTo { get; set; }

        [Display(Name = "chkReplace")] // ID de Settings.ascx
        public bool Replace { get; set; }
    }
}

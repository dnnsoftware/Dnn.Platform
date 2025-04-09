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
        public bool ReplaceTokens { get; set; }

        public bool UseDecorate { get; set; }

        public int SearchDescLength { get; set; }

        public string SelectedWorkflow { get; set; }

        public string ApplyTo { get; set; }

        public bool Replace { get; set; }
    }
}

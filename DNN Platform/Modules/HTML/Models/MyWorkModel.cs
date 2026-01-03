// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Modules.Html.Models
{
    using System.Collections.Generic;

    using DotNetNuke.Web.MvcPipeline.Models;

    public class MyWorkModel : ModuleModelBase
    {
        public IEnumerable<HtmlTextUserModel> HtmlTextUsers { get; set; }

        public string RedirectUrl { get; internal set; }
    }
}

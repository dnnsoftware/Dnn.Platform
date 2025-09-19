// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.MvcPipeline.Modules
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Web;
    using System.Web.Mvc;

    using DotNetNuke.Services.Localization;

    public static partial class ModuleHelpers
    {
        public static IHtmlString LocalizeString(this HtmlHelper htmlHelper, string key, string localResourceFile)
        {
            return MvcHtmlString.Create(Localization.GetString(key, localResourceFile));
        }
    }
}

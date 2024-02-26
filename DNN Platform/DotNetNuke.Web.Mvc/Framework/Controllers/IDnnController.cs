// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.Mvc.Framework.Controllers
{
    using System.Diagnostics.CodeAnalysis;
    using System.Threading.Tasks;
    using System.Web.Mvc;
    using System.Web.UI;

    using DotNetNuke.Entities.Modules.Actions;
    using DotNetNuke.UI.Modules;
    using DotNetNuke.Web.Mvc.Helpers;

    public interface IDnnController : IController
    {
        ControllerContext ControllerContext { get; }

        ActionResult ResultOfLastExecute { get; }

        Page DnnPage { get; set; }

        string LocalResourceFile { get; set; }

        ModuleActionCollection ModuleActions { get; set; }

        Task<ModuleActionCollection> ModuleActionsAsync { get; set; }

        bool IsAsync { get; set; }

        ModuleInstanceContext ModuleContext { get; set; }

        bool ValidateRequest { get; set; }

        [SuppressMessage("Microsoft.Design", "CA1711:IdentifiersShouldNotHaveIncorrectSuffix", Justification = "Breaking change")]
        ViewEngineCollection ViewEngineCollectionEx { get; set; }

        DnnUrlHelper Url { get; set; }

        string LocalizeString(string key);
    }
}

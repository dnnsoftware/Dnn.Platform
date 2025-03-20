// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.MvcPipeline.Controllers
{
    using System;
    using System.Text;
    using System.Web.Mvc;
    using System.Web.Routing;
    using System.Web.UI;

    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Modules.Actions;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Tabs;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.UI.Modules;

    /*
    using DotNetNuke.Web.Mvc.Framework.ActionResults;
    using DotNetNuke.Web.Mvc.Framework.Modules;
    using DotNetNuke.Web.Mvc.Helpers;
    */

    public abstract class DnnPageController : Controller, IMvcController
    {
        /// <summary>Initializes a new instance of the <see cref="DnnPageController"/> class.</summary>
        protected DnnPageController()
        {
            // this.ActionInvoker = new ResultCapturingActionInvoker();
        }

        public TabInfo ActivePage
        {
            get { return (this.PortalSettings == null) ? null : this.PortalSettings.ActiveTab; }
        }

        public PortalSettings PortalSettings
        {
            get
            {
                return PortalController.Instance.GetCurrentPortalSettings();
            }
        }

        /*
        public ActionResult ResultOfLastExecute
        {
            get
            {
                var actionInvoker = this.ActionInvoker as ResultCapturingActionInvoker;
                return (actionInvoker != null) ? actionInvoker.ResultOfLastInvoke : null;
            }
        }
        */
        /*
        public new UserInfo User
        {
            get { return (this.PortalSettings == null) ? null : this.PortalSettings.UserInfo; }
        }

        public new DnnUrlHelper Url { get; set; }
        */
        public string LocalResourceFile { get; set; }

        public string LocalizeString(string key)
        {
            return Localization.GetString(key, this.LocalResourceFile);
        }

        /// <inheritdoc/>
        protected override void Initialize(RequestContext requestContext)
        {
            base.Initialize(requestContext);

            // this.Url = new DnnUrlHelper(requestContext, this);
        }
    }
}

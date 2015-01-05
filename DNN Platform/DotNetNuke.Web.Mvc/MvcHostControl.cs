#region Copyright
// 
// DotNetNuke® - http://www.dnnsoftware.com
// Copyright (c) 2002-2014
// by DNN Corporation
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation 
// the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and 
// to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions 
// of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
// TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
// CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
// DEALINGS IN THE SOFTWARE.
#endregion

using System;
using System.Globalization;
using System.IO;
using System.Web;
using System.Web.Mvc;
using System.Web.UI;
using DotNetNuke.ComponentModel;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Modules.Actions;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Security;
using DotNetNuke.UI.Modules;
using DotNetNuke.Web.Mvc.Framework;
using DotNetNuke.Web.Mvc.Framework.Modules;
using DotNetNuke.Web.Mvc.Helpers;

namespace DotNetNuke.Web.Mvc
{
    public class MvcHostControl : ModuleControlBase, IActionable
    {
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            if (String.IsNullOrEmpty(ModuleContext.Configuration.ModuleControl.ControlKey))
            {
                LoadActions(ModuleContext.Configuration);
            }

            HttpContextBase httpContext = new HttpContextWrapper(HttpContext.Current);

            var moduleExecutionEngine = ComponentFactory.GetComponent<IModuleExecutionEngine>();

            string moduleRoute = "";

            var moduleApplication = moduleExecutionEngine.GetModuleApplication(ModuleContext.Configuration);

            LoadActions(ModuleContext.Configuration);
            if (String.IsNullOrEmpty(ModuleContext.Configuration.ModuleControl.ControlKey))
            {
                moduleRoute = "";
            }
            else
            {
                var controlKey = ModuleContext.Configuration.ModuleControl.ControlKey;
                moduleRoute = String.Format("{0}/{1}", moduleApplication.DefaultControllerName, controlKey);
            }

            ModuleRequestResult result = moduleExecutionEngine.ExecuteModule(httpContext, ModuleContext.Configuration, moduleRoute);

            if (result != null)
            {
                Controls.Add(new LiteralControl(RenderModule(result, httpContext).ToString()));
            }
        }

        private void LoadActions(ModuleInfo module)
        {
            ModuleActions = new ModuleActionCollection();

            if (String.IsNullOrEmpty(module.ModuleControl.ControlKey))
            {
                var moduleControls = ModuleControlController.GetModuleControlsByModuleDefinitionID(module.ModuleDefID);

                foreach (var moduleControl in moduleControls.Values)
                {
                    if (!String.IsNullOrEmpty(moduleControl.ControlKey))
                    {
                        ModuleActions.Add(this.ModuleContext.GetNextActionID(),
                            moduleControl.ControlKey,
                            moduleControl.ControlKey + ".Action",
                            "",
                            "edit.gif",
                            ModuleContext.EditUrl("Edit"),
                            false,
                            SecurityAccessLevel.Edit,
                            true,
                            false);
                    }
                }
            }
        }

        public MvcHtmlString RenderModule(ModuleRequestResult moduleResult, HttpContextBase httpContext)
        {
            MvcHtmlString moduleOutput;

            using (var writer = new StringWriter(CultureInfo.CurrentCulture))
            {
                var moduleExecutionEngine = ComponentFactory.GetComponent<IModuleExecutionEngine>();

                var site = PortalController.Instance.GetPortal(ModuleContext.PortalId);
                var alias = ModuleContext.PortalAlias;
                var page = ModuleContext.PortalSettings.ActiveTab;

                var siteContext = new SiteContext(httpContext)
                                    {
                                        ActiveSite = site,
                                        ActiveSiteAlias = alias,
                                        ActivePage = page,
                                        ActiveModuleRequest = moduleResult
                                    };

                httpContext.SetSiteContext(siteContext);

                moduleExecutionEngine.ExecuteModuleResult(siteContext, moduleResult, writer);

                moduleOutput = MvcHtmlString.Create(writer.ToString());
            }

            return moduleOutput;
        }

        public ModuleActionCollection ModuleActions { get; private set; }
    }
}

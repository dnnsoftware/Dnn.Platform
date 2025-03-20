// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.MvcPipeline
{
    using System;
    using System.Web;
    using System.Web.Helpers;
    using System.Web.Mvc;
    using System.Web.Mvc.Html;

    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Framework;

    // using DotNetNuke.Framework.Models;
    using DotNetNuke.Web.MvcPipeline.Framework;
    using DotNetNuke.Web.MvcPipeline.Models;
    using DotNetNuke.Web.MvcPipeline.UI.Utilities;

    public static partial class HtmlHelpers
    {
        public static IHtmlString ViewComponent(this HtmlHelper htmlHelper, string controllerName, object model)
        {
            return htmlHelper.Action("Invoke", controllerName, model);
        }

        public static IHtmlString Control(this HtmlHelper htmlHelper, string controlSrc, object model)
        {
            try
            {
                return htmlHelper.Action(
                    "Invoke",
                    MvcUtils.GetControlControllerName(controlSrc),
                    model);
            }
            catch (Exception ex)
            {
                throw new Exception($"{ex.Message} - {MvcUtils.GetControlControllerName(controlSrc)} - Invoke", ex);
            }
        }

        public static IHtmlString Control(this HtmlHelper htmlHelper, string controlSrc, ModuleInfo module)
        {
            try
            {
                return htmlHelper.Action(
                    "Invoke",
                    MvcUtils.GetControlControllerName(controlSrc),
                    new ControlViewModel()
                    {
                        ModuleId = module.ModuleID,
                        TabId = module.TabID,
                        ModuleControlId = module.ModuleControlId,
                        PanaName = module.PaneName,
                        ContainerSrc = module.ContainerSrc,
                        ContainerPath = module.ContainerPath,
                        IconFile = module.IconFile,
                    });
            }
            catch (Exception ex)
            {
                throw new Exception($"{ex.Message} - {MvcUtils.GetControlControllerName(controlSrc)} - Invoke", ex);
            }
        }

        public static IHtmlString Control(this HtmlHelper htmlHelper, ModuleInfo module)
        {
            try
            {
                return htmlHelper.Action(
                    "Invoke",
                    MvcUtils.GetControlControllerName(module.ModuleControl.ControlSrc),
                    new ControlViewModel()
                    {
                        ModuleId = module.ModuleID,
                        TabId = module.TabID,
                        ModuleControlId = module.ModuleControlId,
                        PanaName = module.PaneName,
                        ContainerSrc = module.ContainerSrc,
                        ContainerPath = module.ContainerPath,
                        IconFile = module.IconFile,
                    });
            }
            catch (Exception ex)
            {
                throw new Exception($"{ex.Message} - {MvcUtils.GetControlControllerName(module.ModuleControl.ControlSrc)} - Invoke", ex);
            }
        }

        public static IHtmlString CspNonce(this HtmlHelper htmlHelper)
        {
            return new MvcHtmlString(htmlHelper.ViewContext.HttpContext.Items["CSP-NONCE"].ToString());
        }

        public static IHtmlString RegisterAjaxScriptIfRequired(this HtmlHelper htmlHelper)
        {
            if (MvcServicesFrameworkInternal.Instance.IsAjaxScriptSupportRequired)
            {
                MvcServicesFrameworkInternal.Instance.RegisterAjaxScript(htmlHelper.ViewContext.Controller.ControllerContext);
            }

            return new MvcHtmlString(string.Empty);
        }

        public static IHtmlString AntiForgeryIfRequired(this HtmlHelper htmlHelper)
        {
            // ServicesFramework.Instance.RequestAjaxAntiForgerySupport(); // add also jquery
            if (ServicesFrameworkInternal.Instance.IsAjaxAntiForgerySupportRequired)
            {
                // var antiForgery = AntiForgery.GetHtml().ToHtmlString();
                return AntiForgery.GetHtml();
            }

            return new MvcHtmlString(string.Empty);
        }
    }
}

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.DDRMenu.TemplateEngine
{
    using System;
    using System.IO;
    using System.Reflection;
    using System.Security;
    using System.Web;
    using System.Web.UI;

    using DotNetNuke.Entities.Users;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.Web.DDRMenu.DNNCommon;

    public class XsltFunctions
    {
        public bool UserIsInRole(string roleName)
        {
            return UserController.Instance.GetCurrentUserInfo().IsInRole(roleName);
        }

        public string GetLoginURL()
        {
            return DNNAbstract.GetLoginUrl();
        }

        public string GetLoginText()
        {
            try
            {
                return Localization.GetString(
                    HttpContext.Current.Request.IsAuthenticated ? "Logout" : "Login",
                    DNNContext.Current.ResolveUrl("~/Admin/Skins/" + Localization.LocalResourceDirectory + "/Login.ascx"));
            }
            catch (Exception)
            {
                return "Login";
            }
        }

        public string GetUserURL()
        {
            return DNNAbstract.GetUserUrl();
        }

        public string GetUserText()
        {
            try
            {
                return HttpContext.Current.Request.IsAuthenticated
                        ? UserController.Instance.GetCurrentUserInfo().DisplayName
                        : Localization.GetString(
                            "Register",

                            // ReSharper disable PossibleNullReferenceException
                            (HttpContext.Current.Items["DDRMenuHostControl"] as Control).ResolveUrl(
                                "~/Admin/Skins/" + Localization.LocalResourceDirectory + "/User.ascx"));

                // ReSharper restore PossibleNullReferenceException
            }
            catch (Exception exc)
            {
                return exc.ToString();
            }
        }

        public string EscapeXML(string xml)
        {
            return SecurityElement.Escape(xml);
        }

        public string HtmlEncode(string s)
        {
            return HttpUtility.HtmlEncode(s);
        }

        public string GetString(string name, string resourceFile)
        {
            var resolver = HttpContext.Current.Items["Resolver"] as PathResolver;
            var resolvedFile = string.Empty;
            if (resolver != null)
            {
                var localFile = resolver.Resolve(
                    resourceFile,
                    PathResolver.RelativeTo.Manifest,
                    PathResolver.RelativeTo.Skin,
                    PathResolver.RelativeTo.Module,
                    PathResolver.RelativeTo.Dnn);
                if (File.Exists(HttpContext.Current.Server.MapPath(localFile)))
                {
                    resolvedFile = localFile;
                }
            }

            if (string.IsNullOrEmpty(resolvedFile))
            {
                resolvedFile = "~/" + resourceFile;
            }

            return Localization.GetString(name, resolvedFile);
        }

        public bool ActionCommandIs(string commandName, string constName)
        {
            try
            {
                return
                    commandName.Equals(
                        typeof(DotNetNuke.Entities.Modules.Actions.ModuleActionType).InvokeMember(
                            constName, BindingFlags.GetField, null, null, new object[0]));
            }
            catch (MissingFieldException)
            {
                return false;
            }
        }
    }
}

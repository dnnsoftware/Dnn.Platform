// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.UI.Modules.Html5;

using System;
using System.Collections;
using System.Web.UI;

using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Modules.Actions;
using DotNetNuke.Framework;
using DotNetNuke.Instrumentation;
using DotNetNuke.Services.Personalization;
using DotNetNuke.Services.Tokens;

public class Html5ModuleTokenReplace : HtmlTokenReplace
{
    private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Html5ModuleTokenReplace));

    private static Hashtable businessControllers = new Hashtable();

    /// <summary>Initializes a new instance of the <see cref="Html5ModuleTokenReplace"/> class.</summary>
    /// <param name="page"></param>
    /// <param name="html5File"></param>
    /// <param name="moduleContext"></param>
    /// <param name="moduleActions"></param>
    public Html5ModuleTokenReplace(Page page, string html5File, ModuleInstanceContext moduleContext, ModuleActionCollection moduleActions)
        : base(page)
    {
        this.AccessingUser = moduleContext.PortalSettings.UserInfo;
        this.DebugMessages = Personalization.GetUserMode() != Entities.Portals.PortalSettings.Mode.View;
        this.ModuleId = moduleContext.ModuleId;
        this.PortalSettings = moduleContext.PortalSettings;

        this.AddPropertySource("moduleaction", new ModuleActionsPropertyAccess(moduleContext, moduleActions));
        this.AddPropertySource("resx", new ModuleLocalizationPropertyAccess(moduleContext, html5File));
        this.AddPropertySource("modulecontext", new ModuleContextPropertyAccess(moduleContext));
        this.AddPropertySource("request", new RequestPropertyAccess(page.Request));

        // DNN-7750
        var bizClass = moduleContext.Configuration.DesktopModule.BusinessControllerClass;

        var businessController = this.GetBusinessController(bizClass);
        if (businessController != null)
        {
            var tokens = businessController.GetTokens(page, moduleContext);
            foreach (var token in tokens)
            {
                this.AddPropertySource(token.Key, token.Value);
            }
        }
    }

    private ICustomTokenProvider GetBusinessController(string bizClass)
    {
        if (string.IsNullOrEmpty(bizClass))
        {
            return null;
        }

        if (businessControllers.ContainsKey(bizClass))
        {
            return businessControllers[bizClass] as ICustomTokenProvider;
        }

        try
        {
            var controller = Reflection.CreateObject(bizClass, bizClass) as ICustomTokenProvider;
            businessControllers.Add(bizClass, controller);

            return controller;
        }
        catch (Exception exc)
        {
            Logger.Error(exc);
        }

        return null;
    }
}

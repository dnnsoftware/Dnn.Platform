#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2018
// by DotNetNuke Corporation
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
using System.Collections;
using System.Web.UI;
using DotNetNuke.Framework;
using DotNetNuke.Services.Tokens;
using DotNetNuke.Instrumentation;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Modules.Actions;

namespace DotNetNuke.UI.Modules.Html5
{
    public class Html5ModuleTokenReplace : HtmlTokenReplace
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Html5ModuleTokenReplace));

        private static Hashtable _businessControllers = new Hashtable();

        public Html5ModuleTokenReplace(Page page, string html5File, ModuleInstanceContext moduleContext, ModuleActionCollection moduleActions)
            : base(page)
        {
            AccessingUser = moduleContext.PortalSettings.UserInfo;
            DebugMessages = moduleContext.PortalSettings.UserMode != Entities.Portals.PortalSettings.Mode.View;
            ModuleId = moduleContext.ModuleId;
            PortalSettings = moduleContext.PortalSettings;

            PropertySource["moduleaction"] = new ModuleActionsPropertyAccess(moduleContext, moduleActions);
            PropertySource["resx"] = new ModuleLocalizationPropertyAccess(moduleContext, html5File);
            PropertySource["modulecontext"] = new ModuleContextPropertyAccess(moduleContext);
            PropertySource["request"] = new RequestPropertyAccess(page.Request);

            // DNN-7750
            var bizClass = moduleContext.Configuration.DesktopModule.BusinessControllerClass;
            
            var businessController = GetBusinessController(bizClass);
            if (businessController != null)
            {
                var tokens = businessController.GetTokens(page, moduleContext);
                foreach (var token in tokens)
                {
                    PropertySource.Add(token.Key, token.Value);
                }
            }
        }

        private ICustomTokenProvider GetBusinessController(string bizClass)
        {
            if (string.IsNullOrEmpty(bizClass))
            {
                return null;
            }

            if (_businessControllers.ContainsKey(bizClass))
            {
                return _businessControllers[bizClass] as ICustomTokenProvider;
            }

            try
            {
                var controller = Reflection.CreateObject(bizClass, bizClass) as ICustomTokenProvider;
                _businessControllers.Add(bizClass, controller);

                return controller;
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
            }
           
            return null;
        }
    }
}

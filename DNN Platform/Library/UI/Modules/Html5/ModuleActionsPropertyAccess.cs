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
using DotNetNuke.Entities.Modules.Actions;
using DotNetNuke.Entities.Users;
using DotNetNuke.Security;
using DotNetNuke.Services.Localization;
using DotNetNuke.Services.Tokens;
using Newtonsoft.Json;

namespace DotNetNuke.UI.Modules.Html5
{
    public class ModuleActionDto
    {
        [JsonProperty("controlkey")]
        public string ControlKey { get; set; }

        [JsonProperty("icon")]
        public string Icon { get; set; }

        [JsonProperty("localresourcefile")]
        public string LocalResourceFile { get; set; }

        [JsonProperty("securitysccesslevel")]
        public string SecurityAccessLevel { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("titlekey")]
        public string TitleKey { get; set; }

        [JsonProperty("script")]
        public string Script { get; set; }
    }

    public class ModuleActionsPropertyAccess : JsonPropertyAccess<ModuleActionDto>
    {
        private readonly ModuleActionCollection _moduleActions;
        private readonly ModuleInstanceContext _moduleContext;

        public ModuleActionsPropertyAccess(ModuleInstanceContext moduleContext, ModuleActionCollection moduleActions)
        {
            _moduleContext = moduleContext;
            _moduleActions = moduleActions;
        }

        protected override string ProcessToken(ModuleActionDto model, UserInfo accessingUser, Scope accessLevel)
        {
            var title = (!String.IsNullOrEmpty(model.TitleKey) && !String.IsNullOrEmpty(model.LocalResourceFile))
                                ? Localization.GetString(model.TitleKey, model.LocalResourceFile)
                                : model.Title;

            SecurityAccessLevel securityAccessLevel = SecurityAccessLevel.View;

            if (!String.IsNullOrEmpty(model.SecurityAccessLevel))
            {
                switch (model.SecurityAccessLevel)
                {
                    case "Edit":
                        securityAccessLevel = SecurityAccessLevel.Edit;
                        break;
                    case "Admin":
                        securityAccessLevel = SecurityAccessLevel.Admin;
                        break;
                    case "Host":
                        securityAccessLevel = SecurityAccessLevel.Host;
                        break;
                    default:
                        securityAccessLevel = SecurityAccessLevel.View;
                        break;
                }
            }

            var moduleAction = new ModuleAction(_moduleContext.GetNextActionID())
            {
                Title = title,
                Icon = model.Icon,
                Secure = securityAccessLevel
            };

            if (string.IsNullOrEmpty(model.Script))
            {
                moduleAction.Url = _moduleContext.EditUrl(model.ControlKey);
            }
            else
            {
                moduleAction.Url = model.Script.ToLower().StartsWith("javascript:") ? 
                                    model.Script : 
                                    string.Format("javascript:{0}", model.Script);
            }

            _moduleActions.Add(moduleAction);

            return String.Empty;
        }
    }
}

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.UI.Modules.Html5
{
    using System;

    using DotNetNuke.Entities.Modules.Actions;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Security;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.Services.Tokens;
    using Newtonsoft.Json;

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
            this._moduleContext = moduleContext;
            this._moduleActions = moduleActions;
        }

        protected override string ProcessToken(ModuleActionDto model, UserInfo accessingUser, Scope accessLevel)
        {
            var title = (!string.IsNullOrEmpty(model.TitleKey) && !string.IsNullOrEmpty(model.LocalResourceFile))
                                ? Localization.GetString(model.TitleKey, model.LocalResourceFile)
                                : model.Title;

            SecurityAccessLevel securityAccessLevel = SecurityAccessLevel.View;

            if (!string.IsNullOrEmpty(model.SecurityAccessLevel))
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

            var moduleAction = new ModuleAction(this._moduleContext.GetNextActionID())
            {
                Title = title,
                Icon = model.Icon,
                Secure = securityAccessLevel,
            };

            if (string.IsNullOrEmpty(model.Script))
            {
                moduleAction.Url = this._moduleContext.EditUrl(model.ControlKey);
            }
            else
            {
                moduleAction.Url = model.Script.StartsWith("javascript:", StringComparison.InvariantCultureIgnoreCase) ?
                                    model.Script :
                                    string.Format("javascript:{0}", model.Script);
            }

            this._moduleActions.Add(moduleAction);

            return string.Empty;
        }
    }
}

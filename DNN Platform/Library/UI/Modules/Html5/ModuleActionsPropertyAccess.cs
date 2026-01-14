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

    public class ModuleActionsPropertyAccess : JsonPropertyAccess<ModuleActionDto>
    {
        private readonly ModuleActionCollection moduleActions;
        private readonly ModuleInstanceContext moduleContext;

        /// <summary>Initializes a new instance of the <see cref="ModuleActionsPropertyAccess"/> class.</summary>
        /// <param name="moduleContext">The module context.</param>
        /// <param name="moduleActions">The module actions.</param>
        public ModuleActionsPropertyAccess(ModuleInstanceContext moduleContext, ModuleActionCollection moduleActions)
        {
            this.moduleContext = moduleContext;
            this.moduleActions = moduleActions;
        }

        /// <inheritdoc/>
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

            var moduleAction = new ModuleAction(this.moduleContext.GetNextActionID())
            {
                Title = title,
                Icon = model.Icon,
                Secure = securityAccessLevel,
            };

            if (string.IsNullOrEmpty(model.Script))
            {
                moduleAction.Url = this.moduleContext.EditUrl(model.ControlKey);
            }
            else
            {
                moduleAction.Url =
                    model.Script.StartsWith("javascript:", StringComparison.OrdinalIgnoreCase)
                        ? model.Script
                        : $"javascript:{model.Script}";
            }

            this.moduleActions.Add(moduleAction);

            return string.Empty;
        }
    }
}

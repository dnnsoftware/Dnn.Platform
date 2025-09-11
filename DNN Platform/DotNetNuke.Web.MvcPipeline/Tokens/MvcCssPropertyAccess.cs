// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

// ReSharper disable once CheckNamespace
namespace DotNetNuke.Web.MvcPipeline.Tokens
{
    using System;
    using System.Web.Mvc;

    using DotNetNuke.Entities.Users;
    using DotNetNuke.Services.Tokens;
    using DotNetNuke.Web.Client;
    using DotNetNuke.Web.Client.ClientResourceManagement;

    /// <summary>Property Access implementation for CSS registration in MVC context.</summary>
    public class MvcCssPropertyAccess : JsonPropertyAccess<StylesheetDto>
    {
        private readonly ControllerContext controllerContext;

        /// <summary>Initializes a new instance of the <see cref="MvcCssPropertyAccess"/> class.</summary>
        /// <param name="controllerContext">The controller context.</param>
        public MvcCssPropertyAccess(ControllerContext controllerContext)
        {
            this.controllerContext = controllerContext;
        }

        /// <inheritdoc/>
        protected override string ProcessToken(StylesheetDto model, UserInfo accessingUser, Scope accessLevel)
        {
            if (string.IsNullOrEmpty(model.Path))
            {
                throw new ArgumentException("The Css token must specify a path or property.");
            }

            if (model.Priority == 0)
            {
                model.Priority = (int)FileOrder.Css.DefaultPriority;
            }

            if (string.IsNullOrEmpty(model.Provider))
            {
                MvcClientResourceManager.RegisterStyleSheet(this.controllerContext, model.Path, model.Priority);
            }
            else
            {
                MvcClientResourceManager.RegisterStyleSheet(this.controllerContext, model.Path, model.Priority, model.Provider);
            }

            return string.Empty;
        }
    }
}

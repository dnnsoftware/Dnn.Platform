// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

// ReSharper disable once CheckNamespace
namespace DotNetNuke.Services.Tokens
{
    using System;
    using System.Web;
    using System.Web.UI;

    using DotNetNuke.Abstractions.ClientResources;
    using DotNetNuke.Common;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Web.Client.ClientResourceManagement;
    using DotNetNuke.Web.Client.ResourceManager;
    using Microsoft.Extensions.DependencyInjection;

    public class CssPropertyAccess : JsonPropertyAccess<StylesheetDto>
    {
        private readonly Page page;

        /// <summary>Initializes a new instance of the <see cref="CssPropertyAccess"/> class.</summary>
        /// <param name="page">The page to which the CSS should be registered, or <see langword="null"/> when not rendering a WebForms page.</param>
        public CssPropertyAccess(Page page)
        {
            this.page = page;
        }

        /// <inheritdoc />
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

            // ClientResourceManager also skips missing files and strips legacy query strings, which requires a Page.
            var currentPage = this.page ?? HttpContext.Current?.CurrentHandler as Page;
            if (currentPage != null)
            {
                if (string.IsNullOrEmpty(model.Provider))
                {
                    ClientResourceManager.RegisterStyleSheet(currentPage, model.Path, model.Priority);
                }
                else
                {
                    ClientResourceManager.RegisterStyleSheet(currentPage, model.Path, model.Priority, model.Provider);
                }

                return string.Empty;
            }

            var stylesheet = GetClientResourcesController().CreateStylesheet(model.Path).SetPriority(model.Priority);
            if (!string.IsNullOrEmpty(model.Provider))
            {
                stylesheet = stylesheet.SetProvider(model.Provider);
            }

            stylesheet.Register();
            return string.Empty;
        }

        private static IClientResourceController GetClientResourcesController()
        {
            return Globals.GetCurrentServiceProvider().GetRequiredService<IClientResourceController>();
        }
    }
}

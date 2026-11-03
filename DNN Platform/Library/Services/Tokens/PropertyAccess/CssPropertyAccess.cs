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
    using DotNetNuke.Web.Client.ResourceManager;

    using Microsoft.Extensions.DependencyInjection;

    public class CssPropertyAccess : JsonPropertyAccess<StylesheetDto>
    {
        /// <summary>Initializes a new instance of the <see cref="CssPropertyAccess"/> class.</summary>
        public CssPropertyAccess()
        {
        }

        /// <summary>Initializes a new instance of the <see cref="CssPropertyAccess"/> class.</summary>
        /// <param name="page">The page to which the CSS should be registered.</param>
        [Obsolete("Deprecated in DotNetNuke 10.5.0. Please use the parameterless constructor. Scheduled removal in v12.0.0.")]
        public CssPropertyAccess(Page page)
        {
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

            var clientResourceController = Globals.GetCurrentServiceProvider().GetRequiredService<IClientResourceController>();
            if (string.IsNullOrEmpty(model.Provider))
            {
                clientResourceController.CreateStylesheet(model.Path).SetPriority(model.Priority).Register();
            }
            else
            {
                clientResourceController.CreateStylesheet(model.Path).SetPriority(model.Priority).SetProvider(model.Provider).Register();
            }

            return string.Empty;
        }
    }
}

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

// ReSharper disable once CheckNamespace
namespace DotNetNuke.Services.Tokens
{
    using System;
    using System.Net.Configuration;
    using System.Web.UI;

    using DotNetNuke.Abstractions.ClientResources;
    using DotNetNuke.Common;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Services.ClientDependency;
    using DotNetNuke.Web.Client.ResourceManager;
    using Microsoft.Extensions.DependencyInjection;

    public class CssPropertyAccess : JsonPropertyAccess<StylesheetDto>
    {
        private readonly IClientResourceController clientResourceController;

        /// <summary>Initializes a new instance of the <see cref="CssPropertyAccess"/> class.</summary>
        /// <param name="page">The page to which the CSS should be registered.</param>
        [Obsolete("Deprecated in DotNetNuke 10.0.0. Please use overload with IClientResourceController. Scheduled removal in v12.0.0.")]
        public CssPropertyAccess(Page page)
            : this((IClientResourceController)null)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="CssPropertyAccess"/> class.</summary>
        /// <param name="clientResourceController">The client resource controller.</param>
        public CssPropertyAccess(IClientResourceController clientResourceController)
        {
            this.clientResourceController = clientResourceController ?? Globals.GetCurrentServiceProvider().GetRequiredService<IClientResourceController>();
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
                this.clientResourceController.RegisterStylesheet(model.Path, (FileOrder.Css)model.Priority);
            }
            else
            {
                this.clientResourceController
                    .CreateStylesheet()
                    .FromSrc(model.Path)
                    .SetPriority((FileOrder.Css)model.Priority)
                    .SetProvider(model.Provider)
                    .Register();
            }

            return string.Empty;
        }
    }
}

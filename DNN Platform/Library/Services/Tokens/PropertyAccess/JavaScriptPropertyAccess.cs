// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

// ReSharper disable once CheckNamespace
namespace DotNetNuke.Services.Tokens
{
    using System;
    using System.Collections.Generic;
    using System.Web;
    using System.Web.UI;

    using DotNetNuke.Abstractions.ClientResources;
    using DotNetNuke.Common;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Framework.JavaScriptLibraries;
    using DotNetNuke.Web.Client.ClientResourceManagement;
    using DotNetNuke.Web.Client.ResourceManager;

    using Microsoft.Extensions.DependencyInjection;

    /// <summary>Property Access implementation for javascript registration.</summary>
    public class JavaScriptPropertyAccess : JsonPropertyAccess<JavaScriptDto>
    {
        private readonly Page page;

        /// <summary>Initializes a new instance of the <see cref="JavaScriptPropertyAccess"/> class.</summary>
        public JavaScriptPropertyAccess()
        {
        }

        /// <summary>Initializes a new instance of the <see cref="JavaScriptPropertyAccess"/> class.</summary>
        /// <param name="page">The current page.</param>
        [Obsolete("Deprecated in DotNetNuke 10.4.0. Please use the parameterless constructor. Scheduled removal in v12.0.0.")]
        public JavaScriptPropertyAccess(Page page)
        {
            this.page = page;
        }

        /// <inheritdoc />
        protected override string ProcessToken(JavaScriptDto model, UserInfo accessingUser, Scope accessLevel)
        {
            if (string.IsNullOrEmpty(model.JsName) && string.IsNullOrEmpty(model.Path))
            {
                throw new ArgumentException("If the jsname property is not specified then the JavaScript token must specify a path.");
            }

            if (model.Priority == 0)
            {
                model.Priority = (int)FileOrder.Js.DefaultPriority;
            }

            if (string.IsNullOrEmpty(model.Path))
            {
                RegisterInstalledLibrary(model);
            }
            else
            {
                this.RegisterScript(model);
            }

            return string.Empty;
        }

        private static void RegisterInstalledLibrary(JavaScriptDto model)
        {
            Version version = null;
            var specific = SpecificVersion.Latest;
            if (!string.IsNullOrEmpty(model.Version))
            {
                version = new Version(model.Version);

                if (!string.IsNullOrEmpty(model.Specific))
                {
                    switch (model.Specific)
                    {
                        case "Exact":
                            specific = SpecificVersion.Exact;
                            break;
                        case "LatestMajor":
                            specific = SpecificVersion.LatestMajor;
                            break;
                        case "LatestMinor":
                            specific = SpecificVersion.LatestMinor;
                            break;
                        default:
                            specific = SpecificVersion.Latest;
                            break;
                    }
                }
            }

            JavaScript.RequestRegistration(model.JsName, version, specific);
        }

        private void RegisterScript(JavaScriptDto model)
        {
            var page = this.page ?? HttpContext.Current?.CurrentHandler as Page;
            if (page != null)
            {
                ClientResourceManager.RegisterScript(
                    page,
                    model.Path,
                    model.Priority,
                    model.Provider ?? string.Empty,
                    model.JsName ?? string.Empty,
                    model.Version ?? string.Empty,
                    model.HtmlAttributes);
                return;
            }

            var controller = Globals.GetCurrentServiceProvider().GetRequiredService<IClientResourceController>();
            var script = controller.CreateScript(model.Path)
                .SetPriority(model.Priority)
                .SetNameAndVersion(model.JsName ?? string.Empty, model.Version ?? string.Empty, false);
            if (!string.IsNullOrEmpty(model.Provider))
            {
                script = script.SetProvider(model.Provider);
            }

            if (model.HtmlAttributes is not null)
            {
                foreach (var attribute in model.HtmlAttributes)
                {
                    script = script.AddAttribute(attribute.Key, attribute.Value);
                }
            }

            script.Register();
        }
    }
}

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

// ReSharper disable once CheckNamespace
namespace DotNetNuke.Web.MvcPipeline.Tokens
{
    using System;
    using System.Web.Mvc;

    using DotNetNuke.Entities.Users;
    using DotNetNuke.Framework.JavaScriptLibraries;
    using DotNetNuke.Services.Tokens;
    using DotNetNuke.Web.Client;
    using DotNetNuke.Web.Client.ClientResourceManagement;

    /// <summary>Property Access implementation for JavaScript registration in MVC context.</summary>
    public class MvcJavaScriptPropertyAccess : JsonPropertyAccess<JavaScriptDto>
    {
        private readonly ControllerContext controllerContext;

        /// <summary>Initializes a new instance of the <see cref="MvcJavaScriptPropertyAccess"/> class.</summary>
        /// <param name="controllerContext">The controller context.</param>
        public MvcJavaScriptPropertyAccess(ControllerContext controllerContext)
        {
            this.controllerContext = controllerContext;
        }

        /// <inheritdoc/>
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
                MvcClientResourceManager.RegisterScript(
                    this.controllerContext,
                    model.Path,
                    model.Priority,
                    model.Provider ?? string.Empty,
                    model.JsName ?? string.Empty,
                    model.Version ?? string.Empty,
                    model.HtmlAttributes);
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
    }
}

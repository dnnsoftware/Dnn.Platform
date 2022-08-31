// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

// ReSharper disable once CheckNamespace
namespace DotNetNuke.Services.Tokens
{
    using System;
    using System.Collections.Generic;
    using System.Web.UI;

    using DotNetNuke.Entities.Users;
    using DotNetNuke.Framework.JavaScriptLibraries;
    using DotNetNuke.Web.Client;
    using DotNetNuke.Web.Client.ClientResourceManagement;

    /// <summary>
    /// Property Access implementenation for javascript registration.
    /// </summary>
    public class JavaScriptPropertyAccess : JsonPropertyAccess<JavaScriptDto>
    {
        private readonly Page page;

        /// <summary>
        /// Initializes a new instance of the <see cref="JavaScriptPropertyAccess"/> class.
        /// </summary>
        /// <param name="page">The current page.</param>
        public JavaScriptPropertyAccess(Page page)
        {
            this.page = page;
        }

        /// <inheritdoc/>
        protected override string ProcessToken(JavaScriptDto model, UserInfo accessingUser, Scope accessLevel)
        {
            if (string.IsNullOrEmpty(model.JsName) && string.IsNullOrEmpty(model.Path))
            {
                throw new ArgumentException("If the jsname property is not specified then the JavaScript token must specify a path or property.");
            }

            if (model.Priority == 0)
            {
                model.Priority = (int)FileOrder.Js.DefaultPriority;
            }

            if (!string.IsNullOrEmpty(model.JsName) && string.IsNullOrEmpty(model.Path))
            {
                this.RegisterInstalledLibrary(model);
                return string.Empty;
            }

            this.RegisterPath(model);

            return string.Empty;
        }

        private void RegisterInstalledLibrary(JavaScriptDto model)
        {
            Version version = null;
            SpecificVersion specific = SpecificVersion.Latest;
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

        private void RegisterPath(JavaScriptDto model)
        {
            var hasName = !string.IsNullOrEmpty(model.JsName);
            var hasProvider = !string.IsNullOrEmpty(model.Provider);
            var hasPath = !string.IsNullOrEmpty(model.Path);

            var htmlAttributes = new Dictionary<string, string>();
            try
            {
                if (!string.IsNullOrEmpty(model.HtmlAttributesAsString))
                {
                    var attributes = model.HtmlAttributesAsString.Split(',');
                    foreach (var attribute in attributes)
                    {
                        var attributeParts = attribute.Split(':');
                        htmlAttributes.Add(attributeParts[0], attributeParts[1]);
                    }
                }
            }
            catch (Exception)
            {
                throw new ArgumentException("HtmlAttributesAsString is malformed.");
            }

            if (hasName && hasProvider && hasPath)
            {
                ClientResourceManager.RegisterScript(this.page, model.Path, model.Priority, model.Provider, model.JsName, model.Version, htmlAttributes);
                return;
            }

            if (hasName && hasProvider)
            {
                ClientResourceManager.RegisterScript(this.page, model.Path, model.Priority, string.Empty, model.JsName, model.Version, htmlAttributes);
                return;
            }

            if (hasProvider)
            {
                ClientResourceManager.RegisterScript(this.page, model.Path, model.Priority, model.Provider, htmlAttributes);
                return;
            }

            ClientResourceManager.RegisterScript(this.page, model.Path, model.Priority, htmlAttributes);
        }
    }
}

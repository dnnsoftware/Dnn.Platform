// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

// ReSharper disable once CheckNamespace
namespace DotNetNuke.Services.Tokens
{
    using System;
    using System.Collections.Generic;
    using System.Web.UI;

    using DotNetNuke.Abstractions.ClientResources;
    using DotNetNuke.Collections;
    using DotNetNuke.Common;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Framework.JavaScriptLibraries;
    using DotNetNuke.Web.Client.ResourceManager;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>Property Access implementation for javascript registration.</summary>
    public class JavaScriptPropertyAccess : JsonPropertyAccess<JavaScriptDto>
    {
        private readonly IClientResourceController clientResourceController;

        /// <summary>Initializes a new instance of the <see cref="JavaScriptPropertyAccess"/> class.</summary>
        /// <param name="page">The current page.</param>
        [Obsolete("Deprecated in DotNetNuke 10.0.0. Please use overload with IClientResourceController. Scheduled removal in v12.0.0.")]
        public JavaScriptPropertyAccess(Page page)
           : this((IClientResourceController)null)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="JavaScriptPropertyAccess"/> class.</summary>
        /// <param name="clientResourceController">The client resource controller.</param>
        public JavaScriptPropertyAccess(IClientResourceController clientResourceController)
        {
            this.clientResourceController = clientResourceController ?? Globals.GetCurrentServiceProvider().GetRequiredService<IClientResourceController>();
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
                var script = this.clientResourceController
                    .CreateScript(model.Path);
                if (model.Priority > 0)
                {
                    script.SetPriority(model.Priority);
                }

                if (!string.IsNullOrEmpty(model.Provider))
                {
                    script.SetProvider(model.Provider);
                }

                if (!string.IsNullOrEmpty(model.JsName))
                {
                    script.SetNameAndVersion(model.JsName, model.Version, false);
                }

                if (model.HtmlAttributes != null)
                {
                    model.HtmlAttributes.ForEach(kvp => script.AddAttribute(kvp.Key, kvp.Value));
                }

                script.Register();
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

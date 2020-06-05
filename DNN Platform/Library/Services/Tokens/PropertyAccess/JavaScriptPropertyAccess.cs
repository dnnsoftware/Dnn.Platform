﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

using System;
using System.Web.UI;
using DotNetNuke.Entities.Users;
using DotNetNuke.Framework.JavaScriptLibraries;
using DotNetNuke.Web.Client;
using DotNetNuke.Web.Client.ClientResourceManagement;
using Newtonsoft.Json;

// ReSharper disable once CheckNamespace
namespace DotNetNuke.Services.Tokens
{
    public class JavaScriptDto
    {
        [JsonProperty("jsname")]
        public string JsName { get; set; }

        [JsonProperty("path")]
        public string Path { get; set; }

        [JsonProperty("priority")]
        public int Priority { get; set; }

        [JsonProperty("provider")]
        public string Provider { get; set; }

        [JsonProperty("specific")]
        public string Specific { get; set; }

        [JsonProperty("version")]
        public string Version { get; set; }
    }

    public class JavaScriptPropertyAccess : JsonPropertyAccess<JavaScriptDto>
    {
        private readonly Page _page;

        public JavaScriptPropertyAccess(Page page)
        {
            this._page = page;
        }

        protected override string ProcessToken(JavaScriptDto model, UserInfo accessingUser, Scope accessLevel)
        {
            if (String.IsNullOrEmpty(model.JsName))
            {
                if (String.IsNullOrEmpty(model.Path))
                {
                    throw new ArgumentException("If the jsname property is not specified then the JavaScript token must specify a path or property.");
                }
                if (model.Priority == 0)
                {
                    model.Priority = (int)FileOrder.Js.DefaultPriority;
                }
                if (String.IsNullOrEmpty(model.Provider))
                {
                    ClientResourceManager.RegisterScript(this._page, model.Path, model.Priority);
                }
                else
                {
                    ClientResourceManager.RegisterScript(this._page, model.Path, model.Priority, model.Provider);
                }
            }
            else if (!String.IsNullOrEmpty(model.Path))
            {
                if (model.Priority == 0)
                {
                    model.Priority = (int)FileOrder.Js.DefaultPriority;
                }
                if (String.IsNullOrEmpty(model.Provider))
                {
                    ClientResourceManager.RegisterScript(this._page, model.Path, model.Priority, "", model.JsName, model.Version);
                }
                else
                {
                    ClientResourceManager.RegisterScript(this._page, model.Path, model.Priority, model.Provider, model.JsName, model.Version);
                }
            }
            else
            {
                Version version = null;
                SpecificVersion specific = SpecificVersion.Latest;
                if (!String.IsNullOrEmpty(model.Version))
                {
                    version = new Version(model.Version);

                    if (!String.IsNullOrEmpty(model.Specific))
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

            return String.Empty;
        }
    }
}

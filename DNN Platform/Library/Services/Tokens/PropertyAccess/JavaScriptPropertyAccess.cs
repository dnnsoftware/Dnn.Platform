#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2018
// by DotNetNuke Corporation
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation 
// the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and 
// to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions 
// of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
// TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
// CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
// DEALINGS IN THE SOFTWARE.
#endregion

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
            _page = page;
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
                    ClientResourceManager.RegisterScript(_page, model.Path, model.Priority);
                }
                else
                {
                    ClientResourceManager.RegisterScript(_page, model.Path, model.Priority, model.Provider);
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
                    ClientResourceManager.RegisterScript(_page, model.Path, model.Priority, "", model.JsName, model.Version);
                }
                else
                {
                    ClientResourceManager.RegisterScript(_page, model.Path, model.Priority, model.Provider, model.JsName, model.Version);
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

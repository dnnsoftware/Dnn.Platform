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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Dispatcher;
using System.Web.Http.Routing;
using DotNetNuke.Common;
using DotNetNuke.Services.Localization;

namespace DotNetNuke.Web.Api
{
    internal class DnnHttpControllerSelector : IHttpControllerSelector
    {
        private const string ControllerSuffix = "Controller";
        private const string ControllerKey = "controller";

        private readonly HttpConfiguration _configuration;
        private readonly Lazy<ConcurrentDictionary<string, HttpControllerDescriptor>> _descriptorCache;

        public DnnHttpControllerSelector(HttpConfiguration configuration)
        {
            Requires.NotNull("configuration", configuration);

            _configuration = configuration;
            _descriptorCache = new Lazy<ConcurrentDictionary<string, HttpControllerDescriptor>>(InitTypeCache,
                                                                                                isThreadSafe: true);
        }

        private ConcurrentDictionary<string, HttpControllerDescriptor> DescriptorCache
        {
            get { return _descriptorCache.Value; }
        }

        public HttpControllerDescriptor SelectController(HttpRequestMessage request)
        {
            Requires.NotNull("request", request);

            string controllerName = GetControllerName(request);
            IEnumerable<string> namespaces = GetNameSpaces(request);
            if (namespaces == null || !namespaces.Any() || String.IsNullOrEmpty(controllerName))
            {
                throw new HttpResponseException(request.CreateErrorResponse(HttpStatusCode.NotFound,
                                                                            "Unable to locate a controller for " +
                                                                            request.RequestUri));
            }

            var matches = new List<HttpControllerDescriptor>();
            foreach (string ns in namespaces)
            {
                string fullName = GetFullName(controllerName, ns);

                HttpControllerDescriptor descriptor;
                if (DescriptorCache.TryGetValue(fullName, out descriptor))
                {
                    matches.Add(descriptor);
                }
            }

            if(matches.Count == 1)
            {
                return matches.First();
            }

            //only errors thrown beyond this point
            if (matches.Count == 0)
            {
                throw new HttpResponseException(request.CreateErrorResponse(HttpStatusCode.NotFound, string.Format(Localization.GetString("ControllerNotFound", Localization.ExceptionsResourceFile), request.RequestUri, string.Join(", ", namespaces))));
            }

            throw new HttpResponseException(request.CreateErrorResponse(HttpStatusCode.Conflict, string.Format(Localization.GetString("AmbiguousController", Localization.ExceptionsResourceFile), controllerName, string.Join(", ", namespaces))));
        }

        public IDictionary<string, HttpControllerDescriptor> GetControllerMapping()
        {
            return DescriptorCache;
        }

        private string GetFullName(string controllerName, string ns)
        {
            return string.Format("{0}.{1}{2}", ns, controllerName, ControllerSuffix).ToLowerInvariant();
        }

        private string[] GetNameSpaces(HttpRequestMessage request)
        {
            IHttpRouteData routeData = request.GetRouteData();
            if (routeData == null)
            {
                return null;
            }

            return routeData.Route.GetNameSpaces();
        }

        private string GetControllerName(HttpRequestMessage request)
        {
            IHttpRouteData routeData = request.GetRouteData();
            if (routeData == null)
            {
                return null;
            }

            // Look up controller in route data
            object controllerName;
            routeData.Values.TryGetValue(ControllerKey, out controllerName);
            return controllerName as string;
        }

        private ConcurrentDictionary<string, HttpControllerDescriptor> InitTypeCache()
        {
            IAssembliesResolver assembliesResolver = _configuration.Services.GetAssembliesResolver();
            IHttpControllerTypeResolver controllersResolver = _configuration.Services.GetHttpControllerTypeResolver();

            ICollection<Type> controllerTypes = controllersResolver.GetControllerTypes(assembliesResolver);

            var dict = new ConcurrentDictionary<string, HttpControllerDescriptor>();

            foreach (Type type in controllerTypes)
            {
                if (type.FullName != null)
                {
                    string controllerName = type.Name.Substring(0, type.Name.Length - ControllerSuffix.Length);
                    dict.TryAdd(type.FullName.ToLowerInvariant(), new HttpControllerDescriptor(_configuration, controllerName, type));
                }
            }

            return dict;
        }
    }
}
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.Api
{
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

    internal class DnnHttpControllerSelector : IHttpControllerSelector
    {
        private const string ControllerSuffix = "Controller";
        private const string ControllerKey = "controller";

        private readonly HttpConfiguration _configuration;
        private readonly Lazy<ConcurrentDictionary<string, HttpControllerDescriptor>> _descriptorCache;

        public DnnHttpControllerSelector(HttpConfiguration configuration)
        {
            Requires.NotNull("configuration", configuration);

            this._configuration = configuration;
            this._descriptorCache = new Lazy<ConcurrentDictionary<string, HttpControllerDescriptor>>(
                this.InitTypeCache,
                isThreadSafe: true);
        }

        private ConcurrentDictionary<string, HttpControllerDescriptor> DescriptorCache
        {
            get { return this._descriptorCache.Value; }
        }

        public HttpControllerDescriptor SelectController(HttpRequestMessage request)
        {
            Requires.NotNull("request", request);

            string controllerName = this.GetControllerName(request);
            IEnumerable<string> namespaces = this.GetNameSpaces(request);
            if (namespaces == null || !namespaces.Any() || string.IsNullOrEmpty(controllerName))
            {
                throw new HttpResponseException(request.CreateErrorResponse(
                    HttpStatusCode.NotFound,
                    "Unable to locate a controller for " +
                                                                            request.RequestUri));
            }

            var matches = new List<HttpControllerDescriptor>();
            foreach (string ns in namespaces)
            {
                string fullName = this.GetFullName(controllerName, ns);

                HttpControllerDescriptor descriptor;
                if (this.DescriptorCache.TryGetValue(fullName, out descriptor))
                {
                    matches.Add(descriptor);
                }
            }

            if (matches.Count == 1)
            {
                return matches.First();
            }

            // only errors thrown beyond this point
            if (matches.Count == 0)
            {
                throw new HttpResponseException(request.CreateErrorResponse(HttpStatusCode.NotFound, string.Format(Localization.GetString("ControllerNotFound", Localization.ExceptionsResourceFile), request.RequestUri, string.Join(", ", namespaces))));
            }

            throw new HttpResponseException(request.CreateErrorResponse(HttpStatusCode.Conflict, string.Format(Localization.GetString("AmbiguousController", Localization.ExceptionsResourceFile), controllerName, string.Join(", ", namespaces))));
        }

        public IDictionary<string, HttpControllerDescriptor> GetControllerMapping()
        {
            return this.DescriptorCache;
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
            IAssembliesResolver assembliesResolver = this._configuration.Services.GetAssembliesResolver();
            IHttpControllerTypeResolver controllersResolver = this._configuration.Services.GetHttpControllerTypeResolver();

            ICollection<Type> controllerTypes = controllersResolver.GetControllerTypes(assembliesResolver);

            var dict = new ConcurrentDictionary<string, HttpControllerDescriptor>();

            foreach (Type type in controllerTypes)
            {
                if (type.FullName != null)
                {
                    string controllerName = type.Name.Substring(0, type.Name.Length - ControllerSuffix.Length);
                    dict.TryAdd(type.FullName.ToLowerInvariant(), new HttpControllerDescriptor(this._configuration, controllerName, type));
                }
            }

            return dict;
        }
    }
}

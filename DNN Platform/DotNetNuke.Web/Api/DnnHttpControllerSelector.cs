// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.Api
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Globalization;
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

        private readonly HttpConfiguration configuration;
        private readonly Lazy<ConcurrentDictionary<string, HttpControllerDescriptor>> descriptorCache;

        /// <summary>Initializes a new instance of the <see cref="DnnHttpControllerSelector"/> class.</summary>
        /// <param name="configuration">The HTTP configuration.</param>
        public DnnHttpControllerSelector(HttpConfiguration configuration)
        {
            Requires.NotNull("configuration", configuration);

            this.configuration = configuration;
            this.descriptorCache = new Lazy<ConcurrentDictionary<string, HttpControllerDescriptor>>(
                this.InitTypeCache,
                isThreadSafe: true);
        }

        private ConcurrentDictionary<string, HttpControllerDescriptor> DescriptorCache
        {
            get { return this.descriptorCache.Value; }
        }

        /// <inheritdoc/>
        public HttpControllerDescriptor SelectController(HttpRequestMessage request)
        {
            Requires.NotNull("request", request);

            string controllerName = GetControllerName(request);
            IEnumerable<string> namespaces = GetNameSpaces(request);
            if (namespaces == null || !namespaces.Any() || string.IsNullOrEmpty(controllerName))
            {
                throw new HttpResponseException(request.CreateErrorResponse(
                    HttpStatusCode.NotFound,
                    "Unable to locate a controller for " + request.RequestUri));
            }

            var matches = new List<HttpControllerDescriptor>();
            foreach (string ns in namespaces)
            {
                string fullName = GetFullName(controllerName, ns);

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
                throw new HttpResponseException(request.CreateErrorResponse(HttpStatusCode.NotFound, string.Format(CultureInfo.CurrentCulture, Localization.GetString("ControllerNotFound", Localization.ExceptionsResourceFile), request.RequestUri, string.Join(", ", namespaces))));
            }

            throw new HttpResponseException(request.CreateErrorResponse(HttpStatusCode.Conflict, string.Format(CultureInfo.CurrentCulture, Localization.GetString("AmbiguousController", Localization.ExceptionsResourceFile), controllerName, string.Join(", ", namespaces))));
        }

        /// <inheritdoc/>
        public IDictionary<string, HttpControllerDescriptor> GetControllerMapping()
        {
            return this.DescriptorCache;
        }

        private static string GetFullName(string controllerName, string ns)
        {
            return $"{ns}.{controllerName}{ControllerSuffix}".ToLowerInvariant();
        }

        private static string[] GetNameSpaces(HttpRequestMessage request)
        {
            return request.GetRouteData()?.Route.GetNameSpaces();
        }

        private static string GetControllerName(HttpRequestMessage request)
        {
            IHttpRouteData routeData = request.GetRouteData();
            if (routeData == null)
            {
                return null;
            }

            // Look up controller in route data
            routeData.Values.TryGetValue(ControllerKey, out var controllerName);
            return controllerName as string;
        }

        private ConcurrentDictionary<string, HttpControllerDescriptor> InitTypeCache()
        {
            IAssembliesResolver assembliesResolver = this.configuration.Services.GetAssembliesResolver();
            IHttpControllerTypeResolver controllersResolver = this.configuration.Services.GetHttpControllerTypeResolver();

            ICollection<Type> controllerTypes = controllersResolver.GetControllerTypes(assembliesResolver);

            var dict = new ConcurrentDictionary<string, HttpControllerDescriptor>();

            foreach (Type type in controllerTypes)
            {
                if (type.FullName != null)
                {
                    string controllerName = type.Name.Substring(0, type.Name.Length - ControllerSuffix.Length);
                    dict.TryAdd(type.FullName.ToLowerInvariant(), new HttpControllerDescriptor(this.configuration, controllerName, type));
                }
            }

            return dict;
        }
    }
}

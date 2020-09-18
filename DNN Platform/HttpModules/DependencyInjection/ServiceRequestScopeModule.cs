// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

using System;
using System.Web;

using DotNetNuke.Common.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Web.Infrastructure.DynamicModuleHelper;

[assembly: PreApplicationStartMethod(typeof(DotNetNuke.HttpModules.DependencyInjection.ServiceRequestScopeModule), nameof(DotNetNuke.HttpModules.DependencyInjection.ServiceRequestScopeModule.InitModule))]

namespace DotNetNuke.HttpModules.DependencyInjection
{
    public class ServiceRequestScopeModule : IHttpModule
    {
        private static IServiceProvider _serviceProvider;

        public static void InitModule()
        {
            DynamicModuleUtility.RegisterModule(typeof(ServiceRequestScopeModule));
        }

        public static void SetServiceProvider(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public void Init(HttpApplication context)
        {
            context.BeginRequest += this.Context_BeginRequest;
            context.EndRequest += this.Context_EndRequest;
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing,
        /// releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing,
        /// releasing, or resetting unmanaged resources.
        /// </summary>
        /// <param name="disposing">
        /// true if the object is currently disposing.
        /// </param>
        protected virtual void Dispose(bool disposing)
        {
            // left empty by design
        }

        private void Context_BeginRequest(object sender, EventArgs e)
        {
            var context = ((HttpApplication)sender).Context;
            context.SetScope(_serviceProvider.CreateScope());
        }

        private void Context_EndRequest(object sender, EventArgs e)
        {
            var context = ((HttpApplication)sender).Context;
            context.GetScope()?.Dispose();
            context.ClearScope();
        }
    }
}

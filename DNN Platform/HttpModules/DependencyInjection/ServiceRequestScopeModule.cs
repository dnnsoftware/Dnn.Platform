// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.HttpModules.DependencyInjection
{
    using System;
    using System.Threading.Tasks;
    using System.Web;

    using DotNetNuke.Common.Extensions;

    using Microsoft.Extensions.DependencyInjection;

    /// <summary>An HTTP module which creates dependency injection scopes for each request.</summary>
    public class ServiceRequestScopeModule : IHttpModule
    {
        private static IServiceProvider serviceProvider;

        /// <summary>For internal use only. Allows setting the service provider used to create request scopes.</summary>
        /// <param name="serviceProvider">The service provider.</param>
        public static void SetServiceProvider(IServiceProvider serviceProvider)
        {
            ServiceRequestScopeModule.serviceProvider = serviceProvider;
        }

        /// <inheritdoc/>
        public void Init(HttpApplication context)
        {
            context.BeginRequest += Context_BeginRequest;

            var asyncHandler = new EventHandlerTaskAsyncHelper(Context_EndRequest);
            context.AddOnEndRequestAsync(asyncHandler.BeginEventHandler, asyncHandler.EndEventHandler);
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

        private static void Context_BeginRequest(object sender, EventArgs e)
        {
            var context = ((HttpApplication)sender).Context;
            context.SetScope(serviceProvider.CreateScope());
        }

        private static async Task Context_EndRequest(object sender, EventArgs e)
        {
            var context = ((HttpApplication)sender).Context;
            switch (context.GetScope())
            {
                case IAsyncDisposable asyncDisposable:
                    await asyncDisposable.DisposeAsync();
                    break;
                case IDisposable disposable:
                    disposable.Dispose();
                    break;
            }

            context.ClearScope();
        }
    }
}

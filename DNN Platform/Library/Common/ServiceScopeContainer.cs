// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Common
{
    using System;
    using System.Web;
    using DotNetNuke.Common.Extensions;
    using Microsoft.Extensions.DependencyInjection;

    internal struct ServiceScopeContainer : IDisposable
    {
        public ServiceScopeContainer(IServiceScope serviceScope, bool shouldDispose)
        {
            ServiceScope = serviceScope;
            ShouldDispose = shouldDispose;
        }

        /// <summary>
        ///     Service scope that the container holds.
        /// </summary>
        public IServiceScope ServiceScope { get; }

        /// <summary>
        ///     True if the <see cref="ServiceScope"/> should be disposed.
        /// </summary>
        public bool ShouldDispose { get; }

        /// <summary>
        ///     Get the service scope from <see cref="HttpContext"/> or create a new scope from <see cref="Globals.DependencyProvider"/> when the scope doesn't exists.
        /// </summary>
        public static ServiceScopeContainer GetRequestOrCreateScope()
        {
            var requestScope = HttpContextSource.Current?.GetScope();

            if (requestScope != null)
            {
                return new ServiceScopeContainer(requestScope, false);
            }

            return new ServiceScopeContainer(Globals.DependencyProvider.CreateScope(), true);
        }

        /// <inheritdoc />
        public void Dispose()
        {
            if (ShouldDispose)
            {
                ServiceScope.Dispose();
            }
        }
    }
}

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Web.MvcPipeline.Framework
{
    using System;

    using DotNetNuke.Framework;

    /// <summary>
    /// Service locator wrapper for resolving MVC services framework internal implementation.
    /// </summary>
    internal class MvcServicesFrameworkInternal : ServiceLocator<IMvcServiceFrameworkInternals, MvcServicesFrameworkInternal>
    {
        /// <inheritdoc/>
        protected override Func<IMvcServiceFrameworkInternals> GetFactory()
        {
            return () => new MvcServicesFrameworkImpl();
        }
    }
}

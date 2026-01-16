// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Framework
{
    using System;

    using DotNetNuke.Common;

    using Microsoft.Extensions.DependencyInjection;

    internal class ServicesFrameworkInternal : ServiceLocator<IServiceFrameworkInternals, ServicesFrameworkInternal>
    {
        /// <inheritdoc/>
        protected override Func<IServiceFrameworkInternals> GetFactory()
        {
            return static () => ActivatorUtilities.GetServiceOrCreateInstance<ServicesFrameworkImpl>(Globals.DependencyProvider);
        }
    }
}

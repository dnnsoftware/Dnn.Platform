// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
using System;

namespace DotNetNuke.Framework
{
    internal class ServicesFrameworkInternal : ServiceLocator<IServiceFrameworkInternals, ServicesFrameworkInternal>
    {
        protected override Func<IServiceFrameworkInternals> GetFactory()
        {
            return () => new ServicesFrameworkImpl();
        }
    }
}

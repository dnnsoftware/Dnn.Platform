// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
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

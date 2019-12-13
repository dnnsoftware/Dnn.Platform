// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using System;
using System.ComponentModel;

using DotNetNuke.Framework;

namespace DotNetNuke.Entities.Portals.Internal
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Obsolete("This class has been obsoleted in 7.3.0 - please use PortalController instead. Scheduled removal in v10.0.0.")]
    public class TestablePortalController : ServiceLocator<IPortalController, TestablePortalController>
    {
        protected override Func<IPortalController> GetFactory()
        {
            return () => new PortalController();
        }
    }
}

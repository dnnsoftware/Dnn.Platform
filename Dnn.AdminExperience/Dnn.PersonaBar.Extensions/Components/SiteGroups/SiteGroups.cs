// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 

using DotNetNuke.Framework;

namespace Dnn.PersonaBar.SiteGroups
{
    public class SiteGroups : ServiceLocator<IManagePortalGroups, SiteGroups>
    {
        protected override System.Func<IManagePortalGroups> GetFactory()
        {
            return () => new PortalGroupAdapter();
        }
    }
}
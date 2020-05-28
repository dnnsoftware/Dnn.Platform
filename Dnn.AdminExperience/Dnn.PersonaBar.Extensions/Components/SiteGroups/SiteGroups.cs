// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

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
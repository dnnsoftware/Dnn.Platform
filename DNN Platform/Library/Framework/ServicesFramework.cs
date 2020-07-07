// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Framework
{
    using System;

    using DotNetNuke.ComponentModel;
    using DotNetNuke.Entities.Portals;

    /// <summary>
    /// Enables modules to support Services Framework features.
    /// </summary>
    public class ServicesFramework : ServiceLocator<IServicesFramework, ServicesFramework>
    {
        public static string GetServiceFrameworkRoot()
        {
            var portalSettings = PortalSettings.Current;
            if (portalSettings == null)
            {
                return string.Empty;
            }

            var path = portalSettings.PortalAlias.HTTPAlias;
            var index = path.IndexOf('/');
            if (index > 0)
            {
                path = path.Substring(index);
                if (!path.EndsWith("/"))
                {
                    path += "/";
                }
            }
            else
            {
                path = "/";
            }

            return path;
        }

        protected override Func<IServicesFramework> GetFactory()
        {
            return () => new ServicesFrameworkImpl();
        }
    }
}

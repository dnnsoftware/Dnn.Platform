// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Web.MvcPipeline.Framework
{
    using System;

    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Framework;

    /// <summary>Enables modules to support Services Framework features.</summary>
    public class MvcServicesFramework : ServiceLocator<IMvcServicesFramework, MvcServicesFramework>
    {
        /// <summary>
        /// Gets the services framework root path for the current portal alias.
        /// </summary>
        /// <returns>The root path (including application path if present).</returns>
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

        /// <inheritdoc/>
        protected override Func<IMvcServicesFramework> GetFactory()
        {
            return () => new MvcServicesFrameworkImpl();
        }
    }
}

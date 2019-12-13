﻿// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using System;
using DotNetNuke.ComponentModel;
using DotNetNuke.Entities.Portals;

namespace DotNetNuke.Framework
{
    /// <summary>
    /// Enables modules to support Services Framework features
    /// </summary>
    public class ServicesFramework : ServiceLocator<IServicesFramework, ServicesFramework>
    {
        protected override Func<IServicesFramework> GetFactory()
        {
            return () => new ServicesFrameworkImpl();
        }

        public static string GetServiceFrameworkRoot()
        {
            var portalSettings = PortalSettings.Current;
            if (portalSettings == null) return String.Empty;
            var path = portalSettings.PortalAlias.HTTPAlias;
            var index = path.IndexOf('/');
            if (index > 0)
            {
                path = path.Substring(index);
                if (!path.EndsWith("/")) path += "/";
            }
            else
                path = "/";

            return path;
        }
    }
}

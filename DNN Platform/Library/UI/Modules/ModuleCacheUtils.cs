// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.UI.Modules
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Threading;
    using System.Threading.Tasks;

    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Framework.JavaScriptLibraries;
    using DotNetNuke.Services.Exceptions;
    using DotNetNuke.Services.ModuleCache;
    using DotNetNuke.Web.Client.ClientResourceManagement;

    internal class ModuleCacheUtils
    {
        /// <summary>Trys to load previously cached Module Content.</summary>
        /// <param name="moduleConfiguration">The module info.</param>
        /// <param name="cachedContent">The cached content.</param>
        /// <returns>A Boolean that indicates whether the cahed content was loaded.</returns>
        internal static bool TryLoadCached(ModuleInfo moduleConfiguration, out string cachedContent)
        {
            bool success = false;
            cachedContent = string.Empty;
            try
            {
                var cache = ModuleCachingProvider.Instance(moduleConfiguration.GetEffectiveCacheMethod());
                var varyBy = new SortedDictionary<string, string> { { "locale", Thread.CurrentThread.CurrentUICulture.ToString() } };

                string cacheKey = cache.GenerateCacheKey(moduleConfiguration.TabModuleID, varyBy);
                byte[] cachedBytes = ModuleCachingProvider.Instance(moduleConfiguration.GetEffectiveCacheMethod()).GetModule(moduleConfiguration.TabModuleID, cacheKey);

                if (cachedBytes != null && cachedBytes.Length > 0)
                {
                    cachedContent = Encoding.UTF8.GetString(cachedBytes);
                    success = true;
                }
            }
            catch (Exception ex)
            {
                cachedContent = string.Empty;
                Exceptions.LogException(ex);
                success = false;
            }

            return success;
        }

        /// <summary>Gets a flag that indicates whether the Module Instance supports Caching.</summary>
        /// <param name="moduleConfiguration">The module info.</param>
        /// <returns>A Boolean.</returns>
        internal static bool SupportsCaching(ModuleInfo moduleConfiguration)
        {
            return moduleConfiguration.CacheTime > 0;
        }
    }
}

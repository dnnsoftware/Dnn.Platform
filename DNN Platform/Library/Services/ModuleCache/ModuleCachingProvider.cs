// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.ModuleCache
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Text;

    using DotNetNuke.ComponentModel;

    public abstract class ModuleCachingProvider
    {
        public static Dictionary<string, ModuleCachingProvider> GetProviderList()
        {
            return ComponentFactory.GetComponents<ModuleCachingProvider>();
        }

        public static ModuleCachingProvider Instance(string friendlyName)
        {
            return ComponentFactory.GetComponent<ModuleCachingProvider>(friendlyName);
        }

        public static void RemoveItemFromAllProviders(int tabModuleId)
        {
            foreach (KeyValuePair<string, ModuleCachingProvider> kvp in GetProviderList())
            {
                kvp.Value.Remove(tabModuleId);
            }
        }

        public abstract string GenerateCacheKey(int tabModuleId, SortedDictionary<string, string> varyBy);

        public abstract int GetItemCount(int tabModuleId);

        public abstract byte[] GetModule(int tabModuleId, string cacheKey);

        public abstract void Remove(int tabModuleId);

        public abstract void SetModule(int tabModuleId, string cacheKey, TimeSpan duration, byte[] moduleOutput);

        public virtual void PurgeCache(int portalId)
        {
        }

        public virtual void PurgeExpiredItems(int portalId)
        {
        }

        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Breaking change")]
        protected string ByteArrayToString(byte[] arrInput)
        {
            int i;
            var sOutput = new StringBuilder(arrInput.Length);
            for (i = 0; i <= arrInput.Length - 1; i++)
            {
                sOutput.Append(arrInput[i].ToString("X2", CultureInfo.InvariantCulture));
            }

            return sOutput.ToString();
        }
    }
}

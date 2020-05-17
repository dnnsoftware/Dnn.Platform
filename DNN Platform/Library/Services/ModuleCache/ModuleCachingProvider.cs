﻿// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
#region Usings

using System;
using System.Collections.Generic;
using System.Text;

using DotNetNuke.ComponentModel;

#endregion

namespace DotNetNuke.Services.ModuleCache
{
    public abstract class ModuleCachingProvider
    {
        public static Dictionary<string, ModuleCachingProvider> GetProviderList()
        {
            return ComponentFactory.GetComponents<ModuleCachingProvider>();
        }

        public static ModuleCachingProvider Instance(string FriendlyName)
        {
            return ComponentFactory.GetComponent<ModuleCachingProvider>(FriendlyName);
        }

        public static void RemoveItemFromAllProviders(int tabModuleId)
        {
            foreach (KeyValuePair<string, ModuleCachingProvider> kvp in GetProviderList())
            {
                kvp.Value.Remove(tabModuleId);
            }
        }

        protected string ByteArrayToString(byte[] arrInput)
        {
            int i;
            var sOutput = new StringBuilder(arrInput.Length);
            for (i = 0; i <= arrInput.Length - 1; i++)
            {
                sOutput.Append(arrInput[i].ToString("X2"));
            }
            return sOutput.ToString();
        }

        #region Abstract methods

        public abstract string GenerateCacheKey(int tabModuleId, SortedDictionary<string, string> varyBy);

        public abstract int GetItemCount(int tabModuleId);

        public abstract byte[] GetModule(int tabModuleId, string cacheKey);

        public abstract void Remove(int tabModuleId);

        public abstract void SetModule(int tabModuleId, string cacheKey, TimeSpan duration, byte[] moduleOutput);

        #endregion

        #region Virtual Methods

        public virtual void PurgeCache(int portalId)
        {
        }

        public virtual void PurgeExpiredItems(int portalId)
        {
        }

        #endregion
    }
}

#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2017
// by DotNetNuke Corporation
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation 
// the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and 
// to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions 
// of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
// TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
// CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
// DEALINGS IN THE SOFTWARE.
#endregion
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
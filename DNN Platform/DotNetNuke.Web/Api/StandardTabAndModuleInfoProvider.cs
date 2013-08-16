using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Modules.Internal;

namespace DotNetNuke.Web.Api
{
    public sealed class StandardTabAndModuleInfoProvider : ITabAndModuleInfoProvider
    {
        private const string ModuleIdKey = "ModuleId";
        private const string TabIdKey = "TabId";

        public bool TryFindTabId(HttpRequestMessage request, out int tabId)
        {
            tabId = FindInt(request, TabIdKey);
            return tabId > Null.NullInteger;
        }

        public bool TryFindModuleId(HttpRequestMessage request, out int moduleId)
        {
            moduleId = FindInt(request, ModuleIdKey);
            return moduleId > Null.NullInteger;
        }

        public bool TryFindModuleInfo(HttpRequestMessage request, out ModuleInfo moduleInfo)
        {
            moduleInfo = null;

            int tabId, moduleId;
            if(TryFindTabId(request, out tabId) && TryFindModuleId(request, out moduleId))
            {
                moduleInfo = TestableModuleController.Instance.GetModule(moduleId, tabId);
            }

            return moduleInfo != null;
        }

        private static int FindInt(HttpRequestMessage requestMessage, string key)
        {
            IEnumerable<string> values;
            string value = null;
            if (requestMessage.Headers.TryGetValues(key, out values))
            {
                value = values.FirstOrDefault();
            }

            if (String.IsNullOrEmpty(value) && requestMessage.RequestUri != null)
            {
                var queryString = HttpUtility.ParseQueryString(requestMessage.RequestUri.Query);
                value = queryString[key];
            }

            int id;
            if (Int32.TryParse(value, out id))
            {
                return id;
            }

            return Null.NullInteger;
        }

    }
}
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Instrumentation;

namespace DotNetNuke.Web.Api
{
    public sealed class StandardTabAndModuleInfoProvider : ITabAndModuleInfoProvider
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(StandardTabAndModuleInfoProvider));

        private const string ModuleIdKey = "ModuleId";
        private const string TabIdKey = "TabId";
        private const string MonikerQueryKey = "Moniker";
        private const string MonikerHeaderKey = "X-DNN-MONIKER";
        private const string MonikerSettingsKey = "Moniker";

        public bool TryFindTabId(HttpRequestMessage request, out int tabId)
        {
            return TryFindTabId(request, out tabId, true);
        }

        private static bool TryFindTabId(HttpRequestMessage request, out int tabId, bool tryMoniker)
        {
            tabId = FindInt(request, TabIdKey);
            if (tabId > Null.NullInteger)
            {
                return true;
            }

            if (tryMoniker)
            {
                ModuleInfo moduleInfo;
                if (TryFindByMoniker(request, out moduleInfo))
                {
                    tabId = moduleInfo.TabID;
                    return true;
                }
            }

            return false;
        }

        public bool TryFindModuleId(HttpRequestMessage request, out int moduleId)
        {
            return TryFindModuleId(request, out moduleId, true);
        }

        private static bool TryFindModuleId(HttpRequestMessage request, out int moduleId, bool tryMoniker)
        {
            moduleId = FindInt(request, ModuleIdKey);
            if (moduleId > Null.NullInteger)
            {
                return true;
            }

            if (tryMoniker)
            {
                ModuleInfo moduleInfo;
                if (TryFindByMoniker(request, out moduleInfo))
                {
                    moduleId = moduleInfo.ModuleID;
                    return true;
                }
            }

            return false;
        }

        public bool TryFindModuleInfo(HttpRequestMessage request, out ModuleInfo moduleInfo)
        {
            int tabId, moduleId;
            if (TryFindTabId(request, out tabId, false) && TryFindModuleId(request, out moduleId, false))
            {
                moduleInfo = ModuleController.Instance.GetModule(moduleId, tabId, false);
                return moduleInfo != null;
            }

            return TryFindByMoniker(request, out moduleInfo);
        }

        private static int FindInt(HttpRequestMessage requestMessage, string key)
        {
            string value = null;
            IEnumerable<string> values;
            if (requestMessage != null && requestMessage.Headers.TryGetValues(key, out values))
            {
                value = values.FirstOrDefault();
            }

            if (string.IsNullOrEmpty(value) && requestMessage?.RequestUri != null)
            {
                var queryString = HttpUtility.ParseQueryString(requestMessage.RequestUri.Query);
                value = queryString[key];
            }

            int id;
            return int.TryParse(value, out id) ? id : Null.NullInteger;
        }

        private static bool TryFindByMoniker(HttpRequestMessage requestMessage, out ModuleInfo moduleInfo)
        {
            var id = FindIntInHeader(requestMessage, MonikerHeaderKey);
            if (id <= Null.NullInteger)
            {
                id = FindIntInQueryString(requestMessage, MonikerQueryKey);
            }

            moduleInfo = id > Null.NullInteger ? ModuleController.Instance.GetTabModule(id) : null;
            return moduleInfo != null;
        }

        private static int FindIntInHeader(HttpRequestMessage requestMessage, string key)
        {
            IEnumerable<string> values;
            string value = null;
            if (requestMessage != null && requestMessage.Headers.TryGetValues(key, out values))
            {
                value = values.FirstOrDefault();
            }

            return GetTabModuleInfoFromMoniker(value);
        }

        private static int FindIntInQueryString(HttpRequestMessage requestMessage, string key)
        {
            string value = null;
            if (requestMessage.RequestUri != null)
            {
                var queryString = HttpUtility.ParseQueryString(requestMessage.RequestUri.Query);
                value = queryString[key];
            }

            return GetTabModuleInfoFromMoniker(value);
        }

        private static int GetTabModuleInfoFromMoniker(string monikerValue)
        {
            monikerValue = (monikerValue ?? "").Trim();
            if (monikerValue.Length > 0)
            {
                var ids = TabModulesController.Instance.GetTabModuleIdsBySetting(MonikerSettingsKey, monikerValue);
                if (ids != null && ids.Any())
                {
                    return ids.First();
                }
            }

            if (Logger.IsWarnEnabled)
            {
                Logger.WarnFormat("The specified moniker ({0}) is not defined in the system", monikerValue);
            }

            return Null.NullInteger;
        }
    }
}
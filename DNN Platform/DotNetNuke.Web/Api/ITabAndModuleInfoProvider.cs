using System.Net.Http;
using DotNetNuke.Entities.Modules;

namespace DotNetNuke.Web.Api
{
    public interface ITabAndModuleInfoProvider
    {
        bool TryFindTabId(HttpRequestMessage request, out int tabId);
        bool TryFindModuleId(HttpRequestMessage request, out int moduleId);
        bool TryFindModuleInfo(HttpRequestMessage request, out ModuleInfo moduleInfo);
    }
}
using System.Web;
using System.Web.UI;
using DotNetNuke.Application;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Framework;
using DotNetNuke.Framework.JavaScriptLibraries;
using DotNetNuke.UI.Skins.EventListeners;
using DotNetNuke.Web.Client.ClientResourceManagement;
using Newtonsoft.Json;

namespace Dnn.EditBar.AddModule.HttpModules
{
    public class HttpModule : IHttpModule
    {
        private static readonly object LockAppStarted = new object();
        private static bool _hasAppStarted = false;

        public void Init(HttpApplication application)
        {
            if (_hasAppStarted)
            {
                return;
            }
            lock (LockAppStarted)
            {
                if (_hasAppStarted)
                {
                    return;
                }

                ApplicationStart();
                _hasAppStarted = true;
            }
        }

        private void ApplicationStart()
        {
        }

        public void Dispose()
        {
        }
    }
}

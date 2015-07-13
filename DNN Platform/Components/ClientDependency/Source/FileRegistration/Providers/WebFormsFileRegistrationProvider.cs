using System;
using System.Collections.Generic;
using System.Text;
using System.Web.UI;
using System.Configuration.Provider;
using System.Web;
using System.Linq;
using ClientDependency.Core.Controls;
using ClientDependency.Core.Config;

namespace ClientDependency.Core.FileRegistration.Providers
{
    public abstract class WebFormsFileRegistrationProvider : BaseFileRegistrationProvider
    {
        /// <summary>
        /// Called to register the js and css into the page/control/output.
        /// </summary>
        /// <param name="http"></param>
        /// <param name="js"></param>
        /// <param name="css"></param>
        protected abstract void RegisterDependencies(HttpContextBase http, string js, string css);

        /// <summary>
        /// Called to register the dependencies into the page/control/output
        /// </summary>
        /// <param name="dependantControl"></param>
        /// <param name="allDependencies"></param>
        /// <param name="paths"></param>
        /// <param name="http"></param>
        public void RegisterDependencies(
            Control dependantControl, 
            List<IClientDependencyFile> allDependencies, 
            HashSet<IClientDependencyPath> paths, 
            HttpContextBase http)
        {
            string js;
            string css;
            WriteDependencies(allDependencies, paths, out js, out css, http);
            RegisterDependencies(http, js, css);
        }
    }
}

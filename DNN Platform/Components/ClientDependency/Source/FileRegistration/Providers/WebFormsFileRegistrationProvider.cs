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
            var ctl = dependantControl;

            var folderPaths = paths;

            UpdateFilePaths(allDependencies, folderPaths, http);
            EnsureNoDuplicates(allDependencies, folderPaths);

			var cssBuilder = new StringBuilder();
			var jsBuilder = new StringBuilder();

			// find the groups
			var groups = allDependencies
				.Select(x => x.Group)
				.Distinct()
				.ToList();

			// sort them
			groups.Sort((a, b) => a.CompareTo(b));

			foreach (var group in groups)
			{
                var g = group;
				// get the js and css dependencies for this group
				var jsDependencies = allDependencies
					.Where(x => x.Group == g && x.DependencyType == ClientDependencyType.Javascript)
					.ToList();

				var cssDependencies = allDependencies
					.Where(x => x.Group == g && x.DependencyType == ClientDependencyType.Css)
					.ToList();

				// sort according to priority
				jsDependencies.Sort((a, b) => a.Priority.CompareTo(b.Priority));
				cssDependencies.Sort((a, b) => a.Priority.CompareTo(b.Priority));

                //render
                WriteStaggeredDependencies(cssDependencies, http, cssBuilder, RenderCssDependencies, RenderSingleCssFile);
                WriteStaggeredDependencies(jsDependencies, http, jsBuilder, RenderJsDependencies, RenderSingleJsFile);
			}

			var cssOutput = cssBuilder.ToString();
			var jsOutput = jsBuilder.ToString();
            RegisterDependencies(http, jsOutput, cssOutput);
        }
    }
}

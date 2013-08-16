using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration.Provider;
using ClientDependency.Core.Config;
using ClientDependency.Core.FileRegistration.Providers;
using System.IO;
using System.Web;

namespace ClientDependency.Core.FileRegistration.Providers
{
    public abstract class BaseRenderer : BaseFileRegistrationProvider
    {


        public virtual void RegisterDependencies(List<IClientDependencyFile> allDependencies, 
            HashSet<IClientDependencyPath> paths,
            out string jsOutput,
            out string cssOutput,
            HttpContextBase http)
        {            
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
                var jsDependencies = allDependencies
                    .Where(x => x.Group == g && x.DependencyType == ClientDependencyType.Javascript)
                    .ToList();

                var cssDependencies = allDependencies
                    .Where(x => x.Group == g && x.DependencyType == ClientDependencyType.Css)
                    .ToList();

                // sort by priority
                jsDependencies.Sort((a, b) => a.Priority.CompareTo(b.Priority));
                cssDependencies.Sort((a, b) => a.Priority.CompareTo(b.Priority));

                //render
                WriteStaggeredDependencies(cssDependencies, http, cssBuilder, RenderCssDependencies, RenderSingleCssFile);
                WriteStaggeredDependencies(jsDependencies, http, jsBuilder, RenderJsDependencies, RenderSingleJsFile);                
                
            }
            cssOutput = cssBuilder.ToString();
            jsOutput = jsBuilder.ToString();
        }
    }
}


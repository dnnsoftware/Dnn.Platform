// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.Client.ResourceManager
{
    using System.Collections.Generic;
    using System.Linq;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Abstractions.ClientResources;

    /// <inheritdoc />
    public class ClientResourceController : IClientResourceController
    {
        private readonly IHostSettings hostSettings;

        /// <summary>
        /// Initializes a new instance of the <see cref="ClientResourceController"/> class.
        /// </summary>
        /// <param name="hostSettings">The host settings.</param>
        public ClientResourceController(IHostSettings hostSettings)
        {
            this.hostSettings = hostSettings;
            this.RegisterPathNameAlias("SharedScripts", "~/Resources/Shared/Scripts/");
        }

        private List<IFontResource> Fonts { get; set; } = new List<IFontResource>();

        private List<IScriptResource> Scripts { get; set; } = new List<IScriptResource>();

        private List<IStylesheetResource> Stylesheets { get; set; } = new List<IStylesheetResource>();

        private Dictionary<string, string> PathNameAliases { get; set; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        private List<string> FontsToExclude { get; set; } = new List<string>();

        private List<string> ScriptsToExclude { get; set; } = new List<string>();

        private List<string> StylesheetsToExclude { get; set; } = new List<string>();

        /// <inheritdoc />
        public void AddFont(IFontResource font)
        {
            this.Fonts = this.AddResource(this.Fonts, font);
        }

        /// <inheritdoc />
        public void AddScript(IScriptResource script)
        {
            this.Scripts = this.AddResource(this.Scripts, script);
        }

        /// <inheritdoc />
        public void AddStylesheet(IStylesheetResource stylesheet)
        {
            this.Stylesheets = this.AddResource(this.Stylesheets, stylesheet);
        }

        /// <inheritdoc />
        public IFontResource CreateFont()
        {
            return new Models.FontResource(this);
        }

        /// <inheritdoc />
        public IScriptResource CreateScript()
        {
            return new Models.ScriptResource(this);
        }

        /// <inheritdoc />
        public IStylesheetResource CreateStylesheet()
        {
            return new Models.StylesheetResource(this);
        }

        /// <inheritdoc />
        public void RegisterPathNameAlias(string pathNameAlias, string resolvedPath)
        {
            this.PathNameAliases[pathNameAlias.ToLowerInvariant()] = resolvedPath;
        }

        /// <inheritdoc />
        public void RemoveFontByName(string fontName)
        {
            this.FontsToExclude.Add(fontName.ToLowerInvariant());
        }

        /// <inheritdoc />
        public void RemoveFontByPath(string fontPath, string pathNameAlias)
        {
            var fullPath = this.ResolvePath(fontPath, pathNameAlias).ToLowerInvariant();
            this.FontsToExclude.Add(fullPath.ToLowerInvariant());
        }

        /// <inheritdoc />
        public void RemoveScriptByName(string scriptName)
        {
            this.ScriptsToExclude.Add(scriptName.ToLowerInvariant());
        }

        /// <inheritdoc />
        public void RemoveScriptByPath(string scriptPath, string pathNameAlias)
        {
            var fullPath = this.ResolvePath(scriptPath, pathNameAlias).ToLowerInvariant();
            this.ScriptsToExclude.Add(fullPath.ToLowerInvariant());
        }

        /// <inheritdoc />
        public void RemoveStylesheetByName(string stylesheetName)
        {
            this.StylesheetsToExclude.Add(stylesheetName.ToLowerInvariant());
        }

        /// <inheritdoc />
        public void RemoveStylesheetByPath(string stylesheetPath, string pathNameAlias)
        {
            var fullPath = this.ResolvePath(stylesheetPath, pathNameAlias).ToLowerInvariant();
            this.StylesheetsToExclude.Add(fullPath.ToLowerInvariant());
        }

        /// <inheritdoc />
        public string RenderDependencies(ResourceType resourceType, string provider, string applicationPath)
        {
            var sortedList = new List<string>();
            if (resourceType == ResourceType.Font || resourceType == ResourceType.All)
            {
                foreach (var link in this.Fonts.Where(s => (s.Provider == provider || (s.Provider == string.Empty && provider == ClientResourceProviders.DefaultCssProvider)) && !this.FontsToExclude.Contains(s.Name.ToLowerInvariant())).OrderBy(l => l.Priority))
                {
                    sortedList.Add(link.Render(this.hostSettings.CrmVersion, this.hostSettings.CdnEnabled, applicationPath));
                }
            }

            if (resourceType == ResourceType.Stylesheet || resourceType == ResourceType.All)
            {
                foreach (var link in this.Stylesheets.Where(s => (s.Provider == provider || (s.Provider == string.Empty && provider == ClientResourceProviders.DefaultCssProvider)) && !this.StylesheetsToExclude.Contains(s.Name.ToLowerInvariant())).OrderBy(l => l.Priority))
                {
                    sortedList.Add(link.Render(this.hostSettings.CrmVersion, this.hostSettings.CdnEnabled, applicationPath));
                }
            }

            if (resourceType == ResourceType.Script || resourceType == ResourceType.All)
            {
                foreach (var script in this.Scripts.Where(s => (s.Provider == provider || (s.Provider == string.Empty && provider == ClientResourceProviders.DefaultJsProvider)) && !this.ScriptsToExclude.Contains(s.Name.ToLowerInvariant())).OrderBy(s => s.Priority))
                {
                    sortedList.Add(script.Render(this.hostSettings.CrmVersion, this.hostSettings.CdnEnabled, applicationPath));
                }
            }

            return string.Join(string.Empty, sortedList);
        }

        private List<T> AddResource<T>(List<T> resources, T resource)
            where T : IResource
        {
            resource.ResolvedPath = this.ResolvePath(resource.FilePath, resource.PathNameAlias);
            resource.Key = resource.ResolvedPath.ToLowerInvariant();
            resources.RemoveAll(l => l.Key == resource.Key); // remove any existing link with the same key (i.e. exactly the same resolved path)
            if (!string.IsNullOrEmpty(resource.Name))
            {
                // if a resource with the same name and force version is already present we ignore this one
                if (resources.Exists(l => l.Name.ToLowerInvariant() == resource.Name.ToLowerInvariant() && l.ForceVersion))
                {
                    return resources;
                }

                // If we are forcing the version, we need to remove any existing link with the same name
                if (resource.ForceVersion)
                {
                    resources.RemoveAll(l => l.Name.ToLowerInvariant() == resource.Name.ToLowerInvariant());
                }

                // If we have a version, we need to remove any existing link with the same name and a lower version
                if (!string.IsNullOrEmpty(resource.Version))
                {
                    resources.RemoveAll(l => l.Name.ToLowerInvariant() == resource.Name.ToLowerInvariant() && string.Compare(l.Version, resource.Version, System.StringComparison.InvariantCultureIgnoreCase) < 0);

                    // If we have an existing link with the same name and a higher version, we do not add this link
                    if (resources.Exists(l => l.Name.ToLowerInvariant() == resource.Name.ToLowerInvariant() && string.Compare(l.Version, resource.Version, System.StringComparison.InvariantCultureIgnoreCase) >= 0))
                    {
                        return resources;
                    }
                }
            }

            resources.Add(resource);
            return resources;
        }

        private string ResolvePath(string filePath, string pathNameAlias)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                return filePath;
            }

            if (filePath.ToLowerInvariant().StartsWith("http"))
            {
                // Path is already fully qualified
                return filePath;
            }

            // Path is either a relative path including the application path or a path starting with a tilde or a path relative to the path name alias
            filePath = filePath.Replace("\\", "/");
            if (!string.IsNullOrEmpty(pathNameAlias))
            {
                if (this.PathNameAliases.TryGetValue(pathNameAlias.ToLowerInvariant(), out var alias))
                {
                    return $"{alias}/{filePath.TrimStart('/')}";
                }
            }

            return filePath;
        }
    }
}

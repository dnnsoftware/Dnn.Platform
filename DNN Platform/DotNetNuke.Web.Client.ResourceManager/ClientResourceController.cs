// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.Client.ResourceManager
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Abstractions.ClientResources;

    /// <inheritdoc />
    public class ClientResourceController : IClientResourceController
    {
        private readonly IHostSettings hostSettings;

        /// <summary>Initializes a new instance of the <see cref="ClientResourceController"/> class.</summary>
        /// <param name="hostSettings">The host settings.</param>
        public ClientResourceController(IHostSettings hostSettings)
        {
            this.hostSettings = hostSettings;
            this.RegisterPathNameAlias("SharedScripts", "~/Resources/Shared/Scripts/");
        }

        private List<IFontResource> Fonts { get; set; } = [];

        private List<IScriptResource> Scripts { get; set; } = [];

        private List<IStylesheetResource> Stylesheets { get; set; } = [];

        private Dictionary<string, string> PathNameAliases { get; set; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        private HashSet<string> FontsToExclude { get; set; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        private HashSet<string> ScriptsToExclude { get; set; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        private HashSet<string> StylesheetsToExclude { get; set; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

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
        public IFontResource CreateFont(string sourcePath)
        {
            var font = new Models.FontResource(this);
            font.FilePath = sourcePath;

            switch (font.FilePath?.Substring(font.FilePath.LastIndexOf('.')).ToLowerInvariant())
            {
                case ".eot":
                    font.Type = "application/vnd.ms-fontobject";
                    break;
                case ".woff":
                    font.Type = "font/woff";
                    break;
                case ".woff2":
                    font.Type = "font/woff2";
                    break;
                case ".ttf":
                    font.Type = "font/ttf";
                    break;
                case ".svg":
                    font.Type = "image/svg+xml";
                    break;
                case ".otf":
                    font.Type = "font/otf";
                    break;
                default:
                    font.Type = "application/octet-stream";
                    break;
            }

            return font;
        }

        /// <inheritdoc />
        public IFontResource CreateFont(string sourcePath, string pathNameAlias)
        {
            var font = this.CreateFont(sourcePath);
            font.PathNameAlias = pathNameAlias;
            return font;
        }

        /// <inheritdoc />
        public IFontResource CreateFont(string sourcePath, string pathNameAlias, string mimeType)
        {
            var font = this.CreateFont(sourcePath, pathNameAlias);
            font.Type = mimeType;
            return font;
        }

        /// <inheritdoc />
        public IScriptResource CreateScript(string sourcePath)
        {
            var script = new Models.ScriptResource(this);
            script.FilePath = sourcePath;
            return script;
        }

        /// <inheritdoc />
        public IScriptResource CreateScript(string sourcePath, string pathNameAlias)
        {
            var script = this.CreateScript(sourcePath);
            script.PathNameAlias = pathNameAlias;
            return script;
        }

        /// <inheritdoc />
        public IStylesheetResource CreateStylesheet(string sourcePath)
        {
            var stylesheet = new Models.StylesheetResource(this);
            stylesheet.FilePath = sourcePath;
            return stylesheet;
        }

        /// <inheritdoc />
        public IStylesheetResource CreateStylesheet(string sourcePath, string pathNameAlias)
        {
            var stylesheet = this.CreateStylesheet(sourcePath);
            stylesheet.PathNameAlias = pathNameAlias;
            return stylesheet;
        }

        /// <inheritdoc />
        public void RegisterPathNameAlias(string pathNameAlias, string resolvedPath)
        {
            this.PathNameAliases[pathNameAlias] = resolvedPath;
        }

        /// <inheritdoc />
        public void RemoveFontByName(string fontName)
        {
            this.FontsToExclude.Add(fontName);
        }

        /// <inheritdoc />
        public void RemoveFontByPath(string fontPath, string pathNameAlias)
        {
            var fullPath = this.ResolvePath(fontPath, pathNameAlias);
            this.FontsToExclude.Add(fullPath);
        }

        /// <inheritdoc />
        public void RemoveScriptByName(string scriptName)
        {
            this.ScriptsToExclude.Add(scriptName);
        }

        /// <inheritdoc />
        public void RemoveScriptByPath(string scriptPath, string pathNameAlias)
        {
            var fullPath = this.ResolvePath(scriptPath, pathNameAlias);
            this.ScriptsToExclude.Add(fullPath);
        }

        /// <inheritdoc />
        public void RemoveStylesheetByName(string stylesheetName)
        {
            this.StylesheetsToExclude.Add(stylesheetName);
        }

        /// <inheritdoc />
        public void RemoveStylesheetByPath(string stylesheetPath, string pathNameAlias)
        {
            var fullPath = this.ResolvePath(stylesheetPath, pathNameAlias);
            this.StylesheetsToExclude.Add(fullPath);
        }

        /// <inheritdoc />
        public string RenderDependencies(ResourceType resourceType, string provider, string applicationPath)
        {
            var sortedList = new List<string>();
            if (resourceType is ResourceType.Font or ResourceType.All)
            {
                foreach (var link in this.Fonts.Where(s => (s.Provider == provider || (s.Provider == string.Empty && provider == ClientResourceProviders.DefaultCssProvider)) && !this.FontsToExclude.Contains(s.Name)).OrderBy(l => l.Priority))
                {
                    sortedList.Add(link.Render(this.hostSettings.CrmVersion, this.hostSettings.CdnEnabled, applicationPath));
                }
            }

            if (resourceType is ResourceType.Stylesheet or ResourceType.All)
            {
                foreach (var link in this.Stylesheets.Where(s => (s.Provider == provider || (s.Provider == string.Empty && provider == ClientResourceProviders.DefaultCssProvider)) && !this.StylesheetsToExclude.Contains(s.Name)).OrderBy(l => l.Priority))
                {
                    sortedList.Add(link.Render(this.hostSettings.CrmVersion, this.hostSettings.CdnEnabled, applicationPath));
                }
            }

            if (resourceType is ResourceType.Script or ResourceType.All)
            {
                foreach (var script in this.Scripts.Where(s => (s.Provider == provider || (s.Provider == string.Empty && provider == ClientResourceProviders.DefaultJsProvider)) && !this.ScriptsToExclude.Contains(s.Name)).OrderBy(s => s.Priority))
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
            resources.RemoveAll(l => string.Equals(l.ResolvedPath, resource.ResolvedPath, StringComparison.OrdinalIgnoreCase)); // remove any existing link with the same key (i.e. exactly the same resolved path)
            if (!string.IsNullOrEmpty(resource.Name))
            {
                // if a resource with the same name and force version is already present we ignore this one
                if (resources.Exists(r => string.Equals(r.Name, resource.Name, StringComparison.OrdinalIgnoreCase) && r.ForceVersion))
                {
                    return resources;
                }

                // If we are forcing the version, we need to remove any existing link with the same name
                if (resource.ForceVersion)
                {
                    resources.RemoveAll(r => string.Equals(r.Name, resource.Name, StringComparison.OrdinalIgnoreCase));
                }

                // If we have a version, we need to remove any existing link with the same name and a lower version
                if (!string.IsNullOrEmpty(resource.Version))
                {
                    resources.RemoveAll(r => string.Equals(r.Name, resource.Name, StringComparison.OrdinalIgnoreCase) && VersionIsLessThan(r.Version, resource.Version));

                    // If we have an existing link with the same name and a higher version, we do not add this link
                    if (resources.Exists(r => string.Equals(r.Name, resource.Name, StringComparison.OrdinalIgnoreCase) && !VersionIsLessThan(r.Version, resource.Version)))
                    {
                        return resources;
                    }
                }
            }

            resources.Add(resource);
            return resources;

            static bool VersionIsLessThan(string version, string otherVersion)
            {
                if (Version.TryParse(version, out var parsedVersion) &&
                    Version.TryParse(otherVersion, out var otherParsedVersion))
                {
                    return parsedVersion < otherParsedVersion;
                }

                return string.Compare(version, otherVersion, StringComparison.OrdinalIgnoreCase) < 0;
            }
        }

        private string ResolvePath(string filePath, string pathNameAlias)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                return filePath;
            }

            if (filePath.StartsWith("http", StringComparison.OrdinalIgnoreCase))
            {
                // Path is already fully qualified
                return filePath;
            }

            // Path is either a relative path including the application path or a path starting with a tilde or a path relative to the path name alias
            filePath = filePath.Replace("\\", "/");
            if (!string.IsNullOrEmpty(pathNameAlias))
            {
                if (this.PathNameAliases.TryGetValue(pathNameAlias, out var alias))
                {
                    return $"{alias}/{filePath.TrimStart('/')}";
                }
            }

            return filePath;
        }
    }
}

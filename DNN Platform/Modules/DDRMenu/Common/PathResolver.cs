﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.DDRMenu.DNNCommon
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Web;

    /// <summary>
    /// Used to resolved paths.
    /// </summary>
    public class PathResolver
    {
        private readonly string manifestFolder;

        /// <summary>
        /// Initializes a new instance of the <see cref="PathResolver"/> class.
        /// </summary>
        /// <param name="manifestFolder">The folder where the manifest is located.</param>
        public PathResolver(string manifestFolder)
        {
            this.manifestFolder = manifestFolder;
        }

        /// <summary>
        /// To which folder is the path relative to.
        /// </summary>
        public enum RelativeTo
        {
            /// <summary>
            /// Relative to the container.
            /// </summary>
            Container,

            /// <summary>
            /// Relative to the Dnn application.
            /// </summary>
            Dnn,

            /// <summary>
            /// Relative to the manifest.
            /// </summary>
            Manifest,

            /// <summary>
            /// Relative to the module.
            /// </summary>
            Module,

            /// <summary>
            /// Relative to the portal (site).
            /// </summary>
            Portal,

            /// <summary>
            /// Relative to the skin (theme).
            /// </summary>
            Skin,
        }

        /// <summary>
        /// Resolves a relative path.
        /// </summary>
        /// <param name="path">The path to resolve.</param>
        /// <param name="roots">To which folder this path is relative to.</param>
        /// <returns>A resolved path.</returns>
        public string Resolve(string path, params RelativeTo[] roots)
        {
            var context = DNNContext.Current;

            var mappings = new Dictionary<string, RelativeTo>
                           {
                            { "[DDRMENU]", RelativeTo.Module },
                            { "[MODULE]", RelativeTo.Module },
                            { "[MANIFEST]", RelativeTo.Manifest },
                            { "[PORTAL]", RelativeTo.Portal },
                            { "[SKIN]", RelativeTo.Skin },
                            { "[CONTAINER]", RelativeTo.Container },
                            { "[DNN]", RelativeTo.Dnn },
                           };
            foreach (var key in mappings.Keys)
            {
                if (path.StartsWith(key, StringComparison.InvariantCultureIgnoreCase))
                {
                    path = path.Substring(key.Length);
                    roots = new[] { mappings[key] };
                    break;
                }
            }

            if (path.StartsWith("/"))
            {
                path = path.Substring(1);
            }

            if (!path.StartsWith("~") && !path.Contains(":"))
            {
                foreach (var root in roots)
                {
                    string resolvedPath = null;
                    switch (root)
                    {
                        case RelativeTo.Container:
                            var container = context.HostControl;
                            while ((container != null) && !(container is UI.Containers.Container))
                            {
                                container = container.Parent;
                            }

                            var containerRoot = (container == null)
                                                    ? context.SkinPath
                                                    : Path.GetDirectoryName(((UI.Containers.Container)container).AppRelativeVirtualPath).Replace('\\', '/') + "/";
                            resolvedPath = Path.Combine(containerRoot, path);
                            break;
                        case RelativeTo.Dnn:
                            resolvedPath = Path.Combine("~/", path);
                            break;
                        case RelativeTo.Manifest:
                            if (!string.IsNullOrEmpty(this.manifestFolder))
                            {
                                resolvedPath = Path.Combine(this.manifestFolder + "/", path);
                            }

                            break;
                        case RelativeTo.Module:
                            resolvedPath = Path.Combine(DNNContext.ModuleFolder, path);
                            break;
                        case RelativeTo.Portal:
                            resolvedPath = Path.Combine(context.PortalSettings.HomeDirectory, path);
                            break;
                        case RelativeTo.Skin:
                            resolvedPath = Path.Combine(context.SkinPath, path);
                            break;
                    }

                    if (!string.IsNullOrEmpty(resolvedPath))
                    {
                        var sep = resolvedPath.LastIndexOf('/');
                        var dirName = resolvedPath.Substring(0, sep + 1);
                        var fileName = resolvedPath.Substring(sep + 1);

                        var mappedDir = HttpContext.Current.Server.MapPath(dirName);
                        mappedDir = mappedDir.Remove(mappedDir.Length - 1);
                        if (Directory.Exists(mappedDir))
                        {
                            if (string.IsNullOrEmpty(fileName))
                            {
                                return resolvedPath.Replace('\\', '/');
                            }

                            var matches = Directory.GetFileSystemEntries(mappedDir, fileName);
                            if (matches.Length > 0)
                            {
                                resolvedPath = (dirName + Path.GetFileName(matches[0])).Replace('\\', '/');
                                return resolvedPath;
                            }
                        }
                    }
                }
            }

            return path;
        }
    }
}

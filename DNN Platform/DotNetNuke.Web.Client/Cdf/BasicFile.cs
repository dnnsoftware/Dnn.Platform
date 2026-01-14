// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.Client.Cdf
{
    using System.Collections.Generic;

    /// <summary>A file.</summary>
    internal class BasicFile
    {
        /// <summary>Initializes a new instance of the <see cref="BasicFile"/> class.</summary>
        /// <param name="type">The dependency type.</param>
        public BasicFile(ClientDependencyType type)
        {
            this.DependencyType = type;
            this.HtmlAttributes = new Dictionary<string, string>();
            this.Priority = 100;
            this.Group = 100;
            this.Name = string.Empty;
            this.Version = string.Empty;
            this.ForceVersion = false;
        }

        /// <summary>Gets or sets the file path.</summary>
        public string FilePath { get; set; }

        /// <summary>Gets the dependency type.</summary>
        public ClientDependencyType DependencyType { get; private set; }

        /// <summary>Gets or sets the priority.</summary>
        public int Priority { get; set; }

        /// <summary>Gets or sets the group.</summary>
        public int Group { get; set; }

        /// <summary>Gets or sets the path name alias.</summary>
        public string PathNameAlias { get; set; }

        /// <summary>Gets or sets the provider.</summary>
        public string ForceProvider { get; set; }

        /// <summary>Gets the HTML attributes.</summary>
        public IDictionary<string, string> HtmlAttributes { get; private set; }

        /// <summary>Gets or sets the name.</summary>
        public string Name { get; set; }

        /// <summary>Gets or sets the version.</summary>
        public string Version { get; set; }

        /// <summary>Gets or sets a value indicating whether to force the use of this version.</summary>
        public bool ForceVersion { get; set; }
    }
}

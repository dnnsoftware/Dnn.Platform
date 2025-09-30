// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.Client.ClientResourceManagement
{
    using System;
    using System.Web.UI;

    using DotNetNuke.Abstractions.ClientResources;
    using DotNetNuke.Web.Client.Cdf;
    using DotNetNuke.Web.Client.ResourceManager;

    /// <summary>Represents an included client resource.</summary>
    public abstract class ClientResourceInclude : Control
    {
        private readonly IClientResourceController clientResourceController;

        /// <summary>
        /// Initializes a new instance of the <see cref="ClientResourceInclude"/> class.
        /// </summary>
        /// <param name="clientResourceController">The client resources controller.</param>
        protected ClientResourceInclude(IClientResourceController clientResourceController)
        {
            this.clientResourceController = clientResourceController;
        }

        /// <summary>
        /// Gets the type of client dependency for this resource (e.g., Javascript or Css).
        /// </summary>
        public ClientDependencyType DependencyType { get; internal set; }

        /// <summary>
        /// Gets or sets the file path for the client resource to be included.
        /// </summary>
        public string FilePath { get; set; }

        /// <summary>
        /// Gets or sets the path name alias for the client resource.
        /// </summary>
        public string PathNameAlias { get; set; }

        /// <summary>
        /// Gets or sets the priority for the client resource. Resources with lower priority values are included before those with higher values.
        /// </summary>
        public int Priority { get; set; }

        /// <summary>
        /// Gets or sets the group for the client resource. Resources in the same group are processed together.
        /// </summary>
        public int Group { get; set; }

        /// <summary>
        /// Gets or sets the name of the script (e.g. <c>jQuery</c>, <c>Bootstrap</c>, <c>Angular</c>, etc.).
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the version of this resource if it is a named resource. Note this field is only used when <see cref="Name" /> is specified.
        /// </summary>
        public string Version { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to force this version to be used. Meant for skin designers that wish to override choices made by module developers or the framework.
        /// </summary>
        public bool ForceVersion { get; set; }

        /// <summary>
        /// Gets or sets the provider to force for this resource. This can be empty and will use default provider.
        /// If specified, it must match a provider registered in the Client Resource Management configuration.
        /// </summary>
        public string ForceProvider { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to add the HTML tag for this resource to the page output.
        /// </summary>
        public bool AddTag { get; set; }

        protected override void OnInit(EventArgs e)
        {
            switch (this.DependencyType)
            {
                case ClientDependencyType.Css:
                    this.clientResourceController.CreateStylesheet()
                                .FromSrc(this.FilePath, this.PathNameAlias)
                                .SetNameAndVersion(this.Name, this.Version, this.ForceVersion)
                                .SetProvider(this.ForceProvider)
                                .SetPriority(this.Priority)
                                .Register();
                    break;
                case ClientDependencyType.Javascript:
                    this.clientResourceController.CreateScript()
                                .FromSrc(this.FilePath, this.PathNameAlias)
                                .SetNameAndVersion(this.Name, this.Version, this.ForceVersion)
                                .SetProvider(this.ForceProvider)
                                .SetPriority(this.Priority)
                                .Register();
                    break;
            }
        }
    }
}

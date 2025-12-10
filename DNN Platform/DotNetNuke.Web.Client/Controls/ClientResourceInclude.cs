// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.Client.ClientResourceManagement
{
    using System;
    using System.Collections.Generic;
    using System.Web.UI;

    using DotNetNuke.Abstractions.ClientResources;
    using DotNetNuke.Web.Client.Cdf;
    using DotNetNuke.Web.Client.ResourceManager;

    /// <summary>Represents an included client resource.</summary>
    public abstract class ClientResourceInclude : Control
    {
        /// <summary>Initializes a new instance of the <see cref="ClientResourceInclude"/> class.</summary>
        protected ClientResourceInclude()
        {
        }

        /// <summary>Gets the type of client dependency for this resource (e.g., <see cref="ClientDependencyType.Javascript"/> or <see cref="ClientDependencyType.Css"/>).</summary>
        public ClientDependencyType DependencyType { get; internal set; }

        /// <summary>Gets or sets the file path for the client resource to be included.</summary>
        public string FilePath { get; set; }

        /// <summary>Gets or sets the path name alias for the client resource.</summary>
        public string PathNameAlias { get; set; }

        /// <summary>Gets or sets the priority for the client resource. Resources with lower priority values are included before those with higher values.</summary>
        public int Priority { get; set; }

        /// <summary>Gets or sets the name of the script (e.g. <c>jQuery</c>, <c>Bootstrap</c>, <c>Angular</c>, etc.).</summary>
        public string Name { get; set; }

        /// <summary>Gets or sets the version of this resource if it is a named resource. Note this field is only used when <see cref="Name" /> is specified.</summary>
        public string Version { get; set; }

        /// <summary>Gets or sets a value indicating whether to force this version to be used. Meant for skin designers that wish to override choices made by module developers or the framework.</summary>
        public bool ForceVersion { get; set; }

        /// <summary>
        /// Gets or sets the provider to force for this resource. This can be empty and will use default provider.
        /// If specified, it must match a provider registered in the Client Resource Management configuration.
        /// </summary>
        public string ForceProvider { get; set; }

        /// <summary>Gets or sets a value indicating whether to add the HTML tag for this resource to the page output.</summary>
        public bool AddTag { get; set; }

        /// <summary>Gets or sets the CDN URL of the resource.</summary>
        public string CdnUrl { get; set; }

        /// <summary>Gets or sets a value indicating whether to render the <c>blocking</c> attribute.</summary>
        public bool Blocking { get; set; }

        /// <summary>Gets or sets the integrity hash of the resource.</summary>
        public string Integrity { get; set; }

        /// <summary>Gets or sets the value of the <c>crossorigin</c> attribute.</summary>
        public CrossOrigin CrossOrigin { get; set; }

        /// <summary>Gets or sets the value of the <c>fetchpriority</c> attribute.</summary>
        public FetchPriority FetchPriority { get; set; }

        /// <summary>Gets or sets the value of the <c>referrerpolicy</c> attribute.</summary>
        public ReferrerPolicy ReferrerPolicy { get; set; }

        /// <summary>Gets or sets the group for the client resource. Resources in the same group are processed together.</summary>
        [Obsolete("Deprecated in DotNetNuke 10.2.0. Grouping is no longer supported, there is no replacement within DNN for this functionality. Scheduled removal in v12.0.0.")]
        public int Group { get; set; }

        /// <summary>Gets or sets a value indicating whether to force this resource to be bundled. No longer supported.</summary>
        [Obsolete("Deprecated in DotNetNuke 10.2.0. Bundling is no longer supported, there is no replacement within DNN for this functionality. Scheduled removal in v12.0.0.")]
        public bool ForceBundle { get; set; }

        /// <summary>Sets common properties on the <paramref name="resource"/> and registers it.</summary>
        /// <param name="resource">The resource to register.</param>
        protected void RegisterResource(IResource resource)
        {
            resource = resource
                .SetNameAndVersion(this.Name, this.Version, this.ForceVersion)
                .SetProvider(this.ForceProvider)
                .SetPriority(this.Priority)
                .SetCdnUrl(this.CdnUrl)
                .SetIntegrity(this.Integrity)
                .SetCrossOrigin(this.CrossOrigin)
                .SetFetchPriority(this.FetchPriority)
                .SetReferrerPolicy(this.ReferrerPolicy);
            if (this.Blocking)
            {
                resource = resource.SetBlocking();
            }

            resource.Register();
        }
    }
}

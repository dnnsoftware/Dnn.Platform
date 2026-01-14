// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.Client.ClientResourceManagement
{
    using System;

    using DotNetNuke.Abstractions.ClientResources;
    using DotNetNuke.Web.Client.Cdf;
    using DotNetNuke.Web.Client.Controls;

    /// <summary>Represents a control that excludes JavaScript client resources from being loaded.</summary>
    public class DnnJsExclude : ClientResourceExclude
    {
        /// <summary>Initializes a new instance of the <see cref="DnnJsExclude"/> class.</summary>
        [Obsolete("Deprecated in DotNetNuke 10.2.1. Use overload with IClientResourceController. Scheduled removal in v12.0.0.")]
        public DnnJsExclude()
            : this(null)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="DnnJsExclude"/> class.</summary>
        /// <param name="clientResourceController">The controller used to manage client resources.</param>
        public DnnJsExclude(IClientResourceController clientResourceController)
            : base(clientResourceController)
        {
            this.DependencyType = ClientDependencyType.Javascript;
        }
    }
}

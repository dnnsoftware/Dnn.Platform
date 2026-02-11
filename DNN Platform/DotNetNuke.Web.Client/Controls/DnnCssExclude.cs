// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.Client.ClientResourceManagement
{
    using System;

    using DotNetNuke.Abstractions.ClientResources;
    using DotNetNuke.Web.Client.Cdf;
    using DotNetNuke.Web.Client.Controls;

    /// <summary>Excludes a CSS resource.</summary>
    public class DnnCssExclude : ClientResourceExclude
    {
        /// <summary>Initializes a new instance of the <see cref="DnnCssExclude"/> class.</summary>
        [Obsolete("Deprecated in DotNetNuke 10.2.1. Use overload with IClientResourceController. Scheduled removal in v12.0.0.")]
        public DnnCssExclude()
            : this(null)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="DnnCssExclude"/> class.</summary>
        /// <param name="clientResourceController">The controller used to manage client resources.</param>
        public DnnCssExclude(IClientResourceController clientResourceController)
            : base(clientResourceController)
        {
            this.DependencyType = ClientDependencyType.Css;
        }
    }
}

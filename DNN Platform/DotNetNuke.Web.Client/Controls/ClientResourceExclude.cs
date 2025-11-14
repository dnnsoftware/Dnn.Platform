// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.Client.Controls
{
    using System;
    using System.Web.UI;

    using DotNetNuke.Abstractions.ClientResources;
    using DotNetNuke.Web.Client.Cdf;

    /// <summary>
    /// Represents a control that excludes a client resource from being included in the page.
    /// </summary>
    public abstract class ClientResourceExclude : Control
    {
        private readonly IClientResourceController clientResourceController;

        /// <summary>
        /// Initializes a new instance of the <see cref="ClientResourceExclude"/> class.
        /// </summary>
        /// <param name="clientResourceController">The client resources controller.</param>
        protected ClientResourceExclude(IClientResourceController clientResourceController)
        {
            this.clientResourceController = clientResourceController;
        }

        /// <summary>
        /// Gets or sets the name of the client resource to exclude.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets the dependency type of the client resource to exclude.
        /// </summary>
        public ClientDependencyType DependencyType { get; internal set; }

        protected override void OnInit(EventArgs e)
        {
            switch (this.DependencyType)
            {
                case ClientDependencyType.Css:
                    this.clientResourceController.RemoveStylesheetByName(this.Name);
                    break;
                case ClientDependencyType.Javascript:
                    this.clientResourceController.RemoveScriptByName(this.Name);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}

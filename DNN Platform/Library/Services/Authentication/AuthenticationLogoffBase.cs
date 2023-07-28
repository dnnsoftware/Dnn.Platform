// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Authentication
{
    using System;

    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Modules;

    /// <summary>
    /// The AuthenticationLogoffBase class provides a base class for Authentiication
    /// Logoff controls.
    /// </summary>
    public abstract class AuthenticationLogoffBase : UserModuleBase
    {
        /// <summary>Initializes a new instance of the <see cref="AuthenticationLogoffBase"/> class.</summary>
        public AuthenticationLogoffBase()
        {
        }

        /// <summary>Fires when a LogOff occurs.</summary>
        public event EventHandler LogOff;

        /// <summary>Fires when a redirect occurs.</summary>
        public event EventHandler Redirect;

        /// <summary>Gets or sets the Type of Authentication associated with this control.</summary>
        public string AuthenticationType { get; set; } = Null.NullString;

        /// <summary>
        /// Gets the Dependency Provider to resolve registered
        /// services with the container.
        /// </summary>
        /// <value>
        /// The Dependency Service.
        /// </value>
        protected new IServiceProvider DependencyProvider => Globals.GetCurrentServiceProvider();

        /// <summary>Handles the <see cref="LogOff"/> event.</summary>
        /// <param name="a">The event arguments.</param>
        protected virtual void OnLogOff(EventArgs a)
        {
            if (this.LogOff != null)
            {
                this.LogOff(null, a);
            }
        }

        /// <summary>Handles the <see cref="Redirect"/> event.</summary>
        /// <param name="a">The event arguments.</param>
        protected virtual void OnRedirect(EventArgs a)
        {
            if (this.Redirect != null)
            {
                this.Redirect(null, a);
            }
        }
    }
}

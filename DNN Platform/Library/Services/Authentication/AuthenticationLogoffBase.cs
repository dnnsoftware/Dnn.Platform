// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
#region Usings

using System;
using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Modules;

#endregion

namespace DotNetNuke.Services.Authentication
{
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The AuthenticationLogoffBase class provides a base class for Authentiication 
    /// Logoff controls
    /// </summary>
    /// -----------------------------------------------------------------------------
    public abstract class AuthenticationLogoffBase : UserModuleBase
    {
        private string _AuthenticationType = Null.NullString;
        private string _RedirectURL = Null.NullString;

        /// <summary>
        /// Gets or sets the Dependency Provider to resolve registered 
        /// services with the container.
        /// </summary>
        /// <value>
        /// The Dependency Service.
        /// </value>
        protected IServiceProvider DependencyProvider { get; }

        public AuthenticationLogoffBase()
        {
            DependencyProvider = Globals.DependencyProvider;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets and Sets the Type of Authentication associated with this control
        /// </summary>
        /// -----------------------------------------------------------------------------
        public string AuthenticationType
        {
            get
            {
                return _AuthenticationType;
            }
            set
            {
                _AuthenticationType = value;
            }
        }

        public event EventHandler LogOff;
        public event EventHandler Redirect;

        protected virtual void OnLogOff(EventArgs a)
        {
            if (LogOff != null)
            {
                LogOff(null, a);
            }
        }

        protected virtual void OnRedirect(EventArgs a)
        {
            if (Redirect != null)
            {
                Redirect(null, a);
            }
        }
    }
}

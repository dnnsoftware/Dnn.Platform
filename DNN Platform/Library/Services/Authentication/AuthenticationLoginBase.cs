// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Authentication
{
    using System;
    using System.Web;

    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Services.UserRequest;

    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The AuthenticationLoginBase class provides a bas class for Authentiication
    /// Login controls.
    /// </summary>
    /// -----------------------------------------------------------------------------
    public abstract class AuthenticationLoginBase : UserModuleBase
    {
        protected AuthenticationLoginBase()
        {
            this.RedirectURL = Null.NullString;
            this.AuthenticationType = Null.NullString;
            this.Mode = AuthMode.Login;
        }

        public delegate void UserAuthenticatedEventHandler(object sender, UserAuthenticatedEventArgs e);

        public event UserAuthenticatedEventHandler UserAuthenticated;

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets a value indicating whether gets whether the control is Enabled.
        /// </summary>
        /// <remarks>This property must be overriden in the inherited class.</remarks>
        /// -----------------------------------------------------------------------------
        public abstract bool Enabled { get; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the IP address associated with the request.
        /// </summary>
        /// -----------------------------------------------------------------------------
        public string IPAddress
        {
            get
            {
                var controller = UserRequestIPAddressController.Instance;
                var request = new HttpRequestWrapper(HttpContext.Current.Request);
                return controller.GetUserRequestIPAddress(request);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets a value indicating whether gets whether the control supports Registration.
        /// </summary>
        /// <remarks>This property may be overriden in the inherited class.</remarks>
        /// -----------------------------------------------------------------------------
        public virtual bool SupportsRegistration
        {
            get { return false; }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets and Sets the Type of Authentication associated with this control.
        /// </summary>
        /// -----------------------------------------------------------------------------
        public string AuthenticationType { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the Authentication mode of the control (Login or Register).
        /// </summary>
        /// <remarks>This property may be overriden in the inherited class.</remarks>
        /// -----------------------------------------------------------------------------
        public virtual AuthMode Mode { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets and Sets the Redirect Url for this control.
        /// </summary>
        /// -----------------------------------------------------------------------------
        public string RedirectURL { get; set; }

        [Obsolete("Deprecated in 9.2.0. Use UserRequestIPAddressController.Instance.GetUserRequestIPAddress. Scheduled removal in v11.0.0.")]
        public static string GetIPAddress()
        {
            return UserRequestIPAddressController.Instance.GetUserRequestIPAddress(new HttpRequestWrapper(HttpContext.Current.Request));
        }

        protected virtual void OnUserAuthenticated(UserAuthenticatedEventArgs ea)
        {
            if (this.UserAuthenticated != null)
            {
                this.UserAuthenticated(null, ea);
            }
        }
    }
}

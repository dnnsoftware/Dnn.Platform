#region Usings
using System.Web;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Services.UserRequest;
using System;
#endregion

namespace DotNetNuke.Services.Authentication
{
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The AuthenticationLoginBase class provides a bas class for Authentiication 
    /// Login controls
    /// </summary>
    /// -----------------------------------------------------------------------------
    public abstract class AuthenticationLoginBase : UserModuleBase
    {
        #region Delegates

        public delegate void UserAuthenticatedEventHandler(object sender, UserAuthenticatedEventArgs e);

        #endregion

        protected AuthenticationLoginBase()
        {
            RedirectURL = Null.NullString;
            AuthenticationType = Null.NullString;
            Mode = AuthMode.Login;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets and Sets the Type of Authentication associated with this control
        /// </summary>
        /// -----------------------------------------------------------------------------
        public string AuthenticationType { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets whether the control is Enabled
        /// </summary>
        /// <remarks>This property must be overriden in the inherited class</remarks>
        /// -----------------------------------------------------------------------------
        public abstract bool Enabled { get; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the IP address associated with the request
        /// </summary>
        /// -----------------------------------------------------------------------------
        public string IPAddress
        {
            get
            {
                var controller = UserRequestIPAddressController.Instance;
                var request = new HttpRequestWrapper(HttpContext.Current.Request);
                return controller.GetUserRequestIPAddress(request); ;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the Authentication mode of the control (Login or Register)
        /// </summary>
        /// <remarks>This property may be overriden in the inherited class</remarks>
        /// -----------------------------------------------------------------------------        
        public virtual AuthMode Mode { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets and Sets the Redirect Url for this control
        /// </summary>
        /// -----------------------------------------------------------------------------
        public string RedirectURL { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets whether the control supports Registration
        /// </summary>
        /// <remarks>This property may be overriden in the inherited class</remarks>
        /// -----------------------------------------------------------------------------
        public virtual bool SupportsRegistration { get { return false; } }

        public event UserAuthenticatedEventHandler UserAuthenticated;

        protected virtual void OnUserAuthenticated(UserAuthenticatedEventArgs ea)
        {
            if (UserAuthenticated != null)
            {
                UserAuthenticated(null, ea);
            }
        }

        [Obsolete("Deprecated in 9.2.0. Use UserRequestIPAddressController.Instance.GetUserRequestIPAddress. Scheduled removal in v11.0.0.")]
        public static string GetIPAddress()
        {
            return UserRequestIPAddressController.Instance.GetUserRequestIPAddress(new HttpRequestWrapper(HttpContext.Current.Request));                        
        }
    }
}

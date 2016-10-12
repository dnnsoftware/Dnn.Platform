namespace Dnn.PersonaBar.Library
{
    /// <summary>
    /// 
    /// </summary>
    public enum ServiceScope
    {
        /// <summary>
        /// the service available for all users, but namespace is personaBar/common.
        /// </summary>
        Common,
        /// <summary>
        /// the service available for all users.
        /// </summary>
        Regular,
        /// <summary>
        /// the service only available for admin users.
        /// </summary>
        Admin,
        /// <summary>
        /// the service only available for host users.
        /// </summary>
        Host,
        /// <summary>
        /// the service avaiable for both admin and host users.
        /// </summary>
        AdminHost
    }
}

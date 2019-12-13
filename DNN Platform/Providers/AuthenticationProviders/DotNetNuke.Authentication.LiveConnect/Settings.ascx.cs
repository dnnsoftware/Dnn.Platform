#region Usings

using System;

using DotNetNuke.Services.Authentication;
using DotNetNuke.Services.Authentication.OAuth;
using DotNetNuke.Services.Exceptions;

#endregion

namespace DotNetNuke.Authentication.LiveConnect
{
    public partial class Settings : OAuthSettingsBase
    {
        protected override string AuthSystemApplicationName
        {
            get { return "Live"; }
        }
    }
}

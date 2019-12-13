#region Usings

using System;

using DotNetNuke.Authentication.Google.Components;
using DotNetNuke.Services.Authentication;
using DotNetNuke.Services.Authentication.OAuth;
using DotNetNuke.Services.Exceptions;

#endregion

namespace DotNetNuke.Authentication.Google
{
    public partial class Settings : OAuthSettingsBase
    {
        protected override string AuthSystemApplicationName
        {
            get { return "Google"; }
        }
    }
}

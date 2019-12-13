// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
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

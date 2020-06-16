// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Authentication.LiveConnect
{
    using System;

    using DotNetNuke.Services.Authentication;
    using DotNetNuke.Services.Authentication.OAuth;
    using DotNetNuke.Services.Exceptions;

    public partial class Settings : OAuthSettingsBase
    {
        protected override string AuthSystemApplicationName
        {
            get { return "Live"; }
        }
    }
}

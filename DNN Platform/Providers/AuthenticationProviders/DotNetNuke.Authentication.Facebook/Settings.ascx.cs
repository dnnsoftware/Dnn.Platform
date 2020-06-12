
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information


using System;

using DotNetNuke.Services.Authentication.OAuth;

namespace DotNetNuke.Authentication.Facebook
{
    public partial class Settings : OAuthSettingsBase
    {
        protected override string AuthSystemApplicationName
        {
            get { return "Facebook"; }
        }
    }
}

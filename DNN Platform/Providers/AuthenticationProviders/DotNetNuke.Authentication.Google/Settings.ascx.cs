﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Authentication.Google
{
    using System;

    using DotNetNuke.Services.Authentication.OAuth;

    /// <inheritdoc/>
    public partial class Settings : OAuthSettingsBase
    {
        /// <inheritdoc/>
        protected override string AuthSystemApplicationName
        {
            get { return "Google"; }
        }
    }
}

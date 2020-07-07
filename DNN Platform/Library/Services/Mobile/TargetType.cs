// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Mobile
{
    using System;

    public enum TargetType
    {
        /// <summary>
        /// Redirect when request from a mobile
        /// </summary>
        Portal = 1,

        /// <summary>
        /// Redirect when request from a tablet
        /// </summary>
        Tab = 2,

        /// <summary>
        /// Redirect when request from some unknown device, should be determine by match rules;
        /// </summary>
        Url = 3,
    }
}

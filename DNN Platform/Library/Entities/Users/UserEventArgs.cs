// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Entities.Users
{
    using System;

    public class UserEventArgs : EventArgs
    {
        public UserInfo User { get; set; }

        public bool SendNotification { get; set; }
    }
}

// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using System;

namespace DotNetNuke.Entities.Users
{
    public class UserEventArgs : EventArgs
    {
        public UserInfo User { get; set; }
        public bool SendNotification { get; set; }
    }
}

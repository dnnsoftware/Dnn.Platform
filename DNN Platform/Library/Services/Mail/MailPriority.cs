﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Services.Mail
{
    /// <summary>Enum MailPriority, there are 3 levels of priorities: Normal, Low or High.</summary>
    public enum MailPriority
    {
        /// <summary>Normal priority.</summary>
        Normal = 0,

        /// <summary>Low priority.</summary>
        Low = 1,

        /// <summary>High priority.</summary>
        High = 2,
    }
}

﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Entities.Tabs
{
    public class PermissionsNotMetException : TabException
    {
        public PermissionsNotMetException(int tabId, string message)
            : base(tabId, message)
        {
        }
    }
}

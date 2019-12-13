// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
#region Usings

using System;

#endregion

namespace DotNetNuke.Entities.Tabs
{
    public class TabExistsException : TabException
    {
        public TabExistsException(int tabId, string message) : base(tabId, message)
        {            
        }

    }
}

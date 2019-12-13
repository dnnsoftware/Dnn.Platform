// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using System;

namespace DotNetNuke.ExtensionPoints
{
    public interface IEditPageTabControlActions
    {
        void SaveAction(int portalId, int tabId, int moduleId);
        void CancelAction(int portalId, int tabId, int moduleId);
        void BindAction(int portalId, int tabId, int moduleId);
    }
}

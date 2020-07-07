// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.ExtensionPoints
{
    using System;

    public interface IEditPageTabControlActions
    {
        void SaveAction(int portalId, int tabId, int moduleId);

        void CancelAction(int portalId, int tabId, int moduleId);

        void BindAction(int portalId, int tabId, int moduleId);
    }
}

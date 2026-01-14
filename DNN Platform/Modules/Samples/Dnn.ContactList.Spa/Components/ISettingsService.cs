// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.ContactList.Spa.Components
{
    public interface ISettingsService
    {
        bool IsFormEnabled(int moduleId, int tabId);

        void SaveFormEnabled(bool isEnabled, int moduleId);
    }
}

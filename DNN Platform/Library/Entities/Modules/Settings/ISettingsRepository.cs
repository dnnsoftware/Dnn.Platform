// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Entities.Modules.Settings
{
    public interface ISettingsRepository<T>
        where T : class
    {
        T GetSettings(ModuleInfo moduleContext);

        void SaveSettings(ModuleInfo moduleContext, T settings);
    }
}

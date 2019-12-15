// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
namespace DotNetNuke.Entities.Modules.Settings
{
    public interface ISettingsRepository<T> where T : class
    {
        T GetSettings(ModuleInfo moduleContext);
        void SaveSettings(ModuleInfo moduleContext, T settings);
    }
}

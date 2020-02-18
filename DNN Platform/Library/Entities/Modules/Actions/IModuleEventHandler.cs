// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
namespace DotNetNuke.Entities.Modules.Actions
{
    public interface IModuleEventHandler
    {
        void ModuleCreated(object sender, ModuleEventArgs args);
        void ModuleUpdated(object sender, ModuleEventArgs args);
        void ModuleRemoved(object sender, ModuleEventArgs args);
        void ModuleDeleted(object sender, ModuleEventArgs args);
    }
}

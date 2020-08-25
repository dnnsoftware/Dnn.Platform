// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

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

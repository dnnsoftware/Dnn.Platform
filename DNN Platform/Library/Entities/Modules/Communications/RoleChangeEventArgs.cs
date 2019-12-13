// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
namespace DotNetNuke.Entities.Modules.Communications
{
    public class RoleChangeEventArgs : ModuleCommunicationEventArgs
    {
        public string PortalId { get; set; }

        public string RoleId { get; set; }
    }
}

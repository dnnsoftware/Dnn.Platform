// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Recyclebin.Components.Dto
{

    public class ModuleItem
    {
        public int Id { get; set; }

        public int TabModuleId { get; set; }

        public string Title { get; set; }

        public int PortalId { get; set; }

        public string TabName { get; set; }

        public int TabID { get; set; }

        public bool TabDeleted { get; set; }

        public string LastModifiedOnDate { get; set; }

        public string FriendlyLastModifiedOnDate { get; set; }
    }
}

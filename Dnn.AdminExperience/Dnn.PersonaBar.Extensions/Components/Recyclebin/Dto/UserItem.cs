// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Recyclebin.Components.Dto
{

    public class UserItem
    {
        public int Id { get; set; }

        public int PortalId { get; set; }

        public string Username { get; set; }

        public string DisplayName { get; set; }

        public string Email { get; set; }

        public string LastModifiedOnDate { get; set; }

        public string FriendlyLastModifiedOnDate { get; set; }
    }
}

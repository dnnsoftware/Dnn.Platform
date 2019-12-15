// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
﻿namespace Dnn.PersonaBar.Recyclebin.Components.Dto
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

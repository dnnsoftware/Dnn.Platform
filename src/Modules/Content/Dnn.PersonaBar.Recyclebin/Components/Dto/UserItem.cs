﻿#region Copyright
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2016
// by DotNetNuke Corporation
// All Rights Reserved
#endregion

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

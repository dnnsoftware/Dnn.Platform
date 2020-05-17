﻿// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
#region Usings

using DotNetNuke.ComponentModel;
using DotNetNuke.Entities.Users;

#endregion

namespace DotNetNuke.Security.Profile
{
    public abstract class ProfileProvider
    {
        #region "Abstract Properties"
		
		public abstract bool CanEditProviderProperties { get; }
		
		#endregion

        #region "Shared/Static Methods"

        //return the provider		
		public static ProfileProvider Instance()
        {
            return ComponentFactory.GetComponent<ProfileProvider>();
        }
		
		#endregion

        #region "Abstract Methods"
		
		public abstract void GetUserProfile(ref UserInfo user);

        public abstract void UpdateUserProfile(UserInfo user);

		#endregion
    }
}

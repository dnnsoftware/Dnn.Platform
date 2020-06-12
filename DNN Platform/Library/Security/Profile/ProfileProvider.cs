// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

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

        // return the provider		
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

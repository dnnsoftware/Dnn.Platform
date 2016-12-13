#region Copyright
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2017
// by DotNetNuke Corporation
// All Rights Reserved
#endregion

#region "Usings"

using System;

using DotNetNuke.Services.Mobile;

#endregion

namespace DotNetNuke.Modules.PreviewProfileManagement.Components
{
	/// <summary>
	/// Event args of profile instance.
	/// </summary>
	public class ProfileEventArgs : EventArgs
	{
		#region "Public Properties"

		/// <summary>
		/// The profile need to process by caller.
		/// </summary>
		public IPreviewProfile Profile { get; set; }

		#endregion

		#region "Constructors"

		/// <summary>
		/// Default constructor for ProfileEventArgs.
		/// </summary>
		/// <param name="profile">The profile need to process.</param>
		public ProfileEventArgs(IPreviewProfile profile)
		{
			Profile = profile;
		}

		#endregion
	}
}
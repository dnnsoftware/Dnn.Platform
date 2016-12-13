#region Copyright
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2017
// by DotNetNuke Corporation
// All Rights Reserved
#endregion

#region "Usings"

using System;

using DotNetNuke.Modules.PreviewProfileManagement.Components;
using DotNetNuke.Modules.PreviewProfileManagement.ViewModels;
using DotNetNuke.Web.Mvp;

#endregion

namespace DotNetNuke.Modules.PreviewProfileManagement.Views
{
    /// <summary>
    /// 
    /// </summary>
	public interface IProfileManagerView : IModuleView<ProfileManagerViewModel>
    {
		/// <summary>
		/// Event for get profile data.
		/// </summary>
    	event EventHandler GetProfiles;

		/// <summary>
		/// Event for get highlight profile data, this data will be used for auto complete the device name.
		/// </summary>
    	event EventHandler GetHighlightProfiles;

		/// <summary>
		/// Event for save a profile.
		/// </summary>
    	event EventHandler<ProfileEventArgs> SaveProfile;

		/// <summary>
		/// Event for delete a profile.
		/// </summary>
		event EventHandler<PrimaryKeyEventArgs> DeleteProfile;

		/// <summary>
		/// Event for get a profile to edit.
		/// </summary>
		event EventHandler<PrimaryKeyEventArgs> GetEditProfile;
    }
}

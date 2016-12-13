#region Copyright
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2017
// by DotNetNuke Corporation
// All Rights Reserved
#endregion

#region "Usings"

using System.Collections.Generic;

using DotNetNuke.Services.Mobile;

#endregion

namespace DotNetNuke.Modules.PreviewProfileManagement.ViewModels
{
	/// <summary>
	/// View model of profile management.
	/// </summary>
	public class ProfileManagerViewModel
	{
		/// <summary>
		/// Profiles list.
		/// </summary>
		public IList<IPreviewProfile> PreviewProfiles { get; set; }

		/// <summary>
		/// Highlight profiles list.
		/// </summary>
		public List<IPreviewProfile> HighlightProfiles { get; set; }

		/// <summary>
		/// Profile need to edit.
		/// </summary>
		public IPreviewProfile EditProfile { get; set; }
	}
}
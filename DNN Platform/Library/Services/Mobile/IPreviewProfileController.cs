// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
#region Usings

using System;
using System.Collections.Generic;

#endregion


namespace DotNetNuke.Services.Mobile
{
	public interface IPreviewProfileController
	{
		void Save(IPreviewProfile profile);

		void Delete(int portalId, int id);

		IList<IPreviewProfile> GetProfilesByPortal(int portalId);

		IPreviewProfile GetProfileById(int portalId, int id);
	}
}

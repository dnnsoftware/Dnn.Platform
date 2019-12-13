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

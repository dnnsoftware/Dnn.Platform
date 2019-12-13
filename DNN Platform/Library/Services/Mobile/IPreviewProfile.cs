// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
#region Usings

using System;
using System.Collections.Generic;

using DotNetNuke.Entities.Modules;

#endregion

namespace DotNetNuke.Services.Mobile
{
	public interface IPreviewProfile
	{
		int Id { get; set; }

		int PortalId { get; set; }

		string Name { get; set; }

		int Width { get; set; }

		string UserAgent { get; set; }

		int Height { get; set; }

		int SortOrder { get; set; }
	}
}

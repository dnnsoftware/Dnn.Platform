// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

#region Usings

using DotNetNuke.Entities.Modules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

#endregion

namespace DotNetNuke.Services.Search
{
	internal class ModuleIndexInfo
	{
		public ModuleInfo ModuleInfo { get; set; }

		public bool SupportSearch { get; set; }
	}
}

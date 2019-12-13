// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
#region Usings

using System;

#endregion

namespace DotNetNuke.Services.Mobile
{
	public enum TargetType
	{
		/// <summary>
		/// Redirect when request from a mobile
		/// </summary>
		Portal = 1,

		/// <summary>
		/// Redirect when request from a tablet
		/// </summary>
		Tab = 2,

		/// <summary>
		/// Redirect when request from some unknown device, should be determine by match rules; 
		/// </summary>
		Url = 3
	}
}

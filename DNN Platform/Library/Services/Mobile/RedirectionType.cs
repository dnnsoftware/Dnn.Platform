// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
#region Usings

using System;

#endregion

namespace DotNetNuke.Services.Mobile
{
	public enum RedirectionType
	{
		/// <summary>
		/// Redirect when request from a mobile phone
		/// </summary>
		MobilePhone = 1,

		/// <summary>
		/// Redirect when request from a tablet
		/// </summary>
		Tablet = 2,

        /// <summary>
        /// Redirect when request from either a mobile phone or a tablet
        /// </summary>
        AllMobile = 3,

		/// <summary>
		/// Redirect when request from some unknown device, should be determine by match rules; 
		/// </summary>
		Other = 4,

		/// <summary>
		/// Redirect when request from a smart phone
		/// </summary>
		SmartPhone = 5
	}
}

// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
#region Usings

using System;

#endregion

namespace DotNetNuke.Services.Mobile
{
	public interface IMatchRule
	{
		/// <summary>
		/// Primary Id.
		/// </summary>
		int Id { get; }
		/// <summary>
		/// capbility name.
		/// </summary>
		string Capability { get; set; }

		/// <summary>
		/// reg expression to match the request
		/// </summary>
		string Expression { get; set; }
	}
}

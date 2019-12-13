// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
#region Usings

using System;

#endregion

namespace DotNetNuke.Entities.Users
{
	public enum RelationshipDirection
	{
		/// <summary>
		/// One way relationship, e.g. Follower, where user 1 is following user 2, but user 2 is not following user 1.
		/// </summary>
		OneWay = 1,

		/// <summary>
        /// Two way relationship, e.g. Friend, where user 1 and user 2 are both friends and mutually following each other.
		/// </summary>
		TwoWay = 2
	}
}

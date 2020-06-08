﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
#region Usings

using System.Linq;

#endregion

namespace DotNetNuke.Entities.Content.Taxonomy
{
	/// <summary>
	/// Interface of ScopeTypeController.
	/// </summary>
	/// <seealso cref="ScopeTypeController"/>
    public interface IScopeTypeController
    {
        int AddScopeType(ScopeType scopeType);

        void ClearScopeTypeCache();

        void DeleteScopeType(ScopeType scopeType);

        IQueryable<ScopeType> GetScopeTypes();

        void UpdateScopeType(ScopeType scopeType);
    }
}

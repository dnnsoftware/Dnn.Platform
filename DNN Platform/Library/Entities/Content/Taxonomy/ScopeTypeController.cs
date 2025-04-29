// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Entities.Content.Taxonomy;

using System.Collections.Generic;
using System.Linq;

using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Content.Common;
using DotNetNuke.Entities.Content.Data;

/// <summary>ScopeTypeController provides the business layer of ScopeType.</summary>
/// <seealso cref="TermController"/>
public class ScopeTypeController : IScopeTypeController
{
    private const int CacheTimeOut = 20;
    private readonly IDataService dataService;

    /// <summary>Initializes a new instance of the <see cref="ScopeTypeController"/> class.</summary>
    public ScopeTypeController()
        : this(Util.GetDataService())
    {
    }

    /// <summary>Initializes a new instance of the <see cref="ScopeTypeController"/> class.</summary>
    /// <param name="dataService"></param>
    public ScopeTypeController(IDataService dataService)
    {
        this.dataService = dataService;
    }

    /// <inheritdoc/>
    public int AddScopeType(ScopeType scopeType)
    {
        // Argument Contract
        Requires.NotNull("scopeType", scopeType);
        Requires.PropertyNotNullOrEmpty("scopeType", "ScopeType", scopeType.ScopeType);

        scopeType.ScopeTypeId = this.dataService.AddScopeType(scopeType);

        // Refresh cached collection of types
        DataCache.RemoveCache(DataCache.ScopeTypesCacheKey);

        return scopeType.ScopeTypeId;
    }

    /// <inheritdoc/>
    public void ClearScopeTypeCache()
    {
        DataCache.RemoveCache(DataCache.ScopeTypesCacheKey);
    }

    /// <inheritdoc/>
    public void DeleteScopeType(ScopeType scopeType)
    {
        // Argument Contract
        Requires.NotNull("scopeType", scopeType);
        Requires.PropertyNotNegative("scopeType", "ScopeTypeId", scopeType.ScopeTypeId);

        this.dataService.DeleteScopeType(scopeType);

        // Refresh cached collection of types
        DataCache.RemoveCache(DataCache.ScopeTypesCacheKey);
    }

    /// <inheritdoc/>
    public IQueryable<ScopeType> GetScopeTypes()
    {
        return CBO.GetCachedObject<List<ScopeType>>(new CacheItemArgs(DataCache.ScopeTypesCacheKey, CacheTimeOut), this.GetScopeTypesCallBack).AsQueryable();
    }

    /// <inheritdoc/>
    public void UpdateScopeType(ScopeType scopeType)
    {
        // Argument Contract
        Requires.NotNull("scopeType", scopeType);
        Requires.PropertyNotNegative("scopeType", "ScopeTypeId", scopeType.ScopeTypeId);
        Requires.PropertyNotNullOrEmpty("scopeType", "ScopeType", scopeType.ScopeType);

        this.dataService.UpdateScopeType(scopeType);

        // Refresh cached collection of types
        DataCache.RemoveCache(DataCache.ScopeTypesCacheKey);
    }

    private object GetScopeTypesCallBack(CacheItemArgs cacheItemArgs)
    {
        return CBO.FillQueryable<ScopeType>(this.dataService.GetScopeTypes()).ToList();
    }
}

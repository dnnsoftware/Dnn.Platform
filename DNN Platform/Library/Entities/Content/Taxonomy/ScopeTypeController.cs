// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Entities.Content.Taxonomy
{
    using System.Collections.Generic;
    using System.Linq;

    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Content.Common;
    using DotNetNuke.Entities.Content.Data;

    /// <summary>
    /// ScopeTypeController provides the business layer of ScopeType.
    /// </summary>
    /// <seealso cref="TermController"/>
    public class ScopeTypeController : IScopeTypeController
    {
        private const int _CacheTimeOut = 20;
        private readonly IDataService _DataService;

        public ScopeTypeController()
            : this(Util.GetDataService())
        {
        }

        public ScopeTypeController(IDataService dataService)
        {
            this._DataService = dataService;
        }

        public int AddScopeType(ScopeType scopeType)
        {
            // Argument Contract
            Requires.NotNull("scopeType", scopeType);
            Requires.PropertyNotNullOrEmpty("scopeType", "ScopeType", scopeType.ScopeType);

            scopeType.ScopeTypeId = this._DataService.AddScopeType(scopeType);

            // Refresh cached collection of types
            DataCache.RemoveCache(DataCache.ScopeTypesCacheKey);

            return scopeType.ScopeTypeId;
        }

        public void ClearScopeTypeCache()
        {
            DataCache.RemoveCache(DataCache.ScopeTypesCacheKey);
        }

        public void DeleteScopeType(ScopeType scopeType)
        {
            // Argument Contract
            Requires.NotNull("scopeType", scopeType);
            Requires.PropertyNotNegative("scopeType", "ScopeTypeId", scopeType.ScopeTypeId);

            this._DataService.DeleteScopeType(scopeType);

            // Refresh cached collection of types
            DataCache.RemoveCache(DataCache.ScopeTypesCacheKey);
        }

        public IQueryable<ScopeType> GetScopeTypes()
        {
            return CBO.GetCachedObject<List<ScopeType>>(new CacheItemArgs(DataCache.ScopeTypesCacheKey, _CacheTimeOut), this.GetScopeTypesCallBack).AsQueryable();
        }

        public void UpdateScopeType(ScopeType scopeType)
        {
            // Argument Contract
            Requires.NotNull("scopeType", scopeType);
            Requires.PropertyNotNegative("scopeType", "ScopeTypeId", scopeType.ScopeTypeId);
            Requires.PropertyNotNullOrEmpty("scopeType", "ScopeType", scopeType.ScopeType);

            this._DataService.UpdateScopeType(scopeType);

            // Refresh cached collection of types
            DataCache.RemoveCache(DataCache.ScopeTypesCacheKey);
        }

        private object GetScopeTypesCallBack(CacheItemArgs cacheItemArgs)
        {
            return CBO.FillQueryable<ScopeType>(this._DataService.GetScopeTypes()).ToList();
        }
    }
}

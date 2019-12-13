#region Usings

using System.Collections.Generic;
using System.Linq;

using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Content.Common;
using DotNetNuke.Entities.Content.Data;

#endregion

namespace DotNetNuke.Entities.Content.Taxonomy
{
	/// <summary>
	/// ScopeTypeController provides the business layer of ScopeType.
	/// </summary>
	/// <seealso cref="TermController"/>
    public class ScopeTypeController : IScopeTypeController
    {
        private readonly IDataService _DataService;
        private const int _CacheTimeOut = 20;

        #region "Constructors"

        public ScopeTypeController() : this(Util.GetDataService())
        {
        }

        public ScopeTypeController(IDataService dataService)
        {
            _DataService = dataService;
        }

        #endregion

        #region "Private Methods"

        private object GetScopeTypesCallBack(CacheItemArgs cacheItemArgs)
        {
            return CBO.FillQueryable<ScopeType>(_DataService.GetScopeTypes()).ToList();
        }

        #endregion

        #region "Public Methods"

        public int AddScopeType(ScopeType scopeType)
        {
            //Argument Contract
            Requires.NotNull("scopeType", scopeType);
            Requires.PropertyNotNullOrEmpty("scopeType", "ScopeType", scopeType.ScopeType);

            scopeType.ScopeTypeId = _DataService.AddScopeType(scopeType);

            //Refresh cached collection of types
            DataCache.RemoveCache(DataCache.ScopeTypesCacheKey);

            return scopeType.ScopeTypeId;
        }

        public void ClearScopeTypeCache()
        {
            DataCache.RemoveCache(DataCache.ScopeTypesCacheKey);
        }

        public void DeleteScopeType(ScopeType scopeType)
        {
            //Argument Contract
            Requires.NotNull("scopeType", scopeType);
            Requires.PropertyNotNegative("scopeType", "ScopeTypeId", scopeType.ScopeTypeId);

            _DataService.DeleteScopeType(scopeType);

            //Refresh cached collection of types
            DataCache.RemoveCache(DataCache.ScopeTypesCacheKey);
        }

        public IQueryable<ScopeType> GetScopeTypes()
        {
            return CBO.GetCachedObject<List<ScopeType>>(new CacheItemArgs(DataCache.ScopeTypesCacheKey, _CacheTimeOut), GetScopeTypesCallBack).AsQueryable();
        }

        public void UpdateScopeType(ScopeType scopeType)
        {
            //Argument Contract
            Requires.NotNull("scopeType", scopeType);
            Requires.PropertyNotNegative("scopeType", "ScopeTypeId", scopeType.ScopeTypeId);
            Requires.PropertyNotNullOrEmpty("scopeType", "ScopeType", scopeType.ScopeType);

            _DataService.UpdateScopeType(scopeType);

            //Refresh cached collection of types
            DataCache.RemoveCache(DataCache.ScopeTypesCacheKey);
        }

        #endregion
    }
}

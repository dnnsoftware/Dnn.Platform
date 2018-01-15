#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2018
// by DotNetNuke Corporation
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation 
// the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and 
// to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions 
// of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
// TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
// CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
// DEALINGS IN THE SOFTWARE.
#endregion
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
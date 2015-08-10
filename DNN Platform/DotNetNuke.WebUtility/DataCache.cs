#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2015
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

using System;
using System.Collections;
using System.Diagnostics;
using System.Web;
//Copied from DotNetNuke Core

namespace DotNetNuke.UI.Utilities
{
	public class DataCache
	{

		public static object GetCache(string CacheKey)
		{

			System.Web.Caching.Cache objCache = HttpRuntime.Cache;

			return objCache[CacheKey];

		}


		public static void SetCache(string CacheKey, object objObject)
		{
			System.Web.Caching.Cache objCache = HttpRuntime.Cache;

			objCache.Insert(CacheKey, objObject);

		}


		public static void SetCache(string CacheKey, object objObject, System.Web.Caching.CacheDependency objDependency)
		{
			System.Web.Caching.Cache objCache = HttpRuntime.Cache;

			objCache.Insert(CacheKey, objObject, objDependency);

		}

		public static void SetCache(string CacheKey, object objObject, System.Web.Caching.CacheDependency objDependency, System.DateTime AbsoluteExpiration, System.TimeSpan SlidingExpiration)
		{
			System.Web.Caching.Cache objCache = HttpRuntime.Cache;

			objCache.Insert(CacheKey, objObject, objDependency, AbsoluteExpiration, SlidingExpiration);

		}



		public static void SetCache(string CacheKey, object objObject, int SlidingExpiration)
		{
			System.Web.Caching.Cache objCache = HttpRuntime.Cache;

			objCache.Insert(CacheKey, objObject, null, System.Web.Caching.Cache.NoAbsoluteExpiration, TimeSpan.FromSeconds(SlidingExpiration));

		}


		public static void SetCache(string CacheKey, object objObject, System.DateTime AbsoluteExpiration)
		{
			System.Web.Caching.Cache objCache = HttpRuntime.Cache;

			objCache.Insert(CacheKey, objObject, null, AbsoluteExpiration, System.Web.Caching.Cache.NoSlidingExpiration);

		}


		public static void RemoveCache(string CacheKey)
		{
			System.Web.Caching.Cache objCache = HttpRuntime.Cache;

			if ((objCache[CacheKey] != null)) {
				objCache.Remove(CacheKey);
			}

		}
	}
}

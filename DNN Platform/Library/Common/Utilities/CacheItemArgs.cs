﻿// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
#region Usings

using System.Collections;
using System.Web.Caching;

using DotNetNuke.Services.Cache;

#endregion

namespace DotNetNuke.Common.Utilities
{
    /// -----------------------------------------------------------------------------
    /// Project:    DotNetNuke
    /// Namespace:  DotNetNuke.Common.Utilities
    /// Class:      CacheItemArgs
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The CacheItemArgs class provides an EventArgs implementation for the
    /// CacheItemExpiredCallback delegate
    /// </summary>
    /// -----------------------------------------------------------------------------
    public class CacheItemArgs
    {
        private ArrayList _paramList;

        ///-----------------------------------------------------------------------------
        /// <summary>
        /// Constructs a new CacheItemArgs Object
        /// </summary>
        /// <param name="key"></param>
        ///-----------------------------------------------------------------------------
        public CacheItemArgs(string key)
            : this(key, 20, CacheItemPriority.Default, null)
        {
        }

        ///-----------------------------------------------------------------------------
        /// <summary>
        /// Constructs a new CacheItemArgs Object
        /// </summary>
        /// <param name="key"></param>
        /// <param name="timeout"></param>
        ///-----------------------------------------------------------------------------
        public CacheItemArgs(string key, int timeout)
            : this(key, timeout, CacheItemPriority.Default, null)
        {
        }

        ///-----------------------------------------------------------------------------
        /// <summary>
        /// Constructs a new CacheItemArgs Object
        /// </summary>
        /// <param name="key"></param>
        /// <param name="priority"></param>
        ///-----------------------------------------------------------------------------
        public CacheItemArgs(string key, CacheItemPriority priority)
            : this(key, 20, priority, null)
        {
        }

        ///-----------------------------------------------------------------------------
        /// <summary>
        /// Constructs a new CacheItemArgs Object
        /// </summary>
        /// <param name="key"></param>
        /// <param name="timeout"></param>
        /// <param name="priority"></param>
        ///-----------------------------------------------------------------------------
        public CacheItemArgs(string key, int timeout, CacheItemPriority priority)
            : this(key, timeout, priority, null)
        {
        }

        ///-----------------------------------------------------------------------------
        /// <summary>
        /// Constructs a new CacheItemArgs Object
        /// </summary>
        /// <param name="key"></param>
        /// <param name="timeout"></param>
        /// <param name="priority"></param>
        /// <param name="parameters"></param>
        ///-----------------------------------------------------------------------------
        public CacheItemArgs(string key, int timeout, CacheItemPriority priority, params object[] parameters)
        {
            CacheKey = key;
            CacheTimeOut = timeout;
            CachePriority = priority;
            Params = parameters;
        }

        ///-----------------------------------------------------------------------------
        /// <summary>
        /// Gets and sets the Cache Item's CacheItemRemovedCallback delegate
        /// </summary>
        ///-----------------------------------------------------------------------------
        public CacheItemRemovedCallback CacheCallback { get; set; }

        ///-----------------------------------------------------------------------------
        /// <summary>
        /// Gets and sets the Cache Item's CacheDependency
        /// </summary>
        ///-----------------------------------------------------------------------------
        public DNNCacheDependency CacheDependency { get; set; }

        ///-----------------------------------------------------------------------------
        /// <summary>
        /// Gets the Cache Item's Key
        /// </summary>
        ///-----------------------------------------------------------------------------
        public string CacheKey { get; set; }

        ///-----------------------------------------------------------------------------
        /// <summary>
        /// Gets the Cache Item's priority (defaults to Default)
        /// </summary>
        /// <remarks>Note: DotNetNuke currently doesn't support the ASP.NET Cache's
        /// ItemPriority, but this is included for possible future use. </remarks>
        ///-----------------------------------------------------------------------------
        public CacheItemPriority CachePriority { get; set; }

        ///-----------------------------------------------------------------------------
        /// <summary>
        /// Gets the Cache Item's Timeout
        /// </summary>
        ///-----------------------------------------------------------------------------
        public int CacheTimeOut { get; set; }

        ///-----------------------------------------------------------------------------
        /// <summary>
        /// Gets the Cache Item's Parameter List
        /// </summary>
        ///-----------------------------------------------------------------------------
        public ArrayList ParamList
        {
            get
            {
                if (_paramList == null)
                {
                    _paramList = new ArrayList();
					//add additional params to this list if its not null
					if (Params != null)
					{
						foreach (object param in Params)
						{
							_paramList.Add(param);
						}
					}
                }

                return _paramList;
            }
        }

        ///-----------------------------------------------------------------------------
        /// <summary>
        /// Gets the Cache Item's Parameter Array
        /// </summary>
        ///-----------------------------------------------------------------------------
        public object[] Params { get; private set; }

        public string ProcedureName { get; set; }
    }
}

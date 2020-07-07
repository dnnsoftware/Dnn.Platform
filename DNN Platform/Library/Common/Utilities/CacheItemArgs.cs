// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Common.Utilities
{
    using System.Collections;
    using System.Web.Caching;

    using DotNetNuke.Services.Cache;

    /// -----------------------------------------------------------------------------
    /// Project:    DotNetNuke
    /// Namespace:  DotNetNuke.Common.Utilities
    /// Class:      CacheItemArgs
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The CacheItemArgs class provides an EventArgs implementation for the
    /// CacheItemExpiredCallback delegate.
    /// </summary>
    /// -----------------------------------------------------------------------------
    public class CacheItemArgs
    {
        private ArrayList _paramList;

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Initializes a new instance of the <see cref="CacheItemArgs"/> class.
        /// Constructs a new CacheItemArgs Object.
        /// </summary>
        /// <param name="key"></param>
        /// -----------------------------------------------------------------------------
        public CacheItemArgs(string key)
            : this(key, 20, CacheItemPriority.Default, null)
        {
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Initializes a new instance of the <see cref="CacheItemArgs"/> class.
        /// Constructs a new CacheItemArgs Object.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="timeout"></param>
        /// -----------------------------------------------------------------------------
        public CacheItemArgs(string key, int timeout)
            : this(key, timeout, CacheItemPriority.Default, null)
        {
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Initializes a new instance of the <see cref="CacheItemArgs"/> class.
        /// Constructs a new CacheItemArgs Object.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="priority"></param>
        /// -----------------------------------------------------------------------------
        public CacheItemArgs(string key, CacheItemPriority priority)
            : this(key, 20, priority, null)
        {
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Initializes a new instance of the <see cref="CacheItemArgs"/> class.
        /// Constructs a new CacheItemArgs Object.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="timeout"></param>
        /// <param name="priority"></param>
        /// -----------------------------------------------------------------------------
        public CacheItemArgs(string key, int timeout, CacheItemPriority priority)
            : this(key, timeout, priority, null)
        {
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Initializes a new instance of the <see cref="CacheItemArgs"/> class.
        /// Constructs a new CacheItemArgs Object.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="timeout"></param>
        /// <param name="priority"></param>
        /// <param name="parameters"></param>
        /// -----------------------------------------------------------------------------
        public CacheItemArgs(string key, int timeout, CacheItemPriority priority, params object[] parameters)
        {
            this.CacheKey = key;
            this.CacheTimeOut = timeout;
            this.CachePriority = priority;
            this.Params = parameters;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the Cache Item's Parameter List.
        /// </summary>
        /// -----------------------------------------------------------------------------
        public ArrayList ParamList
        {
            get
            {
                if (this._paramList == null)
                {
                    this._paramList = new ArrayList();

                    // add additional params to this list if its not null
                    if (this.Params != null)
                    {
                        foreach (object param in this.Params)
                        {
                            this._paramList.Add(param);
                        }
                    }
                }

                return this._paramList;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets and sets the Cache Item's CacheItemRemovedCallback delegate.
        /// </summary>
        /// -----------------------------------------------------------------------------
        public CacheItemRemovedCallback CacheCallback { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets and sets the Cache Item's CacheDependency.
        /// </summary>
        /// -----------------------------------------------------------------------------
        public DNNCacheDependency CacheDependency { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the Cache Item's Key.
        /// </summary>
        /// -----------------------------------------------------------------------------
        public string CacheKey { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the Cache Item's priority (defaults to Default).
        /// </summary>
        /// <remarks>Note: DotNetNuke currently doesn't support the ASP.NET Cache's
        /// ItemPriority, but this is included for possible future use. </remarks>
        /// -----------------------------------------------------------------------------
        public CacheItemPriority CachePriority { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the Cache Item's Timeout.
        /// </summary>
        /// -----------------------------------------------------------------------------
        public int CacheTimeOut { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the Cache Item's Parameter Array.
        /// </summary>
        /// -----------------------------------------------------------------------------
        public object[] Params { get; private set; }

        public string ProcedureName { get; set; }
    }
}

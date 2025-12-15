// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Framework
{
    using System;
    using System.Text;
    using System.Web.Caching;
    using System.Web.UI;

    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Services.Cache;

    /// <summary>CachePageStatePersister provides a cache based page state persistence mechanism.</summary>
    public class CachePageStatePersister : PageStatePersister
    {
        private const string ViewStateCacheKey = "__VIEWSTATE_CACHEKEY";

        /// <summary>Initializes a new instance of the <see cref="CachePageStatePersister"/> class.</summary>
        /// <param name="page">The <see cref="Page"/> that the view state persistence mechanism is created for.</param>
        public CachePageStatePersister(Page page)
            : base(page)
        {
        }

        /// <summary>Loads the Page State from the Cache.</summary>
        public override void Load()
        {
            // Get the cache key from the web form data
            string key = this.Page.Request.Params[ViewStateCacheKey];

            // Abort if cache key is not available or valid
            if (string.IsNullOrEmpty(key) || !key.StartsWith("VS_"))
            {
                throw new InvalidViewStateCacheKeyException("Missing valid " + ViewStateCacheKey);
            }

            var state = DataCache.GetCache<Pair>(key);
            if (state != null)
            {
                // Set view state and control state
                this.ViewState = state.First;
                this.ControlState = state.Second;
            }

            // Remove this ViewState from the cache as it has served its purpose
            if (!this.Page.IsCallback)
            {
                DataCache.RemoveCache(key);
            }
        }

        /// <summary>Saves the Page State to the Cache.</summary>
        public override void Save()
        {
            // No processing needed if no states available
            if (this.ViewState == null && this.ControlState == null)
            {
                return;
            }

            // Generate a unique cache key
            var key = new StringBuilder();
            {
                key.Append("VS_");
                key.Append(this.Page.Session == null ? Guid.NewGuid().ToString() : this.Page.Session.SessionID);
                key.Append('_');
                key.Append(DateTime.Now.Ticks);
            }

            // Save view state and control state separately
            var state = new Pair(this.ViewState, this.ControlState);

            // Add view state and control state to cache
            DNNCacheDependency objDependency = null;
            DataCache.SetCache(key.ToString(), state, objDependency, DateTime.Now.AddMinutes(this.Page.Session.Timeout), Cache.NoSlidingExpiration, CacheItemPriority.NotRemovable, null);

            // Register hidden field to store cache key in
            this.Page.ClientScript.RegisterHiddenField(ViewStateCacheKey, key.ToString());
        }
    }
}

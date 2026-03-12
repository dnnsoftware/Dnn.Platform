// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Entities.Tabs.TabVersions
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Linq;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Data;
    using DotNetNuke.Framework;

    using Microsoft.Extensions.DependencyInjection;

    /// <summary>An <see cref="ITabVersionDetailController"/> implementation.</summary>
    /// <param name="hostSettings">The host settings.</param>
    public class TabVersionDetailController(IHostSettings hostSettings)
        : ServiceLocator<ITabVersionDetailController, TabVersionDetailController>, ITabVersionDetailController
    {
        private static readonly DataProvider Provider = DataProvider.Instance();
        private readonly IHostSettings hostSettings = hostSettings ?? Globals.GetCurrentServiceProvider().GetRequiredService<IHostSettings>();

        /// <summary>Initializes a new instance of the <see cref="TabVersionDetailController"/> class.</summary>
        [Obsolete("Deprecated in DotNetNuke 10.2.4. Please use overload with IHostSettings. Scheduled removal in v12.0.0.")]
        public TabVersionDetailController()
            : this(null)
        {
        }

        /// <inheritdoc />
        public TabVersionDetail GetTabVersionDetail(int tabVersionDetailId, int tabVersionId, bool ignoreCache = false)
        {
            return this.GetTabVersionDetails(tabVersionId, ignoreCache).SingleOrDefault(tvd => tvd.TabVersionDetailId == tabVersionDetailId);
        }

        /// <inheritdoc />
        public IEnumerable<TabVersionDetail> GetTabVersionDetails(int tabVersionId, bool ignoreCache = false)
        {
            // if we are not using the cache
            if (ignoreCache || this.hostSettings.PerformanceSetting == PerformanceSettings.NoCaching)
            {
                return CBO.FillCollection<TabVersionDetail>(Provider.GetTabVersionDetails(tabVersionId));
            }

            return CBO.GetCachedObject<List<TabVersionDetail>>(
                this.hostSettings,
                new CacheItemArgs(GetTabVersionDetailCacheKey(tabVersionId), DataCache.TabVersionDetailsCacheTimeOut, DataCache.TabVersionDetailsCachePriority),
                _ => CBO.FillCollection<TabVersionDetail>(Provider.GetTabVersionDetails(tabVersionId)));
        }

        /// <inheritdoc />
        public IEnumerable<TabVersionDetail> GetVersionHistory(int tabId, int version)
        {
            return CBO.FillCollection<TabVersionDetail>(Provider.GetTabVersionDetailsHistory(tabId, version));
        }

        /// <inheritdoc />
        public void SaveTabVersionDetail(TabVersionDetail tabVersionDetail)
        {
            this.SaveTabVersionDetail(tabVersionDetail, tabVersionDetail.CreatedByUserID, tabVersionDetail.LastModifiedByUserID);
        }

        /// <inheritdoc />
        [SuppressMessage("Microsoft.Naming", "CA1725:ParameterNamesShouldMatchBaseDeclaration", Justification = "Breaking change")]
        public void SaveTabVersionDetail(TabVersionDetail tabVersionDetail, int createdByUserID)
        {
            this.SaveTabVersionDetail(tabVersionDetail, createdByUserID, createdByUserID);
        }

        /// <inheritdoc />
        [SuppressMessage("Microsoft.Naming", "CA1725:ParameterNamesShouldMatchBaseDeclaration", Justification = "Breaking change")]
        public void SaveTabVersionDetail(TabVersionDetail tabVersionDetail, int createdByUserID, int modifiedByUserID)
        {
            tabVersionDetail.TabVersionDetailId = Provider.SaveTabVersionDetail(
                tabVersionDetail.TabVersionDetailId,
                tabVersionDetail.TabVersionId,
                tabVersionDetail.ModuleId,
                tabVersionDetail.ModuleVersion,
                tabVersionDetail.PaneName,
                tabVersionDetail.ModuleOrder,
                (int)tabVersionDetail.Action,
                createdByUserID,
                modifiedByUserID);
            this.ClearCache(tabVersionDetail.TabVersionId);
        }

        /// <inheritdoc />
        public void DeleteTabVersionDetail(int tabVersionId, int tabVersionDetailId)
        {
            Provider.DeleteTabVersionDetail(tabVersionDetailId);
            this.ClearCache(tabVersionId);
        }

        /// <inheritdoc />
        public void ClearCache(int tabVersionId)
        {
            DataCache.RemoveCache(GetTabVersionDetailCacheKey(tabVersionId));
        }

        /// <inheritdoc />
        protected override System.Func<ITabVersionDetailController> GetFactory()
        {
            return () => new TabVersionDetailController();
        }

        private static string GetTabVersionDetailCacheKey(int tabVersionId)
        {
            return string.Format(CultureInfo.InvariantCulture, DataCache.TabVersionDetailsCacheKey, tabVersionId);
        }
    }
}

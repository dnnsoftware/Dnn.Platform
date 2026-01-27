// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace Dnn.ExportImport.Components.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web.Caching;

    using Dnn.ExportImport.Components.Entities;
    using Dnn.ExportImport.Components.Interfaces;
    using Dnn.ExportImport.Components.Providers;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Framework;

    using Microsoft.Extensions.DependencyInjection;

    /// <inheritdoc cref="ISettingsController" />
    /// <seealso cref="ServiceLocator{TContract,TSelf}"/>
    public class SettingsController : ServiceLocator<ISettingsController, SettingsController>, ISettingsController
    {
        private const string CacheKey = "ExportImport_Settings";
        private const int CacheDuration = 120;
        private readonly IHostSettings hostSettings;

        /// <summary>Initializes a new instance of the <see cref="SettingsController"/> class.</summary>
        [Obsolete("Deprecated in DotNetNuke 10.2.2. Please use overload with IHostSettings. Scheduled removal in v12.0.0.")]
        public SettingsController()
            : this(null)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="SettingsController"/> class.</summary>
        /// <param name="hostSettings">The host settings.</param>
        public SettingsController(IHostSettings hostSettings)
        {
            this.hostSettings = hostSettings ?? Globals.GetCurrentServiceProvider().GetRequiredService<IHostSettings>();
        }

        /// <inheritdoc />
        public IEnumerable<ExportImportSetting> GetAllSettings()
        {
            return CBO.GetCachedObject<List<ExportImportSetting>>(
                this.hostSettings,
                new CacheItemArgs(CacheKey, CacheDuration, CacheItemPriority.Normal),
                c => CBO.FillQueryable<ExportImportSetting>(DataProvider.Instance().GetExportImportSettings()).ToList());
        }

        /// <inheritdoc />
        public ExportImportSetting GetSetting(string settingName)
        {
            return this.GetAllSettings().ToList().FirstOrDefault(x => x.SettingName == settingName);
        }

        /// <inheritdoc />
        public void AddSetting(ExportImportSetting exportImportSetting)
        {
            DataProvider.Instance().AddExportImportSetting(exportImportSetting);
            DataCache.RemoveCache(CacheKey);
        }

        /// <inheritdoc />
        protected override Func<ISettingsController> GetFactory()
        {
            return Globals.DependencyProvider.GetRequiredService<ISettingsController>;
        }
    }
}

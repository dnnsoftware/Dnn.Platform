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
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Framework;

    public class SettingsController : ServiceLocator<ISettingsController, SettingsController>, ISettingsController
    {
        private const string CacheKey = "ExportImport_Settings";
        private const int CacheDuration = 120;

        public IEnumerable<ExportImportSetting> GetAllSettings()
        {
            return CBO.GetCachedObject<List<ExportImportSetting>>(
                new CacheItemArgs(CacheKey, CacheDuration, CacheItemPriority.Normal),
                c => CBO.FillQueryable<ExportImportSetting>(DataProvider.Instance().GetExportImportSettings()).ToList());
        }

        public ExportImportSetting GetSetting(string settingName)
        {
            return this.GetAllSettings().ToList().FirstOrDefault(x => x.SettingName == settingName);
        }

        public void AddSetting(ExportImportSetting exportImportSetting)
        {
            DataProvider.Instance().AddExportImportSetting(exportImportSetting);
            DataCache.RemoveCache(CacheKey);
        }

        protected override Func<ISettingsController> GetFactory()
        {
            return () => new SettingsController();
        }
    }
}

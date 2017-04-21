using System;
using System.Collections.Generic;
using Dnn.ExportImport.Components.Entities;
using Dnn.ExportImport.Components.Interfaces;
using Dnn.ExportImport.Components.Providers;
using DotNetNuke.Framework;
using DotNetNuke.Common.Utilities;
using System.Linq;
using System.Web.Caching;
using DotNetNuke.Services.Cache;

namespace Dnn.ExportImport.Components.Controllers
{
    public class SettingsController : ServiceLocator<ISettingsController, SettingsController>, ISettingsController
    {
        private const string CacheKey = "ExportImport_Settings";
        private const int CacheDuration = 120;
        protected override Func<ISettingsController> GetFactory()
        {
            return () => new SettingsController();
        }

        public IEnumerable<ExportImportSetting> GetAllSettings()
        {
            return CBO.GetCachedObject<List<ExportImportSetting>>(new CacheItemArgs(CacheKey, CacheDuration, CacheItemPriority.NotRemovable),
                                                                c => CBO.FillQueryable<ExportImportSetting>(DataProvider.Instance().GetExportImportSettings()).ToList()).AsQueryable();
        }

        public ExportImportSetting GetSetting(string settingName)
        {
            return GetAllSettings().ToList().FirstOrDefault(x => x.SettingName == settingName);
        }
    }
}

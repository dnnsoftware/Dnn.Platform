using System.Collections.Generic;
using System.Linq;
using Dnn.ExportImport.Components.Dto;
using Dnn.ExportImport.Components.Dto.Portal;
using Dnn.ExportImport.Components.Entities;
using Dnn.ExportImport.Components.Interfaces;
using Dnn.ExportImport.Components.Models;
using DotNetNuke.Common.Utilities;
using System;
using DotNetNuke.Data;
using DotNetNuke.Entities.Modules.Communications;
using DotNetNuke.Services.Localization;
using DataProvider = Dnn.ExportImport.Components.Providers.DataProvider;

namespace Dnn.ExportImport.Components.Services
{
    /// <summary>
    /// Service to export/import portal data.
    /// </summary>
    public class PortalExportService : IPortable2
    {
        private int _progressPercentage;

        public string Category => "PORTAL";
        public string ParentCategory => null;
        public uint Priority => 1;
        public bool CanCancel => true;
        public bool CanRollback => false;

        public int ProgressPercentage
        {
            get { return _progressPercentage; }
            private set
            {
                if (value < 0) value = 0;
                else if (value > 100) value = 100;
                _progressPercentage = value;
            }
        }

        public void ExportData(ExportImportJob exportJob, ExportDto exportDto, IExportImportRepository repository,
            ExportImportResult result)
        {
            ProgressPercentage = 0;
            var portalSettings =
                CBO.FillCollection<ExportPortalSetting>(DataProvider.Instance()
                    .GetPortalSettings(exportJob.PortalId, exportDto.ExportTime?.UtcDateTime));
            repository.CreateItems(portalSettings, null);
            result.AddSummary("Exported Portal Settings", portalSettings.Count.ToString());
            ProgressPercentage += 50;

            var portalLanguages =
                CBO.FillCollection<ExportPortalLanguage>(DataProvider.Instance()
                    .GetPortalLanguages(exportJob.PortalId, exportDto.ExportTime?.UtcDateTime));
            repository.CreateItems(portalLanguages, null);
            result.AddSummary("Exported Portal Languages", portalLanguages.Count.ToString());
            ProgressPercentage += 50;

            /*
             * var portalLocalization =
                CBO.FillCollection<ExportPortalLocalization>(
                    DataProvider.Instance()
                        .GetPortalLocalizations(exportJob.PortalId, exportDto.ExportTime?.UtcDateTime));
            repository.CreateItems(portalLocalization, null);
            result.AddSummary("Exported Portal Localizations", portalLocalization.Count.ToString());
            ProgressPercentage += 40;
            */
        }

        public void ImportData(ExportImportJob importJob, ExportDto exporteDto, IExportImportRepository repository,
            ExportImportResult result)
        {
            ProgressPercentage = 0;
            var portalSettings = repository.GetAllItems<ExportPortalSetting>().ToList();
            ProcessPortalSettings(importJob, exporteDto, portalSettings, result);
            result.AddSummary("Imported Portal Settings", portalSettings.Count.ToString());
            ProgressPercentage += 50;

            ProgressPercentage = 0;
            var portalLanguages = repository.GetAllItems<ExportPortalLanguage>().ToList();
            ProcessPortalLanguages(importJob, exporteDto, portalLanguages, result);
            result.AddSummary("Imported Portal Languages", portalLanguages.Count.ToString());
            ProgressPercentage += 50;

            /*
            ProgressPercentage = 0;
            var portalLocalizations = repository.GetAllItems<ExportPortalLocalization>().ToList();
            ProcessPortalLocalizations(importJob, exporteDto, portalLocalizations, result);
            result.AddSummary("Imported Portal Localizations", portalLocalizations.Count.ToString());
            ProgressPercentage += 40;
            */
        }

        private static void ProcessPortalSettings(ExportImportJob importJob, ExportDto exporteDto,
            IEnumerable<ExportPortalSetting> portalSettings, ExportImportResult result)
        {
            using (var db = DataContext.Instance())
            {
                var repPortalSetting = db.GetRepository<ExportPortalSetting>();

                var portalId = importJob.PortalId;
                var localPortalSettings =
                    CBO.FillCollection<ExportPortalSetting>(DataProvider.Instance().GetPortalSettings(portalId, null));
                foreach (var exportPortalSetting in portalSettings)
                {
                    var createdBy = Common.Util.GetUserIdOrName(importJob, exportPortalSetting.CreatedByUserId,
                        exportPortalSetting.CreatedByUserName);
                    var modifiedBy = Common.Util.GetUserIdOrName(importJob, exportPortalSetting.LastModifiedByUserId,
                        exportPortalSetting.LastModifiedByUserName);
                    var existingPortalSetting =
                        localPortalSettings.FirstOrDefault(
                            t =>
                                t.SettingName == exportPortalSetting.SettingName &&
                                (t.CultureCode == exportPortalSetting.CultureCode ||
                                 (string.IsNullOrEmpty(t.CultureCode) &&
                                  string.IsNullOrEmpty(exportPortalSetting.CultureCode))));
                    var isUpdate = false;
                    if (existingPortalSetting != null)
                    {
                        switch (exporteDto.CollisionResolution)
                        {
                            case CollisionResolution.Overwrite:
                                isUpdate = true;
                                break;
                            case CollisionResolution.Ignore:
                                result.AddLogEntry("Ignored portal settings", exportPortalSetting.SettingName);
                                continue;
                            case CollisionResolution.Duplicate:
                                result.AddLogEntry("Ignored duplicate portal settings", exportPortalSetting.SettingName);
                                continue;
                            default:
                                throw new ArgumentOutOfRangeException(exporteDto.CollisionResolution.ToString());
                        }
                    }
                    exportPortalSetting.PortalId = portalId;
                    exportPortalSetting.LastModifiedByUserId = modifiedBy;
                    exportPortalSetting.LastModifiedOnDate = DateTime.UtcNow;
                    if (isUpdate)
                    {
                        exportPortalSetting.PortalSettingId = existingPortalSetting.PortalSettingId;
                        repPortalSetting.Update(exportPortalSetting);
                        result.AddLogEntry("Updated portal settings", exportPortalSetting.SettingName);
                    }
                    else
                    {
                        exportPortalSetting.PortalSettingId = 0;
                        exportPortalSetting.CreatedByUserId = createdBy;
                        exportPortalSetting.CreatedOnDate = DateTime.UtcNow;
                        repPortalSetting.Insert(exportPortalSetting);
                        result.AddLogEntry("Added portal settings", exportPortalSetting.SettingName);
                    }
                    exportPortalSetting.LocalId = exportPortalSetting.PortalSettingId;
                }
            }
        }

        private static void ProcessPortalLanguages(ExportImportJob importJob, ExportDto exporteDto,
            IEnumerable<ExportPortalLanguage> portalLanguages, ExportImportResult result)
        {
            using (var db = DataContext.Instance())
            {
                var repPortalLanguage = db.GetRepository<ExportPortalLanguage>();

                var portalId = importJob.PortalId;
                var localPortalLanguages =
                    CBO.FillCollection<ExportPortalLanguage>(DataProvider.Instance().GetPortalLanguages(portalId, null));
                var localLanguages = CBO.FillCollection<Locale>(DotNetNuke.Data.DataProvider.Instance().GetLanguages());
                foreach (var exportPortalLanguage in portalLanguages)
                {
                    var createdBy = Common.Util.GetUserIdOrName(importJob, exportPortalLanguage.CreatedByUserId,
                        exportPortalLanguage.CreatedByUserName);
                    var modifiedBy = Common.Util.GetUserIdOrName(importJob, exportPortalLanguage.LastModifiedByUserId,
                        exportPortalLanguage.LastModifiedByUserName);
                    var localLanguageId =
                        localLanguages.FirstOrDefault(x => x.Code == exportPortalLanguage.CultureCode)?.LanguageId;
                    var existingPortalLanguage =
                        localPortalLanguages.FirstOrDefault(
                            t =>
                                t.LanguageId == localLanguageId);
                    var isUpdate = false;
                    if (existingPortalLanguage != null)
                    {
                        switch (exporteDto.CollisionResolution)
                        {
                            case CollisionResolution.Overwrite:
                                isUpdate = true;
                                break;
                            case CollisionResolution.Ignore:
                                result.AddLogEntry("Ignored portal language", exportPortalLanguage.CultureCode);
                                continue;
                            case CollisionResolution.Duplicate:
                                result.AddLogEntry("Ignored duplicate portal language", exportPortalLanguage.CultureCode);
                                continue;
                            default:
                                throw new ArgumentOutOfRangeException(exporteDto.CollisionResolution.ToString());
                        }
                    }
                    exportPortalLanguage.PortalId = portalId;
                    exportPortalLanguage.LastModifiedByUserId = modifiedBy;
                    exportPortalLanguage.LastModifiedOnDate = DateTime.UtcNow;
                    if (isUpdate)
                    {
                        exportPortalLanguage.PortalLanguageId = existingPortalLanguage.PortalLanguageId;
                        repPortalLanguage.Update(exportPortalLanguage);
                        result.AddLogEntry("Updated portal language", exportPortalLanguage.CultureCode);
                    }
                    else
                    {
                        exportPortalLanguage.PortalLanguageId = 0;
                        exportPortalLanguage.CreatedByUserId = createdBy;
                        exportPortalLanguage.CreatedOnDate = DateTime.UtcNow;
                        repPortalLanguage.Insert(exportPortalLanguage);
                        result.AddLogEntry("Added portal language", exportPortalLanguage.CultureCode);

                    }
                    exportPortalLanguage.LocalId = exportPortalLanguage.PortalLanguageId;
                }
            }
        }

#if false
        private static void ProcessPortalLocalizations(ExportImportJob importJob, ExportDto exporteDto,
           IEnumerable<ExportPortalLocalization> portalLocalizations, ExportImportResult result)
        {
            using (var db = DataContext.Instance())
            {
                var repPortalLocalization = db.GetRepository<ExportPortalLocalization>();

                var portalId = importJob.PortalId;
                var localPortalLocalizations =
                    CBO.FillCollection<ExportPortalLocalization>(DataProvider.Instance().GetPortalLocalizations(portalId, null));
                foreach (var exportPortalLocalization in portalLocalizations)
                {
                    var createdBy = Common.Util.GetUserIdOrName(importJob, exportPortalLocalization.CreatedByUserId,
                        exportPortalLocalization.CreatedByUserName);
                    var modifiedBy = Common.Util.GetUserIdOrName(importJob, exportPortalLocalization.LastModifiedByUserId,
                        exportPortalLocalization.LastModifiedByUserName);
                    var existingPortalLocalization =
                        localPortalLocalizations.FirstOrDefault(
                            t =>
                                t.CultureCode == exportPortalLocalization.CultureCode);
                    var isUpdate = false;
                    if (existingPortalLocalization != null)
                    {
                        switch (exporteDto.CollisionResolution)
                        {
                            case CollisionResolution.Overwrite:
                                isUpdate = true;
                                break;
                            case CollisionResolution.Ignore:
                                result.AddLogEntry("Ignored portal localization", exportPortalLocalization.CultureCode);
                                continue;
                            case CollisionResolution.Duplicate:
                                result.AddLogEntry("Ignored duplicate portal localization", exportPortalLocalization.CultureCode);
                                continue;
                            default:
                                throw new ArgumentOutOfRangeException(exporteDto.CollisionResolution.ToString());
                        }
                    }
                    exportPortalLocalization.PortalId = portalId;
                    exportPortalLocalization.LastModifiedByUserId = modifiedBy;
                    exportPortalLocalization.LastModifiedOnDate = DateTime.UtcNow;
                    if (isUpdate)
                    {
                        exportPortalLocalization.PortalLocalizationId = existingPortalLocalization.PortalLocalizationId;
                        repPortalLocalization.Update(exportPortalLocalization);
                        result.AddLogEntry("Updated portal localization", exportPortalLocalization.CultureCode);
                    }
                    else
                    {
                        exportPortalLocalization.PortalLocalizationId = 0;
                        exportPortalLocalization.CreatedByUserId = createdBy;
                        exportPortalLocalization.CreatedOnDate = DateTime.UtcNow;
                        repPortalLocalization.Insert(exportPortalLocalization);
                        result.AddLogEntry("Added portal localization", exportPortalLocalization.CultureCode);
                    }
                    exportPortalLocalization.LocalId = exportPortalLocalization.PortalLocalizationId;
                }
            }
        }
#endif
    }
}
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
            var portalSettings = CBO.FillCollection<ExportPortalSetting>(DataProvider.Instance().GetPortalSettings(exportJob.PortalId, exportDto.ExportTime?.UtcDateTime));
            repository.CreateItems(portalSettings, null);
            result.AddSummary("Exported Portal Settings", portalSettings.Count.ToString());
            ProgressPercentage += 40;

            var portalLanguages = CBO.FillCollection<ExportPortalLanguage>(DataProvider.Instance().GetPortalLanguages(exportJob.PortalId, exportDto.ExportTime?.UtcDateTime));
            repository.CreateItems(portalLanguages, null);
            result.AddSummary("Exported Portal Languages", portalLanguages.Count.ToString());
            ProgressPercentage += 30;

            var portalLocalization = CBO.FillCollection<ExportPortalLocalization>(DataProvider.Instance().GetPortalLocalizations(exportJob.PortalId, exportDto.ExportTime?.UtcDateTime));
            repository.CreateItems(portalLocalization, null);
            result.AddSummary("Exported Portal Localizations", portalLocalization.Count.ToString());
            ProgressPercentage += 40;
        }

        public void ImportData(ExportImportJob importJob, ExportDto exporteDto,  IExportImportRepository repository,
            ExportImportResult result)
        {
            ProgressPercentage = 0;
            var portalSettings = repository.GetAllItems<ExportPortalSetting>().ToList();
            ProcessPortalSettings(importJob, exporteDto, portalSettings);
            result.AddSummary("Imported Portal Settings", portalSettings.Count.ToString());
            ProgressPercentage += 40;
        }

        private static void ProcessPortalSettings(ExportImportJob importJob, ExportDto exporteDto,
            IEnumerable<ExportPortalSetting> portalSettings)
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
                                t.CultureCode == exportPortalSetting.CultureCode);
                    var isUpdate = false;
                    if (existingPortalSetting != null)
                    {
                        switch (exporteDto.CollisionResolution)
                        {
                            case CollisionResolution.Ignore:
                                isUpdate = true;
                                break;
                            case CollisionResolution.Overwrite:
                            case CollisionResolution.Duplicate:
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
                    }
                    else
                    {
                        exportPortalSetting.PortalSettingId = 0;
                        exportPortalSetting.CreatedByUserId = createdBy;
                        exportPortalSetting.CreatedOnDate = DateTime.UtcNow;
                        repPortalSetting.Insert(exportPortalSetting);
                    }
                    exportPortalSetting.LocalId = exportPortalSetting.PortalSettingId;
                }
            }
        }
    }
}
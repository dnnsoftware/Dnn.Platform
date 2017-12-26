#region Copyright
//
// DotNetNuke® - http://www.dnnsoftware.com
// Copyright (c) 2002-2017
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

using System.Collections.Generic;
using System.Linq;
using Dnn.ExportImport.Components.Dto;
using Dnn.ExportImport.Components.Entities;
using DotNetNuke.Common.Utilities;
using System;
using Dnn.ExportImport.Components.Common;
using Dnn.ExportImport.Components.Controllers;
using Dnn.ExportImport.Dto.Portal;
using DotNetNuke.Services.Localization;
using DataProvider = Dnn.ExportImport.Components.Providers.DataProvider;

namespace Dnn.ExportImport.Components.Services
{
    /// <summary>
    /// Service to export/import portal data.
    /// </summary>
    public class PortalExportService : BasePortableService
    {
        public override string Category => Constants.Category_Portal;

        public override string ParentCategory => null;

        public override uint Priority => 1;

        public override void ExportData(ExportImportJob exportJob, ExportDto exportDto)
        {
            var fromDate = (exportDto.FromDateUtc ?? Constants.MinDbTime).ToLocalTime();
            var toDate = exportDto.ToDateUtc.ToLocalTime();
            if (CheckPoint.Stage > 1) return;
            if (CheckCancelled(exportJob)) return;
            List<ExportPortalLanguage> portalLanguages = null;
            if (CheckPoint.Stage == 0)
            {
                var portalSettings = new List<ExportPortalSetting>();
                var settingToMigrate =
                    SettingsController.Instance.GetSetting(Constants.PortalSettingExportKey)?.SettingValue?.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

                if (settingToMigrate != null)
                {
                    portalSettings = CBO.FillCollection<ExportPortalSetting>(DataProvider.Instance().GetPortalSettings(exportJob.PortalId, toDate, fromDate));

                    //Migrate only allowed portal settings.
                    portalSettings =
                        portalSettings.Where(x => settingToMigrate.Any(setting => setting.Trim().Equals(x.SettingName, StringComparison.InvariantCultureIgnoreCase))).ToList();

                    //Update the total items count in the check points. This should be updated only once.
                    CheckPoint.TotalItems = CheckPoint.TotalItems <= 0 ? portalSettings.Count : CheckPoint.TotalItems;
                    if (CheckPoint.TotalItems == portalSettings.Count)
                    {
                        portalLanguages =
                            CBO.FillCollection<ExportPortalLanguage>(
                                DataProvider.Instance().GetPortalLanguages(exportJob.PortalId, toDate, fromDate));
                        CheckPoint.TotalItems += portalLanguages.Count;
                    }
                    CheckPointStageCallback(this);

                    Repository.CreateItems(portalSettings);
                }
                Result.AddSummary("Exported Portal Settings", portalSettings.Count.ToString());

                CheckPoint.Progress = 50;
                CheckPoint.ProcessedItems = portalSettings.Count;
                CheckPoint.Stage++;
                if (CheckPointStageCallback(this)) return;
            }

            if (CheckPoint.Stage == 1)
            {
                if (CheckCancelled(exportJob)) return;
                if (portalLanguages == null)
                    portalLanguages = CBO.FillCollection<ExportPortalLanguage>(DataProvider.Instance()
                        .GetPortalLanguages(exportJob.PortalId, toDate, fromDate));

                Repository.CreateItems(portalLanguages);
                Result.AddSummary("Exported Portal Languages", portalLanguages.Count.ToString());
                CheckPoint.Progress = 100;
                CheckPoint.Completed = true;
                CheckPoint.Stage++;
                CheckPoint.ProcessedItems += portalLanguages.Count;
                CheckPointStageCallback(this);
            }
        }

        public override void ImportData(ExportImportJob importJob, ImportDto importDto)
        {
            //Update the total items count in the check points. This should be updated only once.
            CheckPoint.TotalItems = CheckPoint.TotalItems = CheckPoint.TotalItems <= 0 ? GetImportTotal() : CheckPoint.TotalItems;
            if (CheckPointStageCallback(this)) return;
            if (CheckPoint.Stage > 1) return;
            if (CheckPoint.Stage == 0)
            {
                var portalSettings = Repository.GetAllItems<ExportPortalSetting>().ToList();
                ProcessPortalSettings(importJob, importDto, portalSettings);
                CheckPoint.TotalItems = GetImportTotal();
                Result.AddSummary("Imported Portal Settings", portalSettings.Count.ToString());
                CheckPoint.Progress += 50;
                CheckPoint.Stage++;
                CheckPoint.ProcessedItems = portalSettings.Count;
                if (CheckPointStageCallback(this)) return;
            }
            if (CheckPoint.Stage == 1)
            {
                var portalLanguages = Repository.GetAllItems<ExportPortalLanguage>().ToList();
                ProcessPortalLanguages(importJob, importDto, portalLanguages);
                Result.AddSummary("Imported Portal Languages", portalLanguages.Count.ToString());
                CheckPoint.Progress += 50;
                CheckPoint.Completed = true;
                CheckPoint.Stage++;
                CheckPoint.ProcessedItems += portalLanguages.Count;
                CheckPointStageCallback(this);
            }
            /*
            ProgressPercentage = 0;
            var portalLocalizations = Repository.GetAllItems<ExportPortalLocalization>().ToList();
            ProcessPortalLocalizations(importJob, importDto, portalLocalizations);
            Result.AddSummary("Imported Portal Localizations", portalLocalizations.Count.ToString());
            ProgressPercentage += 40;
            */
        }

        public override int GetImportTotal()
        {
            return Repository.GetCount<ExportPortalSetting>() + Repository.GetCount<ExportPortalLanguage>();
        }

        private void ProcessPortalSettings(ExportImportJob importJob, ImportDto importDto,
            IEnumerable<ExportPortalSetting> portalSettings)
        {
            var portalId = importJob.PortalId;
            var localPortalSettings =
                CBO.FillCollection<ExportPortalSetting>(DataProvider.Instance().GetPortalSettings(portalId, DateUtils.GetDatabaseUtcTime().AddYears(1), null));
            foreach (var exportPortalSetting in portalSettings)
            {
                if (CheckCancelled(importJob)) return;

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
                    switch (importDto.CollisionResolution)
                    {
                        case CollisionResolution.Overwrite:
                            isUpdate = true;
                            break;
                        case CollisionResolution.Ignore:
                            Result.AddLogEntry("Ignored portal settings", exportPortalSetting.SettingName);
                            continue;
                        default:
                            throw new ArgumentOutOfRangeException(importDto.CollisionResolution.ToString());
                    }
                }
                if (isUpdate)
                {
                    var modifiedBy = Util.GetUserIdByName(importJob, exportPortalSetting.LastModifiedByUserId,
                     exportPortalSetting.LastModifiedByUserName);

                    exportPortalSetting.PortalSettingId = existingPortalSetting.PortalSettingId;
                    DotNetNuke.Data.DataProvider.Instance()
                        .UpdatePortalSetting(importJob.PortalId, exportPortalSetting.SettingName,
                            exportPortalSetting.SettingValue, modifiedBy, exportPortalSetting.CultureCode, exportPortalSetting.IsSecure);
                    Result.AddLogEntry("Updated portal settings", exportPortalSetting.SettingName);
                }
                else
                {
                    var createdBy = Util.GetUserIdByName(importJob, exportPortalSetting.CreatedByUserId, exportPortalSetting.CreatedByUserName);

                    DotNetNuke.Data.DataProvider.Instance()
                        .UpdatePortalSetting(importJob.PortalId, exportPortalSetting.SettingName,
                            exportPortalSetting.SettingValue, createdBy, exportPortalSetting.CultureCode, exportPortalSetting.IsSecure);

                    Result.AddLogEntry("Added portal settings", exportPortalSetting.SettingName);
                }
            }
        }

        private void ProcessPortalLanguages(ExportImportJob importJob, ImportDto importDto,
            IEnumerable<ExportPortalLanguage> portalLanguages)
        {
            var portalId = importJob.PortalId;
            var localPortalLanguages =
                CBO.FillCollection<ExportPortalLanguage>(DataProvider.Instance().GetPortalLanguages(portalId, DateUtils.GetDatabaseUtcTime().AddYears(1), null));
            var localLanguages = CBO.FillCollection<Locale>(DotNetNuke.Data.DataProvider.Instance().GetLanguages());
            foreach (var exportPortalLanguage in portalLanguages)
            {
                if (CheckCancelled(importJob)) return;
                var localLanguageId =
                    localLanguages.FirstOrDefault(x => x.Code == exportPortalLanguage.CultureCode)?.LanguageId;
                var existingPortalLanguage =
                    localPortalLanguages.FirstOrDefault(
                        t =>
                            t.LanguageId == localLanguageId);
                var isUpdate = false;
                if (existingPortalLanguage != null)
                {
                    switch (importDto.CollisionResolution)
                    {
                        case CollisionResolution.Overwrite:
                            isUpdate = true;
                            break;
                        case CollisionResolution.Ignore:
                            Result.AddLogEntry("Ignored portal language", exportPortalLanguage.CultureCode);
                            continue;
                        default:
                            throw new ArgumentOutOfRangeException(importDto.CollisionResolution.ToString());
                    }
                }
                if (isUpdate)
                {
                    var modifiedBy = Util.GetUserIdByName(importJob, exportPortalLanguage.LastModifiedByUserId, exportPortalLanguage.LastModifiedByUserName);

                    DotNetNuke.Data.DataProvider.Instance()
                        .UpdatePortalLanguage(importJob.PortalId, exportPortalLanguage.LanguageId,
                            exportPortalLanguage.IsPublished, modifiedBy);

                    Result.AddLogEntry("Updated portal language", exportPortalLanguage.CultureCode);
                }
                else
                {
                    var createdBy = Util.GetUserIdByName(importJob, exportPortalLanguage.CreatedByUserId, exportPortalLanguage.CreatedByUserName);

                    exportPortalLanguage.PortalLanguageId = DotNetNuke.Data.DataProvider.Instance()
                        .AddPortalLanguage(importJob.PortalId, exportPortalLanguage.LanguageId,
                            exportPortalLanguage.IsPublished, createdBy);
                    Result.AddLogEntry("Added portal language", exportPortalLanguage.CultureCode);
                }
            }
        }
    }
}
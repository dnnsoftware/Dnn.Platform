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
using Dnn.ExportImport.Components.Dto.Portal;
using Dnn.ExportImport.Components.Entities;
using DotNetNuke.Common.Utilities;
using System;
using Dnn.ExportImport.Components.Common;
using DotNetNuke.Data;
using DotNetNuke.Services.Localization;
using DataProvider = Dnn.ExportImport.Components.Providers.DataProvider;

namespace Dnn.ExportImport.Components.Services
{
    /// <summary>
    /// Service to export/import portal data.
    /// </summary>
    public class PortalExportService : Portable2Base
    {
        public override string Category => Constants.Category_Portal;

        public override string ParentCategory => null;

        public override uint Priority => 1;

        public override void ExportData(ExportImportJob exportJob, ExportDto exportDto)
        {
            var sinceDate = exportDto.SinceTime?.DateTime;
            var tillDate = exportJob.CreatedOnDate;
            if (CheckPoint.Stage > 1) return;
            if (CheckCancelled(exportJob)) return;

            if (CheckPoint.Stage == 0)
            {
                var portalSettings = CBO.FillCollection<ExportPortalSetting>(DataProvider.Instance()
                    .GetPortalSettings(exportJob.PortalId, tillDate, sinceDate));
                Repository.CreateItems(portalSettings, null);
                Result.AddSummary("Exported Portal Settings", portalSettings.Count.ToString());
                CheckPoint.Progress = 50;
                CheckPoint.Stage++;
                if (CheckPointStageCallback(this)) return;
            }

            if (CheckPoint.Stage == 1)
            {
                if (CheckCancelled(exportJob)) return;
                var portalLanguages = CBO.FillCollection<ExportPortalLanguage>(DataProvider.Instance()
                    .GetPortalLanguages(exportJob.PortalId, tillDate, sinceDate));
                Repository.CreateItems(portalLanguages, null);
                Result.AddSummary("Exported Portal Languages", portalLanguages.Count.ToString());
                CheckPoint.Progress = 100;
                CheckPoint.Stage++;
                CheckPointStageCallback(this);
            }

            /*
            if (CheckPoint.Stage == 2)
            {
                if (CancellationToken.IsCancellationRequested) return;
                var portalLocalization =
                   CBO.FillCollection<ExportPortalLocalization>(
                       DataProvider.Instance()
                           .GetPortalLocalizations(exportJob.PortalId,tillDate, sinceDate));
                Repository.CreateItems(portalLocalization, null);
                Result.AddSummary("Exported Portal Localizations", portalLocalization.Count.ToString());
                ProgressPercentage += 40;

                CheckPoint.Stage++;
                CheckPointStageCallback(this);
            }
             */
        }

        public override void ImportData(ExportImportJob importJob, ExportDto exportDto)
        {
            var portalSettings = Repository.GetAllItems<ExportPortalSetting>().ToList();
            ProcessPortalSettings(importJob, exportDto, portalSettings);
            Result.AddSummary("Imported Portal Settings", portalSettings.Count.ToString());
            CheckPoint.Progress += 50;

            var portalLanguages = Repository.GetAllItems<ExportPortalLanguage>().ToList();
            ProcessPortalLanguages(importJob, exportDto, portalLanguages);
            Result.AddSummary("Imported Portal Languages", portalLanguages.Count.ToString());
            CheckPoint.Progress += 50;

            /*
            ProgressPercentage = 0;
            var portalLocalizations = Repository.GetAllItems<ExportPortalLocalization>().ToList();
            ProcessPortalLocalizations(importJob, exportDto, portalLocalizations);
            Result.AddSummary("Imported Portal Localizations", portalLocalizations.Count.ToString());
            ProgressPercentage += 40;
            */
        }

        private void ProcessPortalSettings(ExportImportJob importJob, ExportDto exportDto,
            IEnumerable<ExportPortalSetting> portalSettings)
        {
            using (var db = DataContext.Instance())
            {
                var repPortalSetting = db.GetRepository<ExportPortalSetting>();

                var portalId = importJob.PortalId;
                var localPortalSettings =
                    CBO.FillCollection<ExportPortalSetting>(DataProvider.Instance().GetPortalSettings(portalId, DateTime.UtcNow.AddYears(1), null));
                foreach (var exportPortalSetting in portalSettings)
                {
                    if (CheckCancelled(importJob)) return;
                    var createdBy = Util.GetUserIdOrName(importJob, exportPortalSetting.CreatedByUserId,
                        exportPortalSetting.CreatedByUserName);
                    var modifiedBy = Util.GetUserIdOrName(importJob, exportPortalSetting.LastModifiedByUserId,
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
                        switch (exportDto.CollisionResolution)
                        {
                            case CollisionResolution.Overwrite:
                                isUpdate = true;
                                break;
                            case CollisionResolution.Ignore:
                                Result.AddLogEntry("Ignored portal settings", exportPortalSetting.SettingName);
                                continue;
                            case CollisionResolution.Duplicate:
                                Result.AddLogEntry("Ignored duplicate portal settings", exportPortalSetting.SettingName);
                                continue;
                            default:
                                throw new ArgumentOutOfRangeException(exportDto.CollisionResolution.ToString());
                        }
                    }
                    exportPortalSetting.PortalId = portalId;
                    exportPortalSetting.LastModifiedByUserId = modifiedBy;
                    exportPortalSetting.LastModifiedOnDate = DateTime.UtcNow;
                    if (isUpdate)
                    {
                        exportPortalSetting.PortalSettingId = existingPortalSetting.PortalSettingId;
                        repPortalSetting.Update(exportPortalSetting);
                        Result.AddLogEntry("Updated portal settings", exportPortalSetting.SettingName);
                    }
                    else
                    {
                        exportPortalSetting.PortalSettingId = 0;
                        exportPortalSetting.CreatedByUserId = createdBy;
                        exportPortalSetting.CreatedOnDate = DateTime.UtcNow;
                        repPortalSetting.Insert(exportPortalSetting);
                        Result.AddLogEntry("Added portal settings", exportPortalSetting.SettingName);
                    }
                    exportPortalSetting.LocalId = exportPortalSetting.PortalSettingId;
                }
            }
        }

        private void ProcessPortalLanguages(ExportImportJob importJob, ExportDto exportDto,
            IEnumerable<ExportPortalLanguage> portalLanguages)
        {
            using (var db = DataContext.Instance())
            {
                var repPortalLanguage = db.GetRepository<ExportPortalLanguage>();

                var portalId = importJob.PortalId;
                var localPortalLanguages =
                    CBO.FillCollection<ExportPortalLanguage>(DataProvider.Instance().GetPortalLanguages(portalId, DateTime.UtcNow.AddYears(1), null));
                var localLanguages = CBO.FillCollection<Locale>(DotNetNuke.Data.DataProvider.Instance().GetLanguages());
                foreach (var exportPortalLanguage in portalLanguages)
                {
                    if (CheckCancelled(importJob)) return;
                    var createdBy = Util.GetUserIdOrName(importJob, exportPortalLanguage.CreatedByUserId,
                        exportPortalLanguage.CreatedByUserName);
                    var modifiedBy = Util.GetUserIdOrName(importJob, exportPortalLanguage.LastModifiedByUserId,
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
                        switch (exportDto.CollisionResolution)
                        {
                            case CollisionResolution.Overwrite:
                                isUpdate = true;
                                break;
                            case CollisionResolution.Ignore:
                                Result.AddLogEntry("Ignored portal language", exportPortalLanguage.CultureCode);
                                continue;
                            case CollisionResolution.Duplicate:
                                Result.AddLogEntry("Ignored duplicate portal language", exportPortalLanguage.CultureCode);
                                continue;
                            default:
                                throw new ArgumentOutOfRangeException(exportDto.CollisionResolution.ToString());
                        }
                    }
                    exportPortalLanguage.PortalId = portalId;
                    exportPortalLanguage.LastModifiedByUserId = modifiedBy;
                    exportPortalLanguage.LastModifiedOnDate = DateTime.UtcNow;
                    if (isUpdate)
                    {
                        exportPortalLanguage.PortalLanguageId = existingPortalLanguage.PortalLanguageId;
                        repPortalLanguage.Update(exportPortalLanguage);
                        Result.AddLogEntry("Updated portal language", exportPortalLanguage.CultureCode);
                    }
                    else
                    {
                        exportPortalLanguage.PortalLanguageId = 0;
                        exportPortalLanguage.CreatedByUserId = createdBy;
                        exportPortalLanguage.CreatedOnDate = DateTime.UtcNow;
                        repPortalLanguage.Insert(exportPortalLanguage);
                        Result.AddLogEntry("Added portal language", exportPortalLanguage.CultureCode);

                    }
                    exportPortalLanguage.LocalId = exportPortalLanguage.PortalLanguageId;
                }
            }
        }

#if false
        private static void ProcessPortalLocalizations(ExportImportJob importJob, ExportDto exportDto,
           IEnumerable<ExportPortalLocalization> portalLocalizations, ExportImportResult result)
        {
            using (var db = DataContext.Instance())
            {
                var repPortalLocalization = db.GetRepository<ExportPortalLocalization>();

                var portalId = importJob.PortalId;
                var localPortalLocalizations =
                    CBO.FillCollection<ExportPortalLocalization>(DataProvider.Instance().GetPortalLocalizations(portalId,DateTime.UtcNow.AddYears(1), null));
                foreach (var exportPortalLocalization in portalLocalizations)
                {
                    if (CancellationToken.IsCancellationRequested) return;
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
                        switch (exportDto.CollisionResolution)
                        {
                            case CollisionResolution.Overwrite:
                                isUpdate = true;
                                break;
                            case CollisionResolution.Ignore:
                                Result.AddLogEntry("Ignored portal localization", exportPortalLocalization.CultureCode);
                                continue;
                            case CollisionResolution.Duplicate:
                                Result.AddLogEntry("Ignored duplicate portal localization", exportPortalLocalization.CultureCode);
                                continue;
                            default:
                                throw new ArgumentOutOfRangeException(exportDto.CollisionResolution.ToString());
                        }
                    }
                    exportPortalLocalization.PortalId = portalId;
                    exportPortalLocalization.LastModifiedByUserId = modifiedBy;
                    exportPortalLocalization.LastModifiedOnDate = DateTime.UtcNow;
                    if (isUpdate)
                    {
                        exportPortalLocalization.PortalLocalizationId = existingPortalLocalization.PortalLocalizationId;
                        repPortalLocalization.Update(exportPortalLocalization);
                        Result.AddLogEntry("Updated portal localization", exportPortalLocalization.CultureCode);
                    }
                    else
                    {
                        exportPortalLocalization.PortalLocalizationId = 0;
                        exportPortalLocalization.CreatedByUserId = createdBy;
                        exportPortalLocalization.CreatedOnDate = DateTime.UtcNow;
                        repPortalLocalization.Insert(exportPortalLocalization);
                        Result.AddLogEntry("Added portal localization", exportPortalLocalization.CultureCode);
                    }
                    exportPortalLocalization.LocalId = exportPortalLocalization.PortalLocalizationId;
                }
            }
        }
#endif
    }
}
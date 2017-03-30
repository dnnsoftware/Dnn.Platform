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
    public class PortalExportService : BasePortableService
    {
        public override string Category => Constants.Category_Portal;

        public override string ParentCategory => null;

        public override uint Priority => 1;

        public override void ExportData(ExportImportJob exportJob, ExportDto exportDto)
        {
            var fromDate = exportDto.FromDate?.DateTime;
            var toDate = exportDto.ToDate;
            if (CheckPoint.Stage > 1) return;
            if (CheckCancelled(exportJob)) return;
            List<ExportPortalLanguage> portalLanguages = null;
            if (CheckPoint.Stage == 0)
            {
                var portalSettings = CBO.FillCollection<ExportPortalSetting>(DataProvider.Instance()
                    .GetPortalSettings(exportJob.PortalId, toDate, fromDate));
                
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

                Repository.CreateItems(portalSettings, null);
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

                Repository.CreateItems(portalLanguages, null);
                Result.AddSummary("Exported Portal Languages", portalLanguages.Count.ToString());
                CheckPoint.Progress = 100;
                CheckPoint.Stage++;
                CheckPoint.ProcessedItems += portalLanguages.Count;
                CheckPointStageCallback(this);
            }

            /*
            if (CheckPoint.Stage == 2)
            {
                if (CancellationToken.IsCancellationRequested) return;
                var portalLocalization =
                   CBO.FillCollection<ExportPortalLocalization>(
                       DataProvider.Instance()
                           .GetPortalLocalizations(exportJob.PortalId,toDate, fromDate));
                Repository.CreateItems(portalLocalization, null);
                Result.AddSummary("Exported Portal Localizations", portalLocalization.Count.ToString());
                ProgressPercentage += 40;

                CheckPoint.Stage++;
                CheckPointStageCallback(this);
            }
             */
        }

        public override void ImportData(ExportImportJob importJob, ImportDto importDto)
        {
            //Update the total items count in the check points. This should be updated only once.
            CheckPoint.TotalItems = CheckPoint.TotalItems = CheckPoint.TotalItems <= 0 ? GetImportTotal() : CheckPoint.TotalItems;
            if (CheckPointStageCallback(this)) return;
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
            using (var db = DataContext.Instance())
            {
                var repPortalSetting = db.GetRepository<ExportPortalSetting>();

                var portalId = importJob.PortalId;
                var localPortalSettings =
                    CBO.FillCollection<ExportPortalSetting>(DataProvider.Instance().GetPortalSettings(portalId, DateUtils.GetDatabaseTime().AddYears(1), null));
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
                    exportPortalSetting.PortalId = portalId;
                    exportPortalSetting.LastModifiedByUserId = modifiedBy;
                    exportPortalSetting.LastModifiedOnDate = DateUtils.GetDatabaseTime();
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
                        exportPortalSetting.CreatedOnDate = DateUtils.GetDatabaseTime();
                        repPortalSetting.Insert(exportPortalSetting);
                        Result.AddLogEntry("Added portal settings", exportPortalSetting.SettingName);
                    }
                    exportPortalSetting.LocalId = exportPortalSetting.PortalSettingId;
                }
            }
        }

        private void ProcessPortalLanguages(ExportImportJob importJob, ImportDto importDto,
            IEnumerable<ExportPortalLanguage> portalLanguages)
        {
            using (var db = DataContext.Instance())
            {
                var repPortalLanguage = db.GetRepository<ExportPortalLanguage>();

                var portalId = importJob.PortalId;
                var localPortalLanguages =
                    CBO.FillCollection<ExportPortalLanguage>(DataProvider.Instance().GetPortalLanguages(portalId, DateUtils.GetDatabaseTime().AddYears(1), null));
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
                    exportPortalLanguage.PortalId = portalId;
                    exportPortalLanguage.LastModifiedByUserId = modifiedBy;
                    exportPortalLanguage.LastModifiedOnDate = DateUtils.GetDatabaseTime();
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
                        exportPortalLanguage.CreatedOnDate = DateUtils.GetDatabaseTime();
                        repPortalLanguage.Insert(exportPortalLanguage);
                        Result.AddLogEntry("Added portal language", exportPortalLanguage.CultureCode);

                    }
                    exportPortalLanguage.LocalId = exportPortalLanguage.PortalLanguageId;
                }
            }
        }

#if false
        private static void ProcessPortalLocalizations(ExportImportJob importJob, ImportDto importDto,
           IEnumerable<ExportPortalLocalization> portalLocalizations, ExportImportResult result)
        {
            using (var db = DataContext.Instance())
            {
                var repPortalLocalization = db.GetRepository<ExportPortalLocalization>();

                var portalId = importJob.PortalId;
                var localPortalLocalizations =
                    CBO.FillCollection<ExportPortalLocalization>(DataProvider.Instance().GetPortalLocalizations(portalId,DateUtils.GetDatabaseTime().AddYears(1), null));
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
                        switch (importDto.CollisionResolution)
                        {
                            case CollisionResolution.Overwrite:
                                isUpdate = true;
                                break;
                            case CollisionResolution.Ignore:
                                Result.AddLogEntry("Ignored portal localization", exportPortalLocalization.CultureCode);
                                continue;
                            default:
                                throw new ArgumentOutOfRangeException(importDto.CollisionResolution.ToString());
                        }
                    }
                    exportPortalLocalization.PortalId = portalId;
                    exportPortalLocalization.LastModifiedByUserId = modifiedBy;
                    exportPortalLocalization.LastModifiedOnDate = DateUtils.GetDatabaseTime();
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
                        exportPortalLocalization.CreatedOnDate = DateUtils.GetDatabaseTime();
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
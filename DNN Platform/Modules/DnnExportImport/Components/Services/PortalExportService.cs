// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.ExportImport.Components.Services
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;

    using Dnn.ExportImport.Components.Common;
    using Dnn.ExportImport.Components.Controllers;
    using Dnn.ExportImport.Components.Dto;
    using Dnn.ExportImport.Components.Entities;
    using Dnn.ExportImport.Dto.Portal;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Services.Localization;

    using DataProvider = Dnn.ExportImport.Components.Providers.DataProvider;

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
            if (this.CheckPoint.Stage > 1)
            {
                return;
            }

            if (this.CheckCancelled(exportJob))
            {
                return;
            }

            List<ExportPortalLanguage> portalLanguages = null;
            if (this.CheckPoint.Stage == 0)
            {
                var portalSettings = new List<ExportPortalSetting>();
                var settingToMigrate =
                    SettingsController.Instance.GetSetting(Constants.PortalSettingExportKey)?.SettingValue?.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

                if (settingToMigrate != null)
                {
                    portalSettings = CBO.FillCollection<ExportPortalSetting>(DataProvider.Instance().GetPortalSettings(exportJob.PortalId, toDate, fromDate));

                    // Migrate only allowed portal settings.
                    portalSettings =
                        portalSettings.Where(x => settingToMigrate.Any(setting => setting.Trim().Equals(x.SettingName, StringComparison.InvariantCultureIgnoreCase))).ToList();

                    // Update the total items count in the check points. This should be updated only once.
                    this.CheckPoint.TotalItems = this.CheckPoint.TotalItems <= 0 ? portalSettings.Count : this.CheckPoint.TotalItems;
                    if (this.CheckPoint.TotalItems == portalSettings.Count)
                    {
                        portalLanguages =
                            CBO.FillCollection<ExportPortalLanguage>(
                                DataProvider.Instance().GetPortalLanguages(exportJob.PortalId, toDate, fromDate));
                        this.CheckPoint.TotalItems += portalLanguages.Count;
                    }

                    this.CheckPointStageCallback(this);

                    this.Repository.CreateItems(portalSettings);
                }

                this.Result.AddSummary("Exported Portal Settings", portalSettings.Count.ToString());

                this.CheckPoint.Progress = 50;
                this.CheckPoint.ProcessedItems = portalSettings.Count;
                this.CheckPoint.Stage++;
                if (this.CheckPointStageCallback(this))
                {
                    return;
                }
            }

            if (this.CheckPoint.Stage == 1)
            {
                if (this.CheckCancelled(exportJob))
                {
                    return;
                }

                if (portalLanguages == null)
                {
                    portalLanguages = CBO.FillCollection<ExportPortalLanguage>(DataProvider.Instance()
                        .GetPortalLanguages(exportJob.PortalId, toDate, fromDate));
                }

                this.Repository.CreateItems(portalLanguages);
                this.Result.AddSummary("Exported Portal Languages", portalLanguages.Count.ToString());
                this.CheckPoint.Progress = 100;
                this.CheckPoint.Completed = true;
                this.CheckPoint.Stage++;
                this.CheckPoint.ProcessedItems += portalLanguages.Count;
                this.CheckPointStageCallback(this);
            }
        }

        public override void ImportData(ExportImportJob importJob, ImportDto importDto)
        {
            // Update the total items count in the check points. This should be updated only once.
            this.CheckPoint.TotalItems = this.CheckPoint.TotalItems = this.CheckPoint.TotalItems <= 0 ? this.GetImportTotal() : this.CheckPoint.TotalItems;
            if (this.CheckPointStageCallback(this))
            {
                return;
            }

            if (this.CheckPoint.Stage > 1)
            {
                return;
            }

            if (this.CheckPoint.Stage == 0)
            {
                var portalSettings = this.Repository.GetAllItems<ExportPortalSetting>().ToList();
                this.ProcessPortalSettings(importJob, importDto, portalSettings);
                this.CheckPoint.TotalItems = this.GetImportTotal();
                this.Result.AddSummary("Imported Portal Settings", portalSettings.Count.ToString());
                this.CheckPoint.Progress += 50;
                this.CheckPoint.Stage++;
                this.CheckPoint.ProcessedItems = portalSettings.Count;
                if (this.CheckPointStageCallback(this))
                {
                    return;
                }
            }

            if (this.CheckPoint.Stage == 1)
            {
                var portalLanguages = this.Repository.GetAllItems<ExportPortalLanguage>().ToList();
                this.ProcessPortalLanguages(importJob, importDto, portalLanguages);
                this.Result.AddSummary("Imported Portal Languages", portalLanguages.Count.ToString());
                this.CheckPoint.Progress += 50;
                this.CheckPoint.Completed = true;
                this.CheckPoint.Stage++;
                this.CheckPoint.ProcessedItems += portalLanguages.Count;
                this.CheckPointStageCallback(this);
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
            return this.Repository.GetCount<ExportPortalSetting>() + this.Repository.GetCount<ExportPortalLanguage>();
        }

        private void ProcessPortalSettings(ExportImportJob importJob, ImportDto importDto,
            IEnumerable<ExportPortalSetting> portalSettings)
        {
            var portalId = importJob.PortalId;
            var localPortalSettings =
                CBO.FillCollection<ExportPortalSetting>(DataProvider.Instance().GetPortalSettings(portalId, DateUtils.GetDatabaseUtcTime().AddYears(1), null));
            foreach (var exportPortalSetting in portalSettings)
            {
                if (this.CheckCancelled(importJob))
                {
                    return;
                }

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
                            this.Result.AddLogEntry("Ignored portal settings", exportPortalSetting.SettingName);
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
                    this.Result.AddLogEntry("Updated portal settings", exportPortalSetting.SettingName);
                }
                else
                {
                    var createdBy = Util.GetUserIdByName(importJob, exportPortalSetting.CreatedByUserId, exportPortalSetting.CreatedByUserName);

                    DotNetNuke.Data.DataProvider.Instance()
                        .UpdatePortalSetting(importJob.PortalId, exportPortalSetting.SettingName,
                            exportPortalSetting.SettingValue, createdBy, exportPortalSetting.CultureCode, exportPortalSetting.IsSecure);

                    this.Result.AddLogEntry("Added portal settings", exportPortalSetting.SettingName);
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
                if (this.CheckCancelled(importJob))
                {
                    return;
                }

                var createdBy = Util.GetUserIdByName(importJob, exportPortalLanguage.CreatedByUserId, exportPortalLanguage.CreatedByUserName);
                var modifiedBy = Util.GetUserIdByName(importJob, exportPortalLanguage.LastModifiedByUserId, exportPortalLanguage.LastModifiedByUserName);

                var localLanguageId =
                    localLanguages.FirstOrDefault(x => x.Code == exportPortalLanguage.CultureCode)?.LanguageId;
                if (!localLanguageId.HasValue)
                {
                    var locale = new Locale
                    {
                        Code = exportPortalLanguage.CultureCode,
                        Fallback = Localization.SystemLocale,
                        Text = CultureInfo.GetCultureInfo(exportPortalLanguage.CultureCode).NativeName,
                    };
                    Localization.SaveLanguage(locale);
                    localLanguageId = locale.LanguageId;
                }

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
                            this.Result.AddLogEntry("Ignored portal language", exportPortalLanguage.CultureCode);
                            continue;
                        default:
                            throw new ArgumentOutOfRangeException(importDto.CollisionResolution.ToString());
                    }
                }

                if (isUpdate)
                {
                    DotNetNuke.Data.DataProvider.Instance()
                        .UpdatePortalLanguage(importJob.PortalId, localLanguageId.GetValueOrDefault(exportPortalLanguage.LanguageId),
                            exportPortalLanguage.IsPublished, modifiedBy);

                    this.Result.AddLogEntry("Updated portal language", exportPortalLanguage.CultureCode);
                }
                else
                {
                    exportPortalLanguage.PortalLanguageId = DotNetNuke.Data.DataProvider.Instance()
                        .AddPortalLanguage(importJob.PortalId, localLanguageId.GetValueOrDefault(exportPortalLanguage.LanguageId),
                            exportPortalLanguage.IsPublished, createdBy);
                    this.Result.AddLogEntry("Added portal language", exportPortalLanguage.CultureCode);
                }
            }
        }
    }
}

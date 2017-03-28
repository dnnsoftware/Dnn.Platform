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

using System;
using System.Linq;
using Dnn.ExportImport.Components.Common;
using Dnn.ExportImport.Components.Dto;
using Dnn.ExportImport.Components.Dto.ProfileProperties;
using Dnn.ExportImport.Components.Entities;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Data;
using DataProvider = Dnn.ExportImport.Components.Providers.DataProvider;

namespace Dnn.ExportImport.Components.Services
{
    public class ProfilePropertiesService : BasePortableService
    {
        public override string Category => Constants.Category_ProfileProps;

        public override string ParentCategory => null;

        public override uint Priority => 5;

        public override void ExportData(ExportImportJob exportJob, ExportDto exportDto)
        {
            var fromDate = exportDto.FromDate?.DateTime;
            var toDate = exportDto.ToDate;
            if (CheckCancelled(exportJob)) return;
            if (CheckPoint.Stage > 0) return;
            if (CheckCancelled(exportJob)) return;

            //TODO: Verify that profile properties stores created on and modified on info in UTC or local
            var profileProperties =
                CBO.FillCollection<ExportProfileProperty>(
                    DataProvider.Instance()
                        .GetPropertyDefinitionsByPortal(exportJob.PortalId, exportDto.IncludeDeletions, toDate,
                            fromDate)).ToList();
            CheckPoint.Progress = 50;
            //Update the total items count in the check points. This should be updated only once.
            CheckPoint.TotalItems = CheckPoint.TotalItems <= 0 ? profileProperties.Count : CheckPoint.TotalItems;
            CheckPointStageCallback(this);

            if (CheckCancelled(exportJob)) return;
            Repository.CreateItems(profileProperties, null);
            Result.AddSummary("Exported Profile Properties", profileProperties.Count.ToString());
            CheckPoint.Progress = 100;
            CheckPoint.ProcessedItems = profileProperties.Count;
            CheckPoint.Stage++;
            CheckPointStageCallback(this);
        }

        public override void ImportData(ExportImportJob importJob, ImportDto importDto)
        {
            if (CheckPoint.Stage > 0) return;
            var profileProperties = Repository.GetAllItems<ExportProfileProperty>().ToList();
            //Update the total items count in the check points. This should be updated only once.
            CheckPoint.TotalItems = CheckPoint.TotalItems <= 0 ? profileProperties.Count : CheckPoint.TotalItems;
            CheckPointStageCallback(this);

            foreach (var profileProperty in profileProperties)
            {
                if (CheckCancelled(importJob)) return;
                using (var db = DataContext.Instance())
                {
                    var existingProfileProperty = CBO.FillObject<ExportProfileProperty>(DotNetNuke.Data.DataProvider.Instance()
                        .GetPropertyDefinitionByName(importJob.PortalId, profileProperty.PropertyName));
                    var modifiedById = Util.GetUserIdOrName(importJob, profileProperty.LastModifiedByUserId,
                        profileProperty.LastModifiedByUserName);

                    if (existingProfileProperty != null)
                    {
                        switch (importDto.CollisionResolution)
                        {
                            case CollisionResolution.Overwrite:
                                ProcessUpdateProfileProperty(db, profileProperty, existingProfileProperty,
                                    existingProfileProperty.CreatedByUserId, modifiedById);
                                break;
                            case CollisionResolution.Ignore:
                                Result.AddLogEntry("Ignored profile property", profileProperty.PropertyName);
                                break;
                            default:
                                throw new ArgumentOutOfRangeException(importDto.CollisionResolution.ToString());
                        }
                    }
                    else
                    {
                        var createdById = Util.GetUserIdOrName(importJob, profileProperty.CreatedByUserId,
                            profileProperty.CreatedByUserName);

                        ProcessCreateProfileProperty(importJob, db, profileProperty, createdById, modifiedById);
                    }
                }
            }

            Result.AddSummary("Imported Profile Properties", profileProperties.Count.ToString());
            CheckPoint.ProcessedItems = profileProperties.Count;
            CheckPoint.Progress = 100;
            CheckPoint.Stage++;
            CheckPointStageCallback(this);
        }

        public override int GetImportTotal()
        {
            return Repository.GetCount<ExportProfileProperty>();
        }

        private void ProcessCreateProfileProperty(ExportImportJob importJob, IDataContext db,
            ExportProfileProperty profileProperty, int createdById, int modifiedById)
        {
            profileProperty.PropertyDefinitionId = 0;
            profileProperty.PortalId = importJob.PortalId;
            profileProperty.CreatedOnDate =
                profileProperty.LastModifiedOnDate = DateTime.UtcNow;
            profileProperty.CreatedByUserId = createdById;
            profileProperty.LastModifiedByUserId = modifiedById;
            var repProfileProperty = db.GetRepository<ExportProfileProperty>();
            repProfileProperty.Insert(profileProperty);
            Result.AddLogEntry("Added profile property", profileProperty.PropertyName);
        }

        private void ProcessUpdateProfileProperty(IDataContext db,
            ExportProfileProperty profileProperty, ExportProfileProperty existingProfileProperty,
            int? createdById, int modifiedById)
        {
            profileProperty.PropertyDefinitionId = existingProfileProperty.PropertyDefinitionId;
            profileProperty.PortalId = existingProfileProperty.PortalId;
            profileProperty.CreatedOnDate = existingProfileProperty.CreatedOnDate;
            profileProperty.CreatedByUserId = createdById;
            profileProperty.LastModifiedOnDate = DateTime.UtcNow;
            profileProperty.LastModifiedByUserId = modifiedById;
            var repProfileProperty = db.GetRepository<ExportProfileProperty>();
            repProfileProperty.Update(profileProperty);
            Result.AddLogEntry("Updated profile property", profileProperty.PropertyName);
        }
    }
}

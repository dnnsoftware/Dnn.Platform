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
using Dnn.ExportImport.Components.Dto;
using Dnn.ExportImport.Components.Dto.ProfileProperties;
using Dnn.ExportImport.Components.Entities;
using Dnn.ExportImport.Components.Interfaces;
using Dnn.ExportImport.Components.Models;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Data;
using DataProvider = Dnn.ExportImport.Components.Providers.DataProvider;

namespace Dnn.ExportImport.Components.Services
{
    public class ProfilePropertiesService : IPortable2
    {
        private int _progressPercentage;
        public string Category => "PROFILE_PROPERTIES";
        public string ParentCategory => "USERS";
        public uint Priority => 3;
        public bool CanCancel => false;
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

        public void ExportData(ExportImportJob exportJob, IExportImportRepository repository, ExportImportResult result, DateTime? utcSinceDate)
        {
            //TODO: Verify that profile properties stores createdon and modified on info in UTC or local
            ProgressPercentage = 0;
            var profileProperties =
                CBO.FillCollection<ExportProfileProperty>(
                    DataProvider.Instance().GetPropertyDefinitionsByPortal(exportJob.PortalId, utcSinceDate)).ToList();
            ProgressPercentage = 50;
            repository.CreateItems(profileProperties, null);
            result.AddSummary("Exported Profile Properties", profileProperties.Count.ToString());
            ProgressPercentage = 100;
        }

        public void ImportData(ExportImportJob importJob, ExportDto exporteDto, IExportImportRepository repository, ExportImportResult result)
        {
            ProgressPercentage = 0;
            var profileProperties = repository.GetAllItems<ExportProfileProperty>().ToList();
            result.AddSummary("Imported Profile Properties", profileProperties.Count.ToString());

            foreach (var profileProperty in profileProperties)
            {
                using (var db = DataContext.Instance())
                {
                    var existingProfileProperty = CBO.FillObject<ExportProfileProperty>(DotNetNuke.Data.DataProvider.Instance()
                        .GetPropertyDefinitionByName(importJob.PortalId, profileProperty.PropertyName));
                    var modifiedById = Common.Util.GetUserIdOrName(importJob, profileProperty.LastModifiedByUserId,
                        profileProperty.LastModifiedByUserName);

                    if (existingProfileProperty != null)
                    {
                        switch (exporteDto.CollisionResolution)
                        {
                            case CollisionResolution.Overwrite:
                                ProcessUpdateProfileProperty(db, profileProperty, existingProfileProperty,
                                    existingProfileProperty.CreatedByUserId, modifiedById);
                                break;
                            case CollisionResolution.Ignore: //Just ignore the record
                            //TODO: Log that profile property was ignored.
                            case CollisionResolution.Duplicate: //Just ignore the record
                                //TODO: Log that profile property was ignored as duplicate not possible for profile properties.
                                break;
                            default:
                                throw new ArgumentOutOfRangeException(exporteDto.CollisionResolution.ToString());
                        }
                    }
                    else
                    {
                        var createdById = Common.Util.GetUserIdOrName(importJob, profileProperty.CreatedByUserId,
                            profileProperty.CreatedByUserName);

                        ProcessCreateProfileProperty(importJob, db, profileProperty, createdById, modifiedById);
                    }
                }
            }
        }

        private static void ProcessCreateProfileProperty(ExportImportJob importJob, IDataContext db,
            ExportProfileProperty profileProperty, int createdById, int modifiedById)
        {
            profileProperty.PropertyDefinitionId = 0;
            profileProperty.PortalId = importJob.PortalId;
            profileProperty.CreatedOnDate = DateTime.UtcNow;
            profileProperty.CreatedByUserId = createdById;
            profileProperty.LastModifiedOnDate = DateTime.UtcNow;
            profileProperty.LastModifiedByUserId = modifiedById;
            var repProfileProperty = db.GetRepository<ExportProfileProperty>();
            repProfileProperty.Insert(profileProperty);
        }

        private static void ProcessUpdateProfileProperty(IDataContext db,
            ExportProfileProperty profileProperty, ExportProfileProperty existingProfileProperty,
            int? createdById, int modifiedById)
        {
            profileProperty.PortalId = existingProfileProperty.PortalId;
            profileProperty.CreatedOnDate = existingProfileProperty.CreatedOnDate;
            profileProperty.CreatedByUserId = createdById;
            profileProperty.LastModifiedOnDate = DateTime.UtcNow;
            profileProperty.LastModifiedByUserId = modifiedById;
            var repProfileProperty = db.GetRepository<ExportProfileProperty>();
            repProfileProperty.Update(profileProperty);
        }
    }
}

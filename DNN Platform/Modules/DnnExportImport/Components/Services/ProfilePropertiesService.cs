// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using System;
using System.Linq;
using Dnn.ExportImport.Components.Common;
using Dnn.ExportImport.Components.Dto;
using Dnn.ExportImport.Components.Entities;
using Dnn.ExportImport.Dto.ProfileProperties;
using DotNetNuke.Common.Utilities;
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
            var fromDate = (exportDto.FromDateUtc ?? Constants.MinDbTime).ToLocalTime();
            var toDate = exportDto.ToDateUtc.ToLocalTime();
            if (CheckCancelled(exportJob)) return;
            if (CheckPoint.Stage > 0) return;
            if (CheckCancelled(exportJob)) return;

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
            Repository.CreateItems(profileProperties);
            Result.AddSummary("Exported Profile Properties", profileProperties.Count.ToString());
            CheckPoint.Progress = 100;
            CheckPoint.ProcessedItems = profileProperties.Count;
            CheckPoint.Completed = true;
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

                var existingProfileProperty = CBO.FillObject<ExportProfileProperty>(DotNetNuke.Data.DataProvider.Instance()
                    .GetPropertyDefinitionByName(importJob.PortalId, profileProperty.PropertyName));

                if (existingProfileProperty != null)
                {
                    switch (importDto.CollisionResolution)
                    {
                        case CollisionResolution.Overwrite:
                            var modifiedById = Util.GetUserIdByName(importJob, profileProperty.LastModifiedByUserId, profileProperty.LastModifiedByUserName);
                            ProcessUpdateProfileProperty(profileProperty, existingProfileProperty, modifiedById);
                            break;
                        case CollisionResolution.Ignore:
                            break;
                        default:
                            throw new ArgumentOutOfRangeException(importDto.CollisionResolution.ToString());
                    }
                }
                else
                {
                    var createdById = Util.GetUserIdByName(importJob, profileProperty.CreatedByUserId,
                        profileProperty.CreatedByUserName);

                    ProcessCreateProfileProperty(importJob, profileProperty, createdById);
                }
            }

            Result.AddSummary("Imported Profile Properties", profileProperties.Count.ToString());
            CheckPoint.ProcessedItems = profileProperties.Count;
            CheckPoint.Completed = true;
            CheckPoint.Progress = 100;
            CheckPoint.Stage++;
            CheckPointStageCallback(this);
        }

        public override int GetImportTotal()
        {
            return Repository.GetCount<ExportProfileProperty>();
        }

        private static void ProcessCreateProfileProperty(ExportImportJob importJob,
            ExportProfileProperty profileProperty, int createdById)
        {
            DotNetNuke.Data.DataProvider.Instance()
                .AddPropertyDefinition(importJob.PortalId, profileProperty.ModuleDefId ?? Null.NullInteger,
                    profileProperty.DataType ?? Null.NullInteger,
                    profileProperty.DefaultValue, profileProperty.PropertyCategory, profileProperty.PropertyName,
                    profileProperty.ReadOnly, profileProperty.Required,
                    profileProperty.ValidationExpression, profileProperty.ViewOrder, profileProperty.Visible,
                    profileProperty.Length, profileProperty.DefaultVisibility, createdById);
        }

        private static void ProcessUpdateProfileProperty(ExportProfileProperty profileProperty, ExportProfileProperty existingProfileProperty,
            int modifiedById)
        {
            DotNetNuke.Data.DataProvider.Instance()
                .UpdatePropertyDefinition(existingProfileProperty.PropertyDefinitionId,
                    profileProperty.DataType ?? Null.NullInteger,
                    profileProperty.DefaultValue, profileProperty.PropertyCategory, profileProperty.PropertyName,
                    profileProperty.ReadOnly, profileProperty.Required,
                    profileProperty.ValidationExpression, profileProperty.ViewOrder, profileProperty.Visible,
                    profileProperty.Length, profileProperty.DefaultVisibility, modifiedById);
        }
    }
}

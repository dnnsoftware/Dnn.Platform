using System;
using System.Linq;
using Dnn.ExportImport.Components.Dto;
using Dnn.ExportImport.Components.Dto.ProfileProperties;
using Dnn.ExportImport.Components.Entities;
using Dnn.ExportImport.Components.Interfaces;
using DotNetNuke.Data;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Profile;

namespace Dnn.ExportImport.Components.Services
{
    public class ProfilePropertiesService : IPortable2
    {
        private int _progressPercentage;
        public string Category => "PROFILEPROPERTIES";
        public uint Priority => 5;
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

        public void ExportData(ExportImportJob exportJob, IExportImportRepository repository)
        {
            ProgressPercentage = 0;
            var profileProperties =
                CBO.FillCollection<ExportProfileProperty>(
                    DataProvider.Instance().GetPropertyDefinitionsByPortal(exportJob.PortalId)).ToList();
            ProgressPercentage = 50;
            repository.CreateItems(profileProperties, null);
            ProgressPercentage = 100;
        }

        public void ImportData(ExportImportJob importJob, ExportDto exporteDto, IExportImportRepository repository)
        {
            ProgressPercentage = 0;
            var profileProperties = repository.GetAllItems<ExportProfileProperty>().ToList();

            foreach (var profileProperty in profileProperties)
            {
                using (var db = DataContext.Instance())
                {
                    var existingProfileProperty = CBO.FillObject<ExportProfileProperty>(DataProvider.Instance()
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

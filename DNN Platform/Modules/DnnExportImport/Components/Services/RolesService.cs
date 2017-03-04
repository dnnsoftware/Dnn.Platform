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
using System.Collections.Generic;
using System.Linq;
using Dnn.ExportImport.Components.Dto;
using Dnn.ExportImport.Components.Dto.Roles;
using Dnn.ExportImport.Components.Entities;
using Dnn.ExportImport.Components.Interfaces;
using Dnn.ExportImport.Components.Providers;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Security.Roles;

namespace Dnn.ExportImport.Components.Services
{
    public class RolesService : IPortable2
    {
        private int _progressPercentage;

        public string Category => "ROLES";

        public uint Priority => 2;

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

        public void ExportData(ExportImportJob exportJob, IExportImportRepository repository)
        {
            ProgressPercentage = 0;

            var roleGroups = CBO.FillCollection<ExportRoleGroup>(DataProvider.Instance().GetAllRoleGroups(exportJob.PortalId));
            repository.CreateItems(roleGroups, null);
            ProgressPercentage += 30;

            var roles = CBO.FillCollection<ExportRole>(DataProvider.Instance().GetAllRoles(exportJob.PortalId));
            repository.CreateItems(roles, null);
            ProgressPercentage += 50;

            var roleSettings = CBO.FillCollection<ExportRoleSetting>(DataProvider.Instance().GetAllRoleSettings(exportJob.PortalId));
            repository.CreateItems(roleSettings, null);
            ProgressPercentage += 20;
        }

        public void ImportData(ExportImportJob importJob, ExportDto exporteDto, IExportImportRepository repository)
        {
            ProgressPercentage = 0;

            var otherRoleGroups = repository.GetAllItems<ExportRoleGroup>().ToList();
            ProcessRoleGroups(importJob, exporteDto, otherRoleGroups);
            ProgressPercentage += 40;

            var otherRoles = repository.GetAllItems<ExportRole>().ToList();
            ProcessRoles(importJob, exporteDto, otherRoleGroups, otherRoles);
            ProgressPercentage += 40;

            var otherRoleSettings = repository.GetAllItems<ExportRoleSetting>().ToList();
            ProcessRoleSettingss(importJob, exporteDto, otherRoles, otherRoleSettings);
            ProgressPercentage += 20;
        }

        private static void ProcessRoleGroups(ExportImportJob importJob, ExportDto exporteDto, IEnumerable<ExportRoleGroup> otherRoleGroups)
        {
            var portalId = importJob.PortalId;
            var localRoleGroups = CBO.FillCollection<ExportRoleGroup>(DataProvider.Instance().GetAllRoleGroups(portalId));
            foreach (var other in otherRoleGroups)
            {
                var createdBy = Common.Util.GetUserIdOrName(importJob, other.CreatedByUserID, other.CreatedByUserName);
                var modifiedBy = Common.Util.GetUserIdOrName(importJob, other.LastModifiedByUserID, other.LastModifiedByUserName);
                var local = localRoleGroups.FirstOrDefault(t => t.RoleGroupName == other.RoleGroupName);

                if (local != null)
                {
                    other.LocalId = local.RoleGroupID;
                    switch (exporteDto.CollisionResolution)
                    {
                        case CollisionResolution.Ignore:
                            break;
                        case CollisionResolution.Overwrite:
                            var roleGroup = new RoleGroupInfo(local.RoleGroupID, portalId, false)
                            {
                                RoleGroupName = other.RoleGroupName,
                                Description = other.Description,
                            };
                            RoleController.UpdateRoleGroup(roleGroup, false);
                            //TODO: set created/update by if it is anything other than -1
                            DataCache.ClearCache(DataCache.RoleGroupsCacheKey);
                            break;
                        case CollisionResolution.Duplicate:
                            local = null; // so we can add new one below
                            break;
                        default:
                            throw new ArgumentOutOfRangeException(exporteDto.CollisionResolution.ToString());
                    }
                }

                if (local == null)
                {
                    var roleGroup = new RoleGroupInfo()
                    {
                        PortalID = portalId,
                        Description = other.Description,
                    };
                    other.LocalId = RoleController.AddRoleGroup(roleGroup);
                    //TODO: set created/update by if it is anything other than -1
                }
            }
        }

        private static void ProcessRoles(ExportImportJob importJob, ExportDto exporteDto,
            List<ExportRoleGroup> otherRoleGroups, List<ExportRole> otherRoles)
        {
            throw new NotImplementedException();
        }

        private static void ProcessRoleSettingss(ExportImportJob importJob, ExportDto exporteDto,
            List<ExportRole> otherRoles, List<ExportRoleSetting> otherRoleSettings)
        {
            throw new NotImplementedException();
        }
    }
}
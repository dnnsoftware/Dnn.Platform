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
using Dnn.ExportImport.Components.Common;
using Dnn.ExportImport.Components.Dto;
using Dnn.ExportImport.Components.Dto.Roles;
using Dnn.ExportImport.Components.Entities;
using Dnn.ExportImport.Components.Providers;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Security.Roles;

namespace Dnn.ExportImport.Components.Services
{
    public class RolesExportService : Potable2Base
    {
        private int _progressPercentage;

        public override string Category => Constants.Category_Roles;

        public override string ParentCategory => Constants.Category_Users;

        public override uint Priority => 2;

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

        public override void ExportData(ExportImportJob exportJob, ExportDto exportDto)
        {
            ProgressPercentage = 0;
            if (CheckPoint.Stage > 2) return;

            if (CheckPoint.Stage == 0)
            {
                if (CheckCancelled(exportJob)) return;
                var roleGroups = CBO.FillCollection<ExportRoleGroup>(
                    DataProvider.Instance().GetAllRoleGroups(exportJob.PortalId, exportDto.ExportTime?.UtcDateTime));
                Repository.CreateItems(roleGroups, null);
                Result.AddSummary("Exported Role Groups", roleGroups.Count.ToString());
                ProgressPercentage = 30;

                CheckPoint.Stage++;
                if (CheckPointStageCallback(this)) return;
            }

            if (CheckPoint.Stage == 1)
            {
                if (CheckCancelled(exportJob)) return;
                var roles = CBO.FillCollection<ExportRole>(
                    DataProvider.Instance().GetAllRoles(exportJob.PortalId, exportDto.ExportTime?.UtcDateTime));
                Repository.CreateItems(roles, null);
                Result.AddSummary("Exported Roles", roles.Count.ToString());
                ProgressPercentage = 80;

                CheckPoint.Stage++;
                if (CheckPointStageCallback(this)) return;
            }

            if (CheckPoint.Stage == 2)
            {
                if (CheckCancelled(exportJob)) return;
                var roleSettings = CBO.FillCollection<ExportRoleSetting>(
                    DataProvider.Instance().GetAllRoleSettings(exportJob.PortalId, exportDto.ExportTime?.UtcDateTime));
                Repository.CreateItems(roleSettings, null);
                Result.AddSummary("Exported Role Settings", roleSettings.Count.ToString());
                ProgressPercentage = 100;

                CheckPoint.Stage++;
                CheckPointStageCallback(this);
            }
        }

        public override void ImportData(ExportImportJob importJob, ExportDto exportDto)
        {
            ProgressPercentage = 0;
            if (CheckPoint.Stage > 2) return;

            if (CheckCancelled(importJob)) return;
            var otherRoleGroups = Repository.GetAllItems<ExportRoleGroup>().ToList();
            if (CheckPoint.Stage == 0)
            {
                ProcessRoleGroups(importJob, exportDto, otherRoleGroups);
                Result.AddSummary("Imported Role Groups", otherRoleGroups.Count.ToString());
                ProgressPercentage = 40;

                CheckPoint.Stage++;
                if (CheckPointStageCallback(this)) return;
            }

            if (CheckCancelled(importJob)) return;
            var otherRoles = Repository.GetAllItems<ExportRole>().ToList();
            if (CheckPoint.Stage == 1)
            {
                Result.AddSummary("Imported Roles", otherRoles.Count.ToString());
                ProcessRoles(importJob, exportDto, otherRoleGroups, otherRoles);
                Repository.UpdateItems(otherRoles);
                ProgressPercentage = 50;

                CheckPoint.Stage++;
                if (CheckPointStageCallback(this)) return;
            }

            if (CheckPoint.Stage == 2)
            {
                if (CheckCancelled(importJob)) return;
                var otherRoleSettings = Repository.GetAllItems<ExportRoleSetting>().ToList();
                ProcessRoleSettings(importJob, exportDto, otherRoles, otherRoleSettings);
                Repository.UpdateItems(otherRoleSettings);
                Result.AddSummary("Imported Role Settings", otherRoleSettings.Count.ToString());
                ProgressPercentage = 100;

                CheckPoint.Stage++;
                CheckPointStageCallback(this);
            }
        }

        private void ProcessRoleGroups(ExportImportJob importJob, ExportDto exportDto,
            IEnumerable<ExportRoleGroup> otherRoleGroups)
        {
            var changedGroups = new List<RoleGroupItem>();
            var portalId = importJob.PortalId;
            var localRoleGroups = CBO.FillCollection<ExportRoleGroup>(DataProvider.Instance().GetAllRoleGroups(portalId, null));
            foreach (var other in otherRoleGroups)
            {
                if (CheckCancelled(importJob)) return;
                var createdBy = Util.GetUserIdOrName(importJob, other.CreatedByUserID, other.CreatedByUserName);
                var modifiedBy = Util.GetUserIdOrName(importJob, other.LastModifiedByUserID, other.LastModifiedByUserName);
                var local = localRoleGroups.FirstOrDefault(t => t.RoleGroupName == other.RoleGroupName);

                if (local != null)
                {
                    other.LocalId = local.RoleGroupID;
                    switch (exportDto.CollisionResolution)
                    {
                        case CollisionResolution.Ignore:
                            Result.AddLogEntry("Ignored role group", other.RoleGroupName);
                            break;
                        case CollisionResolution.Overwrite:
                            var roleGroup = new RoleGroupInfo(local.RoleGroupID, portalId, false)
                            {
                                RoleGroupName = other.RoleGroupName,
                                Description = other.Description,
                            };
                            RoleController.UpdateRoleGroup(roleGroup, false);
                            changedGroups.Add(new RoleGroupItem(roleGroup.RoleGroupID, createdBy, modifiedBy));
                            DataCache.ClearCache(string.Format(DataCache.RoleGroupsCacheKey, local.RoleGroupID));
                            Result.AddLogEntry("Updated role group", other.RoleGroupName);
                            break;
                        case CollisionResolution.Duplicate:
                            local = null; // so we can add new one below
                            break;
                        default:
                            throw new ArgumentOutOfRangeException(exportDto.CollisionResolution.ToString());
                    }
                }

                if (local == null)
                {
                    var roleGroup = new RoleGroupInfo()
                    {
                        PortalID = portalId,
                        RoleGroupName = other.RoleGroupName,
                        Description = other.Description,
                    };
                    other.LocalId = RoleController.AddRoleGroup(roleGroup);
                    changedGroups.Add(new RoleGroupItem(roleGroup.RoleGroupID, createdBy, modifiedBy));
                    Result.AddLogEntry("Added role group", other.RoleGroupName);
                }
            }
            if (changedGroups.Count > 0)
                RefreshRecordsUserIds(changedGroups);
        }

        private void ProcessRoles(ExportImportJob importJob, ExportDto exportDto,
            List<ExportRoleGroup> otherRoleGroups, IEnumerable<ExportRole> otherRoles)
        {
            var roleItems = new List<RoleItem>();
            var portalId = importJob.PortalId;
            foreach (var other in otherRoles)
            {
                if (CheckCancelled(importJob)) return;
                var createdBy = Util.GetUserIdOrName(importJob, other.CreatedByUserID, other.CreatedByUserName);
                var modifiedBy = Util.GetUserIdOrName(importJob, other.LastModifiedByUserID, other.LastModifiedByUserName);
                var localRoleInfo = RoleController.Instance.GetRoleByName(portalId, other.RoleName);
                if (localRoleInfo != null)
                {
                    other.LocalId = localRoleInfo.RoleID;
                    switch (exportDto.CollisionResolution)
                    {
                        case CollisionResolution.Ignore:
                            Result.AddLogEntry("Ignored role", other.RoleName);
                            break;
                        case CollisionResolution.Overwrite:
                            var group = other.RoleGroupID.HasValue
                                ? otherRoleGroups.FirstOrDefault(g => g.RoleGroupID == other.RoleGroupID.Value)
                                : null;

                            localRoleInfo.RoleName = other.RoleName;
                            localRoleInfo.AutoAssignment = other.AutoAssignment;
                            localRoleInfo.BillingFrequency = other.BillingFrequency;
                            localRoleInfo.BillingPeriod = other.BillingPeriod ?? 0;
                            localRoleInfo.Description = other.Description;
                            localRoleInfo.IconFile = other.IconFile;      //TODO: map to local file
                            localRoleInfo.IsPublic = other.IsPublic;
                            localRoleInfo.IsSystemRole = other.IsSystemRole;
                            localRoleInfo.RoleGroupID = group?.LocalId ?? Null.NullInteger;
                            localRoleInfo.RSVPCode = other.RSVPCode;
                            localRoleInfo.SecurityMode = (SecurityMode)other.SecurityMode;
                            localRoleInfo.ServiceFee = Convert.ToSingle(other.ServiceFee ?? 0m);
                            localRoleInfo.Status = (RoleStatus)other.Status;
                            localRoleInfo.TrialFee = Convert.ToSingle(other.TrialFee ?? 0m);
                            localRoleInfo.TrialFrequency = other.TrialFrequency;
                            localRoleInfo.TrialPeriod = other.TrialPeriod ?? 0;

                            RoleController.Instance.UpdateRole(localRoleInfo, other.AutoAssignment);
                            roleItems.Add(new RoleItem(localRoleInfo.RoleID, createdBy, modifiedBy));
                            RoleController.Instance.ClearRoleCache(localRoleInfo.RoleID);
                            Result.AddLogEntry("Updated role", other.RoleName);
                            break;
                        case CollisionResolution.Duplicate:
                            localRoleInfo = null; // so we can add new one below
                            break;
                        default:
                            throw new ArgumentOutOfRangeException(exportDto.CollisionResolution.ToString());
                    }
                }

                if (localRoleInfo == null)
                {
                    var group = other.RoleGroupID.HasValue
                        ? otherRoleGroups.FirstOrDefault(g => g.RoleGroupID == other.RoleGroupID.Value)
                        : null;

                    var roleInfo = new RoleInfo
                    {
                        PortalID = portalId,
                        RoleName = other.RoleName,
                        AutoAssignment = other.AutoAssignment,
                        BillingFrequency = other.BillingFrequency,
                        BillingPeriod = other.BillingPeriod ?? 0,
                        Description = other.Description,
                        IconFile = other.IconFile,      //TODO: map to local file
                        IsPublic = other.IsPublic,
                        IsSystemRole = other.IsSystemRole,
                        RoleGroupID = group?.LocalId ?? Null.NullInteger,
                        RSVPCode = other.RSVPCode,
                        SecurityMode = (SecurityMode)other.SecurityMode,
                        ServiceFee = Convert.ToSingle(other.ServiceFee ?? 0m),
                        Status = (RoleStatus)other.Status,
                        TrialFee = Convert.ToSingle(other.TrialFee ?? 0m),
                        TrialFrequency = other.TrialFrequency,
                        TrialPeriod = other.TrialPeriod ?? 0,
                    };

                    other.LocalId = RoleController.Instance.AddRole(roleInfo, other.AutoAssignment);
                    roleItems.Add(new RoleItem(roleInfo.RoleID, createdBy, modifiedBy));
                    RoleController.Instance.ClearRoleCache(roleInfo.RoleID);
                    Result.AddLogEntry("Added role", other.RoleName);
                }
            }

            //set created/updated for any added/modified item
            if (roleItems.Count > 0)
                RefreshRecordsUserIds(roleItems);
        }

        private void ProcessRoleSettings(ExportImportJob importJob, ExportDto exportDto,
            IList<ExportRole> otherRoles, IEnumerable<ExportRoleSetting> otherRoleSettings)
        {
            var changedSettings = new List<SettingItem>();
            var portalId = importJob.PortalId;
            foreach (var other in otherRoleSettings)
            {
                if (CheckCancelled(importJob)) return;
                var createdBy = Util.GetUserIdOrName(importJob, other.CreatedByUserID, other.CreatedByUserName);
                var modifiedBy = Util.GetUserIdOrName(importJob, other.LastModifiedByUserID, other.LastModifiedByUserName);
                var otherRole = otherRoles.FirstOrDefault(r => r.RoleID == other.RoleID);
                if (otherRole == null || !otherRole.LocalId.HasValue) continue;
                var localRoleInfo = RoleController.Instance.GetRoleById(portalId, otherRole.LocalId.Value);
                if (localRoleInfo == null) continue;

                switch (exportDto.CollisionResolution)
                {
                    case CollisionResolution.Ignore:
                        Result.AddLogEntry("Ignored role setting", other.SettingName);
                        break;
                    case CollisionResolution.Overwrite:
                        string settingValue;
                        if (!localRoleInfo.Settings.TryGetValue(other.SettingName, out settingValue) ||
                            settingValue != other.SettingValue)
                        {
                            changedSettings.Add(new SettingItem(localRoleInfo.RoleID, other.SettingName, createdBy, modifiedBy));
                            localRoleInfo.Settings[other.SettingName] = other.SettingValue;
                            RoleController.Instance.UpdateRoleSettings(localRoleInfo, false);
                            Result.AddLogEntry("Updated role setting", other.SettingName);
                            //No need to clear cache as the caller will do it one time at end
                        }
                        else
                        {
                            Result.AddLogEntry("Ignored role setting", other.SettingName);
                        }
                        break;
                    case CollisionResolution.Duplicate:
                        // there is no meaning for duplicates in a dictionary key
                        Result.AddLogEntry("Ignored duplicate role setting", other.SettingName);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(exportDto.CollisionResolution.ToString());
                }

            }

            if (changedSettings.Count > 0)
                RefreshRecordsUserIds(changedSettings);

            RoleController.Instance.ClearRoleCache(importJob.PortalId);
        }

        private static void RefreshRecordsUserIds(IEnumerable<RoleGroupItem> roleGroupItems)
        {
            var provider = DataProvider.Instance();
            foreach (var roleGroupItem in roleGroupItems)
            {
                provider.UpdateRoleGroupChangers(
                    roleGroupItem.RoleGroupId, roleGroupItem.CreatedBy, roleGroupItem.ModifiedBy);
            }
        }

        private static void RefreshRecordsUserIds(IEnumerable<RoleItem> roleItems)
        {
            var provider = DataProvider.Instance();
            foreach (var roleItem in roleItems)
            {
                provider.UpdateRoleChangers(
                    roleItem.RoleId, roleItem.CreatedBy, roleItem.ModifiedBy);
            }
        }

        private static void RefreshRecordsUserIds(IEnumerable<SettingItem> SettingItem)
        {
            var provider = DataProvider.Instance();
            foreach (var settingItem in SettingItem)
            {
                provider.UpdateRoleSettingChangers(
                    settingItem.RoleId, settingItem.Name, settingItem.CreatedBy, settingItem.ModifiedBy);
            }
        }

        private struct RoleGroupItem
        {
            public int RoleGroupId { get; }
            public int CreatedBy { get; }
            public int ModifiedBy { get; }

            public RoleGroupItem(int roleGroupId, int createdBy, int modifiedBy)
            {
                RoleGroupId = roleGroupId;
                CreatedBy = createdBy;
                ModifiedBy = modifiedBy;
            }
        }

        private struct RoleItem
        {
            public int RoleId { get; }
            public int CreatedBy { get; }
            public int ModifiedBy { get; }

            public RoleItem(int roleId, int createdBy, int modifiedBy)
            {
                RoleId = roleId;
                CreatedBy = createdBy;
                ModifiedBy = modifiedBy;
            }
        }

        private struct SettingItem
        {
            public int RoleId { get; }
            public string Name { get; }
            public int CreatedBy { get; }
            public int ModifiedBy { get; }

            public SettingItem(int roleId, string settingName, int createdBy, int modifiedBy)
            {
                RoleId = roleId;
                Name = settingName;
                CreatedBy = createdBy;
                ModifiedBy = modifiedBy;
            }
        }
    }
}
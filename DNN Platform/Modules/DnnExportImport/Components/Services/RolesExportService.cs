// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.ExportImport.Components.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Dnn.ExportImport.Components.Common;
    using Dnn.ExportImport.Components.Dto;
    using Dnn.ExportImport.Components.Entities;
    using Dnn.ExportImport.Components.Providers;
    using Dnn.ExportImport.Dto.Roles;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Security.Roles;

    public class RolesExportService : BasePortableService
    {
        public override string Category => Constants.Category_Roles;

        public override string ParentCategory => null;

        public override uint Priority => 5;

        public override void ExportData(ExportImportJob exportJob, ExportDto exportDto)
        {
            var fromDate = (exportDto.FromDateUtc ?? Constants.MinDbTime).ToLocalTime();
            var toDate = exportDto.ToDateUtc.ToLocalTime();
            if (this.CheckPoint.Stage > 2)
            {
                return;
            }

            List<ExportRole> roles = null;
            List<ExportRoleSetting> roleSettings = null;
            if (this.CheckPoint.Stage == 0)
            {
                if (this.CheckCancelled(exportJob))
                {
                    return;
                }

                var roleGroups = CBO.FillCollection<ExportRoleGroup>(
                    DataProvider.Instance().GetAllRoleGroups(exportJob.PortalId, toDate, fromDate));

                // Update the total items count in the check points. This should be updated only once.
                this.CheckPoint.TotalItems = this.CheckPoint.TotalItems <= 0 ? roleGroups.Count : this.CheckPoint.TotalItems;
                if (this.CheckPoint.TotalItems == roleGroups.Count)
                {
                    roles = CBO.FillCollection<ExportRole>(
                        DataProvider.Instance().GetAllRoles(exportJob.PortalId, toDate, fromDate));
                    roleSettings = CBO.FillCollection<ExportRoleSetting>(
                        DataProvider.Instance().GetAllRoleSettings(exportJob.PortalId, toDate, fromDate));
                    this.CheckPoint.TotalItems += roles.Count + roleSettings.Count;
                }

                this.CheckPointStageCallback(this);

                this.Repository.CreateItems(roleGroups);
                this.Result.AddSummary("Exported Role Groups", roleGroups.Count.ToString());
                this.CheckPoint.ProcessedItems = roleGroups.Count;
                this.CheckPoint.Progress = 30;
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

                if (roles == null)
                {
                    roles = CBO.FillCollection<ExportRole>(
                    DataProvider.Instance().GetAllRoles(exportJob.PortalId, toDate, fromDate));
                }

                this.Repository.CreateItems(roles);
                this.Result.AddSummary("Exported Roles", roles.Count.ToString());
                this.CheckPoint.Progress = 80;
                this.CheckPoint.ProcessedItems += roles.Count;
                this.CheckPoint.Stage++;
                if (this.CheckPointStageCallback(this))
                {
                    return;
                }
            }

            if (this.CheckPoint.Stage == 2)
            {
                if (this.CheckCancelled(exportJob))
                {
                    return;
                }

                if (roleSettings == null)
                {
                    roleSettings = CBO.FillCollection<ExportRoleSetting>(
                       DataProvider.Instance().GetAllRoleSettings(exportJob.PortalId, toDate, fromDate));
                }

                this.Repository.CreateItems(roleSettings);
                this.Result.AddSummary("Exported Role Settings", roleSettings.Count.ToString());
                this.CheckPoint.Progress = 100;
                this.CheckPoint.ProcessedItems += roleSettings.Count;
                this.CheckPoint.Completed = true;
                this.CheckPoint.Stage++;
                this.CheckPointStageCallback(this);
            }
        }

        public override void ImportData(ExportImportJob importJob, ImportDto importDto)
        {
            if (this.CheckPoint.Stage > 2)
            {
                return;
            }

            if (this.CheckCancelled(importJob))
            {
                return;
            }

            // Update the total items count in the check points. This should be updated only once.
            this.CheckPoint.TotalItems = this.CheckPoint.TotalItems <= 0 ? this.GetImportTotal() : this.CheckPoint.TotalItems;
            this.CheckPointStageCallback(this);

            var otherRoleGroups = this.Repository.GetAllItems<ExportRoleGroup>().ToList();
            if (this.CheckPoint.Stage == 0)
            {
                this.ProcessRoleGroups(importJob, importDto, otherRoleGroups);
                this.Repository.UpdateItems(otherRoleGroups);
                this.Result.AddSummary("Imported Role Groups", otherRoleGroups.Count.ToString());
                this.CheckPoint.Progress = 40;
                this.CheckPoint.ProcessedItems = otherRoleGroups.Count;
                this.CheckPoint.Stage++;
                if (this.CheckPointStageCallback(this))
                {
                    return;
                }
            }

            if (this.CheckCancelled(importJob))
            {
                return;
            }

            var otherRoles = this.Repository.GetAllItems<ExportRole>().ToList();
            if (this.CheckPoint.Stage == 1)
            {
                this.Result.AddSummary("Imported Roles", otherRoles.Count.ToString());
                this.ProcessRoles(importJob, importDto, otherRoleGroups, otherRoles);
                this.Repository.UpdateItems(otherRoles);
                this.CheckPoint.Progress = 50;
                this.CheckPoint.ProcessedItems += otherRoles.Count;
                this.CheckPoint.Stage++;
                if (this.CheckPointStageCallback(this))
                {
                    return;
                }
            }

            if (this.CheckPoint.Stage == 2)
            {
                if (this.CheckCancelled(importJob))
                {
                    return;
                }

                var otherRoleSettings = this.Repository.GetAllItems<ExportRoleSetting>().ToList();
                this.ProcessRoleSettings(importJob, importDto, otherRoles, otherRoleSettings);
                this.Repository.UpdateItems(otherRoleSettings);
                this.Result.AddSummary("Imported Role Settings", otherRoleSettings.Count.ToString());
                this.CheckPoint.Progress = 100;
                this.CheckPoint.ProcessedItems += otherRoleSettings.Count;
                this.CheckPoint.Completed = true;
                this.CheckPoint.Stage++;
                this.CheckPointStageCallback(this);
            }
        }

        public override int GetImportTotal()
        {
            return this.Repository.GetCount<ExportRoleGroup>() + this.Repository.GetCount<ExportRole>() +
                   this.Repository.GetCount<ExportRoleSetting>();
        }

        private static void RefreshRecordsUserIds(IEnumerable<RoleGroupItem> roleGroupItems)
        {
            var provider = DataProvider.Instance();
            foreach (var roleGroupItem in roleGroupItems)
            {
                provider.UpdateRecordChangers("RoleGroups", "RoleGroupID",
                    roleGroupItem.RoleGroupId, roleGroupItem.CreatedBy, roleGroupItem.ModifiedBy);
            }
        }

        private static void RefreshRecordsUserIds(IEnumerable<RoleItem> roleItems)
        {
            var provider = DataProvider.Instance();
            foreach (var roleItem in roleItems)
            {
                provider.UpdateRecordChangers("Roles", "RoleID",
                    roleItem.RoleId, roleItem.CreatedBy, roleItem.ModifiedBy);
            }
        }

        private static void RefreshRecordsUserIds(IEnumerable<SettingItem> settingItems)
        {
            var provider = DataProvider.Instance();
            foreach (var item in settingItems)
            {
                provider.UpdateSettingRecordChangers("RoleSettings", "RoleID",
                    item.RoleId, item.Name, item.CreatedBy, item.ModifiedBy);
            }
        }

        private void ProcessRoleGroups(ExportImportJob importJob, ImportDto importDto,
            IEnumerable<ExportRoleGroup> otherRoleGroups)
        {
            var changedGroups = new List<RoleGroupItem>();
            var portalId = importJob.PortalId;
            var localRoleGroups = CBO.FillCollection<ExportRoleGroup>(DataProvider.Instance().GetAllRoleGroups(portalId, DateUtils.GetDatabaseUtcTime().AddYears(1), null));
            foreach (var other in otherRoleGroups)
            {
                if (this.CheckCancelled(importJob))
                {
                    return;
                }

                var createdBy = Util.GetUserIdByName(importJob, other.CreatedByUserID, other.CreatedByUserName);
                var modifiedBy = Util.GetUserIdByName(importJob, other.LastModifiedByUserID, other.LastModifiedByUserName);
                var local = localRoleGroups.FirstOrDefault(t => t.RoleGroupName == other.RoleGroupName);

                if (local != null)
                {
                    other.LocalId = local.RoleGroupID;
                    switch (importDto.CollisionResolution)
                    {
                        case CollisionResolution.Ignore:
                            this.Result.AddLogEntry("Ignored role group", other.RoleGroupName);
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
                            this.Result.AddLogEntry("Updated role group", other.RoleGroupName);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException(importDto.CollisionResolution.ToString());
                    }
                }
                else
                {
                    var roleGroup = new RoleGroupInfo()
                    {
                        PortalID = portalId,
                        RoleGroupName = other.RoleGroupName,
                        Description = other.Description,
                    };
                    other.LocalId = RoleController.AddRoleGroup(roleGroup);
                    changedGroups.Add(new RoleGroupItem(roleGroup.RoleGroupID, createdBy, modifiedBy));
                    this.Result.AddLogEntry("Added role group", other.RoleGroupName);
                }
            }

            if (changedGroups.Count > 0)
            {
                RefreshRecordsUserIds(changedGroups);
            }
        }

        private void ProcessRoles(ExportImportJob importJob, ImportDto importDto,
            List<ExportRoleGroup> otherRoleGroups, IEnumerable<ExportRole> otherRoles)
        {
            var roleItems = new List<RoleItem>();
            var portalId = importJob.PortalId;
            foreach (var other in otherRoles)
            {
                if (this.CheckCancelled(importJob))
                {
                    return;
                }

                var createdBy = Util.GetUserIdByName(importJob, other.CreatedByUserID, other.CreatedByUserName);
                var modifiedBy = Util.GetUserIdByName(importJob, other.LastModifiedByUserID, other.LastModifiedByUserName);
                var localRoleInfo = RoleController.Instance.GetRoleByName(portalId, other.RoleName);
                if (localRoleInfo != null)
                {
                    other.LocalId = localRoleInfo.RoleID;
                    switch (importDto.CollisionResolution)
                    {
                        case CollisionResolution.Ignore:
                            this.Result.AddLogEntry("Ignored role", other.RoleName);
                            break;
                        case CollisionResolution.Overwrite:
                            var group = other.RoleGroupID.HasValue
                                ? otherRoleGroups.FirstOrDefault(g => g.RoleGroupID == other.RoleGroupID.Value)
                                : null;

                            localRoleInfo.RoleName = other.RoleName;
                            localRoleInfo.AutoAssignment = false; // other.AutoAssignment; CP: said do not do this
                            localRoleInfo.BillingFrequency = other.BillingFrequency;
                            localRoleInfo.BillingPeriod = other.BillingPeriod ?? 0;
                            localRoleInfo.Description = other.Description;
                            localRoleInfo.IconFile = other.IconFile;      // TODO: map to local file
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

                            RoleController.Instance.UpdateRole(localRoleInfo, false);
                            roleItems.Add(new RoleItem(localRoleInfo.RoleID, createdBy, modifiedBy));

                            // do not assign existing users to the roles automatically
                            if (other.AutoAssignment)
                            {
                                DataProvider.Instance().SetRoleAutoAssign(localRoleInfo.RoleID);
                            }

                            RoleController.Instance.ClearRoleCache(localRoleInfo.RoleID);
                            this.Result.AddLogEntry("Updated role", other.RoleName);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException(importDto.CollisionResolution.ToString());
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
                        IconFile = other.IconFile,      // TODO: map to local file
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

                    other.LocalId = RoleController.Instance.AddRole(roleInfo, false);
                    roleItems.Add(new RoleItem(roleInfo.RoleID, createdBy, modifiedBy));
                    RoleController.Instance.ClearRoleCache(roleInfo.RoleID);
                    this.Result.AddLogEntry("Added role", other.RoleName);
                }
            }

            // set created/updated for any added/modified item
            if (roleItems.Count > 0)
            {
                RefreshRecordsUserIds(roleItems);
            }
        }

        private void ProcessRoleSettings(ExportImportJob importJob, ImportDto importDto,
            IList<ExportRole> otherRoles, IEnumerable<ExportRoleSetting> otherRoleSettings)
        {
            var changedSettings = new List<SettingItem>();
            var portalId = importJob.PortalId;
            foreach (var other in otherRoleSettings)
            {
                if (this.CheckCancelled(importJob))
                {
                    return;
                }

                var createdBy = Util.GetUserIdByName(importJob, other.CreatedByUserID, other.CreatedByUserName);
                var modifiedBy = Util.GetUserIdByName(importJob, other.LastModifiedByUserID, other.LastModifiedByUserName);
                var otherRole = otherRoles.FirstOrDefault(r => r.RoleID == other.RoleID);
                if (otherRole == null || !otherRole.LocalId.HasValue)
                {
                    continue;
                }

                var localRoleInfo = RoleController.Instance.GetRoleById(portalId, otherRole.LocalId.Value);
                if (localRoleInfo == null)
                {
                    continue;
                }

                switch (importDto.CollisionResolution)
                {
                    case CollisionResolution.Ignore:
                        this.Result.AddLogEntry("Ignored role setting", other.SettingName);
                        break;
                    case CollisionResolution.Overwrite:
                        string settingValue;
                        if (!localRoleInfo.Settings.TryGetValue(other.SettingName, out settingValue) ||
                            settingValue != other.SettingValue)
                        {
                            changedSettings.Add(new SettingItem(localRoleInfo.RoleID, other.SettingName, createdBy, modifiedBy));
                            localRoleInfo.Settings[other.SettingName] = other.SettingValue;
                            RoleController.Instance.UpdateRoleSettings(localRoleInfo, false);
                            this.Result.AddLogEntry("Updated role setting", other.SettingName);

                            // No need to clear cache as the caller will do it one time at end
                        }
                        else
                        {
                            this.Result.AddLogEntry("Ignored role setting", other.SettingName);
                        }

                        break;
                    default:
                        throw new ArgumentOutOfRangeException(importDto.CollisionResolution.ToString());
                }
            }

            if (changedSettings.Count > 0)
            {
                RefreshRecordsUserIds(changedSettings);
            }

            RoleController.Instance.ClearRoleCache(importJob.PortalId);
        }

        private struct RoleGroupItem
        {
            public RoleGroupItem(int roleGroupId, int createdBy, int modifiedBy)
            {
                this.RoleGroupId = roleGroupId;
                this.CreatedBy = createdBy;
                this.ModifiedBy = modifiedBy;
            }

            public int RoleGroupId { get; }

            public int CreatedBy { get; }

            public int ModifiedBy { get; }
        }

        private struct RoleItem
        {
            public RoleItem(int roleId, int createdBy, int modifiedBy)
            {
                this.RoleId = roleId;
                this.CreatedBy = createdBy;
                this.ModifiedBy = modifiedBy;
            }

            public int RoleId { get; }

            public int CreatedBy { get; }

            public int ModifiedBy { get; }
        }

        private struct SettingItem
        {
            public SettingItem(int roleId, string settingName, int createdBy, int modifiedBy)
            {
                this.RoleId = roleId;
                this.Name = settingName;
                this.CreatedBy = createdBy;
                this.ModifiedBy = modifiedBy;
            }

            public int RoleId { get; }

            public string Name { get; }

            public int CreatedBy { get; }

            public int ModifiedBy { get; }
        }
    }
}

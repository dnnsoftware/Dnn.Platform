using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Security.Principal;
using DotNetNuke.Common;

namespace Dnn.PersonaBar.Security.Components.Checks
{
    public class CheckDiskAcccessPermissions : IAuditCheck
    {
        public string Id => "CheckDiskAccess";

        public bool LazyLoad => false;

        public CheckResult Execute()
        {
            var result = new CheckResult(SeverityEnum.Unverified, Id);
            IList<string> accessErrors = new List<string>();
            try
            {
                accessErrors = CheckAccessToDrives();
            }
            catch (IOException)
            {
                // e.g., a disk error or a drive was not ready
            }
            catch (UnauthorizedAccessException)
            {
                // The caller does not have the required permission.

            }
            catch (System.Security.SecurityException)
            {
                //Some security exception
            }


            if (accessErrors.Count == 0)
            {
                result.Severity = SeverityEnum.Pass;
            }
            else
            {
                result.Severity = SeverityEnum.Warning;
                result.Notes = accessErrors;
            }
            return result;
        }

        #region private methods

        private static IList<string> CheckAccessToDrives()
        {
            var errors = new List<string>();
            var dir = new DirectoryInfo(Globals.ApplicationMapPath);


            while (dir.Parent != null)
            {

                try
                {
                    dir = dir.Parent;
                    var permissions = CheckPermissionOnDir(dir);
                    var isRoot = dir.Name == dir.Root.Name;
                    if (permissions.Create == Yes || permissions.Write == Yes || permissions.Delete == Yes || (!isRoot && permissions.Read == Yes))
                    {
                        errors.Add(GetPermissionText(dir, permissions, isRoot));
                    }
                }
                catch (IOException)
                {
                    // e.g., a disk error or a drive was not ready
                }
                catch (UnauthorizedAccessException)
                {
                    // The caller does not have the required permission.

                }

            }

            var drives = DriveInfo.GetDrives();
            var checkedDrives = new List<string>();
            foreach (var drive in drives.Where(d => d.IsReady && d.RootDirectory.Name != dir.Root.Name))
            {
                try
                {
                    var driveType = drive.DriveType;
                    if (driveType == DriveType.Fixed || driveType == DriveType.Network)
                    {
                        var dir2 = drive.RootDirectory;
                        var key = dir2.FullName.ToLowerInvariant();
                        if (checkedDrives.Contains(key))
                        {
                            continue;
                        }

                        checkedDrives.Add(key);

                        var permissions = CheckPermissionOnDir(dir2);
                        if (permissions.AnyYes)
                        {
                            errors.Add(GetPermissionText(dir2, permissions));
                        }
                    }
                }
                catch (IOException)
                {
                    // e.g., a disk error or a drive was not ready
                }
                catch (UnauthorizedAccessException)
                {
                    // The caller does not have the required permission.
                }
            }

            return errors;
        }

        private static string GetPermissionText(DirectoryInfo dir, Permissions permissions, bool ignoreRead = false)
        {
            var message = ignoreRead
                ? @"{0} - Write:{2}, Create:{3}, Delete:{4}"
                : @"{0} - Read:{1}, Write:{2}, Create:{3}, Delete:{4}";
            return string.Format(@"{0} - Read:{1}, Write:{2}, Create:{3}, Delete:{4}",
                dir.FullName, permissions.Read, permissions.Write, permissions.Create, permissions.Delete);
        }

        private static Permissions CheckPermissionOnDir(DirectoryInfo dir)
        {
            var permissions = new Permissions(No);
            var disSecurity = dir.GetAccessControl(AccessControlSections.Access);
            var accessRules = disSecurity.GetAccessRules(true, true, typeof(SecurityIdentifier));
            var poolIdentity = WindowsIdentity.GetCurrent();
            if (poolIdentity.User != null && poolIdentity.Groups != null)
            {
                foreach (FileSystemAccessRule rule in accessRules)
                {
                    if (poolIdentity.User.Value == rule.IdentityReference.Value || poolIdentity.Groups.Contains(rule.IdentityReference))
                    {
                        if ((rule.FileSystemRights & (FileSystemRights.CreateDirectories | FileSystemRights.CreateFiles)) != 0)
                            if (rule.AccessControlType == AccessControlType.Allow)
                                permissions.Create = Yes;
                            else
                                permissions.SetThenLockCreate(No);

                        if ((rule.FileSystemRights & FileSystemRights.Write) != 0)
                            if (rule.AccessControlType == AccessControlType.Allow)
                                permissions.Write = Yes;
                            else
                                permissions.SetThenLockWrite(No);

                        if ((rule.FileSystemRights & (FileSystemRights.Read | FileSystemRights.ReadData)) != 0)
                            if (rule.AccessControlType == AccessControlType.Allow)
                                permissions.Read = Yes;
                            else
                                permissions.SetThenLockRead(No);

                        if ((rule.FileSystemRights & (FileSystemRights.Delete | FileSystemRights.DeleteSubdirectoriesAndFiles)) != 0)
                            if (rule.AccessControlType == AccessControlType.Allow)
                                permissions.Delete = Yes;
                            else
                                permissions.SetThenLockDelete(No);
                    }
                }
            }

            return permissions;
        }

        #endregion

        #region helpers

        private const char Yes = 'Y';
        private const char No = 'N';

        private class Permissions
        {
            public Permissions(char initial)
            {
                _create = _write = _read = _delete = initial;
            }

            public bool AnyYes
            {
                get { return Create == Yes || Write == Yes || Read == Yes || Delete == Yes; }
            }

            private char _create;
            private char _write;
            private char _read;
            private char _delete;

            private bool _createLocked;
            private bool _writeLocked;
            private bool _readLocked;
            private bool _deleteLocked;

            public char Create
            {
                get { return _create; }
                set { if (!_createLocked) _create = value; }
            }

            public char Write
            {
                get { return _write; }
                set { if (!_writeLocked) _write = value; }
            }

            public char Read
            {
                get { return _read; }
                set { if (!_readLocked) _read = value; }
            }

            public char Delete
            {
                get { return _delete; }
                set { if (!_deleteLocked) _delete = value; }
            }

            public void SetThenLockCreate(char value)
            {
                if (!_createLocked)
                {
                    _createLocked = true;
                    _create = value;
                }
            }

            public void SetThenLockWrite(char value)
            {
                if (!_writeLocked)
                {
                    _writeLocked = true;
                    _write = value;
                }
            }

            public void SetThenLockRead(char value)
            {
                if (!_readLocked)
                {
                    _readLocked = true;
                    _read = value;
                }
            }

            public void SetThenLockDelete(char value)
            {
                if (!_deleteLocked)
                {
                    _deleteLocked = true;
                    _delete = value;
                }
            }
        }

        #endregion
    }
}
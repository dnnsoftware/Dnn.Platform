// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Installer
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.IO;
    using System.Net;
    using System.Text;
    using System.Threading;
    using System.Xml.XPath;

    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Common.Utilities.Internal;
    using DotNetNuke.Entities.Host;
    using DotNetNuke.Services.Installer.Log;
    using DotNetNuke.Services.Installer.Packages;
    using DotNetNuke.UI.Modules;

    using Localization = DotNetNuke.Services.Localization.Localization;

    /// <summary>
    /// The InstallerBase class is a Base Class for all Installer
    ///     classes that need to use Localized Strings.  It provides these strings
    ///     as localized Constants.
    /// </summary>
    public class Util
    {
        // ReSharper disable InconsistentNaming
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1310:FieldNamesMustNotContainUnderscore", Justification = "Breaking Change")]
        public const string DEFAULT_MANIFESTEXT = ".manifest";
        public const string BackupInstallPackageFolder = "App_Data/ExtensionPackages/";
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1310:FieldNamesMustNotContainUnderscore", Justification = "Breaking Change")]
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        public static readonly string ASSEMBLY_Added = GetLocalizedString("ASSEMBLY_Added");
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1310:FieldNamesMustNotContainUnderscore", Justification = "Breaking Change")]
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        public static readonly string ASSEMBLY_AddedBindingRedirect = GetLocalizedString("ASSEMBLY_AddedBindingRedirect");
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1310:FieldNamesMustNotContainUnderscore", Justification = "Breaking Change")]
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        public static readonly string ASSEMBLY_RemovedBindingRedirect = GetLocalizedString("ASSEMBLY_RemovedBindingRedirect");
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1310:FieldNamesMustNotContainUnderscore", Justification = "Breaking Change")]
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        public static readonly string ASSEMBLY_InUse = GetLocalizedString("ASSEMBLY_InUse");
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1310:FieldNamesMustNotContainUnderscore", Justification = "Breaking Change")]
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        public static readonly string ASSEMBLY_Registered = GetLocalizedString("ASSEMBLY_Registered");
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1310:FieldNamesMustNotContainUnderscore", Justification = "Breaking Change")]
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        public static readonly string ASSEMBLY_UnRegistered = GetLocalizedString("ASSEMBLY_UnRegistered");
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1310:FieldNamesMustNotContainUnderscore", Justification = "Breaking Change")]
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        public static readonly string ASSEMBLY_Updated = GetLocalizedString("ASSEMBLY_Updated");
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1310:FieldNamesMustNotContainUnderscore", Justification = "Breaking Change")]
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        public static readonly string AUTHENTICATION_ReadSuccess = GetLocalizedString("AUTHENTICATION_ReadSuccess");
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1310:FieldNamesMustNotContainUnderscore", Justification = "Breaking Change")]
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        public static readonly string AUTHENTICATION_LoginSrcMissing = GetLocalizedString("AUTHENTICATION_LoginSrcMissing");
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1310:FieldNamesMustNotContainUnderscore", Justification = "Breaking Change")]
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        public static readonly string AUTHENTICATION_Registered = GetLocalizedString("AUTHENTICATION_Registered");
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1310:FieldNamesMustNotContainUnderscore", Justification = "Breaking Change")]
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        public static readonly string AUTHENTICATION_SettingsSrcMissing = GetLocalizedString("AUTHENTICATION_SettingsSrcMissing");
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1310:FieldNamesMustNotContainUnderscore", Justification = "Breaking Change")]
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        public static readonly string AUTHENTICATION_TypeMissing = GetLocalizedString("AUTHENTICATION_TypeMissing");
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1310:FieldNamesMustNotContainUnderscore", Justification = "Breaking Change")]
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        public static readonly string AUTHENTICATION_UnRegistered = GetLocalizedString("AUTHENTICATION_UnRegistered");
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1310:FieldNamesMustNotContainUnderscore", Justification = "Breaking Change")]
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        public static readonly string CLEANUP_Processing = GetLocalizedString("CLEANUP_Processing");
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1310:FieldNamesMustNotContainUnderscore", Justification = "Breaking Change")]
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        public static readonly string CLEANUP_ProcessComplete = GetLocalizedString("CLEANUP_ProcessComplete");
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1310:FieldNamesMustNotContainUnderscore", Justification = "Breaking Change")]
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        public static readonly string CLEANUP_ProcessError = GetLocalizedString("CLEANUP_ProcessError");
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1310:FieldNamesMustNotContainUnderscore", Justification = "Breaking Change")]
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        public static readonly string COMPONENT_Installed = GetLocalizedString("COMPONENT_Installed");
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1310:FieldNamesMustNotContainUnderscore", Justification = "Breaking Change")]
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        public static readonly string COMPONENT_Skipped = GetLocalizedString("COMPONENT_Skipped");
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1310:FieldNamesMustNotContainUnderscore", Justification = "Breaking Change")]
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        public static readonly string COMPONENT_RolledBack = GetLocalizedString("COMPONENT_RolledBack");
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1310:FieldNamesMustNotContainUnderscore", Justification = "Breaking Change")]
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        public static readonly string COMPONENT_RollingBack = GetLocalizedString("COMPONENT_RollingBack");
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1310:FieldNamesMustNotContainUnderscore", Justification = "Breaking Change")]
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        public static readonly string COMPONENT_UnInstalled = GetLocalizedString("COMPONENT_UnInstalled");
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1310:FieldNamesMustNotContainUnderscore", Justification = "Breaking Change")]
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        public static readonly string CONFIG_Committed = GetLocalizedString("CONFIG_Committed");
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1310:FieldNamesMustNotContainUnderscore", Justification = "Breaking Change")]
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        public static readonly string CONFIG_RolledBack = GetLocalizedString("CONFIG_RolledBack");
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1310:FieldNamesMustNotContainUnderscore", Justification = "Breaking Change")]
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        public static readonly string CONFIG_Updated = GetLocalizedString("CONFIG_Updated");
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1310:FieldNamesMustNotContainUnderscore", Justification = "Breaking Change")]
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        public static readonly string DNN_Reading = GetLocalizedString("DNN_Reading");
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1310:FieldNamesMustNotContainUnderscore", Justification = "Breaking Change")]
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        public static readonly string DNN_ReadingComponent = GetLocalizedString("DNN_ReadingComponent");
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1310:FieldNamesMustNotContainUnderscore", Justification = "Breaking Change")]
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        public static readonly string DNN_ReadingPackage = GetLocalizedString("DNN_ReadingPackage");
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1310:FieldNamesMustNotContainUnderscore", Justification = "Breaking Change")]
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        public static readonly string DNN_Success = GetLocalizedString("DNN_Success");
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1310:FieldNamesMustNotContainUnderscore", Justification = "Breaking Change")]
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        public static readonly string EVENTMESSAGE_CommandMissing = GetLocalizedString("EVENTMESSAGE_CommandMissing");
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1310:FieldNamesMustNotContainUnderscore", Justification = "Breaking Change")]
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        public static readonly string EVENTMESSAGE_TypeMissing = GetLocalizedString("EVENTMESSAGE_TypeMissing");
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        public static readonly string EXCEPTION = GetLocalizedString("EXCEPTION");
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1310:FieldNamesMustNotContainUnderscore", Justification = "Breaking Change")]
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        public static readonly string EXCEPTION_FileLoad = GetLocalizedString("EXCEPTION_FileLoad");
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1310:FieldNamesMustNotContainUnderscore", Justification = "Breaking Change")]
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        public static readonly string EXCEPTION_FileRead = GetLocalizedString("EXCEPTION_FileRead");
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1310:FieldNamesMustNotContainUnderscore", Justification = "Breaking Change")]
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        public static readonly string EXCEPTION_GlobDotDotNotSupportedInCleanup = GetLocalizedString("EXCEPTION_GlobDotDotNotSupportedInCleanup");
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1310:FieldNamesMustNotContainUnderscore", Justification = "Breaking Change")]
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        public static readonly string EXCEPTION_InstallerCreate = GetLocalizedString("EXCEPTION_InstallerCreate");
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1310:FieldNamesMustNotContainUnderscore", Justification = "Breaking Change")]
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        public static readonly string EXCEPTION_MissingDnn = GetLocalizedString("EXCEPTION_MissingDnn");
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1310:FieldNamesMustNotContainUnderscore", Justification = "Breaking Change")]
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        public static readonly string EXCEPTION_MultipleDnn = GetLocalizedString("EXCEPTION_MultipleDnn");
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1310:FieldNamesMustNotContainUnderscore", Justification = "Breaking Change")]
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        public static readonly string EXCEPTION_NameMissing = GetLocalizedString("EXCEPTION_NameMissing");
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1310:FieldNamesMustNotContainUnderscore", Justification = "Breaking Change")]
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        public static readonly string EXCEPTION_Type = GetLocalizedString("EXCEPTION_Type");
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1310:FieldNamesMustNotContainUnderscore", Justification = "Breaking Change")]
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        public static readonly string EXCEPTION_TypeMissing = GetLocalizedString("EXCEPTION_TypeMissing");
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1310:FieldNamesMustNotContainUnderscore", Justification = "Breaking Change")]
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        public static readonly string EXCEPTION_VersionMissing = GetLocalizedString("EXCEPTION_VersionMissing");
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1310:FieldNamesMustNotContainUnderscore", Justification = "Breaking Change")]
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        public static readonly string FILE_CreateBackup = GetLocalizedString("FILE_CreateBackup");
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1310:FieldNamesMustNotContainUnderscore", Justification = "Breaking Change")]
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        public static readonly string FILE_Created = GetLocalizedString("FILE_Created");
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1310:FieldNamesMustNotContainUnderscore", Justification = "Breaking Change")]
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        public static readonly string FILE_Deleted = GetLocalizedString("FILE_Deleted");
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1310:FieldNamesMustNotContainUnderscore", Justification = "Breaking Change")]
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        public static readonly string FILE_Found = GetLocalizedString("FILE_Found");
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1310:FieldNamesMustNotContainUnderscore", Justification = "Breaking Change")]
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        public static readonly string FILE_Loading = GetLocalizedString("FILE_Loading");
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1310:FieldNamesMustNotContainUnderscore", Justification = "Breaking Change")]
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        public static readonly string FILE_NotAllowed = GetLocalizedString("FILE_NotAllowed");
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1310:FieldNamesMustNotContainUnderscore", Justification = "Breaking Change")]
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        public static readonly string FILE_NotFound = GetLocalizedString("FILE_NotFound");
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1310:FieldNamesMustNotContainUnderscore", Justification = "Breaking Change")]
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        public static readonly string FILE_ReadSuccess = GetLocalizedString("FILE_ReadSuccess");
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1310:FieldNamesMustNotContainUnderscore", Justification = "Breaking Change")]
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        public static readonly string FILE_RestoreBackup = GetLocalizedString("FILE_RestoreBackup");
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1310:FieldNamesMustNotContainUnderscore", Justification = "Breaking Change")]
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        public static readonly string FILES_CreatedResources = GetLocalizedString("FILES_CreatedResources");
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1310:FieldNamesMustNotContainUnderscore", Justification = "Breaking Change")]
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        public static readonly string FILES_Expanding = GetLocalizedString("FILES_Expanding");
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1310:FieldNamesMustNotContainUnderscore", Justification = "Breaking Change")]
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        public static readonly string FILES_Loading = GetLocalizedString("FILES_Loading");
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1310:FieldNamesMustNotContainUnderscore", Justification = "Breaking Change")]
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        public static readonly string FILES_Reading = GetLocalizedString("FILES_Reading");
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1310:FieldNamesMustNotContainUnderscore", Justification = "Breaking Change")]
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        public static readonly string FILES_ReadingEnd = GetLocalizedString("FILES_ReadingEnd");
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1310:FieldNamesMustNotContainUnderscore", Justification = "Breaking Change")]
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        public static readonly string FOLDER_Created = GetLocalizedString("FOLDER_Created");
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1310:FieldNamesMustNotContainUnderscore", Justification = "Breaking Change")]
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        public static readonly string FOLDER_Deleted = GetLocalizedString("FOLDER_Deleted");
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1310:FieldNamesMustNotContainUnderscore", Justification = "Breaking Change")]
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        public static readonly string FOLDER_DeletedBackup = GetLocalizedString("FOLDER_DeletedBackup");
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1310:FieldNamesMustNotContainUnderscore", Justification = "Breaking Change")]
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        public static readonly string INSTALL_Compatibility = GetLocalizedString("INSTALL_Compatibility");
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1310:FieldNamesMustNotContainUnderscore", Justification = "Breaking Change")]
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        public static readonly string INSTALL_Dependencies = GetLocalizedString("INSTALL_Dependencies");
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1310:FieldNamesMustNotContainUnderscore", Justification = "Breaking Change")]
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        public static readonly string INSTALL_Aborted = GetLocalizedString("INSTALL_Aborted");
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1310:FieldNamesMustNotContainUnderscore", Justification = "Breaking Change")]
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        public static readonly string INSTALL_Failed = GetLocalizedString("INSTALL_Failed");
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1310:FieldNamesMustNotContainUnderscore", Justification = "Breaking Change")]
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        public static readonly string INSTALL_Committed = GetLocalizedString("INSTALL_Committed");
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1310:FieldNamesMustNotContainUnderscore", Justification = "Breaking Change")]
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        public static readonly string INSTALL_Namespace = GetLocalizedString("INSTALL_Namespace");
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1310:FieldNamesMustNotContainUnderscore", Justification = "Breaking Change")]
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        public static readonly string INSTALL_Package = GetLocalizedString("INSTALL_Package");
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1310:FieldNamesMustNotContainUnderscore", Justification = "Breaking Change")]
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        public static readonly string INSTALL_Permissions = GetLocalizedString("INSTALL_Permissions");
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1310:FieldNamesMustNotContainUnderscore", Justification = "Breaking Change")]
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        public static readonly string INSTALL_Start = GetLocalizedString("INSTALL_Start");
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1310:FieldNamesMustNotContainUnderscore", Justification = "Breaking Change")]
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        public static readonly string INSTALL_Success = GetLocalizedString("INSTALL_Success");
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1310:FieldNamesMustNotContainUnderscore", Justification = "Breaking Change")]
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        public static readonly string INSTALL_Version = GetLocalizedString("INSTALL_Version");
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1310:FieldNamesMustNotContainUnderscore", Justification = "Breaking Change")]
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        public static readonly string LANGUAGE_PortalsEnabled = GetLocalizedString("LANGUAGE_PortalsEnabled");
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1310:FieldNamesMustNotContainUnderscore", Justification = "Breaking Change")]
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        public static readonly string LANGUAGE_Registered = GetLocalizedString("LANGUAGE_Registered");
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1310:FieldNamesMustNotContainUnderscore", Justification = "Breaking Change")]
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        public static readonly string LANGUAGE_UnRegistered = GetLocalizedString("LANGUAGE_UnRegistered");
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1310:FieldNamesMustNotContainUnderscore", Justification = "Breaking Change")]
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        public static readonly string LIBRARY_ReadSuccess = GetLocalizedString("LIBRARY_ReadSuccess");
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1310:FieldNamesMustNotContainUnderscore", Justification = "Breaking Change")]
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        public static readonly string LIBRARY_Registered = GetLocalizedString("LIBRARY_Registered");
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1310:FieldNamesMustNotContainUnderscore", Justification = "Breaking Change")]
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        public static readonly string LIBRARY_UnRegistered = GetLocalizedString("LIBRARY_UnRegistered");
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1310:FieldNamesMustNotContainUnderscore", Justification = "Breaking Change")]
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        public static readonly string MODULE_ControlKeyMissing = GetLocalizedString("MODULE_ControlKeyMissing");
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1310:FieldNamesMustNotContainUnderscore", Justification = "Breaking Change")]
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        public static readonly string MODULE_ControlTypeMissing = GetLocalizedString("MODULE_ControlTypeMissing");
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1310:FieldNamesMustNotContainUnderscore", Justification = "Breaking Change")]
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        public static readonly string MODULE_FriendlyNameMissing = GetLocalizedString("MODULE_FriendlyNameMissing");
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1310:FieldNamesMustNotContainUnderscore", Justification = "Breaking Change")]
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        public static readonly string MODULE_InvalidVersion = GetLocalizedString("MODULE_InvalidVersion");
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1310:FieldNamesMustNotContainUnderscore", Justification = "Breaking Change")]
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        public static readonly string MODULE_ReadSuccess = GetLocalizedString("MODULE_ReadSuccess");
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1310:FieldNamesMustNotContainUnderscore", Justification = "Breaking Change")]
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        public static readonly string MODULE_Registered = GetLocalizedString("MODULE_Registered");
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1310:FieldNamesMustNotContainUnderscore", Justification = "Breaking Change")]
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        public static readonly string MODULE_UnRegistered = GetLocalizedString("MODULE_UnRegistered");
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1310:FieldNamesMustNotContainUnderscore", Justification = "Breaking Change")]
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        public static readonly string MODULE_AdminPageAdded = GetLocalizedString("MODULE_AdminPageAdded");
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1310:FieldNamesMustNotContainUnderscore", Justification = "Breaking Change")]
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        public static readonly string MODULE_AdminPagemoduleAdded = GetLocalizedString("MODULE_AdminPagemoduleAdded");
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1310:FieldNamesMustNotContainUnderscore", Justification = "Breaking Change")]
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        public static readonly string MODULE_AdminPageRemoved = GetLocalizedString("MODULE_AdminPageRemoved");
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1310:FieldNamesMustNotContainUnderscore", Justification = "Breaking Change")]
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        public static readonly string MODULE_AdminPagemoduleRemoved = GetLocalizedString("MODULE_AdminPagemoduleRemoved");
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1310:FieldNamesMustNotContainUnderscore", Justification = "Breaking Change")]
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        public static readonly string MODULE_HostPageAdded = GetLocalizedString("MODULE_HostPageAdded");
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1310:FieldNamesMustNotContainUnderscore", Justification = "Breaking Change")]
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        public static readonly string MODULE_HostPagemoduleAdded = GetLocalizedString("MODULE_HostPagemoduleAdded");
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1310:FieldNamesMustNotContainUnderscore", Justification = "Breaking Change")]
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        public static readonly string MODULE_HostPageRemoved = GetLocalizedString("MODULE_HostPageRemoved");
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1310:FieldNamesMustNotContainUnderscore", Justification = "Breaking Change")]
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        public static readonly string MODULE_HostPagemoduleRemoved = GetLocalizedString("MODULE_HostPagemoduleRemoved");
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1310:FieldNamesMustNotContainUnderscore", Justification = "Breaking Change")]
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        public static readonly string PACKAGE_NoLicense = GetLocalizedString("PACKAGE_NoLicense");
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1310:FieldNamesMustNotContainUnderscore", Justification = "Breaking Change")]
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        public static readonly string PACKAGE_NoReleaseNotes = GetLocalizedString("PACKAGE_NoReleaseNotes");
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1310:FieldNamesMustNotContainUnderscore", Justification = "Breaking Change")]
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        public static readonly string PACKAGE_UnRecognizable = GetLocalizedString("PACKAGE_UnRecognizable");
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1310:FieldNamesMustNotContainUnderscore", Justification = "Breaking Change")]
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        public static readonly string SECURITY_Installer = GetLocalizedString("SECURITY_Installer");
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1310:FieldNamesMustNotContainUnderscore", Justification = "Breaking Change")]
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        public static readonly string SECURITY_NotRegistered = GetLocalizedString("SECURITY_NotRegistered");
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1310:FieldNamesMustNotContainUnderscore", Justification = "Breaking Change")]
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        public static readonly string SKIN_BeginProcessing = GetLocalizedString("SKIN_BeginProcessing");
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1310:FieldNamesMustNotContainUnderscore", Justification = "Breaking Change")]
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        public static readonly string SKIN_Installed = GetLocalizedString("SKIN_Installed");
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1310:FieldNamesMustNotContainUnderscore", Justification = "Breaking Change")]
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        public static readonly string SKIN_EndProcessing = GetLocalizedString("SKIN_EndProcessing");
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1310:FieldNamesMustNotContainUnderscore", Justification = "Breaking Change")]
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        public static readonly string SKIN_Registered = GetLocalizedString("SKIN_Registered");
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1310:FieldNamesMustNotContainUnderscore", Justification = "Breaking Change")]
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        public static readonly string SKIN_UnRegistered = GetLocalizedString("SKIN_UnRegistered");
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1310:FieldNamesMustNotContainUnderscore", Justification = "Breaking Change")]
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        public static readonly string SQL_Begin = GetLocalizedString("SQL_Begin");
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1310:FieldNamesMustNotContainUnderscore", Justification = "Breaking Change")]
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        public static readonly string SQL_BeginFile = GetLocalizedString("SQL_BeginFile");
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1310:FieldNamesMustNotContainUnderscore", Justification = "Breaking Change")]
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        public static readonly string SQL_BeginUnInstall = GetLocalizedString("SQL_BeginUnInstall");
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1310:FieldNamesMustNotContainUnderscore", Justification = "Breaking Change")]
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        public static readonly string SQL_Committed = GetLocalizedString("SQL_Committed");
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1310:FieldNamesMustNotContainUnderscore", Justification = "Breaking Change")]
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        public static readonly string SQL_End = GetLocalizedString("SQL_End");
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1310:FieldNamesMustNotContainUnderscore", Justification = "Breaking Change")]
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        public static readonly string SQL_EndFile = GetLocalizedString("SQL_EndFile");
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1310:FieldNamesMustNotContainUnderscore", Justification = "Breaking Change")]
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        public static readonly string SQL_EndUnInstall = GetLocalizedString("SQL_EndUnInstall");
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1310:FieldNamesMustNotContainUnderscore", Justification = "Breaking Change")]
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        public static readonly string SQL_Exceptions = GetLocalizedString("SQL_Exceptions");
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1310:FieldNamesMustNotContainUnderscore", Justification = "Breaking Change")]
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        public static readonly string SQL_Executing = GetLocalizedString("SQL_Executing");
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1310:FieldNamesMustNotContainUnderscore", Justification = "Breaking Change")]
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        public static readonly string SQL_Manifest_BadFile = GetLocalizedString("SQL_Manifest_BadFile");
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1310:FieldNamesMustNotContainUnderscore", Justification = "Breaking Change")]
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        public static readonly string SQL_Manifest_Error = GetLocalizedString("SQL_Manifest_Error");
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1310:FieldNamesMustNotContainUnderscore", Justification = "Breaking Change")]
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        public static readonly string SQL_RolledBack = GetLocalizedString("SQL_RolledBack");
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1310:FieldNamesMustNotContainUnderscore", Justification = "Breaking Change")]
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        public static readonly string UNINSTALL_Start = GetLocalizedString("UNINSTALL_Start");
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1310:FieldNamesMustNotContainUnderscore", Justification = "Breaking Change")]
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        public static readonly string UNINSTALL_StartComp = GetLocalizedString("UNINSTALL_StartComp");
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1310:FieldNamesMustNotContainUnderscore", Justification = "Breaking Change")]
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        public static readonly string UNINSTALL_Failure = GetLocalizedString("UNINSTALL_Failure");
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1310:FieldNamesMustNotContainUnderscore", Justification = "Breaking Change")]
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        public static readonly string UNINSTALL_Success = GetLocalizedString("UNINSTALL_Success");
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1310:FieldNamesMustNotContainUnderscore", Justification = "Breaking Change")]
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        public static readonly string UNINSTALL_SuccessComp = GetLocalizedString("UNINSTALL_SuccessComp");
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1310:FieldNamesMustNotContainUnderscore", Justification = "Breaking Change")]
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        public static readonly string UNINSTALL_Warnings = GetLocalizedString("UNINSTALL_Warnings");
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1310:FieldNamesMustNotContainUnderscore", Justification = "Breaking Change")]
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        public static readonly string UNINSTALL_WarningsComp = GetLocalizedString("UNINSTALL_WarningsComp");
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1310:FieldNamesMustNotContainUnderscore", Justification = "Breaking Change")]
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        public static readonly string URLPROVIDER_NameMissing = GetLocalizedString("URLPROVIDER_NameMissing");
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1310:FieldNamesMustNotContainUnderscore", Justification = "Breaking Change")]
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        public static readonly string URLPROVIDER_ReadSuccess = GetLocalizedString("URLPROVIDER_ReadSuccess");
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1310:FieldNamesMustNotContainUnderscore", Justification = "Breaking Change")]
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        public static readonly string URLPROVIDER_Registered = GetLocalizedString("URLPROVIDER_Registered");
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1310:FieldNamesMustNotContainUnderscore", Justification = "Breaking Change")]
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        public static readonly string URLPROVIDER_TypeMissing = GetLocalizedString("URLPROVIDER_TypeMissing");
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1310:FieldNamesMustNotContainUnderscore", Justification = "Breaking Change")]
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        public static readonly string URLPROVIDER_UnRegistered = GetLocalizedString("URLPROVIDER_UnRegistered");
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1310:FieldNamesMustNotContainUnderscore", Justification = "Breaking Change")]
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        public static readonly string WRITER_AddFileToManifest = GetLocalizedString("WRITER_AddFileToManifest");
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1310:FieldNamesMustNotContainUnderscore", Justification = "Breaking Change")]
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        public static readonly string WRITER_CreateArchive = GetLocalizedString("WRITER_CreateArchive");
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1310:FieldNamesMustNotContainUnderscore", Justification = "Breaking Change")]
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        public static readonly string WRITER_CreatedManifest = GetLocalizedString("WRITER_CreatedManifest");
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1310:FieldNamesMustNotContainUnderscore", Justification = "Breaking Change")]
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        public static readonly string WRITER_CreatedPackage = GetLocalizedString("WRITER_CreatedPackage");
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1310:FieldNamesMustNotContainUnderscore", Justification = "Breaking Change")]
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        public static readonly string WRITER_CreatingManifest = GetLocalizedString("WRITER_CreatingManifest");
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1310:FieldNamesMustNotContainUnderscore", Justification = "Breaking Change")]
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        public static readonly string WRITER_CreatingPackage = GetLocalizedString("WRITER_CreatingPackage");
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1310:FieldNamesMustNotContainUnderscore", Justification = "Breaking Change")]
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        public static readonly string WRITER_SavedFile = GetLocalizedString("WRITER_SavedFile");
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1310:FieldNamesMustNotContainUnderscore", Justification = "Breaking Change")]
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        public static readonly string WRITER_SaveFileError = GetLocalizedString("WRITER_SaveFileError");
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1310:FieldNamesMustNotContainUnderscore", Justification = "Breaking Change")]
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        public static readonly string REGEX_Version = "\\d{2}.\\d{2}.\\d{2}";

        // ReSharper restore InconsistentNaming

        /// <summary>The DeleteFile method deletes a file.</summary>
        /// <param name="installFile">The file to delete.</param>
        /// <param name="basePath">The basePath to the file.</param>
        /// <param name="log">A Logger to log the result.</param>
        public static void DeleteFile(InstallFile installFile, string basePath, Logger log)
        {
            DeleteFile(installFile.FullName, basePath, log);
        }

        /// <summary>The DeleteFile method deletes a file.</summary>
        /// <param name="fileName">The file to delete.</param>
        /// <param name="basePath">The basePath to the file.</param>
        /// <param name="log">A Logger to log the result.</param>
        public static void DeleteFile(string fileName, string basePath, Logger log)
        {
            string fullFileName = Path.Combine(basePath, fileName);
            if (File.Exists(fullFileName))
            {
                RetryableAction.RetryEverySecondFor30Seconds(() => FileSystemUtils.DeleteFile(fullFileName), "Delete file " + fullFileName);
                log.AddInfo(string.Format(CultureInfo.InvariantCulture, FILE_Deleted, fileName));
                string folderName = Path.GetDirectoryName(fullFileName);
                if (folderName != null)
                {
                    var folder = new DirectoryInfo(folderName);
                    TryDeleteFolder(folder, log);
                }
            }
        }

        /// <summary>
        /// The GetLocalizedString method provides a convenience wrapper around the
        /// Localization of Strings.
        /// </summary>
        /// <param name="key">The localization key.</param>
        /// <returns>The localized string.</returns>
        public static string GetLocalizedString(string key)
        {
            return Localization.GetString(key, Localization.SharedResourceFile);
        }

        public static bool IsFileValid(InstallFile file, string packageWhiteList)
        {
            // Check the White List
            FileExtensionWhitelist whiteList = Host.AllowedExtensionWhitelist;

            // Check the White Lists
            string strExtension = file.Extension.ToLowerInvariant();
            if (strExtension == "dnn" || whiteList.IsAllowedExtension(strExtension) || packageWhiteList.Contains(strExtension) ||
                 (packageWhiteList.Contains("*dataprovider") && strExtension.EndsWith("dataprovider")))
            {
                // Install File is Valid
                return true;
            }

            return false;
        }

        /// <summary>
        /// The InstallURL method provides a utility method to build the correct url
        /// to install a package (and return to where you came from).
        /// </summary>
        /// <param name="tabId">The id of the tab you are on.</param>
        /// <param name="type">The type of package you are installing.</param>
        /// <returns>The localized string.</returns>
        public static string InstallURL(int tabId, string type)
        {
            var parameters = new string[2];
            parameters[0] = "rtab=" + tabId;
            if (!string.IsNullOrEmpty(type))
            {
                parameters[1] = "ptype=" + type;
            }

            var context = new ModuleInstanceContext();
            return context.NavigateUrl(tabId, "Install", false, parameters);
        }

        public static string InstallURL(int tabId, string returnUrl, string type)
        {
            var parameters = new string[3];
            parameters[0] = "rtab=" + tabId;
            if (!string.IsNullOrEmpty(returnUrl))
            {
                parameters[1] = "returnUrl=" + returnUrl;
            }

            if (!string.IsNullOrEmpty(type))
            {
                parameters[2] = "ptype=" + type;
            }

            var context = new ModuleInstanceContext();
            return context.NavigateUrl(tabId, "Install", false, parameters);
        }

        public static string InstallURL(int tabId, string returnUrl, string type, string package)
        {
            var parameters = new string[4];
            parameters[0] = "rtab=" + tabId;
            if (!string.IsNullOrEmpty(returnUrl))
            {
                parameters[1] = "returnUrl=" + returnUrl;
            }

            if (!string.IsNullOrEmpty(type))
            {
                parameters[2] = "ptype=" + type;
            }

            if (!string.IsNullOrEmpty(package))
            {
                parameters[3] = "package=" + package;
            }

            var context = new ModuleInstanceContext();
            return context.NavigateUrl(tabId, "Install", false, parameters);
        }

        public static string UnInstallURL(int tabId, int packageId, string returnUrl)
        {
            var parameters = new string[3];
            parameters[0] = "rtab=" + tabId;
            parameters[1] = "returnUrl=" + returnUrl;
            parameters[2] = "packageId=" + packageId;
            var context = new ModuleInstanceContext();
            return context.NavigateUrl(tabId, "UnInstall", true, parameters);
        }

        /// <summary>
        /// The PackageWriterURL method provides a utility method to build the correct url
        /// to create a package (and return to where you came from).
        /// </summary>
        /// <param name="context">The ModuleContext of the module.</param>
        /// <param name="packageId">The id of the package you are packaging.</param>
        /// <returns>The localized string.</returns>
        public static string PackageWriterURL(ModuleInstanceContext context, int packageId)
        {
            var parameters = new string[3];
            parameters[0] = "rtab=" + context.TabId;
            parameters[1] = "packageId=" + packageId;
            parameters[2] = "mid=" + context.ModuleId;

            return context.NavigateUrl(context.TabId, "PackageWriter", true, parameters);
        }

        public static string ParsePackageIconFileName(PackageInfo package)
        {
            var filename = string.Empty;
            if ((package.IconFile != null) && (package.PackageType.Equals("Module", StringComparison.OrdinalIgnoreCase) || package.PackageType.Equals("Auth_System", StringComparison.OrdinalIgnoreCase) || package.PackageType.Equals("Container", StringComparison.OrdinalIgnoreCase) || package.PackageType.Equals("Skin", StringComparison.OrdinalIgnoreCase)))
            {
                filename = package.IconFile.StartsWith("~/" + package.FolderName) ? package.IconFile.Remove(0, ("~/" + package.FolderName).Length).TrimStart('/') : package.IconFile;
            }

            return filename;
        }

        public static string ParsePackageIconFile(PackageInfo package)
        {
            var iconFile = string.Empty;
            if ((package.IconFile != null) && (package.PackageType.Equals("Module", StringComparison.OrdinalIgnoreCase) || package.PackageType.Equals("Auth_System", StringComparison.OrdinalIgnoreCase) || package.PackageType.Equals("Container", StringComparison.OrdinalIgnoreCase) || package.PackageType.Equals("Skin", StringComparison.OrdinalIgnoreCase)))
            {
                iconFile = !package.IconFile.StartsWith("~/") ? "~/" + package.FolderName + "/" + package.IconFile : package.IconFile;
            }

            return iconFile;
        }

        public static string ReadAttribute(XPathNavigator nav, string attributeName)
        {
            return ValidateNode(nav.GetAttribute(attributeName, string.Empty), false, null, string.Empty, string.Empty);
        }

        public static string ReadAttribute(XPathNavigator nav, string attributeName, Logger log, string logmessage)
        {
            return ValidateNode(nav.GetAttribute(attributeName, string.Empty), true, log, logmessage, string.Empty);
        }

        public static string ReadAttribute(XPathNavigator nav, string attributeName, bool isRequired, Logger log, string logmessage, string defaultValue)
        {
            return ValidateNode(nav.GetAttribute(attributeName, string.Empty), isRequired, log, logmessage, defaultValue);
        }

        public static string GetPackageBackupName(PackageInfo package)
        {
            var packageName = package.Name;
            var version = package.Version;
            var packageType = package.PackageType;

            var fileName = $"{packageType}_{packageName}_{version}.resources";
            if (fileName.IndexOfAny(Path.GetInvalidFileNameChars()) > Null.NullInteger)
            {
                fileName = Globals.CleanFileName(fileName);
            }

            return fileName;
        }

        public static string GetPackageBackupPath(PackageInfo package)
        {
            var fileName = GetPackageBackupName(package);
            var folderPath = Path.Combine(Globals.ApplicationMapPath, Util.BackupInstallPackageFolder);

            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            return Path.Combine(folderPath, fileName);
        }

        public static string ReadElement(XPathNavigator nav, string elementName)
        {
            return ValidateNode(XmlUtils.GetNodeValue(nav, elementName), false, null, string.Empty, string.Empty);
        }

        public static string ReadElement(XPathNavigator nav, string elementName, string defaultValue)
        {
            return ValidateNode(XmlUtils.GetNodeValue(nav, elementName), false, null, string.Empty, defaultValue);
        }

        public static string ReadElement(XPathNavigator nav, string elementName, Logger log, string logmessage)
        {
            return ValidateNode(XmlUtils.GetNodeValue(nav, elementName), true, log, logmessage, string.Empty);
        }

        public static string ReadElement(XPathNavigator nav, string elementName, bool isRequired, Logger log, string logmessage, string defaultValue)
        {
            return ValidateNode(XmlUtils.GetNodeValue(nav, elementName), isRequired, log, logmessage, defaultValue);
        }

        /// <summary>The RestoreFile method restores a file from the backup folder.</summary>
        /// <param name="installFile">The file to restore.</param>
        /// <param name="basePath">The basePath to the file.</param>
        /// <param name="log">A Logger to log the result.</param>
        public static void RestoreFile(InstallFile installFile, string basePath, Logger log)
        {
            string fullFileName = Path.Combine(basePath, installFile.FullName);
            string backupFileName = Path.Combine(installFile.BackupPath, installFile.Name + ".config");

            // Copy File back over install file
            FileSystemUtils.CopyFile(backupFileName, fullFileName);

            log.AddInfo(string.Format(CultureInfo.InvariantCulture, FILE_RestoreBackup, installFile.FullName));
        }

        /// <summary>
        /// The UnInstallURL method provides a utility method to build the correct url
        /// to uninstall a package (and return to where you came from).
        /// </summary>
        /// <param name="tabId">The id of the tab you are on.</param>
        /// <param name="packageId">The id of the package you are uninstalling.</param>
        /// <returns>The localized string.</returns>
        public static string UnInstallURL(int tabId, int packageId)
        {
            var parameters = new string[2];
            parameters[0] = "rtab=" + tabId;
            parameters[1] = "packageId=" + packageId;
            var context = new ModuleInstanceContext();
            return context.NavigateUrl(tabId, "UnInstall", true, parameters);
        }

        /// <summary>The WriteStream reads a source stream and writes it to a destination file.</summary>
        /// <param name="sourceStream">The Source Stream.</param>
        /// <param name="destFileName">The Destination file.</param>
        public static void WriteStream(Stream sourceStream, string destFileName)
        {
            var file = new FileInfo(destFileName);
            if (file.Directory != null && !file.Directory.Exists)
            {
                file.Directory.Create();
            }

            // HACK: Temporary fix, upping retry limit due to locking for existing filesystem access.  This "fixes" azure, but isn't the most elegant
            TryToCreateAndExecute(destFileName, (f) => StreamToStream(sourceStream, f), 3500);
        }

        /// <summary>Try to create file and perform an action on a file until a specific amount of time.</summary>
        /// <param name="path">Path of the file.</param>
        /// <param name="action">Action to execute on file.</param>
        /// <param name="milliSecondMax">Maimum amount of time to try to do the action.</param>
        /// <returns>true if action occur and false otherwise.</returns>
        public static bool TryToCreateAndExecute(string path, Action<FileStream> action, int milliSecondMax = Timeout.Infinite)
        {
            var started = DateTime.UtcNow;
            var fullPath = Path.GetFullPath(path);
            var directory = Path.GetDirectoryName(fullPath)!;

            AutoResetEvent gate = null;
            FileSystemWatcher watcher = null;
            FileSystemEventHandler changedHandler = null;
            RenamedEventHandler renamedHandler = null;

            try
            {
                while (true)
                {
                    try
                    {
                        // Open for create with shared read/write; dispose via using
                        using var file = File.Open(fullPath, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite);
                        action(file);
                        return true;
                    }
                    catch (IOException)
                    {
                        // Lazily create the wait handle + watcher once
                        if (gate is null)
                        {
                            gate = new AutoResetEvent(false);

                            watcher = new FileSystemWatcher(directory)
                            {
                                Filter = Path.GetFileName(fullPath),
                                NotifyFilter = NotifyFilters.FileName
                                             | NotifyFilters.LastWrite
                                             | NotifyFilters.CreationTime
                                             | NotifyFilters.Size,
                                EnableRaisingEvents = true,
                            };

                            changedHandler = (o, e) =>
                            {
                                // Extra safety: only signal for the exact file
                                if (string.Equals(Path.GetFullPath(e.FullPath), fullPath, StringComparison.OrdinalIgnoreCase))
                                {
                                    gate.Set();
                                }
                            };
                            renamedHandler = (o, e) =>
                            {
                                // Extra safety: only signal for the exact file
                                if (string.Equals(Path.GetFullPath(e.FullPath), fullPath, StringComparison.OrdinalIgnoreCase))
                                {
                                    gate.Set();
                                }
                            };

                            watcher.Changed += changedHandler;
                            watcher.Created += changedHandler;
                            watcher.Renamed += renamedHandler;
                            watcher.Deleted += changedHandler;
                        }

                        // Compute remaining time correctly
                        int waitMs = Timeout.Infinite;
                        if (milliSecondMax != Timeout.Infinite)
                        {
                            var elapsed = (int)(DateTime.UtcNow - started).TotalMilliseconds;
                            var remaining = milliSecondMax - elapsed;
                            if (remaining <= 0)
                            {
                                return false;
                            }

                            waitMs = remaining;
                        }

                        // Wait for a change (or until remaining timeout)
                        gate!.WaitOne(waitMs);
                    }
                }
            }
            finally
            {
                // Clean shutdown: unsubscribe then dispose in reverse order
                if (watcher != null && changedHandler != null)
                {
                    watcher.Changed -= changedHandler;
                    watcher.Created -= changedHandler;
                    watcher.Renamed -= renamedHandler;
                    watcher.Deleted -= changedHandler;
                }

                watcher?.Dispose();
                gate?.Dispose();
            }
        }

        public static WebResponse GetExternalRequest(string url, byte[] data, string username, string password, string domain, string proxyAddress, int proxyPort, bool doPOST, string userAgent, string referer, out string filename)
        {
            return GetExternalRequest(url, data, username, password, domain, proxyAddress, proxyPort, doPOST, userAgent, referer, out filename, Host.WebRequestTimeout);
        }

        public static WebResponse GetExternalRequest(string url, byte[] data, string username, string password, string domain, string proxyAddress, int proxyPort, bool doPOST, string userAgent, string referer, out string filename, int requestTimeout)
        {
            return GetExternalRequest(url, data, username, password, domain, proxyAddress, proxyPort, string.Empty, string.Empty, doPOST, userAgent, referer, out filename, requestTimeout);
        }

        public static WebResponse GetExternalRequest(string url, byte[] data, string username, string password, string domain, string proxyAddress, int proxyPort, string proxyUsername, string proxyPassword, bool doPOST, string userAgent, string referer, out string filename)
        {
            return GetExternalRequest(url, data, username, password, domain, proxyAddress, proxyPort, proxyUsername, proxyPassword, doPOST, userAgent, referer, out filename, Host.WebRequestTimeout);
        }

        public static WebResponse GetExternalRequest(string url, byte[] data, string username, string password, string domain, string proxyAddress, int proxyPort, string proxyUsername, string proxyPassword, bool doPOST, string userAgent, string referer, out string filename, int requestTimeout)
        {
            if (!doPOST && data != null && data.Length > 0)
            {
                string restoftheurl = Encoding.ASCII.GetString(data);
                if (url != null && url.IndexOf("?") <= 0)
                {
                    url = url + "?";
                }

                url = url + restoftheurl;
            }

            var wreq = (HttpWebRequest)WebRequest.Create(url);
            wreq.UserAgent = userAgent;
            wreq.Referer = referer;
            wreq.Method = "GET";
            if (doPOST)
            {
                wreq.Method = "POST";
            }

            wreq.Timeout = requestTimeout;

            if (!string.IsNullOrEmpty(proxyAddress))
            {
                var proxy = new WebProxy(proxyAddress, proxyPort);
                if (!string.IsNullOrEmpty(proxyUsername))
                {
                    var proxyCredentials = new NetworkCredential(proxyUsername, proxyPassword);
                    proxy.Credentials = proxyCredentials;
                }

                wreq.Proxy = proxy;
            }

            if (username != null && password != null && domain != null && username.Trim() != string.Empty && password.Trim() != null && domain.Trim() != null)
            {
                wreq.Credentials = new NetworkCredential(username, password, domain);
            }
            else if (username != null && password != null && username.Trim() != string.Empty && password.Trim() != null)
            {
                wreq.Credentials = new NetworkCredential(username, password);
            }

            if (doPOST && data != null && data.Length > 0)
            {
                wreq.ContentType = "application/x-www-form-urlencoded";
                Stream request = wreq.GetRequestStream();
                request.Write(data, 0, data.Length);
                request.Close();
            }

            filename = string.Empty;
            WebResponse wrsp = wreq.GetResponse();
            string cd = wrsp.Headers["Content-Disposition"];
            if (cd != null && cd.Trim() != string.Empty && cd.StartsWith("attachment"))
            {
                if (cd.IndexOf("filename") > -1 && cd.Substring(cd.IndexOf("filename")).IndexOf("=") > -1)
                {
                    string filenameParam = cd.Substring(cd.IndexOf("filename"));

                    if (filenameParam.IndexOf("\"") > -1)
                    {
                        filename = filenameParam.Substring(filenameParam.IndexOf("\"") + 1).TrimEnd(Convert.ToChar("\"")).TrimEnd(Convert.ToChar("\\"));
                    }
                    else
                    {
                        filename = filenameParam.Substring(filenameParam.IndexOf("=") + 1);
                    }
                }
            }

            return wrsp;
        }

        public static void DeployExtension(WebResponse wr, string myfile, string installFolder)
        {
            Stream remoteStream = null;
            FileStream localStream = null;

            try
            {
                // Once the WebResponse object has been retrieved,
                // get the stream object associated with the response's data
                remoteStream = wr.GetResponseStream();

                // Create the local file with zip extension to ensure installation
                localStream = File.Create(installFolder + "/" + myfile);

                // Allocate a 1k buffer
                var buffer = new byte[1024];
                int bytesRead;

                // Simple do/while loop to read from stream until
                // no bytes are returned
                do
                {
                    // Read data (up to 1k) from the stream
                    bytesRead = remoteStream.Read(buffer, 0, buffer.Length);

                    // Write the data to the local file
                    localStream.Write(buffer, 0, bytesRead);
                }
                while (bytesRead > 0);
            }
            finally
            {
                // Close the response and streams objects here
                // to make sure they're closed even if an exception
                // is thrown at some point
                if (remoteStream != null)
                {
                    remoteStream.Close();
                }

                if (localStream != null)
                {
                    localStream.Close();
                }
            }
        }

        /// <summary>The BackupFile method backs up a file to the backup folder.</summary>
        /// <param name="installFile">The file to backup.</param>
        /// <param name="basePath">The basePath to the file.</param>
        /// <param name="log">A Logger to log the result.</param>
        public static void BackupFile(InstallFile installFile, string basePath, Logger log)
        {
            string fullFileName = Path.Combine(basePath, installFile.FullName);
            string backupFileName = Path.Combine(installFile.BackupPath, installFile.Name + ".config");

            // create the backup folder if necessary
            if (!Directory.Exists(installFile.BackupPath))
            {
                Directory.CreateDirectory(installFile.BackupPath);
            }

            // Copy file to the backup location
            RetryableAction.RetryEverySecondFor30Seconds(() => FileSystemUtils.CopyFile(fullFileName, backupFileName), "Backup file " + fullFileName);
            log.AddInfo(string.Format(CultureInfo.InvariantCulture, FILE_CreateBackup, installFile.FullName));
        }

        /// <summary>The CopyFile method copies a file from the temporary extract location.</summary>
        /// <param name="installFile">The file to copy.</param>
        /// <param name="basePath">The basePath to the file.</param>
        /// <param name="log">A Logger to log the result.</param>
        public static void CopyFile(InstallFile installFile, string basePath, Logger log)
        {
            string filePath = Path.Combine(basePath, installFile.Path);
            string fullFileName = Path.Combine(basePath, installFile.FullName);

            // create the folder if necessary
            if (!Directory.Exists(filePath))
            {
                log.AddInfo(string.Format(CultureInfo.InvariantCulture, FOLDER_Created, filePath));
                Directory.CreateDirectory(filePath);
            }

            // Copy file from temp location
            RetryableAction.RetryEverySecondFor30Seconds(() => FileSystemUtils.CopyFile(installFile.TempFileName, fullFileName), "Copy file to " + fullFileName);

            log.AddInfo(string.Format(CultureInfo.InvariantCulture, FILE_Created, installFile.FullName));
        }

        /// <summary>The StreamToStream method reads a source stream and writes it to a destination stream.</summary>
        /// <param name="sourceStream">The Source Stream.</param>
        /// <param name="destStream">The Destination Stream.</param>
        private static void StreamToStream(Stream sourceStream, Stream destStream)
        {
            var buf = new byte[1024];
            int count;
            do
            {
                // Read the chunk from the source
                count = sourceStream.Read(buf, 0, 1024);

                // Write the chunk to the destination
                destStream.Write(buf, 0, count);
            }
            while (count > 0);
            destStream.Flush();
        }

        private static void TryDeleteFolder(DirectoryInfo folder, Logger log)
        {
            if (folder.GetFiles().Length == 0 && folder.GetDirectories().Length == 0)
            {
                folder.Delete();
                log.AddInfo(string.Format(CultureInfo.InvariantCulture, FOLDER_Deleted, folder.Name));
                TryDeleteFolder(folder.Parent, log);
            }
        }

        private static string ValidateNode(string propValue, bool isRequired, Logger log, string logMessage, string defaultValue)
        {
            if (string.IsNullOrEmpty(propValue))
            {
                if (isRequired)
                {
                    // Log Error
                    log.AddFailure(logMessage);
                }
                else
                {
                    // Use Default
                    propValue = defaultValue;
                }
            }

            return propValue;
        }
    }
}

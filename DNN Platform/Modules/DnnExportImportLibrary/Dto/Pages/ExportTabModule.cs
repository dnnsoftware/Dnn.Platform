// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

// ReSharper disable InconsistentNaming
namespace Dnn.ExportImport.Dto.Pages;

using System;

public class ExportTabModule : BasicExportImportDto
{
    public int TabModuleID { get; set; }

    public int TabID { get; set; }

    public int ModuleID { get; set; }

    public string PaneName { get; set; }

    public int ModuleOrder { get; set; }

    public int CacheTime { get; set; }

    public string Alignment { get; set; }

    public string Color { get; set; }

    public string Border { get; set; }

    public string IconFile { get; set; }

    public int Visibility { get; set; }

    public string ContainerSrc { get; set; }

    public bool DisplayTitle { get; set; }

    public bool DisplayPrint { get; set; }

    public bool DisplaySyndicate { get; set; }

    [Obsolete("Deprecated in DotNetNuke 9.8.0. WebSlice functionality is no longer used. Scheduled removal in v10.0.0.")]
    public bool IsWebSlice { get; set; }

    [Obsolete("Deprecated in DotNetNuke 9.8.0. WebSlice functionality is no longer used. Scheduled removal in v10.0.0.")]
    public string WebSliceTitle { get; set; }

    [Obsolete("Deprecated in DotNetNuke 9.8.0. WebSlice functionality is no longer used. Scheduled removal in v10.0.0.")]
    public DateTime? WebSliceExpiryDate { get; set; }

    [Obsolete("Deprecated in DotNetNuke 9.8.0. WebSlice functionality is no longer used. Scheduled removal in v10.0.0.")]
    public int? WebSliceTTL { get; set; }

    public int? CreatedByUserID { get; set; }

    public DateTime? CreatedOnDate { get; set; }

    public int? LastModifiedByUserID { get; set; }

    public DateTime? LastModifiedOnDate { get; set; }

    public bool IsDeleted { get; set; }

    public string CacheMethod { get; set; }

    public string ModuleTitle { get; set; }

    public string Header { get; set; }

    public string Footer { get; set; }

    public string CultureCode { get; set; }

    public Guid UniqueId { get; set; }

    public Guid VersionGuid { get; set; }

    public Guid? DefaultLanguageGuid { get; set; }

    public Guid LocalizedVersionGuid { get; set; }

    public bool InheritViewPermissions { get; set; }

    public bool IsShareable { get; set; }

    public bool IsShareableViewOnly { get; set; }

    public string FriendlyName { get; set; } // this is ModuleDefinition.FriendlyName

    public string CreatedByUserName { get; set; }

    public string LastModifiedByUserName { get; set; }
}

﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

using System;
using Dnn.ExportImport.Components.Common;
using Dnn.ExportImport.Components.Interfaces;
using DotNetNuke.Entities.Users;
using Newtonsoft.Json;

namespace Dnn.ExportImport.Components.Dto
{
    [JsonObject]
    public class ImportPackageInfo : IDateTimeConverter
    {
        /// <summary>
        /// Package Id. Used to identify the package and path.
        /// </summary>
        public string PackageId { get; set; }

        /// <summary>
        /// Name of the package.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Package file name. It is just fake name for UI representation
        /// </summary>
        public string FileName => this.PackageId;

        /// <summary>
        /// DateTime when the package was exported.
        /// </summary>
        public DateTime ExporTime { get; set; }

        /// <summary>
        /// Formatted DateTime when the package was exported.
        /// </summary>
        public string ExporTimeString => Util.GetDateTimeString(this.ExporTime);

        /// <summary>
        /// The portal from which the exported package was created
        /// </summary>
        public string PortalName { get; set; }

        /// <summary>
        /// Package description
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Path to the thumbnail image for the package.
        /// </summary>
        public string Thumb => this.PackageId + ".jpg";

        /// <summary>
        /// Complete summary of import package
        /// </summary>
        public ImportExportSummary Summary { get; set; }

        public void ConvertToLocal(UserInfo userInfo)
        {
            if (userInfo == null) return;
            this.ExporTime = Util.ToLocalDateTime(this.ExporTime, userInfo);
            this.Summary?.ConvertToLocal(userInfo);
        }
    }
}

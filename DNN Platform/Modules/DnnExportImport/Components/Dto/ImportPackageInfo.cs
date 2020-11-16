// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.ExportImport.Components.Dto
{
    using System;

    using Dnn.ExportImport.Components.Common;
    using Dnn.ExportImport.Components.Interfaces;
    using DotNetNuke.Entities.Users;
    using Newtonsoft.Json;

    [JsonObject]
    public class ImportPackageInfo : IDateTimeConverter
    {
        /// <summary>
        /// Gets package file name. It is just fake name for UI representation.
        /// </summary>
        public string FileName => this.PackageId;

        /// <summary>
        /// Gets formatted DateTime when the package was exported.
        /// </summary>
        public string ExporTimeString => Util.GetDateTimeString(this.ExporTime);

        /// <summary>
        /// Gets path to the thumbnail image for the package.
        /// </summary>
        public string Thumb => this.PackageId + ".jpg";

        /// <summary>
        /// Gets or sets package Id. Used to identify the package and path.
        /// </summary>
        public string PackageId { get; set; }

        /// <summary>
        /// Gets or sets name of the package.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets dateTime when the package was exported.
        /// </summary>
        public DateTime ExporTime { get; set; }

        /// <summary>
        /// Gets or sets the portal from which the exported package was created.
        /// </summary>
        public string PortalName { get; set; }

        /// <summary>
        /// Gets or sets package description.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets complete summary of import package.
        /// </summary>
        public ImportExportSummary Summary { get; set; }

        public void ConvertToLocal(UserInfo userInfo)
        {
            if (userInfo == null)
            {
                return;
            }

            this.ExporTime = Util.ToLocalDateTime(this.ExporTime, userInfo);
            this.Summary?.ConvertToLocal(userInfo);
        }
    }
}

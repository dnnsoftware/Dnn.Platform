// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.ExportImport.Components.Dto
{
    using Dnn.ExportImport.Components.Common;
    using Newtonsoft.Json;

    [JsonObject]
    public class ImportDto
    {
        /// <summary>
        /// Gets or sets specifies the version of the exportes schema.
        /// </summary>
        public string SchemaVersion { get; set; } = Constants.CurrentSchemaVersion;

        /// <summary>
        /// Gets or sets iD of portal to import items to.
        /// </summary>
        public int PortalId { get; set; }

        /// <summary>
        /// Gets or sets id of exported package to be imported.
        /// </summary>
        /// <remarks>
        /// For security reasons, this name does not have a folder or extension name.
        /// These will be used by convention and picked from a very specific location
        /// in the portal (<see cref="Constants.ExportFolder"/>).
        /// </remarks>
        public string PackageId { get; set; }

        /// <summary>
        /// Gets or sets specifies what to do when there is a collision during the import process.
        /// See <see cref="CollisionResolution"/>.
        /// </summary>
        public CollisionResolution CollisionResolution { get; set; } = CollisionResolution.Ignore;

        /// <summary>
        /// Gets or sets snapshot of the export dto from the import package.
        /// </summary>
        public ExportDto ExportDto { get; set; }

        /// <summary>
        /// Gets or sets snapshot of the import file into.
        /// </summary>
        public ExportFileInfo ExportFileInfo { get; set; }

        public bool RunNow { get; set; }
    }
}

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Build
{
    /// <summary>Settings about a local dev site.</summary>
    public class LocalSettings
    {
        /// <summary>Gets or sets the path to the website.</summary>
        public string WebsitePath { get; set; } = string.Empty;

        /// <summary>Gets or sets the URL for the website.</summary>
        public string WebsiteUrl { get; set; } = string.Empty;

        /// <summary>Gets or sets the admin connection string for the database.</summary>
        public string SaConnectionString { get; set; } = "server=(local);Trusted_Connection=True;";

        /// <summary>Gets or sets the website connection string for the database.</summary>
        public string DnnConnectionString { get; set; } = string.Empty;

        /// <summary>Gets or sets the default database schema.</summary>
        public string DbOwner { get; set; } = "dbo";

        /// <summary>Gets or sets the object prefix.</summary>
        public string ObjectQualifier { get; set; } = string.Empty;

        /// <summary>Gets or sets the database name.</summary>
        public string DnnDatabaseName { get; set; } = "Dnn_Platform";

        /// <summary>Gets or sets the SQL username with access to the database.</summary>
        public string DnnSqlUsername { get; set; } = string.Empty;

        /// <summary>Gets or sets the path to the database files.</summary>
        public string DatabasePath { get; set; } = string.Empty;

        /// <summary>Gets or sets a value indicating whether to copy the sample projects to the build output.</summary>
        public bool CopySampleProjects { get; set; } = false;

        /// <summary>Gets or sets the version to use for the build.</summary>
        public string Version { get; set; } = "auto";
    }
}

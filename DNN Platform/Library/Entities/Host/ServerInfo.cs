// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Entities.Host
{
    using System;
    using System.Data;

    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Modules;

    [Serializable]
    public class ServerInfo : IHydratable
    {
        public ServerInfo()
            : this(DateTime.Now, DateTime.Now)
        {
        }

        public ServerInfo(DateTime created, DateTime lastactivity)
        {
            this.IISAppName = Globals.IISAppName;
            this.ServerName = Globals.ServerName;
            this.ServerGroup = string.Empty;
            this.CreatedDate = created;
            this.LastActivityDate = lastactivity;
            this.Enabled = true;
        }

        public int ServerID { get; set; }

        public string IISAppName { get; set; }

        public string ServerGroup { get; set; }

        public string ServerName { get; set; }

        public string Url { get; set; }

        public bool Enabled { get; set; }

        public DateTime CreatedDate { get; set; }

        public DateTime LastActivityDate { get; set; }

        public int PingFailureCount { get; set; }

        public string UniqueId { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets and sets the Key ID.
        /// </summary>
        /// <returns>An Integer.</returns>
        /// -----------------------------------------------------------------------------
        public int KeyID
        {
            get
            {
                return this.ServerID;
            }

            set
            {
                this.ServerID = value;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Fills a ServerInfo from a Data Reader.
        /// </summary>
        /// <param name="dr">The Data Reader to use.</param>
        /// -----------------------------------------------------------------------------
        public void Fill(IDataReader dr)
        {
            this.ServerID = Null.SetNullInteger(dr["ServerID"]);
            this.IISAppName = Null.SetNullString(dr["IISAppName"]);
            this.ServerName = Null.SetNullString(dr["ServerName"]);
            this.Url = Null.SetNullString(dr["URL"]);
            this.Enabled = Null.SetNullBoolean(dr["Enabled"]);
            this.CreatedDate = Null.SetNullDateTime(dr["CreatedDate"]);
            this.LastActivityDate = Null.SetNullDateTime(dr["LastActivityDate"]);

            var schema = dr.GetSchemaTable();
            if (schema != null)
            {
                if (schema.Select("ColumnName = 'PingFailureCount'").Length > 0)
                {
                    this.PingFailureCount = Null.SetNullInteger(dr["PingFailureCount"]);
                }

                if (schema.Select("ColumnName = 'ServerGroup'").Length > 0)
                {
                    this.ServerGroup = Null.SetNullString(dr["ServerGroup"]);
                }

                if (schema.Select("ColumnName = 'UniqueId'").Length > 0)
                {
                    this.UniqueId = Null.SetNullString(dr["UniqueId"]);
                }
            }
        }
    }
}

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.Modules.BulkInstall.Components.DataAccess.Models
{
    using System;

    using DotNetNuke.ComponentModel.DataAnnotations;

    /// <summary>The status of a <see cref="Session"/>.</summary>
    public enum SessionStatus
    {
        /// <summary>A session that has not started its installation.</summary>
        NotStarted = 0,

        /// <summary>A session with an in progress installation.</summary>
        InProgress = 1,

        /// <summary>A session that has completed its installation.</summary>
        Complete = 2,
    }

    /// <summary>A session to which packages can be added and then installed.</summary>
    [TableName("BulkInstall_Sessions")]
    [PrimaryKey("SessionID")]
    public class Session
    {
        /// <summary>Initializes a new instance of the <see cref="Session"/> class.</summary>
        public Session()
        {
        }

        /// <summary>Initializes a new instance of the <see cref="Session"/> class.</summary>
        /// <param name="sessionGuid">The public identifier of the session.</param>
        public Session(string sessionGuid)
        {
            this.SessionGuid = sessionGuid;
            this.Status = SessionStatus.NotStarted;
            this.LastUsed = DateTime.Now;
        }

        /// <summary>Gets or sets the internal ID of the session.</summary>
        [ColumnName("SessionID")]
        public int SessionId { get; set; }

        /// <summary>Gets or sets the public ID of the session.</summary>
        [ColumnName("Guid")]
        public string SessionGuid { get; set; }

        /// <summary>Gets or sets the status of the session.</summary>
        public SessionStatus Status { get; set; }

        /// <summary>Gets or sets the session response (as a JSON string).</summary>
        public string Response { get; set; }

        /// <summary>Gets or sets the date/time the session was last used.</summary>
        public DateTime LastUsed { get; set; }
    }
}

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

// ReSharper disable MemberCanBePrivate.Global
namespace DotNetNuke.Security.Cookies
{
    using System;
    using System.Data;

    using DotNetNuke.Common.Utilities;
    using DotNetNuke.ComponentModel.DataAnnotations;
    using DotNetNuke.Entities.Modules;

    [Serializable]
    [TableName("AuthCookies")]
    [PrimaryKey("CookieId")]
    public class PersistedAuthCookie : IHydratable
    {
        public int CookieId { get; private set; }

        public string CookieValue { get; private set; }

        public DateTime ExpiresOn { get; private set; } // UTC

        public int KeyID
        {
            get { return this.CookieId; }
            set { this.CookieId = value; }
        }

        public void Fill(IDataReader dr)
        {
            this.CookieId = Null.SetNullInteger(dr[nameof(this.CookieId)]);
            this.CookieValue = Null.SetNullString(dr[nameof(this.CookieValue)]);
            this.ExpiresOn = Null.SetNullDateTime(dr[nameof(this.ExpiresOn)]);

            if (this.ExpiresOn.Kind != DateTimeKind.Utc)
            {
                this.ExpiresOn = new DateTime(
                    this.ExpiresOn.Year, this.ExpiresOn.Month, this.ExpiresOn.Day,
                    this.ExpiresOn.Hour, this.ExpiresOn.Minute, this.ExpiresOn.Second,
                    this.ExpiresOn.Millisecond, DateTimeKind.Utc);
            }
        }
    }
}

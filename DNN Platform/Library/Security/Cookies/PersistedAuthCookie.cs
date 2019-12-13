// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using System;
using System.Data;
using DotNetNuke.Common.Utilities;
using DotNetNuke.ComponentModel.DataAnnotations;
using DotNetNuke.Entities.Modules;
// ReSharper disable MemberCanBePrivate.Global

namespace DotNetNuke.Security.Cookies
{
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
            get { return CookieId; }
            set { CookieId = value; }
        }

        public void Fill(IDataReader dr)
        {
            CookieId = Null.SetNullInteger(dr[nameof(CookieId)]);
            CookieValue = Null.SetNullString(dr[nameof(CookieValue)]);
            ExpiresOn = Null.SetNullDateTime(dr[nameof(ExpiresOn)]);

            if (ExpiresOn.Kind != DateTimeKind.Utc)
            {
                ExpiresOn = new DateTime(
                    ExpiresOn.Year, ExpiresOn.Month, ExpiresOn.Day,
                    ExpiresOn.Hour, ExpiresOn.Minute, ExpiresOn.Second,
                    ExpiresOn.Millisecond, DateTimeKind.Utc);
            }
        }
    }
}

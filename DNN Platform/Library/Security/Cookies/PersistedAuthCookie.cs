// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2017
// by DotNetNuke Corporation
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation 
// the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and 
// to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions 
// of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
// TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
// CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
// DEALINGS IN THE SOFTWARE.

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
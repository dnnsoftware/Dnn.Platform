// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
#region Usings

using System.Globalization;

using DotNetNuke.Entities.Controllers;
using DotNetNuke.Entities.Users;
using DotNetNuke.Services.Tokens;

#endregion

namespace DotNetNuke.Entities.Host
{
    public class HostPropertyAccess : DictionaryPropertyAccess
    {
        public HostPropertyAccess() : base(HostController.Instance.GetSettingsDictionary())
        {
        }

        public override string GetProperty(string propertyName, string format, CultureInfo formatProvider, UserInfo AccessingUser, Scope CurrentScope, ref bool PropertyNotFound)
        {
            if (propertyName.ToLowerInvariant() == "hosttitle" || CurrentScope == Scope.Debug)
            {
                return base.GetProperty(propertyName, format, formatProvider, AccessingUser, CurrentScope, ref PropertyNotFound);
            }
            else
            {
                PropertyNotFound = true;
                return PropertyAccess.ContentLocked;
            }
        }
    }
}

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Entities.Host
{
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;

    using DotNetNuke.Entities.Controllers;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Services.Tokens;

    public class HostPropertyAccess : DictionaryPropertyAccess
    {
        /// <summary>Initializes a new instance of the <see cref="HostPropertyAccess"/> class.</summary>
        public HostPropertyAccess()
            : base(HostController.Instance.GetSettingsDictionary())
        {
        }

        /// <inheritdoc/>
        [SuppressMessage("Microsoft.Naming", "CA1725:ParameterNamesShouldMatchBaseDeclaration", Justification = "Breaking change")]
        public override string GetProperty(string propertyName, string format, CultureInfo formatProvider, UserInfo accessingUser, Scope currentScope, ref bool propertyNotFound)
        {
            if (propertyName.Equals("hosttitle", System.StringComparison.OrdinalIgnoreCase) || currentScope == Scope.Debug)
            {
                return base.GetProperty(propertyName, format, formatProvider, accessingUser, currentScope, ref propertyNotFound);
            }
            else
            {
                propertyNotFound = true;
                return PropertyAccess.ContentLocked;
            }
        }
    }
}

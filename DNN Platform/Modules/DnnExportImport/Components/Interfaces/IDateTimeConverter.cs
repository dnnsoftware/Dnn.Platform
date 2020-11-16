// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.ExportImport.Components.Interfaces
{
    using DotNetNuke.Entities.Users;

    /// <summary>
    /// Contract to convert all the datetiem properties in the class to User's local time.
    /// </summary>
    internal interface IDateTimeConverter
    {
        /// <summary>
        /// Converts the datetime properties to user locale.
        /// </summary>
        /// <param name="userInfo"></param>
        void ConvertToLocal(UserInfo userInfo);
    }
}

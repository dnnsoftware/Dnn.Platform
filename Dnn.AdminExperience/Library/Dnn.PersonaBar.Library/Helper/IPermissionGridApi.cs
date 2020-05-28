// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

using System.Net.Http;

namespace Dnn.PersonaBar.Library.Helper
{
    public interface IPermissionGridApi
    {
        // TODO: ? HttpResponseMessage GetPermissionsData();

        /// <summary>
        /// Returns all roles/role groups info
        /// </summary>
        /// <returns></returns>
        HttpResponseMessage GetRoles();
    }
}

// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
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

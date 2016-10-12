#region Copyright
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2016
// by DotNetNuke Corporation
// All Rights Reserved
#endregion

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

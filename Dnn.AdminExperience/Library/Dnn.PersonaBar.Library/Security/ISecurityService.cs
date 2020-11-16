// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Library.Security
{
    /// <summary>
    /// Global level security service to do permission check.
    /// </summary>
    public interface ISecurityService
    {
        bool IsPagesAdminUser();
    }
}

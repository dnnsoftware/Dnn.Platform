// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
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

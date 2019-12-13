// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
namespace Dnn.PersonaBar.Library
{
    /// <summary>
    /// 
    /// </summary>
    public enum ServiceScope
    {
        /// <summary>
        /// the service available for all users.
        /// </summary>
        Regular,
        /// <summary>
        /// the service only available for admin users.
        /// </summary>
        Admin,
        /// <summary>
        /// the service only available for host users.
        /// </summary>
        Host
    }
}

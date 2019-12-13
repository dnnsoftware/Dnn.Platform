// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using System.Collections.Generic;

namespace DotNetNuke.Services.Registration
{
    public interface IRegistrationProfileController
    {
        IEnumerable<string> Search(int portalId, string searchValue);
    }
}

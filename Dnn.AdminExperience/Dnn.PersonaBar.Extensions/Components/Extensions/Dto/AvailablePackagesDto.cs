
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

using System.Collections.Generic;

namespace Dnn.PersonaBar.Extensions.Components.Dto
{
    public class AvailablePackagesDto
    {
        public string PackageType { get; set; }

        public List<PackageInfoSlimDto> ValidPackages { get; set; }

        public List<string> InvalidPackages { get; set; }
    }
}

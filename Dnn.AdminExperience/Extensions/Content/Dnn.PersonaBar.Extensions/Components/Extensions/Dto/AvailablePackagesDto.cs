#region Usings

using System.Collections.Generic;

#endregion

namespace Dnn.PersonaBar.Extensions.Components.Dto
{
    public class AvailablePackagesDto
    {
        public string PackageType { get; set; }

        public List<PackageInfoSlimDto> ValidPackages { get; set; }

        public List<string> InvalidPackages { get; set; }
    }
}

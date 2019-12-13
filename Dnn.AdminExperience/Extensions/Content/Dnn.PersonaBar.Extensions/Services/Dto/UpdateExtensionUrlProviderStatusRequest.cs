#region Usings

using System;
using System.Data;
using System.Runtime.Serialization;
using DotNetNuke.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

#endregion

namespace Dnn.PersonaBar.Seo.Services.Dto
{
    public class UpdateExtensionUrlProviderStatusRequest
    {
        public int ProviderId { get; set; }

        public bool IsActive { get; set; }
    }
}

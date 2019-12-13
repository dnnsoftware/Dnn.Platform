#region Usings

using System;
using System.Data;
using System.Runtime.Serialization;
using DotNetNuke.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

#endregion

namespace Dnn.PersonaBar.Seo.Services.Dto
{
    public class UpdateSitemapProviderRequest
    {
        public string Name { get; set; }

        public bool Enabled { get; set; }

        public float Priority { get; set; }

        public bool OverridePriority { get; set; }
    }
}

#region Usings

using System;
using System.Data;
using System.Runtime.Serialization;
using DotNetNuke.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

#endregion

namespace Dnn.PersonaBar.CssEditor.Services.Dto
{
    public class UpdateCssRequest
    {
        public int PortalId { get; set; }

        public string StyleSheetContent { get; set; }
    }
}

#region Usings

using System;
using System.Collections.Generic;
using System.Data;
using System.Runtime.Serialization;
using DotNetNuke.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

#endregion

namespace Dnn.PersonaBar.Security.Services.Dto
{
    public class TabDto
    {
        public string Name { get; set; }

        public string TabId { get; set; }

        public int ParentTabId { get; set; }

        public IList<TabDto> ChildTabs { get; set; }
    }
}

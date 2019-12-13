using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DotNetNuke.Web.Components.Controllers.Models
{
    public class MenuItemViewModel
    {
        public string ID { get; set; }
        public string Text { get; set; }
        public string Source { get; set; }
        public int Order { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNetNuke.Entities.Modules.Settings
{
    public interface IParameterGrouping
    {
        string Category { get; set; }

        string Prefix { get; set; }
    }
}

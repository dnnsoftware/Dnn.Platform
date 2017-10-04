using System;
using System.Collections.Generic;
using System.Text;

namespace ClientDependency.Core
{
    public interface IClientDependencyFile
    {
        string FilePath { get; set; }
        ClientDependencyType DependencyType { get; }
        int Priority { get; set; }
        int Group { get; set; }
        string PathNameAlias { get; set; }
        string ForceProvider { get; set; }
        bool ForceBundle { get; set; }
        //*** DNN related change *** begin
        string Name { get; set; }
        string Version { get; set; }
        bool ForceVersion { get; set; }
        //*** DNN related change *** end

    }
}

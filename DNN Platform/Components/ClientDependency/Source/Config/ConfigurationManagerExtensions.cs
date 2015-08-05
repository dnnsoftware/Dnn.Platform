using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.Web.Configuration;

namespace ClientDependency.Core.Config
{
    public static class ConfigurationHelper
    {

        public static bool IsCompilationDebug
        {
            get
            {
                CompilationSection compilation = ConfigurationManager.GetSection("system.web/compilation") as CompilationSection;
                if (compilation != null)
                {
                    return compilation.Debug;
                }
                return false; //by default, return false!
            }
        }

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;

namespace ClientDependency.Core.Config
{
    
    public class RogueFileCompressionExcludeElement : ConfigurationElement
    {
        [ConfigurationProperty("path", IsRequired = true)]
        public string FilePath
        {
            get
            {
                return (string)this["path"];
            }
        }

        public override int GetHashCode()
        {
            return this.FilePath.GetHashCode();
        }

    }
}

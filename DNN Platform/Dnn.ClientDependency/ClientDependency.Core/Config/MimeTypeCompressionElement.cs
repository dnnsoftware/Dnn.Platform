using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;

namespace ClientDependency.Core.Config
{
    public class MimeTypeCompressionElement : ConfigurationElement
    {
        [ConfigurationProperty("type", IsRequired = true)]
        public string MimeType
        {
            get
            {
                return (string)this["type"];
            }            
        }

        [ConfigurationProperty("path", DefaultValue = "*")]
        public string FilePath
        {
            get
            {
                return (string)this["path"];
            }            
        }

        public override int GetHashCode()
        {
            return (this.MimeType + this.FilePath).GetHashCode();
        }

        public override bool Equals(object compareTo)
        {
            var e = compareTo as MimeTypeCompressionElement;
            if (e != null)
            {
                return (e.GetHashCode().Equals(this.GetHashCode()));
            }
            return false;
        }

    }
}

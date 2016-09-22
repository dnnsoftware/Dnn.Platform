using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Dnn.EditBar.Library.Items
{
    [DataContract]
    public abstract class BaseMenuItem
    {
        [DataMember(Name = "name")]
        public abstract string Name { get; }

        [DataMember(Name = "button")]
        public abstract string Button { get; }

        [DataMember(Name = "cssClass")]
        public abstract string CssClass { get; }

        [DataMember(Name = "parent")]
        public abstract string Parent { get; }

        [DataMember(Name = "order")]
        public virtual int Order { get; } = 0;

        [DataMember(Name = "loader")]
        public abstract string Loader { get; }

        [DataMember(Name = "settings")]
        public virtual IDictionary<string, object> Settings { get; } = new Dictionary<string, object>();


        public virtual bool Visible()
        {
            return true;
        }
    }
}

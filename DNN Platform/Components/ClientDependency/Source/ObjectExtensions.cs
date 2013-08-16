using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace ClientDependency.Core
{
    public static class ObjectExtensions
    {
        public static IDictionary<string, object> ToDictionary(this object o)
        {
            if (o != null)
            {
                var props = TypeDescriptor.GetProperties(o);
                var d = new Dictionary<string, object>();
                foreach (var prop in props.Cast<PropertyDescriptor>())
                {
                    var val = prop.GetValue(o);
                    if (val != null)
                    {
                        d.Add(prop.Name, val);
                    }
                }
                return d;
            }
            return new Dictionary<string, object>();
        }
    }
}
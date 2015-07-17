using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace ClientDependency.Core
{
    public static class ObjectExtensions
    {
        public static IDictionary<string, object> ToDictionary(this object o)
        {
            var asObjectDictionary = o as IDictionary<string, object>;
            if (asObjectDictionary != null)
                return asObjectDictionary;
            var asStringDictionary = o as IDictionary<string, string>;
            if (asStringDictionary != null)
                return asStringDictionary.ToDictionary(x => x.Key, x => (object) x.Value);

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
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Syndication
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;

    /// <summary>
    ///   Helper class to enable the data binding logic generate column names at runtime.
    /// </summary>
    internal class RssElementCustomTypeDescriptor : ICustomTypeDescriptor
    {
        private readonly Dictionary<string, string> _attributes;

        public RssElementCustomTypeDescriptor(Dictionary<string, string> attributes)
        {
            this._attributes = attributes;
        }

        AttributeCollection ICustomTypeDescriptor.GetAttributes()
        {
            return AttributeCollection.Empty;
        }

        string ICustomTypeDescriptor.GetClassName()
        {
            return this.GetType().Name;
        }

        string ICustomTypeDescriptor.GetComponentName()
        {
            return null;
        }

        TypeConverter ICustomTypeDescriptor.GetConverter()
        {
            return null;
        }

        EventDescriptor ICustomTypeDescriptor.GetDefaultEvent()
        {
            return null;
        }

        PropertyDescriptor ICustomTypeDescriptor.GetDefaultProperty()
        {
            return null;
        }

        object ICustomTypeDescriptor.GetEditor(Type editorBaseType)
        {
            return null;
        }

        EventDescriptorCollection ICustomTypeDescriptor.GetEvents(Attribute[] attributes)
        {
            return null;
        }

        EventDescriptorCollection ICustomTypeDescriptor.GetEvents()
        {
            return null;
        }

        PropertyDescriptorCollection ICustomTypeDescriptor.GetProperties(Attribute[] attributes)
        {
            return this.GetPropertyDescriptors();
        }

        PropertyDescriptorCollection ICustomTypeDescriptor.GetProperties()
        {
            return this.GetPropertyDescriptors();
        }

        object ICustomTypeDescriptor.GetPropertyOwner(PropertyDescriptor pd)
        {
            return (pd is RssElementCustomPropertyDescriptor) ? this : null;
        }

        private PropertyDescriptorCollection GetPropertyDescriptors()
        {
            var propertyDescriptors = new PropertyDescriptor[this._attributes.Count];
            int i = 0;

            foreach (KeyValuePair<string, string> a in this._attributes)
            {
                propertyDescriptors[i++] = new RssElementCustomPropertyDescriptor(a.Key);
            }

            return new PropertyDescriptorCollection(propertyDescriptors);
        }

        private class RssElementCustomPropertyDescriptor : PropertyDescriptor
        {
            public RssElementCustomPropertyDescriptor(string propertyName)
                : base(propertyName, null)
            {
            }

            public override Type ComponentType
            {
                get
                {
                    return typeof(RssElementCustomTypeDescriptor);
                }
            }

            public override bool IsReadOnly
            {
                get
                {
                    return true;
                }
            }

            public override Type PropertyType
            {
                get
                {
                    return typeof(string);
                }
            }

            public override bool CanResetValue(object o)
            {
                return false;
            }

            public override void ResetValue(object o)
            {
            }

            public override void SetValue(object o, object value)
            {
            }

            public override bool ShouldSerializeValue(object o)
            {
                return true;
            }

            public override object GetValue(object o)
            {
                var element = o as RssElementCustomTypeDescriptor;

                if (element != null)
                {
                    string propertyValue;

                    if (element._attributes.TryGetValue(this.Name, out propertyValue))
                    {
                        return propertyValue;
                    }
                }

                return string.Empty;
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DotNetNuke.Entities.Modules.Settings
{
    [Serializable]
    public class ParameterMapping
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ParameterMapping"/> class.
        /// </summary>
        /// <param name="attribute">The attribute.</param>
        /// <param name="property">The property.</param>
        public ParameterMapping(ParameterAttributeBase attribute, PropertyInfo property)
        {
            this.Attribute = attribute;
            this.Property = property;

            var parameterName = attribute.ParameterName;
            if (string.IsNullOrWhiteSpace(parameterName))
            {
                parameterName = property.Name;
            }

            if (!string.IsNullOrWhiteSpace(attribute.Prefix))
            {
                parameterName = attribute.Prefix + parameterName;
            }

            this.FullParameterName = parameterName;
        }

        #endregion

        #region Properties

        public ParameterAttributeBase Attribute { get; set; }

        public string FullParameterName { get; set; }

        public PropertyInfo Property { get; set; }

        #endregion
    }
}

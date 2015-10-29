using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DotNetNuke.Entities.Modules.Settings
{
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

            var parameterGrouping = attribute as IParameterGrouping;
            if (parameterGrouping != null)
            {
                if (!string.IsNullOrWhiteSpace(parameterGrouping.Prefix))
                {
                    parameterName = parameterGrouping.Prefix + parameterName;
                }
            }

            this.ParameterName = parameterName;
        }

        #endregion

        #region Properties

        public ParameterAttributeBase Attribute { get; set; }

        public string ParameterName { get; set; }

        public PropertyInfo Property { get; set; }

        #endregion
    }
}

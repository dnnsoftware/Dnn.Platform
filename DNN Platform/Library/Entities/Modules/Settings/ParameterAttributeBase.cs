﻿using System;

namespace DotNetNuke.Entities.Modules.Settings
{
    /// <summary>
    /// Base class for attributes that are used to decorate properties (parameters) related to application settings (storage) or parameters (control) like query string parameters.
    /// </summary>
    public abstract class ParameterAttributeBase : Attribute
    {
        public object DefaultValue { get; set; }

        public string ParameterName { get; set; }
    }
}

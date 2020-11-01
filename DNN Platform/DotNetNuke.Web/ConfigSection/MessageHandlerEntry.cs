// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.ConfigSection
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Configuration;

    using DotNetNuke.Web.Api;

    /// <summary>
    /// Represents a Message Handler Entry in web.config.
    /// </summary>
    public class MessageHandlerEntry : ConfigurationElement
    {
        private const string NameTag = "name";
        private const string ClassNameTag = "type";
        private const string EnabledNameTag = "enabled";
        private const string DefaultIncludeTag = "defaultInclude";
        private const string ForceSslTag = "forceSSL";
        private const string AccessControlAllowOriginsTag = "accessControlAllowOrigins";
        private const string AccessControlAllowHeadersTag = "accessControlAllowHeaders";
        private const string AccessControlAllowMethodsTag = "accessControlAllowMethods";

        // [StringValidator(InvalidCharacters = "~!@#$%^&*()[]{}/;'\"|\\", MinLength = 1, MaxLength = 60)]

        /// <summary>
        /// Gets or sets the name of the message handler.
        /// </summary>
        [ConfigurationProperty(NameTag, DefaultValue = "", IsRequired = true)]
        public string Name
        {
            get
            {
                return (string)this[NameTag];
            }

            set
            {
                this[NameTag] = value;
            }
        }

        // [StringValidator(InvalidCharacters = "~!@#$%^&*()[]{}/;'\"|\\", MinLength = 1, MaxLength = 300)]

        /// <summary>
        /// Gets or sets the class name of the message handler.
        /// </summary>
        [ConfigurationProperty(ClassNameTag, DefaultValue = "", IsRequired = true)]
        public string ClassName
        {
            get
            {
                return (string)this[ClassNameTag];
            }

            set
            {
                this[ClassNameTag] = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this message handler is enabled.
        /// </summary>
        [ConfigurationProperty(EnabledNameTag, DefaultValue = false, IsRequired = true)]
        public bool Enabled
        {
            get
            {
                var b = (bool?)this[EnabledNameTag];
                return b.Value;
            }

            set
            {
                this[EnabledNameTag] = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this property specifies whether this is automatically
        /// included when the <see cref="DnnAuthorizeAttribute"/> is used.
        /// </summary>
        [ConfigurationProperty(DefaultIncludeTag, DefaultValue = false, IsRequired = true)]
        public bool DefaultInclude
        {
            get
            {
                var b = (bool?)this[DefaultIncludeTag];
                return b.Value;
            }

            set
            {
                this[DefaultIncludeTag] = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether to enforce SSL for this message handler.
        /// </summary>
        [ConfigurationProperty(ForceSslTag, DefaultValue = true, IsRequired = true)]
        public bool ForceSsl
        {
            get
            {
                var b = (bool?)this[ForceSslTag];
                return b.Value;
            }

            set
            {
                this[ForceSslTag] = value;
            }
        }

        /// <summary>
        /// Gets or sets a list of origins allowed to be used by this message handler for CORS support.
        /// </summary>
        [ConfigurationProperty(AccessControlAllowOriginsTag, DefaultValue = "", IsRequired = false)]
        public string AccessControlAllowOrigins
        {
            get
            {
                return (string)this[AccessControlAllowOriginsTag];
            }

            set
            {
                this[AccessControlAllowOriginsTag] = value;
            }
        }

        /// <summary>
        /// Gets or sets the list of headers allowed to be used by this message handler for CORS support.
        /// </summary>
        [ConfigurationProperty(AccessControlAllowHeadersTag, DefaultValue = "", IsRequired = false)]
        public string AccessControlAllowHeaders
        {
            get
            {
                return (string)this[AccessControlAllowHeadersTag];
            }

            set
            {
                this[AccessControlAllowHeadersTag] = value;
            }
        }

        /// <summary>
        /// Gets or sets the list of methods allowed to be used by this message handler for CORS support.
        /// </summary>
        [ConfigurationProperty(AccessControlAllowMethodsTag, DefaultValue = "", IsRequired = false)]
        public string AccessControlAllowMethods
        {
            get
            {
                return (string)this[AccessControlAllowMethodsTag];
            }

            set
            {
                this[AccessControlAllowMethodsTag] = value;
            }
        }
    }
}

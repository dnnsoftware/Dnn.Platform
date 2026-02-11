// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.ConfigSection
{
    using System.Configuration;

    using DotNetNuke.Web.Api;

    /// <summary>A <see cref="ConfigurationElement"/> for an auth message handler.</summary>
    public class MessageHandlerEntry : ConfigurationElement
    {
        private const string NameTag = "name";
        private const string ClassNameTag = "type";
        private const string EnabledNameTag = "enabled";
        private const string DefaultIncludeTag = "defaultInclude";
        private const string ForceSslTag = "forceSSL";

        /// <summary>Gets or sets the name of the message handler.</summary>
        [ConfigurationProperty(NameTag, DefaultValue = "", IsRequired = true)]
        ////[StringValidator(InvalidCharacters = "~!@#$%^&*()[]{}/;'\"|\\", MinLength = 1, MaxLength = 60)]
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

        /// <summary>Gets or sets the fully qualified name of the class that implements this message handler.</summary>
        [ConfigurationProperty(ClassNameTag, DefaultValue = "", IsRequired = true)]
        ////[StringValidator(InvalidCharacters = "~!@#$%^&*()[]{}/;'\"|\\", MinLength = 1, MaxLength = 300)]
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

        /// <summary>Gets or sets a value indicating whether this message handler is enabled.</summary>
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

        /// <summary>Gets or sets a value indicating whether this is automatically included when the <see cref="DnnAuthorizeAttribute"/>.</summary>
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

        /// <summary>Gets or sets a value indicating whether this message handler always uses HTTPS.</summary>
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
    }
}

using System.Configuration;
using DotNetNuke.Web.Api;

namespace DotNetNuke.Web.ConfigSection
{
    public enum SslModes { Default, No, Yes }

    public class MessageHandlerEntry : ConfigurationElement
    {
        private const string NameTag = "name";
        private const string ClassNameTag = "type";
        private const string EnabledNameTag = "enabled";
        private const string DefaultIncludeTag = "defaultInclude";
        private const string SslModeTag = "sslMode";
        private const string BypassAFT = "bypassAntiForgeryToken";

        [ConfigurationProperty(NameTag, DefaultValue = "", IsRequired = true)]
        //[StringValidator(InvalidCharacters = "~!@#$%^&*()[]{}/;'\"|\\", MinLength = 1, MaxLength = 60)]
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

        [ConfigurationProperty(ClassNameTag, DefaultValue = "", IsRequired = true)]
        //[StringValidator(InvalidCharacters = "~!@#$%^&*()[]{}/;'\"|\\", MinLength = 1, MaxLength = 300)]
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
        /// This property specifies whether this is automatically included when the <see cref="DnnAuthorizeAttribute"/>
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

        [ConfigurationProperty(SslModeTag, DefaultValue = SslModes.Default, IsRequired = true)]
        //[RegexStringValidator("/^Yes$|^No$|^Default$/i")]
        public SslModes SslMode
        {
            get
            {
                return (SslModes)this[SslModeTag];
            }
            set
            {
                this[SslModeTag] = value;
            }
        }

        /// <summary>
        /// This property specifies whether to bypass the <see cref="ValidateAntiForgeryTokenAttribute"/> setting on the API.
        /// </summary>
        [ConfigurationProperty(BypassAFT, DefaultValue = false, IsRequired = false)]
        public bool BypassAntiForgeryToken
        {
            get
            {
                var b = (bool?)this[BypassAFT];
                return b.Value;
            }
            set
            {
                this[BypassAFT] = value;
            }
        }

    }
}

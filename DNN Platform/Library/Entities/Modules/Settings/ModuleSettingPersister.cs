using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Web.Caching;
using DotNetNuke.Common;
using DotNetNuke.Collections;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Services.Cache;
using DotNetNuke.Services.Exceptions;

namespace DotNetNuke.Entities.Modules.Settings
{
    public class ModuleSettingPersister<TType>
        where TType : new()
    {
        public const string CachePrefix = "ModuleSettingsPersister_";

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ModuleSettingPersister{TType}"/> class.
        /// </summary>
        public ModuleSettingPersister()
        {
            this.Mapping = this.LoadMapping();
        }

        #endregion

        #region Properties

        private IList<ParameterMapping> Mapping { get; set; }

        #endregion

        public TType Load(ModuleInfo ctlModule)
        {
            var settings = new TType();

            this.Mapping.ForEach(mapping =>
            {
                object settingValue = null;

                var attribute = mapping.Attribute;
                var property = mapping.Property;
                if (attribute is PortalSettingAttribute)
                {
                    settingValue = PortalController.GetPortalSetting(mapping.ParameterName, ctlModule.PortalID, null);
                    if (string.IsNullOrWhiteSpace((string)settingValue) && (attribute.DefaultValue != null))
                    {
                        settingValue = attribute.DefaultValue;
                    }
                }
                else if (attribute is TabModuleSettingAttribute && ctlModule.TabModuleSettings.ContainsKey(mapping.ParameterName))
                {
                    settingValue = ctlModule.TabModuleSettings[mapping.ParameterName];
                }
                else if (attribute is ModuleSettingAttribute && ctlModule.ModuleSettings.ContainsKey(mapping.ParameterName))
                {
                    settingValue = ctlModule.ModuleSettings[mapping.ParameterName];
                }
                else if (attribute.DefaultValue != null)
                {
                    settingValue = attribute.DefaultValue;
                }

                if (settingValue != null && property.CanWrite)
                {
                    this.WriteProperty(settings, property, settingValue);
                }
            });

            return settings;
        }

        public void Save(TType settings, ModuleInfo ctlModule)
        {
            Requires.NotNull("settings", settings);
            Requires.NotNull("ctlModule", ctlModule);

            var controller = new ModuleController();
            this.Mapping.ForEach(mapping =>
            {
                var attribute = mapping.Attribute;
                var property = mapping.Property;

                if (property.CanRead) // Should be, because we asked for properties with a Get accessor.
                {
                    var settingValue = property.GetValue(settings, null);
                    if (settingValue != null)
                    {
                        if (attribute is ModuleSettingAttribute)
                        {
                            controller.UpdateModuleSetting(ctlModule.ModuleID, mapping.ParameterName, settingValue.ToString());
                        }
                        else if (attribute is TabModuleSettingAttribute)
                        {
                            controller.UpdateTabModuleSetting(ctlModule.TabModuleID, mapping.ParameterName, settingValue.ToString());
                        }
                        else if (attribute is PortalSettingAttribute)
                        {
                            PortalController.UpdatePortalSetting(ctlModule.PortalID, mapping.ParameterName, settingValue.ToString());
                        }
                    }
                }
            });
        }

        #region Helpers

        protected IList<ParameterMapping> LoadMapping()
        {
            var cacheKey = this.CacheKey;
            var mapping = CachingProvider.Instance().GetItem(cacheKey) as IList<ParameterMapping>;
            if (mapping == null)
            {
                mapping = this.CreateMapping();
                // HARDCODED: 2 hour expiration. 
                // Note that "caching" can also be accomplished with a static dictionary since the Attribute/Property mapping does not change unless the module is updated.
                CachingProvider.Instance().Insert(cacheKey, mapping, (DNNCacheDependency)null, DateTime.Now.AddHours(2), Cache.NoSlidingExpiration);
            }

            return mapping;
        }

        protected virtual string CacheKey
        {
            get
            {
                var type = typeof(TType);
                return ModuleSettingPersister<TType>.CachePrefix + type.FullName.Replace(".", "_");
            }
        }

        protected virtual IList<ParameterMapping> CreateMapping()
        {
            var mapping = new List<ParameterMapping>();
            var type = typeof(TType);
            var properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetProperty | BindingFlags.SetProperty);

            properties.ForEach(property =>
            {
                // In .NET Framework 4.5.x the call below can be replaced by property.GetCustomAttributes<BaseParameterAttribute>(true);
                var attributes = property.GetCustomAttributes(typeof(ParameterAttributeBase), true).OfType<ParameterAttributeBase>();
                attributes.ForEach(attribute => mapping.Add(new ParameterMapping(attribute, property)));
            });

            return mapping;
        }

        /// <summary>
        /// Writes the property.
        /// </summary>
        /// <param name="settings">The settings.</param>
        /// <param name="property">The property.</param>
        /// <param name="propertyValue">The property value.</param>
        /// <exception cref="System.InvalidCastException"></exception>
        private void WriteProperty(TType settings, PropertyInfo property, object propertyValue)
        {
            try
            {
                var valueType = propertyValue.GetType();
                var propertyType = property.PropertyType;
                if (propertyType.GetGenericArguments().Any() && propertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
                {
                    // Nullable type
                    propertyType = propertyType.GetGenericArguments()[0];
                }

                if (propertyType == valueType)
                {
                    // The property and settingsValue have the same type - no conversion needed - just update!
                    property.SetValue(settings, propertyValue, null);
                }
                else if (propertyType.BaseType == typeof(Enum))
                {
                    // The property is an enum. Determine if the enum value is persisted as string or numeric.
                    if (Regex.IsMatch(propertyValue.ToString(), "^\\d+$"))
                    {
                        // The enum value is a number
                        property.SetValue(settings, Enum.ToObject(propertyType, Convert.ToInt32(propertyValue, CultureInfo.InvariantCulture)), null);
                    }
                    else
                    {
                        try
                        {
                            property.SetValue(settings, Enum.Parse(propertyType, propertyValue.ToString(), true), null);
                        }
                        catch (ArgumentException exception)
                        {
                            // Just log the exception. Use the default.
                            Exceptions.LogException(exception);
                        }
                    }
                }
                else if (!(propertyValue is IConvertible))
                {
                    // The property value does not support IConvertible interface - assign the value direct.
                    property.SetValue(settings, propertyValue, null);
                }
                else
                {
                    property.SetValue(settings, Convert.ChangeType(propertyValue, propertyType, CultureInfo.InvariantCulture), null);
                }
            }
            catch (Exception exception)
            {
                // TODO: Localize exception
                throw new InvalidCastException(string.Format(CultureInfo.CurrentUICulture, "Could not cast {0} to property {1} of type {2}",
                                                             propertyValue,
                                                             property.Name,
                                                             property.PropertyType), exception);
            }
        }

        #endregion
    }
}

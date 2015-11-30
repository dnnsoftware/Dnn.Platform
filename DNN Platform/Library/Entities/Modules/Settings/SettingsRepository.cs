using System;
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
using DotNetNuke.Common.Utilities;

namespace DotNetNuke.Entities.Modules.Settings
{
    public abstract class SettingsRepository<T> : ISettingsRepository<T> where T : class, new()
    {
        #region Properties

        private IList<ParameterMapping> Mapping { get; set; }

        #endregion

        public SettingsRepository()
        {
            this.Mapping = this.LoadMapping();
        }

        public T GetSettings(ModuleInfo moduleContext)
        {
            return CBO.GetCachedObject<T>(new CacheItemArgs(this.CacheKey(moduleContext.TabModuleID), 20, CacheItemPriority.AboveNormal, moduleContext),
                                                       Load,
                                                       true);
        }

        #region Serialization
        public void SaveSettings(ModuleInfo moduleContext, T settings)
        {
            Requires.NotNull("settings", settings);
            Requires.NotNull("ctlModule", moduleContext);

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
                        var settingValueAsString = "";
                        if (!string.IsNullOrEmpty(attribute.Serializer))
                        {
                            ISettingsSerializer<T> serializer = (ISettingsSerializer<T>)Framework.Reflection.CreateType(attribute.Serializer, true);
                            if (serializer != null)
                            {
                                settingValueAsString = serializer.Serialize((T)settingValue);
                            }
                        }

                        if(string.IsNullOrEmpty(settingValueAsString))
                        {
                            if (settingValue is DateTime)
                            {
                                settingValueAsString = ((DateTime)settingValue).ToString("u");
                            }
                            else
                            {
                                settingValueAsString = settingValue.ToString();
                            }
                        }

                        if (attribute is ModuleSettingAttribute)
                        {
                            controller.UpdateModuleSetting(moduleContext.ModuleID, mapping.FullParameterName, settingValueAsString);
                        }
                        else if (attribute is TabModuleSettingAttribute)
                        {
                            controller.UpdateTabModuleSetting(moduleContext.TabModuleID, mapping.FullParameterName, settingValueAsString);
                        }
                        else if (attribute is PortalSettingAttribute)
                        {
                            PortalController.UpdatePortalSetting(moduleContext.PortalID, mapping.FullParameterName, settingValueAsString);
                        }
                    }
                }
            });
            DataCache.SetCache(this.CacheKey(moduleContext.TabModuleID), settings);
        }
        #endregion

        #region Mappings
        protected IList<ParameterMapping> LoadMapping()
        {
            var cacheKey = this.MappingCacheKey;
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

        public const string CachePrefix = "ModuleSettingsPersister_";
        protected virtual string MappingCacheKey
        {
            get
            {
                var type = typeof(T);
                return SettingsRepository<T>.CachePrefix + type.FullName.Replace(".", "_");
            }
        }

        protected virtual IList<ParameterMapping> CreateMapping()
        {
            var mapping = new List<ParameterMapping>();
            var type = typeof(T);
            var properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetProperty | BindingFlags.SetProperty);

            properties.ForEach(property =>
            {
                var attributes = property.GetCustomAttributes<ParameterAttributeBase>(true);
                attributes.ForEach(attribute => mapping.Add(new ParameterMapping(attribute, property)));
            });

            return mapping;
        }
        #endregion

        #region Deserialization
        private T Load(CacheItemArgs args)
        {
            var ctlModule = (ModuleInfo)args.ParamList[0];
            var settings = new T();

            this.Mapping.ForEach(mapping =>
            {
                object settingValue = null;

                var attribute = mapping.Attribute;
                var property = mapping.Property;
                if (attribute is PortalSettingAttribute)
                {
                    settingValue = PortalController.GetPortalSetting(mapping.FullParameterName, ctlModule.PortalID, null);
                    if (string.IsNullOrWhiteSpace((string)settingValue))
                    {
                        settingValue = null;
                    }
                }
                else if (attribute is TabModuleSettingAttribute && ctlModule.TabModuleSettings.ContainsKey(mapping.FullParameterName))
                {
                    settingValue = ctlModule.TabModuleSettings[mapping.FullParameterName];
                }
                else if (attribute is ModuleSettingAttribute && ctlModule.ModuleSettings.ContainsKey(mapping.FullParameterName))
                {
                    settingValue = ctlModule.ModuleSettings[mapping.FullParameterName];
                }
                if (settingValue != null && property.CanWrite)
                {
                    this.DeserializeProperty(settings, property, attribute, settingValue);
                }
            });

            return settings;
        }

        private string CacheKey(int moduleId)
        {
            return string.Format("SettingsModule{0}", moduleId);
        }

        /// <summary>
        /// Deserializes the property.
        /// </summary>
        /// <param name="settings">The settings.</param>
        /// <param name="property">The property.</param>
        /// <param name="propertyValue">The property value.</param>
        /// <exception cref="System.InvalidCastException"></exception>
        private void DeserializeProperty(T settings, PropertyInfo property, ParameterAttributeBase attribute, object propertyValue)
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
                else if (!string.IsNullOrEmpty(attribute.Serializer))
                {
                    ISettingsSerializer<T> serializer = (ISettingsSerializer<T>)Framework.Reflection.CreateType(attribute.Serializer, true);
                    if (serializer != null)
                    {
                        property.SetValue(settings, serializer.Deserialize((string)propertyValue), null);
                    }
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

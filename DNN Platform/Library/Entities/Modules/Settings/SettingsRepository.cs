// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Entities.Modules.Settings
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Globalization;
    using System.Linq;
    using System.Reflection;
    using System.Text.RegularExpressions;
    using System.Web.Caching;

    using DotNetNuke.Collections;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Services.Cache;
    using DotNetNuke.Services.Exceptions;

    public abstract class SettingsRepository<T> : ISettingsRepository<T>
        where T : class, new()
    {
        public const string CachePrefix = "ModuleSettingsPersister_";

        private readonly IModuleController _moduleController;

        protected SettingsRepository()
        {
            this.Mapping = this.LoadMapping();
            this._moduleController = ModuleController.Instance;
        }

        protected virtual string MappingCacheKey
        {
            get
            {
                var type = typeof(T);
                return SettingsRepository<T>.CachePrefix + type.FullName.Replace(".", "_");
            }
        }

        private IList<ParameterMapping> Mapping { get; }

        public T GetSettings(ModuleInfo moduleContext)
        {
            return CBO.GetCachedObject<T>(new CacheItemArgs(this.CacheKey(moduleContext.TabModuleID), 20, CacheItemPriority.AboveNormal, moduleContext), this.Load, false);
        }

        public void SaveSettings(ModuleInfo moduleContext, T settings)
        {
            Requires.NotNull("settings", settings);
            Requires.NotNull("ctlModule", moduleContext);

            this.Mapping.ForEach(mapping =>
            {
                var attribute = mapping.Attribute;
                var property = mapping.Property;

                if (property.CanRead) // Should be, because we asked for properties with a Get accessor.
                {
                    var settingValue = property.GetValue(settings, null) ?? string.Empty;
                    string settingValueAsString = null;
                    if (!string.IsNullOrEmpty(attribute.Serializer))
                    {
                        settingValueAsString = (string)CallSerializerMethod(attribute.Serializer, property.PropertyType, settingValue, nameof(ISettingsSerializer<T>.Serialize));
                    }

                    if (settingValueAsString == null)
                    {
                        settingValueAsString = GetSettingValueAsString(settingValue);
                    }

                    if (attribute is ModuleSettingAttribute)
                    {
                        this._moduleController.UpdateModuleSetting(moduleContext.ModuleID, mapping.FullParameterName, settingValueAsString);
                        moduleContext.ModuleSettings[mapping.FullParameterName] = settingValueAsString; // temporary fix for issue 3692
                    }
                    else if (attribute is TabModuleSettingAttribute)
                    {
                        this._moduleController.UpdateTabModuleSetting(moduleContext.TabModuleID, mapping.FullParameterName, settingValueAsString);
                        moduleContext.TabModuleSettings[mapping.FullParameterName] = settingValueAsString; // temporary fix for issue 3692
                    }
                    else if (attribute is PortalSettingAttribute)
                    {
                        PortalController.UpdatePortalSetting(moduleContext.PortalID, mapping.FullParameterName, settingValueAsString);
                    }
                }
            });
            DataCache.SetCache(this.CacheKey(moduleContext.TabModuleID), settings);
        }

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

        private static string GetSettingValueAsString(object settingValue)
        {
            var dateTimeValue = settingValue as DateTime?;
            if (dateTimeValue != null)
            {
                return dateTimeValue.Value.ToString("o", CultureInfo.InvariantCulture);
            }

            var timeSpanValue = settingValue as TimeSpan?;
            if (timeSpanValue != null)
            {
                return timeSpanValue.Value.ToString("c", CultureInfo.InvariantCulture);
            }

            return Convert.ToString(settingValue, CultureInfo.InvariantCulture);
        }

        private static object CallSerializerMethod(string serializerTypeName, Type typeArgument, object value, string methodName)
        {
            var serializerType = Framework.Reflection.CreateType(serializerTypeName, true);
            if (serializerType == null)
            {
                return null;
            }

            var serializer = Framework.Reflection.CreateInstance(serializerType);
            if (serializer == null)
            {
                return null;
            }

            var serializerInterfaceType = typeof(ISettingsSerializer<>).MakeGenericType(typeArgument);
            var method = serializerInterfaceType.GetMethod(methodName);
            return method.Invoke(serializer, new[] { value, });
        }

        private T Load(CacheItemArgs args)
        {
            var ctlModule = (ModuleInfo)args.ParamList[0];
            var settings = new T();

            this.Mapping.ForEach(mapping =>
            {
                string settingValue = null;

                var attribute = mapping.Attribute;
                var property = mapping.Property;

                // TODO: Make more extensible, enable other attributes to be defined
                if (attribute is PortalSettingAttribute && PortalController.Instance.GetPortalSettings(ctlModule.PortalID).ContainsKey(mapping.FullParameterName))
                {
                    settingValue = PortalController.Instance.GetPortalSettings(ctlModule.PortalID)[mapping.FullParameterName];
                }
                else if (attribute is TabModuleSettingAttribute && ctlModule.TabModuleSettings.ContainsKey(mapping.FullParameterName))
                {
                    settingValue = (string)ctlModule.TabModuleSettings[mapping.FullParameterName];
                }
                else if (attribute is ModuleSettingAttribute && ctlModule.ModuleSettings.ContainsKey(mapping.FullParameterName))
                {
                    settingValue = (string)ctlModule.ModuleSettings[mapping.FullParameterName];
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
        private void DeserializeProperty(T settings, PropertyInfo property, ParameterAttributeBase attribute, string propertyValue)
        {
            try
            {
                var valueType = propertyValue.GetType();
                var propertyType = property.PropertyType;
                if (propertyType.GetGenericArguments().Any() && propertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
                {
                    // Nullable type
                    propertyType = propertyType.GetGenericArguments()[0];
                    if (string.IsNullOrEmpty(propertyValue))
                    {
                        property.SetValue(settings, null, null);
                        return;
                    }
                }

                if (propertyType == valueType)
                {
                    // The property and settingsValue have the same type - no conversion needed - just update!
                    property.SetValue(settings, propertyValue, null);
                    return;
                }

                if (!string.IsNullOrEmpty(attribute.Serializer))
                {
                    var deserializedValue = CallSerializerMethod(attribute.Serializer, property.PropertyType, propertyValue, nameof(ISettingsSerializer<T>.Deserialize));
                    property.SetValue(settings, deserializedValue, null);
                    return;
                }

                if (propertyType.BaseType == typeof(Enum))
                {
                    // The property is an enum. Determine if the enum value is persisted as string or numeric.
                    if (Regex.IsMatch(propertyValue, "^\\d+$"))
                    {
                        // The enum value is a number
                        property.SetValue(settings, Enum.ToObject(propertyType, Convert.ToInt32(propertyValue, CultureInfo.InvariantCulture)), null);
                    }
                    else
                    {
                        try
                        {
                            property.SetValue(settings, Enum.Parse(propertyType, propertyValue, true), null);
                        }
                        catch (ArgumentException exception)
                        {
                            // Just log the exception. Use the default.
                            Exceptions.LogException(exception);
                        }
                    }

                    return;
                }

                TimeSpan timeSpanValue;
                if (propertyType.IsAssignableFrom(typeof(TimeSpan)) && TimeSpan.TryParse(propertyValue, CultureInfo.InvariantCulture, out timeSpanValue))
                {
                    property.SetValue(settings, timeSpanValue);
                    return;
                }

                DateTime dateTimeValue;
                if (propertyType.IsAssignableFrom(typeof(DateTime)) && DateTime.TryParse(propertyValue, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out dateTimeValue))
                {
                    property.SetValue(settings, dateTimeValue);
                    return;
                }

                if (propertyType.GetInterface(typeof(IConvertible).FullName) != null)
                {
                    propertyValue = this.ChangeFormatForBooleansIfNeeded(propertyType, propertyValue);
                    property.SetValue(settings, Convert.ChangeType(propertyValue, propertyType, CultureInfo.InvariantCulture), null);
                    return;
                }

                var converter = TypeDescriptor.GetConverter(propertyType);
                if (converter.IsValid(propertyValue))
                {
                    converter.ConvertFromInvariantString(propertyValue);
                }
            }
            catch (Exception exception)
            {
                // TODO: Localize exception
                throw new InvalidCastException(
                    string.Format(CultureInfo.CurrentUICulture, "Could not cast {0} to property {1} of type {2}",
                                                             propertyValue,
                                                             property.Name,
                                                             property.PropertyType), exception);
            }
        }

        private string ChangeFormatForBooleansIfNeeded(Type propertyType, string propertyValue)
        {
            if (!propertyType.Name.Equals("Boolean"))
            {
                return propertyValue;
            }

            bool boolValue;
            if (bool.TryParse(propertyValue, out boolValue))
            {
                return propertyValue;
            }

            if (propertyValue.Equals("1"))
            {
                return bool.TrueString;
            }

            if (propertyValue.Equals("0"))
            {
                return bool.FalseString;
            }

            return propertyValue;
        }
    }
}

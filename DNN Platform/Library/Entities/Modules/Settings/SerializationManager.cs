// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Entities.Modules.Settings
{
    using System;
    using System.ComponentModel;
    using System.Globalization;
    using System.Linq;
    using System.Reflection;
    using System.Text.RegularExpressions;

    using DotNetNuke.Abstractions;
    using DotNetNuke.Services.Exceptions;

    /// <inheritdoc/>
    public class SerializationManager : ISerializationManager
    {
        /// <inheritdoc/>
        string ISerializationManager.SerializeValue<T>(T value) =>
            this.SerializeValue(value, null, typeof(T));

        /// <inheritdoc/>
        string ISerializationManager.SerializeValue<T>(T value, string serializer) =>
            this.SerializeValue(value, serializer, typeof(T));

        /// <inheritdoc/>
        string ISerializationManager.SerializeProperty<T>(T myObject, PropertyInfo property) =>
            ((ISerializationManager)this).SerializeProperty(myObject, property, null);

        /// <inheritdoc/>
        string ISerializationManager.SerializeProperty<T>(T myObject, PropertyInfo property, string serializer)
        {
            var settingValue = property.GetValue(myObject, null) ?? string.Empty;
            return this.SerializeValue(settingValue, serializer, property.PropertyType);
        }

        /// <inheritdoc />
        public T DeserializeValue<T>(string value) => ((ISerializationManager)this).DeserializeValue<T>(value, null);

        /// <inheritdoc />
        public T DeserializeValue<T>(string value, string serializer)
        {
            return (T)this.DeserializeValue(value, serializer, typeof(T));
        }

        /// <inheritdoc/>
        void ISerializationManager.DeserializeProperty<T>(T myObject, PropertyInfo property, string propertyValue) =>
            ((ISerializationManager)this).DeserializeProperty(myObject, property, propertyValue, null);

        /// <inheritdoc/>
        void ISerializationManager.DeserializeProperty<T>(T myObject, PropertyInfo property, string propertyValue, string serializer)
        {
            try
            {
                var propertyType = property.PropertyType;
                var deserializedValue = this.DeserializeValue(propertyValue, serializer, propertyType);
                property.SetValue(myObject, deserializedValue, null);
            }
            catch (Exception exception)
            {
                // TODO: Localize exception
                var message = string.Format(
                    CultureInfo.CurrentUICulture,
                    "Could not cast {0} to property {1} of type {2}",
                    propertyValue,
                    property.Name,
                    property.PropertyType);
                throw new InvalidCastException(message, exception);
            }
        }

        private static string GetSettingValueAsString<T>(T settingValue)
        {
            if (settingValue is DateTime dateTimeValue)
            {
                return dateTimeValue.ToString("o", CultureInfo.InvariantCulture);
            }

            if (settingValue is TimeSpan timeSpanValue)
            {
                return timeSpanValue.ToString("c", CultureInfo.InvariantCulture);
            }

            return Convert.ToString(settingValue, CultureInfo.InvariantCulture);
        }

        private static string ChangeFormatForBooleansIfNeeded(Type propertyType, string propertyValue)
        {
            if (!propertyType.Name.Equals("Boolean"))
            {
                return propertyValue;
            }

            if (bool.TryParse(propertyValue, out _))
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
            return method?.Invoke(serializer, new[] { value, });
        }

        private string SerializeValue<T>(T value, string serializer, Type valueType)
        {
            string settingValueAsString = null;
            if (!string.IsNullOrEmpty(serializer))
            {
                settingValueAsString = (string)CallSerializerMethod(serializer, valueType, value, nameof(ISettingsSerializer<T>.Serialize));
            }

            if (settingValueAsString == null)
            {
                settingValueAsString = GetSettingValueAsString(value);
            }

            return settingValueAsString;
        }

        private object DeserializeValue(
            string propertyValue,
            string serializer,
            Type destinationType)
        {
            if (destinationType.GetGenericArguments().Length != 0
                && destinationType.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                // Nullable type
                destinationType = destinationType.GetGenericArguments()[0];
                if (string.IsNullOrEmpty(propertyValue))
                {
                    return null;
                }
            }

            if (destinationType == propertyValue.GetType())
            {
                // The property and settingsValue have the same type - no conversion needed - just update!
                return propertyValue;
            }

            if (!string.IsNullOrEmpty(serializer))
            {
                return CallSerializerMethod(
                    serializer,
                    destinationType,
                    propertyValue,
                    nameof(ISettingsSerializer<int>.Deserialize));
            }

            if (destinationType.BaseType == typeof(Enum))
            {
                // The property is an enum. Determine if the enum value is persisted as string or numeric.
                if (Regex.IsMatch(propertyValue, "^\\d+$"))
                {
                    // The enum value is a number
                    return Enum.ToObject(destinationType, Convert.ToInt32(propertyValue, CultureInfo.InvariantCulture));
                }

                try
                {
                    return Enum.Parse(destinationType, propertyValue, true);
                }
                catch (ArgumentException exception)
                {
                    // Just log the exception. Use the default.
                    Exceptions.LogException(exception);
                }

                return 0;
            }

            if (destinationType.IsAssignableFrom(typeof(TimeSpan))
                && TimeSpan.TryParse(propertyValue, CultureInfo.InvariantCulture, out var timeSpanValue))
            {
                return timeSpanValue;
            }

            if (destinationType.IsAssignableFrom(typeof(DateTime))
                && DateTime.TryParse(
                    propertyValue,
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.RoundtripKind,
                    out var dateTimeValue))
            {
                return dateTimeValue;
            }

            if (destinationType.GetInterface(typeof(IConvertible).FullName) != null)
            {
                propertyValue = ChangeFormatForBooleansIfNeeded(destinationType, propertyValue);
                return Convert.ChangeType(propertyValue, destinationType, CultureInfo.InvariantCulture);
            }

            var converter = TypeDescriptor.GetConverter(destinationType);
            if (converter.IsValid(propertyValue))
            {
                return converter.ConvertFromInvariantString(propertyValue);
            }

            // TODO: Localize exception
            throw new InvalidCastException($"Could not cast {propertyValue} to {destinationType}");
        }
    }
}

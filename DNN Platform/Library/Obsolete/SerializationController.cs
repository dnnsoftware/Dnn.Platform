// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Entities.Modules.Settings
{
    using System;
    using System.Reflection;

    using DotNetNuke.Abstractions;
    using DotNetNuke.Common;
    using Microsoft.Extensions.DependencyInjection;

    [Obsolete("Deprecated in 9.8.0. Use Dependency Injection to resolve 'DotNetNuke.Abstractions.ISerializationManager' instead. Scheduled for removal in v11.0.0.")]
    public partial class SerializationController
    {
        static ISerializationManager SerializationManager =>
            Globals.DependencyProvider.GetRequiredService<ISerializationManager>();

        [Obsolete("Deprecated in 9.8.0. Use Dependency Injection to resolve 'DotNetNuke.Abstractions.ISerializationManager' instead. Scheduled for removal in v11.0.0.")]
        public static string SerializeProperty<T>(T myObject, PropertyInfo property) =>
            SerializationManager.SerializeProperty(myObject, property);

        [Obsolete("Deprecated in 9.8.0. Use Dependency Injection to resolve 'DotNetNuke.Abstractions.ISerializationManager' instead. Scheduled for removal in v11.0.0.")]
        public static string SerializeProperty<T>(T myObject, PropertyInfo property, string serializer) =>
            SerializationManager.SerializeProperty(myObject, property, serializer);

        [Obsolete("Deprecated in 9.8.0. Use Dependency Injection to resolve 'DotNetNuke.Abstractions.ISerializationManager' instead. Scheduled for removal in v11.0.0.")]
        public static void DeserializeProperty<T>(T myObject, PropertyInfo property, string propertyValue)
            where T : class, new() => SerializationManager.DeserializeProperty(myObject, property, propertyValue);

        [Obsolete("Deprecated in 9.8.0. Use Dependency Injection to resolve 'DotNetNuke.Abstractions.ISerializationManager' instead. Scheduled for removal in v11.0.0.")]
        public static void DeserializeProperty<T>(T myObject, PropertyInfo property, string propertyValue, string serializer)
            where T : class, new() => SerializationManager.DeserializeProperty(myObject, property, propertyValue, serializer);
    }
}

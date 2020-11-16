// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.UI.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Dnn.PersonaBar.Library;
    using Dnn.PersonaBar.Library.Attributes;
    using DotNetNuke.Framework.Reflections;
    using DotNetNuke.Web.Api;

    public class ServiceRouteMapper : IServiceRouteMapper
    {
        public void RegisterRoutes(IMapRoute routeManager)
        {
            // get all persona bar services from persona bar modules.
            var services = this.FindPersonaBarServices();

            if (services.Count > 0)
            {
                routeManager.MapHttpRoute("PersonaBar", "default", "{controller}/{action}", services.ToArray());
            }
        }

        private static IEnumerable<Type> GetAllApiControllers()
        {
            var typeLocator = new TypeLocator();
            return typeLocator.GetAllMatchingTypes(
                t => t != null &&
                     t.IsClass &&
                     !t.IsAbstract &&
                     t.IsVisible &&
                     typeof(PersonaBarApiController).IsAssignableFrom(t));
        }

        private IList<string> FindPersonaBarServices()
        {
            var controllerTypes = GetAllApiControllers();
            var namespaces = new List<string>();
            foreach (var type in controllerTypes)
            {
                var scopeAttr = (MenuPermissionAttribute)type.GetCustomAttributes(false).Cast<Attribute>().FirstOrDefault(a => a is MenuPermissionAttribute);
                if (scopeAttr != null)
                {
                    if (!namespaces.Contains(type.Namespace))
                    {
                        namespaces.Add(type.Namespace);
                    }
                }
            }

            return namespaces;
        }
    }
}

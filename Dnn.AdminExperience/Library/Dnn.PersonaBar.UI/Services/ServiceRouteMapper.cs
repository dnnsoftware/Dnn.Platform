// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using System;
using System.Collections.Generic;
using System.Linq;
using Dnn.PersonaBar.Library;
using Dnn.PersonaBar.Library.Attributes;
using DotNetNuke.Framework.Reflections;
using DotNetNuke.Web.Api;

namespace Dnn.PersonaBar.UI.Services
{
    public class ServiceRouteMapper : IServiceRouteMapper
    {
        public void RegisterRoutes(IMapRoute routeManager)
        {
            //get all persona bar services from persona bar modules.
            var services = FindPersonaBarServices();

            if (services.Count > 0)
            {
                routeManager.MapHttpRoute("PersonaBar", "default", "{controller}/{action}", services.ToArray());
            }
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
    }
}

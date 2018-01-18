#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2018
// by DotNetNuke Corporation
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation 
// the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and 
// to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions 
// of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
// TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
// CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
// DEALINGS IN THE SOFTWARE.
#endregion

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
#region Copyright
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2016
// by DotNetNuke Corporation
// All Rights Reserved
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
            var commonServices = new List<string>();
            var regularServices = new List<string>();
            var adminServices = new List<string>();
            var hostServices = new List<string>();
            var adminHostServices = new List<string>();

            //get all persona bar services from persona bar modules.
            FindPersonaBarServices(ref commonServices, ref regularServices, ref adminServices, ref hostServices, ref adminHostServices);

            if (commonServices.Count > 0)
            {
                routeManager.MapHttpRoute("PersonaBar/Common", "default", "{controller}/{action}", commonServices.ToArray());
            }

            if (regularServices.Count > 0)
            {
                routeManager.MapHttpRoute("PersonaBar/Regular", "default", "{controller}/{action}", regularServices.ToArray());
            }

            if (adminServices.Count > 0)
            { 
                routeManager.MapHttpRoute("PersonaBar/Admin", "default", "{controller}/{action}", adminServices.ToArray());
            }

            if (hostServices.Count > 0)
            {
                routeManager.MapHttpRoute("PersonaBar/Host", "default", "{controller}/{action}", hostServices.ToArray());
            }

            if (adminHostServices.Count > 0)
            {
                routeManager.MapHttpRoute("PersonaBar/AdminHost", "default", "{controller}/{action}", adminHostServices.ToArray());
            }

        }

        private void FindPersonaBarServices(ref List<string> commonServices, ref List<string> regularServices, ref List<string> adminServices, ref List<string> hostServices, ref List<string> adminHostServices)
        {
            var controllerTypes = GetAllApiControllers();
            foreach (var type in controllerTypes)
            {
                var scopeAttr = (ServiceScopeAttribute)type.GetCustomAttributes(false).Cast<Attribute>().FirstOrDefault(a => a is ServiceScopeAttribute);
                if (scopeAttr != null)
                {
                    var typeNamespace = type.Namespace;
                    switch (scopeAttr.Scope)
                    {
                        case ServiceScope.Admin:
                            if (!adminServices.Contains(typeNamespace))
                            {
                                adminServices.Add(typeNamespace);
                            }
                            break;
                        case ServiceScope.AdminHost:
                            if (!adminHostServices.Contains(typeNamespace))
                            {
                                adminHostServices.Add(typeNamespace);
                            }
                            break;
                        case ServiceScope.Host:
                            if (!hostServices.Contains(typeNamespace))
                            {
                                hostServices.Add(typeNamespace);
                            }
                            break;
                        case ServiceScope.Regular:
                            if (!regularServices.Contains(typeNamespace))
                            {
                                regularServices.Add(typeNamespace);
                            }
                            break;
                        default:
                            if (!commonServices.Contains(typeNamespace))
                            {
                                commonServices.Add(typeNamespace);
                            }
                            break;
                    }
                }
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
    }
}
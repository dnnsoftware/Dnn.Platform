// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Tests.Web.Api
{
    using System;
    using System.Diagnostics;

    using DotNetNuke.Web.Api;

    // various ServiceRouteMappers that will be reflected upon by the tests
    public class ReflectedServiceRouteMappers
    {
        public class EmbeddedServiceRouteMapper : IServiceRouteMapper
        {
            public void RegisterRoutes(IMapRoute mapRouteManager)
            {
                throw new NotImplementedException();
            }
        }
    }

    public class ExceptionOnCreateInstanceServiceRouteMapper : IServiceRouteMapper
    {
        public ExceptionOnCreateInstanceServiceRouteMapper(int i)
        {
            // no default constructor prevents Activator.CreateInstance from working
            Debug.WriteLine(i);
        }

        public void RegisterRoutes(IMapRoute mapRouteManager)
        {
            throw new NotImplementedException();
        }
    }

    public class ExceptionOnRegisterServiceRouteMapper : IServiceRouteMapper
    {
        public void RegisterRoutes(IMapRoute mapRouteManager)
        {
            throw new NotImplementedException();
        }
    }

    public class FakeServiceRouteMapper : IServiceRouteMapper
    {
        public static int RegistrationCalls { get; set; }

        public void RegisterRoutes(IMapRoute mapRouteManager)
        {
            RegistrationCalls++;
        }
    }

    public abstract class AbstractServiceRouteMapper : IServiceRouteMapper
    {
        public void RegisterRoutes(IMapRoute mapRouteManager)
        {
            throw new NotImplementedException();
        }
    }

    internal class InternalServiceRouteMapper : IServiceRouteMapper
    {
        public void RegisterRoutes(IMapRoute mapRouteManager)
        {
            throw new NotImplementedException();
        }
    }
}

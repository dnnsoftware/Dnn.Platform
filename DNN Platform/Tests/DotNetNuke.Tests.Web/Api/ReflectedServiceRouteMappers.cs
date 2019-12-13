using System;
using System.Diagnostics;

using DotNetNuke.Web.Api;

namespace DotNetNuke.Tests.Web.Api
{
    //various ServiceRouteMappers that will be reflected upon by the tests

    public class ReflectedServiceRouteMappers
    {
        #region Nested type: EmbeddedServiceRouteMapper

        public class EmbeddedServiceRouteMapper : IServiceRouteMapper
        {
            #region IServiceRouteMapper Members

            public void RegisterRoutes(IMapRoute mapRouteManager)
            {
                throw new NotImplementedException();
            }

            #endregion
        }

        #endregion
    }

    public class ExceptionOnCreateInstanceServiceRouteMapper : IServiceRouteMapper
    {
        public ExceptionOnCreateInstanceServiceRouteMapper(int i)
        {
            //no default constructor prevents Activator.CreateInstance from working
            Debug.WriteLine(i);
        }

        #region IServiceRouteMapper Members

        public void RegisterRoutes(IMapRoute mapRouteManager)
        {
            throw new NotImplementedException();
        }

        #endregion
    }

    public class ExceptionOnRegisterServiceRouteMapper : IServiceRouteMapper
    {
        #region IServiceRouteMapper Members

        public void RegisterRoutes(IMapRoute mapRouteManager)
        {
            throw new NotImplementedException();
        }

        #endregion
    }

    public class FakeServiceRouteMapper : IServiceRouteMapper
    {
        public static int RegistrationCalls { get; set; }

        #region IServiceRouteMapper Members

        public void RegisterRoutes(IMapRoute mapRouteManager)
        {
            RegistrationCalls++;
        }

        #endregion
    }

    public abstract class AbstractServiceRouteMapper : IServiceRouteMapper
    {
        #region IServiceRouteMapper Members

        public void RegisterRoutes(IMapRoute mapRouteManager)
        {
            throw new NotImplementedException();
        }

        #endregion
    }

    internal class InternalServiceRouteMapper : IServiceRouteMapper
    {
        #region IServiceRouteMapper Members

        public void RegisterRoutes(IMapRoute mapRouteManager)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}

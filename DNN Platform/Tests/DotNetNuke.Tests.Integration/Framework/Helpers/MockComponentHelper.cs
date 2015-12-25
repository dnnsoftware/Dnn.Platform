using DotNetNuke.ComponentModel;
using DotNetNuke.Services.Cache;
using Moq;

namespace DotNetNuke.Tests.Integration.Framework.Helpers
{
    /// <summary>
    /// Class copied from Platform Tests: DotNetNuke.Tests.Utilities.Mocks.MockComponentProvider
    /// 
    /// This class helps to mock any components that have been registered using the ComponentFactory
    /// </summary>
    public static class MockComponentHelper
    {
        public static Mock<CachingProvider> CreateDataCacheProvider()
        {
            return CreateNew<CachingProvider>();
        }

        public static Mock<T> CreateNew<T>() where T : class
        {
            if (ComponentFactory.Container == null)
            {
                ComponentFactory.Container = new SimpleContainer();
            }

            //Try and get mock
            var mockComp = ComponentFactory.GetComponent<Mock<T>>();
            var objComp = ComponentFactory.GetComponent<T>();

            if (mockComp == null)
            {
                mockComp = new Mock<T>();
                ComponentFactory.RegisterComponentInstance<Mock<T>>(mockComp);
            }

            if (objComp == null)
            {
                ComponentFactory.RegisterComponentInstance<T>(mockComp.Object);
            }

            return mockComp;
        }
    }
}
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DNN.Integration.Test.Framework.Helpers
{
    using DotNetNuke.ComponentModel;
    using DotNetNuke.Services.Cache;
    using Moq;

    /// <summary>
    /// Class copied from Platform Tests: DotNetNuke.Tests.Utilities.Mocks.MockComponentProvider
    ///
    /// This class helps to mock any components that have been registered using the ComponentFactory.
    /// </summary>
    public static class MockComponentHelper
    {
        public static Mock<CachingProvider> CreateDataCacheProvider()
        {
            return CreateNew<CachingProvider>();
        }

        public static Mock<T> CreateNew<T>()
            where T : class
        {
            if (ComponentFactory.Container == null)
            {
                ComponentFactory.Container = new SimpleContainer();
            }

            // Try and get mock
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

using DotNetNuke.Tests.Utilities.Mocks;

using NUnit.Framework;

namespace DotNetNuke.Tests.Core
{
    [SetUpFixture]
    internal class TestSetup
    {
        [SetUp]
        public void SetUp()
        {
        }

        [TearDown]
        public void TearDown()
        {
            MockComponentProvider.ResetContainer();
        }
    }
}

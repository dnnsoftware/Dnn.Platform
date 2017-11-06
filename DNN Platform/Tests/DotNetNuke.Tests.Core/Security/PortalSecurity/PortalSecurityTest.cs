using NUnit.Framework;
using DotNetNuke.Security;


namespace DotNetNuke.Tests.Core.Security.PortalSecurity
{
    [TestFixture]
    public class PortalSecurityTest
    {
        #region Setup
        [SetUp]
        public void Setup()
        {
            //TODO: implement environemnt set up
        }

        #endregion

        #region Tear Down
        [TearDown]
        public void TearDown()
        {
            //TODO:implement clean up
        }
        #endregion

        #region Test Methods
        [Test]
        public void Typing_Html_Source_Tag_Should_Not_Be_Allowed()
        {
            //Arrange
            var portalSecurity = new DotNetNuke.Security.PortalSecurity();
            var htmlEntry = @"<source>
                               Hi this is personal site
                               Test for source tag  
                              </source> ";
            var markup = DotNetNuke.Security.PortalSecurity.FilterFlag.NoMarkup |
                         DotNetNuke.Security.PortalSecurity.FilterFlag.NoScripting |
                         DotNetNuke.Security.PortalSecurity.FilterFlag.NoAngleBrackets;
            var expectedOutput = "  ";

            //Act
            var filterOutput = portalSecurity.InputFilter(htmlEntry, markup);

            //Assert
            Assert.AreEqual(filterOutput, expectedOutput);

        }
        #endregion
    }

}

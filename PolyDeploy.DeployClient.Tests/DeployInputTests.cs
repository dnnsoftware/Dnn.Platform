namespace PolyDeploy.DeployClient.Tests
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    using FakeItEasy;

    using Shouldly;

    using Xunit;
    using Xunit.Abstractions;

    public class DeployInputTests
    {
        [InlineData("", false)]
        [InlineData("/test", false)]
        [InlineData("https://test.com", true)]
        [Theory]
        public void Validate_TargetUri(string targetUri, bool isSuccess)
        {
            var validate = TestHelpers.CreateDeployInput(targetUri).Validate();

            validate.Successful.ShouldBe(isSuccess);

            validate.Message.ShouldBe(isSuccess ? null : "--target-uri must be a valid URI");
        }

        [InlineData(-1, false)]
        [InlineData(0, true)]
        [InlineData(1, true)]
        [Theory]
        public void Validate_InstallationStatusTimeout(int timeout, bool isSuccess)
        {
            var validate = TestHelpers.CreateDeployInput(installationStatusTimeout: timeout).Validate();

            validate.Successful.ShouldBe(isSuccess);

            validate.Message.ShouldBe(isSuccess ? null : "--installation-status-timeout must be non-negative");
        }

    }
}

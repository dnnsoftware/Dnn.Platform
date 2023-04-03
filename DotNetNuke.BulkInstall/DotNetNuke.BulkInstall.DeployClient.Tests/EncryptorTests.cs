// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Tests.BulkInstall.DeployClient
{
    using System.IO;
    using System.Text;
    using System.Threading.Tasks;
    using DotNetNuke.BulkInstall.DeployClient;
    using DotNetNuke.BulkInstall.Encryption;
    using Encryption;
    using Shouldly;
    using Xunit;

    public class EncryptorTests
    {
        [Fact]
        public async Task GetEncryptedStream_EncryptsFileContents()
        {
            var deployInput = TestHelpers.CreateDeployInput(encryptionKey: "abcd1234");
            var encryptor = new Encryptor();

            var encryptedStream = await encryptor.GetEncryptedStream(deployInput, new MemoryStream(Encoding.UTF8.GetBytes("ZIP")));

            var decryptedStream = Crypto.Decrypt(encryptedStream, deployInput.EncryptionKey);
            var decryptedContents = await new StreamReader(decryptedStream).ReadToEndAsync();
            decryptedContents.ShouldBe("ZIP");
        }
    }
}

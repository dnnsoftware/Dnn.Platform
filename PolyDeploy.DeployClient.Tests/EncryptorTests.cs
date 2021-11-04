namespace PolyDeploy.DeployClient.Tests
{
    using System.IO;
    using System.Text;
    using System.Threading.Tasks;
    using PolyDeploy.Encryption;
    using Shouldly;
    using Xunit;

    public class EncryptorTests
    {
        [Fact]
        public async Task GetEncryptedStream_EncryptsFileContents()
        {
            var deployInput = new DeployInput("https://example.com", "abcd1234");
            var encryptor = new Encryptor();

            var encryptedStream = await encryptor.GetEncryptedStream(deployInput, new MemoryStream(Encoding.UTF8.GetBytes("ZIP")));

            var decryptedStream = Crypto.Decrypt(encryptedStream, deployInput.EncryptionKey);
            var decyptedContents = new StreamReader(decryptedStream).ReadToEnd();
            decyptedContents.ShouldBe("ZIP");
        }
    }
}

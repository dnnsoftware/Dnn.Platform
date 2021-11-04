namespace PolyDeploy.DeployClient
{
    using System.IO;
    using System.Threading.Tasks;
    using PolyDeploy.Encryption;

    public class Encryptor : IEncryptor
    {
        public Task<Stream> GetEncryptedStream(DeployInput options, Stream packageStream)
        {
            return Task.FromResult(Crypto.Encrypt(packageStream, options.EncryptionKey));
        }
    }
}
namespace PolyDeploy.DeployClient
{
    using System.IO;
    using System.Threading.Tasks;

    public class Encryptor : IEncryptor
    {
        public Task<Stream> GetEncryptedStream(Stream packageStream)
        {
            throw new System.NotImplementedException();
        }
    }
}
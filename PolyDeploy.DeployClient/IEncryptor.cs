namespace PolyDeploy.DeployClient
{
    using System.IO;
    using System.Threading.Tasks;

    public interface IEncryptor
    {
        Task<Stream> GetEncryptedStream(Stream packageStream);
    }
}
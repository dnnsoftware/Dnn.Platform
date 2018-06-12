using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using DotNetNuke.Services.Installer;
using NUnit.Framework;

namespace DotNetNuke.Tests.Core.Services
{
    [TestFixture]
    public class UtilTest
    {
        [Test]
        public void Dnn_9838_TryToCreateAndExecute_UnsuccesfulRewrite()
        {
            TryToRewriteFile(lockFileFor: 1000, tryRewriteFileFor: 500, isRewritten: false);
        }

        [Test]
        public void Dnn_9838_TryToCreateAndExecute_SuccessfulRewrite()
        {
            TryToRewriteFile(lockFileFor: 500, tryRewriteFileFor: 1000, isRewritten: true);
        }

        private void TryToRewriteFile(int lockFileFor, int tryRewriteFileFor, bool isRewritten)
        {
            string filePath = Path.GetTempFileName();
            Debug.WriteLine(filePath);

            var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.ReadWrite, FileShare.None);

            var task = Task.Run(() =>
            {
                
                Debug.WriteLine("fileStream locked");
                Task.Delay(lockFileFor).Wait();

                fileStream.Dispose();
                Debug.WriteLine("fileStream unlocked");

            });

            Debug.WriteLine("Rewrite process attempts started");
            var result = Util.TryToCreateAndExecute(filePath, f =>
            {
                f.WriteByte(100);
                f.Flush();
            }, tryRewriteFileFor);

            Assert.AreEqual(isRewritten, result);
            task.Wait();
        }
    }
}
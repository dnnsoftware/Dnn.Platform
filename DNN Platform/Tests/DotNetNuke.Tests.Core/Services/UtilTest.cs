// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Tests.Core.Services
{
    using System.Diagnostics;
    using System.IO;
    using System.Threading.Tasks;

    using DotNetNuke.Services.Installer;
    using NUnit.Framework;

    [TestFixture]
    public class UtilTest
    {
        [Test]
        public void Dnn_9838_TryToCreateAndExecute_UnsuccesfulRewrite()
        {
            this.TryToRewriteFile(lockFileFor: 1000, tryRewriteFileFor: 500, isRewritten: false);
        }

        [Test]
        public void Dnn_9838_TryToCreateAndExecute_SuccessfulRewrite()
        {
            this.TryToRewriteFile(lockFileFor: 500, tryRewriteFileFor: 1000, isRewritten: true);
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

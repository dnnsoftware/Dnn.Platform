// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
using System.Configuration;
using System.IO;

using DotNetNuke.Entities.Controllers;
using DotNetNuke.Tests.UI.WatiN.Utilities;

namespace DotNetNuke.Tests.Utilities
{
    public class MailManager
    {
        private static string GetEmailDumpFolderPath()
        {
            return Directory.GetCurrentDirectory().Replace("\\Fixtures", "\\Packages") + "\\TestEmails";
        }

        public static void ClearDumpFolder()
        {
            string emailPath = GetEmailDumpFolderPath();

            if (Directory.Exists(emailPath))
            {
                foreach (var file in Directory.GetFiles(emailPath))
                {
                    File.Delete(file);
                }
            }
        }

        public static void SetUpMailDumpFolder()
        {
            string emailPath = GetEmailDumpFolderPath();
            var mailDropPath = Directory.GetCurrentDirectory().Replace("\\Fixtures", "\\Community\\Tests\\Packages");

            if (!Directory.Exists(emailPath))
            {
                Directory.CreateDirectory(emailPath);
            }

            WebConfigManager.UpdateConfigForMailDrop(mailDropPath, emailPath);
            HostController.Instance.Update("SMTPServer", "localhost", false);
        }
    }
}

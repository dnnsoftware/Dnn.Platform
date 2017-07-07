using Cantarus.Libraries.Encryption;
using Cantarus.Modules.PolyDeploy.DataAccess.Models;
using DotNetNuke.Entities.Modules;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Web;

namespace Cantarus.Modules.PolyDeploy.Components
{
    internal class CIDeploy : Deployment
    {
        private APIUser APIUser { get; set; }

        public CIDeploy(string apiKey) : base()
        {
            // Retrieve our API user.
            APIUser = APIUserController.GetByAPIKey(apiKey);

            // Did we find an API user?
            if (APIUser == null)
            {
                throw new Exception("API user not found, cannot continue. Shouldn't have been able to get here.");
            }
        }

        public void DecryptAndAddZip(Stream encryptedStream, string filename)
        {
            using (Stream ds = Crypto.Decrypt(encryptedStream, APIUser.EncryptionKey))
            {
                using (FileStream fs = File.Create(Path.Combine(IntakePath, filename)))
                {
                    ds.CopyTo(fs);
                }
            }
        }
    }
}

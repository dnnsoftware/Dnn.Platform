using Cantarus.Modules.PolyDeploy.Components.DataAccess.Models;
using DotNetNuke.Services.Log.EventLog;
using System;
using System.Collections.Generic;

namespace Cantarus.Modules.PolyDeploy.Components
{
    internal class RemoteDeployment : Deployment
    {
        private APIUser APIUser { get; set; }

        public RemoteDeployment(Session session, string ipAddress, string apiKey) : base(session, ipAddress)
        {
            // Retrieve our API user.
            APIUser = APIUserManager.GetByAPIKey(apiKey);

            // Did we find an API user?
            if (APIUser == null)
            {
                throw new Exception("API user not found, cannot continue. Shouldn't have been able to get here.");
            }
        }

        //public void DecryptAndAddZip(Stream encryptedStream, string filename)
        //{
        //    using (Stream ds = Crypto.Decrypt(encryptedStream, APIUser.EncryptionKey))
        //    {
        //        using (FileStream fs = File.Create(Path.Combine(IntakePath, filename)))
        //        {
        //            ds.CopyTo(fs);
        //        }
        //    }
        //}

        protected override void LogAnyFailures(List<InstallJob> jobs)
        {
            EventLogController elc = new EventLogController();

            foreach (InstallJob job in jobs)
            {
                foreach (string failure in job.Failures)
                {
                    string log = string.Format("(IP: {0} | APIUserID: {1}) {2}", IPAddress, APIUser.APIUserId, failure);

                    elc.AddLog("PolyDeploy", log, EventLogController.EventLogType.HOST_ALERT);
                }
            }
        }
    }
}

using Cantarus.Libraries.Encryption;
using Cantarus.Modules.PolyDeploy.DataAccess.DataControllers;
using Cantarus.Modules.PolyDeploy.DataAccess.Models;
using System;
using System.IO;

namespace Cantarus.Modules.PolyDeploy.Components
{
    internal static class SessionManager
    {
        private static SessionDataController SessionDC = new SessionDataController();

        public static Session CreateSession()
        {
            string directory = new DirectoryInfo(AvailableSessionDirectory()).Name;

            Session session = new Session(directory);

            SessionDC.Create(session);

            return session;
        }

        public static Session GetSession(string sessionGuid)
        {
            return SessionDC.FindByGuid(sessionGuid); ;
        }

        public static bool SessionExists(string sessionGuid)
        {
            Session session = SessionDC.FindByGuid(sessionGuid);

            if (session == null)
            {
                return false;
            }

            session.LastUsed = DateTime.Now;

            SessionDC.Update(session);

            return true;
        }

        public static void AddPackage(string sessionGuid, Stream packageStream, string filename)
        {
            Session session = SessionDC.FindByGuid(sessionGuid);

            if (session == null)
            {
                throw new Exception(string.Format("No session exists with guid: {0}", sessionGuid));
            }

            using (FileStream fs = File.Create(Path.Combine(Utilities.ModulePath, "Sessions", session.Guid, filename)))
            {
                packageStream.CopyTo(fs);
            }
        }

        public static string PathForSession (string sessionGuid)
        {
            Session session = SessionDC.FindByGuid(sessionGuid);

            if (session == null)
            {
                throw new Exception(string.Format("No session exists with guid: {0}", sessionGuid));
            }

            return Path.Combine(Utilities.ModulePath, "Sessions", session.Guid);
        }

        private static string AvailableSessionDirectory()
        {
            // Start with the module root.
            string basePath = Utilities.ModulePath;

            // Check we found the module directory.
            if (Directory.Exists(basePath))
            {
                // Prepare a sessions directory.
                basePath = Path.Combine(basePath, "Sessions");

                // Does it exist?
                if (!Directory.Exists(basePath))
                {
                    // No, create it.
                    Directory.CreateDirectory(basePath);
                }
            }

            // Generate a random folder in the desired path.
            string dir = Path.Combine(basePath, Utilities.RandomName());

            // Does it already exist? I doubt it.
            if (Directory.Exists(dir))
            {
                // My mistake, try again!
                return AvailableSessionDirectory();
            }

            // Create the folder.
            Directory.CreateDirectory(dir);

            return dir;
        }
    }
}

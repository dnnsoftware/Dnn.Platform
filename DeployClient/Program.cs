using Cantarus.Libraries.Encryption;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.IO;

namespace DeployClient
{
    class Program
    {
        private static bool IsSilent = false;
        private static bool NoPrompt = false;

        enum ExitCode : int
        {
            Success = 0,
            Error = 1,
            NoModulesFound = 2,
            UserExit = 3,
            InstallFailure = 4
        }

        static void Main(string[] args)
        {
            try
            {
                foreach (string arg in args)
                {
                    if (arg.ToLower().Contains("--silent"))
                    {
                        IsSilent = true;
                    }

                    if (arg.ToLower().Contains("--no-prompt"))
                    {
                        NoPrompt = true;
                    }
                }

                // Output start.
                WriteLine("*** Polly Deployment Client ***");
                WriteLine();

                // Get the properties we need.
                string targetUri = Properties.Settings.Default.TargetUri;
                string apiKey = Properties.Settings.Default.APIKey;
                string encryptionKey = Properties.Settings.Default.EncryptionKey;

                // Do we have a target uri.
                if (targetUri.Equals(string.Empty))
                {
                    throw new Exception("No target uri has been set.");
                }

                // Do we have an api key?
                if (apiKey.Equals(string.Empty))
                {
                    throw new Exception("No api key has been set.");
                }

                // Do we have an encryption key?
                if (encryptionKey.Equals(string.Empty))
                {
                    throw new Exception("No encryption key has been set.");
                }

                // Output identifying module archives.
                WriteLine("Identifying module archives...");

                // Read zip files in current directory.
                string currentDirectory = Directory.GetCurrentDirectory();
                List<string> zipFiles = new List<string>(Directory.GetFiles(currentDirectory, "*.zip"));

                // Is there something to do?
                if (zipFiles.Count <= 0)
                {
                    // No, exit.
                    WriteLine("No module archives found.");
                    WriteLine("Exiting.");
                    ReadLine();
                    Environment.Exit((int)ExitCode.NoModulesFound);
                }

                // Inform user of modules found.
                WriteLine(string.Format("Found {0} module archives in {1}:", zipFiles.Count, currentDirectory));

                foreach (string zipFile in zipFiles)
                {
                    WriteLine(string.Format("\t{0}. {1}", zipFiles.IndexOf(zipFile) + 1, Path.GetFileName(zipFile)));
                }
                WriteLine();

                if (!NoPrompt)
                {
                    // Prompt to continue.
                    WriteLine("Would you like to continue? (y/n)");

                    // Continue?
                    if (!Confirm())
                    {
                        // No, exit.
                        WriteLine("Exiting.");
                        Environment.Exit((int)ExitCode.UserExit);
                    }
                    WriteLine();
                }

                // Inform user of encryption.
                WriteLine("Starting encryption and upload...");

                // Get a session.
                string session = API.CreateSession();

                WriteLine(string.Format("Got session: {0}", session));

                DateTime startTime = DateTime.Now;

                foreach (string zipFile in zipFiles)
                {

                    using (FileStream fs = new FileStream(zipFile, FileMode.Open))
                    {
                        Write(string.Format("\t{0} encrypting...", Path.GetFileName(zipFile)));

                        using (Stream es = Crypto.Encrypt(fs, Properties.Settings.Default.EncryptionKey))
                        {
                            Write("uploading...");

                            API.AddPackageAsync(session, es, Path.GetFileName(zipFile));
                        }

                        WriteLine("done.");
                    }
                }

                WriteLine(string.Format("Finished encryption and upload in {0} ms.", (DateTime.Now - startTime).TotalMilliseconds));
                WriteLine();

                WriteLine("Starting installation...");

                DateTime installStartTime = DateTime.Now;

                Dictionary<string, dynamic> results = API.Install(session);

                ArrayList installed = results.ContainsKey("Installed") ? results["Installed"] : null;
                ArrayList failed = results.ContainsKey("Failed") ? results["Failed"] : null;

                // Any failures?
                if (failed.Count > 0)
                {
                    WriteLine(string.Format("{0} module archives failed to install.", failed.Count));
                    ReadLine();
                    Environment.Exit((int)ExitCode.InstallFailure);
                }

                // Output result
                WriteLine(string.Format("{0} module archives installed successfully.", installed.Count));
                ReadLine();
                WriteLine(string.Format("Finished installation in {0} ms.", (DateTime.Now - installStartTime).TotalMilliseconds));
            }
            catch (Exception ex)
            {
                // Output exception message and stack trace.
                WriteLine(string.Format("Exception caught at: {0}.", DateTime.Now.ToString()));
                WriteException(ex);

                ReadLine();
                Environment.Exit((int)ExitCode.Error);
            }
        }

        private static void WriteException(Exception ex, int maxDepth = 10, int depth = 0)
        {
            WriteLine(ex.Message);
            WriteLine(ex.StackTrace);

            if (depth < maxDepth && ex.InnerException != null)
            {
                depth++;
                WriteException(ex.InnerException, maxDepth, depth);
            }
        }

        private static void Write(string message)
        {
            if(IsSilent)
            {
                return;
            }

            Console.Write(message);
        }

        private static void WriteLine(string message = "")
        {
            if(IsSilent)
            {
                return;
            }

            Console.WriteLine(message);
        }

        private static string ReadLine()
        {
            if (IsSilent || NoPrompt)
            {
                return null;
            }

            return Console.ReadLine();
        }

        private static bool Confirm()
        {
            if (IsSilent || NoPrompt)
            {
                return true;
            }

            char ch = Console.ReadKey(true).KeyChar;

            if (ch.Equals('y') || ch.Equals('Y'))
            {
                return true;
            }

            return false;
        }
    }
}

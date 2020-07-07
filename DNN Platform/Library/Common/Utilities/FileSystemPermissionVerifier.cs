// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Common.Utilities
{
    using System;
    using System.IO;

    using DotNetNuke.Common.Utilities.Internal;
    using DotNetNuke.Instrumentation;

    /// <summary>
    ///   Verifies the abililty to create and delete files and folders.
    /// </summary>
    /// <remarks>
    ///   This class is not meant for use in modules, or in any other manner outside the DotNetNuke core.
    /// </remarks>
    public class FileSystemPermissionVerifier
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(FileSystemPermissionVerifier));
        private readonly string _basePath;

        private int _retryTimes = 30;

        public FileSystemPermissionVerifier(string basePath)
        {
            this._basePath = basePath;
        }

        public FileSystemPermissionVerifier(string basePath, int retryTimes)
            : this(basePath)
        {
            this._retryTimes = retryTimes;
        }

        /// <summary>
        /// Gets base path need to verify permission.
        /// </summary>
        public string BasePath
        {
            get
            {
                return this._basePath;
            }
        }

        public bool VerifyAll()
        {
            lock (typeof(FileSystemPermissionVerifier))
            {
                // All these steps must be executed in this sequence as one unit
                return this.VerifyFolderCreate() &&
                       this.VerifyFileCreate() &&
                       this.VerifyFileDelete() &&
                       this.VerifyFolderDelete();
            }
        }

        private static void FileCreateAction(string verifyPath)
        {
            if (File.Exists(verifyPath))
            {
                File.Delete(verifyPath);
            }

            using (File.Create(verifyPath))
            {
                // do nothing just let it close
            }
        }

        private static void FolderCreateAction(string verifyPath)
        {
            if (Directory.Exists(verifyPath))
            {
                Directory.Delete(verifyPath, true);
            }

            Directory.CreateDirectory(verifyPath);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///   VerifyFileCreate checks whether a file can be created.
        /// </summary>
        /// -----------------------------------------------------------------------------
        private bool VerifyFileCreate()
        {
            string verifyPath = Path.Combine(this._basePath, "Verify\\Verify.txt");
            bool verified = true;

            // Attempt to create the File
            try
            {
                this.Try(() => FileCreateAction(verifyPath), "Creating verification file");
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                verified = false;
            }

            return verified;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///   VerifyFileDelete checks whether a file can be deleted.
        /// </summary>
        /// -----------------------------------------------------------------------------
        private bool VerifyFileDelete()
        {
            string verifyPath = Path.Combine(this._basePath, "Verify\\Verify.txt");
            bool verified = true;

            // Attempt to delete the File
            try
            {
                this.Try(() => File.Delete(verifyPath), "Deleting verification file");
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                verified = false;
            }

            return verified;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///   VerifyFolderCreate checks whether a folder can be created.
        /// </summary>
        /// -----------------------------------------------------------------------------
        private bool VerifyFolderCreate()
        {
            string verifyPath = Path.Combine(this._basePath, "Verify");
            bool verified = true;

            // Attempt to create the Directory
            try
            {
                this.Try(() => FolderCreateAction(verifyPath), "Creating verification folder");
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                verified = false;
            }

            return verified;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///   VerifyFolderDelete checks whether a folder can be deleted.
        /// </summary>
        /// -----------------------------------------------------------------------------
        private bool VerifyFolderDelete()
        {
            string verifyPath = Path.Combine(this._basePath, "Verify");
            bool verified = true;

            // Attempt to delete the Directory
            try
            {
                this.Try(() => Directory.Delete(verifyPath, true), "Deleting verification folder");
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                verified = false;
            }

            return verified;
        }

        private void Try(Action action, string description)
        {
            new RetryableAction(action, description, this._retryTimes, TimeSpan.FromSeconds(1)).TryIt();
        }
    }
}

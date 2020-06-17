// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

#if INDIVIDUAL_LOCKS
using System.Collections;
#endif

namespace DotNetNuke.Services.GeneratedImage
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Web;
    using System.Web.Hosting;

    internal interface IImageStore
    {
        void Add(string id, byte[] data);

        bool TryTransmitIfContains(string id, HttpResponseBase response);

        void ForcePurgeFromServerCache(string cacheId);
    }

    public class DiskImageStore : IImageStore
    {
        private const string TempFileExtension = ".tmp";
        private const string CacheAppRelativePath = @"~\App_Data\_imagecache\";
        private static DiskImageStore _diskImageStore;
        private static readonly object InstanceLock = new object();
        private static string _cachePath;
        private static TimeSpan _purgeInterval;
        private DateTime _lastPurge;
        private readonly object _purgeQueuedLock = new object();
        private bool _purgeQueued;

#if INDIVIDUAL_LOCKS
        private Hashtable _fileLocks = new Hashtable();
#else
        private readonly object _fileLock = new object();
        static DiskImageStore()
        {
            EnableAutoPurge = true;
            PurgeInterval = new TimeSpan(0, 5, 0);
            CachePath = HostingEnvironment.MapPath(CacheAppRelativePath);
        }

        internal DiskImageStore()
        {
            if (CachePath != null && !Directory.Exists(CachePath))
            {
                Directory.CreateDirectory(CachePath);
            }

            this._lastPurge = DateTime.Now;
        }

#endif

        public static string CachePath
        {
            get
            {
                return _cachePath;
            }

            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    throw new ArgumentNullException(nameof(value));
                }

                _cachePath = value;
            }
        }

        public static bool EnableAutoPurge { get; set; } // turn on/off purge feature

        public static TimeSpan PurgeInterval
        {
            get
            {
                return _purgeInterval;
            }

            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException(nameof(value));
                }

                if (value.Ticks < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(value));
                }

                _purgeInterval = value;
            }
        }

        internal static IImageStore Instance
        {
            get
            {
                if (_diskImageStore == null)
                {
                    lock (InstanceLock)
                    {
                        if (_diskImageStore == null)
                        {
                            _diskImageStore = new DiskImageStore();
                        }
                    }
                }

                return _diskImageStore;
            }
        }

        private DateTime LastPurge
        {
            get
            {
                if (this._lastPurge < new DateTime(1990, 1, 1))
                {
                    this._lastPurge = DateTime.Now.Subtract(PurgeInterval);
                }

                return this._lastPurge;
            }

            set
            {
                this._lastPurge = value;
            }
        }

        public void ForcePurgeFromServerCache(string cacheId)
        {
            var files = new DirectoryInfo(CachePath).GetFiles();
            var fileInfo = files.FirstOrDefault(file => file.Name.Contains(cacheId));
            try
            {
                fileInfo?.Delete();
            }
            catch (Exception)
            {
                // do nothing at this point.
            }
        }

        void IImageStore.Add(string id, byte[] data)
        {
            this.Add(id, data);
        }

        bool IImageStore.TryTransmitIfContains(string id, HttpResponseBase response)
        {
            return this.TryTransmitIfContains(id, response);
        }

#if INDIVIDUAL_LOCKS
        private static string GetEntryId(FileInfo fileinfo) {
            string id = fileinfo.Name.Substring(0, fileinfo.Name.Length - s_tempFileExtension.Length);
            return id;
        }

        private void DiscardFileLockObject(string id) {
            // lock on hashtable to prevent other writers
            lock (_fileLocks) {
                _fileLocks.Remove(id);
            }
        }
#endif

        private static string BuildFilePath(string id)
        {
            return CachePath + id + TempFileExtension;
        }

        private void PurgeCallback(object target)
        {
            var files = new DirectoryInfo(CachePath).GetFiles();
            var threshold = DateTime.Now.Subtract(PurgeInterval);
            var toTryDeleteAgain = new List<FileInfo>();
            foreach (var fileinfo in files)
            {
                if (fileinfo.CreationTime < threshold)
                {
#if INDIVIDUAL_LOCKS
                    string id = GetEntryId(fileinfo);
                    object lockObject = GetFileLockObject(id);
                    if (lockObject != null) {
                        if (!Monitor.TryEnter(lockObject)) {
                            toTryDeleteAgain.Add(fileinfo);
                            continue;
                        }

                        try {
                            fileinfo.Delete();
                            DiscardFileLockObject(id);
                        }
                        catch (Exception) {
                            // do nothing
                        }
                        finally {
                            Monitor.Exit(lockObject);
                        }
                    }
#else
                    try
                    {
                        fileinfo.Delete();
                    }
                    catch (Exception)
                    {
                        toTryDeleteAgain.Add(fileinfo);
                    }
#endif
                }
            }

            Thread.Sleep(0);
            foreach (var fileinfo in toTryDeleteAgain)
            {
#if INDIVIDUAL_LOCKS
                string id = GetEntryId(fileinfo);
                object lockObject = GetFileLockObject(id);
                if (!Monitor.TryEnter(lockObject)) {
                    continue;
                }
                try {
                    fileinfo.Delete();
                    DiscardFileLockObject(id);
                }
                catch (Exception) {
                    // do nothing, delete will be tried next time purge is called
                }
                finally {
                    Monitor.Exit(lockObject);
                }
#else
                try
                {
                    fileinfo.Delete();
                }
                catch (Exception)
                {
                    // do nothing at this point, try to delete file during next purge
                }
#endif
            }

            this.LastPurge = DateTime.Now;
            this._purgeQueued = false;
        }

        private void Add(string id, byte[] data)
        {
            var path = BuildFilePath(id);
            lock (this.GetFileLockObject(id))
            {
                try
                {
                    File.WriteAllBytes(path, data);
                }
                catch (Exception)
                {
                    // REVIEW for now ignore any write problems
                }
            }
        }

        private bool TryTransmitIfContains(string id, HttpResponseBase response)
        {
            if (EnableAutoPurge)
            {
                this.QueueAutoPurge();
            }

            string path = BuildFilePath(id);
            lock (this.GetFileLockObject(id))
            {
                if (File.Exists(path))
                {
                    response.TransmitFile(path);
                    return true;
                }

                return false;
            }
        }

        private void QueueAutoPurge()
        {
            var now = DateTime.Now;
            if (!this._purgeQueued && now.Subtract(this.LastPurge) > PurgeInterval)
            {
                lock (this._purgeQueuedLock)
                {
                    if (!this._purgeQueued)
                    {
                        this._purgeQueued = true;
                        ThreadPool.QueueUserWorkItem(this.PurgeCallback);
                    }
                }
            }
        }

        private object GetFileLockObject(string id)
        {
#if INDIVIDUAL_LOCKS
            object lockObject = _fileLocks[id];

            if (lockObject == null) {
                // lock on the hashtable to prevent other writers
                lock (_fileLocks) {
                    lockObject = new object();
                    _fileLocks[id] = lockObject;
                }
            }

            return lockObject;
#else
            return this._fileLock;
#endif
        }
    }
}

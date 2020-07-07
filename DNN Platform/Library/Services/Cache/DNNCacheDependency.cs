// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Cache
{
    using System;
    using System.Web.Caching;

    /// <summary>
    /// DNNCacheDependency provides dependency policies of cache system.
    /// </summary>
    /// <remarks>
    /// The CacheDependency class monitors the dependency relationships so that when any of them changes, the cached item will be automatically removed.
    /// </remarks>
    public class DNNCacheDependency : IDisposable
    {
        private readonly DateTime _utcStart = DateTime.MaxValue;
        private DNNCacheDependency _cacheDependency;
        private string[] _cacheKeys;
        private string[] _fileNames;
        private CacheDependency _systemCacheDependency;

        /// <summary>
        /// Initializes a new instance of the <see cref="DNNCacheDependency"/> class.
        /// </summary>
        /// <param name="systemCacheDependency">The system cache dependency.</param>
        public DNNCacheDependency(CacheDependency systemCacheDependency)
        {
            this._systemCacheDependency = systemCacheDependency;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DNNCacheDependency"/> class that monitors a file or directory for changes.
        /// </summary>
        /// <param name="filename"></param>
        public DNNCacheDependency(string filename)
        {
            this._fileNames = new[] { filename };
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DNNCacheDependency"/> class that monitors an array of paths (to files or directories) for changes.
        /// </summary>
        /// <param name="filenames">set the cache depend on muti files.</param>
        public DNNCacheDependency(string[] filenames)
        {
            this._fileNames = filenames;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DNNCacheDependency"/> class that monitors an array of paths
        ///  (to files or directories) for changes and specifies a time when change monitoring begins.
        /// </summary>
        /// <param name="filenames">The filenames.</param>
        /// <param name="start">The start.</param>
        public DNNCacheDependency(string[] filenames, DateTime start)
        {
            this._utcStart = start.ToUniversalTime();
            this._fileNames = filenames;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DNNCacheDependency"/> class that monitors an array of paths (to files or directories),
        /// an array of cache keys, or both for changes.
        /// </summary>
        /// <param name="filenames">The filenames.</param>
        /// <param name="cachekeys">The cachekeys.</param>
        public DNNCacheDependency(string[] filenames, string[] cachekeys)
        {
            this._fileNames = filenames;
            this._cacheKeys = cachekeys;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DNNCacheDependency"/> class that monitors a file or directory for changes.
        /// </summary>
        /// <param name="filename">The filename.</param>
        /// <param name="start">The start.</param>
        public DNNCacheDependency(string filename, DateTime start)
        {
            this._utcStart = start.ToUniversalTime();
            if (filename != null)
            {
                this._fileNames = new[] { filename };
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DNNCacheDependency"/> class that monitors an array of paths (to files or directories),
        /// an array of cache keys, or both for changes.
        /// </summary>
        /// <param name="filenames">The filenames.</param>
        /// <param name="cachekeys">The cachekeys.</param>
        /// <param name="start">The start.</param>
        public DNNCacheDependency(string[] filenames, string[] cachekeys, DateTime start)
        {
            this._utcStart = start.ToUniversalTime();
            this._fileNames = filenames;
            this._cacheKeys = cachekeys;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DNNCacheDependency"/> classthat monitors an array of paths (to files or directories),
        /// an array of cache keys, or both for changes. It also makes itself dependent upon a separate instance of the <see cref="DNNCacheDependency"/> class.
        /// </summary>
        /// <param name="filenames">The filenames.</param>
        /// <param name="cachekeys">The cachekeys.</param>
        /// <param name="dependency">The dependency.</param>
        public DNNCacheDependency(string[] filenames, string[] cachekeys, DNNCacheDependency dependency)
        {
            this._fileNames = filenames;
            this._cacheKeys = cachekeys;
            this._cacheDependency = dependency;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DNNCacheDependency"/> class that monitors an array of paths (to files or directories),
        /// an array of cache keys, or both for changes.
        /// It also makes itself dependent upon another instance of the <see cref="DNNCacheDependency"/> class and a time when the change monitoring begins.
        /// </summary>
        /// <param name="filenames">The filenames.</param>
        /// <param name="cachekeys">The cachekeys.</param>
        /// <param name="dependency">The dependency.</param>
        /// <param name="start">The start.</param>
        public DNNCacheDependency(string[] filenames, string[] cachekeys, DNNCacheDependency dependency, DateTime start)
        {
            this._utcStart = start.ToUniversalTime();
            this._fileNames = filenames;
            this._cacheKeys = cachekeys;
            this._cacheDependency = dependency;
        }

        /// <summary>
        /// Gets the cache keys.
        /// </summary>
        public string[] CacheKeys
        {
            get
            {
                return this._cacheKeys;
            }
        }

        /// <summary>
        /// Gets the file names.
        /// </summary>
        public string[] FileNames
        {
            get
            {
                return this._fileNames;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance has changed.
        /// </summary>
        /// <value>
        ///     <c>true</c> if this instance has changed; otherwise, <c>false</c>.
        /// </value>
        public bool HasChanged
        {
            get
            {
                return this.SystemCacheDependency.HasChanged;
            }
        }

        /// <summary>
        /// Gets the cache dependency.
        /// </summary>
        public DNNCacheDependency CacheDependency
        {
            get
            {
                return this._cacheDependency;
            }
        }

        /// <summary>
        /// Gets the start time.
        /// </summary>
        public DateTime StartTime
        {
            get
            {
                return this._utcStart;
            }
        }

        /// <summary>
        /// Gets the system cache dependency.
        /// </summary>
        public CacheDependency SystemCacheDependency
        {
            get
            {
                if (this._systemCacheDependency == null)
                {
                    if (this._cacheDependency == null)
                    {
                        this._systemCacheDependency = new CacheDependency(this._fileNames, this._cacheKeys, this._utcStart);
                    }
                    else
                    {
                        this._systemCacheDependency = new CacheDependency(this._fileNames, this._cacheKeys, this._cacheDependency.SystemCacheDependency, this._utcStart);
                    }
                }

                return this._systemCacheDependency;
            }
        }

        /// <summary>
        /// Gets the UTC last modified.
        /// </summary>
        public DateTime UtcLastModified
        {
            get
            {
                return this.SystemCacheDependency.UtcLastModified;
            }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        // Method that does the actual disposal of resources
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this._cacheDependency != null)
                {
                    this._cacheDependency.Dispose(disposing);
                }

                if (this._systemCacheDependency != null)
                {
                    this._systemCacheDependency.Dispose();
                }

                this._fileNames = null;
                this._cacheKeys = null;
                this._cacheDependency = null;
                this._systemCacheDependency = null;
            }
        }
    }
}

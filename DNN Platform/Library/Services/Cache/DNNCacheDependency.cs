#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2017
// by DotNetNuke Corporation
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation 
// the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and 
// to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions 
// of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
// TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
// CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
// DEALINGS IN THE SOFTWARE.
#endregion
#region Usings

using System;
using System.Web.Caching;

#endregion

namespace DotNetNuke.Services.Cache
{
	/// <summary>
	/// DNNCacheDependency provides dependency policies of cache system.
	/// </summary>
	/// <remarks>
	/// The CacheDependency class monitors the dependency relationships so that when any of them changes, the cached item will be automatically removed.
	/// </remarks>
    public class DNNCacheDependency : IDisposable
    {
		#region "Private Members"

        private readonly DateTime _utcStart = DateTime.MaxValue;
        private DNNCacheDependency _cacheDependency;
        private string[] _cacheKeys;
        private string[] _fileNames;
        private CacheDependency _systemCacheDependency;
		
		#endregion

		#region "Constructors"

		/// <summary>
		/// Initializes a new instance of the <see cref="DNNCacheDependency"/> class.
		/// </summary>
		/// <param name="systemCacheDependency">The system cache dependency.</param>
        public DNNCacheDependency(CacheDependency systemCacheDependency)
        {
            _systemCacheDependency = systemCacheDependency;
        }

		/// <summary>
		/// Initializes a new instance of the <see cref="DNNCacheDependency"/> class that monitors a file or directory for changes.
		/// </summary>
		/// <param name="filename"></param>
        public DNNCacheDependency(string filename)
        {
            _fileNames = new[] {filename};
        }

		/// <summary>
		/// Initializes a new instance of the <see cref="DNNCacheDependency"/> class that monitors an array of paths (to files or directories) for changes.
		/// </summary>
		/// <param name="filenames">set the cache depend on muti files.</param>
        public DNNCacheDependency(string[] filenames)
        {
            _fileNames = filenames;
        }

		/// <summary>
		/// Initializes a new instance of the <see cref="DNNCacheDependency"/> class that monitors an array of paths
		///  (to files or directories) for changes and specifies a time when change monitoring begins.
		/// </summary>
		/// <param name="filenames">The filenames.</param>
		/// <param name="start">The start.</param>
        public DNNCacheDependency(string[] filenames, DateTime start)
        {
            _utcStart = start.ToUniversalTime();
            _fileNames = filenames;
        }

		/// <summary>
		/// Initializes a new instance of the <see cref="DNNCacheDependency"/> class that monitors an array of paths (to files or directories), 
		/// an array of cache keys, or both for changes.
		/// </summary>
		/// <param name="filenames">The filenames.</param>
		/// <param name="cachekeys">The cachekeys.</param>
        public DNNCacheDependency(string[] filenames, string[] cachekeys)
        {
            _fileNames = filenames;
            _cacheKeys = cachekeys;
        }

		/// <summary>
		/// Initializes a new instance of the <see cref="DNNCacheDependency"/> class that monitors a file or directory for changes.
		/// </summary>
		/// <param name="filename">The filename.</param>
		/// <param name="start">The start.</param>
        public DNNCacheDependency(string filename, DateTime start)
        {
            _utcStart = start.ToUniversalTime();
            if (filename != null)
            {
                _fileNames = new[] {filename};
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
            _utcStart = start.ToUniversalTime();
            _fileNames = filenames;
            _cacheKeys = cachekeys;
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
            _fileNames = filenames;
            _cacheKeys = cachekeys;
            _cacheDependency = dependency;
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
            _utcStart = start.ToUniversalTime();
            _fileNames = filenames;
            _cacheKeys = cachekeys;
            _cacheDependency = dependency;
        }
		
		#endregion

		#region "Public Properties"

		/// <summary>
		/// Gets the cache keys.
		/// </summary>
        public string[] CacheKeys
        {
            get
            {
                return _cacheKeys;
            }
        }

		/// <summary>
		/// Gets the file names.
		/// </summary>
        public string[] FileNames
        {
            get
            {
                return _fileNames;
            }
        }

		/// <summary>
		/// Gets a value indicating whether this instance has changed.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if this instance has changed; otherwise, <c>false</c>.
		/// </value>
        public bool HasChanged
        {
            get
            {
                return SystemCacheDependency.HasChanged;
            }
        }

		/// <summary>
		/// Gets the cache dependency.
		/// </summary>
        public DNNCacheDependency CacheDependency
        {
            get
            {
                return _cacheDependency;
            }
        }

		/// <summary>
		/// Gets the start time.
		/// </summary>
        public DateTime StartTime
        {
            get
            {
                return _utcStart;
            }
        }

		/// <summary>
		/// Gets the system cache dependency.
		/// </summary>
        public CacheDependency SystemCacheDependency
        {
            get
            {
                if (_systemCacheDependency == null)
                {
                    if (_cacheDependency == null)
                    {
                        _systemCacheDependency = new CacheDependency(_fileNames, _cacheKeys, _utcStart);
                    }
                    else
                    {
                        _systemCacheDependency = new CacheDependency(_fileNames, _cacheKeys, _cacheDependency.SystemCacheDependency, _utcStart);
                    }
                }
                return _systemCacheDependency;
            }
        }

		/// <summary>
		/// Gets the UTC last modified.
		/// </summary>
        public DateTime UtcLastModified
        {
            get
            {
                return SystemCacheDependency.UtcLastModified;
            }
        }
		
		#endregion

        #region IDisposable Members

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }


        //Method that does the actual disposal of resources
        protected virtual void Dispose(bool disposing)
        {
            if ((disposing))
            {
                if (_cacheDependency != null)
                {
                    _cacheDependency.Dispose(disposing);
                }
                if (_systemCacheDependency != null)
                {
                    _systemCacheDependency.Dispose();
                }
                _fileNames = null;
                _cacheKeys = null;
                _cacheDependency = null;
                _systemCacheDependency = null;
            }
        }
		
		#endregion
    }
}
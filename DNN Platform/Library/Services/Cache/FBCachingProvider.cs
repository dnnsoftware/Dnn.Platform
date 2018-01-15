#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2018
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
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Web.Caching;

using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Instrumentation;

#endregion

namespace DotNetNuke.Services.Cache
{
    public class FBCachingProvider : CachingProvider
    {
    	private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof (FBCachingProvider));
        internal const string CacheFileExtension = ".resources";
        internal static string CachingDirectory = "Cache\\";

		#region Abstract Method Implementation

        public override void Insert(string cacheKey, object itemToCache, DNNCacheDependency dependency, DateTime absoluteExpiration, TimeSpan slidingExpiration, CacheItemPriority priority,
                                    CacheItemRemovedCallback onRemoveCallback)
        {
			//initialize cache dependency
            DNNCacheDependency d = dependency;

            //if web farm is enabled
            if (IsWebFarm())
            {
                //get hashed file name
                var f = new string[1];
                f[0] = GetFileName(cacheKey);
                //create a cache file for item
                CreateCacheFile(f[0], cacheKey);
                //create a cache dependency on the cache file
                d = new DNNCacheDependency(f, null, dependency);
            }
			
            //Call base class method to add obect to cache
            base.Insert(cacheKey, itemToCache, d, absoluteExpiration, slidingExpiration, priority, onRemoveCallback);
        }

        public override bool IsWebFarm()
        {
            bool _IsWebFarm = Null.NullBoolean;
            if (!string.IsNullOrEmpty(Config.GetSetting("IsWebFarm")))
            {
                _IsWebFarm = bool.Parse(Config.GetSetting("IsWebFarm"));
            }
            return _IsWebFarm;
        }

        public override string PurgeCache()
        {
            //called by scheduled job to remove cache files which are no longer active
            return PurgeCacheFiles(Globals.HostMapPath + CachingDirectory);
        }

        public override void Remove(string Key)
        {
            base.Remove(Key);

            //if web farm is enabled in config file
            if (IsWebFarm())
            {
                //get hashed filename
                string f = GetFileName(Key);
                //delete cache file - this synchronizes the cache across servers in the farm
                DeleteCacheFile(f);
            }
        }
		
		#endregion
		
		#region Private Methods

        private static string ByteArrayToString(byte[] arrInput)
        {
            int i;
            var sOutput = new StringBuilder(arrInput.Length);
            for (i = 0; i <= arrInput.Length - 1; i++)
            {
                sOutput.Append(arrInput[i].ToString("X2"));
            }
            return sOutput.ToString();
        }

        private static void CreateCacheFile(string FileName, string CacheKey)
        {
			//declare stream
            StreamWriter s = null;
            try
            {
				//if the cache file does not already exist
                if (!File.Exists(FileName))
                {
					//create the cache file
                    s = File.CreateText(FileName);
                    //write the CacheKey to the file to provide a documented link between cache item and cache file
                    s.Write(CacheKey);
					//close the stream
                }
            }
            catch (Exception ex)
            {
                //permissions issue creating cache file or more than one thread may have been trying to write the cache file simultaneously
                Exceptions.Exceptions.LogException(ex);
            }
            finally
            {
                if (s != null)
                {
                    s.Close();
                }
            }
        }

        private static void DeleteCacheFile(string FileName)
        {
            try
            {
                if (File.Exists(FileName))
                {
                    File.Delete(FileName);
                }
            }
            catch (Exception ex)
            {
                //an error occurred when trying to delete the cache file - this is serious as it means that the cache will not be synchronized
                Exceptions.Exceptions.LogException(ex);
            }
        }

        private static string GetFileName(string CacheKey)
        {
            //cache key may contain characters invalid for a filename - this method creates a valid filename
            byte[] FileNameBytes = Encoding.ASCII.GetBytes(CacheKey);
            using (var sha256 = new SHA256CryptoServiceProvider())
            {
                FileNameBytes = sha256.ComputeHash(FileNameBytes);
                string FinalFileName = ByteArrayToString(FileNameBytes);
                return Path.GetFullPath(Globals.HostMapPath + CachingDirectory + FinalFileName + CacheFileExtension);
            }
        }

        private string PurgeCacheFiles(string Folder)
        {
            //declare counters
            int PurgedFiles = 0;
            int PurgeErrors = 0;
            int i;
			
            //get list of cache files
            string[] f;
            f = Directory.GetFiles(Folder);

            //loop through cache files
            for (i = 0; i <= f.Length - 1; i++)
            {
                //get last write time for file
                DateTime dtLastWrite;
                dtLastWrite = File.GetLastWriteTime(f[i]);
                //if the cache file is more than 2 hours old ( no point in checking most recent cache files )
                if (dtLastWrite < DateTime.Now.Subtract(new TimeSpan(2, 0, 0)))
                {
					//get cachekey
                    string strCacheKey = Path.GetFileNameWithoutExtension(f[i]);
                    //if the cache key does not exist in memory
                    if (DataCache.GetCache(strCacheKey) == null)
                    {
                        try
                        {
							//delete the file
                            File.Delete(f[i]);
                            PurgedFiles += 1;
                        }
                        catch (Exception exc)
                        {
							//an error occurred
                            Logger.Error(exc);

                            PurgeErrors += 1;
                        }
                    }
                }
            }
			
        	//return a summary message for the job
            return string.Format("Cache Synchronization Files Processed: " + f.Length + ", Purged: " + PurgedFiles + ", Errors: " + PurgeErrors);
		}

		#endregion
	}
}
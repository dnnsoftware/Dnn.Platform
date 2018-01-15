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

using System;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Web;
using DotNetNuke.Common;
using DotNetNuke.Instrumentation;

namespace DotNetNuke.Web.Common.Internal
{
    internal static class DotNetNukeShutdownOverload
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(DotNetNukeShutdownOverload));

        private static Timer _shutDownDelayTimer;
        private static bool _handleShutdowns;
        private static bool _shutdownInprogress;
        private static FileSystemWatcher _binOrRootWatcher;
        private static string _binFolder = "";

        internal static void InitializeFcnSettings()
        {
            // any error/message logged below should be informational only
            try
            {
                // ReSharper disable once PossibleNullReferenceException
                var fileChangesMonitor = typeof(HttpRuntime)
                    .GetProperty("FileChangesMonitor", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static)
                    .GetValue(null, null);

                if (fileChangesMonitor == null)
                {
                    Logger.Info("fileChangesMonitor is null");
                    //AddSiteFilesMonitoring(true);
                }
                else
                {
                    // ReSharper disable once PossibleNullReferenceException
                    var fcnVal = fileChangesMonitor.GetType()
                        .GetField("_FCNMode", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.IgnoreCase)
                        .GetValue(fileChangesMonitor);

                    Logger.Info("FCNMode = " + fcnVal + " (Modes: NotSet/Default=0, Disabled=1, Single=2)");

                    var dirMonCompletion = typeof(HttpRuntime).Assembly.GetType("System.Web.DirMonCompletion");
                    var dirMonCount = (int)dirMonCompletion.InvokeMember("_activeDirMonCompletions",
                        BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.GetField,
                        null, null, null);
                    Logger.Trace("DirMonCompletion count: " + dirMonCount);
                    // enable our monitor only when fcnMode="Disabled"
                    //AddSiteFilesMonitoring(fcnVal.ToString() == "1");
                }

                // just monitor the root folder but don't interfere
                AddSiteFilesMonitoring(false);
            }
            catch (Exception e)
            {
                Logger.Info(e);
            }
        }

        private static void AddSiteFilesMonitoring(bool handleShutdowns)
        {
            if (_binOrRootWatcher == null)
            {
                lock (typeof(Initialize))
                {
                    if (_binOrRootWatcher == null)
                    {
                        try
                        {
                            _handleShutdowns = handleShutdowns;
                            if (_handleShutdowns)
                                _shutDownDelayTimer = new Timer(InitiateShutdown);

                            _binFolder = Path.Combine(Globals.ApplicationMapPath, "bin").ToLower();
                            _binOrRootWatcher = new FileSystemWatcher
                            {
                                Filter = "*.*",
                                Path = handleShutdowns ? _binFolder : Globals.ApplicationMapPath,
                                NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName,
                                IncludeSubdirectories = true,
                            };

                            _binOrRootWatcher.Created += WatcherOnCreated;
                            _binOrRootWatcher.Deleted += WatcherOnDeleted;
                            _binOrRootWatcher.Renamed += WatcherOnRenamed;
                            _binOrRootWatcher.Changed += WatcherOnChanged;
                            _binOrRootWatcher.Error += WatcherOnError;

                            // begin watching;
                            _binOrRootWatcher.EnableRaisingEvents = true;
                            Logger.Trace("Added watcher for: " + _binOrRootWatcher.Path + "\\" + _binOrRootWatcher.Filter);
                        }
                        catch (Exception ex)
                        {
                            Logger.Trace("Error adding our own file monitoring object. " + ex);
                        }
                    }
                }
            }
        }

        private static void InitiateShutdown(object state)
        {
            if (!_handleShutdowns) return;
            try
            {
                HttpRuntime.UnloadAppDomain();
            }
            catch (Exception ex)
            {
                _shutdownInprogress = false;
                Logger.Error(ex);
            }
        }

        private static void ShceduleShutdown()
        {
            // no need for locking; worst case is timer extended a bit more
            if (_handleShutdowns && !_shutdownInprogress)
            {
                _shutdownInprogress = true;
                // delay for a very short period
                _shutDownDelayTimer.Change(1500, Timeout.Infinite);
            }
        }

        private static void WatcherOnChanged(object sender, FileSystemEventArgs e)
        {
            if (Logger.IsInfoEnabled && !e.FullPath.EndsWith(".log.resources"))
                Logger.Info($"Watcher Activity: {e.ChangeType}. Path: {e.FullPath}");

            if (_handleShutdowns && !_shutdownInprogress && (e.FullPath ?? "").ToLower().StartsWith(_binFolder))
            {
                ShceduleShutdown();
            }
        }

        private static void WatcherOnCreated(object sender, FileSystemEventArgs e)
        {
            if (Logger.IsInfoEnabled && !e.FullPath.EndsWith(".log.resources"))
                Logger.Info($"Watcher Activity: {e.ChangeType}. Path: {e.FullPath}");

            if (_handleShutdowns && !_shutdownInprogress && (e.FullPath ?? "").ToLower().StartsWith(_binFolder))
                ShceduleShutdown();
        }

        private static void WatcherOnRenamed(object sender, RenamedEventArgs e)
        {
            if (Logger.IsInfoEnabled && !e.FullPath.EndsWith(".log.resources"))
                Logger.Info($"Watcher Activity: {e.ChangeType}. New Path: {e.FullPath}. Old Path: {e.OldFullPath}");

            if (_handleShutdowns && !_shutdownInprogress && (e.FullPath ?? "").ToLower().StartsWith(_binFolder))
                ShceduleShutdown();
        }

        private static void WatcherOnDeleted(object sender, FileSystemEventArgs e)
        {
            if (Logger.IsInfoEnabled && !e.FullPath.EndsWith(".log.resources"))
                Logger.Info($"Watcher Activity: {e.ChangeType}. Path: {e.FullPath}");

            if (_handleShutdowns && !_shutdownInprogress && (e.FullPath ?? "").ToLower().StartsWith(_binFolder))
                ShceduleShutdown();
        }

        private static void WatcherOnError(object sender, ErrorEventArgs e)
        {
            if (Logger.IsInfoEnabled)
                Logger.Info("Watcher Activity: N/A. Error: " + e.GetException());
        }
    }
}
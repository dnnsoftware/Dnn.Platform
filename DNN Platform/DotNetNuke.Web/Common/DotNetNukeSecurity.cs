// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

#if false
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using DotNetNuke.Application;
using DotNetNuke.Common;
using DotNetNuke.Entities.Controllers;
using DotNetNuke.Entities.Host;
using DotNetNuke.Instrumentation;
using DotNetNuke.Services.Localization;
using DotNetNuke.Services.Log.EventLog;
using DotNetNuke.Services.Mail;

// ReSharper disable once CheckNamespace
namespace DotNetNuke.Web.Common.Internal
{
    internal static class DotNetNukeSecurity
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(DotNetNukeSecurity));
        private static FileSystemWatcher _fileWatcher;
        private static DateTime _lastRead;
        private static Globals.UpgradeStatus _appStatus = Globals.UpgradeStatus.None;
        private static IEnumerable<string> _settingsRestrictExtensions = new string[] { };
        private static Queue<string> _filesQ;
        private static Timer _qTimer;

        // used to indicate already send first real-time email within last SlidingDelay period
        private static int _notificationSent;

        private const int CacheTimeOut = 5; //obtain the setting and do calculations once every 5 minutes at most, plus no need for locking
        private const int SlidingDelay = 30 * 1000; // milliseconds

        // Source: Configuring Blocked File Extensions
        // https://msdn.microsoft.com/en-us/library/cc767397.aspx
        private static readonly IEnumerable<string> DefaultRestrictExtensions =
            (
                ".ade,.adp,.app,.ashx,.asmx,.asp,.aspx,.bas,.bat,.chm,.class,.cmd,.com,.cpl,.crt,.dll,.exe," +
                ".fxp,.hlp,.hta,.ins,.isp,.jse,.lnk,.mda,.mdb,.mde,.mdt,.mdw,.mdz,.msc,.msi,.msp,.mst,.ops,.pcd,.php," +
                ".pif,.prf,.prg,.py,.reg,.scf,.scr,.sct,.shb,.shs,.url,.vb,.vbe,.vbs,.wsc,.wsf,.wsh"
            )
            .ToLowerInvariant()
            .Split(',')
            .Where(e => !string.IsNullOrEmpty(e))
            .Select(e => e.Trim())
            .ToList();

        internal static void Initialize()
        {
            if (_fileWatcher == null)
            {
                lock (typeof(DotNetNukeSecurity))
                {
                    if (_fileWatcher == null)
                    {
                        try
                        {
                            InitializeFileWatcher();
                        }
                        catch (Exception e)
                        {
                            Logger.Error(e);
                        }
                    }
                }
            }
        }


        private static void InitializeFileWatcher()
        {
            _fileWatcher = new FileSystemWatcher
            {
                Filter = "*.*",
                Path = Globals.ApplicationMapPath,
                NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName,
                IncludeSubdirectories = true,
            };

            _fileWatcher.Created += WatcherOnCreated;
            _fileWatcher.Renamed += WatcherOnRenamed;
            _fileWatcher.Error += WatcherOnError;

            _filesQ = new Queue<string>();
            _qTimer = new Timer(QTimerCallBack);

            AppDomain.CurrentDomain.DomainUnload += (sender, args) =>
            {
                _fileWatcher.Dispose();
                QTimerCallBack(null);
            };

            _fileWatcher.EnableRaisingEvents = true;
        }

        private static void QTimerCallBack(object obj)
        {
            try
            {
                string[] items;
                lock (_filesQ)
                {
                    while (!_qTimer.Change(Timeout.Infinite, Timeout.Infinite)) { }
                    Interlocked.Exchange(ref _notificationSent, 0);
                    items = _filesQ.ToArray();
                    while (_filesQ.Count > 0) _filesQ.Dequeue();
                }

                if (items.Length > 0)
                    NotifyManager(items);
            }
            catch (Exception ex)
            {
                LogException(ex);
            }
        }

        private static void WatcherOnRenamed(object sender, RenamedEventArgs e)
        {
            CheckFile(e.FullPath);
        }

        private static void WatcherOnCreated(object sender, FileSystemEventArgs e)
        {
            CheckFile(e.FullPath);
        }

        private static void WatcherOnError(object sender, ErrorEventArgs e)
        {
            LogException(e.GetException());
        }

        private static void LogException(Exception ex)
        {
            Logger.Error("Watcher Activity Error: " + ex?.Message);
        }

        private static void CheckFile(string path)
        {
            try
            {
                if (IsRestrictdExtension(path))
                {
                    if (_appStatus != Globals.UpgradeStatus.Install && _appStatus != Globals.UpgradeStatus.Upgrade)
                    {
                        Globals.UpgradeStatus appStatus;

                        try
                        {
                            appStatus = Globals.Status;
                        }
                        catch (NullReferenceException)
                        {
                            appStatus = Globals.UpgradeStatus.None;
                        }

                        // make status sticky; once set to install/upgrade, it stays so until finishing & appl restarts
                        if (appStatus == Globals.UpgradeStatus.Install || appStatus == Globals.UpgradeStatus.Upgrade)
                        {
                            _appStatus = appStatus;
                        }
                        else
                        {
                            ThreadPool.QueueUserWorkItem(_ => AddEventLog(path));
                            var val = Interlocked.Increment(ref _notificationSent);
                            if (val <= 1)
                            {
                                // first notification goes immediately
                                ThreadPool.QueueUserWorkItem(_ => NotifyManager(new[] { path }));
                            }
                            else
                            {
                                lock (_filesQ)
                                {
                                    while (!_qTimer.Change(val >= 100 ? 1 : SlidingDelay, Timeout.Infinite)) { }
                                    _filesQ.Enqueue(path);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogException(ex);
            }
        }

        private static bool IsRestrictdExtension(string path)
        {
            var extension = Path.GetExtension(path)?.ToLowerInvariant();
            return !string.IsNullOrEmpty(extension) &&
                GetRestrictExtensions().Contains(extension);
        }

        private static IEnumerable<string> GetRestrictExtensions()
        {
            // obtain the setting and do calculations once every 5 minutes at most, plus no need for locking
            if ((DateTime.Now - _lastRead).TotalMinutes > CacheTimeOut)
            {
                _lastRead = DateTime.Now;
                var settings = HostController.Instance.GetString("SA_RestrictExtensions", string.Empty);
                _settingsRestrictExtensions = string.IsNullOrEmpty(settings)
                    ? DefaultRestrictExtensions
                    : settings.ToLowerInvariant()
                        .Split(',')
                        .Where(e => !string.IsNullOrEmpty(e))
                        .Select(e => e.Trim())
                        .Concat(DefaultRestrictExtensions)
                        .ToList();
            }

            return _settingsRestrictExtensions ?? DefaultRestrictExtensions;
        }

        private static void AddEventLog(string path)
        {
            try
            {
                var log = new LogInfo
                {
                    LogTypeKey = EventLogController.EventLogType.HOST_ALERT.ToString(),
                };
                log.AddProperty("Summary", Localization.GetString("PotentialDangerousFile.Text"));
                log.AddProperty("File Name", path);

                new LogController().AddLog(log);
            }
            catch (Exception ex)
            {
                LogException(ex);
            }
        }

        private static void NotifyManager(string[] paths)
        {
            try
            {
                var pathNames = string.Join("<br/>", paths);
                var subject = Localization.GetString("RestrictFileMail_Subject.Text");
                var body = Localization.GetString("RestrictFileMail_Body.Text")
                    .Replace("[Path]", pathNames)
                    .Replace("[AppName]", Host.HostTitle)
                    .Replace("[AppVersion]", DotNetNukeContext.Current.Application.CurrentVersion.ToString());

                Mail.SendEmail(Host.HostEmail, Host.HostEmail, subject, body);
            }
            catch (Exception ex)
            {
                LogException(ex);
            }
        }

    }
}
#endif

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

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DotNetNuke.Application;
using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Data;
using DotNetNuke.Entities.Controllers;
using DotNetNuke.Entities.Host;
using DotNetNuke.Instrumentation;
using DotNetNuke.Services.Installer.Blocker;
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
        private static IEnumerable<string> _settingsRestrictExtensions = new string[] { };
        private static Task _pendingTask;
        private static IDictionary<int, bool> _taskStatus = new Dictionary<int, bool>();

        private const int CacheTimeOut = 5; //obtain the setting and do calculations once every 5 minutes at most, plus no need for locking
        private const int InstallProcessingTime = 300; // do not start the watcher process in 5 minutes after database install complete.
        private const int WaitingTime = 60; //let thread wait for 60s to check again.
        private const int UpgradeIndicateWaitingTime = 180; //seconds to wait for indicate whether current changing is in upgrade process.

        // Source: Configuring Blocked File Extensions
        // https://msdn.microsoft.com/en-us/library/cc767397.aspx
        private static readonly IEnumerable<string> DefaultRestrictExtensions =
            (
                ".ade,.adp,.app,.ashx,.asmx,.asp,.aspx,.bas,.bat,.chm,.class,.cmd,.com,.cpl,.crt,.exe," +
                ".fxp,.hlp,.hta,.ins,.isp,.jse,.lnk,.mda,.mdb,.mde,.mdt,.mdw,.mdz,.msc,.msi,.msp,.mst,.ops,.pcd,.php," +
                ".pif,.prf,.prg,.py,.reg,.scf,.scr,.sct,.shb,.shs,.url,.vb,.vbe,.vbs,.wsc,.wsf,.wsh"
            )
            .ToLowerInvariant()
            .Split(',')
            .Where(e => !string.IsNullOrEmpty(e))
            .Select(e => e.Trim())
            .ToList();

        private static readonly IEnumerable<string> ProtectedPaths = new List<string>
        {
            "Default.aspx",
            "ErrorPage.aspx",
            "KeepAlive.aspx",
            "admin\\Sales\\paypalipn.aspx",
            "admin\\Sales\\paypalsubscription.aspx",
            "Install\\Install.aspx",
            "Install\\InstallWizard.aspx",
            "Install\\UpgradeWizard.aspx",
            "Portals\\_default\\subhost.aspx"
        };

        private static readonly IEnumerable<string> UpgradeIndicateFiles = new List<string>
        {
            "bin\\DotNetNuke.dll"
        };

        private static IList<string> _pendingNotifyPaths = new List<string>(); 

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
                            Task.Run(() =>
                            {
                                InitializeFileWatcher();
                            });
                        }
                        catch (Exception e)
                        {
                            Logger.Error(e);
                        }
                    }
                }
            }
        }

        #region File Watcher Functions

        private static void InitializeFileWatcher()
        {
            if (InInstallOrUpgradeProcess())
            {
                Thread.Sleep(WaitingTime * 1000);
                InitializeFileWatcher();

                return;
            }

            _fileWatcher = new FileSystemWatcher
            {
                Filter = "*.*",
                Path = Globals.ApplicationMapPath,
                NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName,
                IncludeSubdirectories = true,
            };

            _fileWatcher.Created += WatcherOnCreated;
            _fileWatcher.Changed += _fileWatcher_Changed;
            _fileWatcher.Renamed += WatcherOnRenamed;
            _fileWatcher.Error += WatcherOnError;

            AppDomain.CurrentDomain.DomainUnload += (sender, args) =>
            {
                _fileWatcher.Dispose();
            };

            _fileWatcher.EnableRaisingEvents = true;
        }

        private static void _fileWatcher_Changed(object sender, FileSystemEventArgs e)
        {
            CheckFile(e.FullPath);
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
            Logger.Error("Watcher Activity: N/A. Error: " + ex?.Message);
        }

        private static void CheckFile(string path)
        {
            try
            {
                //if the path is upgrade indicate file, then stop the watcher, because app will restart to run upgrade process.
                if (IsUpgradeIndicateFile(path))
                {
                    _fileWatcher?.Dispose();
                    _pendingNotifyPaths.Clear();
                    CancelCurrentPendingTask();

                    return;
                }

                if (IsRestrictdExtension(path))
                {
                    if (IsProtectedFile(path))
                    {
                        if (!_pendingNotifyPaths.Contains(path))
                        {
                            _pendingNotifyPaths.Add(path);
                            StartPendingFileTask();
                        }
                    }
                    else
                    {
                        ThreadPool.QueueUserWorkItem(_ => AddEventLog(new List<string> { path }));
                        ThreadPool.QueueUserWorkItem(_ => NotifyManager(new List<string> { path }));
                    }
                }
            }
            catch (Exception ex)
            {
                LogException(ex);
            }
        }

        private static void StartPendingFileTask()
        {
            CancelCurrentPendingTask();

            _pendingTask = Task.Factory.StartNew(() =>
            {
                Thread.Sleep(UpgradeIndicateWaitingTime * 1000);

                var taskId = Task.CurrentId.GetValueOrDefault(Null.NullInteger);
                if (_taskStatus.ContainsKey(taskId) && !_taskStatus[taskId])
                {
                    var files = _pendingNotifyPaths.ToArray();
                    ThreadPool.QueueUserWorkItem(_ => AddEventLog(files));
                    ThreadPool.QueueUserWorkItem(_ => NotifyManager(files));

                    _pendingNotifyPaths.Clear();
                }
            });

            if (_taskStatus.ContainsKey(_pendingTask.Id))
            {
                _taskStatus.Remove(_pendingTask.Id);
            }

            _taskStatus.Add(_pendingTask.Id, false);
        }

        private static void CancelCurrentPendingTask()
        {
            if (_pendingTask != null && _taskStatus.ContainsKey(_pendingTask.Id))
            {
                _taskStatus[_pendingTask.Id] = true;
            }
        }

        private static bool IsProtectedFile(string path)
        {
            return ProtectedPaths.Any(p =>
            {
                var protectedPath = Path.Combine(Globals.ApplicationMapPath, p);
                var comparePath = path.Replace("/", "\\");

                return protectedPath.Equals(comparePath, StringComparison.InvariantCultureIgnoreCase);
            });
        }

        private static bool IsUpgradeIndicateFile(string path)
        {
            return UpgradeIndicateFiles.Any(p =>
            {
                var protectedPath = Path.Combine(Globals.ApplicationMapPath, p);
                var comparePath = path.Replace("/", "\\");

                return protectedPath.Equals(comparePath, StringComparison.InvariantCultureIgnoreCase);
            });
        }

        private static bool IsRestrictdExtension(string path)
        {
            var extension = Path.GetExtension(path)?.ToLowerInvariant();
            return !string.IsNullOrEmpty(extension) &&
                GetRestrictExtensions().Contains(extension);
        }

        private static IEnumerable<string> GetRestrictExtensions()
        {
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

        private static void AddEventLog(IEnumerable<string> paths)
        {
            try
            {
                var log = new LogInfo
                {
                    LogTypeKey = EventLogController.EventLogType.HOST_ALERT.ToString(),
                };
                log.AddProperty("Summary", Localization.GetString("PotentialDangerousFile.Text"));
                foreach (var path in paths)
                {
                    log.AddProperty("File Name", path);
                }

                new LogController().AddLog(log);
            }
            catch (Exception ex)
            {
                LogException(ex);
            }
        }

        private static void NotifyManager(IEnumerable<string> paths)
        {
            try
            {
                var path = string.Join("", paths.Select(p => "<div>" + p + "</div>"));
                var subject = Localization.GetString("RestrictFileMail_Subject.Text");
                var body = Localization.GetString("RestrictFileMail_Body.Text")
                    .Replace("[Path]", path)
                    .Replace("[AppName]", Host.HostTitle)
                    .Replace("[AppVersion]", DotNetNukeContext.Current.Application.CurrentVersion.ToString());

                Mail.SendEmail(Host.HostEmail, Host.HostEmail, subject, body);
            }
            catch (Exception ex)
            {
                LogException(ex);
            }
        }

        private static bool InInstallOrUpgradeProcess()
        {
            var status = Globals.Status;

            if (status != Globals.UpgradeStatus.None || InstallBlocker.Instance.IsInstallInProgress())
            {
                return true;
            }

            return (DateTime.Now - GetCheckTime()).TotalSeconds < InstallProcessingTime;
        }

        private static DateTime GetCheckTime()
        {
            var checkTime = DateTime.Now;
            try
            {
                using (var reader = DataProvider.Instance().ExecuteSQL("SELECT MAX([CreatedDate]) AS CreatedDate FROM {databaseOwner}[{objectQualifier}Version]"))
                {
                    if (reader.Read())
                    {
                        checkTime = Null.SetNullDateTime(reader["CreatedDate"]); ;
                    }
                    reader.Close();
                }
            }
            catch (Exception)
            {
                //do nothing
            }

            return checkTime;
        }

        #endregion
    }
}
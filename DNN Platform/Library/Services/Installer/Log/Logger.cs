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
using System.Collections.Generic;
using System.Web.UI.HtmlControls;

using DotNetNuke.Common.Utilities;
using DotNetNuke.Instrumentation;

#endregion

namespace DotNetNuke.Services.Installer.Log
{
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The Logger class provides an Installer Log
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// -----------------------------------------------------------------------------
    public class Logger
    {
    	private static readonly ILog DnnLogger = LoggerSource.Instance.GetLogger(typeof (Logger));
        private readonly IList<LogEntry> _logs;
        private string _errorClass;
        private bool _hasWarnings;
        private string _highlightClass;
        private string _normalClass;
        private bool _valid;

        public Logger()
        {
            _logs = new List<LogEntry>();
            
            _valid = true;
            _hasWarnings = Null.NullBoolean;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets and sets the Css Class used for Error Log Entries
        /// </summary>
        /// <value>A String</value>
        /// -----------------------------------------------------------------------------
        public string ErrorClass
        {
            get
            {
                if (String.IsNullOrEmpty(_errorClass))
                {
                    _errorClass = "NormalRed";
                }
                return _errorClass;
            }
            set
            {
                _errorClass = value;
            }
        }

        public bool HasWarnings
        {
            get
            {
                return _hasWarnings;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets and sets the Css Class used for Log Entries that should be highlighted
        /// </summary>
        /// <value>A String</value>
        /// -----------------------------------------------------------------------------
        public string HighlightClass
        {
            get
            {
                if (String.IsNullOrEmpty(_highlightClass))
                {
                    _highlightClass = "NormalBold";
                }
                return _highlightClass;
            }
            set
            {
                _highlightClass = value;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets a List of Log Entries
        /// </summary>
        /// <value>A List of LogEntrys</value>
        /// -----------------------------------------------------------------------------
        public IList<LogEntry> Logs
        {
            get
            {
                return _logs;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets and sets the Css Class used for normal Log Entries
        /// </summary>
        /// <value>A String</value>
        /// -----------------------------------------------------------------------------
        public string NormalClass
        {
            get
            {
                if (String.IsNullOrEmpty(_normalClass))
                {
                    _normalClass = "Normal";
                }
                return _normalClass;
            }
            set
            {
                _normalClass = value;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets a Flag that indicates whether the Installation was Valid
        /// </summary>
        /// <value>A List of LogEntrys</value>
        /// -----------------------------------------------------------------------------
        public bool Valid
        {
            get
            {
                return _valid;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// The AddFailure method adds a new LogEntry of type Failure to the Logs collection
        /// </summary>
        /// <remarks>This method also sets the Valid flag to false</remarks>
        /// <param name="failure">The description of the LogEntry</param>
        /// -----------------------------------------------------------------------------
        public void AddFailure(string failure)
        {
            _logs.Add(new LogEntry(LogType.Failure, failure));
            DnnLogger.Error(failure);
            _valid = false;
        }

        public void AddFailure(Exception ex)
        {
            AddFailure((Util.EXCEPTION + ex));
            Exceptions.Exceptions.LogException(ex);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// The AddInfo method adds a new LogEntry of type Info to the Logs collection
        /// </summary>
        /// <param name="info">The description of the LogEntry</param>
        /// -----------------------------------------------------------------------------
        public void AddInfo(string info)
        {            
            _logs.Add(new LogEntry(LogType.Info, info));
			DnnLogger.Info(info);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// The AddWarning method adds a new LogEntry of type Warning to the Logs collection
        /// </summary>
        /// <param name="warning">The description of the LogEntry</param>
        /// -----------------------------------------------------------------------------
        public void AddWarning(string warning)
        {
            _logs.Add(new LogEntry(LogType.Warning, warning));
			DnnLogger.Warn(warning);
            _hasWarnings = true;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// The EndJob method adds a new LogEntry of type EndJob to the Logs collection
        /// </summary>
        /// <param name="job">The description of the LogEntry</param>
        /// -----------------------------------------------------------------------------
        public void EndJob(string job)
        {
            _logs.Add(new LogEntry(LogType.EndJob, job));
            DnnLogger.Info(job);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// GetLogsTable formats log entries in an HtmlTable
        /// </summary>
        /// -----------------------------------------------------------------------------
        public HtmlTable GetLogsTable()
        {
            var arrayTable = new HtmlTable();
            foreach (LogEntry entry in Logs)
            {
                var tr = new HtmlTableRow();
                var tdType = new HtmlTableCell();
                tdType.InnerText = Util.GetLocalizedString("LOG.PALogger." + entry.Type);
                var tdDescription = new HtmlTableCell();
                tdDescription.InnerText = entry.Description;
                tr.Cells.Add(tdType);
                tr.Cells.Add(tdDescription);
                switch (entry.Type)
                {
                    case LogType.Failure:
                    case LogType.Warning:
                        tdType.Attributes.Add("class", ErrorClass);
                        tdDescription.Attributes.Add("class", ErrorClass);
                        break;
                    case LogType.StartJob:
                    case LogType.EndJob:
                        tdType.Attributes.Add("class", HighlightClass);
                        tdDescription.Attributes.Add("class", HighlightClass);
                        break;
                    case LogType.Info:
                        tdType.Attributes.Add("class", NormalClass);
                        tdDescription.Attributes.Add("class", NormalClass);
                        break;
                }
                arrayTable.Rows.Add(tr);
                if (entry.Type == LogType.EndJob)
                {
                    var spaceTR = new HtmlTableRow();
                    spaceTR.Cells.Add(new HtmlTableCell {ColSpan = 2, InnerHtml = "&nbsp;"});
                    arrayTable.Rows.Add(spaceTR);
                }
            }
            return arrayTable;
        }

        public void ResetFlags()
        {
            _valid = true;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// The StartJob method adds a new LogEntry of type StartJob to the Logs collection
        /// </summary>
        /// <param name="job">The description of the LogEntry</param>
        /// -----------------------------------------------------------------------------
        public void StartJob(string job)
        {
            _logs.Add(new LogEntry(LogType.StartJob, job));
            DnnLogger.Info(job);
        }
    }
}
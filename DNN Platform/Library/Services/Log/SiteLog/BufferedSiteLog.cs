#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2014
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
using System.Collections;

using DotNetNuke.Data;
using DotNetNuke.Instrumentation;

#endregion

namespace DotNetNuke.Services.Log.SiteLog
{
    public class BufferedSiteLog
    {
    	private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof (BufferedSiteLog));
        public ArrayList SiteLog;
        public string SiteLogStorage;

        public void AddSiteLog()
        {
            try
            {
                SiteLogInfo objSiteLog;
                var objSiteLogs = new SiteLogController();

				//iterate through buffered sitelog items and insert into database
                int intIndex;
                for (intIndex = 0; intIndex <= SiteLog.Count - 1; intIndex++)
                {
                    objSiteLog = (SiteLogInfo) SiteLog[intIndex];
                    switch (SiteLogStorage)
                    {
                        case "D": //database
                            DataProvider.Instance().AddSiteLog(objSiteLog.DateTime,
                                                               objSiteLog.PortalId,
                                                               objSiteLog.UserId,
                                                               objSiteLog.Referrer,
                                                               objSiteLog.URL,
                                                               objSiteLog.UserAgent,
                                                               objSiteLog.UserHostAddress,
                                                               objSiteLog.UserHostName,
                                                               objSiteLog.TabId,
                                                               objSiteLog.AffiliateId);
                            break;
                        case "F": //file system
                            objSiteLogs.W3CExtendedLog(objSiteLog.DateTime,
                                                       objSiteLog.PortalId,
                                                       objSiteLog.UserId,
                                                       objSiteLog.Referrer,
                                                       objSiteLog.URL,
                                                       objSiteLog.UserAgent,
                                                       objSiteLog.UserHostAddress,
                                                       objSiteLog.UserHostName,
                                                       objSiteLog.TabId,
                                                       objSiteLog.AffiliateId);
                            break;
                    }
                }
            }
            catch (Exception exc)
            {
                Logger.Error(exc);

            }
        }
    }
}
#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2013
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
using System.ComponentModel.Composition;
using DotNetNuke.Common;
using DotNetNuke.ComponentModel;
using DotNetNuke.ExtensionPoints;
using DotNetNuke.Instrumentation;

namespace DotNetNuke.Services.FileSystem
{
    internal class FileEventHandlersContainer : ComponentBase<IFileEventHandlersContainer, FileEventHandlersContainer>, IFileEventHandlersContainer
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(FileEventHandlersContainer));

        [ImportMany]
        private IEnumerable<Lazy<IFileEventHandlers>> eventsHandlers = new List<Lazy<IFileEventHandlers>>();

        public FileEventHandlersContainer()
        {
            try
            {
                if (GetCurrentStatus() != Globals.UpgradeStatus.None)
                {
                    return;
                }
                ExtensionPointManager.ComposeParts(this);             
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
            }
        }

        public IEnumerable<Lazy<IFileEventHandlers>> FileEventsHandlers
        {
            get
            {
                return eventsHandlers;
            }
        }

        private Globals.UpgradeStatus GetCurrentStatus()
        {
            try
            {
                return Globals.Status;
            }
            catch (NullReferenceException)
            {
                return Globals.UpgradeStatus.Unknown;
            }
        }
    }
}

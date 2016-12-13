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

using DotNetNuke.Services.Localization;

#endregion

namespace Dnn.Modules.Dashboard.Components
{
    [Serializable]
    public class DashboardControl : IComparable
    {
        private string _ControllerClass;
        private int _DashboardControlID;
        private string _DashboardControlKey;
        private string _DashboardControlLocalResources;
        private string _DashboardControlSrc;
        private bool _IsDirty;
        private bool _IsEnabled;
        private int _PackageID;
        private int _ViewOrder;

        public string ControllerClass
        {
            get
            {
                return _ControllerClass;
            }
            set
            {
                _ControllerClass = value;
            }
        }

        public int DashboardControlID
        {
            get
            {
                return _DashboardControlID;
            }
            set
            {
                _DashboardControlID = value;
            }
        }

        public string DashboardControlKey
        {
            get
            {
                return _DashboardControlKey;
            }
            set
            {
                _DashboardControlKey = value;
            }
        }

        public string DashboardControlLocalResources
        {
            get
            {
                return _DashboardControlLocalResources;
            }
            set
            {
                _DashboardControlLocalResources = value;
            }
        }

        public string DashboardControlSrc
        {
            get
            {
                return _DashboardControlSrc;
            }
            set
            {
                _DashboardControlSrc = value;
            }
        }

        public bool IsDirty
        {
            get
            {
                return _IsDirty;
            }
        }

        public bool IsEnabled
        {
            get
            {
                return _IsEnabled;
            }
            set
            {
                if (_IsEnabled != value)
                {
                    _IsDirty = true;
                }
                _IsEnabled = value;
            }
        }

        public string LocalizedTitle
        {
            get
            {
                return Localization.GetString(DashboardControlKey + ".Title", "~/" + DashboardControlLocalResources);
            }
        }

        public int PackageID
        {
            get
            {
                return _PackageID;
            }
            set
            {
                _PackageID = value;
            }
        }

        public int ViewOrder
        {
            get
            {
                return _ViewOrder;
            }
            set
            {
                if (_ViewOrder != value)
                {
                    _IsDirty = true;
                }
                _ViewOrder = value;
            }
        }

        #region IComparable Members

        public int CompareTo(object obj)
        {
            var dashboardControl = obj as DashboardControl;
            if (dashboardControl == null)
            {
                throw new ArgumentException("object is not a DashboardControl");
            }
            return ViewOrder.CompareTo(dashboardControl.ViewOrder);
        }

        #endregion
    }
}
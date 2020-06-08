// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
#region Usings

using System;

#endregion

namespace DotNetNuke.Services.Analytics.Config
{
    [Serializable]
    public class AnalyticsRule
    {
        private string _label;
        private int _roleId;
        private int _tabId;

        public int RoleId
        {
            get
            {
                return _roleId;
            }
            set
            {
                _roleId = value;
            }
        }

        public int TabId
        {
            get
            {
                return _tabId;
            }
            set
            {
                _tabId = value;
            }
        }

        public string Label
        {
            get
            {
                return _label;
            }
            set
            {
                _label = value;
            }
        }

        public string Value { get; set; }
    }
}

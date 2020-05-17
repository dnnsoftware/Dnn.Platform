﻿// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
#region Usings

using System;
using System.Text.RegularExpressions;
using DotNetNuke.Common.Utilities;

#endregion

namespace DotNetNuke.Entities.Urls.Config
{
    [Serializable]
    public class RewriterRule
    {
       [NonSerialized]
       private Regex _matchRx;

        private string _lookFor;
        private string _sendTo;

        public string LookFor
        {
            get
            {
                return _lookFor;
            }
            set
            {
                if (_lookFor != value)
                {
                    _lookFor = value;
                    _matchRx = null;
                }
            }
        }

        public string SendTo
        {
            get
            {
                return _sendTo;
            }
            set
            {
                _sendTo = value;
            }
        }

        //HACK: we cache this in the first call assuming applicationPath never changes during the whole lifetime of the application
        // also don't worry about locking; the worst case this will be created more than once
        public Regex GetRuleRegex(string applicationPath)
        {
            return _matchRx ?? (_matchRx =
                RegexUtils.GetCachedRegex("^" + RewriterUtils.ResolveUrl(applicationPath, LookFor) + "$",
                    RegexOptions.IgnoreCase | RegexOptions.CultureInvariant));
        }
    }
}

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

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

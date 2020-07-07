// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Entities.Urls.Config
{
    using System;
    using System.Text.RegularExpressions;

    using DotNetNuke.Common.Utilities;

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
                return this._lookFor;
            }

            set
            {
                if (this._lookFor != value)
                {
                    this._lookFor = value;
                    this._matchRx = null;
                }
            }
        }

        public string SendTo
        {
            get
            {
                return this._sendTo;
            }

            set
            {
                this._sendTo = value;
            }
        }

        // HACK: we cache this in the first call assuming applicationPath never changes during the whole lifetime of the application
        // also don't worry about locking; the worst case this will be created more than once
        public Regex GetRuleRegex(string applicationPath)
        {
            return this._matchRx ?? (this._matchRx =
                RegexUtils.GetCachedRegex(
                    "^" + RewriterUtils.ResolveUrl(applicationPath, this.LookFor) + "$",
                    RegexOptions.IgnoreCase | RegexOptions.CultureInvariant));
        }
    }
}

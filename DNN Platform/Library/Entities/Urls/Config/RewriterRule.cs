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
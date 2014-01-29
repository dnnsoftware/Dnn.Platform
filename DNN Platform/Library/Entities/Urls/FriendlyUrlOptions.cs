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
using System.Collections.Generic;

#endregion

namespace DotNetNuke.Entities.Urls
{
    /// <summary>
    /// This class encapsulates different options used in generating friendly urls
    /// </summary>
    [Serializable]
    public class FriendlyUrlOptions
    {
        public bool ConvertDiacriticChars;
        public string IllegalChars;
        public int MaxUrlPathLength;
        public string PageExtension;
        public string PunctuationReplacement;
        //922 : change to use regexMatch pattern for allowable characters
        public string RegexMatch;
        public Dictionary<string, string> ReplaceCharWithChar = new Dictionary<string, string>();
        public string ReplaceChars;
        public bool ReplaceDoubleChars;
        public string SpaceEncoding;

        public bool CanGenerateNonStandardPath
        {
            //replaces statements like this
            //if ((settings.ReplaceSpaceWith != null && settings.ReplaceSpaceWith.Length > 0) || settings.ReplaceCharWithCharDict != null && settings.ReplaceCharWithCharDict.Count > 0)
            get
            {
                bool result = false;
                if (string.IsNullOrEmpty(PunctuationReplacement) == false)
                {
                    result = true;
                }
                else if (ReplaceCharWithChar != null && ReplaceCharWithChar.Count > 0)
                {
                    result = true;
                }
                else if (ConvertDiacriticChars)
                {
                    result = true;
                }

                return result;
            }
        }

        public FriendlyUrlOptions Clone()
        {
            var cloned = new FriendlyUrlOptions
                {
                    PunctuationReplacement = PunctuationReplacement,
                    SpaceEncoding = SpaceEncoding,
                    MaxUrlPathLength = MaxUrlPathLength,
                    ConvertDiacriticChars = ConvertDiacriticChars,
                    PageExtension = PageExtension,
                    RegexMatch = RegexMatch,
                    ReplaceCharWithChar = ReplaceCharWithChar,
                    IllegalChars = IllegalChars,
                    ReplaceChars = ReplaceChars
                };
            return cloned;
        }
    }
}
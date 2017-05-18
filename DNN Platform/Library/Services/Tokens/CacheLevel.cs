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

namespace DotNetNuke.Services.Tokens
{
    /// <summary>
    /// CacheLevel is used to specify the cachability of a string, determined as minimum of the used token cachability
    /// </summary>
    /// <remarks>
    /// CacheLevel is determined as minimum of the used tokens' cachability 
    /// </remarks>
    public enum CacheLevel : byte
    {
        /// <summary>
        /// Caching of the text is not suitable and might expose security risks
        /// </summary>
        notCacheable = 0,
        /// <summary>
        /// Caching of the text might result in inaccurate display (e.g. time), but does not expose a security risk
        /// </summary>
        secureforCaching = 5,
        /// <summary>
        /// Caching of the text can be done without limitations or any risk
        /// </summary>
        fullyCacheable = 10
    }
}
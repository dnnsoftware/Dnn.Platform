#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2018
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

namespace Dnn.PersonaBar.Servers.Components.DatabaseServer
{
    public class DbFileInfo
    {
        public string FileType { get; set; }

        public string Name { get; set; }

        public long Size { get; set; }

        public decimal Megabytes => Convert.ToDecimal(Size/1024);

        public string FileName { get; set; }

        public string ShortFileName
        {
            get
            {
                if(FileName.IndexOf('\\') == FileName.LastIndexOf('\\'))
                {
                    return FileName;
                }

                return string.Format("{0}...{1}", FileName.Substring(0, FileName.IndexOf('\\') + 1), FileName.Substring(FileName.LastIndexOf('\\', FileName.LastIndexOf('\\') - 1)));
            }
        }
    }
}
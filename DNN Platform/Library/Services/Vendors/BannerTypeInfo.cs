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

#endregion

namespace DotNetNuke.Services.Vendors
{
    public enum BannerType
    {
        Banner = 1,
        MicroButton = 2,
        Button = 3,
        Block = 4,
        Skyscraper = 5,
        Text = 6,
        Script = 7
    }

    [Serializable]
    public class BannerTypeInfo
    {
        private int _BannerTypeId;
        private string _BannerTypeName;

        public BannerTypeInfo()
        {
        }

        public BannerTypeInfo(int BannerTypeId, string BannerTypeName)
        {
            _BannerTypeId = BannerTypeId;
            _BannerTypeName = BannerTypeName;
        }

        public int BannerTypeId
        {
            get
            {
                return _BannerTypeId;
            }
            set
            {
                _BannerTypeId = value;
            }
        }

        public string BannerTypeName
        {
            get
            {
                return _BannerTypeName;
            }
            set
            {
                _BannerTypeName = value;
            }
        }
    }
}
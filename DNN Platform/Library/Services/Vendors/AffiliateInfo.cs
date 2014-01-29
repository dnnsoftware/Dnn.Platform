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
    [Serializable]
    public class AffiliateInfo
    {
        private int _Acquisitions;
        private int _AffiliateId;
        private double _CPA;
        private double _CPATotal;
        private double _CPC;
        private double _CPCTotal;
        private int _Clicks;
        private DateTime _EndDate;
        private DateTime _StartDate;
        private int _VendorId;

        public int AffiliateId
        {
            get
            {
                return _AffiliateId;
            }
            set
            {
                _AffiliateId = value;
            }
        }

        public int VendorId
        {
            get
            {
                return _VendorId;
            }
            set
            {
                _VendorId = value;
            }
        }

        public DateTime StartDate
        {
            get
            {
                return _StartDate;
            }
            set
            {
                _StartDate = value;
            }
        }

        public DateTime EndDate
        {
            get
            {
                return _EndDate;
            }
            set
            {
                _EndDate = value;
            }
        }

        public double CPC
        {
            get
            {
                return _CPC;
            }
            set
            {
                _CPC = value;
            }
        }

        public int Clicks
        {
            get
            {
                return _Clicks;
            }
            set
            {
                _Clicks = value;
            }
        }

        public double CPCTotal
        {
            get
            {
                return _CPCTotal;
            }
            set
            {
                _CPCTotal = value;
            }
        }

        public double CPA
        {
            get
            {
                return _CPA;
            }
            set
            {
                _CPA = value;
            }
        }

        public int Acquisitions
        {
            get
            {
                return _Acquisitions;
            }
            set
            {
                _Acquisitions = value;
            }
        }

        public double CPATotal
        {
            get
            {
                return _CPATotal;
            }
            set
            {
                _CPATotal = value;
            }
        }
    }
}
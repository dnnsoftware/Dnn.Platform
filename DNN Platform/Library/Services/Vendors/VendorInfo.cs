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
    public class VendorInfo
    {
        public int VendorId { get; set; }

        public string VendorName { get; set; }

        public string Street { get; set; }

        public string City { get; set; }

        public string Region { get; set; }

        public string Country { get; set; }

        public string PostalCode { get; set; }

        public string Telephone { get; set; }

        public int PortalId { get; set; }

        public string Fax { get; set; }

        public string Cell { get; set; }

        public string Email { get; set; }

        public string Website { get; set; }

        public int ClickThroughs { get; set; }

        public int Views { get; set; }

        public string CreatedByUser { get; set; }

        public DateTime CreatedDate { get; set; }

        public string LogoFile { get; set; }

        public string KeyWords { get; set; }

        public string Unit { get; set; }

        public bool Authorized { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public int Banners { get; set; }

        public string UserName { get; set; }
    }
}
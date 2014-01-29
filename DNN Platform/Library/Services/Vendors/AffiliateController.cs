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

using System.Collections;

using DotNetNuke.Common.Utilities;
using DotNetNuke.Data;

#endregion

namespace DotNetNuke.Services.Vendors
{
    public class AffiliateController
    {
        public ArrayList GetAffiliates(int VendorId)
        {
            return CBO.FillCollection(DataProvider.Instance().GetAffiliates(VendorId), typeof (AffiliateInfo));
        }

        public AffiliateInfo GetAffiliate(int AffiliateId, int VendorId, int PortalID)
        {
            return (AffiliateInfo) CBO.FillObject(DataProvider.Instance().GetAffiliate(AffiliateId, VendorId, PortalID), typeof (AffiliateInfo));
        }

        public void DeleteAffiliate(int AffiliateId)
        {
            DataProvider.Instance().DeleteAffiliate(AffiliateId);
        }

        public void AddAffiliate(AffiliateInfo objAffiliate)
        {
            DataProvider.Instance().AddAffiliate(objAffiliate.VendorId, objAffiliate.StartDate, objAffiliate.EndDate, objAffiliate.CPC, objAffiliate.CPA);
        }

        public void UpdateAffiliate(AffiliateInfo objAffiliate)
        {
            DataProvider.Instance().UpdateAffiliate(objAffiliate.AffiliateId, objAffiliate.StartDate, objAffiliate.EndDate, objAffiliate.CPC, objAffiliate.CPA);
        }

        public void UpdateAffiliateStats(int AffiliateId, int Clicks, int Acquisitions)
        {
            DataProvider.Instance().UpdateAffiliateStats(AffiliateId, Clicks, Acquisitions);
        }
    }
}
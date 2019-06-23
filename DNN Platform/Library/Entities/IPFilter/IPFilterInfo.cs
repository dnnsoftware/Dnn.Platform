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
#region Usings

using System;
using System.Data;
using System.Xml.Serialization;

using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Entities.Users;
using DotNetNuke.Instrumentation;
using DotNetNuke.Security.Roles;

#endregion

namespace DotNetNuke.Entities.Host
{

    [Serializable]
    public class IPFilterInfo : BaseEntityInfo, IHydratable
    {

        #region Constructors

        /// <summary>
        /// Create new IPFilterInfo instance
        /// </summary>
        /// <remarks>
        /// </remarks>
        public IPFilterInfo(string ipAddress, string subnetMask, int ruleType)
        {
            IPAddress = ipAddress;
            SubnetMask = subnetMask;
            RuleType = ruleType;
        }

        public IPFilterInfo()
        {
            IPAddress = String.Empty;
            SubnetMask = String.Empty;
            RuleType = -1;
        }

        #endregion        	

        #region Auto_Properties

        public int IPFilterID { get; set; }

        public string IPAddress { get; set; }

        public string SubnetMask { get; set; }

        public int RuleType { get; set; }

        #endregion

        
        #region IHydratable Members

        /// <summary>
        /// Fills an IPFilterInfo from a Data Reader
        /// </summary>
        /// <param name="dr">The Data Reader to use</param>
        /// <remarks>Standard IHydratable.Fill implementation
        /// <seealso cref="KeyID"></seealso></remarks>
        public void Fill(IDataReader dr)
        {
            IPFilterID = Null.SetNullInteger(dr["IPFilterID"]);

            try
            {
                IPFilterID = Null.SetNullInteger(dr["IPFilterID"]);
            }
            catch (IndexOutOfRangeException)
            {
            
                //else swallow the error
            }

            IPAddress = Null.SetNullString(dr["IPAddress"]);
            SubnetMask = Null.SetNullString(dr["SubnetMask"]);
            RuleType = Null.SetNullInteger(dr["RuleType"]);
            
            FillInternal(dr);
        }

        /// <summary>
        /// Gets and sets the Key ID
        /// </summary>
        /// <returns>KeyId of the IHydratable.Key</returns>
        /// <remarks><seealso cref="Fill"></seealso></remarks>
        public int KeyID
        {
            get
            {
                return IPFilterID;
            }
            set
            {
                IPFilterID = value;
            }
        }

        #endregion
    }
}

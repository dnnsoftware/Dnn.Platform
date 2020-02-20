// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
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

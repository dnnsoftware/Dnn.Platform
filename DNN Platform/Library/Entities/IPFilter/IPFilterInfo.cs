// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

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
using DotNetNuke.Security.Roles.Internal;

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
            this.IPAddress = ipAddress;
            this.SubnetMask = subnetMask;
            this.RuleType = ruleType;
        }

        public IPFilterInfo()
        {
            this.IPAddress = String.Empty;
            this.SubnetMask = String.Empty;
            this.RuleType = -1;
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
            this.IPFilterID = Null.SetNullInteger(dr["IPFilterID"]);

            try
            {
                this.IPFilterID = Null.SetNullInteger(dr["IPFilterID"]);
            }
            catch (IndexOutOfRangeException)
            {

                // else swallow the error
            }

            this.IPAddress = Null.SetNullString(dr["IPAddress"]);
            this.SubnetMask = Null.SetNullString(dr["SubnetMask"]);
            this.RuleType = Null.SetNullInteger(dr["RuleType"]);

            this.FillInternal(dr);
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
                return this.IPFilterID;
            }
            set
            {
                this.IPFilterID = value;
            }
        }

        #endregion
    }
}

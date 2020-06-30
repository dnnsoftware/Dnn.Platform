// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Entities
{
    using System;
    using System.ComponentModel;
    using System.Data;
    using System.Xml.Serialization;

    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Users;

    /// -----------------------------------------------------------------------------
    /// Project  : DotNetNuke
    /// Namespace: DotNetNuke.Entities
    /// Class    : BaseEntityInfo
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// BaseEntityInfo provides auditing fields for Core tables.
    /// </summary>
    /// -----------------------------------------------------------------------------
    [Serializable]
    public abstract class BaseEntityInfo
    {
        protected BaseEntityInfo()
        {
            this.CreatedByUserID = Null.NullInteger;
            this.LastModifiedByUserID = Null.NullInteger;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the CreatedByUserID.
        /// </summary>
        /// <returns>An Integer.</returns>
        /// -----------------------------------------------------------------------------
        [Browsable(false)]
        [XmlIgnore]
        public int CreatedByUserID { get; internal set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the CreatedOnDate.
        /// </summary>
        /// <returns>A DateTime.</returns>
        /// -----------------------------------------------------------------------------
        [Browsable(false)]
        [XmlIgnore]
        public DateTime CreatedOnDate { get; private set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the LastModifiedByUserID.
        /// </summary>
        /// <returns>An Integer.</returns>
        /// -----------------------------------------------------------------------------
        [Browsable(false)]
        [XmlIgnore]
        public int LastModifiedByUserID { get; internal set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the LastModifiedOnDate.
        /// </summary>
        /// <returns>A DateTime.</returns>
        /// -----------------------------------------------------------------------------
        [Browsable(false)]
        [XmlIgnore]
        public DateTime LastModifiedOnDate { get; private set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the UserInfo object associated with this user.
        /// </summary>
        /// <param name="portalId">The PortalID associated with the desired user.</param>
        /// <returns>A UserInfo object.</returns>
        /// -----------------------------------------------------------------------------
        public UserInfo CreatedByUser(int portalId)
        {
            if (this.CreatedByUserID > Null.NullInteger)
            {
                UserInfo user = UserController.GetUserById(portalId, this.CreatedByUserID);
                return user;
            }

            return null;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the UserInfo object associated with this user.
        /// </summary>
        /// <param name="portalId">The PortalID associated with the desired user.</param>
        /// <returns>A UserInfo object.</returns>
        /// -----------------------------------------------------------------------------
        public UserInfo LastModifiedByUser(int portalId)
        {
            if (this.LastModifiedByUserID > Null.NullInteger)
            {
                UserInfo user = UserController.GetUserById(portalId, this.LastModifiedByUserID);
                return user;
            }

            return null;
        }

        /// <summary>
        /// method used by cbo to fill readonly properties ignored by HydrateObject reflection.
        /// </summary>
        /// <param name="dr">the data reader to use.</param>
        /// <remarks></remarks>
        internal void FillBaseProperties(IDataReader dr)
        {
            this.FillInternal(dr);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Fills a BaseEntityInfo from a Data Reader.
        /// </summary>
        /// <param name="dr">The Data Reader to use.</param>
        /// -----------------------------------------------------------------------------
        protected virtual void FillInternal(IDataReader dr)
        {
            this.CreatedByUserID = Null.SetNullInteger(dr["CreatedByUserID"]);
            this.CreatedOnDate = Null.SetNullDateTime(dr["CreatedOnDate"]);
            this.LastModifiedByUserID = Null.SetNullInteger(dr["LastModifiedByUserID"]);
            this.LastModifiedOnDate = Null.SetNullDateTime(dr["LastModifiedOnDate"]);
        }

        protected void CloneBaseProperties(BaseEntityInfo clonedItem, BaseEntityInfo originalItem)
        {
            clonedItem.CreatedByUserID = originalItem.CreatedByUserID;
            clonedItem.CreatedOnDate = originalItem.CreatedOnDate;
            clonedItem.LastModifiedByUserID = originalItem.LastModifiedByUserID;
            clonedItem.LastModifiedOnDate = originalItem.LastModifiedOnDate;
        }
    }
}

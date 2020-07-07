// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Common.Utilities
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;

    /// -----------------------------------------------------------------------------
    /// Project:    DotNetNuke
    /// Namespace:  DotNetNuke.Common.Utilities
    /// Class:      ObjectMappingInfo
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The ObjectMappingInfo class is a helper class that holds the mapping information
    /// for a particular type.  This information is in two parts:
    ///     - Information about the Database Table that the object is mapped to
    ///     - Information about how the object is cached.
    /// For each object, when it is first accessed, reflection is used on the class and
    /// an instance of ObjectMappingInfo is created, which is cached for performance.
    /// </summary>
    /// -----------------------------------------------------------------------------
    [Serializable]
    public class ObjectMappingInfo
    {
        private const string RootCacheKey = "ObjectCache_";
        private readonly Dictionary<string, string> _ColumnNames;
        private readonly Dictionary<string, PropertyInfo> _Properties;
        private string _CacheByProperty;
        private int _CacheTimeOutMultiplier;
        private string _ObjectType;
        private string _PrimaryKey;
        private string _TableName;

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Initializes a new instance of the <see cref="ObjectMappingInfo"/> class.
        /// Constructs a new ObjectMappingInfo Object.
        /// </summary>
        /// -----------------------------------------------------------------------------
        public ObjectMappingInfo()
        {
            this._Properties = new Dictionary<string, PropertyInfo>();
            this._ColumnNames = new Dictionary<string, string>();
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets cacheKey gets the root value of the key used to identify the cached collection
        /// in the ASP.NET Cache.
        /// </summary>
        /// -----------------------------------------------------------------------------
        public string CacheKey
        {
            get
            {
                string _CacheKey = RootCacheKey + this.TableName + "_";
                if (!string.IsNullOrEmpty(this.CacheByProperty))
                {
                    _CacheKey += this.CacheByProperty + "_";
                }

                return _CacheKey;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets columnNames gets a dictionary of Database Column Names for the Object.
        /// </summary>
        /// -----------------------------------------------------------------------------
        public Dictionary<string, string> ColumnNames
        {
            get
            {
                return this._ColumnNames;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets properties gets a dictionary of Properties for the Object.
        /// </summary>
        /// -----------------------------------------------------------------------------
        public Dictionary<string, PropertyInfo> Properties
        {
            get
            {
                return this._Properties;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets cacheByProperty gets and sets the property that is used to cache collections
        /// of the object.  For example: Modules are cached by the "TabId" proeprty.  Tabs
        /// are cached by the PortalId property.
        /// </summary>
        /// <remarks>If empty, a collection of all the instances of the object is cached.</remarks>
        /// -----------------------------------------------------------------------------
        public string CacheByProperty
        {
            get
            {
                return this._CacheByProperty;
            }

            set
            {
                this._CacheByProperty = value;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets cacheTimeOutMultiplier gets and sets the multiplier used to determine how long
        /// the cached collection should be cached.  It is multiplied by the Performance
        /// Setting - which in turn can be modified by the Host Account.
        /// </summary>
        /// <remarks>Defaults to 20.</remarks>
        /// -----------------------------------------------------------------------------
        public int CacheTimeOutMultiplier
        {
            get
            {
                return this._CacheTimeOutMultiplier;
            }

            set
            {
                this._CacheTimeOutMultiplier = value;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets objectType gets and sets the type of the object.
        /// </summary>
        /// -----------------------------------------------------------------------------
        public string ObjectType
        {
            get
            {
                return this._ObjectType;
            }

            set
            {
                this._ObjectType = value;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets primaryKey gets and sets the property of the object that corresponds to the
        /// primary key in the database.
        /// </summary>
        /// -----------------------------------------------------------------------------
        public string PrimaryKey
        {
            get
            {
                return this._PrimaryKey;
            }

            set
            {
                this._PrimaryKey = value;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets tableName gets and sets the name of the database table that is used to
        /// persist the object.
        /// </summary>
        /// -----------------------------------------------------------------------------
        public string TableName
        {
            get
            {
                return this._TableName;
            }

            set
            {
                this._TableName = value;
            }
        }
    }
}

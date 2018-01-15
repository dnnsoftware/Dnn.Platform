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
using System.Collections.Generic;
using System.Reflection;

#endregion

namespace DotNetNuke.Common.Utilities
{
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

        ///-----------------------------------------------------------------------------
        /// <summary>
        /// Constructs a new ObjectMappingInfo Object
        /// </summary>
        ///-----------------------------------------------------------------------------
        public ObjectMappingInfo()
        {
            _Properties = new Dictionary<string, PropertyInfo>();
            _ColumnNames = new Dictionary<string, string>();
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// CacheKey gets the root value of the key used to identify the cached collection 
        /// in the ASP.NET Cache.
        /// </summary>
        /// -----------------------------------------------------------------------------
        public string CacheKey
        {
            get
            {
                string _CacheKey = RootCacheKey + TableName + "_";
                if (!string.IsNullOrEmpty(CacheByProperty))
                {
                    _CacheKey += CacheByProperty + "_";
                }
                return _CacheKey;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// CacheByProperty gets and sets the property that is used to cache collections
        /// of the object.  For example: Modules are cached by the "TabId" proeprty.  Tabs 
        /// are cached by the PortalId property.
        /// </summary>
        /// <remarks>If empty, a collection of all the instances of the object is cached.</remarks>
        /// -----------------------------------------------------------------------------
        public string CacheByProperty
        {
            get
            {
                return _CacheByProperty;
            }
            set
            {
                _CacheByProperty = value;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// CacheTimeOutMultiplier gets and sets the multiplier used to determine how long
        /// the cached collection should be cached.  It is multiplied by the Performance
        /// Setting - which in turn can be modified by the Host Account.
        /// </summary>
        /// <remarks>Defaults to 20.</remarks>
        /// -----------------------------------------------------------------------------
        public int CacheTimeOutMultiplier
        {
            get
            {
                return _CacheTimeOutMultiplier;
            }
            set
            {
                _CacheTimeOutMultiplier = value;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// ColumnNames gets a dictionary of Database Column Names for the Object
        /// </summary>
        /// -----------------------------------------------------------------------------
        public Dictionary<string, string> ColumnNames
        {
            get
            {
                return _ColumnNames;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// ObjectType gets and sets the type of the object
        /// </summary>
        /// -----------------------------------------------------------------------------
        public string ObjectType
        {
            get
            {
                return _ObjectType;
            }
            set
            {
                _ObjectType = value;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// PrimaryKey gets and sets the property of the object that corresponds to the
        /// primary key in the database
        /// </summary>
        /// -----------------------------------------------------------------------------
        public string PrimaryKey
        {
            get
            {
                return _PrimaryKey;
            }
            set
            {
                _PrimaryKey = value;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Properties gets a dictionary of Properties for the Object
        /// </summary>
        /// -----------------------------------------------------------------------------
        public Dictionary<string, PropertyInfo> Properties
        {
            get
            {
                return _Properties;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// TableName gets and sets the name of the database table that is used to
        /// persist the object.
        /// </summary>
        /// -----------------------------------------------------------------------------
        public string TableName
        {
            get
            {
                return _TableName;
            }
            set
            {
                _TableName = value;
            }
        }
    }
}

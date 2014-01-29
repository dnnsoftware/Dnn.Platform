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
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Serialization;

using DotNetNuke.Entities;
using DotNetNuke.Entities.Host;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.Services.Scheduling;

#endregion

namespace DotNetNuke.Common.Utilities
{
    /// -----------------------------------------------------------------------------
    /// Project:    DotNetNuke
    /// Namespace:  DotNetNuke.Common.Utilities
    /// Class:      CBO
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The CBO class generates objects.
    /// </summary>
    /// <history>
    ///     [cnurse]	12/01/2007	Documented
    /// </history>
    /// -----------------------------------------------------------------------------
    public class CBO
    {
		#region Private Constants
		
        private const string defaultPrimaryKey = "ItemID";

        private const string objectMapCacheKey = "ObjectMap_";

		#endregion

		#region Private Shared Methods

		#region Object Creation/Hydration Helper Methods

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// CreateObjectFromReader creates an object of a specified type from the
        /// provided DataReader
        /// </summary>
        /// <param name="objType">The type of the Object</param>
        /// <param name="dr">The IDataReader to use to fill the object</param>
        /// <param name="closeReader">A flag that indicates whether the DataReader should be closed</param>
        /// <returns>The object (TObject)</returns>
        /// <history>
        /// 	[cnurse]	11/30/2007	Created
        /// </history>
        /// -----------------------------------------------------------------------------
        private static object CreateObjectFromReader(Type objType, IDataReader dr, bool closeReader)
        {
            object objObject = null;
            bool isSuccess = Null.NullBoolean;
            bool canRead = true;

            if (closeReader)
            {
                canRead = false;
                //read datareader
                if (dr.Read())
                {
                    canRead = true;
                }
            }
            try
            {
                if (canRead)
                {
					//Create the Object
                    objObject = CreateObject(objType, false);

                    //hydrate the custom business object
                    FillObjectFromReader(objObject, dr);
                }
                isSuccess = true;
            }
            finally
            {
				//Ensure DataReader is closed
                if ((!isSuccess))
                {
                    closeReader = true;
                }
                CloseDataReader(dr, closeReader);
            }
            return objObject;
        }

		/// -----------------------------------------------------------------------------
		/// <summary>
		/// FillDictionaryFromReader fills a dictionary of objects of a specified type
		/// from a DataReader.
		/// </summary>
		/// <typeparam name="TKey">The type of the key</typeparam>
		/// <typeparam name="TValue">The type of the value</typeparam>
		/// <param name="keyField">The key field for the object.  This is used as
		/// the key in the Dictionary.</param>
		/// <param name="dr">The IDataReader to use to fill the objects</param>
		/// <param name="objDictionary">The Dictionary to fill.</param>
		/// <param name="closeReader">Whether close the data reader when operation complete.</param>
		/// <returns>A Dictionary of objects (T)</returns>
		/// <history>
		/// 	[cnurse]	11/30/2007	Created
		/// </history>
		/// -----------------------------------------------------------------------------
        private static IDictionary<TKey, TValue> FillDictionaryFromReader<TKey, TValue>(string keyField, IDataReader dr,
                                                                                        IDictionary<TKey, TValue>
                                                                                            objDictionary,
                                                                                        bool closeReader)
        {
            TValue objObject;
            TKey keyValue = default(TKey);
            bool isSuccess = Null.NullBoolean;

            try
            {
                //iterate datareader
                while (dr.Read())
                {
					//Create the Object
                    objObject = (TValue) CreateObjectFromReader(typeof (TValue), dr, false);
                    if (keyField == "KeyID" && objObject is IHydratable)
                    {
						//Get the value of the key field from the KeyID
                        keyValue = (TKey) Null.SetNull(((IHydratable) objObject).KeyID, keyValue);
                    }
                    else
                    {
						//Get the value of the key field from the DataReader
                        if (typeof (TKey).Name == "Int32" && dr[keyField].GetType().Name == "Decimal")
                        {
                            keyValue = (TKey) Convert.ChangeType(Null.SetNull(dr[keyField], keyValue), typeof (TKey));
                        }
                        else if (typeof (TKey).Name.ToLower() == "string" &&
                                 dr[keyField].GetType().Name.ToLower() == "dbnull")
                        {
                            keyValue = (TKey) Convert.ChangeType(Null.SetNull(dr[keyField], ""), typeof (TKey));
                        }
                        else
                        {
                            keyValue = (TKey) Convert.ChangeType(Null.SetNull(dr[keyField], ""), typeof (TKey));
                        }
                    }
					//add to dictionary
                    if (objObject != null)
                    {
                        objDictionary[keyValue] = objObject;
                    }
                }
                isSuccess = true;
            }
            finally
            {
				//Ensure DataReader is closed
                if ((!isSuccess))
                {
                    closeReader = true;
                }
                CloseDataReader(dr, closeReader);
            }
			
            //Return the dictionary
            return objDictionary;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// FillListFromReader fills a list of objects of a specified type 
        /// from a DataReader
        /// </summary>
        /// <param name="objType">The type of the business object</param>
        /// <param name="dr">The IDataReader to use to fill the objects</param>
        /// <param name="objList">The List to Fill</param>
        /// <param name="closeReader">A flag that indicates whether the DataReader should be closed</param>
        /// <returns>A List of objects (TItem)</returns>
        /// <remarks></remarks>
        /// <history>
        /// 	[cnurse]	11/30/2007	Created
        /// </history>
        /// -----------------------------------------------------------------------------
        private static IList FillListFromReader(Type objType, IDataReader dr, IList objList, bool closeReader)
        {
            object objObject;
            bool isSuccess = Null.NullBoolean;
            try
            {
                //iterate datareader
                while (dr.Read())
                {
					//Create the Object
                    objObject = CreateObjectFromReader(objType, dr, false);
					//add to collection
                    objList.Add(objObject);
                }
                isSuccess = true;
            }
            finally
            {
				//Ensure DataReader is closed
                if ((!isSuccess))
                {
                    closeReader = true;
                }
                CloseDataReader(dr, closeReader);
            }
            return objList;
        }

		/// -----------------------------------------------------------------------------
		/// <summary>
		/// FillListFromReader fills a list of objects of a specified type
		/// from a DataReader
		/// </summary>
		/// <param name="dr">The IDataReader to use to fill the objects</param>
		/// <param name="objList">The List to Fill</param>
		/// <param name="closeReader">A flag that indicates whether the DataReader should be closed</param>
		/// <returns>A List of objects (TItem)</returns>
		/// <remarks></remarks>
		/// <history>
		/// 	[cnurse]	11/30/2007	Created
		/// </history>
		/// -----------------------------------------------------------------------------
        private static IList<TItem> FillListFromReader<TItem>(IDataReader dr, IList<TItem> objList, bool closeReader)
        {
            TItem objObject;
            bool isSuccess = Null.NullBoolean;
            try
            {
				//iterate datareader
                while (dr.Read())
                {
					//Create the Object
                    objObject = (TItem) CreateObjectFromReader(typeof (TItem), dr, false);
					//add to collection
                    objList.Add(objObject);
                }
                isSuccess = true;
            }
            finally
            {
				//Ensure DataReader is closed
                if ((!isSuccess))
                {
                    closeReader = true;
                }
                CloseDataReader(dr, closeReader);
            }
            return objList;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// FillObjectFromReader fills an object from the provided DataReader.  If the object 
        /// implements the IHydratable interface it will use the object's IHydratable.Fill() method.
        /// Otherwise, it will use reflection to fill the object.
        /// </summary>
        /// <param name="objObject">The object to fill</param>
        /// <param name="dr">The DataReader</param>
        /// <history>
        /// 	[cnurse]	11/30/2007	Created
        /// </history>
        /// -----------------------------------------------------------------------------
        private static void FillObjectFromReader(object objObject, IDataReader dr)
        {
            try
            {
				//Determine if object is IHydratable
                if (objObject is IHydratable)
                {
					//Use IHydratable's Fill
                    var objHydratable = objObject as IHydratable;
                    if (objHydratable != null)
                    {
                        objHydratable.Fill(dr);
                    }
                }
                else
                {
					//Use Reflection
                    HydrateObject(objObject, dr);
                }
            }
            catch (IndexOutOfRangeException iex)
            {
				//Call to GetOrdinal is being made with a bad column name
                if (Host.ThrowCBOExceptions)
                {
                    throw new ObjectHydrationException("Error Reading DataReader", iex, objObject.GetType(), dr);
                }
                else
                {
                    Exceptions.LogException(iex);
                }
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// HydrateObject uses reflection to hydrate an object.
        /// </summary>
        /// <param name="hydratedObject">The object to Hydrate</param>
        /// <param name="dr">The IDataReader that contains the columns of data for the object</param>
        /// <history>
        /// 	[cnurse]	11/29/2007	Created
        /// </history>
        /// -----------------------------------------------------------------------------
        private static void HydrateObject(object hydratedObject, IDataReader dr)
        {
            PropertyInfo objPropertyInfo = null;
            Type propType = null;
            object coloumnValue;
            Type objDataType;
            int intIndex;
            //get cached object mapping for type
            ObjectMappingInfo objMappingInfo = GetObjectMapping(hydratedObject.GetType());
            if (hydratedObject is BaseEntityInfo && !(hydratedObject is ScheduleItem))
            {
                //Call the base classes fill method to populate base class properties
                ((BaseEntityInfo) hydratedObject).FillBaseProperties(dr);
            }
            //fill object with values from datareader
            for (intIndex = 0; intIndex <= dr.FieldCount - 1; intIndex++)
            {
				//If the Column matches a Property in the Object Map's PropertyInfo Dictionary
                if (objMappingInfo.Properties.TryGetValue(dr.GetName(intIndex).ToUpperInvariant(), out objPropertyInfo))
                {
					//Get its type
                    propType = objPropertyInfo.PropertyType;
                    //If property can be set
                    if (objPropertyInfo.CanWrite)
                    {
						//Get the Data Value from the data reader
                        coloumnValue = dr.GetValue(intIndex);
                        //Get the Data Value's type
                        objDataType = coloumnValue.GetType();
                        if (coloumnValue == null || coloumnValue == DBNull.Value)
                        {
                            //set property value to Null
                            objPropertyInfo.SetValue(hydratedObject, Null.SetNull(objPropertyInfo), null);
                        }
                        else if (propType.Equals(objDataType))
                        {
							//Property and data objects are the same type
                            objPropertyInfo.SetValue(hydratedObject, coloumnValue, null);
                        }
                        else
                        {
							//business object info class member data type does not match datareader member data type
							//need to handle enumeration conversions differently than other base types
                            if (propType.BaseType.Equals(typeof (Enum)))
                            {
								//check if value is numeric and if not convert to integer ( supports databases like Oracle )
                                if (Regex.IsMatch(coloumnValue.ToString(), "^\\d+$"))
                                {
                                    objPropertyInfo.SetValue(hydratedObject,
                                                             Enum.ToObject(propType, Convert.ToInt32(coloumnValue)),
                                                             null);
                                }
                                else
                                {
                                    objPropertyInfo.SetValue(hydratedObject, Enum.ToObject(propType, coloumnValue), null);
                                }
                            }
                            else if (propType == typeof (Guid))
                            {
								//guid is not a datatype common across all databases ( ie. Oracle )
                                objPropertyInfo.SetValue(hydratedObject,
                                                         Convert.ChangeType(new Guid(coloumnValue.ToString()), propType),
                                                         null);
                            }
                            else if (propType == typeof (Version))
                            {
                                objPropertyInfo.SetValue(hydratedObject, new Version(coloumnValue.ToString()), null);
                            }
                            else if (coloumnValue is IConvertible)
                            {
                                objPropertyInfo.SetValue(hydratedObject, ChangeType(coloumnValue, propType), null);
                            }
                            else
                            {
                                // try explicit conversion
                                objPropertyInfo.SetValue(hydratedObject, coloumnValue, null);
                            }
                        }
                    }
                }
            }
        }

         /// <summary>
         /// Changes type of an object, taking into account Nullable types
         /// </summary>
         /// <param name="obj"></param>
         /// <param name="type"></param>
         /// <returns></returns>
         static object ChangeType(object obj, Type type)
         {
             Type u = Nullable.GetUnderlyingType(type);
 
             if (u != null) 
             {
                 if (obj == null)
                 {
                     return GetDefault(type);
                 }
 
                 return Convert.ChangeType(obj, u);
             }
             return Convert.ChangeType(obj, type);
         }
 
         /// <summary>
         /// Returns default value for a type - i.e. null for reference types and default value for value types
         /// </summary>
         /// <param name="type"></param>
         /// <returns></returns>
         static object GetDefault(Type type)
         {
             if (type.IsValueType)
             {
                 return Activator.CreateInstance(type);
             }
 
             return null;
         }
 
		#endregion

		#region Object Mapping Helper Methods

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// GetColumnName gets the name of the Database Column that maps to the property.
        /// </summary>
        /// <param name="objProperty">The proeprty of the business object</param>
        /// <returns>The name of the Database Column</returns>
        /// <history>
        /// 	[cnurse]	12/02/2007	Created
        /// </history>
        /// -----------------------------------------------------------------------------
        private static string GetColumnName(PropertyInfo objProperty)
        {
            string columnName = objProperty.Name;
            return columnName;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// GetObjectMapping gets an instance of the ObjectMappingInfo class for the type.
        /// This is cached using a high priority as reflection is expensive.
        /// </summary>
        /// <param name="objType">The type of the business object</param>
        /// <returns>An ObjectMappingInfo object representing the mapping for the object</returns>
        /// <history>
        /// 	[cnurse]	12/01/2007	Created
        /// </history>
        /// -----------------------------------------------------------------------------
        private static ObjectMappingInfo GetObjectMapping(Type objType)
        {
            string cacheKey = objectMapCacheKey + objType.FullName;
            var objMap = (ObjectMappingInfo) DataCache.GetCache(cacheKey);
            if (objMap == null)
            {
                //Create an ObjectMappingInfo instance
                objMap = new ObjectMappingInfo();
                objMap.ObjectType = objType.FullName;
                //Reflect on class to create Object Map
                objMap.PrimaryKey = GetPrimaryKey(objType);
                objMap.TableName = GetTableName(objType);
                //Iterate through the objects properties and add each one to the ObjectMappingInfo's Properties Dictionary 
                foreach (PropertyInfo objProperty in objType.GetProperties())
                {
                    objMap.Properties.Add(objProperty.Name.ToUpperInvariant(), objProperty);
                    objMap.ColumnNames.Add(objProperty.Name.ToUpperInvariant(), GetColumnName(objProperty));
                }
                //Persist to Cache
                DataCache.SetCache(cacheKey, objMap);
            }
			
            //Return Object Map
            return objMap;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// GetPrimaryKey gets the Primary Key property
        /// </summary>
        /// <param name="objType">The type of the business object</param>
        /// <returns>The name of the Primary Key property</returns>
        /// <history>
        /// 	[cnurse]	12/01/2007	Created
        /// </history>
        /// -----------------------------------------------------------------------------
        private static string GetPrimaryKey(Type objType)
        {
            string primaryKey = defaultPrimaryKey;
            return primaryKey;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// GetTableName gets the name of the Database Table that maps to the object.
        /// </summary>
        /// <param name="objType">The type of the business object</param>
        /// <returns>The name of the Database Table</returns>
        /// <history>
        /// 	[cnurse]	11/30/2007	Created
        /// </history>
        /// -----------------------------------------------------------------------------
        private static string GetTableName(Type objType)
        {
            string tableName = string.Empty;
            //If no attrubute then use Type Name
            if (string.IsNullOrEmpty(tableName))
            {
                tableName = objType.Name;
                if (tableName.EndsWith("Info"))
                {
					//Remove Info ending
                    tableName.Replace("Info", string.Empty);
                }
            }
            //Check if there is an object qualifier
            if (!string.IsNullOrEmpty(Config.GetSetting("ObjectQualifier")))
            {
                tableName = Config.GetSetting("ObjectQualifier") + tableName;
            }
            return tableName;
        }
		
		#endregion

		#endregion

		#region Public Shared Methods

		#region Clone Object

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// CloneObject clones an object
        /// </summary>
        /// <param name="objObject">The Object to Clone</param>
        /// <history>
        /// 	[cnurse]	11/29/2007	Created
        /// </history>
        /// -----------------------------------------------------------------------------
        public static object CloneObject(object objObject)
        {
            try
            {
                Type objType = objObject.GetType();
                object objNewObject = Activator.CreateInstance(objType);
                //get cached object mapping for type
                ObjectMappingInfo objMappingInfo = GetObjectMapping(objType);
                foreach (KeyValuePair<string, PropertyInfo> kvp in objMappingInfo.Properties)
                {
                    PropertyInfo objProperty = kvp.Value;
                    if (objProperty.CanWrite)
                    {
                        //Check if property is ICloneable
                        var objPropertyClone = objProperty.GetValue(objObject, null) as ICloneable;
                        if (objPropertyClone == null)
                        {
                            objProperty.SetValue(objNewObject, objProperty.GetValue(objObject, null), null);
                        }
                        else
                        {
                            objProperty.SetValue(objNewObject, objPropertyClone.Clone(), null);
                        }
                        //Check if Property is IEnumerable
                        var enumerable = objProperty.GetValue(objObject, null) as IEnumerable;
                        if (enumerable != null)
                        {
                            var list = objProperty.GetValue(objNewObject, null) as IList;
                            if (list != null)
                            {
                                foreach (object obj in enumerable)
                                {
                                    list.Add(CloneObject(obj));
                                }
                            }
                            var dic = objProperty.GetValue(objNewObject, null) as IDictionary;
                            if (dic != null)
                            {
                                foreach (DictionaryEntry de in enumerable)
                                {
                                    dic.Add(de.Key, CloneObject(de.Value));
                                }
                            }
                        }
                    }
                }
                return objNewObject;
            }
            catch (Exception exc)
            {
                Exceptions.LogException(exc);
                return null;
            }
        }

		#endregion

		#region CloseDataReader

        public static void CloseDataReader(IDataReader dr, bool closeReader)
        {
			//close datareader
            if (dr != null && closeReader)
            {
                dr.Close();
            }
        }

		#endregion

		#region Create Object

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// CreateObject creates a new object of Type TObject.
        /// </summary>
        /// <typeparam name="TObject">The type of object to create.</typeparam>
        /// <remarks>This overload does not initialise the object</remarks>
        /// <history>
        /// 	[cnurse]	11/30/2007	Created
        /// </history>
        /// -----------------------------------------------------------------------------
        public static TObject CreateObject<TObject>()
        {
            return (TObject) CreateObject(typeof (TObject), false);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// CreateObject creates a new object of Type TObject.
        /// </summary>
        /// <typeparam name="TObject">The type of object to create.</typeparam>
        /// <param name="initialise">A flag that indicates whether to initialise the
        /// object.</param>
        /// <history>
        /// 	[cnurse]	11/30/2007	Created
        /// </history>
        /// -----------------------------------------------------------------------------
        public static TObject CreateObject<TObject>(bool initialise)
        {
            return (TObject) CreateObject(typeof (TObject), initialise);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// CreateObject creates a new object.
        /// </summary>
        /// <param name="objType">The type of object to create.</param>
        /// <param name="initialise">A flag that indicates whether to initialise the
        /// object.</param>
        /// <history>
        /// 	[cnurse]	11/30/2007	Created
        /// </history>
        /// -----------------------------------------------------------------------------
        public static object CreateObject(Type objType, bool initialise)
        {
            object objObject = Activator.CreateInstance(objType);

            if (initialise)
            {
                InitializeObject(objObject);
            }
            return objObject;
        }
		
		#endregion

		#region DeserializeObject

        public static TObject DeserializeObject<TObject>(string fileName)
        {
            return DeserializeObject<TObject>(XmlReader.Create(new FileStream(fileName, FileMode.Open, FileAccess.Read)));
        }

        public static TObject DeserializeObject<TObject>(XmlDocument document)
        {
            return DeserializeObject<TObject>(XmlReader.Create(new StringReader(document.OuterXml)));
        }

        public static TObject DeserializeObject<TObject>(Stream stream)
        {
            return DeserializeObject<TObject>(XmlReader.Create(stream));
        }

        public static TObject DeserializeObject<TObject>(TextReader reader)
        {
            return DeserializeObject<TObject>(XmlReader.Create(reader));
        }

        public static TObject DeserializeObject<TObject>(XmlReader reader)
        {
			//First Create the Object
            var objObject = CreateObject<TObject>(true);
            //Try to cast the Object as IXmlSerializable
            var xmlSerializableObject = objObject as IXmlSerializable;
            if (xmlSerializableObject == null)
            {
				//Use XmlSerializer
                var serializer = new XmlSerializer(objObject.GetType());
                objObject = (TObject) serializer.Deserialize(reader);
            }
            else
            {
				//Use XmlReader
                xmlSerializableObject.ReadXml(reader);
            }
            return objObject;
        }
		
		#endregion

		#region FillCollection

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// FillCollection fills a Collection of objects from a DataReader
        /// </summary>
        /// <param name="dr">The Data Reader</param>
        /// <param name="objType">The type of the Object</param>
        /// <history>
        /// 	[cnurse]	11/29/2007	Documented
        /// </history>
        /// -----------------------------------------------------------------------------
        public static ArrayList FillCollection(IDataReader dr, Type objType)
        {
            return (ArrayList) FillListFromReader(objType, dr, new ArrayList(), true);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// FillCollection fills a Collection of objects from a DataReader
        /// </summary>
        /// <param name="dr">The Data Reader</param>
        /// <param name="objType">The type of the Object</param>
        /// <param name="closeReader">Flag that indicates whether the Data Reader should be closed.</param>
        /// <history>
        /// 	[cnurse]	11/29/2007	Documented
        /// </history>
        /// -----------------------------------------------------------------------------
        public static ArrayList FillCollection(IDataReader dr, Type objType, bool closeReader)
        {
            return (ArrayList) FillListFromReader(objType, dr, new ArrayList(), closeReader);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// FillCollection fills a Collection of objects from a DataReader
        /// </summary>
        /// <param name="dr">The Data Reader</param>
        /// <param name="objType">The type of the Object</param>
        /// <param name="objToFill">An IList to fill</param>
        /// <history>
        /// 	[cnurse]	11/29/2007	Documented
        /// </history>
        /// -----------------------------------------------------------------------------
        public static IList FillCollection(IDataReader dr, Type objType, ref IList objToFill)
        {
            return FillListFromReader(objType, dr, objToFill, true);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// FillCollection fills a Collection of objects from a DataReader
        /// </summary>
        /// <typeparam name="TItem">The type of object</typeparam>
        /// <param name="dr">The Data Reader</param>
        /// <history>
        /// 	[cnurse]	11/29/2007	Documented
        /// </history>
        /// -----------------------------------------------------------------------------
        public static List<TItem> FillCollection<TItem>(IDataReader dr)
        {
            return (List<TItem>) FillListFromReader(dr, new List<TItem>(), true);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// FillCollection fills a Collection of objects from a DataReader
        /// </summary>
        /// <typeparam name="TItem">The type of object</typeparam>
        /// <param name="objToFill">The List to fill</param>
        /// <param name="dr">The Data Reader</param>
        /// <history>
        /// 	[cnurse]	11/29/2007	Documented
        /// </history>
        /// -----------------------------------------------------------------------------
        public static IList<TItem> FillCollection<TItem>(IDataReader dr, ref IList<TItem> objToFill)
        {
            return FillListFromReader(dr, objToFill, true);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// FillCollection fills a List of objects from a DataReader
        /// </summary>
        /// <typeparam name="TItem">The type of the Object</typeparam>
        /// <param name="objToFill">The List to fill</param>
        /// <param name="dr">The Data Reader</param>
        /// <param name="closeReader">A flag that indicates whether the DataReader should be closed</param>
        /// <history>
        /// 	[cnurse]	11/29/2007	Created
        /// </history>
        /// -----------------------------------------------------------------------------
        public static IList<TItem> FillCollection<TItem>(IDataReader dr, IList<TItem> objToFill, bool closeReader)
        {
            return FillListFromReader(dr, objToFill, closeReader);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Generic version of FillCollection fills a List custom business object of a specified type 
        /// from the supplied DataReader
        /// </summary>
        /// <param name="dr">The IDataReader to use to fill the object</param>
        /// <param name="objType">The type of the Object</param>
        /// <param name="totalRecords">The total No of records</param>
        /// <returns>A List of custom business objects</returns>
        /// <remarks></remarks>
        /// <history>
        /// 	[cnurse]	01/28/2008	Created
        /// </history>
        /// -----------------------------------------------------------------------------
        public static ArrayList FillCollection(IDataReader dr, ref Type objType, ref int totalRecords)
        {
            var objFillCollection = (ArrayList) FillListFromReader(objType, dr, new ArrayList(), false);
            try
            {
                if (dr.NextResult())
                {
					//Get the total no of records from the second result
                    totalRecords = Globals.GetTotalRecords(ref dr);
                }
            }
            catch (Exception exc)
            {
                Exceptions.LogException(exc);
            }
            finally
            {
				//Ensure DataReader is closed
                CloseDataReader(dr, true);
            }
            return objFillCollection;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Generic version of FillCollection fills a List custom business object of a specified type 
        /// from the supplied DataReader
        /// </summary>
        /// <typeparam name="T">The type of the business object</typeparam>
        /// <param name="dr">The IDataReader to use to fill the object</param>
        /// <param name="totalRecords"></param>
        /// <returns>A List of custom business objects</returns>
        /// <remarks></remarks>
        /// <history>
        /// 	[cnurse]	10/10/2005	Created
        /// </history>
        /// -----------------------------------------------------------------------------
        public static List<T> FillCollection<T>(IDataReader dr, ref int totalRecords)
        {
            IList<T> objFillCollection = FillCollection(dr, new List<T>(), false);
            try
            {
                if (dr.NextResult())
                {
					//Get the total no of records from the second result
                    totalRecords = Globals.GetTotalRecords(ref dr);
                }
            }
            catch (Exception exc)
            {
                Exceptions.LogException(exc);
            }
            finally
            {
				//Ensure DataReader is closed
                CloseDataReader(dr, true);
            }
            return (List<T>) objFillCollection;
        }
		
		#endregion

		#region FillDictionary

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// FillDictionary fills a Dictionary of objects from a DataReader
        /// </summary>
        /// <typeparam name="TItem">The value for the Dictionary Item</typeparam>
        /// <param name="dr">The Data Reader</param>
        /// <history>
        /// 	[cnurse]	11/29/2007	Created
        /// </history>
        /// -----------------------------------------------------------------------------
        public static IDictionary<int, TItem> FillDictionary<TItem>(IDataReader dr) where TItem : IHydratable
        {
            return FillDictionaryFromReader("KeyID", dr, new Dictionary<int, TItem>(), true);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// FillDictionary fills a Dictionary of objects from a DataReader
        /// </summary>
        /// <typeparam name="TItem">The value for the Dictionary Item</typeparam>
        /// <param name="objToFill">The Dictionary to fill</param>
        /// <param name="dr">The Data Reader</param>
        /// <history>
        /// 	[cnurse]	11/29/2007	Created
        /// </history>
        /// -----------------------------------------------------------------------------
        public static IDictionary<int, TItem> FillDictionary<TItem>(IDataReader dr,
                                                                    ref IDictionary<int, TItem> objToFill)
            where TItem : IHydratable
        {
            return FillDictionaryFromReader("KeyID", dr, objToFill, true);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// FillDictionary fills a Dictionary of objects from a DataReader
        /// </summary>
        /// <typeparam name="TKey">The key for the Dictionary</typeparam>
        /// <typeparam name="TValue">The value for the Dictionary Item</typeparam>
        /// <param name="keyField">The key field used for the Key</param>
        /// <param name="dr">The Data Reader</param>
        /// <history>
        /// 	[cnurse]	11/29/2007	Created
        /// </history>
        /// -----------------------------------------------------------------------------
        public static Dictionary<TKey, TValue> FillDictionary<TKey, TValue>(string keyField, IDataReader dr)
        {
            return
                (Dictionary<TKey, TValue>) FillDictionaryFromReader(keyField, dr, new Dictionary<TKey, TValue>(), true);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// FillDictionary fills a Dictionary of objects from a DataReader
        /// </summary>
        /// <typeparam name="TKey">The key for the Dictionary</typeparam>
        /// <typeparam name="TValue">The value for the Dictionary Item</typeparam>
        /// <param name="keyField">The key field used for the Key</param>
        /// <param name="dr">The Data Reader</param>
        /// <param name="closeReader">if true, closes reader. If false, rdr left open</param>
        /// <history>
        /// 	[bchapman]	10/29/2012	Created
        /// </history>
        /// -----------------------------------------------------------------------------
        public static Dictionary<TKey, TValue> FillDictionary<TKey, TValue>(string keyField, IDataReader dr,
                                                                            bool closeReader)
        {
            return
                (Dictionary<TKey, TValue>)
                FillDictionaryFromReader(keyField, dr, new Dictionary<TKey, TValue>(), closeReader);
        }


        /// -----------------------------------------------------------------------------
        /// <summary>
        /// FillDictionary fills a Dictionary of objects from a DataReader
        /// </summary>
        /// <typeparam name="TKey">The key for the Dictionary</typeparam>
        /// <typeparam name="TValue">The value for the Dictionary Item</typeparam>
        /// <param name="keyField">The key field used for the Key</param>
        /// <param name="objDictionary">The Dictionary to fill</param>
        /// <param name="dr">The Data Reader</param>
        /// <history>
        /// 	[cnurse]	11/29/2007	Created
        /// </history>
        /// -----------------------------------------------------------------------------
        public static Dictionary<TKey, TValue> FillDictionary<TKey, TValue>(string keyField, IDataReader dr,
                                                                            IDictionary<TKey, TValue> objDictionary)
        {
            return (Dictionary<TKey, TValue>) FillDictionaryFromReader(keyField, dr, objDictionary, true);
        }
		
		#endregion

		#region FillObject

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// FillObject fills an object from a DataReader
        /// </summary>
        /// <typeparam name="TObject">The type of the object</typeparam>
        /// <param name="dr">The Data Reader</param>
        /// <history>
        /// 	[cnurse]	11/29/2007	Documented
        /// </history>
        /// -----------------------------------------------------------------------------
        public static TObject FillObject<TObject>(IDataReader dr)
        {
            return (TObject) CreateObjectFromReader(typeof (TObject), dr, true);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// FillObject fills an object from a DataReader
        /// </summary>
        /// <typeparam name="TObject">The type of the object</typeparam>
        /// <param name="dr">The Data Reader</param>
        /// <param name="closeReader">A flag that indicates the reader should be closed</param>
        /// <history>
        /// 	[cnurse]	11/29/2007	Documented
        /// </history>
        /// -----------------------------------------------------------------------------
        public static TObject FillObject<TObject>(IDataReader dr, bool closeReader)
        {
            return (TObject) CreateObjectFromReader(typeof (TObject), dr, closeReader);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// FillObject fills an object from a DataReader
        /// </summary>
        /// <param name="dr">The Data Reader</param>
        /// <param name="objType">The type of the object</param>
        /// <history>
        /// 	[cnurse]	11/29/2007	Documented
        /// </history>
        /// -----------------------------------------------------------------------------
        public static object FillObject(IDataReader dr, Type objType)
        {
            return CreateObjectFromReader(objType, dr, true);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// FillObject fills an object from a DataReader
        /// </summary>
        /// <param name="dr">The Data Reader</param>
        /// <param name="objType">The type of the object</param>
        /// <param name="closeReader">A flag that indicates the reader should be closed</param>
        /// <history>
        /// 	[cnurse]	11/29/2007	Documented
        /// </history>
        /// -----------------------------------------------------------------------------
        public static object FillObject(IDataReader dr, Type objType, bool closeReader)
        {
            return CreateObjectFromReader(objType, dr, closeReader);
        }
		
		#endregion

        public static IQueryable<TItem> FillQueryable<TItem>(IDataReader dr)
        {
            return FillListFromReader(dr, new List<TItem>(), true).AsQueryable();
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// FillSortedList fills a SortedList of objects from a DataReader
        /// </summary>
        /// <typeparam name="TKey">The key for the SortedList</typeparam>
        /// <typeparam name="TValue">The value for the SortedList Item</typeparam>
        /// <param name="keyField">The key field used for the Key</param>
        /// <param name="dr">The Data Reader</param>
        /// <history>
        /// 	[cnurse]	11/29/2007	Created
        /// </history>
        /// -----------------------------------------------------------------------------
        public static SortedList<TKey, TValue> FillSortedList<TKey, TValue>(string keyField, IDataReader dr)
        {
            return
                (SortedList<TKey, TValue>) FillDictionaryFromReader(keyField, dr, new SortedList<TKey, TValue>(), true);
        }

        public static void DeserializeSettings(IDictionary dictionary, XmlNode rootNode, string elementName)
        {
            string sKey = null;
            string sValue = null;

            foreach (XmlNode settingNode in rootNode.SelectNodes(elementName))
            {
                sKey = XmlUtils.GetNodeValue(settingNode.CreateNavigator(), "settingname");
                sValue = XmlUtils.GetNodeValue(settingNode.CreateNavigator(), "settingvalue");

                dictionary[sKey] = sValue;
            }
        }

        ///<summary>
        ///  Iterates items in a IDictionary object and generates XML nodes
        ///</summary>
        ///<param name = "dictionary">The IDictionary to iterate</param>
        ///<param name = "document">The XML document the node should be added to</param>
        ///<param name="targetPath">Path at which to serialize settings</param>
        ///<param name = "elementName">The name of the new element created</param>
        ///<remarks>
        ///</remarks>
        ///<history>
        ///  [jlucarino]	09/18/2009	created
        ///  [kbeigi] updated to IDictionary
        ///</history>
        public static void SerializeSettings(IDictionary dictionary, XmlDocument document, string targetPath,
                                             string elementName)
        {
            string sOuterElementName = elementName + "s";
            string sInnerElementName = elementName;
            XmlNode nodeSetting = default(XmlNode);
            XmlNode nodeSettings = default(XmlNode);
            XmlNode nodeSettingName = default(XmlNode);
            XmlNode nodeSettingValue = default(XmlNode);

            XmlNode targetNode = document.SelectSingleNode(targetPath);

            if (targetNode != null)
            {
                nodeSettings = targetNode.AppendChild(document.CreateElement(sOuterElementName));
                foreach (object sKey in dictionary.Keys)
                {
                    nodeSetting = nodeSettings.AppendChild(document.CreateElement(sInnerElementName));

                    nodeSettingName = nodeSetting.AppendChild(document.CreateElement("settingname"));
                    nodeSettingName.InnerText = sKey.ToString();

                    nodeSettingValue = nodeSetting.AppendChild(document.CreateElement("settingvalue"));
                    nodeSettingValue.InnerText = dictionary[sKey].ToString();
        }
            }
            else
            {
                throw new ArgumentException("Invalid Target Path");
            }
        }
		
		#region "GetCachedObject"

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// GetCachedObject gets an object from the Cache
        /// </summary>
        /// <typeparam name="TObject">The type of th object to fetch</typeparam>
        /// <param name="cacheItemArgs">A CacheItemArgs object that provides parameters to manage the
        /// cache AND to fetch the item if the cache has expired</param>
        /// <param name="cacheItemExpired">A CacheItemExpiredCallback delegate that is used to repopulate
        /// the cache if the item has expired</param>
        /// <history>
        /// 	[cnurse]	01/13/2008	Created
        /// </history>
        /// -----------------------------------------------------------------------------
        public static TObject GetCachedObject<TObject>(CacheItemArgs cacheItemArgs,
                                                       CacheItemExpiredCallback cacheItemExpired)
        {
            return DataCache.GetCachedData<TObject>(cacheItemArgs, cacheItemExpired);
        }

        public static TObject GetCachedObject<TObject>(CacheItemArgs cacheItemArgs,
                                                       CacheItemExpiredCallback cacheItemExpired, bool saveInDictionary)
        {
            return DataCache.GetCachedData<TObject>(cacheItemArgs, cacheItemExpired, saveInDictionary);
        }
		
		#endregion

		#region "GetProperties"

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// GetProperties gets a Dictionary of the Properties for an object
        /// </summary>
        /// <typeparam name="TObject">The type of the object</typeparam>
        /// <history>
        /// 	[cnurse]	01/17/2008	Created
        /// </history>
        /// -----------------------------------------------------------------------------
        public static Dictionary<string, PropertyInfo> GetProperties<TObject>()
        {
            return GetObjectMapping(typeof (TObject)).Properties;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// GetProperties gets a Dictionary of the Properties for an object
        /// </summary>
        /// <param name="objType">The type of the object</param>
        /// <history>
        /// 	[cnurse]	01/17/2008	Created
        /// </history>
        /// -----------------------------------------------------------------------------
        public static Dictionary<string, PropertyInfo> GetProperties(Type objType)
        {
            return GetObjectMapping(objType).Properties;
        }
		
		#endregion

		#region "InitializeObject"

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// InitializeObject initialises all the properties of an object to their 
        /// Null Values.
        /// </summary>
        /// <param name="objObject">The object to Initialise</param>
        /// <history>
        /// 	[cnurse]	11/29/2007	Created
        /// </history>
        /// -----------------------------------------------------------------------------
        public static void InitializeObject(object objObject)
        {
			//initialize properties
            foreach (PropertyInfo objPropertyInfo in GetObjectMapping(objObject.GetType()).Properties.Values)
            {
                if (objPropertyInfo.CanWrite)
                {
                    objPropertyInfo.SetValue(objObject, Null.SetNull(objPropertyInfo), null);
                }
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// InitializeObject initialises all the properties of an object to their 
        /// Null Values.
        /// </summary>
        /// <param name="objObject">The object to Initialise</param>
        /// <param name="objType">The type of the object</param>
        /// <history>
        /// 	[cnurse]	11/29/2007	Created
        /// </history>
        /// -----------------------------------------------------------------------------
        public static object InitializeObject(object objObject, Type objType)
        {
			//initialize properties
            foreach (PropertyInfo objPropertyInfo in GetObjectMapping(objType).Properties.Values)
            {
                if (objPropertyInfo.CanWrite)
                {
                    objPropertyInfo.SetValue(objObject, Null.SetNull(objPropertyInfo), null);
                }
            }
            return objObject;
        }
		
		#endregion

		#region "SerializeObject"

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// SerializeObject serializes an Object
        /// </summary>
        /// <param name="objObject">The object to Initialise</param>
        /// <param name="fileName">A filename for the resulting serialized xml</param>
        /// <history>
        /// 	[cnurse]	01/17/2007	Created
        /// </history>
        /// -----------------------------------------------------------------------------
        public static void SerializeObject(object objObject, string fileName)
        {
            using (
                XmlWriter writer = XmlWriter.Create(fileName, XmlUtils.GetXmlWriterSettings(ConformanceLevel.Fragment)))
            {
                SerializeObject(objObject, writer);
                writer.Flush();
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// SerializeObject serializes an Object
        /// </summary>
        /// <param name="objObject">The object to Initialise</param>
        /// <param name="document">An XmlDocument to serialize to</param>
        /// <history>
        /// 	[cnurse]	01/17/2007	Created
        /// </history>
        /// -----------------------------------------------------------------------------
        public static void SerializeObject(object objObject, XmlDocument document)
        {
            var sb = new StringBuilder();
            //Serialize the object
            SerializeObject(objObject, XmlWriter.Create(sb, XmlUtils.GetXmlWriterSettings(ConformanceLevel.Document)));
            //Load XmlDocument
            document.LoadXml(sb.ToString());
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// SerializeObject serializes an Object
        /// </summary>
        /// <param name="objObject">The object to Initialise</param>
        /// <param name="stream">A Stream to serialize to</param>
        /// <history>
        /// 	[cnurse]	01/17/2007	Created
        /// </history>
        /// -----------------------------------------------------------------------------
        public static void SerializeObject(object objObject, Stream stream)
        {
            using (XmlWriter writer = XmlWriter.Create(stream, XmlUtils.GetXmlWriterSettings(ConformanceLevel.Fragment))
                )
            {
                SerializeObject(objObject, writer);
                writer.Flush();
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// SerializeObject serializes an Object
        /// </summary>
        /// <param name="objObject">The object to Initialise</param>
        /// <param name="textWriter">A TextWriter to serialize to</param>
        /// <history>
        /// 	[cnurse]	01/17/2007	Created
        /// </history>
        /// -----------------------------------------------------------------------------
        public static void SerializeObject(object objObject, TextWriter textWriter)
        {
            using (
                XmlWriter writer = XmlWriter.Create(textWriter, XmlUtils.GetXmlWriterSettings(ConformanceLevel.Fragment))
                )
            {
                SerializeObject(objObject, writer);
                writer.Flush();
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// SerializeObject serializes an Object
        /// </summary>
        /// <param name="objObject">The object to Initialise</param>
        /// <param name="writer">An XmlWriter to serialize to</param>
        /// <history>
        /// 	[cnurse]	01/17/2007	Created
        /// </history>
        /// -----------------------------------------------------------------------------
        public static void SerializeObject(object objObject, XmlWriter writer)
        {
			//Try to cast the Object as IXmlSerializable
            var xmlSerializableObject = objObject as IXmlSerializable;
            if (xmlSerializableObject == null)
            {
				//Use XmlSerializer
                var serializer = new XmlSerializer(objObject.GetType());
                serializer.Serialize(writer, objObject);
            }
            else
            {
				//Use XmlWriter
                xmlSerializableObject.WriteXml(writer);
            }
        }

		#endregion
		
		#endregion

		#region Obsolete

        [Obsolete("Obsolete in DotNetNuke 5.0.  Replaced by GetProperties(Of TObject)() ")]
        public static ArrayList GetPropertyInfo(Type objType)
        {
            var arrProperties = new ArrayList();

            //get cached object mapping for type
            ObjectMappingInfo objMappingInfo = GetObjectMapping(objType);

            arrProperties.AddRange(objMappingInfo.Properties.Values);

            return arrProperties;
        }

        [Obsolete("Obsolete in DotNetNuke 5.0.  Replaced by SerializeObject(Object) ")]
        public static XmlDocument Serialize(object objObject)
        {
            var document = new XmlDocument();
            SerializeObject(objObject, document);
            return document;
        }
		
		#endregion
    }
}

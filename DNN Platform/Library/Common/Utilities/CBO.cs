// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Common.Utilities
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
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
    using DotNetNuke.Framework;
    using DotNetNuke.Services.Exceptions;
    using DotNetNuke.Services.Scheduling;

    /// <summary>
    /// The CBO class generates objects.
    /// </summary>
    public partial class CBO : ServiceLocator<ICBO, CBO>, ICBO
    {
        private const string defaultPrimaryKey = "ItemID";

        private const string objectMapCacheKey = "ObjectMap_";

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// CloneObject clones an object.
        /// </summary>
        /// <param name="objObject">The Object to Clone.</param>
        /// <returns></returns>
        /// -----------------------------------------------------------------------------
        public static object CloneObject(object objObject)
        {
            try
            {
                Type objType = objObject.GetType();
                object objNewObject = Activator.CreateInstance(objType);

                // get cached object mapping for type
                ObjectMappingInfo objMappingInfo = GetObjectMapping(objType);
                foreach (KeyValuePair<string, PropertyInfo> kvp in objMappingInfo.Properties)
                {
                    PropertyInfo objProperty = kvp.Value;
                    if (objProperty.CanWrite)
                    {
                        // Check if property is ICloneable
                        var objPropertyClone = objProperty.GetValue(objObject, null) as ICloneable;
                        if (objPropertyClone == null)
                        {
                            objProperty.SetValue(objNewObject, objProperty.GetValue(objObject, null), null);
                        }
                        else
                        {
                            objProperty.SetValue(objNewObject, objPropertyClone.Clone(), null);
                        }

                        // Check if Property is IEnumerable
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

        public static void CloseDataReader(IDataReader dr, bool closeReader)
        {
            // close datareader
            if (dr != null && closeReader)
            {
                using (dr)
                {
                    dr.Close();
                }
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// CreateObject creates a new object of Type TObject.
        /// </summary>
        /// <typeparam name="TObject">The type of object to create.</typeparam>
        /// <param name="initialise">A flag that indicates whether to initialise the
        /// object.</param>
        /// <returns></returns>
        /// -----------------------------------------------------------------------------
        public static TObject CreateObject<TObject>(bool initialise)
        {
            return (TObject)CreateObjectInternal(typeof(TObject), initialise);
        }

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
            // First Create the Object
            var objObject = CreateObject<TObject>(true);

            // Try to cast the Object as IXmlSerializable
            var xmlSerializableObject = objObject as IXmlSerializable;
            if (xmlSerializableObject == null)
            {
                // Use XmlSerializer
                var serializer = new XmlSerializer(objObject.GetType());
                objObject = (TObject)serializer.Deserialize(reader);
            }
            else
            {
                // Use XmlReader
                xmlSerializableObject.ReadXml(reader);
            }

            return objObject;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// FillCollection fills a Collection of objects from a DataReader.
        /// </summary>
        /// <param name="dr">The Data Reader.</param>
        /// <param name="objType">The type of the Object.</param>
        /// <returns></returns>
        /// -----------------------------------------------------------------------------
        public static ArrayList FillCollection(IDataReader dr, Type objType)
        {
            return (ArrayList)FillListFromReader(objType, dr, new ArrayList(), true);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// FillCollection fills a Collection of objects from a DataReader.
        /// </summary>
        /// <param name="dr">The Data Reader.</param>
        /// <param name="objType">The type of the Object.</param>
        /// <param name="closeReader">Flag that indicates whether the Data Reader should be closed.</param>
        /// <returns></returns>
        /// -----------------------------------------------------------------------------
        public static ArrayList FillCollection(IDataReader dr, Type objType, bool closeReader)
        {
            return (ArrayList)FillListFromReader(objType, dr, new ArrayList(), closeReader);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// FillCollection fills a Collection of objects from a DataReader.
        /// </summary>
        /// <param name="dr">The Data Reader.</param>
        /// <param name="objType">The type of the Object.</param>
        /// <param name="objToFill">An IList to fill.</param>
        /// <returns></returns>
        /// -----------------------------------------------------------------------------
        public static IList FillCollection(IDataReader dr, Type objType, ref IList objToFill)
        {
            return FillListFromReader(objType, dr, objToFill, true);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// FillCollection fills a Collection of objects from a DataReader.
        /// </summary>
        /// <typeparam name="TItem">The type of object.</typeparam>
        /// <param name="dr">The Data Reader.</param>
        /// <returns></returns>
        /// -----------------------------------------------------------------------------
        public static List<TItem> FillCollection<TItem>(IDataReader dr)
        {
            return (List<TItem>)FillListFromReader(dr, new List<TItem>(), true);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// FillCollection fills a Collection of objects from a DataReader.
        /// </summary>
        /// <typeparam name="TItem">The type of object.</typeparam>
        /// <param name="objToFill">The List to fill.</param>
        /// <param name="dr">The Data Reader.</param>
        /// <returns></returns>
        /// -----------------------------------------------------------------------------
        public static IList<TItem> FillCollection<TItem>(IDataReader dr, ref IList<TItem> objToFill)
        {
            return FillListFromReader(dr, objToFill, true);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// FillCollection fills a List of objects from a DataReader.
        /// </summary>
        /// <typeparam name="TItem">The type of the Object.</typeparam>
        /// <param name="objToFill">The List to fill.</param>
        /// <param name="dr">The Data Reader.</param>
        /// <param name="closeReader">A flag that indicates whether the DataReader should be closed.</param>
        /// <returns></returns>
        /// -----------------------------------------------------------------------------
        public static IList<TItem> FillCollection<TItem>(IDataReader dr, IList<TItem> objToFill, bool closeReader)
        {
            return FillListFromReader(dr, objToFill, closeReader);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Generic version of FillCollection fills a List custom business object of a specified type
        /// from the supplied DataReader.
        /// </summary>
        /// <param name="dr">The IDataReader to use to fill the object.</param>
        /// <param name="objType">The type of the Object.</param>
        /// <param name="totalRecords">The total No of records.</param>
        /// <returns>A List of custom business objects.</returns>
        /// <remarks></remarks>
        /// -----------------------------------------------------------------------------
        public static ArrayList FillCollection(IDataReader dr, ref Type objType, ref int totalRecords)
        {
            var objFillCollection = (ArrayList)FillListFromReader(objType, dr, new ArrayList(), false);
            try
            {
                if (dr.NextResult())
                {
                    // Get the total no of records from the second result
                    totalRecords = Globals.GetTotalRecords(ref dr);
                }
            }
            catch (Exception exc)
            {
                Exceptions.LogException(exc);
            }
            finally
            {
                // Ensure DataReader is closed
                CloseDataReader(dr, true);
            }

            return objFillCollection;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Generic version of FillCollection fills a List custom business object of a specified type
        /// from the supplied DataReader.
        /// </summary>
        /// <typeparam name="T">The type of the business object.</typeparam>
        /// <param name="dr">The IDataReader to use to fill the object.</param>
        /// <param name="totalRecords"></param>
        /// <returns>A List of custom business objects.</returns>
        /// <remarks></remarks>
        /// -----------------------------------------------------------------------------
        public static List<T> FillCollection<T>(IDataReader dr, ref int totalRecords)
        {
            IList<T> objFillCollection = FillCollection(dr, new List<T>(), false);
            try
            {
                if (dr.NextResult())
                {
                    // Get the total no of records from the second result
                    totalRecords = Globals.GetTotalRecords(ref dr);
                }
            }
            catch (Exception exc)
            {
                Exceptions.LogException(exc);
            }
            finally
            {
                // Ensure DataReader is closed
                CloseDataReader(dr, true);
            }

            return (List<T>)objFillCollection;
        }

        /// <summary>
        /// FillDictionary fills a Dictionary of objects from a DataReader.
        /// </summary>
        /// <typeparam name="TKey">The key for the Dictionary.</typeparam>
        /// <typeparam name="TValue">The value for the Dictionary Item.</typeparam>
        /// <param name="keyField">The key field used for the Key.</param>
        /// <param name="dr">The Data Reader.</param>
        /// <returns></returns>
        public static Dictionary<TKey, TValue> FillDictionary<TKey, TValue>(string keyField, IDataReader dr)
        {
            return
                (Dictionary<TKey, TValue>)FillDictionaryFromReader(keyField, dr, new Dictionary<TKey, TValue>(), true);
        }

        /// <summary>
        /// FillDictionary fills a Dictionary of objects from a DataReader.
        /// </summary>
        /// <typeparam name="TKey">The key for the Dictionary.</typeparam>
        /// <typeparam name="TValue">The value for the Dictionary Item.</typeparam>
        /// <param name="keyField">The key field used for the Key.</param>
        /// <param name="dr">The Data Reader.</param>
        /// <param name="closeReader">A flag indicating whether to close the reader.</param>
        /// <returns></returns>
        public static Dictionary<TKey, TValue> FillDictionary<TKey, TValue>(string keyField, IDataReader dr, bool closeReader)
        {
            return (Dictionary<TKey, TValue>)FillDictionaryFromReader(keyField, dr, new Dictionary<TKey, TValue>(), closeReader);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// FillDictionary fills a Dictionary of objects from a DataReader.
        /// </summary>
        /// <typeparam name="TKey">The key for the Dictionary.</typeparam>
        /// <typeparam name="TValue">The value for the Dictionary Item.</typeparam>
        /// <param name="keyField">The key field used for the Key.</param>
        /// <param name="objDictionary">The Dictionary to fill.</param>
        /// <param name="dr">The Data Reader.</param>
        /// <returns></returns>
        /// -----------------------------------------------------------------------------
        public static Dictionary<TKey, TValue> FillDictionary<TKey, TValue>(string keyField, IDataReader dr, IDictionary<TKey, TValue> objDictionary)
        {
            return (Dictionary<TKey, TValue>)FillDictionaryFromReader(keyField, dr, objDictionary, true);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// FillObject fills an object from a DataReader.
        /// </summary>
        /// <typeparam name="TObject">The type of the object.</typeparam>
        /// <param name="dr">The Data Reader.</param>
        /// <returns></returns>
        /// -----------------------------------------------------------------------------
        public static TObject FillObject<TObject>(IDataReader dr)
        {
            return (TObject)CreateObjectFromReader(typeof(TObject), dr, true);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// FillObject fills an object from a DataReader.
        /// </summary>
        /// <typeparam name="TObject">The type of the object.</typeparam>
        /// <param name="dr">The Data Reader.</param>
        /// <param name="closeReader">A flag that indicates the reader should be closed.</param>
        /// <returns></returns>
        /// -----------------------------------------------------------------------------
        public static TObject FillObject<TObject>(IDataReader dr, bool closeReader)
        {
            return (TObject)CreateObjectFromReader(typeof(TObject), dr, closeReader);
        }

        public static IQueryable<TItem> FillQueryable<TItem>(IDataReader dr)
        {
            return FillListFromReader(dr, new List<TItem>(), true).AsQueryable();
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// FillSortedList fills a SortedList of objects from a DataReader.
        /// </summary>
        /// <typeparam name="TKey">The key for the SortedList.</typeparam>
        /// <typeparam name="TValue">The value for the SortedList Item.</typeparam>
        /// <param name="keyField">The key field used for the Key.</param>
        /// <param name="dr">The Data Reader.</param>
        /// <returns></returns>
        /// -----------------------------------------------------------------------------
        public static SortedList<TKey, TValue> FillSortedList<TKey, TValue>(string keyField, IDataReader dr)
        {
            return
                (SortedList<TKey, TValue>)FillDictionaryFromReader(keyField, dr, new SortedList<TKey, TValue>(), true);
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

        /// <summary>
        ///  Iterates items in a IDictionary object and generates XML nodes.
        /// </summary>
        /// <param name = "dictionary">The IDictionary to iterate.</param>
        /// <param name = "document">The XML document the node should be added to.</param>
        /// <param name="targetPath">Path at which to serialize settings.</param>
        /// <param name = "elementName">The name of the new element created.</param>
        /// <remarks>
        /// </remarks>
        public static void SerializeSettings(IDictionary dictionary, XmlDocument document, string targetPath, string elementName)
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

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// GetCachedObject gets an object from the Cache.
        /// </summary>
        /// <typeparam name="TObject">The type of th object to fetch.</typeparam>
        /// <param name="cacheItemArgs">A CacheItemArgs object that provides parameters to manage the
        /// cache AND to fetch the item if the cache has expired.</param>
        /// <param name="cacheItemExpired">A CacheItemExpiredCallback delegate that is used to repopulate
        /// the cache if the item has expired.</param>
        /// <returns></returns>
        /// -----------------------------------------------------------------------------
        public static TObject GetCachedObject<TObject>(CacheItemArgs cacheItemArgs, CacheItemExpiredCallback cacheItemExpired)
        {
            return DataCache.GetCachedData<TObject>(cacheItemArgs, cacheItemExpired);
        }

        public static TObject GetCachedObject<TObject>(CacheItemArgs cacheItemArgs, CacheItemExpiredCallback cacheItemExpired, bool saveInDictionary)
        {
            return DataCache.GetCachedData<TObject>(cacheItemArgs, cacheItemExpired, saveInDictionary);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// GetProperties gets a Dictionary of the Properties for an object.
        /// </summary>
        /// <typeparam name="TObject">The type of the object.</typeparam>
        /// <returns></returns>
        /// -----------------------------------------------------------------------------
        public static Dictionary<string, PropertyInfo> GetProperties<TObject>()
        {
            return GetObjectMapping(typeof(TObject)).Properties;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// GetProperties gets a Dictionary of the Properties for an object.
        /// </summary>
        /// <param name="objType">The type of the object.</param>
        /// <returns></returns>
        /// -----------------------------------------------------------------------------
        public static Dictionary<string, PropertyInfo> GetProperties(Type objType)
        {
            return GetObjectMapping(objType).Properties;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// InitializeObject initialises all the properties of an object to their
        /// Null Values.
        /// </summary>
        /// <param name="objObject">The object to Initialise.</param>
        /// -----------------------------------------------------------------------------
        public static void InitializeObject(object objObject)
        {
            // initialize properties
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
        /// <param name="objObject">The object to Initialise.</param>
        /// <param name="objType">The type of the object.</param>
        /// <returns></returns>
        /// -----------------------------------------------------------------------------
        public static object InitializeObject(object objObject, Type objType)
        {
            // initialize properties
            foreach (PropertyInfo objPropertyInfo in GetObjectMapping(objType).Properties.Values)
            {
                if (objPropertyInfo.CanWrite)
                {
                    objPropertyInfo.SetValue(objObject, Null.SetNull(objPropertyInfo), null);
                }
            }

            return objObject;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// SerializeObject serializes an Object.
        /// </summary>
        /// <param name="objObject">The object to Initialise.</param>
        /// <param name="fileName">A filename for the resulting serialized xml.</param>
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
        /// SerializeObject serializes an Object.
        /// </summary>
        /// <param name="objObject">The object to Initialise.</param>
        /// <param name="document">An XmlDocument to serialize to.</param>
        /// -----------------------------------------------------------------------------
        public static void SerializeObject(object objObject, XmlDocument document)
        {
            var sb = new StringBuilder();
            using (var writer = XmlWriter.Create(sb, XmlUtils.GetXmlWriterSettings(ConformanceLevel.Document)))
            {
                // Serialize the object
                SerializeObject(objObject, writer);

                // Load XmlDocument
                document.LoadXml(sb.ToString());
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// SerializeObject serializes an Object.
        /// </summary>
        /// <param name="objObject">The object to Initialise.</param>
        /// <param name="stream">A Stream to serialize to.</param>
        /// -----------------------------------------------------------------------------
        public static void SerializeObject(object objObject, Stream stream)
        {
            using (XmlWriter writer = XmlWriter.Create(stream, XmlUtils.GetXmlWriterSettings(ConformanceLevel.Fragment)))
            {
                SerializeObject(objObject, writer);
                writer.Flush();
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// SerializeObject serializes an Object.
        /// </summary>
        /// <param name="objObject">The object to Initialise.</param>
        /// <param name="textWriter">A TextWriter to serialize to.</param>
        /// -----------------------------------------------------------------------------
        public static void SerializeObject(object objObject, TextWriter textWriter)
        {
            using (
                XmlWriter writer = XmlWriter.Create(textWriter, XmlUtils.GetXmlWriterSettings(ConformanceLevel.Fragment)))
            {
                SerializeObject(objObject, writer);
                writer.Flush();
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// SerializeObject serializes an Object.
        /// </summary>
        /// <param name="objObject">The object to Initialise.</param>
        /// <param name="writer">An XmlWriter to serialize to.</param>
        /// -----------------------------------------------------------------------------
        public static void SerializeObject(object objObject, XmlWriter writer)
        {
            // Try to cast the Object as IXmlSerializable
            var xmlSerializableObject = objObject as IXmlSerializable;
            if (xmlSerializableObject == null)
            {
                // Use XmlSerializer
                var serializer = new XmlSerializer(objObject.GetType());
                serializer.Serialize(writer, objObject);
            }
            else
            {
                // Use XmlWriter
                xmlSerializableObject.WriteXml(writer);
            }
        }

        protected override Func<ICBO> GetFactory()
        {
            return () => new CBO();
        }

        private static object CreateObjectInternal(Type objType, bool initialise)
        {
            object objObject = Activator.CreateInstance(objType);

            if (initialise)
            {
                InitializeObject(objObject);
            }

            return objObject;
        }

        private static object CreateObjectFromReader(Type objType, IDataReader dr, bool closeReader)
        {
            object objObject = null;
            bool isSuccess = Null.NullBoolean;
            bool canRead = true;

            if (closeReader)
            {
                canRead = false;

                // read datareader
                if (dr.Read())
                {
                    canRead = true;
                }
            }

            try
            {
                if (canRead)
                {
                    // Create the Object
                    objObject = CreateObjectInternal(objType, false);

                    // hydrate the custom business object
                    FillObjectFromReader(objObject, dr);
                }

                isSuccess = true;
            }
            finally
            {
                // Ensure DataReader is closed
                if (!isSuccess)
                {
                    closeReader = true;
                }

                CloseDataReader(dr, closeReader);
            }

            return objObject;
        }

        private static IDictionary<TKey, TValue> FillDictionaryFromReader<TKey, TValue>(string keyField, 
                                                                                        IDataReader dr,
                                                                                        IDictionary<TKey, TValue> objDictionary,
                                                                                        bool closeReader)
        {
            TValue objObject;
            TKey keyValue = default(TKey);
            bool isSuccess = Null.NullBoolean;

            try
            {
                // iterate datareader
                while (dr.Read())
                {
                    // Create the Object
                    objObject = (TValue)CreateObjectFromReader(typeof(TValue), dr, false);
                    if (keyField == "KeyID" && objObject is IHydratable)
                    {
                        // Get the value of the key field from the KeyID
                        keyValue = (TKey)Null.SetNull(((IHydratable)objObject).KeyID, keyValue);
                    }
                    else
                    {
                        // Get the value of the key field from the DataReader
                        if (typeof(TKey).Name == "Int32" && dr[keyField].GetType().Name == "Decimal")
                        {
                            keyValue = (TKey)Convert.ChangeType(Null.SetNull(dr[keyField], keyValue), typeof(TKey));
                        }
                        else if (typeof(TKey).Name.Equals("string", StringComparison.OrdinalIgnoreCase) &&
                                 dr[keyField].GetType().Name.Equals("dbnull", StringComparison.OrdinalIgnoreCase))
                        {
                            keyValue = (TKey)Convert.ChangeType(Null.SetNull(dr[keyField], string.Empty), typeof(TKey));
                        }
                        else
                        {
                            keyValue = (TKey)Convert.ChangeType(Null.SetNull(dr[keyField], string.Empty), typeof(TKey));
                        }
                    }

                    // add to dictionary
                    if (objObject != null)
                    {
                        objDictionary[keyValue] = objObject;
                    }
                }

                isSuccess = true;
            }
            finally
            {
                // Ensure DataReader is closed
                if (!isSuccess)
                {
                    closeReader = true;
                }

                CloseDataReader(dr, closeReader);
            }

            // Return the dictionary
            return objDictionary;
        }

        private static IList FillListFromReader(Type objType, IDataReader dr, IList objList, bool closeReader)
        {
            object objObject;
            bool isSuccess = Null.NullBoolean;
            try
            {
                // iterate datareader
                while (dr.Read())
                {
                    // Create the Object
                    objObject = CreateObjectFromReader(objType, dr, false);

                    // add to collection
                    objList.Add(objObject);
                }

                isSuccess = true;
            }
            finally
            {
                // Ensure DataReader is closed
                if (!isSuccess)
                {
                    closeReader = true;
                }

                CloseDataReader(dr, closeReader);
            }

            return objList;
        }

        private static IList<TItem> FillListFromReader<TItem>(IDataReader dr, IList<TItem> objList, bool closeReader)
        {
            TItem objObject;
            bool isSuccess = Null.NullBoolean;
            try
            {
                // iterate datareader
                while (dr.Read())
                {
                    // Create the Object
                    objObject = (TItem)CreateObjectFromReader(typeof(TItem), dr, false);

                    // add to collection
                    objList.Add(objObject);
                }

                isSuccess = true;
            }
            finally
            {
                // Ensure DataReader is closed
                if (!isSuccess)
                {
                    closeReader = true;
                }

                CloseDataReader(dr, closeReader);
            }

            return objList;
        }

        private static void FillObjectFromReader(object objObject, IDataReader dr)
        {
            try
            {
                // Determine if object is IHydratable
                if (objObject is IHydratable)
                {
                    // Use IHydratable's Fill
                    var objHydratable = objObject as IHydratable;
                    if (objHydratable != null)
                    {
                        objHydratable.Fill(dr);
                    }
                }
                else
                {
                    // Use Reflection
                    HydrateObject(objObject, dr);
                }
            }
            catch (IndexOutOfRangeException iex)
            {
                // Call to GetOrdinal is being made with a bad column name
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

        private static void HydrateObject(object hydratedObject, IDataReader dr)
        {
            PropertyInfo objPropertyInfo = null;
            Type propType = null;
            object coloumnValue;
            Type objDataType;
            int intIndex;

            // get cached object mapping for type
            ObjectMappingInfo objMappingInfo = GetObjectMapping(hydratedObject.GetType());
            if (hydratedObject is BaseEntityInfo && !(hydratedObject is ScheduleItem))
            {
                // Call the base classes fill method to populate base class properties
                ((BaseEntityInfo)hydratedObject).FillBaseProperties(dr);
            }

            // fill object with values from datareader
            for (intIndex = 0; intIndex <= dr.FieldCount - 1; intIndex++)
            {
                // If the Column matches a Property in the Object Map's PropertyInfo Dictionary
                if (objMappingInfo.Properties.TryGetValue(dr.GetName(intIndex).ToUpperInvariant(), out objPropertyInfo))
                {
                    // Get its type
                    propType = objPropertyInfo.PropertyType;

                    // If property can be set
                    if (objPropertyInfo.CanWrite)
                    {
                        // Get the Data Value from the data reader
                        coloumnValue = dr.GetValue(intIndex);

                        // Get the Data Value's type
                        objDataType = coloumnValue.GetType();
                        if (coloumnValue == null || coloumnValue == DBNull.Value)
                        {
                            // set property value to Null
                            objPropertyInfo.SetValue(hydratedObject, Null.SetNull(objPropertyInfo), null);
                        }
                        else if (propType.Equals(objDataType))
                        {
                            // Property and data objects are the same type
                            objPropertyInfo.SetValue(hydratedObject, coloumnValue, null);
                        }
                        else
                        {
                            // business object info class member data type does not match datareader member data type
                            // need to handle enumeration conversions differently than other base types
                            if (propType.BaseType.Equals(typeof(Enum)))
                            {
                                // check if value is numeric and if not convert to integer ( supports databases like Oracle )
                                if (Globals.NumberMatchRegex.IsMatch(coloumnValue.ToString()))
                                {
                                    objPropertyInfo.SetValue(
                                        hydratedObject,
                                        Enum.ToObject(propType, Convert.ToInt32(coloumnValue)),
                                        null);
                                }
                                else
                                {
                                    objPropertyInfo.SetValue(hydratedObject, Enum.ToObject(propType, coloumnValue), null);
                                }
                            }
                            else if (propType == typeof(Guid))
                            {
                                // guid is not a datatype common across all databases ( ie. Oracle )
                                objPropertyInfo.SetValue(
                                    hydratedObject,
                                    Convert.ChangeType(new Guid(coloumnValue.ToString()), propType),
                                    null);
                            }
                            else if (propType == typeof(Version))
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

        private static object ChangeType(object obj, Type type)
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

        private static object GetDefault(Type type)
        {
            if (type.IsValueType)
            {
                return Activator.CreateInstance(type);
            }

            return null;
        }

        private static string GetColumnName(PropertyInfo objProperty)
        {
            string columnName = objProperty.Name;
            return columnName;
        }

        private static ObjectMappingInfo GetObjectMapping(Type objType)
        {
            string cacheKey = objectMapCacheKey + objType.FullName;
            var objMap = (ObjectMappingInfo)DataCache.GetCache(cacheKey);
            if (objMap == null)
            {
                // Create an ObjectMappingInfo instance
                objMap = new ObjectMappingInfo();
                objMap.ObjectType = objType.FullName;

                // Reflect on class to create Object Map
                objMap.PrimaryKey = GetPrimaryKey(objType);
                objMap.TableName = GetTableName(objType);

                // Iterate through the objects properties and add each one to the ObjectMappingInfo's Properties Dictionary
                foreach (PropertyInfo objProperty in objType.GetProperties())
                {
                    objMap.Properties.Add(objProperty.Name.ToUpperInvariant(), objProperty);
                    objMap.ColumnNames.Add(objProperty.Name.ToUpperInvariant(), GetColumnName(objProperty));
                }

                // Persist to Cache
                DataCache.SetCache(cacheKey, objMap);
            }

            // Return Object Map
            return objMap;
        }

        private static string GetPrimaryKey(Type objType)
        {
            string primaryKey = defaultPrimaryKey;
            return primaryKey;
        }

        private static string GetTableName(Type objType)
        {
            string tableName = string.Empty;

            // If no attrubute then use Type Name
            if (string.IsNullOrEmpty(tableName))
            {
                tableName = objType.Name;
                if (tableName.EndsWith("Info"))
                {
                    // Remove Info ending
                    tableName.Replace("Info", string.Empty);
                }
            }

            // Check if there is an object qualifier
            if (!string.IsNullOrEmpty(Config.GetSetting("ObjectQualifier")))
            {
                tableName = Config.GetSetting("ObjectQualifier") + tableName;
            }

            return tableName;
        }

        List<TItem> ICBO.FillCollection<TItem>(IDataReader dr)
        {
            return (List<TItem>)FillListFromReader(dr, new List<TItem>(), true);
        }

        TObject ICBO.FillObject<TObject>(IDataReader dr)
        {
            return (TObject)CreateObjectFromReader(typeof(TObject), dr, true);
        }

        TObject ICBO.GetCachedObject<TObject>(CacheItemArgs cacheItemArgs, CacheItemExpiredCallback cacheItemExpired, bool saveInDictionary)
        {
            return DataCache.GetCachedData<TObject>(cacheItemArgs, cacheItemExpired, saveInDictionary);
        }
    }
}

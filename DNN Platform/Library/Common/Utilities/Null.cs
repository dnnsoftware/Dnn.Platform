// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Common.Utilities
{
    using System;
    using System.Reflection;

    public class Null
    {
        public static short NullShort
        {
            get
            {
                return -1;
            }
        }

        public static int NullInteger
        {
            get
            {
                return -1;
            }
        }

        public static byte NullByte
        {
            get
            {
                return 255;
            }
        }

        public static float NullSingle
        {
            get
            {
                return float.MinValue;
            }
        }

        public static double NullDouble
        {
            get
            {
                return double.MinValue;
            }
        }

        public static decimal NullDecimal
        {
            get
            {
                return decimal.MinValue;
            }
        }

        public static DateTime NullDate
        {
            get
            {
                return DateTime.MinValue;
            }
        }

        public static string NullString
        {
            get
            {
                return string.Empty;
            }
        }

        public static bool NullBoolean
        {
            get
            {
                return false;
            }
        }

        public static Guid NullGuid
        {
            get
            {
                return Guid.Empty;
            }
        }

        // sets a field to an application encoded null value ( used in BLL layer )
        public static object SetNull(object objValue, object objField)
        {
            object returnValue = null;
            if (objValue == DBNull.Value)
            {
                if (objField is short)
                {
                    returnValue = NullShort;
                }
                else if (objField is byte)
                {
                    returnValue = NullByte;
                }
                else if (objField is int)
                {
                    returnValue = NullInteger;
                }
                else if (objField is float)
                {
                    returnValue = NullSingle;
                }
                else if (objField is double)
                {
                    returnValue = NullDouble;
                }
                else if (objField is decimal)
                {
                    returnValue = NullDecimal;
                }
                else if (objField is DateTime)
                {
                    returnValue = NullDate;
                }
                else if (objField is string)
                {
                    returnValue = NullString;
                }
                else if (objField is bool)
                {
                    returnValue = NullBoolean;
                }
                else if (objField is Guid)
                {
                    returnValue = NullGuid;
                }
                else // complex object
                {
                    returnValue = null;
                }
            }
            else // return value
            {
                returnValue = objValue;
            }

            return returnValue;
        }

        // sets a field to an application encoded null value ( used in BLL layer )
        public static object SetNull(PropertyInfo objPropertyInfo)
        {
            object returnValue = null;
            switch (objPropertyInfo.PropertyType.ToString())
            {
                case "System.Int16":
                    returnValue = NullShort;
                    break;
                case "System.Int32":
                case "System.Int64":
                    returnValue = NullInteger;
                    break;
                case "system.Byte":
                    returnValue = NullByte;
                    break;
                case "System.Single":
                    returnValue = NullSingle;
                    break;
                case "System.Double":
                    returnValue = NullDouble;
                    break;
                case "System.Decimal":
                    returnValue = NullDecimal;
                    break;
                case "System.DateTime":
                    returnValue = NullDate;
                    break;
                case "System.String":
                case "System.Char":
                    returnValue = NullString;
                    break;
                case "System.Boolean":
                    returnValue = NullBoolean;
                    break;
                case "System.Guid":
                    returnValue = NullGuid;
                    break;
                default:
                    // Enumerations default to the first entry
                    Type pType = objPropertyInfo.PropertyType;
                    if (pType.BaseType.Equals(typeof(Enum)))
                    {
                        Array objEnumValues = Enum.GetValues(pType);
                        Array.Sort(objEnumValues);
                        returnValue = Enum.ToObject(pType, objEnumValues.GetValue(0));
                    }
                    else // complex object
                    {
                        returnValue = null;
                    }

                    break;
            }

            return returnValue;
        }

        public static bool SetNullBoolean(object objValue)
        {
            return objValue != DBNull.Value ? Convert.ToBoolean(objValue) : NullBoolean;
        }

        public static DateTime SetNullDateTime(object objValue)
        {
            return objValue != DBNull.Value ? Convert.ToDateTime(objValue) : NullDate;
        }

        public static int SetNullInteger(object objValue)
        {
            return objValue != DBNull.Value ? Convert.ToInt32(objValue) : NullInteger;
        }

        public static float SetNullSingle(object objValue)
        {
            return objValue != DBNull.Value ? Convert.ToSingle(objValue) : NullSingle;
        }

        public static string SetNullString(object objValue)
        {
            return objValue != DBNull.Value ? Convert.ToString(objValue) : NullString;
        }

        public static Guid SetNullGuid(object objValue)
        {
            if ((!(objValue == DBNull.Value)) && !string.IsNullOrEmpty(objValue.ToString()))
            {
                return new Guid(objValue.ToString());
            }

            return Guid.Empty;
        }

        // convert an application encoded null value to a database null value ( used in DAL )
        public static object GetNull(object objField, object objDBNull)
        {
            object returnValue = objField;
            if (objField == null)
            {
                returnValue = objDBNull;
            }
            else if (objField is byte)
            {
                if (Convert.ToByte(objField) == NullByte)
                {
                    returnValue = objDBNull;
                }
            }
            else if (objField is short)
            {
                if (Convert.ToInt16(objField) == NullShort)
                {
                    returnValue = objDBNull;
                }
            }
            else if (objField is int)
            {
                if (Convert.ToInt32(objField) == NullInteger)
                {
                    returnValue = objDBNull;
                }
            }
            else if (objField is float)
            {
                if (Convert.ToSingle(objField) == NullSingle)
                {
                    returnValue = objDBNull;
                }
            }
            else if (objField is double)
            {
                if (Convert.ToDouble(objField) == NullDouble)
                {
                    returnValue = objDBNull;
                }
            }
            else if (objField is decimal)
            {
                if (Convert.ToDecimal(objField) == NullDecimal)
                {
                    returnValue = objDBNull;
                }
            }
            else if (objField is DateTime)
            {
                // compare the Date part of the DateTime with the DatePart of the NullDate ( this avoids subtle time differences )
                if (Convert.ToDateTime(objField).Date == NullDate.Date)
                {
                    returnValue = objDBNull;
                }
            }
            else if (objField is string)
            {
                if (objField == null)
                {
                    returnValue = objDBNull;
                }
                else
                {
                    if (objField.ToString() == NullString)
                    {
                        returnValue = objDBNull;
                    }
                }
            }
            else if (objField is bool)
            {
                if (Convert.ToBoolean(objField) == NullBoolean)
                {
                    returnValue = objDBNull;
                }
            }
            else if (objField is Guid)
            {
                if (((Guid)objField).Equals(NullGuid))
                {
                    returnValue = objDBNull;
                }
            }

            return returnValue;
        }

        // checks if a field contains an application encoded null value
        public static bool IsNull(object objField)
        {
            bool isNull = false;
            if (objField != null)
            {
                if (objField is int)
                {
                    isNull = objField.Equals(NullInteger);
                }
                else if (objField is short)
                {
                    isNull = objField.Equals(NullShort);
                }
                else if (objField is byte)
                {
                    isNull = objField.Equals(NullByte);
                }
                else if (objField is float)
                {
                    isNull = objField.Equals(NullSingle);
                }
                else if (objField is double)
                {
                    isNull = objField.Equals(NullDouble);
                }
                else if (objField is decimal)
                {
                    isNull = objField.Equals(NullDecimal);
                }
                else if (objField is DateTime)
                {
                    var objDate = (DateTime)objField;
                    isNull = objDate.Date.Equals(NullDate.Date);
                }
                else if (objField is string)
                {
                    isNull = objField.Equals(NullString);
                }
                else if (objField is bool)
                {
                    isNull = objField.Equals(NullBoolean);
                }
                else if (objField is Guid)
                {
                    isNull = objField.Equals(NullGuid);
                }
                else // complex object
                {
                    isNull = false;
                }
            }
            else
            {
                isNull = true;
            }

            return isNull;
        }
    }
}

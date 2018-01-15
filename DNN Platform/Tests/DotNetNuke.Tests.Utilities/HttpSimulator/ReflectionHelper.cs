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
using System;
using System.Reflection;

namespace DotNetNuke.Tests.Instance.Utilities.HttpSimulator
{
	/// <summary>
	/// Helper class to simplify common reflection tasks.
	/// </summary>
	public static class ReflectionHelper
	{
	    /// <summary>
		/// Returns the value of the private member specified.
		/// </summary>
		/// <param name="fieldName">Name of the member.</param>
        /// <param name="type">Type of the member.</param>
		public static T GetStaticFieldValue<T>(string fieldName, Type type)
		{
			var field = type.GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Static);
			if(field != null)
			{
				return (T)field.GetValue(type);
			}
			return default(T);
		}

	    /// <summary>
	    /// Returns the value of the private member specified.
	    /// </summary>
	    /// <param name="fieldName">Name of the member.</param>
	    /// <param name="typeName"></param>
	    public static T GetStaticFieldValue<T>(string fieldName, string typeName)
        {
            var type = Type.GetType(typeName, true);
            var field = type.GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Static);
            if (field != null)
            {
                return (T)field.GetValue(type);
            }
            return default(T);
        }

        /// <summary>
        /// Sets the value of the private static member.
        /// </summary>
        /// <param name="fieldName"></param>
        /// <param name="type"></param>
        /// <param name="value"></param>
        public static void SetStaticFieldValue<T>(string fieldName, Type type, T value)
        {
            var field = type.GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Static);
            if (field == null)
                throw new ArgumentException(string.Format("Could not find the private instance field '{0}'", fieldName));
            
            field.SetValue(null, value);
        }

	    /// <summary>
	    /// Sets the value of the private static member.
	    /// </summary>
	    /// <param name="fieldName"></param>
	    /// <param name="typeName"></param>
	    /// <param name="value"></param>
	    public static void SetStaticFieldValue<T>(string fieldName, string typeName, T value)
        {
            var type = Type.GetType(typeName, true);
            var field = type.GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Static);
            if (field == null)
                throw new ArgumentException(string.Format("Could not find the private instance field '{0}'", fieldName));

            field.SetValue(null, value);
        }

	    /// <summary>
		/// Returns the value of the private member specified.
		/// </summary>
		/// <param name="fieldName">Name of the member.</param>
		/// <param name="source">The object that contains the member.</param>
		public static T GetPrivateInstanceFieldValue<T>(string fieldName, object source)
		{
			var field = source.GetType().GetField(fieldName, BindingFlags.GetField | BindingFlags.NonPublic | BindingFlags.Instance);
			if(field != null)
			{
				return (T)field.GetValue(source);
			}
			return default(T);
		}

        /// <summary>
        /// Returns the value of the private member specified.
        /// </summary>
        /// <param name="memberName">Name of the member.</param>
        /// <param name="source">The object that contains the member.</param>
        /// <param name="value">The value to set the member to.</param>
        public static void SetPrivateInstanceFieldValue(string memberName, object source, object value)
        {
            var field = source.GetType().GetField(memberName, BindingFlags.GetField | BindingFlags.NonPublic | BindingFlags.Instance);
            if (field == null)
                throw new ArgumentException(string.Format("Could not find the private instance field '{0}'",memberName));
            
            field.SetValue(source, value);
        }

        public static object Instantiate(string typeName)
        {
            return Instantiate(typeName, null, null);
        }

	    public static object Instantiate(string typeName, Type[] constructorArgumentTypes, params object[] constructorParameterValues)
        {
	    	return Instantiate(Type.GetType(typeName, true), constructorArgumentTypes, constructorParameterValues);
        }

		public static object Instantiate(Type type, Type[] constructorArgumentTypes, params object[] constructorParameterValues)
		{
			var constructor = type.GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null, constructorArgumentTypes, null);
			return constructor.Invoke(constructorParameterValues);
		}

		/// <summary>
        /// Invokes a non-public static method.
        /// </summary>
        /// <typeparam name="TReturn"></typeparam>
        /// <param name="type"></param>
        /// <param name="methodName"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static TReturn InvokeNonPublicMethod<TReturn>(Type type, string methodName, params object[] parameters)
        {
            var paramTypes = Array.ConvertAll(parameters, o => o.GetType());
            var method = type.GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Static, null, paramTypes, null);

            if (method == null)
                throw new ArgumentException(string.Format("Could not find a method with the name '{0}'", methodName), "methodName");

            return (TReturn)method.Invoke(null, parameters);
        }

	    public static TReturn InvokeNonPublicMethod<TReturn>(object source, string methodName, params object[] parameters)
        {
            var paramTypes = Array.ConvertAll(parameters, o => o.GetType());
            var method = source.GetType().GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance, null, paramTypes, null);

            if (method == null)
                throw new ArgumentException(string.Format("Could not find a method with the name '{0}'", methodName), "methodName");

            return (TReturn)method.Invoke(source, parameters);
        }

        public static TReturn InvokeProperty<TReturn>(object source, string propertyName)
        {
            var propertyInfo = source.GetType().GetProperty(propertyName);
            if (propertyInfo == null)
                throw new ArgumentException(string.Format("Could not find a propertyName with the name '{0}'", propertyName), "propertyName");

            return (TReturn)propertyInfo.GetValue(source, null);
        }

	    public static TReturn InvokeNonPublicProperty<TReturn>(object source, string propertyName)
        {
            var propertyInfo = source.GetType().GetProperty(propertyName, BindingFlags.NonPublic | BindingFlags.Instance, null, typeof(TReturn), new Type[0], null);
            if (propertyInfo == null)
                throw new ArgumentException(string.Format("Could not find a propertyName with the name '{0}'", propertyName), "propertyName");

            return (TReturn) propertyInfo.GetValue(source, null);
        }

        public static object InvokeNonPublicProperty(object source, string propertyName)
        {
            var propertyInfo = source.GetType().GetProperty(propertyName, BindingFlags.NonPublic | BindingFlags.Instance);
            if (propertyInfo == null)
                throw new ArgumentException(string.Format("Could not find a propertyName with the name '{0}'", propertyName), "propertyName");

            return propertyInfo.GetValue(source, null);
        }
	}
}

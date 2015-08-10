#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2015
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
using System.Collections;
using System.Diagnostics;
using System.Web.Compilation;
using System.Reflection;

namespace DotNetNuke.UI.Utilities
{

	public class Reflection
	{

		#region "Public Shared Methods"

		public static object CreateObject(string TypeName, string CacheKey)
		{

			return CreateObject(TypeName, CacheKey, true);

		}

		public static object CreateObject(string TypeName, string CacheKey, bool UseCache)
		{

			// dynamically create the object
			return Activator.CreateInstance(CreateType(TypeName, CacheKey, UseCache));

		}

		public static T CreateObject<T>()
		{

			// dynamically create the object
			return Activator.CreateInstance<T>();

		}

		public static Type CreateType(string TypeName)
		{
			return CreateType(TypeName, "", true, false);
		}

		public static Type CreateType(string TypeName, bool IgnoreErrors)
		{
			return CreateType(TypeName, "", true, IgnoreErrors);
		}

		public static Type CreateType(string TypeName, string CacheKey, bool UseCache)
		{
			return CreateType(TypeName, CacheKey, UseCache, false);
		}

		public static Type CreateType(string TypeName, string CacheKey, bool UseCache, bool IgnoreErrors)
		{

			if (string.IsNullOrEmpty(CacheKey)) {
				CacheKey = TypeName;
			}

			Type objType = null;

			// use the cache for performance
			if (UseCache) {
				objType = (Type)DataCache.GetCache(CacheKey);
			}

			// is the type in the cache?
			if (objType == null) {
				try {
					// use reflection to get the type of the class
					objType = BuildManager.GetType(TypeName, true, true);

					if (UseCache) {
						// insert the type into the cache
						DataCache.SetCache(CacheKey, objType);
					}
				} catch (Exception exc) {
					// could not load the type
					if (!IgnoreErrors) {
						//LogException(exc)
						throw exc;
					}
				}
			}

			return objType;
		}

		public static object CreateInstance(Type Type)
		{
			if ((Type != null)) {
				return Type.InvokeMember("", System.Reflection.BindingFlags.CreateInstance, null, null, null, null);
			} else {
				return null;
			}
		}

		public static object GetProperty(Type Type, string PropertyName, object Target)
		{
			if ((Type != null)) {
				return Type.InvokeMember(PropertyName, System.Reflection.BindingFlags.GetProperty, null, Target, null);
			} else {
				return null;
			}
		}

		public static void SetProperty(Type Type, string PropertyName, object Target, object[] Args)
		{
			if ((Type != null)) {
				Type.InvokeMember(PropertyName, System.Reflection.BindingFlags.SetProperty, null, Target, Args);
			}
		}

		public static object InvokeMethod(Type Type, string MethodName, object Target, object[] Args)
		{
			if ((Type != null)) {
				//Can't use this. in case generic method defined, params match and ambiguous match found error
				//Dim method As MethodInfo = Type.GetMethod(MethodName, Type.GetTypeArray(Args))
				bool match = false;
				MethodInfo method = null;
				foreach (MethodInfo method_loopVariable in Type.GetMethods()) {
					method = method_loopVariable;
					if (method.Name == MethodName && method.IsGenericMethod == false && method.GetParameters().Length == Args.Length) {
						if (ParamsSameType(method.GetParameters(), Args)) {
							match = true;
							break; // TODO: might not be correct. Was : Exit For
						}
					}
				}
				if (match && (method != null)) {
					return method.Invoke(Target, Args);
				}
			}
			return null;
		}

		private static bool ParamsSameType(ParameterInfo[] @params, object[] Args)
		{
			bool match = true;
			for (int i = 0; i <= Args.Length - 1; i++) {
				if (@params[i].ParameterType.IsAssignableFrom(Args[i].GetType()) == false) {
					match = false;
					break; // TODO: might not be correct. Was : Exit For
				}
			}
			return match;
		}

		public static T InvokeGenericMethod<T>(Type Type, string MethodName, object Target, object[] Args)
		{
			if ((Type != null)) {
				MethodInfo method = null;
				foreach (MethodInfo method_loopVariable in Type.GetMembers()) {
					method = method_loopVariable;
					if (method.Name == MethodName && method.IsGenericMethod)
						break; // TODO: might not be correct. Was : Exit For
				}
				if ((method != null)) {
					MethodInfo genMethod = method.MakeGenericMethod(typeof(T));
					return (T)genMethod.Invoke(Target, Args);
				}
			}
			return default(T);
		}

		#endregion

	}

}

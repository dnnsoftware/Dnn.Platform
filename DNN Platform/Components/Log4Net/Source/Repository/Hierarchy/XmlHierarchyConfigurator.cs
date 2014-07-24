using log4net.Appender;
using log4net.Core;
using log4net.ObjectRenderer;
using log4net.Util;
using System;
using System.Collections;
using System.Globalization;
using System.Reflection;
using System.Security;
using System.Xml;

namespace log4net.Repository.Hierarchy
{
	public class XmlHierarchyConfigurator
	{
		private const string CONFIGURATION_TAG = "log4net";

		private const string RENDERER_TAG = "renderer";

		private const string APPENDER_TAG = "appender";

		private const string APPENDER_REF_TAG = "appender-ref";

		private const string PARAM_TAG = "param";

		private const string CATEGORY_TAG = "category";

		private const string PRIORITY_TAG = "priority";

		private const string LOGGER_TAG = "logger";

		private const string NAME_ATTR = "name";

		private const string TYPE_ATTR = "type";

		private const string VALUE_ATTR = "value";

		private const string ROOT_TAG = "root";

		private const string LEVEL_TAG = "level";

		private const string REF_ATTR = "ref";

		private const string ADDITIVITY_ATTR = "additivity";

		private const string THRESHOLD_ATTR = "threshold";

		private const string CONFIG_DEBUG_ATTR = "configDebug";

		private const string INTERNAL_DEBUG_ATTR = "debug";

		private const string EMIT_INTERNAL_DEBUG_ATTR = "emitDebug";

		private const string CONFIG_UPDATE_MODE_ATTR = "update";

		private const string RENDERING_TYPE_ATTR = "renderingClass";

		private const string RENDERED_TYPE_ATTR = "renderedClass";

		private const string INHERITED = "inherited";

		private Hashtable m_appenderBag;

		private readonly log4net.Repository.Hierarchy.Hierarchy m_hierarchy;

		private readonly static Type declaringType;

		static XmlHierarchyConfigurator()
		{
			XmlHierarchyConfigurator.declaringType = typeof(XmlHierarchyConfigurator);
		}

		public XmlHierarchyConfigurator(log4net.Repository.Hierarchy.Hierarchy hierarchy)
		{
			this.m_hierarchy = hierarchy;
			this.m_appenderBag = new Hashtable();
		}

		public void Configure(XmlElement element)
		{
			if (element == null || this.m_hierarchy == null)
			{
				return;
			}
			if (element.LocalName != "log4net")
			{
				LogLog.Error(XmlHierarchyConfigurator.declaringType, "Xml element is - not a <log4net> element.");
				return;
			}
			if (!LogLog.EmitInternalMessages)
			{
				string attribute = element.GetAttribute("emitDebug");
				LogLog.Debug(XmlHierarchyConfigurator.declaringType, string.Concat("emitDebug attribute [", attribute, "]."));
				if (attribute.Length <= 0 || !(attribute != "null"))
				{
					LogLog.Debug(XmlHierarchyConfigurator.declaringType, "Ignoring emitDebug attribute.");
				}
				else
				{
					LogLog.EmitInternalMessages = OptionConverter.ToBoolean(attribute, true);
				}
			}
			if (!LogLog.InternalDebugging)
			{
				string str = element.GetAttribute("debug");
				LogLog.Debug(XmlHierarchyConfigurator.declaringType, string.Concat("debug attribute [", str, "]."));
				if (str.Length <= 0 || !(str != "null"))
				{
					LogLog.Debug(XmlHierarchyConfigurator.declaringType, "Ignoring debug attribute.");
				}
				else
				{
					LogLog.InternalDebugging = OptionConverter.ToBoolean(str, true);
				}
				string attribute1 = element.GetAttribute("configDebug");
				if (attribute1.Length > 0 && attribute1 != "null")
				{
					LogLog.Warn(XmlHierarchyConfigurator.declaringType, "The \"configDebug\" attribute is deprecated.");
					LogLog.Warn(XmlHierarchyConfigurator.declaringType, "Use the \"debug\" attribute instead.");
					LogLog.InternalDebugging = OptionConverter.ToBoolean(attribute1, true);
				}
			}
			XmlHierarchyConfigurator.ConfigUpdateMode configUpdateMode = XmlHierarchyConfigurator.ConfigUpdateMode.Merge;
			string str1 = element.GetAttribute("update");
			if (str1 != null && str1.Length > 0)
			{
				try
				{
					configUpdateMode = (XmlHierarchyConfigurator.ConfigUpdateMode)OptionConverter.ConvertStringTo(typeof(XmlHierarchyConfigurator.ConfigUpdateMode), str1);
				}
				catch
				{
					LogLog.Error(XmlHierarchyConfigurator.declaringType, string.Concat("Invalid update attribute value [", str1, "]"));
				}
			}
			LogLog.Debug(XmlHierarchyConfigurator.declaringType, string.Concat("Configuration update mode [", configUpdateMode.ToString(), "]."));
			if (configUpdateMode == XmlHierarchyConfigurator.ConfigUpdateMode.Overwrite)
			{
				this.m_hierarchy.ResetConfiguration();
				LogLog.Debug(XmlHierarchyConfigurator.declaringType, "Configuration reset before reading config.");
			}
			foreach (XmlNode childNode in element.ChildNodes)
			{
				if (childNode.NodeType != XmlNodeType.Element)
				{
					continue;
				}
				XmlElement xmlElement = (XmlElement)childNode;
				if (xmlElement.LocalName == "logger")
				{
					this.ParseLogger(xmlElement);
				}
				else if (xmlElement.LocalName == "category")
				{
					this.ParseLogger(xmlElement);
				}
				else if (xmlElement.LocalName == "root")
				{
					this.ParseRoot(xmlElement);
				}
				else if (xmlElement.LocalName != "renderer")
				{
					if (xmlElement.LocalName == "appender")
					{
						continue;
					}
					this.SetParameter(xmlElement, this.m_hierarchy);
				}
				else
				{
					this.ParseRenderer(xmlElement);
				}
			}
			string attribute2 = element.GetAttribute("threshold");
			LogLog.Debug(XmlHierarchyConfigurator.declaringType, string.Concat("Hierarchy Threshold [", attribute2, "]"));
			if (attribute2.Length > 0 && attribute2 != "null")
			{
				Level level = (Level)this.ConvertStringTo(typeof(Level), attribute2);
				if (level != null)
				{
					this.m_hierarchy.Threshold = level;
					return;
				}
				LogLog.Warn(XmlHierarchyConfigurator.declaringType, string.Concat("Unable to set hierarchy threshold using value [", attribute2, "] (with acceptable conversion types)"));
			}
		}

		protected object ConvertStringTo(Type type, string value)
		{
			if (typeof(Level) != type)
			{
				return OptionConverter.ConvertStringTo(type, value);
			}
			Level item = this.m_hierarchy.LevelMap[value];
			if (item == null)
			{
				LogLog.Error(XmlHierarchyConfigurator.declaringType, string.Concat("XmlHierarchyConfigurator: Unknown Level Specified [", value, "]"));
			}
			return item;
		}

		protected object CreateObjectFromXml(XmlElement element, Type defaultTargetType, Type typeConstraint)
		{
			object obj;
			Type typeFromString = null;
			string attribute = element.GetAttribute("type");
			if (attribute == null || attribute.Length == 0)
			{
				if (defaultTargetType == null)
				{
					LogLog.Error(XmlHierarchyConfigurator.declaringType, string.Concat("Object type not specified. Cannot create object of type [", typeConstraint.FullName, "]. Missing Value or Type."));
					return null;
				}
				typeFromString = defaultTargetType;
			}
			else
			{
				try
				{
					typeFromString = SystemInfo.GetTypeFromString(attribute, true, true);
					goto Label0;
				}
				catch (Exception exception1)
				{
					Exception exception = exception1;
					LogLog.Error(XmlHierarchyConfigurator.declaringType, string.Concat("Failed to find type [", attribute, "]"), exception);
					obj = null;
				}
				return obj;
			}
		Label0:
			bool flag = false;
			if (typeConstraint != null && !typeConstraint.IsAssignableFrom(typeFromString))
			{
				if (!OptionConverter.CanConvertTypeTo(typeFromString, typeConstraint))
				{
					Type type = XmlHierarchyConfigurator.declaringType;
					string[] fullName = new string[] { "Object type [", typeFromString.FullName, "] is not assignable to type [", typeConstraint.FullName, "]. There are no acceptable type conversions." };
					LogLog.Error(type, string.Concat(fullName));
					return null;
				}
				flag = true;
			}
			object obj1 = null;
			try
			{
				obj1 = Activator.CreateInstance(typeFromString);
			}
			catch (Exception exception3)
			{
				Exception exception2 = exception3;
				LogLog.Error(XmlHierarchyConfigurator.declaringType, string.Concat("XmlHierarchyConfigurator: Failed to construct object of type [", typeFromString.FullName, "] Exception: ", exception2.ToString()));
			}
			foreach (XmlNode childNode in element.ChildNodes)
			{
				if (childNode.NodeType != XmlNodeType.Element)
				{
					continue;
				}
				this.SetParameter((XmlElement)childNode, obj1);
			}
			IOptionHandler optionHandler = obj1 as IOptionHandler;
			if (optionHandler != null)
			{
				optionHandler.ActivateOptions();
			}
			if (!flag)
			{
				return obj1;
			}
			return OptionConverter.ConvertTypeTo(obj1, typeConstraint);
		}

		protected IAppender FindAppenderByReference(XmlElement appenderRef)
		{
			string attribute = appenderRef.GetAttribute("ref");
			IAppender item = (IAppender)this.m_appenderBag[attribute];
			if (item != null)
			{
				return item;
			}
			XmlElement xmlElement = null;
			if (attribute != null && attribute.Length > 0)
			{
				foreach (XmlElement elementsByTagName in appenderRef.OwnerDocument.GetElementsByTagName("appender"))
				{
					if (elementsByTagName.GetAttribute("name") != attribute)
					{
						continue;
					}
					xmlElement = elementsByTagName;
					break;
				}
			}
			if (xmlElement == null)
			{
				LogLog.Error(XmlHierarchyConfigurator.declaringType, string.Concat("XmlHierarchyConfigurator: No appender named [", attribute, "] could be found."));
				return null;
			}
			item = this.ParseAppender(xmlElement);
			if (item != null)
			{
				this.m_appenderBag[attribute] = item;
			}
			return item;
		}

		private MethodInfo FindMethodInfo(Type targetType, string name)
		{
			string str = name;
			string str1 = string.Concat("Add", name);
			MethodInfo[] methods = targetType.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			for (int i = 0; i < (int)methods.Length; i++)
			{
				MethodInfo methodInfo = methods[i];
				if (!methodInfo.IsStatic && (string.Compare(methodInfo.Name, str, true, CultureInfo.InvariantCulture) == 0 || string.Compare(methodInfo.Name, str1, true, CultureInfo.InvariantCulture) == 0) && (int)methodInfo.GetParameters().Length == 1)
				{
					return methodInfo;
				}
			}
			return null;
		}

		private bool HasAttributesOrElements(XmlElement element)
		{
			bool flag;
			IEnumerator enumerator = element.ChildNodes.GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					XmlNode current = (XmlNode)enumerator.Current;
					if (current.NodeType != XmlNodeType.Attribute && current.NodeType != XmlNodeType.Element)
					{
						continue;
					}
					flag = true;
					return flag;
				}
				return false;
			}
			finally
			{
				IDisposable disposable = enumerator as IDisposable;
				if (disposable != null)
				{
					disposable.Dispose();
				}
			}
			return flag;
		}

		private static bool IsTypeConstructible(Type type)
		{
			if (type.IsClass && !type.IsAbstract)
			{
				ConstructorInfo constructor = type.GetConstructor(new Type[0]);
				if (constructor != null && !constructor.IsAbstract && !constructor.IsPrivate)
				{
					return true;
				}
			}
			return false;
		}

		protected IAppender ParseAppender(XmlElement appenderElement)
		{
			IAppender appender;
			string attribute = appenderElement.GetAttribute("name");
			string str = appenderElement.GetAttribute("type");
			Type type = XmlHierarchyConfigurator.declaringType;
			string[] strArrays = new string[] { "Loading Appender [", attribute, "] type: [", str, "]" };
			LogLog.Debug(type, string.Concat(strArrays));
			try
			{
				IAppender appender1 = (IAppender)Activator.CreateInstance(SystemInfo.GetTypeFromString(str, true, true));
				appender1.Name = attribute;
				foreach (XmlNode childNode in appenderElement.ChildNodes)
				{
					if (childNode.NodeType != XmlNodeType.Element)
					{
						continue;
					}
					XmlElement xmlElement = (XmlElement)childNode;
					if (xmlElement.LocalName != "appender-ref")
					{
						this.SetParameter(xmlElement, appender1);
					}
					else
					{
						string attribute1 = xmlElement.GetAttribute("ref");
						IAppenderAttachable appenderAttachable = appender1 as IAppenderAttachable;
						if (appenderAttachable == null)
						{
							Type type1 = XmlHierarchyConfigurator.declaringType;
							string[] name = new string[] { "Requesting attachment of appender named [", attribute1, "] to appender named [", appender1.Name, "] which does not implement log4net.Core.IAppenderAttachable." };
							LogLog.Error(type1, string.Concat(name));
						}
						else
						{
							Type type2 = XmlHierarchyConfigurator.declaringType;
							string[] name1 = new string[] { "Attaching appender named [", attribute1, "] to appender named [", appender1.Name, "]." };
							LogLog.Debug(type2, string.Concat(name1));
							IAppender appender2 = this.FindAppenderByReference(xmlElement);
							if (appender2 == null)
							{
								continue;
							}
							appenderAttachable.AddAppender(appender2);
						}
					}
				}
				IOptionHandler optionHandler = appender1 as IOptionHandler;
				if (optionHandler != null)
				{
					optionHandler.ActivateOptions();
				}
				LogLog.Debug(XmlHierarchyConfigurator.declaringType, string.Concat("reated Appender [", attribute, "]"));
				appender = appender1;
			}
			catch (Exception exception1)
			{
				Exception exception = exception1;
				Type type3 = XmlHierarchyConfigurator.declaringType;
				string[] strArrays1 = new string[] { "Could not create Appender [", attribute, "] of type [", str, "]. Reported error follows." };
				LogLog.Error(type3, string.Concat(strArrays1), exception);
				appender = null;
			}
			return appender;
		}

		protected void ParseChildrenOfLoggerElement(XmlElement catElement, Logger log, bool isRoot)
		{
			log.RemoveAllAppenders();
			foreach (XmlNode childNode in catElement.ChildNodes)
			{
				if (childNode.NodeType != XmlNodeType.Element)
				{
					continue;
				}
				XmlElement xmlElement = (XmlElement)childNode;
				if (xmlElement.LocalName == "appender-ref")
				{
					IAppender appender = this.FindAppenderByReference(xmlElement);
					string attribute = xmlElement.GetAttribute("ref");
					if (appender == null)
					{
						LogLog.Error(XmlHierarchyConfigurator.declaringType, string.Concat("Appender named [", attribute, "] not found."));
					}
					else
					{
						Type type = XmlHierarchyConfigurator.declaringType;
						string[] name = new string[] { "Adding appender named [", attribute, "] to logger [", log.Name, "]." };
						LogLog.Debug(type, string.Concat(name));
						log.AddAppender(appender);
					}
				}
				else if (xmlElement.LocalName == "level" || xmlElement.LocalName == "priority")
				{
					this.ParseLevel(xmlElement, log, isRoot);
				}
				else
				{
					this.SetParameter(xmlElement, log);
				}
			}
			IOptionHandler optionHandler = log as IOptionHandler;
			if (optionHandler != null)
			{
				optionHandler.ActivateOptions();
			}
		}

		protected void ParseLevel(XmlElement element, Logger log, bool isRoot)
		{
			string name = log.Name;
			if (isRoot)
			{
				name = "root";
			}
			string attribute = element.GetAttribute("value");
			Type type = XmlHierarchyConfigurator.declaringType;
			string[] strArrays = new string[] { "Logger [", name, "] Level string is [", attribute, "]." };
			LogLog.Debug(type, string.Concat(strArrays));
			if ("inherited" == attribute)
			{
				if (isRoot)
				{
					LogLog.Error(XmlHierarchyConfigurator.declaringType, "Root level cannot be inherited. Ignoring directive.");
					return;
				}
				LogLog.Debug(XmlHierarchyConfigurator.declaringType, string.Concat("Logger [", name, "] level set to inherit from parent."));
				log.Level = null;
				return;
			}
			log.Level = log.Hierarchy.LevelMap[attribute];
			if (log.Level == null)
			{
				Type type1 = XmlHierarchyConfigurator.declaringType;
				string[] strArrays1 = new string[] { "Undefined level [", attribute, "] on Logger [", name, "]." };
				LogLog.Error(type1, string.Concat(strArrays1));
				return;
			}
			Type type2 = XmlHierarchyConfigurator.declaringType;
			object[] objArray = new object[] { "Logger [", name, "] level set to [name=\"", log.Level.Name, "\",value=", log.Level.Value, "]." };
			LogLog.Debug(type2, string.Concat(objArray));
		}

		protected void ParseLogger(XmlElement loggerElement)
		{
			string attribute = loggerElement.GetAttribute("name");
			LogLog.Debug(XmlHierarchyConfigurator.declaringType, string.Concat("Retrieving an instance of log4net.Repository.Logger for logger [", attribute, "]."));
			Logger logger = this.m_hierarchy.GetLogger(attribute) as Logger;
			lock (logger)
			{
				bool flag = OptionConverter.ToBoolean(loggerElement.GetAttribute("additivity"), true);
				Type type = XmlHierarchyConfigurator.declaringType;
				object[] name = new object[] { "Setting [", logger.Name, "] additivity to [", flag, "]." };
				LogLog.Debug(type, string.Concat(name));
				logger.Additivity = flag;
				this.ParseChildrenOfLoggerElement(loggerElement, logger, false);
			}
		}

		protected void ParseRenderer(XmlElement element)
		{
			string attribute = element.GetAttribute("renderingClass");
			string str = element.GetAttribute("renderedClass");
			Type type = XmlHierarchyConfigurator.declaringType;
			string[] strArrays = new string[] { "Rendering class [", attribute, "], Rendered class [", str, "]." };
			LogLog.Debug(type, string.Concat(strArrays));
			IObjectRenderer objectRenderer = (IObjectRenderer)OptionConverter.InstantiateByClassName(attribute, typeof(IObjectRenderer), null);
			if (objectRenderer == null)
			{
				LogLog.Error(XmlHierarchyConfigurator.declaringType, string.Concat("Could not instantiate renderer [", attribute, "]."));
				return;
			}
			try
			{
				this.m_hierarchy.RendererMap.Put(SystemInfo.GetTypeFromString(str, true, true), objectRenderer);
			}
			catch (Exception exception1)
			{
				Exception exception = exception1;
				LogLog.Error(XmlHierarchyConfigurator.declaringType, string.Concat("Could not find class [", str, "]."), exception);
			}
		}

		protected void ParseRoot(XmlElement rootElement)
		{
			Logger root = this.m_hierarchy.Root;
			lock (root)
			{
				this.ParseChildrenOfLoggerElement(rootElement, root, true);
			}
		}

		protected void SetParameter(XmlElement element, object target)
		{
			string attribute = element.GetAttribute("name");
			if (element.LocalName != "param" || attribute == null || attribute.Length == 0)
			{
				attribute = element.LocalName;
			}
			Type type = target.GetType();
			Type parameterType = null;
			PropertyInfo property = null;
			MethodInfo methodInfo = null;
			property = type.GetProperty(attribute, BindingFlags.IgnoreCase | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			if (property == null || !property.CanWrite)
			{
				property = null;
				methodInfo = this.FindMethodInfo(type, attribute);
				if (methodInfo != null)
				{
					parameterType = methodInfo.GetParameters()[0].ParameterType;
				}
			}
			else
			{
				parameterType = property.PropertyType;
			}
			if (parameterType == null)
			{
				Type type1 = XmlHierarchyConfigurator.declaringType;
				string[] str = new string[] { "XmlHierarchyConfigurator: Cannot find Property [", attribute, "] to set object on [", target.ToString(), "]" };
				LogLog.Error(type1, string.Concat(str));
				return;
			}
			string attribute1 = null;
			if (element.GetAttributeNode("value") != null)
			{
				attribute1 = element.GetAttribute("value");
			}
			else if (element.HasChildNodes)
			{
				foreach (XmlNode childNode in element.ChildNodes)
				{
					if (childNode.NodeType != XmlNodeType.CDATA && childNode.NodeType != XmlNodeType.Text)
					{
						continue;
					}
					attribute1 = (attribute1 != null ? string.Concat(attribute1, childNode.InnerText) : childNode.InnerText);
				}
			}
			if (attribute1 == null)
			{
				object obj = null;
				if (parameterType != typeof(string) || this.HasAttributesOrElements(element))
				{
					Type type2 = null;
					if (XmlHierarchyConfigurator.IsTypeConstructible(parameterType))
					{
						type2 = parameterType;
					}
					obj = this.CreateObjectFromXml(element, type2, parameterType);
				}
				else
				{
					obj = "";
				}
				if (obj == null)
				{
					LogLog.Error(XmlHierarchyConfigurator.declaringType, string.Concat("Failed to create object to set param: ", attribute));
					return;
				}
				if (property != null)
				{
					Type type3 = XmlHierarchyConfigurator.declaringType;
					object[] name = new object[] { "Setting Property [", property.Name, "] to object [", obj, "]" };
					LogLog.Debug(type3, string.Concat(name));
					try
					{
						property.SetValue(target, obj, BindingFlags.SetProperty, null, null, CultureInfo.InvariantCulture);
					}
					catch (TargetInvocationException targetInvocationException1)
					{
						TargetInvocationException targetInvocationException = targetInvocationException1;
						Type type4 = XmlHierarchyConfigurator.declaringType;
						object[] objArray = new object[] { "Failed to set parameter [", property.Name, "] on object [", target, "] using value [", obj, "]" };
						LogLog.Error(type4, string.Concat(objArray), targetInvocationException.InnerException);
					}
				}
				else if (methodInfo != null)
				{
					Type type5 = XmlHierarchyConfigurator.declaringType;
					object[] name1 = new object[] { "Setting Collection Property [", methodInfo.Name, "] to object [", obj, "]" };
					LogLog.Debug(type5, string.Concat(name1));
					try
					{
						object[] objArray1 = new object[] { obj };
						methodInfo.Invoke(target, BindingFlags.InvokeMethod, null, objArray1, CultureInfo.InvariantCulture);
					}
					catch (TargetInvocationException targetInvocationException3)
					{
						TargetInvocationException targetInvocationException2 = targetInvocationException3;
						Type type6 = XmlHierarchyConfigurator.declaringType;
						object[] name2 = new object[] { "Failed to set parameter [", methodInfo.Name, "] on object [", target, "] using value [", obj, "]" };
						LogLog.Error(type6, string.Concat(name2), targetInvocationException2.InnerException);
					}
				}
			}
			else
			{
				try
				{
					attribute1 = OptionConverter.SubstituteVariables(attribute1, Environment.GetEnvironmentVariables());
				}
				catch (SecurityException securityException)
				{
					LogLog.Debug(XmlHierarchyConfigurator.declaringType, "Security exception while trying to expand environment variables. Error Ignored. No Expansion.");
				}
				Type type7 = null;
				string str1 = element.GetAttribute("type");
				if (str1 != null && str1.Length > 0)
				{
					try
					{
						Type typeFromString = SystemInfo.GetTypeFromString(str1, true, true);
						Type type8 = XmlHierarchyConfigurator.declaringType;
						string[] fullName = new string[] { "Parameter [", attribute, "] specified subtype [", typeFromString.FullName, "]" };
						LogLog.Debug(type8, string.Concat(fullName));
						if (parameterType.IsAssignableFrom(typeFromString))
						{
							parameterType = typeFromString;
						}
						else if (!OptionConverter.CanConvertTypeTo(typeFromString, parameterType))
						{
							Type type9 = XmlHierarchyConfigurator.declaringType;
							string[] strArrays = new string[] { "subtype [", typeFromString.FullName, "] set on [", attribute, "] is not a subclass of property type [", parameterType.FullName, "] and there are no acceptable type conversions." };
							LogLog.Error(type9, string.Concat(strArrays));
						}
						else
						{
							type7 = parameterType;
							parameterType = typeFromString;
						}
					}
					catch (Exception exception1)
					{
						Exception exception = exception1;
						Type type10 = XmlHierarchyConfigurator.declaringType;
						string[] strArrays1 = new string[] { "Failed to find type [", str1, "] set on [", attribute, "]" };
						LogLog.Error(type10, string.Concat(strArrays1), exception);
					}
				}
				object obj1 = this.ConvertStringTo(parameterType, attribute1);
				if (obj1 != null && type7 != null)
				{
					Type type11 = XmlHierarchyConfigurator.declaringType;
					string[] strArrays2 = new string[] { "Performing additional conversion of value from [", obj1.GetType().Name, "] to [", type7.Name, "]" };
					LogLog.Debug(type11, string.Concat(strArrays2));
					obj1 = OptionConverter.ConvertTypeTo(obj1, type7);
				}
				if (obj1 == null)
				{
					Type type12 = XmlHierarchyConfigurator.declaringType;
					object[] objArray2 = new object[] { "Unable to set property [", attribute, "] on object [", target, "] using value [", attribute1, "] (with acceptable conversion types)" };
					LogLog.Warn(type12, string.Concat(objArray2));
					return;
				}
				if (property != null)
				{
					Type type13 = XmlHierarchyConfigurator.declaringType;
					string[] name3 = new string[] { "Setting Property [", property.Name, "] to ", obj1.GetType().Name, " value [", obj1.ToString(), "]" };
					LogLog.Debug(type13, string.Concat(name3));
					try
					{
						property.SetValue(target, obj1, BindingFlags.SetProperty, null, null, CultureInfo.InvariantCulture);
					}
					catch (TargetInvocationException targetInvocationException5)
					{
						TargetInvocationException targetInvocationException4 = targetInvocationException5;
						Type type14 = XmlHierarchyConfigurator.declaringType;
						object[] objArray3 = new object[] { "Failed to set parameter [", property.Name, "] on object [", target, "] using value [", obj1, "]" };
						LogLog.Error(type14, string.Concat(objArray3), targetInvocationException4.InnerException);
					}
				}
				else if (methodInfo != null)
				{
					Type type15 = XmlHierarchyConfigurator.declaringType;
					string[] strArrays3 = new string[] { "Setting Collection Property [", methodInfo.Name, "] to ", obj1.GetType().Name, " value [", obj1.ToString(), "]" };
					LogLog.Debug(type15, string.Concat(strArrays3));
					try
					{
						object[] objArray4 = new object[] { obj1 };
						methodInfo.Invoke(target, BindingFlags.InvokeMethod, null, objArray4, CultureInfo.InvariantCulture);
					}
					catch (TargetInvocationException targetInvocationException7)
					{
						TargetInvocationException targetInvocationException6 = targetInvocationException7;
						Type type16 = XmlHierarchyConfigurator.declaringType;
						object[] objArray5 = new object[] { "Failed to set parameter [", attribute, "] on object [", target, "] using value [", obj1, "]" };
						LogLog.Error(type16, string.Concat(objArray5), targetInvocationException6.InnerException);
					}
				}
			}
		}

		private enum ConfigUpdateMode
		{
			Merge,
			Overwrite
		}
	}
}
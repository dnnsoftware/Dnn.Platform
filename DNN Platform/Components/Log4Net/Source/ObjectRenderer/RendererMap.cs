using log4net.Util;
using System;
using System.Collections;
using System.Globalization;
using System.IO;

namespace log4net.ObjectRenderer
{
	public class RendererMap
	{
		private readonly static Type declaringType;

		private Hashtable m_map;

		private Hashtable m_cache = new Hashtable();

		private static IObjectRenderer s_defaultRenderer;

		public IObjectRenderer DefaultRenderer
		{
			get
			{
				return RendererMap.s_defaultRenderer;
			}
		}

		static RendererMap()
		{
			RendererMap.declaringType = typeof(RendererMap);
			RendererMap.s_defaultRenderer = new log4net.ObjectRenderer.DefaultRenderer();
		}

		public RendererMap()
		{
			this.m_map = Hashtable.Synchronized(new Hashtable());
		}

		public void Clear()
		{
			this.m_map.Clear();
			this.m_cache.Clear();
		}

		public string FindAndRender(object obj)
		{
			string str = obj as string;
			if (str != null)
			{
				return str;
			}
			StringWriter stringWriter = new StringWriter(CultureInfo.InvariantCulture);
			this.FindAndRender(obj, stringWriter);
			return stringWriter.ToString();
		}

		public void FindAndRender(object obj, TextWriter writer)
		{
			if (obj == null)
			{
				writer.Write(SystemInfo.NullText);
				return;
			}
			string str = obj as string;
			if (str != null)
			{
				writer.Write(str);
				return;
			}
			try
			{
				this.Get(obj.GetType()).RenderObject(this, obj, writer);
			}
			catch (Exception exception1)
			{
				Exception exception = exception1;
				LogLog.Error(RendererMap.declaringType, string.Concat("Exception while rendering object of type [", obj.GetType().FullName, "]"), exception);
				string fullName = "";
				if (obj != null && obj.GetType() != null)
				{
					fullName = obj.GetType().FullName;
				}
				writer.Write(string.Concat("<log4net.Error>Exception rendering object type [", fullName, "]"));
				if (exception != null)
				{
					string str1 = null;
					try
					{
						str1 = exception.ToString();
					}
					catch
					{
					}
					writer.Write(string.Concat("<stackTrace>", str1, "</stackTrace>"));
				}
				writer.Write("</log4net.Error>");
			}
		}

		public IObjectRenderer Get(object obj)
		{
			if (obj == null)
			{
				return null;
			}
			return this.Get(obj.GetType());
		}

		public IObjectRenderer Get(Type type)
		{
			if (type == null)
			{
				throw new ArgumentNullException("type");
			}
			IObjectRenderer item = null;
			item = (IObjectRenderer)this.m_cache[type];
			if (item == null)
			{
				for (Type i = type; i != null; i = i.BaseType)
				{
					item = this.SearchTypeAndInterfaces(i);
					if (item != null)
					{
						break;
					}
				}
				if (item == null)
				{
					item = RendererMap.s_defaultRenderer;
				}
				this.m_cache[type] = item;
			}
			return item;
		}

		public void Put(Type typeToRender, IObjectRenderer renderer)
		{
			this.m_cache.Clear();
			if (typeToRender == null)
			{
				throw new ArgumentNullException("typeToRender");
			}
			if (renderer == null)
			{
				throw new ArgumentNullException("renderer");
			}
			this.m_map[typeToRender] = renderer;
		}

		private IObjectRenderer SearchTypeAndInterfaces(Type type)
		{
			IObjectRenderer item = (IObjectRenderer)this.m_map[type];
			if (item != null)
			{
				return item;
			}
			Type[] interfaces = type.GetInterfaces();
			for (int i = 0; i < (int)interfaces.Length; i++)
			{
				item = this.SearchTypeAndInterfaces(interfaces[i]);
				if (item != null)
				{
					return item;
				}
			}
			return null;
		}
	}
}
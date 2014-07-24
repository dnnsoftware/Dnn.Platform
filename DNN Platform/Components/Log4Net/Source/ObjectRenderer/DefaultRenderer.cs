using log4net.Util;
using System;
using System.Collections;
using System.IO;

namespace log4net.ObjectRenderer
{
	public sealed class DefaultRenderer : IObjectRenderer
	{
		public DefaultRenderer()
		{
		}

		private void RenderArray(RendererMap rendererMap, Array array, TextWriter writer)
		{
			if (array.Rank != 1)
			{
				writer.Write(array.ToString());
				return;
			}
			writer.Write(string.Concat(array.GetType().Name, " {"));
			int length = array.Length;
			if (length > 0)
			{
				rendererMap.FindAndRender(array.GetValue(0), writer);
				for (int i = 1; i < length; i++)
				{
					writer.Write(", ");
					rendererMap.FindAndRender(array.GetValue(i), writer);
				}
			}
			writer.Write("}");
		}

		private void RenderDictionaryEntry(RendererMap rendererMap, DictionaryEntry entry, TextWriter writer)
		{
			rendererMap.FindAndRender(entry.Key, writer);
			writer.Write("=");
			rendererMap.FindAndRender(entry.Value, writer);
		}

		private void RenderEnumerator(RendererMap rendererMap, IEnumerator enumerator, TextWriter writer)
		{
			writer.Write("{");
			if (enumerator != null && enumerator.MoveNext())
			{
				rendererMap.FindAndRender(enumerator.Current, writer);
				while (enumerator.MoveNext())
				{
					writer.Write(", ");
					rendererMap.FindAndRender(enumerator.Current, writer);
				}
			}
			writer.Write("}");
		}

		public void RenderObject(RendererMap rendererMap, object obj, TextWriter writer)
		{
			if (rendererMap == null)
			{
				throw new ArgumentNullException("rendererMap");
			}
			if (obj == null)
			{
				writer.Write(SystemInfo.NullText);
				return;
			}
			Array arrays = obj as Array;
			if (arrays != null)
			{
				this.RenderArray(rendererMap, arrays, writer);
				return;
			}
			IEnumerable enumerable = obj as IEnumerable;
			if (enumerable != null)
			{
				ICollection collections = obj as ICollection;
				if (collections != null && collections.Count == 0)
				{
					writer.Write("{}");
					return;
				}
				IDictionary dictionaries = obj as IDictionary;
				if (dictionaries != null)
				{
					this.RenderEnumerator(rendererMap, dictionaries.GetEnumerator(), writer);
					return;
				}
				this.RenderEnumerator(rendererMap, enumerable.GetEnumerator(), writer);
				return;
			}
			IEnumerator enumerator = obj as IEnumerator;
			if (enumerator != null)
			{
				this.RenderEnumerator(rendererMap, enumerator, writer);
				return;
			}
			if (obj is DictionaryEntry)
			{
				this.RenderDictionaryEntry(rendererMap, (DictionaryEntry)obj, writer);
				return;
			}
			string str = obj.ToString();
			writer.Write((str == null ? SystemInfo.NullText : str));
		}
	}
}
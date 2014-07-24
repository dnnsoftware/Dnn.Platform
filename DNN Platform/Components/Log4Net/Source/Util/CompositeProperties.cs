using System;
using System.Collections;

namespace log4net.Util
{
	public sealed class CompositeProperties
	{
		private PropertiesDictionary m_flattened;

		private ArrayList m_nestedProperties = new ArrayList();

		public object this[string key]
		{
			get
			{
				object item;
				if (this.m_flattened != null)
				{
					return this.m_flattened[key];
				}
				IEnumerator enumerator = this.m_nestedProperties.GetEnumerator();
				try
				{
					while (enumerator.MoveNext())
					{
						ReadOnlyPropertiesDictionary current = (ReadOnlyPropertiesDictionary)enumerator.Current;
						if (!current.Contains(key))
						{
							continue;
						}
						item = current[key];
						return item;
					}
					return null;
				}
				finally
				{
					IDisposable disposable = enumerator as IDisposable;
					if (disposable != null)
					{
						disposable.Dispose();
					}
				}
				return item;
			}
		}

		internal CompositeProperties()
		{
		}

		public void Add(ReadOnlyPropertiesDictionary properties)
		{
			this.m_flattened = null;
			this.m_nestedProperties.Add(properties);
		}

		public PropertiesDictionary Flatten()
		{
			if (this.m_flattened == null)
			{
				this.m_flattened = new PropertiesDictionary();
				int count = this.m_nestedProperties.Count;
				while (true)
				{
					int num = count - 1;
					count = num;
					if (num < 0)
					{
						break;
					}
					foreach (DictionaryEntry item in (IEnumerable)((ReadOnlyPropertiesDictionary)this.m_nestedProperties[count]))
					{
						this.m_flattened[(string)item.Key] = item.Value;
					}
				}
			}
			return this.m_flattened;
		}
	}
}
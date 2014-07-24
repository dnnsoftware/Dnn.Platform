using System;

namespace log4net.Util
{
	public sealed class ThreadContextStacks
	{
		private readonly ContextPropertiesBase m_properties;

		private readonly static Type declaringType;

		public ThreadContextStack this[string key]
		{
			get
			{
				ThreadContextStack threadContextStack = null;
				object item = this.m_properties[key];
				if (item != null)
				{
					threadContextStack = item as ThreadContextStack;
					if (threadContextStack == null)
					{
						string nullText = SystemInfo.NullText;
						try
						{
							nullText = item.ToString();
						}
						catch
						{
						}
						Type type = ThreadContextStacks.declaringType;
						string[] strArrays = new string[] { "ThreadContextStacks: Request for stack named [", key, "] failed because a property with the same name exists which is a [", item.GetType().Name, "] with value [", nullText, "]" };
						LogLog.Error(type, string.Concat(strArrays));
						threadContextStack = new ThreadContextStack();
					}
				}
				else
				{
					threadContextStack = new ThreadContextStack();
					this.m_properties[key] = threadContextStack;
				}
				return threadContextStack;
			}
		}

		static ThreadContextStacks()
		{
			ThreadContextStacks.declaringType = typeof(ThreadContextStacks);
		}

		internal ThreadContextStacks(ContextPropertiesBase properties)
		{
			this.m_properties = properties;
		}
	}
}
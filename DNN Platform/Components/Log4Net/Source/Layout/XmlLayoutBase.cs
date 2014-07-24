using log4net.Core;
using log4net.Util;
using System;
using System.IO;
using System.Xml;

namespace log4net.Layout
{
	public abstract class XmlLayoutBase : LayoutSkeleton
	{
		private bool m_locationInfo;

		private readonly ProtectCloseTextWriter m_protectCloseTextWriter = new ProtectCloseTextWriter(null);

		private string m_invalidCharReplacement = "?";

		public override string ContentType
		{
			get
			{
				return "text/xml";
			}
		}

		public string InvalidCharReplacement
		{
			get
			{
				return this.m_invalidCharReplacement;
			}
			set
			{
				this.m_invalidCharReplacement = value;
			}
		}

		public bool LocationInfo
		{
			get
			{
				return this.m_locationInfo;
			}
			set
			{
				this.m_locationInfo = value;
			}
		}

		protected XmlLayoutBase() : this(false)
		{
			this.IgnoresException = false;
		}

		protected XmlLayoutBase(bool locationInfo)
		{
			this.IgnoresException = false;
			this.m_locationInfo = locationInfo;
		}

		public override void ActivateOptions()
		{
		}

		public override void Format(TextWriter writer, LoggingEvent loggingEvent)
		{
			if (loggingEvent == null)
			{
				throw new ArgumentNullException("loggingEvent");
			}
			this.m_protectCloseTextWriter.Attach(writer);
			XmlTextWriter xmlTextWriter = new XmlTextWriter(this.m_protectCloseTextWriter)
			{
				Formatting = Formatting.None,
				Namespaces = false
			};
			this.FormatXml(xmlTextWriter, loggingEvent);
			xmlTextWriter.WriteWhitespace(SystemInfo.NewLine);
			xmlTextWriter.Close();
			this.m_protectCloseTextWriter.Attach(null);
		}

		protected abstract void FormatXml(XmlWriter writer, LoggingEvent loggingEvent);
	}
}
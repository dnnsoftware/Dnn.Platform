using log4net.Core;
using log4net.Layout;
using System;
using System.Data;

namespace log4net.Appender
{
	public class AdoNetAppenderParameter
	{
		private string m_parameterName;

		private System.Data.DbType m_dbType;

		private bool m_inferType = true;

		private byte m_precision;

		private byte m_scale;

		private int m_size;

		private IRawLayout m_layout;

		public System.Data.DbType DbType
		{
			get
			{
				return this.m_dbType;
			}
			set
			{
				this.m_dbType = value;
				this.m_inferType = false;
			}
		}

		public IRawLayout Layout
		{
			get
			{
				return this.m_layout;
			}
			set
			{
				this.m_layout = value;
			}
		}

		public string ParameterName
		{
			get
			{
				return this.m_parameterName;
			}
			set
			{
				this.m_parameterName = value;
			}
		}

		public byte Precision
		{
			get
			{
				return this.m_precision;
			}
			set
			{
				this.m_precision = value;
			}
		}

		public byte Scale
		{
			get
			{
				return this.m_scale;
			}
			set
			{
				this.m_scale = value;
			}
		}

		public int Size
		{
			get
			{
				return this.m_size;
			}
			set
			{
				this.m_size = value;
			}
		}

		public AdoNetAppenderParameter()
		{
			this.m_precision = 0;
			this.m_scale = 0;
			this.m_size = 0;
		}

		public virtual void FormatValue(IDbCommand command, LoggingEvent loggingEvent)
		{
			IDbDataParameter item = (IDbDataParameter)command.Parameters[this.m_parameterName];
			object value = this.Layout.Format(loggingEvent);
			if (value == null)
			{
				value = DBNull.Value;
			}
			item.Value = value;
		}

		public virtual void Prepare(IDbCommand command)
		{
			IDbDataParameter mParameterName = command.CreateParameter();
			mParameterName.ParameterName = this.m_parameterName;
			if (!this.m_inferType)
			{
				mParameterName.DbType = this.m_dbType;
			}
			if (this.m_precision != 0)
			{
				mParameterName.Precision = this.m_precision;
			}
			if (this.m_scale != 0)
			{
				mParameterName.Scale = this.m_scale;
			}
			if (this.m_size != 0)
			{
				mParameterName.Size = this.m_size;
			}
			command.Parameters.Add(mParameterName);
		}
	}
}
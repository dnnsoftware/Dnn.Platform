using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ClientDependency.Core.Logging
{
    internal class NullLogger : ILogger
    {
        #region ILogger Members

        public void Debug(string msg)
        {
        }

        public void Info(string msg)
        {
        }

        public void Warn(string msg)
        {
        }

        public void Error(string msg, Exception ex)
        {
        }

        public void Fatal(string msg, Exception ex)
        {
        }

        #endregion
    }
}

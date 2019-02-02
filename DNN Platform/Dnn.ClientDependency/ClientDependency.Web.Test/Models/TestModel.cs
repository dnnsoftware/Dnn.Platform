using System;

namespace ClientDependency.Web.Test.Models
{

    public class TestLogger : ClientDependency.Core.Logging.ILogger
    {
        public void Debug(string msg)
        {

        }

        public void Info(string msg)
        {

        }

        public void Warn(string msg)
        {
            throw new Exception(msg);
        }

        public void Error(string msg, Exception ex)
        {
            throw new Exception(msg, ex);
        }

        public void Fatal(string msg, Exception ex)
        {
            throw new Exception(msg, ex);
        }
    }

    public class TestModel
    {
        
        public string Heading { get; set; }
        public string BodyContent { get; set; }

    }
}
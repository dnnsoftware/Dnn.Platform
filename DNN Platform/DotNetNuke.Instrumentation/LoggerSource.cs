namespace DotNetNuke.Instrumentation
{
    public static class LoggerSource 
    {
        static ILoggerSource _instance = new LoggerSourceImpl();

        public static ILoggerSource Instance
        {
            get { return _instance; }
        }

        public static void SetTestableInstance(ILoggerSource loggerSource)
        {
            _instance = loggerSource;
        }
    }
}
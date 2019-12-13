namespace DotNetNuke.Tests.Core.ComponentModel.Helpers
{
    public class Service2Impl : IService2
    {
        private readonly IService _service;

        public Service2Impl(IService service)
        {
            _service = service;
        }

        #region IService2 Members

        public IService Service
        {
            get { return _service; }
        }

        #endregion
    }
}

using System;

namespace DotNetNuke.Services.Log.EventLog
{
    public interface ILogViewer
    {
        void BindData();

        string EventFilter { get; set; }
    }
}

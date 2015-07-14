using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace ClientDependency.Core.Module
{
    public interface IFilter
    {
        void SetHttpContext(HttpContextBase ctx);
        string UpdateOutputHtml(string html);
        HttpContextBase CurrentContext { get; }
        bool CanExecute();
        bool ValidateCurrentHandler();
    }
}

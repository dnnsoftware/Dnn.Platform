using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace ClientDependency.Core
{
    /// <summary>
    /// A provider that requires initialization under an Http context.
    /// The Http initialization will happen after the standard provider initialization.
    /// </summary>
    public interface IHttpProvider
    {

        void Initialize(HttpContextBase http);

    }
}

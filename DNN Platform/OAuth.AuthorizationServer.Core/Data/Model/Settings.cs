using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OAuth.AuthorizationServer.Core.Data.Model
{
    public partial class Settings
    {
        public string AuthorizationServerPrivateKey { get; set; }
        public string ResourceServerDecryptionKey { get; set; }
        public string AuthorizationServerVerificationKey { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ClientDependency.Core.Config;
using System.IO;
using System.Web;
using System.Net;
using System.IO.Compression;

namespace ClientDependency.Core
{
	public enum CompressionType
	{
		deflate, gzip, none
	}
}

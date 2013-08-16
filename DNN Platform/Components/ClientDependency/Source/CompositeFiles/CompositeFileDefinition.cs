using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ClientDependency.Core.Config;
using System.IO;
using System.Web;
using System.Net;
using System.IO.Compression;

namespace ClientDependency.Core.CompositeFiles
{
	/// <summary>
	/// A simple class defining a Uri string and whether or not it is a local application file
	/// </summary>
	public class CompositeFileDefinition
	{
		public CompositeFileDefinition(string uri, bool isLocalFile)
		{
			IsLocalFile = isLocalFile;
			Uri = uri;
		}
		public bool IsLocalFile { get; set; }
		public string Uri { get; set; }

		public override bool Equals(object obj)
		{
			return (obj.GetType().Equals(this.GetType())
				&& ((CompositeFileDefinition)obj).IsLocalFile.Equals(IsLocalFile)
				&& ((CompositeFileDefinition)obj).Uri.Equals(Uri));
		}

        /// <summary>
        /// overrides hash code to ensure that it is unique per machine
        /// </summary>
        /// <returns></returns>
		public override int GetHashCode()
        {
            string machineName;
            //catch usecase where user is running with EnvironmentPermission
            try
            {
                machineName=Environment.MachineName;
            }
            catch (Exception)
            {
                machineName = HttpContext.Current.Server.MachineName;
            }
            return (machineName.ToString() + Uri).GetHashCode();
		}
	}
}

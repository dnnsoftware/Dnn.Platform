#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2017
// by DotNetNuke Corporation
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation 
// the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and 
// to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions 
// of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
// TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
// CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
// DEALINGS IN THE SOFTWARE.
#endregion
#region Usings

using System.IO;
using System.Text;
using System.Web.UI;

using DotNetNuke.Common;
using DotNetNuke.Entities.Portals;

#endregion

namespace DotNetNuke.Framework
{
    /// -----------------------------------------------------------------------------
    /// Namespace:  DotNetNuke.Framework
    /// Project:    DotNetNuke
    /// Class:      DiskPageStatePersister
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// DiskPageStatePersister provides a disk (stream) based page state peristence mechanism
    /// </summary>
    /// -----------------------------------------------------------------------------
    public class DiskPageStatePersister : PageStatePersister
    {
        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Creates the DiskPageStatePersister
        /// </summary>
        /// -----------------------------------------------------------------------------
        public DiskPageStatePersister(Page page) : base(page)
        {
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// The CacheDirectory property is used to return the location of the "Cache"
        /// Directory for the Portal
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// -----------------------------------------------------------------------------
        public string CacheDirectory
        {
            get
            {
                return PortalController.Instance.GetCurrentPortalSettings().HomeSystemDirectoryMapPath + "Cache";
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// The StateFileName property is used to store the FileName for the State
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// -----------------------------------------------------------------------------
        public string StateFileName
        {
            get
            {
                var key = new StringBuilder();
                {
                    key.Append("VIEWSTATE_");
                    key.Append(Page.Session.SessionID);
                    key.Append("_");
                    key.Append(Page.Request.RawUrl);
                }
                return CacheDirectory + "\\" + Globals.CleanFileName(key.ToString()) + ".txt";
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Loads the Page State from the Cache
        /// </summary>
        /// -----------------------------------------------------------------------------
        public override void Load()
        {
            StreamReader reader = null;
            //Read the state string, using the StateFormatter.
            try
            {
                reader = new StreamReader(StateFileName);

                string serializedStatePair = reader.ReadToEnd();

                IStateFormatter formatter = StateFormatter;

                //Deserialize returns the Pair object that is serialized in
                //the Save method.      
                var statePair = (Pair) formatter.Deserialize(serializedStatePair);
                ViewState = statePair.First;
                ControlState = statePair.Second;
            }
            finally
            {
                if (reader != null)
                {
                    reader.Close();
                }
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Saves the Page State to the Cache
        /// </summary>
        /// -----------------------------------------------------------------------------
        public override void Save()
        {
			//No processing needed if no states available
            if (ViewState == null && ControlState == null)
            {
                return;
            }
            if (Page.Session != null)
            {
                if (!Directory.Exists(CacheDirectory))
                {
                    Directory.CreateDirectory(CacheDirectory);
                }

                //Write a state string, using the StateFormatter.
                using (var writer = new StreamWriter(StateFileName, false))
                {
                    IStateFormatter formatter = StateFormatter;
                    var statePair = new Pair(ViewState, ControlState);
                    string serializedState = formatter.Serialize(statePair);
                    writer.Write(serializedState);
                    writer.Close();
                }
            }
        }
    }
}

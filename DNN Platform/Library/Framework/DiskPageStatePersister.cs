// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
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

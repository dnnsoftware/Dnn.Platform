// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Framework
{
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Text;
    using System.Web.UI;

    using DotNetNuke.Common;
    using DotNetNuke.Entities.Portals;

    /// <summary>DiskPageStatePersister provides a disk (stream) based page state persistence mechanism.</summary>
    public class DiskPageStatePersister : PageStatePersister
    {
        /// <summary>Initializes a new instance of the <see cref="DiskPageStatePersister"/> class.</summary>
        /// <param name="page">The <see cref="Page"/> that the view state persistence mechanism is created for.</param>
        public DiskPageStatePersister(Page page)
            : base(page)
        {
        }

        /// <summary>
        /// Gets the CacheDirectory property is used to return the location of the "Cache"
        /// Directory for the Portal.
        /// </summary>
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Breaking change")]
        public string CacheDirectory => PortalController.Instance.GetCurrentPortalSettings().HomeSystemDirectoryMapPath + "Cache";

        /// <summary>Gets the StateFileName property is used to store the FileName for the State.</summary>
        public string StateFileName
        {
            get
            {
                var key = new StringBuilder();
                {
                    key.Append("VIEWSTATE_");
                    key.Append(this.Page.Session.SessionID);
                    key.Append('_');
                    key.Append(this.Page.Request.RawUrl);
                }

                return $@"{this.CacheDirectory}\{Globals.CleanFileName(key.ToString())}.txt";
            }
        }

        /// <summary>Loads the Page State from the Cache.</summary>
        public override void Load()
        {
            StreamReader reader = null;

            // Read the state string, using the StateFormatter.
            try
            {
                reader = new StreamReader(this.StateFileName);

                string serializedStatePair = reader.ReadToEnd();

                IStateFormatter formatter = this.StateFormatter;

                // Deserialize returns the Pair object that is serialized in
                // the Save method.
                var statePair = (Pair)formatter.Deserialize(serializedStatePair);
                this.ViewState = statePair.First;
                this.ControlState = statePair.Second;
            }
            finally
            {
                if (reader != null)
                {
                    reader.Close();
                }
            }
        }

        /// <summary>Saves the Page State to the Cache.</summary>
        public override void Save()
        {
            // No processing needed if no states available
            if (this.ViewState == null && this.ControlState == null)
            {
                return;
            }

            if (this.Page.Session != null)
            {
                if (!Directory.Exists(this.CacheDirectory))
                {
                    Directory.CreateDirectory(this.CacheDirectory);
                }

                // Write a state string, using the StateFormatter.
                using (var writer = new StreamWriter(this.StateFileName, false))
                {
                    IStateFormatter formatter = this.StateFormatter;
                    var statePair = new Pair(this.ViewState, this.ControlState);
                    string serializedState = formatter.Serialize(statePair);
                    writer.Write(serializedState);
                    writer.Close();
                }
            }
        }
    }
}

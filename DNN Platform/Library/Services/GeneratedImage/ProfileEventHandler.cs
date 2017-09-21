using DotNetNuke.Entities.Profile;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace DotNetNuke.Services.GeneratedImage
{
    /// <summary>
    /// this class handles profile changes
    /// </summary>
    [Export(typeof(IProfileEventHandlers))]
    [ExportMetadata("MessageType", "ProfileEventHandler")]
    public class ProfileEventHandler  : IProfileEventHandlers
    {
        /// <summary>
        /// this metod clears client and server cache when profile is updated
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        public void ProfileUpdated(object sender, ProfileEventArgs args)
        {
            //extract old and new user profile from args and clear both client and server caching
            var profileArgs = (ProfileEventArgs)args;
            var oldProfile = profileArgs.OldProfile;
            var currentUser = profileArgs.User;

            //might logged above variables

            ClearCurrentContextImageCache();
        }

        private void ClearCurrentContextImageCache()
        {
            var context = HttpContext.Current;

            string cacheId = HttpContext.Current.Session["DnnImageHandlerClientCacheId"] != null ? HttpContext.Current.Session["DnnImageHandlerClientCacheId"].ToString() : null;
            //delete client-side caching
            if (!string.IsNullOrWhiteSpace(context.Request.Headers["If-None-Match"]))
            {
                context.Request.Headers.Remove("If-None-Match");
            }

            //delete server side caching
            if (!string.IsNullOrWhiteSpace(cacheId))
            {
                DiskImageStore.ForcePurgeFromServerCache(cacheId);
            }
        }
    }
}

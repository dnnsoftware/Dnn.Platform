namespace DotNetNuke.Common.Utilities
{
    /// -----------------------------------------------------------------------------
    /// Project:    DotNetNuke
    /// Namespace:  DotNetNuke.Common.Utilities
    /// Class:      CacheItemExpiredCallback
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The CacheItemExpiredCallback delegate defines a callback method that notifies
    /// the application when a CacheItem is Expired (when an attempt is made to get the item)
    /// </summary>
    /// -----------------------------------------------------------------------------
    public delegate object CacheItemExpiredCallback(CacheItemArgs dataArgs);
}

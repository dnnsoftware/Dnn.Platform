namespace DotNetNuke.Security
{
    ///-----------------------------------------------------------------------------
    /// <summary>
    /// The SecurityAccessLevel enum is used to determine which level of access rights
    /// to assign to a specific module or module action. 
    /// </summary>
    ///-----------------------------------------------------------------------------
    public enum SecurityAccessLevel
    {
        ControlPanel = -3,
        SkinObject = -2,
        Anonymous = -1,
        View = 0,
        Edit = 1,
        Admin = 2,
        Host = 3,
        ViewPermissions = 4
    }
}

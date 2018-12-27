export function canManageRoles(settings, user) {
    return !user.isSuperUser && (settings.isAdmin || settings.permissions.MANAGE_ROLES);
}

export function canManageProfile(settings) {
    return settings.isAdmin || settings.permissions.MANAGE_PROFILE;
}

export function canViewSettings(settings) {
    return settings.isAdmin || settings.permissions.VIEW_SETTINGS;
}

export function canAddUser(settings) {
    return (settings.isAdmin || settings.permissions.ADD_USER);
}

export function canManagePassword(settings)
{
    return (settings.isAdmin || settings.permissions.MANAGE_PASSWORD);
}

export function canEditSettings(settings)
{
    return (settings.isAdmin || settings.permissions.EDIT_SETTINGS);
}

export function canDeleteUser(settings, userId)
{
    return (settings.isAdmin || settings.permissions.DELETE_USER) 
        && userId!==settings.userId;
}

export function canAuthorizeUnAuthorizeUser(settings, userId)
{
    return (settings.isAdmin || settings.permissions.AUTHORIZE_UNAUTHORIZE_USER) 
        && userId!==settings.userId;
}

export function canPromoteDemote(settings, userId) {
    return settings.isHost && userId!==settings.userId;
}
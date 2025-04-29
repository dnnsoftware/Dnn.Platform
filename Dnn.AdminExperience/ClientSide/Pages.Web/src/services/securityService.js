import utils from "../utils";

const securityService = {
    userHasPermission(permission, selectedPage) {
        const isSuperUser = utils.getIsSuperUser();
        if (isSuperUser) {
            return true;
        }

        if (!permission) {
            return true;
        }
        const userPermissionsOverPage =
      (selectedPage && selectedPage.pagePermissions) ||
      utils.getCurrentPagePermissions();

        return userPermissionsOverPage[permission];
    },
    isSuperUser() {
        return utils.getIsSuperUser();
    },
    canSeePagesList() {
        return utils.canSeePagesList();
    },
    canAddPages() {
        return utils.canAddPages();
    },
};
export default securityService;

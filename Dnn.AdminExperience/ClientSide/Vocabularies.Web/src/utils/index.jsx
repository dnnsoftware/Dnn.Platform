const utils = {
    init(options) {
        if (!options || !options.utility) {
            throw new Error("Utilities is undefined.");
        }

        this.utilities = options.utility;
        this.moduleName = options.moduleName;
        this.settings = options.settings;    
    },
    canEdit: function () {
        return this.settings.isHost || this.settings.isAdmin || (this.settings.permissions && this.settings.permissions.EDIT === true);
    },
    isHost: function () {
        return this.settings.isHost;
    },
    utilities: null,
    moduleName: null,
    settings: null
};
export default utils;
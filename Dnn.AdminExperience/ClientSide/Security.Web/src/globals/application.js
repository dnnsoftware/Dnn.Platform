import utilities from "../utils";
import "../less/style.less";

const boilerPlate = {
    init() {
        let options = window.dnn.initSecurity();
        utilities.init(options.utility);
        utilities.moduleName = options.moduleName;
        const rawSettings = options && options.settings ? options.settings : {};
        utilities.settings = {
            ...rawSettings,
            isHost: !!rawSettings.isHost,
            isAdmin: !!rawSettings.isAdmin,
            permissions: rawSettings.permissions || {}
        };
    },
    dispatch() {
        throw new Error("dispatch method needs to be overwritten from the Redux store");
    }
};


export default boilerPlate;
import utilities from "../utils";
const boilerPlate = {
    init() {
        let options = window.dnn.initRoles();

        utilities.init(options.utility);
        utilities.moduleName = options.moduleName;
        utilities.settings = options.settings;
    },
    dispatch() {
        throw new Error("dispatch method needs to be overwritten from the Redux store");
    }
};


export default boilerPlate;
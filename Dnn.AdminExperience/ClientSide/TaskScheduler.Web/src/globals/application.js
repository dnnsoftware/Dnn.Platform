import utilities from "../utils";
import "../less/style.less";

const boilerPlate = {
    init() {
        let options = window.dnn.initScheduler();

        utilities.init(options.utility);
        utilities.moduleName = options.moduleName;
    },
    dispatch() {
        throw new Error("dispatch method needs to be overwritten from the Redux store");
    }
};


export default boilerPlate;
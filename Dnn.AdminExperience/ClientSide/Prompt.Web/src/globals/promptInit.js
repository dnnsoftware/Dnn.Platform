import { util } from "utils/helpers";

const promptInit = {
    init() {
        let options = window.dnn.initPrompt();

        util.init(options.utility);
        util.moduleName = options.moduleName;
        util.settings = options.settings;
    },
    dispatch() {
        throw new Error("dispatch method needs to be overwritten from the Redux store");
    }
};

export default promptInit;

 
export const IS_DEV = process.env.NODE_ENV !== "production";
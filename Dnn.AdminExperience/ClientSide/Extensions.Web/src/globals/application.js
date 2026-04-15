import utilities from "../utils";
const extensions = {
    init() {
        let options = window.dnn.initExtensions();
        utilities.init(options);

        // eslint-disable-next-line no-undef
        require("../less/style.less");
    },
    dispatch() {
        throw new Error("dispatch method needs to be overwritten from the Redux store");
    }
};


export default extensions;

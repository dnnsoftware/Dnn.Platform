import api from "./api";
import globalActions from "../actions/globalActions";
import localizeService from "../services/localizeService.js";

const resourceManager = {
    init(options) {
        api.init(options);
        localizeService.init(options.resx);
        require("../less/styles.less");

        resourceManager.dispatch(globalActions.loadInitialParameters(options));
        const container = document.getElementById(options.containerId);

        this.render(container);
    },

    dispatch() {
        throw new Error("dispatch method needs to be overwritten from the Redux store");
    },

    render() {
        throw new Error("render method needs to be overwritten from the Redux store");
    }

};

if (typeof window.dnn === "undefined" || window.dnn === null) {
    window.dnn = {};
}
if (typeof window.dnn.ResourceManager === "undefined") {
    window.dnn.ResourceManager = {};
}
window.dnn.ResourceManager.instance = resourceManager;

export default resourceManager;
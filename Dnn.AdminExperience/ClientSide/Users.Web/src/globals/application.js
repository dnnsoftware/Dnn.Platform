import appSettings from "utils/applicationSettings";
import "../less/style.less";

const usersApplication = {
    init() {
        let options = window.dnn.initUsers();
        appSettings.init(options);
    },
    dispatch() {
        throw new Error("dispatch method needs to be overwritten from the Redux store");
    }
};


export default usersApplication;
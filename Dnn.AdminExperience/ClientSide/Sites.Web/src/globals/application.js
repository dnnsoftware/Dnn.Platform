import utilities from "utils/applicationSettings";
import "../less/style.less";

const Sites = {
  init(initCallback) {
    if (typeof window.dnn[initCallback] === "function") {
      let options = window.dnn[initCallback]();
      utilities.init(options);
    }
  },
  dispatch() {
    throw new Error(
      "dispatch method needs to be overwritten from the Redux store",
    );
  },
};

export default Sites;

import utilities from "../utils";
import "../less/style.less";

const boilerPlate = {
  init() {
    let options = window.dnn.initSiteSettings();

    utilities.init(options.utility);
    utilities.moduleName = options.moduleName;
    utilities.settings = options.settings;
    utilities.identifier = options.identifier;
    utilities.siteRoot = options.siteRoot;

    if (!window.dnn.SiteSettings) {
      window.dnn.SiteSettings = {};
    }

    window.dnn.SiteSettings.bundleLoaded = true;
  },
  dispatch() {
    throw new Error(
      "dispatch method needs to be overwritten from the Redux store",
    );
  },
};

export default boilerPlate;

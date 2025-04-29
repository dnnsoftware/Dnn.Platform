import utilities from "../utils";
const extensions = {
  init() {
    // This setting is required and define the public path
    // to allow the web application to download assets on demand

    // __webpack_public_path__ = options.publicPath;
    let options = window.dnn.initExtensions();
    utilities.init(options);

    // delay the styles loading after the __webpack_public_path__ is set
    // this allows the fonts associated to be loaded properly in production
    // eslint-disable-next-line no-undef
    require("../less/style.less");
  },
  dispatch() {
    throw new Error(
      "dispatch method needs to be overwritten from the Redux store",
    );
  },
};

export default extensions;

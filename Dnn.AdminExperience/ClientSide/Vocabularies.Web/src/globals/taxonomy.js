import utilities from "../utils";
import "../less/style.less";

const vocabularies = {
  init() {
    let options = window.dnn.initVocabularies();

    utilities.init(options);
  },
  dispatch() {
    throw new Error(
      "dispatch method needs to be overwritten from the Redux store",
    );
  },
};

export default vocabularies;

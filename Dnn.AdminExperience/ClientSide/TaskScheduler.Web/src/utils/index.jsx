const utils = {
  init(utilities) {
    if (!utilities) {
      throw new Error("Utilities is undefined.");
    }
    this.utilities = utilities;
  },

  formatString() {
    const format = arguments[0];
    const methodsArgs = arguments;
    return format.replace(/[{[](\d+)[\]}]/gi, function (_, index) {
      let argsIndex = parseInt(index) + 1;
      return methodsArgs[argsIndex];
    });
  },

  utilities: null,
};
export default utils;

const utils = {
    init(utilities) {
        if (!utilities) {
            throw new Error("Utilities is undefined.");
        }
        this.utilities = utilities;      
    },
    utilities: null
};
export default utils;
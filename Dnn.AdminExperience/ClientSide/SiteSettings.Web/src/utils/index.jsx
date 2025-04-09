const utils = {
    init(utilities) {
        if (!utilities) {
            throw new Error("Utilities is undefined.");
        }
        this.utilities = utilities;      
    },
    utilities: null,

    isPlatform() {       
        return window.dnn.utility.getSKU().toLowerCase() === "dnn";
    }
};
export default utils;
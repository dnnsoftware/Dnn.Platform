const utils = {
    init(utilities) {
        if (!utilities) {
            throw new Error("Utilities is undefined.");
        }
        this.utilities = utilities;      
    },
    utilities: null,
    formatDateNoTime: function (date) {
        const dateOptions = { year: "numeric", month: "numeric", day: "numeric" };
        return new Date(date).toLocaleDateString("es-US", dateOptions);
    },
    formatNumeric: function (value) {       
        return value.toLocaleString("es-US");
    }
};
export default utils;
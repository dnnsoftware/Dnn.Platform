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
    },
    formatNumeric2Decimals: function (value) {       
        return parseFloat(Math.round(value * 100) / 100).toFixed(2);
    }
};
export default utils;
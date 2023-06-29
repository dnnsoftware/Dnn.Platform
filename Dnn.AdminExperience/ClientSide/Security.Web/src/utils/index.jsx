const dateFormatter = new Intl.DateTimeFormat(dnn.utility.getCulture(), { dateStyle: "short" });
const dateTimeFormatter = new Intl.DateTimeFormat(dnn.utility.getCulture(), { dateStyle: "full", timeStyle: "long" });

const utils = {
    init(utilities) {
        if (!utilities) {
            throw new Error("Utilities is undefined.");
        }
        this.utilities = utilities;
    },
    utilities: null,
    formatDate(datestringOrNull) {
        if (!datestringOrNull) {
            return "";
        }
        return dateFormatter.format(new Date(datestringOrNull));
    },
    formatDateTime(datestringOrNull) {
        if (!datestringOrNull) {
            return "";
        }
        return dateTimeFormatter.format(new Date(datestringOrNull));
    },
    dateInPast(firstDate, secondDate) {
        if (firstDate.setHours(0, 0, 0, 0) <= secondDate.setHours(0, 0, 0, 0)) {
            return true;
        }
        return false;
    }
};
export default utils;
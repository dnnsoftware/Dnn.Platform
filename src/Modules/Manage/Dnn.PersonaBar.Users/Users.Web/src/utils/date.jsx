const date = {
    format(dateValue, longformat) {
        let date = new Date(dateValue);
        let dayValue = date.getDate(),
            monthValue = date.getMonth() + 1,
            yearValue = date.getFullYear(),
            hourValue = date.getHours(),
            minsValue = date.getMinutes(),
            secsValue = date.getSeconds();
        let amPMValue = date.getHours() >= 12 ? "PM" : "AM";
        hourValue = hourValue >= 12 ? hourValue - 12 : hourValue;
        if (yearValue < 1900) {
            return "-";
        }

        let returnValue = monthValue + "/" + dayValue + "/" + yearValue;
        if (longformat === true) {
            returnValue += " " + hourValue + ":" + minsValue + ":" + secsValue + " " + amPMValue;
        }
        return returnValue;
    }
};
export default date;

const date = {
    format(dateValue) {
        let date = new Date(dateValue);

        let dayValue = date.getDate(),
            monthValue = date.getMonth() + 1,
            yearValue = date.getFullYear();

        if (yearValue < 1900) {
            return "-";
        }

        return monthValue + "/" + dayValue + "/" + yearValue;
    }
};
export default date;

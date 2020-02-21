import Localization from "../localization";

const regExpPositiveIntegerValue = /^(0|[1-9]\d*)$/;

export default function validateFields(field, value) {
    const errors = {};

    if (field === "smtpConnectionLimit" || field === "smtpMaxIdleTime" || field === "messageSchedulerBatchSize" ) {
        if (!regExpPositiveIntegerValue.test(value)) {
            errors[field] = Localization.get("NoIntegerValueError");
        }
        else {
            errors[field] = undefined;
        }
    }

    return errors;
}
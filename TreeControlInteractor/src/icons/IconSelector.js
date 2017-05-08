import {
    ArrowIcon
} from "./arrow-icon/_arrow_icon";


export const IconSelector = (type) => {
    switch (type) {
        case "arrow_bullet":
            return ArrowIcon;

        default:
            return ArrowIcon;
    }
};
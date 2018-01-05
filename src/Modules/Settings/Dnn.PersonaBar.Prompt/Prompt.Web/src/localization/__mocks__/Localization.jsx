import { util } from "../../utils/helpers";

const Localization = {
    get(key) {
        let moduleName = "Prompt";
        return util.utilities.resx[moduleName][key];
    }
};
export default Localization;
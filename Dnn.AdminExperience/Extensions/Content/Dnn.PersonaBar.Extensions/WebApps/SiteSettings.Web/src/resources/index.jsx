import util from "../utils";

const resx = {
    get(key) {
        return util.utilities.getResx("SiteSettings", key);
    }
};
export default resx;
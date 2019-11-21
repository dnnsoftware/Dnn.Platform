import util from "./utils";

const resx = {
    get(key) {
        return util.getResx(key);
    }
};

export default resx;
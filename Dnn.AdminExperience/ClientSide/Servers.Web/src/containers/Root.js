import RootProd from "./Root.prod";
import RootDev from "./Root.dev";

const Root = process.env.NODE_ENV === "production" ? RootProd : RootDev;

export default Root;

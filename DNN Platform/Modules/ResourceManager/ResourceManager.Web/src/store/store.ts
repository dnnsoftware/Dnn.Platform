import { createStore } from "@stencil/store";
import { GetFoldersResponse } from "../services/InternalServicesClient";

const { state } = createStore<{
    moduleId: number;
    rootFolders: GetFoldersResponse;
}>({
    moduleId: -1,
    rootFolders: undefined,
});

export default state;
import { createStore } from "@stencil/store";
import { GetFoldersResponse } from "../services/InternalServicesClient";
import { GetFolderContentResponse } from "../services/ItemsClient";

const { state } = createStore<{
    moduleId: number;
    rootFolders?: GetFoldersResponse;
    currentItems?: GetFolderContentResponse;
    layout: "list" | "card";
}>({
    moduleId: -1,
    layout: "list",
});

export default state;